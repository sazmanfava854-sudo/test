using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class V48SafetyFixesTests
{
    [Fact]
    public void Demoted_admin_without_supplied_permissions_loses_all_granted()
    {
        var user = new AppUserRecord { IsAdmin = true, DirectPermissions = AppUserDirectPermissions.AllGranted() };

        AppUserUpdateRules.Apply(user, new UpdateAppUserRequest { IsAdmin = false });

        Assert.False(user.IsAdmin);
        Assert.False(user.DirectPermissions.CanAccessUnsentFiches);
        Assert.False(user.DirectPermissions.CanManageUsers);
    }

    [Fact]
    public void Non_admin_update_keeps_permissions_when_not_supplied()
    {
        var perms = new AppUserDirectPermissions { CanAccessUnsentFiches = true };
        var user = new AppUserRecord { IsAdmin = false, DirectPermissions = perms };

        AppUserUpdateRules.Apply(user, new UpdateAppUserRequest { IsActive = false });

        Assert.True(user.DirectPermissions.CanAccessUnsentFiches);
        Assert.False(user.IsActive);
    }

    [Theory]
    [InlineData(true, true, true, false)]
    [InlineData(false, false, null, false)]
    [InlineData(false, false, false, false)]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, null, true)]
    public void Only_admin_may_grant_or_touch_admin_accounts(bool callerIsAdmin, bool targetIsAdmin, bool? requested, bool denied)
    {
        var message = AppUserUpdateRules.DenyAdminChangeByNonAdmin(callerIsAdmin, targetIsAdmin, requested);
        Assert.Equal(denied, message != null);
    }

    [Fact]
    public void Login_throttle_locks_after_max_failures_and_expires()
    {
        var now = new DateTime(2026, 10, 28, 8, 0, 0, DateTimeKind.Utc);
        var throttle = new LoginThrottle(() => now);
        var key = LoginThrottle.Key("Admin", "10.0.0.1");

        for (var i = 0; i < LoginThrottle.MaxFailures - 1; i++)
            throttle.RecordFailure(key);
        Assert.False(throttle.IsLocked(key, out _));

        throttle.RecordFailure(key);
        Assert.True(throttle.IsLocked(key, out var retryAfter));
        Assert.True(retryAfter > TimeSpan.Zero);
        Assert.False(throttle.IsLocked(LoginThrottle.Key("admin", "10.0.0.2"), out _));

        now = now.Add(LoginThrottle.Window).AddSeconds(1);
        Assert.False(throttle.IsLocked(key, out _));
    }

    [Fact]
    public void Login_throttle_reset_clears_failures()
    {
        var throttle = new LoginThrottle(() => DateTime.UtcNow);
        var key = LoginThrottle.Key("user", "ip");
        for (var i = 0; i < LoginThrottle.MaxFailures; i++)
            throttle.RecordFailure(key);

        throttle.Reset(key);

        Assert.False(throttle.IsLocked(key, out _));
    }

    [Theory]
    [InlineData("""{"intResualt":1,"strResualt":"پرداخت ناموفق","pay":0}""")]
    [InlineData("""{"intResualt":0,"strResualt":"قبض پرداخت نشده است","pay":0}""")]
    [InlineData("""{"intResualt":1,"strResualt":"تراکنش لغو شد","pay":0}""")]
    public void Online_bank_negative_messages_are_not_paid(string raw)
    {
        var step = BankInquiryResponseParser.ParseOnlineBankStep(raw, 200);
        Assert.NotEqual(BankInquiryStepKind.Paid, step.Kind);
    }

    [Fact]
    public void Fiche_lookup_isPay_zero_with_only_payment_date_is_not_paid()
    {
        var raw = """{"intResult":0,"strResult":"","isPay":0,"paymentDate":"1405/04/23","fichesId":5}""";
        var step = BankInquiryResponseParser.ParseFicheLookupStep(raw, 200);
        Assert.NotEqual(BankInquiryStepKind.Paid, step.Kind);
    }

    [Theory]
    [InlineData("قبض مورد نظر پرداخت شده است", true)]
    [InlineData("پرداخت موفق", true)]
    [InlineData("پرداخت ناموفق", false)]
    [InlineData("پرداخت نشده", false)]
    [InlineData("موفق", false)]
    [InlineData("", false)]
    public void Paid_message_hint_respects_negation(string message, bool expected)
    {
        Assert.Equal(expected, BankInquiryResponseParser.LooksLikePaidMessage(message));
    }

    [Fact]
    public void Tracking_where_clause_is_parenthesized_for_AND_composition()
    {
        var clause = InstallmentIdentifierDetector.BuildTrackingNoWhereClause("TrackingNo");
        Assert.StartsWith("(", clause);
        Assert.EndsWith(")", clause);
        var depth = 0;
        for (var i = 0; i < clause.Length; i++)
        {
            depth += clause[i] == '(' ? 1 : clause[i] == ')' ? -1 : 0;
            if (depth == 0)
                Assert.Equal(clause.Length - 1, i);
        }
    }

    [Theory]
    [InlineData("۰۱۲۳۴۵", "012345")]
    [InlineData("٠١٢٣", "0123")]
    [InlineData("12-34 ۵۶", "123456")]
    public void ExtractDigits_normalizes_persian_and_arabic_digits(string raw, string expected)
    {
        Assert.Equal(expected, InstallmentIdentifierDetector.ExtractDigits(raw));
    }

    [Theory]
    [InlineData("fava")]
    [InlineData("FAVA")]
    [InlineData(" فاوا ")]
    public void NormalizeDistrict_recognizes_fava(string raw)
    {
        Assert.Equal(DistrictAccessService.FavaDistrictCode, DistrictAccessService.NormalizeDistrict(raw));
    }

    [Fact]
    public void NormalizeDistrict_converts_persian_digits()
    {
        Assert.Equal("5", DistrictAccessService.NormalizeDistrict("۲۰۵"));
    }

    [Fact]
    public void Unsent_scope_forces_regional_user_district()
    {
        var req = new UnsentFicheSearchRequest { District = "7", FicheKind = UnsentFicheKind.Income };

        Assert.True(UnsentFicheService.ApplyUserDistrictScope(req, User(district: "203")));

        Assert.Equal("3", req.District);
        Assert.Null(req.RestrictIncomeAccountGroup);
    }

    [Fact]
    public void Unsent_scope_center_user_sees_only_tahator_157_income()
    {
        var income = new UnsentFicheSearchRequest { District = "4", FicheKind = UnsentFicheKind.Income };
        Assert.True(UnsentFicheService.ApplyUserDistrictScope(income, User(district: "102")));
        Assert.Null(income.District);
        Assert.Equal(TahatorRowBuilder.IncomeAccountGroupTahatorAmount, income.RestrictIncomeAccountGroup);

        var duty = new UnsentFicheSearchRequest { FicheKind = UnsentFicheKind.Duty };
        Assert.False(UnsentFicheService.ApplyUserDistrictScope(duty, User(district: "102")));
    }

    [Fact]
    public void Unsent_scope_user_without_district_gets_nothing()
    {
        var req = new UnsentFicheSearchRequest { FicheKind = UnsentFicheKind.Income };
        Assert.False(UnsentFicheService.ApplyUserDistrictScope(req, User(district: "")));
    }

    [Fact]
    public void Unsent_scope_admin_and_fava_keep_requested_district()
    {
        var adminReq = new UnsentFicheSearchRequest { District = "7", FicheKind = UnsentFicheKind.Income };
        Assert.True(UnsentFicheService.ApplyUserDistrictScope(adminReq, User(district: "", admin: true)));
        Assert.Equal("7", adminReq.District);

        var favaReq = new UnsentFicheSearchRequest { District = "9", FicheKind = UnsentFicheKind.Income };
        Assert.True(UnsentFicheService.ApplyUserDistrictScope(favaReq, User(district: "fava")));
        Assert.Equal("9", favaReq.District);
    }

    [Fact]
    public void Income_fiche_scope_is_null_for_admin_and_exists_for_regional_user()
    {
        Assert.Null(DistrictAccessService.BuildIncomeFicheScopeClause(User(district: "", admin: true)));
        Assert.Null(DistrictAccessService.BuildIncomeFicheScopeClause(User(district: "fava")));

        var regional = DistrictAccessService.BuildIncomeFicheScopeClause(User(district: "3"));
        Assert.NotNull(regional);
        Assert.Contains("f.NidIncome", regional.Value.Clause);
        Assert.NotEmpty(regional.Value.Parameters);
    }

    [Theory]
    [InlineData(false, false, false, false, false)]
    [InlineData(true, false, false, true, true)]
    [InlineData(false, true, false, true, false)]
    [InlineData(false, false, true, false, true)]
    public void Rayvarz_dry_run_forces_every_send_path_into_dry_run(
        bool rayvarz, bool tahator, bool accounting, bool expectTahator, bool expectAccounting)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rayvarz:DryRun"] = rayvarz.ToString(),
                ["Tahator:DryRun"] = tahator.ToString(),
                ["AccountingDoc:DryRun"] = accounting.ToString()
            })
            .Build();

        Assert.Equal(expectTahator, SendDryRun.Tahator(config));
        Assert.Equal(expectAccounting, SendDryRun.AccountingDoc(config));
    }

    [Fact]
    public void Fiche_send_lock_rejects_concurrent_send_of_same_fiche()
    {
        var ficheNo = "lock-test-" + Guid.NewGuid().ToString("N");
        using (var first = FicheSendLock.TryAcquire(ficheNo))
        {
            Assert.NotNull(first);
            Assert.Null(FicheSendLock.TryAcquire(ficheNo));
            using var other = FicheSendLock.TryAcquire(ficheNo + "-x");
            Assert.NotNull(other);
        }

        using var again = FicheSendLock.TryAcquire(ficheNo);
        Assert.NotNull(again);
    }

    [Fact]
    public void PickReloaded_matches_client_nid_fiche_only()
    {
        var a = new FicheHeaderDto { FicheNo = "1", NidFiche = Guid.NewGuid() };
        var b = new FicheHeaderDto { FicheNo = "1", NidFiche = Guid.NewGuid() };

        Assert.Same(b, FicheSendService.PickReloaded(new[] { a, b }, new FicheHeaderDto { NidFiche = b.NidFiche }));
        Assert.Null(FicheSendService.PickReloaded(new[] { a }, new FicheHeaderDto { NidFiche = Guid.NewGuid() }));
        Assert.Same(a, FicheSendService.PickReloaded(new[] { a, b }, new FicheHeaderDto()));
        Assert.Null(FicheSendService.PickReloaded(Array.Empty<FicheHeaderDto>(), new FicheHeaderDto()));
    }

    [Fact]
    public void Principal_mismatch_detected_after_demotion_or_district_change()
    {
        var user = new AppUserRecord { Id = Guid.NewGuid(), Username = "u", FirstName = "a", LastName = "b", District = "3", IsAdmin = true };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(AuthClaimTypes.IsAdmin, "1"),
            new Claim(AuthClaimTypes.District, "3"),
            new Claim(AuthClaimTypes.DisplayName, AppAuthService.BuildDisplayName(user))
        }, "test"));

        Assert.True(AppAuthService.PrincipalMatches(principal, user));

        user.IsAdmin = false;
        Assert.False(AppAuthService.PrincipalMatches(principal, user));

        user.IsAdmin = true;
        user.District = "4";
        Assert.False(AppAuthService.PrincipalMatches(principal, user));
    }

    private static ClaimsPrincipal User(string district, bool admin = false)
    {
        var claims = new List<Claim>
        {
            new(AuthClaimTypes.District, district),
            new(AuthClaimTypes.IsAdmin, admin ? "1" : "0")
        };
        if (admin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
