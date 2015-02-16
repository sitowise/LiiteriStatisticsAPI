using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Core = LiiteriStatisticsCore;

namespace LiiteriStatisticsService
{
    /* Normally we could refer to the Core method in our .svc, but in this
     * case it would return the version number from a wrong assembly */
    public class VersionService : Core.Controllers.IVersionController
    {
        public IEnumerable<Core.Models.ApplicationVersion> GetVersion()
        {
            Core.Controllers.IVersionController controller =
                new Core.Controllers.VersionController();

            return controller.GetVersion();
        }
    }
}