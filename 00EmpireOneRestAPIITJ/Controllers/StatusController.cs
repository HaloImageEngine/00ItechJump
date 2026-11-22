using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;

namespace EmpireOneRestAPIITJ.Controllers
{
    [RoutePrefix("api/status")]
    public class StatusController : ApiController
    {
        [HttpGet, Route("")]
        public IHttpActionResult GetStatus()
        {
            var appVersion = ConfigurationManager.AppSettings["AppVersion"] ?? "unknown";
            var versionId = ConfigurationManager.AppSettings["VersionId"] ?? "unknown";
            return Ok(new { status = "ok", appVersion, versionId, timeUtc = DateTime.UtcNow });
        }

        /// <summary>Version id for splash/static pages (from Web.config VersionId).</summary>
        [HttpGet, Route("version")]
        public IHttpActionResult GetVersion()
        {
            var versionId = ConfigurationManager.AppSettings["VersionId"] ?? "unknown";
            var appVersion = ConfigurationManager.AppSettings["AppVersion"] ?? "unknown";
            return Ok(new { versionId, appVersion });
        }

        [HttpGet, Route("ping")]
        public HttpResponseMessage Ping()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "pong");
        }


    }


}
