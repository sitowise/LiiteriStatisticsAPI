using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Reflection;
using System.IO;

using System.ServiceModel; // WCF

namespace LiiteriStatisticsCore.Controllers
{
    [ServiceContract]
    public interface IVersionController
    {
        [OperationContract]
        IEnumerable<Models.ApplicationVersion> GetVersion();
    }

    public class VersionController : IVersionController
    {
        public IEnumerable<Models.ApplicationVersion> GetVersion()
        {
            var versions = new List<Models.ApplicationVersion>();

            Assembly asm = Assembly.GetCallingAssembly();

            FileVersionInfo fv = FileVersionInfo.GetVersionInfo(asm.Location);

            versions.Add(new Models.ApplicationVersion() {
                Application = asm.GetName().Name,
                Version = fv.FileVersion.ToString()
            });

            return versions;
        }
    }
}