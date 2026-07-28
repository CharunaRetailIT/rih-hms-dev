
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.Promotions;
using RIT.HMS.Domain.Promotions;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.MasterEnums;
using RIT.HMS.Domain;
using RIT.HMS.Domain.ViewModels.Promotions;
using RIT.HMS.Domain.Promotions.Enums;
using System.IO;
using OfficeOpenXml;
using System.Data;
using OfficeOpenXml.Style;
using System.Drawing;

namespace HospitalityManagement.Controllers.Promotion
{
    [SessionTimeout]
    [Authorize(Roles = "PrdCreatee")]
    public class PromotionController : Controller
    {


        private BLL_Promotion _bllPromotion;
        private BLL_CateringMood _bllCateringMood;
        private BLL_Customer _bllCustomer;
        BLL_ItemToItemPromotion _itemToItemPromotionService;
        BLL_Location _location;
        public PromotionController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
             _bllPromotion = new BLL_Promotion(cn);
            _bllCateringMood = new BLL_CateringMood(cn);
            _bllCustomer = new BLL_Customer(cn);
            _itemToItemPromotionService = new BLL_ItemToItemPromotion(cn);
            _location = new BLL_Location(cn);
        }
        public ActionResult Index()
        {
            InvPromotionMaster pm = new InvPromotionMaster();

            pm.VMPromotionSchedular = new List<RIT.HMS.Domain.ViewModels.Promotions.VMPromotionSchedular>();
            return View("~/Views/Promotions/PromoMaster/Index.cshtml", pm);
        }

