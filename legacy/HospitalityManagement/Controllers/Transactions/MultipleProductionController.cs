
using Newtonsoft.Json;
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.Configurations;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Transactions
{
    [SessionTimeout]
    public class MultipleProductionController : Controller
    {
        private readonly BLL_ProductionNote _bllproductionnote;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_Common _bllCommon;
        private AppManager _appmanager = new AppManager();
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_RequestNote _bllrequestnote;
        private readonly BLL_ServingUnit _bllservingunit;

        public MultipleProductionController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllproductionnote = new BLL_ProductionNote(cn);
            _bllproduct = new BLL_Product(cn);
            _bllCommon = new BLL_Common(cn);
             _appmanager = new AppManager(cn);
            _bllconfiguration = new BLL_Configuration(cn);
            _bllrequestnote = new BLL_RequestNote(cn);
            _bllservingunit = new BLL_ServingUnit(cn);
        }
        public ActionResult Index()
        {

            // check permissions
            if (!_appmanager.SetPermissions(7, Session["loggeduserempcode"].ToString(), "ProductionCreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create Production Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            Session["POWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            var productionheader = new ProductionNoteHeader();
            // productionheader.DocumentNo = commonService.GetDocumentNo("Production", 1, "1", 2, true);

            int docid = _bllCommon.GetDcumentId("Production", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllCommon.GetDocumentNo("Production",
                                                    Convert.ToInt32(Session["loggeduserlocId"]),
                                                    "1",
                                                     docid,
                                                     true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            productionheader.DocumentNo = docnum;
            return View("~/Views/Transactions/MultipleProduction/MultipleProductions.cshtml", productionheader);

        }

    //    [Authorize(Roles = "ProductionView")]
        [HttpGet]
        public ActionResult ViewAllProductions()
        {

            // check permissions
            if (!_appmanager.SetPermissions(7, Session["loggeduserempcode"].ToString(), "ProductionView"))
            {
                @ViewBag.Permissions = "No user permissions to View Production Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            var productions = _bllproductionnote.GetActiveProductions(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            productions.ToList().ForEach(
                p =>
                {

                    p.ProductionDetail = _bllproductionnote.GetProductionNoteDetailByHeaderId(p.ProductionNoteHeaderId).ToList();
                    p.ProductionDetail.ToList().ForEach(
                        pd =>
                        {
                            pd.ProductName = _bllproduct.GetProductById(pd.ProductId).ProductName;
                        }
                        );

                });

            return View("~/Views/Transactions/Production/ViewProductionNotes.cshtml", productions);
        }

      //  [Authorize(Roles = "ProductionCreatee")]
        [HttpPost]
        public ActionResult SubmitProduction(ProductionNoteHeader pnheader)
        {
            // check permissions
            if (!_appmanager.SetPermissions(7, Session["loggeduserempcode"].ToString(), "ProductionCreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create Production Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            if (pnheader.ProductionLocId == 0)
            {
                ModelState.AddModelError("ProductionLocId", "Please select the location !");
                return View("~/Views/Transactions/MultipleProduction/MultipleProductions.cshtml", pnheader);
            }


            if (pnheader.ProductionDetail.Count == 0)
            {
                
                    ViewBag.ProductionLocId = pnheader.ProductionLocId;
                    pnheader.Remark = pnheader.Remark;
                    ModelState.AddModelError("ProductionDetail", "Please enter production details !");
                    @ViewBag.Message = "3";
                    pnheader.DocumentNo = pnheader.DocumentNo;
                    return View("~/Views/Transactions/MultipleProduction/MultipleProductions.cshtml", pnheader);
                
            }

            int docid = _bllCommon.GetDcumentId("Production", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllCommon.GetDocumentNo("Production",
                                                    Convert.ToInt32(Session["loggeduserlocId"]),
                                                    "1",
                                                     docid,
                                                     false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            if (pnheader.ProductionDetail.Count > 0)
            {
                // pnheader.DocumentNo = commonService.GetDocumentNo("Production", 1, "1", 2, false);


                pnheader.DocumentId = docid;
                pnheader.DocumentNo = docnum;

                pnheader.CompanyID = 1;
                pnheader.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                pnheader.DataTransfer = 0;
                pnheader.CreatedDate = DateTime.Now;
                pnheader.CreatedUser = Session["loggeduser"].ToString();
                pnheader.IsTempPN = false;
                pnheader.ReceiptNo = "";
                pnheader.UnitNo = 0;
                pnheader.ReceiptLocID = Convert.ToInt32(pnheader.ProductionLocId);
                ViewBag.PCode = pnheader.DocumentNo;
                pnheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                
                if (_bllproductionnote.SubmitMaltipleProduction(pnheader))
                {
                    @ViewBag.Message = "1";
                    ModelState.Clear();
                    var ProductionNote = new ProductionNoteHeader();
                    docnum = _bllCommon.GetDocumentNo("Production",
                                               Convert.ToInt32(Session["loggeduserlocId"]),
                                               "1",
                                                docid,
                                                true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    ProductionNote.DocumentNo = docnum;
                    return View("~/Views/Transactions/MultipleProduction/MultipleProductions.cshtml", ProductionNote);
                }
                else
                {
                    @ViewBag.Message = "3";
                    return View("~/Views/Transactions/MultipleProduction/MultipleProductions.cshtml", pnheader);
                }
            }
            else
            {
                ViewBag.ProductionLocId = pnheader.ProductionLocId;
                pnheader.Remark = pnheader.Remark;
                ModelState.AddModelError("ProductionDetail", "Please enter production details !");
                @ViewBag.Message = "3";
                pnheader.DocumentNo = _bllCommon.GetDocumentNo("Production", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                return View("~/Views/Transactions/MultipleProduction/MultipleProductions.cshtml", pnheader);
            }
        }

     //   [Authorize(Roles = "ProductionCreatee")]
        [HttpPost]
        public ActionResult TempSaveProduction(ProductionNoteHeader pnheader)
        {

            // check permissions
            if (!_appmanager.SetPermissions(7, Session["loggeduserempcode"].ToString(), "ProductionCreatee"))
            {
                @ViewBag.Permissions = "No user permissions to Create Production Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            int docid = _bllCommon.GetDcumentId("Production", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            if (pnheader.ProductionDetail.Count > 0)
            {
                // pnheader.DocumentNo = commonService.GetDocumentNo("Production", 1, "1", 2, true);


                var docnum = _bllCommon.GetDocumentNo("Production",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                        "1",
                                                         docid,
                                                         true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                pnheader.DocumentId = docid;
                pnheader.DocumentNo = docnum;
                pnheader.CompanyID = 1;
                pnheader.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                pnheader.DataTransfer = 0;
                pnheader.CreatedDate = DateTime.Now;
                pnheader.CreatedUser = Session["loggeduser"].ToString();
                pnheader.IsTempPN = true;
                ViewBag.PCode = pnheader.DocumentNo;

                if (_bllproductionnote.SubmitProduction(pnheader))
                {
                    @ViewBag.Message = "2";
                    return View("~/Views/Transactions/Production/Production.cshtml", new ProductionNoteHeader());
                }
                else
                {
                    @ViewBag.Message = "3";
                    return View("~/Views/Transactions/Production/Production.cshtml", pnheader);
                }
            }
            else
            {
                @ViewBag.Message = "3";

                // pnheader.DocumentNo = commonService.GetDocumentNo("Production", 1, "1", 2, true);

                pnheader.DocumentNo = _bllCommon.GetDocumentNo("Production",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                        "1",
                                                         docid,
                                                         true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                return View("~/Views/Transactions/Production/Production.cshtml", pnheader);
            }

        }
    //    [Authorize(Roles = "ProductionEdit")]
        [HttpGet]
        public ActionResult EditProduction(long id)
        {

            // check permissions
            if (!_appmanager.SetPermissions(7, Session["loggeduserempcode"].ToString(), "ProductionEdit"))
            {
                @ViewBag.Permissions = "No user permissions to Edit Production Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            var production = _bllproductionnote.GetActiveProductionsById(id);
            production.ProductionDetail = _bllproductionnote.GetActiveProductionDetById(id);
            production.ProductionDetail.ForEach(pd =>
            {
                pd.QtyUOM = pd.MaterialQty + "" + _bllproduct.GetUOMById(_bllproduct.GetProductById(pd.MaterialId).PurchasingUnit);
            }
            );
            ViewBag.PNode = production.DocumentNo;
            ViewBag.ProductionLocId = production.ProductionLocId;
            ViewBag.ProductId = production.ProductId;

            return View("~/Views/Transactions/Production/EditProduction.cshtml", production);

        }

    //    [Authorize(Roles = "ProductionCreatee")]
        [HttpPost]
        public ActionResult EditProduction(ProductionNoteHeader production)
        {

            // check permissions
            if (!_appmanager.SetPermissions(7, Session["loggeduserempcode"].ToString(), "ProductionEdit"))
            {
                @ViewBag.Permissions = "No user permissions to Edit Production Notes";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions

            int docid = _bllCommon.GetDcumentId("Production", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            if (production.ProductionDetail.Count > 0)
            {
                if (production.IsTempPN)
                {
                    //  production.DocumentNo = commonService.GetDocumentNo("Production", 1, "1", 2, false);

                    production.DocumentNo = _bllCommon.GetDocumentNo("Production",
                                                     Convert.ToInt32(Session["loggeduserlocId"]),
                                                     "1",
                                                      docid,
                                                      false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    production.DocumentId = docid;

                    production.IsTempPN = false;
                }

                var dbproduction = _bllproductionnote.GetActiveProductionsById(production.ProductionNoteHeaderId);
                dbproduction.ProductionDetail = _bllproductionnote.GetActiveProductionDetById(production.ProductionNoteHeaderId);

                var oldproduction = dbproduction;

                //dbproduction.ProductId = production.ProductId;
                //dbproduction.ProductQty = production.ProductQty;
                //dbproduction.ProductCostPrice = production.ProductCostPrice;
                //dbproduction.ProductSellingPrice = production.ProductSellingPrice;
                //dbproduction.IsTempPN = dbproduction.IsTempPN;
                //dbproduction.Remark = production.Remark;
                //dbproduction.ProductionLocId = dbproduction.ProductionLocId;
                //dbproduction.ModifiedUser= Session["loggeduser"].ToString();
                //dbproduction.ModifiedDate = DateTime.Now;
                //ViewBag.PCode = production.DocumentNo;


                if (_bllproductionnote.EditProduction(production, oldproduction))
                {
                    @ViewBag.Message = "1";
                    return View("~/Views/Transactions/Production/Production.cshtml", new ProductionNoteHeader());
                }
                else
                {
                    @ViewBag.Message = "3";
                    return View("~/Views/Transactions/Production/Production.cshtml", production);
                }
            }
            else
            {
                //  production.DocumentNo = commonService.GetDocumentNo("Production", 1, "1", 2, true);

                production.DocumentNo = _bllCommon.GetDocumentNo("Production",
                                                    Convert.ToInt32(Session["loggeduserlocId"]),
                                                    "1",
                                                     docid,
                                                     true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                production.DocumentId = docid;

                return View("~/Views/Transactions/Production/Production.cshtml", production);
            }



        }

        [HttpGet]
        public JsonResult GetMultipleProductionData(long productid, decimal qty,
                                                    long locid, string locname,
                                                    string productname, long servingunitid)
        {


            //List<ProductionViewModel> vvm = new List<ProductionViewModel>();
            var prereceipe = _bllproductionnote.GetDefinedRecepieByProductId(productid, locid, qty, servingunitid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var itemToRemoved = prereceipe.Single(r => r.MaterialId == 0);
            prereceipe.Remove(itemToRemoved);
            if (prereceipe != null && prereceipe.Count != 0)
            {

                //var itemToRemove = prereceipe.Single(r => r.MaterialId == 0);
                //prereceipe.Remove(itemToRemove);
                var dbstock = _bllproduct.GetProductStockMasterByProductIdLocId(productid, locid);
                if (prereceipe.Count != 0)
                {
                    prereceipe.First().FinalProductServingUnitId = Convert.ToInt32(servingunitid);
                    prereceipe.First().FinalProductionQuantity = qty;
                    prereceipe.First().FinalProductCostPrice = qty * (dbstock.CostPrice);
                    prereceipe.First().FinalProductSellingPrice = qty * (dbstock.SellingPrice);
                }
                return new JsonResult { Data = prereceipe, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {

                var receipe = _bllproductionnote.GetRecepieByProductId(productid, locid, qty, servingunitid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var itemToRemove = receipe.Single(r => r.MaterialId == 0);
                receipe.Remove(itemToRemove);
                var dbstock = _bllproduct.GetProductStockMasterByProductIdLocId(productid, locid);
                if (receipe.Count != 0)
                {
                    receipe.First().FinalProductServingUnitId = Convert.ToInt32(servingunitid);
                    receipe.First().FinalProductionQuantity = qty;
                    receipe.First().FinalProductCostPrice = qty * (dbstock.CostPrice);
                    receipe.First().FinalProductSellingPrice = qty * (dbstock.SellingPrice);
                }
                return new JsonResult { Data = receipe, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

        }


        [HttpGet]
        public JsonResult GetRequestNoteData(int requestnoteid, int locid)
        {
          
            var reqheader = _bllrequestnote.GetAcceptedHeaderById(requestnoteid);
            var reqdetail = _bllrequestnote.GetAcceptedDetailByHeaderId(requestnoteid);
            var req = _bllrequestnote.GetRequestNoteByDocNummber(reqheader.DocumentNo);
            List<ProductionViewModel> prereceipe1 = new List<ProductionViewModel>();

            List<ProductionViewModel> prereceipe = new List<ProductionViewModel>();

            foreach (var det in req.RequestDetail)
            {
                var prd = req.RequestDetail.Where(p => p.ProductId == det.ProductId).FirstOrDefault();
                decimal reqqty = 0;
                if (prd == null)
                {
                     reqqty = _bllrequestnote.GetRequestNoteByDocNummber(reqheader.DocumentNo)
                         .RequestDetail.Where(p => p.ProductId == det.ProductId).FirstOrDefault().RequestQty;


                }
                decimal prdqty = 0;
                if (prd != null)
                {
                    prdqty = prd.RequestQty;
                }
                else
                {
                    prdqty = 0;
                }
                //  1
                var su = _bllservingunit.GetProductServingUnitsByPrdIdServingUnit(det.ProductId,det.ServingUnit,Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                prereceipe = _bllproductionnote.GetDefinedRecepieForProductions(det.ProductId, locid, prdqty, su.ProductServingUnitId, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var itemToRemoved = prereceipe.Single(r => r.MaterialId == 0);
                prereceipe.Remove(itemToRemoved);
                if (prereceipe != null && prereceipe.Count != 0)
                {
                    var dbstock = _bllproduct.GetProductStockMasterByProductIdLocId(det.ProductId, locid);
                    if (prereceipe.Count != 0)
                    {
                        prereceipe.First().FinalProductServingUnitId = Convert.ToInt32(det.ServingUnitId);
                        if (prd != null)
                        {
                            prereceipe.First().FinalProductionQuantity = prd.RequestQty;
                        }
                        else
                        {
                            prereceipe.First().FinalProductionQuantity = reqqty;
                        }           
                        prereceipe.First().FinalProductCostPrice = prd.RequestQty * (dbstock.CostPrice);
                        prereceipe.First().FinalProductSellingPrice = prd.RequestQty * (dbstock.SellingPrice);
                        prereceipe.First().ProductId = det.ProductId;

                        prereceipe1.AddRange(prereceipe);
                       
                        // prereceipe1.Add(prereceipe.First());

                    }
                }
                else
                {

                    prereceipe = _bllproductionnote.GetDefinedRecepieForProductions(det.ProductId, locid, prdqty, det.ServingUnitId, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    var itemToRemove = prereceipe.Single(r => r.MaterialId == 0);
                    prereceipe.Remove(itemToRemove);
                    var dbstock = _bllproduct.GetProductStockMasterByProductIdLocId(det.ProductId, locid);
                    if (prereceipe.Count != 0)
                    {
                        prereceipe.First().FinalProductServingUnitId = Convert.ToInt32(det.ServingUnitId);

                        if (prd != null)
                        {
                            prereceipe.First().FinalProductionQuantity = prd.RequestQty;
                        }
                        else
                        {
                            prereceipe.First().FinalProductionQuantity = reqqty;
                        }

                        prereceipe.First().FinalProductCostPrice = prd.RequestQty * (dbstock.CostPrice);
                        prereceipe.First().FinalProductSellingPrice = prd.RequestQty * (dbstock.SellingPrice);
                        prereceipe.First().ProductId = det.ProductId;                                          
                        prereceipe1.AddRange(prereceipe);

                    }                 
                }
                prereceipe.ForEach(p => {
                    p.RequestProductName = _bllproduct.GetProductById(Convert.ToInt64(det.ProductId)).ProductName;
                    p.ProductId = det.ProductId;
                  
                });           
            }

            prereceipe1.ForEach(p =>
            {
                p.RequestProductName = _bllproduct.GetProductById(Convert.ToInt64(p.ProductId)).ProductName;
                p.ProductId = p.ProductId;

            });

            return new JsonResult { Data = prereceipe1, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            

        }

        [HttpGet]
        [Authorize(Roles = "Reports")]
        public ActionResult ProductionReport()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTProduction"))
                {
                    @ViewBag.Permissions = "No user permissions to View Production";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            return View("~/Views/Reports/Production/ProductionDetails.cshtml");
        }

        [HttpGet]
        public JsonResult GetProductionNotes(long locid, DateTime datefrom, DateTime dateto)
        {

            var vvm = _bllproductionnote.GetPeoductionsByLocId(locid, datefrom, dateto, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(vvm, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        [Authorize(Roles = "Reports")]
        public ActionResult ProductionReport(ProductionReportViewModel vm)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTProduction"))
                {
                    @ViewBag.Permissions = "No user permissions to View Production Report";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }
            var sss = _bllproductionnote.GetProductions(vm.LocationId, vm.DateFrom, vm.DateTo,vm.DocumentId);
            return View("~/Views/Reports/Production/ProductionDetails.cshtml", sss);
        }

    }
}