using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiiteriStatisticsService.App_Code
{
    public class Init
    {
        public static void AppInitialize()
        {
            /* enabling log4net with AsemblyInfo.cs does not work for some 
             * reason, enabling it here instead */
            log4net.Config.XmlConfigurator.Configure(); 
        }
    }
}