        [HttpGet]
        public ActionResult ViewAllPromotions()
        {
            var promo = _bllPromotion.GetAllPromos(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return View("~/Views/Promotions/PromoMaster/ViewAllPromotions.cshtml", promo);
        }

      
        public void HMSPromotions()
        {

            
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var promotions = _bllPromotion.GetPromotionItems(companyid);

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Promotions");
            //---------------2023/03/30 ----------Tharaka---------------
            string compName, Address1, Address2, Address3, Tele, Fax, website = "";
            compName = _location.GetCompanyDetails().CompanyName;
            Address1 = _location.GetCompanyDetails().Address1;
            Address2 = _location.GetCompanyDetails().Address2;
            Address3 = _location.GetCompanyDetails().Address3;
            Tele = _location.GetCompanyDetails().Telephone;
            Fax = _location.GetCompanyDetails().Fax;
            website = _location.GetCompanyDetails().Website;
            #region Headings

            Sheet.Cells[1, 2].Value = compName;
            Sheet.Cells[1, 2, 3, 12].Merge = true;
            Sheet.Cells[1, 2, 3, 12].Style.Font.Size = 12;
            Sheet.Cells[1, 2, 3, 12].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;

            Sheet.Cells[4, 2].Value = Address1 + " " + Address2 + " " + Address3;
            ;
            Sheet.Cells[4, 2, 4, 12].Merge = true;
            Sheet.Cells[4, 2, 4, 12].Style.Font.Size = 10;

            Sheet.Cells[5, 2].Value = "Tel:- " + Tele + " / " + ",  Fax:- " + Fax + ",  Web Site:- " + website;
            Sheet.Cells[5, 2, 5, 12].Merge = true;
            Sheet.Cells[5, 2, 5, 12].Style.Font.Size = 10;

            Sheet.Cells[6, 2].Value = "Promotion Details Report";
            Sheet.Cells[6, 2, 6, 12].Merge = true;
            Sheet.Cells[6, 2, 6, 12].Style.Font.Size = 12;

            var businessUnitDetail = Sheet.Cells[1, 2, 6, 12];
            businessUnitDetail.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            businessUnitDetail.Style.Font.Bold = true;
            businessUnitDetail.Style.Font.Name = "Calibri";

            #endregion
            #region Print Detail Box

            var printDetailBox = Sheet.Cells[3, 13, 5, 14];
            printDetailBox.Style.Font.Size = 8;
            Sheet.Cells[3, 13, 5, 13].Style.Font.Bold = true;

            Sheet.Cells[3, 13].Value = "Date";
            Sheet.Cells[4, 13].Value = "Time";
            Sheet.Cells[5, 13].Value = "Req. By";

            Sheet.Cells[3, 14].Value = DateTime.Now.Date.ToShortDateString();
            Sheet.Cells[4, 14].Value = DateTime.Now.ToString("h:mm tt");

            if (Session["loggeduser"] != null)
                Sheet.Cells[5, 14].Value = Session["loggeduser"].ToString();
            else
                Sheet.Cells[5, 14].Value = " ";


            #endregion 
            Sheet.Cells[8, 1].Value = "Promotion Name";
            Sheet.Cells[8, 2].Value = "Promotion Code";
            Sheet.Cells[8, 3].Value = "Promotion Item";
            Sheet.Cells[8, 4].Value = "Promotion Item Code";
            Sheet.Cells[8, 5].Value = "Promotion Item Serving Unit";
            Sheet.Cells[8, 6].Value = "Promotion Item Qty";
            Sheet.Cells[8, 7].Value = "Free Item";
            Sheet.Cells[8, 8].Value = "Free Item Code";
            Sheet.Cells[8, 9].Value = "Free Item Serving Unit";
            Sheet.Cells[8, 10].Value = "Free Item Qty";

            //Sheet.Cells["A1"].Value = "PromotionName";
            //Sheet.Cells["B1"].Value = "PromotionCode";
            //Sheet.Cells["C1"].Value = "PromotionItem";
            //Sheet.Cells["D1"].Value = "PromotionItemCode";
            //Sheet.Cells["E1"].Value = "PromotionItemServingUnit";
            //Sheet.Cells["F1"].Value = "PromotionItemQty";
            //Sheet.Cells["G1"].Value = "FreeItem";
            //Sheet.Cells["H1"].Value = "FreeItemCode";
            //Sheet.Cells["I1"].Value = "FreeItemServingUnit";
            //Sheet.Cells["J1"].Value = "FreeItemQty";
            int row = 9;
            foreach (var item in promotions)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.PromotionName;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.PromotionCode;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.PromotionItem;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.PromotionItemCode;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.PromotionItemServingUnit;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.PromotionItemQty;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.FreeItem;
                Sheet.Cells[string.Format("H{0}", row)].Value = item.FreeItemCode;
                Sheet.Cells[string.Format("I{0}", row)].Value = item.FreeItemServingUnit;
                Sheet.Cells[string.Format("J{0}", row)].Value = item.FreeItemQty;


                row++;
            }
            #region Header Bold
            Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

            Sheet.Cells[8, 1, 8, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[8, 1, 8, 10].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

            var table = Sheet.Cells[8, 1, 8, 10];
            table.Style.Border.Top.Style =
            table.Style.Border.Left.Style =
            table.Style.Border.Right.Style =
            table.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            table.Style.Font.Bold = true;
            table.Style.Font.Name = "Calibri";
            table.AutoFitColumns();

            #endregion
            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSPromotions.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }


        }

        [HttpPost]
        public ActionResult CreatePromotion(InvPromotionMaster promo)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (!ModelState.IsValid)
            {
                // return View();
                promo.VMPromotionSchedular = new List<VMPromotionSchedular>();
                return View("~/Views/Promotions/PromoMaster/Index.cshtml", promo);
            }

            // bool existscom = _bllPromotion.GetpromoByCode(promo.PromotionCode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            bool existscom = _bllPromotion.GetpromoByGivenCodes(promo.PromotionCode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            
            if (existscom == true)
            {
                ViewBag.Message = "3";
                return View("~/Views/Promotions/PromoMaster/Index.cshtml", promo);
            }

            //int[] numbers = { promo.LocationIds.Count(),
            //                  promo.BusinessTypeIDs.Count(),
            //                  promo.CustomerGroupIds.Count()
            //                };

