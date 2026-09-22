using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class AppUserInputNormalizerTests
{
    [Fact]
    public void ResolveLoginUsername_uses_national_id_when_username_empty()
    {
        var req = new CreateAppUserRequest
        {
            NationalId = "1234567890",
            FirstName = "علی",
            LastName = "رضایی",
            Password = "secret1",
            Domain = "hoseine-sh",
            District = "2"
        };
        AppUserInputNormalizer.ValidateAndApply(req);
        Assert.Equal("1234567890", req.Username);
        Assert.Equal("hoseine-sh", req.Domain);
    }

    [Fact]
    public void ValidateAndApply_normalizes_windows_domain_prefix()
    {
        var req = new CreateAppUserRequest
        {
            NationalId = "1234567890",
            FirstName = "علی",
            LastName = "رضایی",
            Password = "secret1",
            Domain = @"MASHHAD\hoseine-sh",
            District = "2"
        };
        AppUserInputNormalizer.ValidateAndApply(req);
        Assert.Equal("hoseine-sh", req.Domain);
    }

    [Fact]
    public void ValidateAndApply_rejects_missing_domain()
    {
        var req = new CreateAppUserRequest
        {
            NationalId = "1234567890",
            FirstName = "علی",
            LastName = "رضایی",
            Password = "secret1",
            District = "2"
        };
        var ex = Assert.Throws<ArgumentException>(() => AppUserInputNormalizer.ValidateAndApply(req));
        Assert.Contains("دامین", ex.Message);
    }

    [Fact]
    public void ValidateAndApply_rejects_invalid_national_id()
    {
        var req = new CreateAppUserRequest
        {
            NationalId = "123",
            FirstName = "علی",
            LastName = "رضایی",
            Password = "secret1",
            District = "2"
        };
        Assert.Throws<ArgumentException>(() => AppUserInputNormalizer.ValidateAndApply(req));
    }
}
