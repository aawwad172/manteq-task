namespace ManteqTask.Infrastructure.Configurations.Seed;

public static class AuthSeedConstants
{
    // WARNING: These GUIDs are placeholder examples. 
    // They are unique, static, and fixed, which is what EF Core needs.

    // --- System & Admin User IDs ---
    // User ID: System/Automation User
    public static readonly Guid SystemUserId = new("A0000000-0000-7000-8000-000000000000");

    // User ID: Initial Super Admin User
    public static readonly Guid InitialAdminUserId = new("11111111-1111-7111-8111-111111111111");

    public static readonly string SystemSecurityStampGuid = "0199ecd3-b844-792f-8f83-431df66c629d";

    public static readonly string AdminSecurityStampGuid = "0199ecd4-f5b6-7211-9ec7-ce26d0966b72";
    // --- Role IDs ---
    // Role ID: SuperAdmin
    public static readonly Guid RoleIdSuperAdmin = new("22222222-2222-7222-8222-222222222222");

    // Role ID: Admin
    public static readonly Guid RoleIdAdmin = new("33333333-3333-7333-8333-333333333333");

    // Role ID: Standard User
    public static readonly Guid RoleIdUser = new("44444444-4444-7444-8444-444444444444");

    // Role ID: Doctor
    public static readonly Guid RoleIdDoctor = new("77777777-7777-7777-8777-777777777777");

    // --- Request workflow permissions ---
    // Permission ID: requests.create
    public static readonly Guid PermissionIdRequestsCreate = new("88888888-8888-7888-8888-888888888888");

    // Permission ID: requests.edit
    public static readonly Guid PermissionIdRequestsEdit = new("99999999-9999-7999-8999-999999999999");

    // Permission ID: requests.submit
    public static readonly Guid PermissionIdRequestsSubmit = new("aaaaaaaa-aaaa-7aaa-8aaa-aaaaaaaaaaaa");

    // Permission ID: requests.view.own
    public static readonly Guid PermissionIdRequestsViewOwn = new("bbbbbbbb-bbbb-7bbb-8bbb-bbbbbbbbbbbb");

    // Permission ID: requests.view.all
    public static readonly Guid PermissionIdRequestsViewAll = new("cccccccc-cccc-7ccc-8ccc-cccccccccccc");

    // Permission ID: requests.approve
    public static readonly Guid PermissionIdRequestsApprove = new("dddddddd-dddd-7ddd-8ddd-dddddddddddd");

    // Permission ID: requests.reject
    public static readonly Guid PermissionIdRequestsReject = new("eeeeeeee-eeee-7eee-8eee-eeeeeeeeeeee");

    // Permission ID: audit.view
    public static readonly Guid PermissionIdAuditView = new("ffffffff-ffff-7fff-8fff-ffffffffffff");

    public static readonly DateTime SeedDateUtc = new(2025, 10, 15, 0, 0, 0, DateTimeKind.Utc);

}