            List<InvPromotionMaster> promotionmaster = new List<InvPromotionMaster>();
            if (promo.CustomerGroupIds != null)
            {
                foreach (var l in promo.LocationIds)
                {
                    foreach (var b in promo.BusinessTypeIDs)
                    {
                        foreach (var c in promo.CustomerGroupIds)
                        {
                            InvPromotionMaster pr = new InvPromotionMaster();
                            pr.LocationID = Convert.ToInt16(l);
                            pr.PromotionTypeNew = Convert.ToInt16(b);
                            pr.CustomerGroupId = Convert.ToInt16(c);
                            pr.EndDate = promo.EndDate;
                            pr.PromotionCode = promo.PromotionCode;
                            pr.PromotionName = promo.PromotionName;
                            pr.StartDate = promo.StartDate;
                            pr.PromotionTypeID = promo.PromotionTypeID;
                            promotionmaster.Add(pr);
                        }
                    }
                }
            }
            else
            {
                foreach (var l in promo.LocationIds)
                {
                    foreach (var b in promo.BusinessTypeIDs)
                    {
                        //foreach (var c in promo.CustomerGroupIds)
                       // {
                            InvPromotionMaster pr = new InvPromotionMaster();
                            pr.LocationID = Convert.ToInt16(l);
                            pr.PromotionTypeNew = Convert.ToInt16(b);
                            pr.CustomerGroupId = 0;
                            pr.EndDate = promo.EndDate;
                            pr.PromotionCode = promo.PromotionCode;
                            pr.PromotionName = promo.PromotionName;
                            pr.StartDate = promo.StartDate;
                            pr.PromotionTypeID = promo.PromotionTypeID;
                            promotionmaster.Add(pr);
                       // }
                    }
                }
            }
            promo.IsWednesday = false;
            promo.IsWednesdayTime = false;
            promo.IsMonday = false;
            promo.IsMondayTime = false;
            promo.IsSaturday = false;
            promo.IsSaturdayTime = false;
            promo.IsSunday = false;
            promo.IsSundayTime = false;
            promo.IsThuresday = false;
            promo.IsThuresdayTime = false;
            promo.IsTuesday = false;
            promo.IsTuesdayTime = false;
            promo.IsFriday = false;
            promo.IsFridayTime = false;


            List<InvPromotionMaster> promotionlist = new List<InvPromotionMaster>();

            foreach (InvPromotionMaster pr in promotionmaster)
            {
                foreach (var detail in promo.VMPromotionSchedular)
                {
                    if (detail.Day == "Sunday")
                    {
                        pr.IsSunday = true;
                        if (detail.FromTime != null)
                            pr.IsSundayTime = true;
                        pr.SundayStartTime = detail.FromTime;
                        pr.SundayEndTime = detail.ToTime;
                    }
                    if (detail.Day == "Monday")
                    {
                        pr.IsMonday = true;
                        if (detail.FromTime != null)
                            pr.IsMondayTime = true;
                        pr.MondayStartTime = detail.FromTime;
                        pr.MondayEndTime = detail.ToTime;
                    }
                    if (detail.Day == "Tuesday")
                    {
                        pr.IsTuesday = true;
                        if (detail.FromTime != null)
                            pr.IsTuesdayTime = true;
                        pr.TuesdayStartTime = detail.FromTime;
                        pr.TuesdayEndTime = detail.ToTime;
                    }
                    if (detail.Day == "Wednesday")
                    {
                        pr.IsWednesday = true;
                        if (detail.FromTime != null)
                            pr.IsWednesdayTime = true;
                        pr.WednesdayStartTime = detail.FromTime;
                        pr.WednesdayEndTime = detail.ToTime;
                    }
                    if (detail.Day == "Thursday")
                    {
                        pr.IsThuresday = true;
                        if (detail.FromTime != null)
                            pr.IsThuresdayTime = true;
                        pr.ThuresdayStartTime = detail.FromTime;
                        pr.ThuresdayEndTime = detail.ToTime;
                    }
                    if (detail.Day == "Friday")
                    {
                        pr.IsFriday = true;
                        if (detail.FromTime != null)
                            pr.IsFridayTime = true;
                        pr.FridayStartTime = detail.FromTime;
                        pr.FridayEndTime = detail.ToTime;
                    }
                    if (detail.Day == "Saturday")
                    {
                        pr.IsSaturday = true;
                        if (detail.FromTime != null)
                            pr.IsSaturdayTime = true;
                        pr.SaturdayStartTime = detail.FromTime;
                        pr.SaturdayEndTime = detail.ToTime;
                    }
                }

                pr.CashierMessage = "";
                pr.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                pr.CostCentreID = 1;
                pr.DiscountPercentage = 0;
                pr.DiscountValue = 0;
                pr.DisplayMessage = "";
                pr.IsAllLocations = false;
                pr.IsAllType = false;
                pr.IsAutoApply = false;
                pr.IsDelete = false;
                pr.IsIncreseQty = false;
                pr.IsProvider = false;
                pr.IsRaffle = false;
                pr.IsValueRange = false;
                pr.MaximumValue = 0;
                pr.MinimumValue = 0;
                pr.ModifiedDate = DateTime.Now;
                pr.PaymentMethodID = 1;
                pr.Points = 0;
                pr.Remark = "";
                pr.SupplierID = 1;
                pr.CreateDate = DateTime.Now;
                pr.CreateUser = Session["loggeduser"].ToString();
                pr.IsActive = true;
                promotionlist.Add(pr);

                promotionlist.ForEach(p=> 
                {
                    if (p.CustomerGroupId == null)
                    {
                        p.CustomerGroupId = 0;
                    }
                });
                
            }
          
