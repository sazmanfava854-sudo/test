using System.Text.RegularExpressions;

namespace RayvarzResend.Web.Services;

/// <summary>قوانین رمز عبور محلی دستیار مالی.</summary>
public static partial class PasswordPolicy
{
    public const string RequirementMessage =
        "رمز عبور باید حداقل ۸ کاراکتر و شامل حرف انگلیسی کوچک، حرف انگلیسی بزرگ و علامت (مثل @) باشد.";

    public static string? Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "رمز عبور الزامی است";
        if (password.Length < 8)
            return "رمز عبور حداقل ۸ کاراکتر باشد";
        if (!LowerEnglish().IsMatch(password))
            return "رمز عبور باید حداقل یک حرف انگلیسی کوچک داشته باشد";
        if (!UpperEnglish().IsMatch(password))
            return "رمز عبور باید حداقل یک حرف انگلیسی بزرگ داشته باشد";
        if (!SpecialChar().IsMatch(password))
            return "رمز عبور باید حداقل یک علامت (مثل @) داشته باشد";
        return null;
    }

    [GeneratedRegex("[a-z]")]
    private static partial Regex LowerEnglish();

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UpperEnglish();

    [GeneratedRegex(@"[!@#$%^&*()_+\-=\[\]{}|;:'"",.<>?/\\`~]")]
    private static partial Regex SpecialChar();
}
