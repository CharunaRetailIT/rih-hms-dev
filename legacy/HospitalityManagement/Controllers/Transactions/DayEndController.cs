
using RIT.HMS.BLL.TransactionData;
using System;
using System.Collections.Generic;
//using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Transactions
{
    [SessionTimeout]
    public class DayEndController : Controller
    {

        private readonly BLL_DayEnd _blldayend = new BLL_DayEnd();
        public DayEndController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _blldayend = new BLL_DayEnd(cn);
        }

        [Authorize(Roles = "GrnCreatee")]
        public ActionResult DayEnd()
        {
            return View("~/Views/Transactions/DayEnd/DayEnd.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "GrnCreatee")]
        public JsonResult ProcessDayEnd(DateTime datefrom,DateTime dateto)
        {
         //   DayEndService dayEndService = new DayEndService();
            string data = "";
             if (_blldayend.DayEnd(datefrom, dateto) == true)
            {
                data = "1";
            }
            else
            {
                data = "2";
            }


            
            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

    }
}