            if (_bllPromotion.SavePromotionMaster(promotionlist, Convert.ToInt32(Session["loggedusercompanyId"].ToString())) == true)
            {

                ViewBag.Message = "1";
                ModelState.Clear();
                InvPromotionMaster promomaster = new InvPromotionMaster();
                promomaster.VMPromotionSchedular = new List<RIT.HMS.Domain.ViewModels.Promotions.VMPromotionSchedular>();
                return View("~/Views/Promotions/PromoMaster/Index.cshtml", promomaster);

            }
            else
            {
                ViewBag.Message = "2";
                return View("~/Views/Promotions/PromoMaster/Index.cshtml", new InvProductMaster());
            }
            
        }

        public JsonResult GetActiveCateringMoods()
        {
            //PromotionService reporsitory = new PromotionService();
            var cateringmood = _bllPromotion.GetActiveCateringMoods(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(cateringmood, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetPromoTypes()
        {
            //PromotionService reporsitory = new PromotionService();
            var promotype = _bllPromotion.GetPromoTypes(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).OrderBy(d=>d.PromotionTypeName);
            return Json(JsonConvert.SerializeObject(promotype, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActivePromos()
        {
            var interdepts = _bllPromotion.GetActivePromos(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(interdepts, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDiscountPromotions()
        {
            int billvaluebaseditemdis = (int)enumPromotionTypes.BillValueBasedDiscounts;
            int billvaluebasedfreeitems = (int)enumPromotionTypes.BillValueBasedItems;
            int discountsfortotalbill = (int)enumPromotionTypes.DiscountForTotalBill;


            var promotionmasters = _itemToItemPromotionService.GetPromotionMasters(Convert.ToInt32(Session["loggedusercompanyId"].ToString())).Where(p => p.PromotionTypeID == billvaluebaseditemdis
                                                                                             || p.PromotionTypeID == billvaluebasedfreeitems || p.PromotionTypeID == discountsfortotalbill);
            return Json(JsonConvert.SerializeObject(promotionmasters, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult LowestPriceWaveOff()
        {
            return View("~/Views/Promotions/LowestPriceWaveOffPromotions/LowestPriceWaveOff.cshtml");
        }

        [HttpPost]
        public ActionResult CreateLowestPriceWaveOff(InvPromoLowestPriceWaveOff lowestpricewave)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Promotions/LowestPriceWaveOffPromotions/LowestPriceWaveOff.cshtml", lowestpricewave);
            }

            var exists = _bllPromotion.GetLowestPriceWaveOffByCode(lowestpricewave.LowestPriceWaveOffCode, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            lowestpricewave.CreatedDate = DateTime.UtcNow;
            lowestpricewave.CreatedUser = Session["loggeduser"].ToString();
            lowestpricewave.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (exists != null)
            {
                ViewBag.Message = "3";
                return View("~/Views/Promotions/LowestPriceWaveOffPromotions/LowestPriceWaveOff.cshtml", lowestpricewave);
            }
            if (_bllPromotion.SaveLowestPriceWave(lowestpricewave) == 1)
            {
                @ViewBag.Message = "1";
            }

            else
            {
                @ViewBag.Message = "2";
            }



            ViewBag.LowestPriceWaveOffCode = lowestpricewave.LowestPriceWaveOffCode;
            return View("~/Views/Promotions/LowestPriceWaveOffPromotions/LowestPriceWaveOff.cshtml", lowestpricewave);

        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            var promo = _bllPromotion.GetPromoById(id);     
            ViewBag.PromotionCode = promo.PromotionCode;
            ViewBag.PromotionName = promo.PromotionName;
            ViewBag.StartDate = promo.StartDate;
            ViewBag.LocationID = promo.LocationID;
            ViewBag.PromotionTypeId = promo.PromotionTypeID;
            ViewBag.EndDate = promo.EndDate;
            ViewBag.customerGroupId = promo.CustomerGroupId;
            ViewBag.businessTypeID = promo.PromotionTypeNew;
            List<VMPromotionSchedular> dates = new List<VMPromotionSchedular>();
            VMPromotionSchedular date;

            if (promo.IsSunday)
            {
                date = new VMPromotionSchedular();
                date.Day = "Sunday";
                date.DayId = "Sunday";
                date.FromTime = promo.SundayStartTime;
                date.ToTime = promo.SundayEndTime;
                dates.Add(date);
            }
            if (promo.IsMonday)
            {
                date = new VMPromotionSchedular();
                date.Day = "Monday";
                date.DayId = "Monday";
                date.FromTime = promo.MondayStartTime;
                date.ToTime = promo.MondayEndTime;
                dates.Add(date);
            }
            if (promo.IsTuesday)
            {
                date = new VMPromotionSchedular();
                date.Day = "Tuesday";
                date.DayId = "Tuesday";
                date.FromTime = promo.TuesdayStartTime;
                date.ToTime = promo.TuesdayEndTime;
                dates.Add(date);
            }
            if (promo.IsWednesday)
            {
                date = new VMPromotionSchedular();
                date.Day = "Wednesday";
                date.DayId = "Wednesday";
                date.FromTime = promo.WednesdayStartTime;
                date.ToTime = promo.WednesdayEndTime;
                dates.Add(date);
            }
            if (promo.IsThuresday)
            {
                date = new VMPromotionSchedular();
                date.Day = "Thursday";
                date.DayId = "Thursday";
                date.FromTime = promo.ThuresdayStartTime;
                date.ToTime = promo.ThuresdayStartTime;
                dates.Add(date);
            }
            if (promo.IsFriday)
            {
                date = new VMPromotionSchedular();
                date.Day = "Friday";
                date.DayId = "Friday";
                date.FromTime = promo.FridayStartTime;
                date.ToTime = promo.FridayStartTime;
                dates.Add(date);
            }
            if (promo.IsSaturday)
            {
                date = new VMPromotionSchedular();
                date.Day = "Saturday";
                date.DayId = "Saturday";
                date.FromTime = promo.SaturdayStartTime;
                date.ToTime = promo.SaturdayStartTime;
                dates.Add(date);
            }


            promo.VMPromotionSchedular = dates;
            return View("~/Views/Promotions/PromoMaster/Edit.cshtml", promo);
        }

        public ActionResult EditLowest()
        {
            return View("~/Views/Promotions/LowestPriceWaveOffPromotions/EditLowest.cshtml");
        }

        [HttpPost]
        public ActionResult Edit(InvPromotionMaster promo)
        {

            var existspromo = _bllPromotion.GetPromoById(promo.InvPromotionMasterID);
            ViewBag.PromotionCode = promo.PromotionCode;
            ViewBag.PromotionName = promo.PromotionName;
            ViewBag.StartDate = promo.StartDate;
            ViewBag.LocationID = promo.LocationID;
            ViewBag.PromotionTypeId = promo.PromotionTypeID;
            ViewBag.EndDate = promo.EndDate;
            ViewBag.customerGroupId = promo.CustomerGroupId;
            ViewBag.businessTypeID = promo.PromotionTypeNew;

            //if (promo.VMPromotionSchedular == null || promo.VMPromotionSchedular.Count == 0)
            //{
            //    @ViewBag.Message = "3";
            //    return View("~/Views/Promotions/PromoMaster/Edit.cshtml", promo);
            //}

            existspromo.VMPromotionSchedular = promo.VMPromotionSchedular;
            existspromo.StartDate = promo.StartDate;
            existspromo.EndDate = promo.EndDate;
            existspromo.PromotionTypeNew = promo.PromotionTypeNew;
            existspromo.LocationID = promo.LocationID;
            existspromo.PromotionTypeID = promo.PromotionTypeID;
            existspromo.PromotionCode = promo.PromotionCode;
            existspromo.PromotionName = promo.PromotionName;
            existspromo.CustomerGroupId = promo.CustomerGroupId;
            existspromo.PromotionCount = promo.PromotionCount;

            foreach (var detail in promo.VMPromotionSchedular)
            {
                if (detail.Day == "Sunday")
                {
                    existspromo.IsSunday = true;
                    if (detail.FromTime != null)
                        existspromo.IsSundayTime = true;
                    existspromo.SundayStartTime = detail.FromTime;
                    existspromo.SundayEndTime = detail.ToTime;
                }
                else if (!promo.VMPromotionSchedular.Contains(promo.VMPromotionSchedular.Where(s => s.Day == "Sunday").FirstOrDefault()))
                {
                    existspromo.IsSunday = false;                    
                    existspromo.IsSundayTime = false;
                    existspromo.SundayStartTime = null;
                    existspromo.SundayEndTime = null;
                }

                if (detail.Day == "Monday")
                {
                    existspromo.IsMonday = true;
                    if (detail.FromTime != null)
                        existspromo.IsMondayTime = true;
                    existspromo.MondayStartTime = detail.FromTime;
                    existspromo.MondayEndTime = detail.ToTime;
                }
                else if (!promo.VMPromotionSchedular.Contains(promo.VMPromotionSchedular.Where(s => s.Day == "Monday").FirstOrDefault()))
                {
                    existspromo.IsMonday = false;                    
                    existspromo.IsMondayTime = false;
                    existspromo.MondayStartTime = null;
                    existspromo.MondayEndTime = null;
                }

                if (detail.Day == "Tuesday")
                {
                    existspromo.IsTuesday = true;
                    if (detail.FromTime != null)
                        existspromo.IsTuesdayTime = true;
                    existspromo.TuesdayStartTime = detail.FromTime;
                    existspromo.TuesdayEndTime = detail.ToTime;
                }
                else if (!promo.VMPromotionSchedular.Contains(promo.VMPromotionSchedular.Where(s => s.Day == "Tuesday").FirstOrDefault()))
                {
                    existspromo.IsTuesday = false;                   
                    existspromo.IsTuesdayTime = false;
                    existspromo.TuesdayStartTime = null;
                    existspromo.TuesdayEndTime = null;
                }

                if (detail.Day == "Wednesday")
                {
                    existspromo.IsWednesday = true;
                    if (detail.FromTime != null)
                        existspromo.IsWednesdayTime = true;
                    existspromo.WednesdayStartTime = detail.FromTime;
                    existspromo.WednesdayEndTime = detail.ToTime;
                }
                else if (!promo.VMPromotionSchedular.Contains(promo.VMPromotionSchedular.Where(s => s.Day == "Wednesday").FirstOrDefault()))
                {
                    existspromo.IsWednesday = false;                   
                    existspromo.IsWednesdayTime = false;
                    existspromo.WednesdayStartTime = null;
                    existspromo.WednesdayEndTime = null;

                }

                if (detail.Day == "Thursday")
                {
                    existspromo.IsThuresday = true;
                    if (detail.FromTime != null)
                        existspromo.IsThuresdayTime = true;
                    existspromo.ThuresdayStartTime = detail.FromTime;
                    existspromo.ThuresdayEndTime = detail.ToTime;
                }
                else if (!promo.VMPromotionSchedular.Contains(promo.VMPromotionSchedular.Where(s => s.Day == "Thursday").FirstOrDefault()))
                {
                    existspromo.IsThuresday = false;                   
                    existspromo.IsThuresdayTime = false;
                    existspromo.ThuresdayStartTime = null;
                    existspromo.ThuresdayEndTime = null;
                }
                if (detail.Day == "Friday")
                {
                    existspromo.IsFriday = true;
                    if (detail.FromTime != null)
                        existspromo.IsFridayTime = true;
                    existspromo.FridayStartTime = detail.FromTime;
                    existspromo.FridayEndTime = detail.ToTime;
                }
                else if (!promo.VMPromotionSchedular.Contains(promo.VMPromotionSchedular.Where(s => s.Day == "Friday").FirstOrDefault()))
                {
                    existspromo.IsFriday = false;                   
                    existspromo.IsFridayTime = false;
                    existspromo.FridayStartTime = null;
                    existspromo.FridayEndTime = null;
                }
                if (detail.Day == "Saturday")
                {
                    existspromo.IsSaturday = true;
                    if (detail.FromTime != null)
                        existspromo.IsSaturdayTime = true;
                    existspromo.SaturdayStartTime = detail.FromTime;
                    existspromo.SaturdayEndTime = detail.ToTime;
                }
                else if (!promo.VMPromotionSchedular.Contains(promo.VMPromotionSchedular.Where(s => s.Day == "Saturday").FirstOrDefault()))
                {
                    existspromo.IsSaturday = false;                   
                    existspromo.IsSaturdayTime = false;
                    existspromo.SaturdayStartTime = null;
                    existspromo.SaturdayEndTime = null;
                }

            }

            //var id = RouteData.Values["id"] + Request.Url.Query;
            //promo.InvPromotionMasterID = Convert.ToInt64(promo.InvPromotionMasterID);
            existspromo.IsActive = promo.IsActive;
            existspromo.ModifiedDate = DateTime.Now;
            existspromo.ModifiedUser = Session["loggeduser"].ToString();
            existspromo.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());


            @ViewBag.PromotionCode = existspromo.PromotionCode;

            if (_bllPromotion.UpdatePromoMaster(existspromo))
            {
                @ViewBag.Message = "1";
                ViewBag.PromotionCode = existspromo.PromotionCode;
                return View("~/Views/Promotions/PromoMaster/Edit.cshtml", existspromo);

            }
            else
            {
                @ViewBag.Message = "2";
                return View("~/Views/Promotions/PromoMaster/Edit.cshtml", existspromo);
            }

        }

        [HttpPost]
        public ActionResult EditLowest(InvPromoLowestPriceWaveOff promolowestwave)
        {
           
            var promolowest = _bllPromotion.GetLowestPriceWaveOffById(promolowestwave.InvPromoLowestPriceWaveOffID);
            promolowest.LowestPriceWaveOffCode = promolowestwave.LowestPriceWaveOffCode;
            promolowest.InvPromotionMasterID = promolowestwave.InvPromotionMasterID;
            promolowest.LowestPrice = promolowestwave.LowestPrice;
            promolowest.IsFullWaveOff = promolowestwave.IsFullWaveOff;
            promolowest.Qty = promolowestwave.Qty;
            promolowest.Remark = promolowestwave.Remark;
            promolowest.Status = promolowestwave.Status;
            promolowest.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (_bllPromotion.UpdateLowestPriceWave(promolowest) == 1)
            {

                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }
            ViewBag.InvPromoLowestPriceWaveOffID = promolowest.InvPromoLowestPriceWaveOffID;
            return View();
        }




    }
}