using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace LiiteriStatisticsDirectAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy =
                IncludeErrorDetailPolicy.Always;
        }

        void Application_BeginRequest(Object source, EventArgs e)
        {
            HttpApplication app = (HttpApplication) source;
            HttpContext context = app.Context;

            // Attempt to peform first request initialization
            FirstRequestInitialization.Initialize(context);
        }
    }

    class FirstRequestInitialization
    {
        private static bool s_InitializedAlready = false;
        private static Object s_lock = new Object();

        // Initialize only on the first request
        public static void Initialize(HttpContext context)
        {
            if (s_InitializedAlready) {
                return;
            }
            lock (s_lock) {
                if (s_InitializedAlready) {
                    return;
                }
                // Perform first-request initialization here ...

                string userHost = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(userHost) ||
                        string.Compare(userHost, "unknown", true) == 0) {
                    userHost = context.Request.UserHostAddress;
                }
                if (string.Compare(userHost, context.Request.UserHostAddress) != 0) {
                    userHost += " ( " + context.Request.UserHostAddress + ")";
                }
                log4net.GlobalContext.Properties["userHost"] = userHost;

                s_InitializedAlready = true;
            }
        }
    }
}
