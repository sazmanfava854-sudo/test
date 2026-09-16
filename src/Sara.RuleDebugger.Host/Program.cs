using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Sara.RuleDebugger;

namespace Sara.RuleDebugger.Host;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public static int Main(string[] args)
    {
        if (args.Any(a => string.Equals(a, "--self-test", StringComparison.OrdinalIgnoreCase)))
            return DemoSelfTest();

        int port = 17890;
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "--port" && int.TryParse(args[i + 1], out int p) && p > 0)
                port = p;

        var svc = new RuleDebuggerService();
        var listener = Bind(port);
        Console.WriteLine("Sara Rule Debugger");
        Console.WriteLine(listener.Prefixes.Cast<string>().First());
        Console.WriteLine("POST /api/debug/init  { script, host }");
        Console.WriteLine("POST /api/debug/step  { sessionId }");
        if (args.Any(a => a == "--once"))
        {
            Handle(listener.GetContext(), svc);
            listener.Stop();
            return 0;
        }
        while (true)
        {
            var ctx = listener.GetContext();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try { Handle(ctx, svc); }
                catch { try { ctx.Response.Abort(); } catch { } }
            });
        }
    }

    private static HttpListener Bind(int preferred)
    {
        Exception? last = null;
        foreach (int port in new[] { preferred, preferred + 1, 17890, 17891 })
        {
            try
            {
                var l = new HttpListener();
                l.Prefixes.Add("http://127.0.0.1:" + port + "/");
                l.Start();
                return l;
            }
            catch (Exception ex) { last = ex; }
        }
        throw new InvalidOperationException("Could not bind debugger host: " + last?.Message);
    }

    private static void Handle(HttpListenerContext ctx, RuleDebuggerService svc)
    {
        string path = (ctx.Request.Url?.AbsolutePath ?? "/").TrimEnd('/').ToLowerInvariant();
        if (path.Length == 0) path = "/";
        string method = ctx.Request.HttpMethod.ToUpperInvariant();
        if (method == "GET" && (path == "/" || path == "/index.html"))
        {
            Write(ctx, 200, "text/html; charset=utf-8", LoadHtml());
            return;
        }
        string body;
        using (var r = new StreamReader(ctx.Request.InputStream, Encoding.UTF8))
            body = r.ReadToEnd();
        Dictionary<string, JsonElement> map = Parse(body);

        try
        {
            object result = path switch
            {
                "/api/debug/init" => Pack(svc.InitializeSession(Str(map, "script"), HostOf(map))),
                "/api/debug/step" => Pack(svc.StepNext(Id(map))),
                "/api/debug/continue" => PackState(svc.Continue(Id(map))),
                "/api/debug/reset" => Reset(svc, map),
                "/api/debug/locals" => new { ok = true, locals = svc.GetLocals(Id(map)) },
                "/api/debug/state" => PackState(svc.GetSessionState(Id(map))),
                "/api/ping" => new { ok = true, engine = "Sara.RuleDebugger" },
                _ => throw new KeyNotFoundException(path)
            };
            WriteJson(ctx, path == "/" ? 200 : 200, result);
        }
        catch (KeyNotFoundException ex)
        {
            WriteJson(ctx, 404, new { ok = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            WriteJson(ctx, 400, new { ok = false, message = ex.Message });
        }
    }

    private static object Reset(RuleDebuggerService svc, Dictionary<string, JsonElement> map)
    {
        Guid id = Id(map);
        svc.Reset(id);
        return PackState(svc.GetSessionState(id));
    }

    private static Guid Id(Dictionary<string, JsonElement> map)
    {
        string s = Str(map, "sessionId");
        if (!Guid.TryParse(s, out var id))
            throw new ArgumentException("sessionId is required");
        return id;
    }

    private static string Str(Dictionary<string, JsonElement> map, string key)
    {
        if (map.TryGetValue(key, out var el) && el.ValueKind == JsonValueKind.String)
            return el.GetString() ?? "";
        return "";
    }

    private static object? HostOf(Dictionary<string, JsonElement> map)
    {
        if (!map.TryGetValue("host", out var el) || el.ValueKind != JsonValueKind.Object)
            return null;
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in el.EnumerateObject())
            dict[p.Name] = JsonToClr(p.Value);
        return dict;
    }

    private static object? JsonToClr(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.String => el.GetString(),
        JsonValueKind.Number when el.TryGetInt32(out int n) => n,
        JsonValueKind.Number when el.TryGetInt64(out long l) => l,
        JsonValueKind.Number => el.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => el.ToString()
    };

    private static Dictionary<string, JsonElement> Parse(string body)
    {
        var map = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(body)) return map;
        using var doc = JsonDocument.Parse(body);
        if (doc.RootElement.ValueKind != JsonValueKind.Object) return map;
        foreach (var p in doc.RootElement.EnumerateObject())
            map[p.Name] = p.Value.Clone();
        return map;
    }

    private static object Pack(DebugSession session) => PackState(session.Snapshot());

    private static object Pack(DebugStepResult step) => PackState(step.State);

    private static object PackState(DebugSessionState s)
    {
        return new
        {
            ok = true,
            sessionId = s.SessionId,
            currentLine = s.CurrentLine,
            currentColumn = s.CurrentColumn,
            executionState = s.ExecutionState.ToString(),
            currentStatement = s.CurrentStatement,
            locals = s.Locals.Select(v => new { v.Name, v.TypeName, v.ValueString, v.Scope }),
            callStack = s.CallStack,
            fault = s.Fault == null ? null : new
            {
                s.Fault.LineNumber,
                s.Fault.Column,
                s.Fault.FailingStatement,
                s.Fault.ExceptionType,
                s.Fault.ErrorMessage,
                variablesSnapshot = s.Fault.VariablesSnapshot.Select(v => new { v.Name, v.TypeName, v.ValueString, v.Scope })
            }
        };
    }

    private static void WriteJson(HttpListenerContext ctx, int status, object value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, JsonOpts));
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        ctx.Response.ContentLength64 = bytes.Length;
        ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
        ctx.Response.Close();
    }

    private static void Write(HttpListenerContext ctx, int status, string type, string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = type;
        ctx.Response.ContentLength64 = bytes.Length;
        ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
        ctx.Response.Close();
    }

    private static string LoadHtml()
    {
        var asm = Assembly.GetExecutingAssembly();
        string name = asm.GetManifestResourceNames().First(n => n.EndsWith("index.html", StringComparison.OrdinalIgnoreCase));
        using var s = asm.GetManifestResourceStream(name)!;
        using var r = new StreamReader(s, Encoding.UTF8);
        return r.ReadToEnd();
    }

    private static int DemoSelfTest()
    {
        var svc = new RuleDebuggerService();
        var session = svc.InitializeSession("int a = 10;\nint b = 0;\nint c = a / b;");
        var state = svc.Continue(session.Id);
        Console.WriteLine(JsonSerializer.Serialize(PackState(state), JsonOpts));
        return state.ExecutionState == ExecutionState.Faulted && state.Fault?.LineNumber == 3 ? 0 : 1;
    }
}
