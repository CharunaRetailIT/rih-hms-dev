using HospitalityManagement.App_Start;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;



namespace HospitalityManagement
{
    public class MvcApplication : System.Web.HttpApplication
    {
        
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        


        public static string MyProperty { get; set; }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                //FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authCookie.Value != null && !authTicket.Expired)
                {
                    var roles = authTicket.UserData.Split(',');
                    HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(new
                                                                             FormsIdentity(authTicket), roles);
                }
            }
        }

        protected void Session_Timout()
        {
            Session.Timeout = 60;
        }
        protected void Application_BeginRequest()
        {

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now.AddHours(-1));
            Response.Cache.SetNoStore();

        }

        protected void Application_Error()
        {
            Exception ex = Server.GetLastError();

            if (ex != null)
            {
                Server.ClearError();
                Response.Redirect("~/Account/Login");
                
            }

        }
    }
}
