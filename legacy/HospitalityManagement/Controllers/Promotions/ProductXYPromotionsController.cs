//using HospitalityManagement.Models.Promotions.Enums;
//using HospitalityManagement.Models.ViewModels;
//using HospitalityManagement.Models.ViewModels;
//using HospitalityManagement.Service.Promotion;
using Newtonsoft.Json;
using RIT.HMS.BLL.Promotions;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Promotions;
using RIT.HMS.Domain.Promotions.Enums;
using RIT.HMS.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Promotions
{
    [SessionTimeout]
    [Authorize(Roles = "PrdCreatee")]

    public class ProductXYPromotionsController : Controller
    {
        BLL_ItemToItemPromotion _itemToItemPromotionService;
        public ProductXYPromotionsController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _itemToItemPromotionService = new BLL_ItemToItemPromotion(cn);
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateProductXYPromotions()
        {
            return View("~/Views/Promotions/ProductXYPromotions/CreateXYPromotions.cshtml");
        }

        public JsonResult SubmitPromotions(List<ProductStockMasterViewModel> promotions)
        {

            var res = _itemToItemPromotionService.SubmitPromotion(promotions, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(res);
        }

        // Selectors
        public ActionResult GetPromotionItems()
        {
            var products = _itemToItemPromotionService.GetProducts(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            return Json(JsonConvert.SerializeObject(products, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetServingUnits(long id)
        {

            var servingunits  = new List<ProductServingUnit>();

            try
            {
                int getLocation = 0;
                var loguserloca = Session["loggeduserlocId"];//get the log user location
                getLocation = Convert.ToInt32(loguserloca);
                
                if (getLocation!=null)
                {
                    servingunits = _itemToItemPromotionService.GetServingUnits(id).Where(o => o.LocationId == getLocation).ToList();
                }
                else
                {
                    getLocation = 0;
                    servingunits = _itemToItemPromotionService.GetServingUnits(id).Where(o => o.LocationId == 1).ToList();
                }
               
            }
            catch (Exception)
            {

            }
          
            return Json(JsonConvert.SerializeObject(servingunits, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetProductDetails(long id, int servingunitid)
        {

            var product = _itemToItemPromotionService.GetProductDetailsById(id, servingunitid);
            return new JsonResult
            {
                Data = product,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

        public JsonResult GetComboPackProductDetails(long id, int servingunitid)
        {
            @ViewBag.Message = 1;
           
            var product = _itemToItemPromotionService.GetComboPackProductDetailsById(id, servingunitid);
            return new JsonResult
            {
                Data = product,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }
        public JsonResult PromotionValidation(long id, int servingunitid, int PromotionID)
        {
            var product = _itemToItemPromotionService.GetComboProductDetailsByID(id);
            var product1 = _itemToItemPromotionService.GetComboProductDetailsByID1(id);
            var product2 = _itemToItemPromotionService.GetComboProductDetailsByID2(id);

            ComboPackBundleItemPromotion obj = new ComboPackBundleItemPromotion();
            obj.ValidationErrorMsg = "X";
            var ComboPromotionStartDate = _itemToItemPromotionService.GetPromotionMasterById(PromotionID).StartDate;
            var ComboPromotionEndDate = _itemToItemPromotionService.GetPromotionMasterById(PromotionID).EndDate;

            #region product
            if (product != null)
            {
                DateTime PStartdte = Convert.ToDateTime(product.StartDate.ToString("yyyy-MM-dd"));
                DateTime PEndDate = Convert.ToDateTime(product.EndDate.ToString("yyyy-MM-dd"));

                DateTime CStartdte = Convert.ToDateTime(ComboPromotionStartDate.Value.ToString("yyyy-MM-dd"));
                DateTime CEndDate = Convert.ToDateTime(ComboPromotionEndDate.Value.ToString("yyyy-MM-dd"));



                if (PStartdte >= CStartdte && PEndDate <= CEndDate)
                {
                    obj.ValidationErrorMsg = "Duplicate Promotion Found.";
                    return new JsonResult
                    {
                        Data = obj,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else if (PStartdte <= CStartdte && PEndDate >= CEndDate)
                {
                    obj.ValidationErrorMsg = "Duplicate Promotion Found.";
                    return new JsonResult
                    {
                        Data = obj,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else if (PStartdte <= CEndDate && PEndDate <= CEndDate && CStartdte >= PStartdte && CStartdte <= PEndDate)
                {
                    obj.ValidationErrorMsg = "Duplicate Promotion Found.";
                    return new JsonResult
                    {
                        Data = obj,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                { 
                    return new JsonResult
                    {
                        Data = obj,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

            }
            #endregion

            #region product1
            if (product1 != null)
            {
                DateTime PStartdte = Convert.ToDateTime(product1.StartDate.ToString("yyyy-MM-dd"));
                DateTime PEndDate = Convert.ToDateTime(product1.EndDate.ToString("yyyy-MM-dd"));

                DateTime CStartdte = Convert.ToDateTime(ComboPromotionStartDate.Value.ToString("yyyy-MM-dd"));
                DateTime CEndDate = Convert.ToDateTime(ComboPromotionEndDate.Value.ToString("yyyy-MM-dd"));

                if (PStartdte >= CStartdte && PEndDate <= CEndDate)
                {
                    obj.ValidationErrorMsg = "Duplicate Promotion Found.";
                    return new JsonResult
                    {
                        Data = obj,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else if (PStartdte <= CStartdte && PEndDate >= CEndDate)
                {
                    obj.ValidationErrorMsg = "Duplicate Promotion Found.";
                    return new JsonResult
                    {
                        Data = obj,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else if (PStartdte <= CEndDate && PEndDate <= CEndDate && CStartdte >= PStartdte && CStartdte <= PEndDate)
                {
                    obj.ValidationErrorMsg = "Duplicate Promotion Found.";
                    return new JsonResult
                    {
                        Data = obj,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    return new JsonResult
                    {
                        Data = obj,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
            }
            #endregion

            #region product2
            if (product2 != null)
            {
                DateTime PStartdte = Convert.ToDateTime(product2.StartDate.ToString("yyyy-MM-dd"));
                DateTime PEndDate = Convert.ToDateTime(product2.EndDate.ToString("yyyy-MM-dd"));

                DateTime CStartdte = Convert.ToDateTime(ComboPromotionStartDate.Value.ToString("yyyy-MM-dd"));
                DateTime CEndDate = Convert.ToDateTime(ComboPromotionEndDate.Value.ToString("yyyy-MM-dd"));

                if (PStartdte >= CStartdte && PEndDate <= CEndDate)
                {
                    obj.ValidationErrorMsg = "Duplicate Promotion Found.";
                    return new JsonResult
                    {
                        Data = obj,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else if (PStartdte <= CStartdte && PEndDate >= CEndDate)
                {
                    obj.ValidationErrorMsg = "Duplicate Promotion Found.";
                    return new JsonResult
                    {
                        Data = obj,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else if (PStartdte <= CEndDate && PEndDate <= CEndDate && CStartdte >= PStartdte && CStartdte <= PEndDate)
                {
                    obj.ValidationErrorMsg = "Duplicate Promotion Found.";
                    return new JsonResult
                    {
                        Data = obj,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    return new JsonResult
                    {
                        Data = obj,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
            }
            #endregion


            return new JsonResult
            {
                Data = obj,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public JsonResult GetComboProductDetails(long id, int servingunitid,int PromotionID)
        {
            ComboPackBundleItemPromotion obj = new ComboPackBundleItemPromotion();
            var product = _itemToItemPromotionService.GetComboProductDetailsByID(id);
            obj.ValidationErrorMsg = "X";
            var ComboPromotionStartDate = _itemToItemPromotionService.GetPromotionMasterById(PromotionID).StartDate;
            var ComboPromotionEndDate = _itemToItemPromotionService.GetPromotionMasterById(PromotionID).EndDate;

            if (product != null)
            {
                DateTime PStartdte = Convert.ToDateTime(product.StartDate.ToString("yyyy-MM-dd"));
                DateTime PEndDate = Convert.ToDateTime(product.EndDate.ToString("yyyy-MM-dd"));

                DateTime CStartdte = Convert.ToDateTime(ComboPromotionStartDate.Value.ToString("yyyy-MM-dd"));
                DateTime CEndDate = Convert.ToDateTime(ComboPromotionEndDate.Value.ToString("yyyy-MM-dd"));



                if (PStartdte >= CStartdte && PEndDate <= CEndDate)
                {

                    @ViewBag.Message = 1;
                    return new JsonResult
                    {

                    };
                    //product = null;
                    //return new JsonResult
                    //{
                    //    Data = product,
                    //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    //};
                }
                else if (PStartdte <= CStartdte && PEndDate >= CEndDate)
                {
                    product.ProductId = 0;
                    return new JsonResult
                    {
                        Data = product,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else if (PStartdte <= CEndDate && PEndDate <= CEndDate && CStartdte >= PStartdte && CStartdte <= PEndDate)
                {
                    product.ProductId = 0;
                    return new JsonResult
                    {
                        Data = product,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
            }

           

            return new JsonResult
            {

            };
        }

        [HttpGet]
        public JsonResult GetPromotionMasters()
        {
            int buyxgety = (int)enumPromotionTypes.ButXGetYFree;
            int buyxgetynfree = (int)enumPromotionTypes.BuyXGetYnFree;
            int buyxngetyfree = (int)enumPromotionTypes.ButXnGetYFree;
            int buyxngetynfree = (int)enumPromotionTypes.ButXnGetYnFree;
            var promotionmasters = _itemToItemPromotionService.GetPromotionMasters(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(p => p.PromotionTypeID == buyxgety
                                                                                            || p.PromotionTypeID == buyxgetynfree || p.PromotionTypeID == buyxngetyfree || p.PromotionTypeID == buyxngetynfree);
            
            return Json(JsonConvert.SerializeObject(promotionmasters, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetBundlePromotionMasters()
        {
          
            int bundlepromotype = (int)enumPromotionTypes.BundleItems;
            var promotionmasters = _itemToItemPromotionService.GetPromotionMasters(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(p => p.PromotionTypeID == bundlepromotype);
            return Json(JsonConvert.SerializeObject(promotionmasters, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetComboPackBundlePromotionMasters()
        {

            int bundlepromotype = (int)enumPromotionTypes.ComboPackBundleItems;
            var promotionmasters = _itemToItemPromotionService.GetComboPromotionMasters(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(p => p.PromotionTypeID == bundlepromotype);
            return Json(JsonConvert.SerializeObject(promotionmasters, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetDiscountsPromotionMasters()
        {
            int buyxgetdiscountfory = (int)enumPromotionTypes.BuyXGetDiscountForY;
            int buyxgetdiscountsforyn = (int)enumPromotionTypes.BuyXGetDiscountsForYn;
            int buyxngetdiscountfory = (int)enumPromotionTypes.ButXnGetDiscounttoY;
            int buyxngetdiscountforyn = (int)enumPromotionTypes.ButXnGetDiscounttoYn;

            var promotionmasters = _itemToItemPromotionService.GetPromotionMasters(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(p => p.PromotionTypeID == buyxgetdiscountfory
                                                                                             || p.PromotionTypeID == buyxgetdiscountsforyn || p.PromotionTypeID == buyxngetdiscountfory || p.PromotionTypeID == buyxngetdiscountforyn);
            return Json(JsonConvert.SerializeObject(promotionmasters, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public JsonResult GetPromotionType(int promotionmasterid)
        {
       
            var promotionmasters = _itemToItemPromotionService.MyProperty(promotionmasterid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            Session["selectedLocation"] = promotionmasters.LocationId;
            var loc = Session["selectedLocation"];
            return new JsonResult
            {
                Data = promotionmasters,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            //return Json(JsonConvert.SerializeObject(promotionmasters, Formatting.None, new JsonSerializerSettings
            //{ ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDiscountsPromotionType(int promotionmasterid)
        {

            var promotionmasters = _itemToItemPromotionService.GetPromotionMasterById(promotionmasterid);
            return new JsonResult
            {
                Data = promotionmasters,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }


        [HttpGet]
        public ActionResult ViewPromotionItems(int promotionmasterid,int promotiontypeid)
        {
            @ViewBag.PromotionMasterId = promotionmasterid;
            @ViewBag.PromotionTypeId = promotiontypeid;
            @ViewBag.PromotionName= _itemToItemPromotionService.MyProperty(
                promotionmasterid, 
                Convert.ToInt32(Session["loggedusercompanyId"].ToString())
                ).PromotionName;
            return View("~/Views/Promotions/ProductXYPromotions/RemoveXYPromotions.cshtml");
        }

        [HttpGet]
        public JsonResult GetPromotions(int promotionmasterid)
        {

            var promotionmasters = _itemToItemPromotionService.GetPromotionItems(promotionmasterid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var orderedpromotions = promotionmasters.OrderBy(v => v.GroupId);
            return Json(JsonConvert.SerializeObject(orderedpromotions, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RemovePromotionItemGroup(InvPromotionDetailsBuyXProduct promoitem)
        {
            promoitem.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var res = _itemToItemPromotionService.RemovePromotionItemGroup(promoitem);
            return Json(res);
        }
    }
}