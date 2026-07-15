using HRPerformance.Application.Interfaces;
using HRPerformance.Domain.Interfaces;
using HRPerformance.Infrastructure.BackgroundServices;
using HRPerformance.Infrastructure.Data;
using HRPerformance.Infrastructure.Repositories;
using HRPerformance.Infrastructure.Services;
using HRPerformance.Infrastructure.Services.ExternalHr;
using HRPerformance.Infrastructure.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HRPerformance.Domain.Entities;

namespace HRPerformance.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
            options.Password.RequiredLength = 8; options.Password.RequireDigit = true; options.Password.RequireUppercase = true;
            options.Lockout.MaxFailedAccessAttempts = 5; options.User.RequireUniqueEmail = true;
        }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
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
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddHttpClient("AttendanceSync");
        services.AddHostedService<AttendanceSyncBackgroundService>();
        return services;
    }
}
