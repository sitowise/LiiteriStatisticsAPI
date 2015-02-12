using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace LiiteriStatisticsDirectAPI.Controllers
{
    public class VersionController : ApiController
    {
        [Route("version/")]
        [Route("v1/version/")]
        [HttpGet]
        public HttpResponseMessage GetVersion()
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            // AssemblyVersion
            //return asm.GetName().Version.ToString();

            // AssemblyFileVersion
            FileVersionInfo fv = FileVersionInfo.GetVersionInfo(asm.Location);
            return Request.CreateResponse(
                HttpStatusCode.OK,
                fv.FileVersion.ToString(),
                new Formatters.TextPlainFormatter());
        }
    }
}