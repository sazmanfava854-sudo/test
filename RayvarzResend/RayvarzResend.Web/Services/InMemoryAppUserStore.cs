using System.Collections.Concurrent;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>ذخیره کاربر در حافظه — فقط برای توسعه/تست وقتی SQL در دسترس نیست.</summary>
public sealed class InMemoryAppUserStore
{
    private readonly ConcurrentDictionary<string, AppUserRecord> _byUsername = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, AppUserRecord> _byId = new();
    private readonly ConcurrentDictionary<Guid, AppUserGroupRecord> _groups = new();
    private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _userGroups = new();

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
        _userGroups[user.Id] = [];
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
                CreatedAtUtc = u.CreatedAtUtc.ToString("O"),
                GroupIds = GetUserGroupIds(u.Id)
            })
            .ToList();

    public List<AppUserGroupDto> ListGroups() =>
        _groups.Values
            .OrderBy(g => g.Name)
            .Select(g => new AppUserGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                CanAccessUnsentFiches = g.CanAccessUnsentFiches,
                CanAccessInstallment = g.CanAccessInstallment,
                CanAccessFicheDateChange = g.CanAccessFicheDateChange,
                CanAccessBankInquiryConfirm = g.CanAccessBankInquiryConfirm,
                CanManageUsers = g.CanManageUsers,
                CreatedAtUtc = g.CreatedAtUtc.ToString("O")
            })
            .ToList();

    public AppUserGroupDto AddGroup(CreateAppUserGroupRequest req)
    {
        var name = (req.Name ?? "").Trim();
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("نام گروه الزامی است");
        if (_groups.Values.Any(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("گروه با این نام قبلاً ثبت شده است");

        var group = new AppUserGroupRecord
        {
            Id = Guid.NewGuid(),
            Name = name,
            CanAccessUnsentFiches = req.CanAccessUnsentFiches,
            CanAccessInstallment = req.CanAccessInstallment,
            CanAccessFicheDateChange = req.CanAccessFicheDateChange,
            CanAccessBankInquiryConfirm = req.CanAccessBankInquiryConfirm,
            CanManageUsers = req.CanManageUsers,
            CreatedAtUtc = DateTime.UtcNow
        };
        _groups[group.Id] = group;
        return new AppUserGroupDto
        {
            Id = group.Id,
            Name = group.Name,
            CanAccessUnsentFiches = group.CanAccessUnsentFiches,
            CanAccessInstallment = group.CanAccessInstallment,
            CanAccessFicheDateChange = group.CanAccessFicheDateChange,
            CanAccessBankInquiryConfirm = group.CanAccessBankInquiryConfirm,
            CanManageUsers = group.CanManageUsers,
            CreatedAtUtc = group.CreatedAtUtc.ToString("O")
        };
    }

    public AppUserGroupDto UpdateGroup(Guid id, UpdateAppUserGroupRequest req)
    {
        if (!_groups.TryGetValue(id, out var group))
            throw new InvalidOperationException("گروه یافت نشد");

        var name = (req.Name ?? "").Trim();
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("نام گروه الزامی است");

        group.Name = name;
        group.CanAccessUnsentFiches = req.CanAccessUnsentFiches;
        group.CanAccessInstallment = req.CanAccessInstallment;
        group.CanAccessFicheDateChange = req.CanAccessFicheDateChange;
        group.CanAccessBankInquiryConfirm = req.CanAccessBankInquiryConfirm;
        group.CanManageUsers = req.CanManageUsers;

        return new AppUserGroupDto
        {
            Id = group.Id,
            Name = group.Name,
            CanAccessUnsentFiches = group.CanAccessUnsentFiches,
            CanAccessInstallment = group.CanAccessInstallment,
            CanAccessFicheDateChange = group.CanAccessFicheDateChange,
            CanAccessBankInquiryConfirm = group.CanAccessBankInquiryConfirm,
            CanManageUsers = group.CanManageUsers,
            CreatedAtUtc = group.CreatedAtUtc.ToString("O")
        };
    }

    public List<Guid> GetUserGroupIds(Guid userId) =>
        _userGroups.TryGetValue(userId, out var set) ? set.ToList() : [];

    public void SetUserGroups(Guid userId, IReadOnlyList<Guid> groupIds)
    {
        if (!_byId.ContainsKey(userId))
            throw new InvalidOperationException("کاربر یافت نشد");
        _userGroups[userId] = groupIds.Distinct().ToHashSet();
    }

    public List<(Guid UserId, Guid GroupId)> ListMemberships() =>
        _userGroups.SelectMany(kv => kv.Value.Select(g => (kv.Key, g))).ToList();

    public AppUserDto UpdateUser(Guid id, UpdateAppUserRequest req)
    {
        if (!_byId.TryGetValue(id, out var user))
            throw new InvalidOperationException("کاربر یافت نشد");

        if (req.IsAdmin.HasValue)
            user.IsAdmin = req.IsAdmin.Value;
        if (req.IsActive.HasValue)
            user.IsActive = req.IsActive.Value;
        if (req.GroupIds != null)
            SetUserGroups(id, req.GroupIds);

        return List().First(u => u.Id == id);
    }

    public void ResetPassword(Guid id, string password)
    {
        if (!_byId.TryGetValue(id, out var user))
            throw new InvalidOperationException("کاربر یافت نشد");
        user.PasswordHash = PasswordHasherUtil.Hash(password);
    }
}
