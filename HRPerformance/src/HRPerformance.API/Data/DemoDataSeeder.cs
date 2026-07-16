using HRPerformance.Entities;
using HRPerformance.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Data;

public static class DemoDataSeeder
{
    public const string DefaultAdminUser = "admin";
    public const string DefaultAdminPassword = "Admin@123";
    public const string ManagerUser = "manager";
    public const string ManagerPassword = "Manager@123";
    public const string EmployeeUser = "employee";
    public const string EmployeePassword = "Employee@123";

    public static async Task SeedAsync(IServiceProvider services, ILogger logger, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        await context.Database.EnsureCreatedAsync(ct);
        if (await context.Organizations.AnyAsync(ct))
        {
            logger.LogInformation("Demo data already exists");
            return;
        }

        logger.LogInformation("Seeding demo data (InMemory — no SQL Server required)");

        var orgId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var deptId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var adminEmpId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var mgrEmpId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var empId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        context.Organizations.Add(new Organization
        {
            Id = orgId,
            Name = "شهرداری مشهد (دمو)",
            Code = "DEMO-001",
            IsActive = true
        });

        context.OrganizationUnits.Add(new OrganizationUnit
        {
            Id = deptId,
            OrganizationId = orgId,
            Name = "اداره منابع انسانی",
            Code = "HR-001",
            UnitType = OrganizationUnitType.Department,
            Level = 0,
            IsActive = true
        });

        context.Employees.AddRange(
            new Employee
            {
                Id = adminEmpId,
                OrganizationId = orgId,
                OrganizationUnitId = deptId,
                PersonnelCode = "10001",
                NationalCode = "0012345678",
                FirstName = "مدیر",
                LastName = "سیستم",
                EmploymentDate = DateTime.UtcNow.Date.AddYears(-2),
                EmploymentType = EmploymentType.Permanent,
                Position = "مدیر سیستم",
                Status = EmployeeStatus.Active,
                CurrentScore = 85,
                MonthlyScore = 12,
                YearlyScore = 85
            },
            new Employee
            {
                Id = mgrEmpId,
                OrganizationId = orgId,
                OrganizationUnitId = deptId,
                PersonnelCode = "10002",
                NationalCode = "0022345678",
                FirstName = "علی",
                LastName = "مدیری",
                EmploymentDate = DateTime.UtcNow.Date.AddYears(-3),
                EmploymentType = EmploymentType.Permanent,
                Position = "مدیر واحد",
                Status = EmployeeStatus.Active,
                CurrentScore = 78,
                MonthlyScore = 8,
                YearlyScore = 78
            },
            new Employee
            {
                Id = empId,
                OrganizationId = orgId,
                OrganizationUnitId = deptId,
                PersonnelCode = "10003",
                NationalCode = "0032345678",
                FirstName = "رضا",
                LastName = "کارمندی",
                EmploymentDate = DateTime.UtcNow.Date.AddYears(-1),
                EmploymentType = EmploymentType.Permanent,
                Position = "کارشناس",
                Status = EmployeeStatus.Active,
                ManagerId = mgrEmpId,
                CurrentScore = 72,
                MonthlyScore = 5,
                YearlyScore = 72
            });

        context.EvaluationCategories.Add(new EvaluationCategory
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            OrganizationId = orgId,
            Name = "انضباط",
            Description = "امتیاز انضباط کارکنان",
            Color = "#1976d2",
            Weight = 1,
            IsActive = true
        });

        context.Settings.AddRange(
            new Setting { Id = Guid.NewGuid(), OrganizationId = orgId, Key = "ScoreThreshold", Value = "60", Category = "Scoring", DataType = "decimal" },
            new Setting { Id = Guid.NewGuid(), OrganizationId = orgId, Key = "WorkingHoursStart", Value = "07:30", Category = "Attendance", DataType = "time" },
            new Setting { Id = Guid.NewGuid(), OrganizationId = orgId, Key = "WorkingHoursEnd", Value = "15:00", Category = "Attendance", DataType = "time" }
        );

        var today = DateTime.UtcNow.Date;
        context.AttendanceLogs.AddRange(
            new AttendanceLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                EmployeeId = empId,
                AttendanceDate = today,
                EntryTime = new TimeSpan(7, 45, 0),
                ExitTime = new TimeSpan(15, 0, 0),
                DelayMinutes = 15,
                IsAbsent = false,
                WorkingHours = 7.25m,
                Source = "Demo"
            },
            new AttendanceLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                EmployeeId = mgrEmpId,
                AttendanceDate = today,
                EntryTime = new TimeSpan(7, 20, 0),
                ExitTime = new TimeSpan(15, 0, 0),
                DelayMinutes = 0,
                IsAbsent = false,
                WorkingHours = 7.5m,
                Source = "Demo"
            }
        );

        await context.SaveChangesAsync(ct);

        await EnsureRoleAsync(roleManager, "SuperAdministrator", "مدیر کل سیستم");
        await EnsureRoleAsync(roleManager, "OrganizationAdministrator", "مدیر سازمان");
        await EnsureRoleAsync(roleManager, "Manager", "مدیر");
        await EnsureRoleAsync(roleManager, "Employee", "کارمند");

        await EnsureUserAsync(userManager, DefaultAdminUser, DefaultAdminPassword, "مدیر", "سیستم", orgId, adminEmpId, "SuperAdministrator");
        await EnsureUserAsync(userManager, ManagerUser, ManagerPassword, "علی", "مدیری", orgId, mgrEmpId, "Manager");
        await EnsureUserAsync(userManager, EmployeeUser, EmployeePassword, "رضا", "کارمندی", orgId, empId, "Employee");

        logger.LogInformation("Demo users: admin / {Password}, manager / {MgrPass}, employee / {EmpPass}",
            DefaultAdminPassword, ManagerPassword, EmployeePassword);
    }

    private static async Task EnsureRoleAsync(RoleManager<ApplicationRole> roleManager, string name, string description)
    {
        if (await roleManager.RoleExistsAsync(name)) return;
        await roleManager.CreateAsync(new ApplicationRole { Name = name, Description = description });
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string userName,
        string password,
        string firstName,
        string lastName,
        Guid orgId,
        Guid employeeId,
        string role)
    {
        if (await userManager.FindByNameAsync(userName) != null) return;

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = $"{userName}@demo.local",
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            OrganizationId = orgId,
            EmployeeId = employeeId,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create demo user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(user, role);
    }
}
