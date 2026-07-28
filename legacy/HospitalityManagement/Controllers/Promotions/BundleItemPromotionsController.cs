using Newtonsoft.Json;
using RIT.HMS.BLL.Promotions;
using RIT.HMS.Domain.Promotions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Promotions
{
    [SessionTimeout]
    [Authorize(Roles = "PrdCreatee")]
    public class BundleItemPromotionsController : Controller
    {
        BLL_BundleItemPromotion _bllbundleitempromotion;
        BLL_ItemToItemPromotion _itemToItemPromotionService;
        public BundleItemPromotionsController()
        {
             string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllbundleitempromotion = new BLL_BundleItemPromotion(cn);
            _itemToItemPromotionService = new BLL_ItemToItemPromotion(cn);
        }

        public ActionResult BundleItemPromotion()
        {
            InvBundleItemPrice _invbundleitemprice = new InvBundleItemPrice();
            _invbundleitemprice.IsExists = false;
            return View("~/Views/Promotions/BundleItemPromotions/CreateBundleItemPromotion.cshtml", _invbundleitemprice);
        }

        public ActionResult CombopackBundleItemPromotion()
        {
            InvBundleItemPrice _invbundleitemprice = new InvBundleItemPrice();
            _invbundleitemprice.IsExists = false;
            return View("~/Views/Promotions/BundleItemPromotions/CreateComboPackBundleItemPromotion.cshtml", _invbundleitemprice);
        }
        public ActionResult SubmitBundleItemPromotion(List<InvBundleItemPrice> invbundleitems)
        {
            invbundleitems.ForEach(
                p=> {
                p.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                p.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                p.CreatedDate = DateTime.Now;
                p.CreatedUser = Session["loggeduser"].ToString();
            });
            var res = _bllbundleitempromotion.SubmitPromotion(invbundleitems);
            return Json(res);
        }

        public ActionResult SubmitCombopackBundleItemPromotion(List<InvComboPackBundleItemPrice> invbundleitems)
        {
            invbundleitems.ForEach(
                p =>
                {
                    p.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                    p.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                    p.CreatedDate = DateTime.Now;
                    p.CreatedUser = Session["loggeduser"].ToString();
                });
            var res = _bllbundleitempromotion.SubmitComboPackPromotion(invbundleitems);
            return Json(res);
        }

        [HttpGet]
        public ActionResult ViewPromotionItems(int promotionmasterid)
        {
            @ViewBag.PromotionMasterId = promotionmasterid;       
            @ViewBag.PromotionName = _itemToItemPromotionService.MyProperty(
                promotionmasterid,
                Convert.ToInt32(Session["loggedusercompanyId"].ToString())
                ).PromotionName;
            return View("~/Views/Promotions/BundleItemPromotions/RemoveBundlePromotions.cshtml");
        }

        [HttpGet]
        public JsonResult GetPromotions(int promotionmasterid)
        {

            var promotionmasters = _bllbundleitempromotion.GetBundlePromotions(promotionmasterid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var orderedpromotions = promotionmasters.OrderBy(v => v.GroupId);
            return Json(JsonConvert.SerializeObject(orderedpromotions, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RemoveBundleItemGroup(InvBundleItemPrice promoitem)
        {
            promoitem.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var res = _bllbundleitempromotion.RemoveBundleItemGroup(promoitem);
            return Json(res);
        }
    }
}