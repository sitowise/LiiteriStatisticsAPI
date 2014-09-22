using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using LiiteriStatisticsCore.Util;

namespace LiiteriDataAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // disable xml
            //GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

            GlobalConfiguration.Configuration.BindParameter(
                typeof(DateRange), new Binders.DateRangeModelBinder());

            GlobalConfiguration.Configuration.BindParameter(
                typeof(int[]), new Binders.IntegerArrayModelBinder());

            config.Formatters.Add(new Formatters.TextPlainFormatter());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}