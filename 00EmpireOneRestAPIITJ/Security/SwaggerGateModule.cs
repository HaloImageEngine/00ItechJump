using System;
using System.Configuration;
using System.Text;
using System.Web;

namespace EmpireOneRestAPIITJ.Security
{
    /// <summary>
    /// Hides Swagger when disabled; optional HTTP Basic Auth when enabled.
    /// </summary>
    public sealed class SwaggerGateModule : IHttpModule
    {
        private const string Realm = "iTechJump Swagger";

        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
        }

        public void Dispose() { }

        private static void OnBeginRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var request = app.Context.Request;
            var response = app.Context.Response;

            if (!IsSwaggerPath(request.Path))
                return;

            if (!IsSwaggerEnabled())
            {
                DenyNotFound(response);
                return;
            }

            if (AllowLocalWithoutAuth(request) || !RequireBasicAuth())
                return;

            var expectedUser = ConfigurationManager.AppSettings["SwaggerUser"] ?? string.Empty;
            var expectedPassword = ConfigurationManager.AppSettings["SwaggerPassword"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(expectedUser) || string.IsNullOrWhiteSpace(expectedPassword))
            {
                DenyNotFound(response);
                return;
            }

            if (!TryGetBasicCredentials(request, out var user, out var password)
                || !SecureEquals(user, expectedUser)
                || !SecureEquals(password, expectedPassword))
            {
                DenyUnauthorized(response);
            }
        }

        private static bool IsSwaggerPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return path.IndexOf("/swagger", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsSwaggerEnabled()
        {
            var enabled = ConfigurationManager.AppSettings["EnableSwagger"];
            return !string.Equals(enabled, "false", StringComparison.OrdinalIgnoreCase);
        }

        private static bool RequireBasicAuth()
        {
            var require = ConfigurationManager.AppSettings["SwaggerRequireBasicAuth"];
            return !string.Equals(require, "false", StringComparison.OrdinalIgnoreCase);
        }

        private static bool AllowLocalWithoutAuth(HttpRequest request)
        {
            if (!string.Equals(
                    ConfigurationManager.AppSettings["SwaggerAllowLocalWithoutAuth"],
                    "true",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return request.IsLocal;
        }

        private static bool TryGetBasicCredentials(HttpRequest request, out string user, out string password)
        {
            user = password = null;
            var header = request.Headers["Authorization"];
            if (string.IsNullOrEmpty(header)
                || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                var encoded = header.Substring("Basic ".Length).Trim();
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var colon = decoded.IndexOf(':');
                if (colon < 0)
                    return false;

                user = decoded.Substring(0, colon);
                password = decoded.Substring(colon + 1);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool SecureEquals(string a, string b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null)
                return false;
            return string.Equals(a, b, StringComparison.Ordinal);
        }

        private static void DenyNotFound(HttpResponse response)
        {
            response.Clear();
            response.StatusCode = 404;
            response.SuppressContent = true;
            response.TrySkipIisCustomErrors = true;
            response.End();
        }

        private static void DenyUnauthorized(HttpResponse response)
        {
            response.Clear();
            response.StatusCode = 401;
            response.AddHeader("WWW-Authenticate", "Basic realm=\"" + Realm + "\"");
            response.SuppressContent = true;
            response.TrySkipIisCustomErrors = true;
            response.End();
        }
    }
}