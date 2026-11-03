using System.Collections.Concurrent;

namespace RayvarzResend.Web.Services;

/// <summary>قفل موقت پس از چند رمز اشتباه پیاپی برای یک نام کاربری از یک IP.</summary>
public sealed class LoginThrottle
{
    public const int MaxFailures = 5;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    private readonly ConcurrentDictionary<string, (int Failures, DateTime FirstFailureUtc)> _attempts = new();
    private readonly Func<DateTime> _clock;

    public LoginThrottle() : this(() => DateTime.UtcNow) { }

    internal LoginThrottle(Func<DateTime> clock) => _clock = clock;

    public static string Key(string? username, string? remoteIp) =>
        $"{(username ?? "").Trim().ToLowerInvariant()}|{remoteIp ?? ""}";

    public bool IsLocked(string key, out TimeSpan retryAfter)
    {
        retryAfter = TimeSpan.Zero;
        if (!_attempts.TryGetValue(key, out var entry))
            return false;

        var elapsed = _clock() - entry.FirstFailureUtc;
        if (elapsed >= Window)
        {
            _attempts.TryRemove(key, out _);
            return false;
        }

        if (entry.Failures < MaxFailures)
            return false;

        retryAfter = Window - elapsed;
        return true;
    }

    public void RecordFailure(string key)
    {
        var now = _clock();
        _attempts.AddOrUpdate(
            key,
            _ => (1, now),
            (_, e) => now - e.FirstFailureUtc >= Window ? (1, now) : (e.Failures + 1, e.FirstFailureUtc));
    }

    public void Reset(string key) => _attempts.TryRemove(key, out _);
}
