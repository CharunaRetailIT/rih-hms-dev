

using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    public class GiftVoucherPurchaseOrderController : Controller
    {
        BLL_GiftVoucherPO _billGiftVPO;
        BLL_GiftVoucherGroup _billGiftVoucherGroup;
        private InvGiftVoucherMaster existingInvGiftVoucherMaster;
        public GiftVoucherPurchaseOrderController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _billGiftVoucherGroup = new BLL_GiftVoucherGroup(cn);
            _billGiftVPO = new BLL_GiftVoucherPO(cn);
        }

        // GET: GiftVoucherPurchaseOrder
        public ActionResult Index()
        {
            return View();
        }

        // GET: GiftVoucherPurchaseOrder/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: GiftVoucherPurchaseOrder/Create
        public ActionResult Create()
        {
            GetNewGiftVoucherDocumentNo();
            return View();
        }

        // POST: GiftVoucherPurchaseOrder/Create
        [HttpPost]
        public ActionResult Create(Models.Transactions.InvGiftVoucherPurchaseOrderHeader GVPurchaseOrderHead)

        {
            try
            {

                var ff = SaveGiftVoucherPO(GVPurchaseOrderHead);

                if (ff == 0)
                {
                    ViewBag.Message = "0";
                }
                //else if (ff[0].BlockedUnitID == 4)
                //{
                //    ViewBag.Message = "4";
                //}
                else
                {
                    ViewBag.Message = "3";
                }
                // TODO: Add insert logic here
                return View();
            }
            catch
            {
                return View();
            }
        }

        // GET: GiftVoucherPurchaseOrder/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: GiftVoucherPurchaseOrder/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: GiftVoucherPurchaseOrder/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: GiftVoucherPurchaseOrder/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public JsonResult GetGiftvoucherSupplier()
        {
            //List<InvGiftVoucherBookCode> giftvoucherBookTest = _billGiftVPO.SpgetBookcode().ToList();

            //  LocationService reporsitory = new LocationService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<Supplier> giftvoucherPO = _billGiftVPO.GetSuppliers(companyid).ToList();
            //giftvoucherPO = giftvouchergroupss[0].GiftVoucherGroupCode;
            return Json(JsonConvert.SerializeObject(giftvoucherPO, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetPaymentTerm()
        {
            //  LocationService reporsitory = new LocationService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<PaymentTerm> giftvoucherTerm = _billGiftVPO.GetPaymentTerm(companyid).ToList();
            return Json(JsonConvert.SerializeObject(giftvoucherTerm, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetLocationAll()
        {
            //  LocationService reporsitory = new LocationService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<SysLocation> LocationA = _billGiftVPO.GetAllLocation().ToList();
            return Json(JsonConvert.SerializeObject(LocationA, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
        public ActionResult HMSGVPOLoad(Models.Transactions.InvGiftVoucherPurchaseOrderHeader GVPurchaseOrderHead)
        {
            int y = 0;
            int GVQTy = 0;
            decimal GVPercentage = 0;
            decimal GVTotal = 0;
            decimal GVNetAmount = 0;
            decimal GVGrossAmount = 0;
            decimal GVDiscountAmount = 0;
            decimal GVAmount = 0;

            GVPercentage = GVPurchaseOrderHead.DiscountPercentage / 100;
            List<InvGiftVoucherMaster> giftvoucherMasterBook = new List<InvGiftVoucherMaster>();
            //LoadGiftVouchers(GVPurchaseOrderHead);
            try
            {

                List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetInvGiftVoucherBookcodeByCode(GVPurchaseOrderHead.BookCode).ToList();


                //List<InvGiftVoucherBookCode> giftvoucherBookTest = _billGiftVPO.SpgetBookcode().ToList();

                existingInvGiftVoucherMaster = _billGiftVoucherGroup.GetInvGiftVoucherMasterByBookID(giftvoucherBook[0].InvGiftVoucherBookCodeID);

                if (giftvoucherBook != null && existingInvGiftVoucherMaster != null)
                {


                    giftvoucherMasterBook = _billGiftVoucherGroup.GetInvGiftVoucherMasterByBookCodeID(giftvoucherBook[0].InvGiftVoucherBookCodeID, GVPurchaseOrderHead.GiftVoucherQty).ToList();

                    //return View("~/Views/GiftVoucherPurchaseOrder/Create.cshtml", giftvoucherMasterBook);
                    //if (giftvoucherMasterBook[0].PageCount != 0)
                    if (giftvoucherMasterBook.Count != 0)
                    {
                        for (y = 0; y < giftvoucherMasterBook.Count; y++)
                        {
                            GVTotal += giftvoucherMasterBook[y].GiftVoucherValue;
                            GVGrossAmount = GVTotal;
                        }
                        GVNetAmount = GVTotal - (GVTotal * GVPercentage);
                        GVDiscountAmount = GVTotal * GVPercentage;
                    }


                    ViewBag.GiftVoucherAmount = giftvoucherMasterBook[0].GiftVoucherValue;
                    ViewBag.ItemList = giftvoucherMasterBook;
                    GVPurchaseOrderHead.NetAmount = GVNetAmount;
                    ViewBag.NetAmo = GVNetAmount;
                    ViewBag.GVGrossAmount = GVGrossAmount;
                    ViewBag.DiscountAmount = GVDiscountAmount;
                }
            }
            catch (Exception ex)
            {

            }
            return View("~/Views/GiftVoucherPurchaseOrder/Create.cshtml", GVPurchaseOrderHead);
            //return View("~/Views/GiftVoucherPurchaseOrder/Create.cshtml");

        }

        private int SaveGiftVoucherPO(Models.Transactions.InvGiftVoucherPurchaseOrderHeader GVPurchaseOrderHead)
        {
            int resu = 0;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            InvGiftVoucherPurchaseOrderHeader invGiftVoucherPurchaseOrderHeader = new InvGiftVoucherPurchaseOrderHeader();
            InvGiftVoucherPurchaseOrderHeader invGiftVoucherPOHResult = new InvGiftVoucherPurchaseOrderHeader();
            InvGiftVoucherPurchaseOrderDetail invGiftVoucherPurchaseOrderDetails = new InvGiftVoucherPurchaseOrderDetail();
            List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetInvGiftVoucherBookcodeByCode(GVPurchaseOrderHead.BookCodehidden).ToList();

            string GetGVDocNo_ = "";
            List<InvGiftVoucherDocumentNumber> resDocNo = _billGiftVPO.GetnewVoucherDocNo(companyid).ToList();

            if (resDocNo != null && resDocNo.Count > 0)
            {
                var GetGVDocNo = resDocNo[0].DocumentNo;
                GetGVDocNo_ = GetGVDocNo;

                // Extract the prefix and numerical portion of the code
                string prefix = GetGVDocNo.Substring(0, 1); // "G"
                string numericalPart = GetGVDocNo.Substring(1); // "0011"
                int numericalValue = int.Parse(numericalPart);
                numericalValue++;
                // Format the numerical value back into the desired format
                GetGVDocNo_ = prefix + numericalValue.ToString("D11"); // "G0012"

            }
            else
            {
                GetGVDocNo_ = "T" + "00000000001";
            }
            ViewBag.GVDocumentNo = GetGVDocNo_;


            try
            {
                // invGiftVoucherPurchaseOrderHeader = _billGiftVPO.CheckPOHeader(GVPurchaseOrderHead.DocumentID);

                // if (invGiftVoucherPurchaseOrderHeader == null)
                // {
                invGiftVoucherPurchaseOrderHeader.CompanyID = companyid;
                invGiftVoucherPurchaseOrderHeader.LocationId = GVPurchaseOrderHead.LocationAhidden;
                //costcenterID
                invGiftVoucherPurchaseOrderHeader.DocumentID = GVPurchaseOrderHead.DocumentID;
                //invGiftVoucherPurchaseOrderHeader.DocumentNo = GVPurchaseOrderHead.DocumentNo;
                invGiftVoucherPurchaseOrderHeader.DocumentNo = GetGVDocNo_;
                invGiftVoucherPurchaseOrderHeader.DocumentDate = GVPurchaseOrderHead.DocumentDate;
                //invGiftVoucherPurchaseOrderHeader.DocumentDate = DateTime.Now;
                invGiftVoucherPurchaseOrderHeader.SupplierID = GVPurchaseOrderHead.SupplierIDhidden;
                invGiftVoucherPurchaseOrderHeader.GiftVoucherAmount = GVPurchaseOrderHead.GiftVoucherAmount;
                invGiftVoucherPurchaseOrderHeader.GiftVoucherPercentage = GVPurchaseOrderHead.GiftVoucherPercentage;
                invGiftVoucherPurchaseOrderHeader.GrossAmount = GVPurchaseOrderHead.GrossAmount;
                invGiftVoucherPurchaseOrderHeader.DiscountAmount = GVPurchaseOrderHead.DiscountAmount;
                invGiftVoucherPurchaseOrderHeader.DiscountPercentage = GVPurchaseOrderHead.DiscountPercentage;
                invGiftVoucherPurchaseOrderHeader.OtherCharges = GVPurchaseOrderHead.OtherCharges;
                invGiftVoucherPurchaseOrderHeader.TaxAmount1 = GVPurchaseOrderHead.TaxAmount1;
                invGiftVoucherPurchaseOrderHeader.TaxAmount2 = GVPurchaseOrderHead.TaxAmount2;
                invGiftVoucherPurchaseOrderHeader.TaxAmount3 = GVPurchaseOrderHead.TaxAmount3;
                invGiftVoucherPurchaseOrderHeader.TaxAmount4 = GVPurchaseOrderHead.TaxAmount4;
                invGiftVoucherPurchaseOrderHeader.TaxAmount5 = GVPurchaseOrderHead.TaxAmount5;
                invGiftVoucherPurchaseOrderHeader.TaxAmount = GVPurchaseOrderHead.TaxAmount;
                invGiftVoucherPurchaseOrderHeader.NetAmount = GVPurchaseOrderHead.NetAmount;
                invGiftVoucherPurchaseOrderHeader.CreditLimit = GVPurchaseOrderHead.CreditLimit;
                invGiftVoucherPurchaseOrderHeader.CreditPeriod = GVPurchaseOrderHead.CreditPeriod;
                invGiftVoucherPurchaseOrderHeader.ChequeLimit = GVPurchaseOrderHead.ChequeLimit;
                invGiftVoucherPurchaseOrderHeader.ChequePeriod = GVPurchaseOrderHead.ChequePeriod;
                invGiftVoucherPurchaseOrderHeader.GiftVoucherQty = GVPurchaseOrderHead.GiftVoucherQty;
                invGiftVoucherPurchaseOrderHeader.PaymentTermID = GVPurchaseOrderHead.PaymentTermhidden;
                invGiftVoucherPurchaseOrderHeader.VoucherType = GVPurchaseOrderHead.VoucherType;
                invGiftVoucherPurchaseOrderHeader.DocumentStatus = GVPurchaseOrderHead.DocumentStatus;
                invGiftVoucherPurchaseOrderHeader.GroupOfCompanyID = GVPurchaseOrderHead.GroupOfCompanyID;
                invGiftVoucherPurchaseOrderHeader.CreatedUser = GVPurchaseOrderHead.CreatedUser;
                //invGiftVoucherPurchaseOrderHeader.CreatedDate = GVPurchaseOrderHead.CreatedDate;
                invGiftVoucherPurchaseOrderHeader.CreatedDate = DateTime.Now;
                invGiftVoucherPurchaseOrderHeader.ModifiedUser = GVPurchaseOrderHead.ModifiedUser;
                //invGiftVoucherPurchaseOrderHeader.ModifiedDate = GVPurchaseOrderHead.ModifiedDate;
                invGiftVoucherPurchaseOrderHeader.ModifiedDate = DateTime.Now;
                invGiftVoucherPurchaseOrderHeader.DataTransfer = GVPurchaseOrderHead.DataTransfer;
                invGiftVoucherPurchaseOrderHeader.ExpectedDate = GVPurchaseOrderHead.ExpectedDate;
                //invGiftVoucherPurchaseOrderHeader.ExpectedDate = DateTime.Now;
                //invGiftVoucherPurchaseOrderHeader.ExpiryDate = GVPurchaseOrderHead.ExpiryDate;
                invGiftVoucherPurchaseOrderHeader.ExpiryDate = DateTime.Now;
                invGiftVoucherPurchaseOrderHeader.PaymentPeriod = GVPurchaseOrderHead.PaymentPeriod;
                invGiftVoucherPurchaseOrderHeader.GiftVoucherPurchaseOrderHeaderID = 1;
                invGiftVoucherPurchaseOrderHeader.Remark = GVPurchaseOrderHead.Remark;
                invGiftVoucherPurchaseOrderHeader.ReferenceNo = GVPurchaseOrderHead.ReferenceNo;

                int res1 = 0;
                if(GVPurchaseOrderHead.NetAmount !=0)
                {
                    var res = _billGiftVPO.SaveGiftVoucherPOH(invGiftVoucherPurchaseOrderHeader);
                    res1 = res;
                }
                else
                {
                    res1 = 0;
                }
                

                if (res1 != 0)
                {
                    ViewBag.Message = "3";

                }
                else
                {//fail
                    ViewBag.Message = "0";
                }

                invGiftVoucherPOHResult = _billGiftVPO.GetPurchaseOrderHeaderID();

                //var savedetails = _billGiftVPO.SaveGiftVoucherDetails();
                List<InvGiftVoucherMaster> giftvoucherMasterBook = new List<InvGiftVoucherMaster>();
                //}(int giftVoucherbookcode,int giftvoucherQTY)
                giftvoucherMasterBook = _billGiftVoucherGroup.GetInvGiftVoucherMasterByBookCodeID(giftvoucherBook[0].InvGiftVoucherBookCodeID, GVPurchaseOrderHead.GiftVoucherQty).ToList();
                if (giftvoucherMasterBook.Count != 0)
                {
                    for (int y = 0; y < giftvoucherMasterBook.Count; y++)
                    {
                        //invGiftVoucherPurchaseOrderDetails.InvGiftVoucherPurchaseOrderDetailID = 0;
                        invGiftVoucherPurchaseOrderDetails.GiftVoucherPurchaseOrderDetailID = 0;
                        invGiftVoucherPurchaseOrderDetails.InvGiftVoucherPurchaseOrderHeaderID = invGiftVoucherPOHResult.InvGiftVoucherPurchaseOrderHeaderID;
                        invGiftVoucherPurchaseOrderDetails.CompanyID = invGiftVoucherPOHResult.CompanyID;
                        invGiftVoucherPurchaseOrderDetails.LocationId = invGiftVoucherPOHResult.LocationId;
                        invGiftVoucherPurchaseOrderDetails.DocumentID = invGiftVoucherPOHResult.DocumentID;
                        invGiftVoucherPurchaseOrderDetails.DocumentDate = invGiftVoucherPOHResult.DocumentDate;
                        invGiftVoucherPurchaseOrderDetails.LineNo = y+1;
                        invGiftVoucherPurchaseOrderDetails.InvGiftVoucherMasterID = giftvoucherMasterBook[y].InvGiftVoucherMasterID;
                        invGiftVoucherPurchaseOrderDetails.NumberOfCount = GVPurchaseOrderHead.GiftVoucherQty;
                        invGiftVoucherPurchaseOrderDetails.VoucherAmount = GVPurchaseOrderHead.GiftVoucherAmount;
                        invGiftVoucherPurchaseOrderDetails.VoucherType = GVPurchaseOrderHead.VoucherType;
                        invGiftVoucherPurchaseOrderDetails.IsPurchase = false;
                        invGiftVoucherPurchaseOrderDetails.DocumentStatus = GVPurchaseOrderHead.DocumentStatus;
                        invGiftVoucherPurchaseOrderDetails.GroupOfCompanyID = GVPurchaseOrderHead.GroupOfCompanyID;
                        invGiftVoucherPurchaseOrderDetails.CreatedUser = GVPurchaseOrderHead.CreatedUser;
                        invGiftVoucherPurchaseOrderDetails.CreatedDate = GVPurchaseOrderHead.CreatedDate;
                        invGiftVoucherPurchaseOrderDetails.ModifiedUser = GVPurchaseOrderHead.ModifiedUser;
                        invGiftVoucherPurchaseOrderDetails.ModifiedDate = GVPurchaseOrderHead.ModifiedDate;
                        invGiftVoucherPurchaseOrderDetails.DataTransfer = GVPurchaseOrderHead.DataTransfer;
                        var DetailsSaveresult = _billGiftVPO.SaveGiftVoucherDetails(invGiftVoucherPurchaseOrderDetails);
                        //if(_billGiftVPO.UpdateGVMaster(giftvoucherMasterBook)==1)
                        //{
                        //    string jvj = "";
                        //    // if (_bllPaymentTerms.UpdatePaymentMethod(paymentterm) == 1)
                        //}
                    }
                }
                
                InvGiftVoucherDocumentNumber giftvoucherDocNo = new InvGiftVoucherDocumentNumber();
                giftvoucherDocNo.DocumentId = 3;
                giftvoucherDocNo.DocumentName = "GiftVoucherPO";
                giftvoucherDocNo.DocumentNo = GetGVDocNo_;
                giftvoucherDocNo.GroupOfCompanyID = GVPurchaseOrderHead.GroupOfCompanyID;
                giftvoucherDocNo.CompanyID = companyid;
                giftvoucherDocNo.LocationId= GVPurchaseOrderHead.LocationAhidden;

                var SerultGVDocNo = _billGiftVPO.SaveGiftVoucherDocNo(giftvoucherDocNo);

                

            }
            catch (Exception ex)
            {
                return 0;
            }
            return 1;

        }
        public ActionResult GetNewGiftVoucherDocumentNo()
        {
            string GetGVDocNo_ = "";
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<InvGiftVoucherDocumentNumber> res = _billGiftVPO.GetnewVoucherDocNo(companyid).ToList();

            if (res != null && res.Count > 0)
            {
                var GetGVDocNo = res[0].DocumentNo;
                GetGVDocNo_ = GetGVDocNo;

                // Extract the prefix and numerical portion of the code
                string prefix = GetGVDocNo.Substring(0, 1); // "G"
                string numericalPart = GetGVDocNo.Substring(1); // "0011"
                int numericalValue = int.Parse(numericalPart);
                numericalValue++;
                // Format the numerical value back into the desired format
                GetGVDocNo_ = prefix + numericalValue.ToString("D11"); // "G0012"

            }
            else
            {
                GetGVDocNo_ = "T" + "00000000001";
            }
            ViewBag.GVDocumentNo = GetGVDocNo_;
            return View("~/Views/GiftVoucherPurchaseOrder/Create.cshtml");
        }

        private void LoadGiftVouchers(Models.Transactions.InvGiftVoucherPurchaseOrderHeader GVPurchaseOrderHeada)
        {
            //try
            //{
            //    List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetInvGiftVoucherBookcodeByCode(GVPurchaseOrderHeada.BookCode).ToList();

            //    existingInvGiftVoucherMaster = _billGiftVoucherGroup.GetInvGiftVoucherMasterByBookID(giftvoucherBook[0].InvGiftVoucherBookCodeID);

            //    if(giftvoucherBook!=null && existingInvGiftVoucherMaster!=null)
            //    {


            //             List<InvGiftVoucherMaster> giftvoucherMasterBook = _billGiftVoucherGroup.GetInvGiftVoucherMasterByBookCodeID(giftvoucherBook[0].InvGiftVoucherBookCodeID, GVPurchaseOrderHeada.GiftVoucherQty).ToList();
            //    }
            //}
            //catch (Exception ex)
            //{

            //}

            //if (cmbSelectionCriteria.SelectedIndex.Equals(0))
            //{
            //    if (invGiftVoucherMasterService.GetInvGiftVoucherMasterByBookID(invGiftVoucherBookCodeGenerationService.GetInvGiftVoucherMasterBookByCode(txtBookCode.Text.Trim()).InvGiftVoucherBookCodeID) != null)
            //    {
            //        Common.SetAutoComplete(txtVoucherNo, invGiftVoucherMasterService.GetAllVoucherNosByBookIDForQty(invGiftVoucherBookCodeGenerationService.GetInvGiftVoucherMasterBookByCode(txtBookCode.Text.Trim()).InvGiftVoucherBookCodeID, Common.ConvertStringToInt(txtGiftVoucherQty.Text.Trim())), chkAutoCompleationVoucher.Checked);
            //        Common.SetAutoComplete(txtVoucherSerial, invGiftVoucherMasterService.GetAllVoucherSerialsByBookIDForQty(invGiftVoucherBookCodeGenerationService.GetInvGiftVoucherMasterBookByCode(txtBookCode.Text.Trim()).InvGiftVoucherBookCodeID, Common.ConvertStringToInt(txtGiftVoucherQty.Text.Trim())), chkAutoCompleationVoucher.Checked);
            //    }
            //}
        }
    }
}
