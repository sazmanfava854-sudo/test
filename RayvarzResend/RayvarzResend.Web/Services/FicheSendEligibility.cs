using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>
/// قبل از ارسال (تکی/جمعی/تهاتر): اول رایورز، بعد Accounting_DocHeader — در هر دو نباید فیش باشد.
/// </summary>
public static class FicheSendEligibility
{
    public sealed record Result(
        bool InRayvarz,
        bool InAccountingDocHeader,
        bool RayvarzQueryFailed);

    public static bool IsBlocked(Result r) => r.InRayvarz || r.InAccountingDocHeader;

    public static string? BuildBlockReason(Result r)
    {
        if (r.RayvarzQueryFailed && !r.InRayvarz && !r.InAccountingDocHeader)
            return null;

        if (r.InRayvarz && r.InAccountingDocHeader)
            return "فیش در رایورز و Accounting_DocHeader موجود است — ارسال نشد";
        if (r.InRayvarz)
            return "فیش در رایورز موجود است — ارسال نشد";
        if (r.InAccountingDocHeader)
            return "فیش در Accounting_DocHeader موجود است — ارسال نشد";
        return null;
    }

    /// <summary>ترتیب: ۱) incmdocsys ۲) Accounting_DocHeader</summary>
    public static async Task<Result> CheckAsync(
        FicheRepository repo,
        FicheHeaderDto? fiche,
        string ficheNo,
        int? shamsiYear = null,
        CancellationToken ct = default)
    {
        ficheNo = TahatorRowBuilder.NormalizeFicheNo(ficheNo.Trim());

        var inRayvarz = false;
        var rayvarzQueryFailed = false;
        try
        {
            if (fiche != null && TahatorRowBuilder.IsTahatorFiche(fiche))
            {
                var years = RayvarzYearResolver.CollectCandidates(
                    fiche.RayvarzDocDate,
                    fiche.RayvarzActDate,
                    fiche.RayvarzDueDate).ToList();
                if (shamsiYear is > 0 && !years.Contains(shamsiYear.Value))
                    years.Add(shamsiYear.Value);
                inRayvarz = await repo.ExistsTahatorDocumentInRayvarzRobustAsync(
                    ficheNo,
                    TahatorRowBuilder.IsTahatorAmountFiche(fiche),
                    years,
                    ct);
            }
            else
            {
                inRayvarz = await repo.ExistsInRayvarzAsync(
                    ficheNo, shamsiYear > 0 ? shamsiYear : null, ct);
            }
        }
        catch (SqlException)
        {
            rayvarzQueryFailed = true;
        }

        var inHeader = await repo.ExistsInAccountingDocHeaderAsync(ficheNo, ct);
        return new Result(inRayvarz, inHeader, rayvarzQueryFailed);
    }
}
