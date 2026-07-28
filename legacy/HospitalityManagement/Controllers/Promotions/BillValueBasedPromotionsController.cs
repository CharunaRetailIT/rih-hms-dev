//using HospitalityManagement.Models.ViewModels;
//using HospitalityManagement.Service.Promotion;
using Newtonsoft.Json;
using RIT.HMS.BLL.Promotions;
using RIT.HMS.Domain.Promotions;
using RIT.HMS.Domain.Promotions.Enums;
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
    public class BillValueBasedPromotionsController : Controller
    {
        // GET: BillValueBasedGetYProductPromotions

        BLL_BillValueBasedGetYProductPromotion _billValuePromotionService;
        BLL_ItemToItemPromotion _itemToItemPromotionService;
        BLL_Promotion _promotionmanster;
        public BillValueBasedPromotionsController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _billValuePromotionService = new BLL_BillValueBasedGetYProductPromotion(cn);
            _itemToItemPromotionService = new BLL_ItemToItemPromotion(cn);
            _promotionmanster = new BLL_Promotion(cn);
        }

        [HttpGet]
        public ActionResult CreateBillValueBasedGetYProductPromo()
        {
            return View("~/Views/Promotions/BillValueBasedPromotions/CreateBillValueBasedGetYProductPromo.cshtml");
        }

        public JsonResult SubmitPromotions(List<ProductStockMasterViewModel> promotions)
        {

            var res = _billValuePromotionService.SubmitPromotion(promotions, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(res);
        }


        public ActionResult GetPromotionItems()
        {
            var products = _billValuePromotionService.GetProducts(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            return Json(JsonConvert.SerializeObject(products, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetServingUnits(long id)
        {
            var servingunits = _billValuePromotionService.GetServingUnits(id, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            return Json(JsonConvert.SerializeObject(servingunits, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult CreateBillValueDiscounts()
        {
            InvBillValueDiscount billdiscount = new InvBillValueDiscount();
            billdiscount.IsExists = false;
            return View("~/Views/Promotions/BillValueBasedPromotions/CreateBillValueDiscounts.cshtml", billdiscount);
        }

        //  [HttpPost]
        //public JsonResult SubmitBillValueDiscounts(int pmid, string billtype, decimal fromvalue, decimal tovalue,
        //                                            string discounttype, decimal discountvalue)

        public ActionResult EditBillValueDiscount()
        {
            var existing = _billValuePromotionService.GetAllBillValueDiscounts(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            existing.ForEach(p=> 
            {
                p.PromotionName = _promotionmanster.GetActivePromosById(Convert.ToInt32(Session["loggedusercompanyId"].ToString()),p.PromotionMasterId).PromotionName;
            }
            );
            return View("~/Views/Promotions/BillValueBasedPromotions/ViewAllBillValueDiscountsPromotions.cshtml", existing);
        }

        [HttpGet]
        public ActionResult EditBillValueDiscountById(int billvaldiscountsid)
        {
            var existing = _billValuePromotionService.GetBillValueDiscountsById(Convert.ToInt32(Session["loggedusercompanyId"].ToString()), billvaldiscountsid);
            existing.IsExists = true;
            return View("~/Views/Promotions/BillValueBasedPromotions/CreateBillValueDiscounts.cshtml", existing);
        }
        public JsonResult SubmitBillValueDiscounts(InvBillValueDiscount invbillvaluediscount)
        {

            if (invbillvaluediscount.IsExists == false)
            {
                invbillvaluediscount.CreatedDate = DateTime.Now;
                invbillvaluediscount.CreatedUser = Session["loggeduser"].ToString();            
            }
            else
            {
                invbillvaluediscount.ModifiedDate = DateTime.Now;
                invbillvaluediscount.ModifiedUser = Session["loggeduser"].ToString();
            }
            invbillvaluediscount.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            invbillvaluediscount.LocationId = Convert.ToInt32(Session["loggeduserlocid"].ToString());
            return Json(_billValuePromotionService.SubmitBillValueDiscountPromotion(invbillvaluediscount));
        }

        [HttpGet]
        public JsonResult GetProductDetails(long id, int servingunitid)
        {

            var product = _billValuePromotionService.GetProductDetailsById(id, servingunitid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return new JsonResult
            {
                Data = product,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }
        public ActionResult CreateBillValueBasedDiscountPromo()
        {
            return View("~/Views/Promotions/BillValueBasedPromotions/CreateBillValueBasedDiscountPromo.cshtml");
        }

       

    }
}