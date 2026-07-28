
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Transactions
{
    [SessionTimeout]
    public class ProductionController : Controller
    {
        private readonly BLL_ProductionNote _bllproductionnote;
        private readonly  BLL_Product _bllproduct;
        private readonly BLL_Common _bllcommon;

        public ProductionController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllproductionnote = new BLL_ProductionNote(cn);
            _bllproduct = new BLL_Product(cn);
            _bllcommon = new BLL_Common(cn);

        }


    [Authorize(Roles = "ProductionCreatee")]
        [HttpGet]
        public ActionResult Index()
        {
            var productionheader = new ProductionNoteHeader();

            //  productionheader.DocumentNo = commonService.GetDocumentNo("Production", 1, "1", 2, true);

            int docid = _bllcommon.GetDcumentId("Production", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentNo("Production",
                                                    Convert.ToInt32(Session["loggeduserlocId"]),
                                                    "1",
                                                     docid,
                                                     true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            productionheader.DocumentNo = docnum;

            return View("~/Views/Transactions/Production/Production.cshtml", productionheader);
        }

        [Authorize(Roles = "ProductionView")]
        [HttpGet]
        public ActionResult ViewAllProductions()
        {
            var productions = _bllproductionnote.GetActiveProductions(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            productions.ToList().ForEach(
                p=>{ p.ProductName = _bllproduct.GetProductById(p.ProductId).ProductName; });

            return View("~/Views/Transactions/Production/ViewProductionNotes.cshtml", productions);
        }

        [Authorize(Roles = "ProductionCreatee")]
        [HttpPost]
        public ActionResult SubmitProduction(ProductionNoteHeader pnheader)
        {
            if (pnheader.ProductionLocId == 0)
            {
                ModelState.AddModelError("ProductionLocId", "Please select the location !");
                return View("~/Views/Transactions/Production/Production.cshtml", pnheader);
            }
            if (pnheader.ProductionDetail.Count > 0)
            {

                //   pnheader.DocumentNo = commonService.GetDocumentNo("Production", 1, "1", 2, false);
                int docid = _bllcommon.GetDcumentId("Production", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentNo("Production",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                        "1",
                                                         docid,
                                                         true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                pnheader.DocumentNo = docnum;

                pnheader.CompanyID = 1;
                pnheader.LocationId=Convert.ToInt32(Session["loggeduserlocId"].ToString());
                pnheader.DataTransfer = 0;
                pnheader.CreatedDate = DateTime.Now;
                pnheader.CreatedUser = Session["loggeduser"].ToString();
                pnheader.IsTempPN = false;
                ViewBag.PCode = pnheader.DocumentNo;
                pnheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                if (_bllproductionnote.SubmitProduction(pnheader))
                {
                    @ViewBag.Message = "1";
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
                ViewBag.ProductionLocId = pnheader.ProductionLocId;
                pnheader.Remark = pnheader.Remark;
                ModelState.AddModelError("ProductionDetail", "Please enter production details !");
                
                @ViewBag.Message = "3";
                pnheader.DocumentNo = _bllcommon.GetDocumentNo("Production", 1, "1", 2, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                return View("~/Views/Transactions/Production/Production.cshtml", pnheader);
            }          
        }

        [Authorize(Roles = "ProductionCreatee")]
        [HttpPost]
        public ActionResult TempSaveProduction(ProductionNoteHeader pnheader)
        {
            if (pnheader.ProductionDetail.Count > 0)
            {
                pnheader.DocumentNo = _bllcommon.GetDocumentNo("Production", 1, "1", 2, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
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
                pnheader.DocumentNo = _bllcommon.GetDocumentNo("Production", 1, "1", 2, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                return View("~/Views/Transactions/Production/Production.cshtml", pnheader);
            }

        }
        [Authorize(Roles = "ProductionEdit")]
        [HttpGet]
        public ActionResult EditProduction(long id)
        {
            
            var production = _bllproductionnote.GetActiveProductionsById(id);
            production.ProductionDetail = _bllproductionnote.GetActiveProductionDetById(id);
            production.ProductionDetail.ForEach(pd=>
                {
                    pd.QtyUOM =pd.MaterialQty+""+ _bllproduct.GetUOMById(_bllproduct.GetProductById(pd.MaterialId).PurchasingUnit);
                }
            );
            ViewBag.PNode = production.DocumentNo;
            ViewBag.ProductionLocId = production.ProductionLocId;
            ViewBag.ProductId= production.ProductId;

            return View("~/Views/Transactions/Production/EditProduction.cshtml", production);
           
        }

        [Authorize(Roles = "ProductionCreatee")]
        [HttpPost]
        public ActionResult EditProduction(ProductionNoteHeader production)
        {
            if (production.ProductionDetail.Count > 0)
            {
                if (production.IsTempPN)
                {
                    production.DocumentNo = _bllcommon.GetDocumentNo("Production", 1, "1", 2, false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
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
                production.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

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
                production.DocumentNo = _bllcommon.GetDocumentNo("Production", 1, "1", 2, true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                return View("~/Views/Transactions/Production/Production.cshtml", production);
            }

            

        }

        [HttpGet] 
        public JsonResult GetProductionData(long productid, decimal qty,
                                            long locid,string locname,string productname)
        {
            List<ProductionViewModel> vvm = new List<ProductionViewModel>();
            var receipe = _bllproductionnote.GetRecepieByProductId(productid, locid,qty,0, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var itemToRemove = receipe.Single(r => r.MaterialId == 0);
            receipe.Remove(itemToRemove);

            return new JsonResult { Data = receipe, JsonRequestBehavior = JsonRequestBehavior.AllowGet };


        }

    }
}