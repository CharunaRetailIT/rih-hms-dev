using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace HospitalityManagement.App_Start
{
    public  static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services  

            // Web API routes - To enable attribute base routing.  


            config.MapHttpAttributeRoutes();


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}