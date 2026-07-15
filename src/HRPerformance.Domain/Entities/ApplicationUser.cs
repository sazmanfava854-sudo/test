using HRPerformance.Domain.Common;
using HRPerformance.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace HRPerformance.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public Guid? EmployeeId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Organization? Organization { get; set; }
    public Employee? Employee { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public string FullName => $"{FirstName} {LastName}";
}
