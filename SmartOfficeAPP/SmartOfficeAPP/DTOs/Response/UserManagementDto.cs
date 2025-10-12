namespace SmartOfficeAPP.DTOs.Response;

public class UserDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedDate { get; set; }
    public DateTime LastLogin { get; set; }
    public bool IsActive => !LockoutEnabled || (LockoutEnd == null || LockoutEnd <= DateTimeOffset.UtcNow);
}

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class UserStatisticsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int LockedUsers { get; set; }
    public int UnconfirmedEmails { get; set; }
    public int TotalRoles { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    public Dictionary<string, int> NewUsersLast30Days { get; set; } = new();
}