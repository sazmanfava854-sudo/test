using System.Collections.Concurrent;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>ذخیره کاربر در حافظه — فقط برای توسعه/تست وقتی SQL در دسترس نیست.</summary>
public sealed class InMemoryAppUserStore
{
    private readonly ConcurrentDictionary<string, AppUserRecord> _byUsername = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, AppUserRecord> _byId = new();

    public int Count => _byUsername.Count;

    public AppUserRecord Add(CreateAppUserRequest req)
    {
        AppUserInputNormalizer.ValidateAndApply(req);
        var username = req.Username!;
        if (_byUsername.ContainsKey(username))
            throw new InvalidOperationException("کاربر با این کد ملی قبلاً ثبت شده است");

        var user = new AppUserRecord
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = PasswordHasherUtil.Hash(req.Password!),
            FirstName = (req.FirstName ?? "").Trim(),
            LastName = (req.LastName ?? "").Trim(),
            NationalId = (req.NationalId ?? "").Trim(),
            Position = (req.Position ?? "").Trim(),
            District = (req.District ?? "").Trim(),
            IsAdmin = req.IsAdmin,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _byUsername[user.Username] = user;
        _byId[user.Id] = user;
        return user;
    }

    public AppUserRecord? FindByUsername(string username) =>
        _byUsername.TryGetValue(username.Trim(), out var user) ? user : null;

    public AppUserRecord? FindById(Guid id) =>
        _byId.TryGetValue(id, out var user) ? user : null;

    public List<AppUserDto> List() =>
        _byUsername.Values
            .OrderByDescending(u => u.CreatedAtUtc)
            .Select(u => new AppUserDto
            {
                Id = u.Id,
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                NationalId = u.NationalId,
                Position = u.Position,
                District = u.District,
                IsAdmin = u.IsAdmin,
                IsActive = u.IsActive,
                CreatedAtUtc = u.CreatedAtUtc.ToString("O")
            })
            .ToList();
}
