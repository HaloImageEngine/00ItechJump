using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EmpireOneRestAPIITJ.Security
{
    /// <summary>Requires Authorization: Bearer {sessionToken} from login.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class RequireItjSessionAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var auth = actionContext.Request.Headers.Authorization;
            if (auth == null ||
                !string.Equals(auth.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(auth.Parameter))
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    new { ok = false, error = "Missing or invalid Authorization header." });
                return;
            }

            if (!ItjSessionToken.TryValidate(auth.Parameter, out var userId, out _))
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    new { ok = false, error = "Session expired or invalid." });
                return;
            }

            var identity = new GenericIdentity(userId.ToString());
            actionContext.RequestContext.Principal =
                new GenericPrincipal(identity, roles: Array.Empty<string>());
        }
    }
}