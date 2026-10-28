using System.Collections.Concurrent;

namespace RayvarzResend.Web.Services;

/// <summary>
/// Rayvarz:DryRun کلید اصلی است: اگر روشن باشد هیچ مسیری (تکی، جمعی، تهاتر، واسط Sara) واقعی ارسال/ثبت نمی‌کند.
/// کلیدهای Tahator:DryRun و AccountingDoc:DryRun فقط می‌توانند همان مسیر را هم آزمایشی کنند، نه برعکس.
/// </summary>
public static class SendDryRun
{
    public static bool Rayvarz(IConfiguration config) => config.GetValue<bool>("Rayvarz:DryRun");

    public static bool Tahator(IConfiguration config) =>
        Rayvarz(config) || config.GetValue<bool>("Tahator:DryRun");

    public static bool AccountingDoc(IConfiguration config) =>
        Rayvarz(config) || config.GetValue<bool>("AccountingDoc:DryRun");
}

/// <summary>جلوگیری از ارسال هم‌زمان یک فیش (دو کلیک، یا تکی و جمعی هم‌زمان) — چک تکراری بین بررسی و INSERT قفل ندارد.</summary>
public static class FicheSendLock
{
    public const string BusyMessage = "ارسال این فیش هم‌اکنون در حال انجام است — چند لحظه بعد نتیجه را بررسی کنید";

    private static readonly ConcurrentDictionary<string, byte> InFlight = new(StringComparer.Ordinal);

    public static IDisposable? TryAcquire(string? ficheNo)
    {
        var key = (ficheNo ?? "").Trim();
        if (key.Length == 0)
            return new Releaser(null);
        return InFlight.TryAdd(key, 0) ? new Releaser(key) : null;
    }

    private sealed class Releaser(string? key) : IDisposable
    {
        private string? _key = key;

        public void Dispose()
        {
            var k = Interlocked.Exchange(ref _key, null);
            if (k != null)
                InFlight.TryRemove(k, out _);
        }
    }
}
