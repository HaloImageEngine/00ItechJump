using System;
using System.Configuration;
using System.Web;
using System.Web.Http;

namespace EmpireOneRestAPIITJ
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        // Optional: keep your redirect to Swagger UI
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var requireHttps = string.Equals(
                ConfigurationManager.AppSettings["RequireHttps"],
                "true",
                StringComparison.OrdinalIgnoreCase);

            if (requireHttps && !Request.IsLocal && !Request.IsSecureConnection)
            {
                // HTTP hop from itechjump.com IIS reverse proxy (browser is already HTTPS).
                if (!IsBehindHttpsProxy(Request) && !IsLoopbackCaller(Request))
                {
                    var https = new UriBuilder(Request.Url) { Scheme = "https", Port = 443 }.Uri;
                    Response.Redirect(https.ToString(), endResponse: true);
                    return;
                }
            }

            // Show splash at site root (https://api.itechjump.com/) without /index.html in the URL.
            var path = Request.Url.AbsolutePath ?? string.Empty;
            if (path == "/" || path.Equals("/index.html", StringComparison.OrdinalIgnoreCase))
            {
                if (path == "/")
                    Context.RewritePath("~/index.html");
                return;
            }
        }

        private static bool IsBehindHttpsProxy(HttpRequest request)
        {
            var proto = request.Headers["X-Forwarded-Proto"];
            if (!string.IsNullOrEmpty(proto)
                && proto.IndexOf("https", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            var frontEndHttps = request.Headers["Front-End-Https"];
            return string.Equals(frontEndHttps, "on", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLoopbackCaller(HttpRequest request)
        {
            var ip = request.UserHostAddress ?? string.Empty;
            return ip == "127.0.0.1"
                || ip == "::1"
                || ip.StartsWith("127.", StringComparison.Ordinal);
        }
    }
}
