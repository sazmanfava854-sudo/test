using System.Text.Json;
using System.Text.Json.Serialization;
using IoTRecommendation.Web.Session;

namespace IoTRecommendation.Web.Extensions;

public static class SessionExtensions
{
    private const string SessionKey = "WorkflowSession";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static WorkflowSession GetWorkflowSession(this ISession session)
    {
        var json = session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json))
            return new WorkflowSession();
        return JsonSerializer.Deserialize<WorkflowSession>(json, Options) ?? new WorkflowSession();
    }

    public static void SetWorkflowSession(this ISession session, WorkflowSession data)
    {
        session.SetString(SessionKey, JsonSerializer.Serialize(data, Options));
    }
}
