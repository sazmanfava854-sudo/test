using HRPerformance.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Infrastructure.Services;

public static class AuthBootstrap
{
    public const string DefaultAdminUserName = "admin";
    public const string DefaultAdminPassword = "Admin@123";

    public static async Task EnsureDefaultAdminPasswordAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AuthBootstrap");

        ApplicationUser? user;
        try
        {
            user = await userManager.FindByNameAsync(DefaultAdminUserName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not query admin user — database may not be initialized yet.");
            return;
        }

        if (user == null)
        {
            logger.LogWarning(
                "کاربر admin یافت نشد. اسکریپت database/08_SeedData.sql را روی SQL Server اجرا کنید.");
            return;
        }

        if (await userManager.CheckPasswordAsync(user, DefaultAdminPassword))
            return;

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, DefaultAdminPassword);
        if (result.Succeeded)
        {
            logger.LogWarning(
                "رمز عبور admin به {Password} بازنشانی شد (hash قبلی نامعتبر بود).",
                DefaultAdminPassword);
            return;
        }

        logger.LogError(
            "بازنشانی رمز admin ناموفق بود: {Errors}",
            string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
