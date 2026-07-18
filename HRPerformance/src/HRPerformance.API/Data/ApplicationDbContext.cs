using HRPerformance.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
    IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EvaluationCategory> EvaluationCategories => Set<EvaluationCategory>();
    public DbSet<EvaluationItem> EvaluationItems => Set<EvaluationItem>();
    public DbSet<EvaluationRule> EvaluationRules => Set<EvaluationRule>();
    public DbSet<AttendanceLog> AttendanceLogs => Set<AttendanceLog>();
    public DbSet<AttendanceSyncLog> AttendanceSyncLogs => Set<AttendanceSyncLog>();
    public DbSet<EmployeeScore> EmployeeScores => Set<EmployeeScore>();
    public DbSet<EmployeeEvaluation> EmployeeEvaluations => Set<EmployeeEvaluation>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Appeal> Appeals => Set<Appeal>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<EmployeeTimeline> EmployeeTimelines => Set<EmployeeTimeline>();
    public DbSet<Ranking> Rankings => Set<Ranking>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<AttendanceIntegrationSetting> AttendanceIntegrationSettings => Set<AttendanceIntegrationSetting>();
    public DbSet<HrMisSyncState> HrMisSyncStates => Set<HrMisSyncState>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        builder.Entity<Employee>().HasIndex(e => new { e.OrganizationId, e.PersonnelCode }).IsUnique();
        builder.Entity<Employee>().HasOne(e => e.Manager).WithMany(e => e.Subordinates).HasForeignKey(e => e.ManagerId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<OrganizationUnit>().HasOne(u => u.Parent).WithMany(u => u.Children).HasForeignKey(u => u.ParentId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
        builder.Entity<Setting>().HasIndex(s => new { s.OrganizationId, s.Key }).IsUnique();
        builder.Entity<AttendanceLog>().HasIndex(a => new { a.OrganizationId, a.ExternalId })
            .IsUnique()
            .HasFilter("[ExternalId] IS NOT NULL");
        builder.Entity<AttendanceLog>().HasIndex(a => new { a.EmployeeId, a.AttendanceDate });
        builder.Entity<RefreshToken>().HasIndex(r => r.Token);
        builder.Entity<HrMisSyncState>().HasKey(s => s.OrganizationId);

        // Match SQL DECIMAL types from database/02_Tables.sql
        builder.Entity<AlertRule>().Property(e => e.Threshold).HasPrecision(10, 2);
        builder.Entity<AttendanceLog>().Property(e => e.WorkingHours).HasPrecision(5, 2);
        builder.Entity<AttendanceLog>().Property(e => e.OvertimeHours).HasPrecision(5, 2);
        builder.Entity<Employee>().Property(e => e.CurrentScore).HasPrecision(10, 2);
        builder.Entity<Employee>().Property(e => e.MonthlyScore).HasPrecision(10, 2);
        builder.Entity<Employee>().Property(e => e.YearlyScore).HasPrecision(10, 2);
        builder.Entity<EmployeeEvaluation>().Property(e => e.Score).HasPrecision(10, 2);
        builder.Entity<EmployeeScore>().Property(e => e.Score).HasPrecision(10, 2);
        builder.Entity<EvaluationCategory>().Property(e => e.Weight).HasPrecision(5, 2);
        builder.Entity<EvaluationItem>().Property(e => e.DefaultScore).HasPrecision(10, 2);
        builder.Entity<EvaluationItem>().Property(e => e.MaxScore).HasPrecision(10, 2);
        builder.Entity<EvaluationItem>().Property(e => e.MinScore).HasPrecision(10, 2);
        builder.Entity<EvaluationItem>().Property(e => e.Weight).HasPrecision(5, 2);
        builder.Entity<EvaluationRule>().Property(e => e.MinValue).HasPrecision(10, 2);
        builder.Entity<EvaluationRule>().Property(e => e.MaxValue).HasPrecision(10, 2);
        builder.Entity<EvaluationRule>().Property(e => e.ScoreImpact).HasPrecision(10, 2);
        builder.Entity<Ranking>().Property(e => e.Score).HasPrecision(10, 2);
    }
}
