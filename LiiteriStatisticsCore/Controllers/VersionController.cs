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

            string assemblyNames = null; // could be a parameter

            Assembly[] assemblies;

            if (assemblyNames != null && assemblyNames.Length > 0) {
                assemblies =
                    AppDomain.CurrentDomain.GetAssemblies()
                    .Where(b => assemblyNames.Contains(b.GetName().Name))
                    .ToArray();
            } else {
                assemblies =
                    AppDomain.CurrentDomain.GetAssemblies()
                    .Where(b => b.GetName().Name.StartsWith("Liiteri"))
                    .ToArray();
            }

            foreach (Assembly asm in assemblies) {
                FileVersionInfo fv = FileVersionInfo.GetVersionInfo(
                    asm.Location);
                versions.Add(new Models.ApplicationVersion() {
                    Application = asm.GetName().Name,
                    Version = fv.FileVersion.ToString()
                });
            }

            return versions;
        }
    }
}