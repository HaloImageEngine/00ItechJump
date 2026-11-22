using System;
using System.Web;

namespace EmpireOneRestAPIITJ.Security
{
    /// <summary>Adds standard HTTP security headers to API responses.</summary>
    public sealed class SecurityHeadersModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        public void Dispose() { }

        private static void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var response = HttpContext.Current?.Response;
            if (response == null) return;

            response.Headers.Remove("Server");
            response.Headers["X-Content-Type-Options"] = "nosniff";
            response.Headers["X-Frame-Options"] = "SAMEORIGIN";
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            response.Headers["X-XSS-Protection"] = "1; mode=block";
            response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

            var rawPath = HttpContext.Current?.Request?.Path ?? string.Empty;
            var isSplash =
                rawPath.Equals("/", StringComparison.OrdinalIgnoreCase)
                || rawPath.Equals("/index.html", StringComparison.OrdinalIgnoreCase);

            if (isSplash)
            {
                // Splash uses inline &lt;style&gt; / &lt;script&gt; (same as Lotto index.html).
                response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; frame-ancestors 'self'; base-uri 'self'";
                return;
            }

            var path = HttpContext.Current?.Request?.AppRelativeCurrentExecutionFilePath ?? string.Empty;
            if (path.IndexOf("swagger", StringComparison.OrdinalIgnoreCase) < 0)
            {
                response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; frame-ancestors 'self'; base-uri 'self'";
            }
        }
    }
}