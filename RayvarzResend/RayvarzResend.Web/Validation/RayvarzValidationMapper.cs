using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Validation;

public static class RayvarzValidationMapper
{
    public static RayvarzValidationResultDto ToDto(this RayvarzValidationResult result) => new()
    {
        CanSend = result.CanSend,
        Issues = result.Issues.Select(ToDto).ToList(),
        BlockingIssues = result.BlockingIssues.Select(ToDto).ToList(),
        Warnings = result.Warnings.Select(ToDto).ToList()
    };

    public static RayvarzValidationIssueDto ToDto(this RayvarzValidationIssue issue) => new()
    {
        Code = issue.Code,
        Field = issue.Field,
        Operation = issue.Operation,
        Severity = issue.Severity.ToString(),
        Blocking = issue.Blocking,
        Message = issue.Message
    };

    public static string FormatBlockingMessage(RayvarzValidationResult result) =>
        string.Join("; ", result.BlockingIssues.Select(i => $"[{i.Code}] {i.Message}"));
}
