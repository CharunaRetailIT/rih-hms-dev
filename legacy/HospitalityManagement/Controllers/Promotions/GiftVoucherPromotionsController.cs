using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Promotions
{
    [SessionTimeout]
    [Authorize(Roles = "PrdCreatee")]
    public class GiftVoucherPromotionsController : Controller
    {
        // GET: GiftVoucherPromotions
        public ActionResult CreateGiftVoucherPromotions()
        {

            return View("~/Views/Promotions/GiftVoucherPromotions/CreateGiftVoucherPromotions.cshtml");
        }
    }
}