//using HospitalityManagement.Models.ViewModels;
//using HospitalityManagement.Service.Promotion;
using RIT.HMS.BLL.Promotions;
using RIT.HMS.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Promotions
{
    [SessionTimeout]
    [Authorize(Roles = "PrdCreatee")]
    public class DiscountsPromotionsController : Controller
    {
        BLL_ItemToItemPromotion _itemToItemPromotionService;
        public DiscountsPromotionsController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _itemToItemPromotionService = new BLL_ItemToItemPromotion(cn);
        }
        public ActionResult Index()
        {
            return View();

        }

        public ActionResult CreateDiscountsPromotion()
        {
       
            return View("~/Views/Promotions/DiscountsPromotions/CreateDiscountPromotions.cshtml");
        }

        public JsonResult SubmitPromotions(List<ProductStockMasterViewModel> promotions)
        {

            var res = _itemToItemPromotionService.SubmitPromotion(promotions, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(res);
        }

    }
}