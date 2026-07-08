using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Entities.Auditing;
using ManteqTask.Domain.Entities.Authentication;
using ManteqTask.Domain.Entities.Requests;
using ManteqTask.Domain.Enums;
using ManteqTask.Domain.Interfaces.Application.Services;
using ManteqTask.Domain.Interfaces.Domain.Auditing;
using ManteqTask.Infrastructure.Configurations;
using ManteqTask.Infrastructure.Configurations.Seed;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManteqTask.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IConfiguration configuration,
    ILogger<ApplicationDbContext> logger,
    ICurrentUserService currentUser)
    : DbContext(options)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ApplicationDbContext> _logger = logger;
    private readonly ICurrentUserService _currentUser = currentUser;

    private static readonly JsonSerializerOptions AuditJsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
    // DbSet properties for the main entities and join tables
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<AuditTrail> AuditTrails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ApplySoftDeleteFilters(modelBuilder); // Cleaner call in OnModelCreating

        // Apply configurations in specific order
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UsersSeed(_configuration));

        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolesSeed());

        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionsSeed());

        // Apply relationship configurations last
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new UsersRolesSeed());

        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RolesPermissionsSeed());

        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

        // Request workflow
        modelBuilder.ApplyConfiguration(new RequestConfiguration());
        modelBuilder.ApplyConfiguration(new AuditTrailConfiguration());

        // Human-readable request numbers come from a native, concurrency-safe sequence.
        modelBuilder.HasSequence<long>("request_number_seq").StartsAt(1);

    }

    // _logger service
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        // Capture audit rows before saving, while Original/Current values are still available.
        // All PKs are client-assigned Guids, so EntityId is known up front — no two-phase save needed.
        List<AuditTrail> auditEntries = CaptureAuditEntries();

        // Log database changes before saving
        LogChanges();

        if (auditEntries.Count > 0)
            await AuditTrails.AddRangeAsync(auditEntries, cancellationToken);

        int result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        // Log successful save
        _logger.LogInformation("Database changes successfully saved. Affected rows: {Result}", result);

        return result;
    }

    /// <summary>
    /// Builds one <see cref="AuditTrail"/> row per changed entity that opts in via <see cref="IAuditable"/>.
    /// AuditTrail itself is never audited (guards against recursion); the concurrency token (xmin) and the
    /// primary key are skipped from the diff. Navigations are naturally excluded — only scalar properties
    /// appear in <see cref="EntityEntry.Properties"/>.
    /// </summary>
    private List<AuditTrail> CaptureAuditEntries()
    {
        Guid? changedBy = _currentUser.UserId == Guid.Empty ? null : _currentUser.UserId;
        DateTime changedAt = DateTime.UtcNow;

        // Snapshot before adding audit rows so we don't iterate over our own inserts.
        List<EntityEntry> entries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable
                && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        List<AuditTrail> audits = [];

        foreach (EntityEntry entry in entries)
        {
            AuditAction action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Deleted => AuditAction.Deleted,
                _ => AuditAction.Updated,
            };

            Dictionary<string, AuditChange> changes = BuildChanges(entry);

            // Nothing actually changed on an update (e.g. only a navigation touched): skip the row.
            if (entry.State == EntityState.Modified && changes.Count == 0)
                continue;

            audits.Add(new AuditTrail
            {
                EntityType = entry.Metadata.ClrType.Name,
                EntityId = ResolvePrimaryKey(entry),
                Action = action,
                ChangedBy = changedBy,
                ChangedAt = changedAt,
                Changes = changes.Count > 0 ? JsonSerializer.Serialize(changes, AuditJsonOptions) : null,
            });
        }

        return audits;
    }

    private static Dictionary<string, AuditChange> BuildChanges(EntityEntry entry)
    {
        Dictionary<string, AuditChange> changes = [];

        foreach (PropertyEntry property in entry.Properties)
        {
            // Skip PK and the concurrency token (xmin).
            if (property.Metadata.IsPrimaryKey() || property.Metadata.IsConcurrencyToken)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    changes[property.Metadata.Name] = new AuditChange(null, property.CurrentValue);
                    break;

                case EntityState.Deleted:
                    changes[property.Metadata.Name] = new AuditChange(property.OriginalValue, null);
                    break;

                case EntityState.Modified:
                    if (!Equals(property.OriginalValue, property.CurrentValue))
                        changes[property.Metadata.Name] = new AuditChange(property.OriginalValue, property.CurrentValue);
                    break;
            }
        }

        return changes;
    }

    private static Guid ResolvePrimaryKey(EntityEntry entry)
    {
        Microsoft.EntityFrameworkCore.Metadata.IProperty keyProperty = entry.Metadata.FindPrimaryKey()!.Properties[0];
        object? value = entry.Property(keyProperty.Name).CurrentValue;
        return value is Guid id ? id : Guid.Empty;
    }

    /// <summary>Field-level diff entry serialized to jsonb as { "old": ..., "new": ... }.</summary>
    private sealed record AuditChange(
        [property: JsonPropertyName("old")] object? Old,
        [property: JsonPropertyName("new")] object? New);

    private void LogChanges()
    {
        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                _logger.LogInformation($"Adding new entity: {entry.Entity.GetType().Name} - Values: {entry.CurrentValues.ToObject()}");
            }
            else if (entry.State == EntityState.Modified)
            {
                _logger.LogInformation($"Updating entity: {entry.Entity.GetType().Name} - Old Values: {entry.OriginalValues.ToObject()} - New Values: {entry.CurrentValues.ToObject()}");
            }
            else if (entry.State == EntityState.Deleted)
            {
                _logger.LogInformation($"Deleting entity: {entry.Entity.GetType().Name} - Values: {entry.OriginalValues.ToObject()}");
            }
        }
    }

    /// <summary>
    /// Applies the global query filter for soft deletion to all entities 
    /// that implement the ISoftDelete interface.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance.</param>
    private void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        // Find all entity types that implement ISoftDelete
        IEnumerable<IMutableEntityType> softDeleteEntityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(ISoftDelete).IsAssignableFrom(e.ClrType));

        // Apply the filter for each found entity
        foreach (IMutableEntityType? entityType in softDeleteEntityTypes)
        {
            // Use a helper method to create and apply the non-generic filter expression
            MethodInfo method = typeof(ApplicationDbContext)
                .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(null, new object[] { modelBuilder });
        }
    }

    // A generic static method to create and apply the filter.
    // Making it static avoids potential closure capture issues with the DbContext instance.
    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ISoftDelete
    {
        // The filter expression: entity.DeletedAt == null (i.e. not soft-deleted)
        modelBuilder.Entity<TEntity>().HasQueryFilter(
            e => EF.Property<DateTime?>(e, nameof(ISoftDelete.DeletedAt)) == null);
    }
}
