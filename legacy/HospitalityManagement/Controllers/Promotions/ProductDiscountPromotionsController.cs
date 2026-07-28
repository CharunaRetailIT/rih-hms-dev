using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.BLL.Promotions;
using Newtonsoft.Json;
using RIT.HMS.Domain.Promotions;
using RIT.HMS.Domain.ViewModels.Promotions;
using RIT.HMS.Domain.Promotions.Enums;

namespace HospitalityManagement.Controllers.Promotions
{
    [SessionTimeout]
    [Authorize(Roles = "PrdCreatee")]
    public class ProductDiscountPromotionsController : Controller
    {
        BLL_ProductDiscountsPromotions _bllproductdiscounts;
        BLL_ItemToItemPromotion _itemToItemPromotionService;

        public ProductDiscountPromotionsController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllproductdiscounts = new BLL_ProductDiscountsPromotions(cn);
           _itemToItemPromotionService = new BLL_ItemToItemPromotion(cn);
        }
        public ActionResult ProductDiscountPromotions()
        {

            return View("~/Views/Promotions/ProductDiscountPromotions/ProductDiscountPromotions.cshtml",
                _bllproductdiscounts.ProductDiscounts(Convert.ToInt32(Session["loggedusercompanyId"].ToString())));
        }


        public JsonResult SubmitProductDiscouts(int deptid, int catid, int subcatid,
                                                int productid, long servingunitid, decimal FromQty, decimal ToQty, decimal Rate,
                                                long Points, decimal DiscountAmt, decimal DiscountPrc,int promomasterid)
        {
                       
            return new JsonResult
            {
                Data =_bllproductdiscounts.SubmitProductDiscounts(deptid, catid, subcatid, productid, servingunitid,
                FromQty, ToQty, Rate, Points, DiscountAmt, DiscountPrc, promomasterid, Convert.ToInt32(Session["loggedusercompanyId"].ToString())),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public JsonResult RemoveProductDiscount(long rowid)
        {

            var responsemsg = _bllproductdiscounts.RemoveProductDiscount(rowid);
            return Json(JsonConvert.SerializeObject(responsemsg, Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
            JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetPromotionMasters()
        {
            //int buyxgety = (int)enumPromotionTypes.ButXGetYFree;
            //int buyxgetynfree = (int)enumPromotionTypes.BuyXGetYnFree;
            //int buyxngetyfree = (int)enumPromotionTypes.ButXnGetYFree;
            //int buyxngetynfree = (int)enumPromotionTypes.ButXnGetYnFree;
            int promotype = (int)enumPromotionTypes.DiptCatSubCat;
            
            var promotionmasters = _itemToItemPromotionService.GetPromotionMasters(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(p => p.PromotionTypeID == promotype);

            //var x = promotionmasters.Select(Y => Y.LocationID).FirstOrDefault();
            Session["selectedLocation"] = promotionmasters.Select(Y => Y.LocationID).FirstOrDefault();
            var loc = Session["selectedLocation"];

            return Json(JsonConvert.SerializeObject(promotionmasters, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }
    }
}