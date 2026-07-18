using HRPerformance.BackgroundServices;
using HRPerformance.Data;
using HRPerformance.Entities;
using HRPerformance.Interfaces;
using HRPerformance.Repositories;
using HRPerformance.Services;
using HRPerformance.Services.App;
using HRPerformance.Services.ExternalHr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance;

public static class DependencyInjection
{
    public static IServiceCollection AddHrPerformance(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.User.RequireUniqueEmail = true;
        }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IRuleEngineService, RuleEngineService>();
        services.AddScoped<IRankingService, RankingService>();
        services.AddScoped<IAttendanceSyncService, AttendanceSyncService>();
        services.AddScoped<MisHrDataReader>();
        services.AddScoped<MisHrEmployeeSyncService>();
        services.AddScoped<MisSyncStateService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        services.AddScoped<AuthService>();
        services.AddScoped<EmployeeService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<AppealService>();
        services.AddScoped<EvaluationService>();
        services.AddScoped<NotificationAppService>();
        services.AddScoped<SettingService>();

        services.AddHttpClient("AttendanceSync");

        if (configuration.GetValue<bool>("HrIntegration:Enabled"))
            services.AddHostedService<AttendanceSyncBackgroundService>();

        return services;
    }
}
