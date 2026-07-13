using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FicheRepository>();
builder.Services.AddSingleton<SoapBuilder>();
builder.Services.AddSingleton<RayvarzClient>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/config", (IConfiguration config) => new
{
    dryRun = config.GetValue<bool>("Rayvarz:DryRun"),
    serviceUrl = config["Rayvarz:ServiceUrl"],
    sourceSystemId = config["Rayvarz:SourceSystemId"] ?? "11111",
    branches = new[] {
        new { id = 201, name = "منطقه 1", fund = 200201012 },
        new { id = 202, name = "منطقه 2", fund = 200202012 },
        new { id = 203, name = "منطقه 3", fund = 200203013 },
        new { id = 204, name = "منطقه 4", fund = 200204017 },
        new { id = 205, name = "منطقه 5", fund = 200205008 },
        new { id = 206, name = "منطقه 6", fund = 200206006 },
        new { id = 207, name = "منطقه 7", fund = 200207009 },
        new { id = 208, name = "منطقه 8", fund = 200208010 },
        new { id = 209, name = "منطقه 9", fund = 200209004 },
        new { id = 210, name = "منطقه 10", fund = 200210020 },
        new { id = 211, name = "منطقه 11", fund = 200211007 },
        new { id = 212, name = "منطقه 12", fund = 200212004 },
        new { id = 218, name = "منطقه ثامن", fund = 200218011 }
    }
});

app.MapPost("/api/fiche/load", async (LoadFicheRequest req, FicheRepository repo, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.IdentifierValue))
        return Results.BadRequest(new { error = "شناسه فیش خالی است" });

    try
    {
        var fiche = await repo.LoadAsync(req.IdentifierType, req.IdentifierValue.Trim(), ct);
        if (fiche == null)
            return Results.NotFound(new { error = "فیش در Income_Fiche یا Duty_Fiche یافت نشد" });

        fiche.ExistsInRayvarz = await repo.ExistsInRayvarzAsync(fiche.FicheNo, ct);
        if (fiche.ExistsInRayvarz)
            fiche.StatusMessage = "تکراری — در رایورز موجود است";
        else if (fiche.Payable <= 0)
            fiche.StatusMessage = "مبلغ قابل پرداخت صفر است";
        else if (fiche.Rows.Count == 0)
            fiche.StatusMessage = "ردیف IncmNo یافت نشد";
        else
            fiche.StatusMessage = "آماده ارسال";

        return Results.Ok(fiche);
    }
    catch (Exception ex)
    {
        return Results.Problem($"خطا در بارگذاری: {ex.Message}");
    }
});

app.MapPost("/api/fiche/preview", (SendFicheRequest req, SoapBuilder soap) =>
{
    var xml = soap.Build(req.Fiche, req.Branch, req.Fund, req.DocDate);
    return Results.Ok(new { xml });
});

app.MapPost("/api/fiche/send", async (SendFicheRequest req, FicheRepository repo, SoapBuilder soap, RayvarzClient client, IConfiguration config, CancellationToken ct) =>
{
    var fiche = req.Fiche;
    if (fiche.ExistsInRayvarz || await repo.ExistsInRayvarzAsync(fiche.FicheNo, ct))
        return Results.BadRequest(new { error = "فیش در رایورز موجود است — ارسال نشد" });

    if (req.ResetStatus)
    {
        try { await repo.ResetStatusAsync(fiche, ct); }
        catch (Exception ex) { return Results.Problem($"خطا در ریست وضعیت: {ex.Message}"); }
    }

    var xml = soap.Build(fiche, req.Branch, req.Fund, req.DocDate);
    var dryRun = config.GetValue<bool>("Rayvarz:DryRun");
    var result = await client.SendAsync(xml, dryRun, ct);

    if (!dryRun && result.Success)
        result.VerifiedInRayvarz = await repo.ExistsInRayvarzAsync(fiche.FicheNo, ct);

    if (!dryRun && !result.VerifiedInRayvarz)
        result.DocNotSentError = await repo.GetDocNotSentErrorAsync(fiche.FicheNo, ct);

    return Results.Ok(result);
});

app.Run();
