
using ManteqTask.Domain.Entities.Authentication;
using ManteqTask.Domain.Interfaces.Domain.Auditing;
namespace ManteqTask.Domain.Entities;


public class User : IBaseEntity
{
    public required Guid Id { get; init; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required DateTime CreatedAt { get; init; }
    public required Guid CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public required bool IsActive { get; set; }
    public required string SecurityStamp { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public required bool IsVerified { get; set; } = false;
}
