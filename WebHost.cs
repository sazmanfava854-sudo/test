using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace RuleTrace
{
    /// <summary>Local-only HttpListener for the Persian web UI. Binds 127.0.0.1 so no URL ACL / admin is needed.</summary>
    internal sealed class WebHost : IDisposable
    {
        private readonly WebApp _app;
        private HttpListener _listener;
        private Thread _thread;
        private volatile bool _running;
        public Action StopRequested;

        public WebHost(WebApp app)
        {
            _app = app;
        }

        public string Url { get; private set; }
        public int Port { get; private set; }
        public bool IsListening { get { return _running && _listener != null && _listener.IsListening; } }

        public void Start(int preferredPort)
        {
            Exception last = null;
            int[] ports = preferredPort > 0
                ? new[] { preferredPort, preferredPort + 1, preferredPort + 2, 17880, 17881, 5055 }
                : new[] { 17880, 17881, 17882, 5055, 8088 };
            foreach (int port in ports)
            {
                try
                {
                    var l = new HttpListener();
                    string prefix = "http://127.0.0.1:" + port + "/";
                    l.Prefixes.Add(prefix);
                    l.Start();
                    _listener = l;
                    Port = port;
                    Url = prefix;
                    _running = true;
                    _thread = new Thread(Loop) { IsBackground = true, Name = "RuleTrace-http" };
                    _thread.Start();
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                    try { if (_listener != null) _listener.Close(); } catch { }
                    _listener = null;
                }
            }
            throw new InvalidOperationException("نتوانست پورت محلی را باز کند: " + (last != null ? last.Message : "unknown"));
        }

        public void Stop()
        {
            _running = false;
            try { if (_listener != null) _listener.Stop(); } catch { }
            try { if (_listener != null) _listener.Close(); } catch { }
        }

        public void Dispose() { Stop(); }

        private void Loop()
        {
            while (_running && _listener != null && _listener.IsListening)
            {
                HttpListenerContext ctx;
                try { ctx = _listener.GetContext(); }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
                catch { continue; }
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try { Handle(ctx); }
                    catch { try { ctx.Response.Abort(); } catch { } }
                });
            }
        }

        private void Handle(HttpListenerContext ctx)
        {
            string path = (ctx.Request.Url.AbsolutePath ?? "/").TrimEnd('/').ToLowerInvariant();
            if (path.Length == 0) path = "/";
            string method = (ctx.Request.HttpMethod ?? "GET").ToUpperInvariant();

            if (method == "GET" && (path == "/" || path == "/index.html"))
            {
                Write(ctx, 200, "text/html; charset=utf-8", LoadHtml());
                return;
            }
            if (method == "GET" && path == "/favicon.ico")
            {
                ctx.Response.StatusCode = 204;
                ctx.Response.Close();
                return;
            }

            Dictionary<string, object> body = null;
            if (method == "POST")
            {
                string raw;
                using (var r = new StreamReader(ctx.Request.InputStream, Encoding.UTF8))
                    raw = r.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(raw))
                    body = Json.ParseObject(raw);
            }
            if (body == null) body = new Dictionary<string, object>();

            Dictionary<string, object> result;
            bool shutdown = false;
            try
            {
                switch (path)
                {
                    case "/api/ping": result = _app.Ping(); break;
                    case "/api/bootstrap": result = _app.Bootstrap(); break;
                    case "/api/settings": result = _app.SaveSettings(body); break;
                    case "/api/detect-dll": result = _app.DetectDll(); break;
                    case "/api/test-db": result = _app.TestDb(); break;
                    case "/api/lookup": result = _app.Lookup(body); break;
                    case "/api/combine": result = _app.Combine(body); break;
                    case "/api/analyze-members": result = _app.AnalyzeMembers(body); break;
                    case "/api/analyze-chidman": result = _app.AnalyzeChidman(body); break;
                    case "/api/inspect": result = _app.Inspect(body); break;
                    case "/api/run": result = _app.RunFormula(body); break;
                    case "/api/shutdown":
                        result = new Dictionary<string, object> { { "ok", true }, { "message", "در حال خروج..." } };
                        shutdown = true;
                        break;
                    default:
                        result = new Dictionary<string, object> { { "ok", false }, { "message", "not found: " + path } };
                        WriteJson(ctx, 404, result);
                        return;
                }
                WriteJson(ctx, 200, result);
            }
            catch (Exception ex)
            {
                WriteJson(ctx, 500, new Dictionary<string, object>
                {
                    { "ok", false },
                    { "message", ex.Message },
                    { "log", new[] { "FATAL: " + ex.Message } },
                });
            }

            if (shutdown)
            {
                ThreadPool.QueueUserWorkItem(__ =>
                {
                    Thread.Sleep(200);
                    Action a = StopRequested;
                    if (a != null) a();
                    else Stop();
                });
            }
        }

        private static void WriteJson(HttpListenerContext ctx, int status, Dictionary<string, object> map)
        {
            Write(ctx, status, "application/json; charset=utf-8", Json.Encode(map));
        }

        private static void Write(HttpListenerContext ctx, int status, string contentType, string body)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(body ?? string.Empty);
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = contentType;
            ctx.Response.Headers["Cache-Control"] = "no-store";
            ctx.Response.ContentLength64 = bytes.Length;
            ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
            ctx.Response.Close();
        }

        internal static string LoadHtml()
        {
            string[] names = { "RuleTrace.WebUi.html", "WebUi.html" };
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (string name in names)
            {
                using (Stream s = asm.GetManifestResourceStream(name))
                {
                    if (s == null) continue;
                    using (var r = new StreamReader(s, Encoding.UTF8))
                        return r.ReadToEnd();
                }
            }
            foreach (string name in asm.GetManifestResourceNames())
            {
                if (name.EndsWith("WebUi.html", StringComparison.OrdinalIgnoreCase))
                {
                    using (Stream s = asm.GetManifestResourceStream(name))
                    using (var r = new StreamReader(s, Encoding.UTF8))
                        return r.ReadToEnd();
                }
            }
            string dir = Path.GetDirectoryName(asm.Location) ?? ".";
            string[] files =
            {
                Path.Combine(dir, "WebUi.html"),
                Path.Combine(dir, "..", "WebUi.html"),
                Path.Combine(Environment.CurrentDirectory, "WebUi.html"),
            };
            foreach (string f in files)
                if (File.Exists(f)) return File.ReadAllText(f, Encoding.UTF8);
            throw new FileNotFoundException("WebUi.html not found (embedded or next to exe)");
        }
    }
}
