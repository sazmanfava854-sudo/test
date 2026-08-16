using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine;

/// <summary>
/// Stub محلی SaraBridge — همان قرارداد HTTP ولی با SoapBuilder (LegacyCSharp).
/// برای تست PayloadSource=RuleEngineBridge بدون Sara واقعی.
/// </summary>
public sealed class SaraBridgeStubService
{
    private readonly FicheRepository _fiches;
    private readonly SoapBuilder _soap;
    private readonly IConfiguration _config;

    public SaraBridgeStubService(FicheRepository fiches, SoapBuilder soap, IConfiguration config)
    {
        _fiches = fiches;
        _soap = soap;
        _config = config;
    }

    public int ConfiguredNidMember => _config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);

    public async Task<SaraBridgeBuildResponse> BuildAsync(SaraBridgeBuildRequest req, CancellationToken ct = default)
    {
        if (req.NidMember > 0 && req.NidMember != ConfiguredNidMember)
        {
            return new SaraBridgeBuildResponse
            {
                Error = $"NidMember={req.NidMember} پشتیبانی نمی‌شود — stub فقط Member {ConfiguredNidMember} را شبیه‌سازی می‌کند."
            };
        }

        FicheHeaderDto? fiche = null;
        if (!string.IsNullOrWhiteSpace(req.FicheNo))
            fiche = await _fiches.LoadAsync(IdentifierType.FicheNo, req.FicheNo.Trim(), ct);

        if (fiche == null)
        {
            return new SaraBridgeBuildResponse
            {
                Error = string.IsNullOrWhiteSpace(req.FicheNo)
                    ? "FicheNo یا NidFiche لازم است."
                    : $"فیش {req.FicheNo} در Sara یافت نشد."
            };
        }

        return BuildFromFiche(fiche, req);
    }

    public SaraBridgeBuildResponse BuildFromFiche(FicheHeaderDto fiche, SaraBridgeBuildRequest req)
    {
        if (req.NidMember > 0 && req.NidMember != ConfiguredNidMember)
        {
            return new SaraBridgeBuildResponse
            {
                Error = $"NidMember={req.NidMember} پشتیبانی نمی‌شود — stub فقط Member {ConfiguredNidMember} را شبیه‌سازی می‌کند."
            };
        }

        var branch = req.Branch;
        var fund = req.Fund;
        if (branch <= 0 && fiche.ResolvedDistrictBranch is > 0)
            branch = fiche.ResolvedDistrictBranch.Value;
        if (fund <= 0 && fiche.SuggestedFund is > 0)
            fund = fiche.SuggestedFund.Value;

        var xml = _soap.Build(fiche, branch, fund, req.DocDate, req.ActDate, req.DueDate);
        return new SaraBridgeBuildResponse
        {
            SoapXml = xml,
            Source = "LocalStub/LegacyCSharp",
            Warning = "Stub محلی — VB Member 1388 در Sara اجرا نشده؛ خروجی همان SoapBuilder است."
        };
    }
}
