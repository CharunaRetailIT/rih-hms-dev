using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;

namespace HospitalityManagement
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;
            if (HttpContext.Current.Session["loggeduser"] == null)
            {
               // filterContext.Result = new RedirectResult("~/Handler/Error440");
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}