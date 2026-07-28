using HospitalityManagement.Models.Transactions;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    public class GiftVoucherTransferController : Controller
    {
        BLL_GiftVoucherGoodReceiveNote _billGiftVoucherGoodReceiveNote;
        BLL_Location _blllocation;
        BLL_GiftVoucherGroup _billGiftVoucherGroup;
        BLL_GiftVoucherPO _billGiftVPO;
        private string giftvouchergroupcode = string.Empty;
        private string giftvoucherGRN = string.Empty;
        BLL_GiftVoucherGoodReceiveNote _billGiftVGRN;
        BLL_GiftVoucherTransfer _billGiftVTransfer;
        public GiftVoucherTransferController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _billGiftVoucherGoodReceiveNote = new BLL_GiftVoucherGoodReceiveNote(cn);
            _blllocation = new BLL_Location(cn);
            _billGiftVoucherGroup = new BLL_GiftVoucherGroup(cn);
            _billGiftVPO = new BLL_GiftVoucherPO(cn);
            _billGiftVGRN=new BLL_GiftVoucherGoodReceiveNote(cn);
            _billGiftVTransfer= new BLL_GiftVoucherTransfer(cn);
            
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Details(int id)
        {
            return View();
        }
        public ActionResult Create()
        {
            string GetGVDocNo_ = "";
            HospitalityManagement.Models.GiftVoucherTransfer invGiftVoucherTransfer = new HospitalityManagement.Models.GiftVoucherTransfer();
            List<InvGiftVoucherDocumentNumber> res = _billGiftVPO.GetnewVoucherDocNo("GVT").ToList();

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
            invGiftVoucherTransfer.DocumentNo = GetGVDocNo_;
            return View(invGiftVoucherTransfer);
        }
        
        [Authorize(Roles = "CusView")]
        public ActionResult ViewBookCode()
        {

            int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //var exists = _billGiftVoucherBookCode.GetGiftVoucherBookCode();
            //return View("~/Views/GiftVoucherBookCode/ViewVoucherBook.cshtml", exists);
            return View();
        }
        public ActionResult Edit(int id)
        {
            return View();
        }
        public ActionResult SaveDetails(Models.GiftVoucherTransfer GVoucherTransfer)

        {
            try
            {

                var ff = SaveGiftVoucherTransfer(GVoucherTransfer);

                if (ff == 0)
                {
                    ViewBag.Message = "0";
                }
                else
                {
                    ViewBag.Message = "3";
                }
                // TODO: Add insert logic here
                return View("~/Views/GiftVoucherTransfer/Create.cshtml");
            }
            catch
            {
                return View("~/Views/GiftVoucherTransfer/Create.cshtml");
            }
        }
        [HttpGet]
        public JsonResult GetViewGRNDetails(long DocumentNO)
        {
            int POHNO = 0;
            int GiftVoucherMasterID = 0;
            string GetGVDocNo_ = "";
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<Models.GiftVoucherGoodReceiveNote> itemList = new List<Models.GiftVoucherGoodReceiveNote>();
            try
            {
                List<RIT.HMS.Domain.Transactions.invGiftVoucherPurchaseHeaders> giftvoucherPOH = _billGiftVGRN.GetPurchaseHeaderIDNew(DocumentNO).ToList(); ; //invGVMaster.PurchaseOrderNo).ToList();
                POHNO = Convert.ToInt32(giftvoucherPOH[0].InvGiftVoucherPurchaseHeaderID);
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseDetails> giftvoucherPOD = _billGiftVGRN.GetPODetails(POHNO).ToList();
                RIT.HMS.Domain.Transactions.InvGiftVoucherMaster DBOInvGiftvoucherMaster = new RIT.HMS.Domain.Transactions.InvGiftVoucherMaster();
                //Gift Voucher Master Id
                GiftVoucherMasterID = Convert.ToInt32(giftvoucherPOD[0].InvGiftVoucherMasterID);
                //Get Book Code
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> GiftVoucherMaster = _billGiftVPO.GetGVMasterDetails(GiftVoucherMasterID).ToList();
                List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetgvBookDetailsByID(companyid, Convert.ToInt32(GiftVoucherMaster[0].InvGiftVoucherBookCodeID)).ToList();
                InvGiftVoucherBookCodeA objInvGiftVoucherBookCodeA = new InvGiftVoucherBookCodeA();
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherGroup> GiftVoucherGroup = _billGiftVoucherGroup.GetGroupCodeName(GiftVoucherMaster[0].InvGiftVoucherGroupID).ToList();
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
                //InvGiftVoucherGroup
                //GetGroupCodeName

                objInvGiftVoucherBookCodeA.giftvoucherBook = giftvoucherBook;
                objInvGiftVoucherBookCodeA.GiftVoucherGroupCode = GiftVoucherGroup[0].GiftVoucherGroupCode;
                if (giftvoucherBook != null && giftvoucherBook.Count > 0)
                {
                    string prefix = giftvoucherBook[0].BookPrefix;
                    int length = giftvoucherBook[0].SerialLength;
                    int startingNo = giftvoucherBook[0].StartingNo;
                    objInvGiftVoucherBookCodeA.SerialFormat = GetBookCodeFormat(prefix, length, startingNo);
                }
                return Json(JsonConvert.SerializeObject(objInvGiftVoucherBookCodeA, Formatting.None, new JsonSerializerSettings
                { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetBookCodeFormat(string prefix, int length, int pageNo)
        {
            string bookFormat = "";
            if (prefix != null && length > 0)
            {
                if (length > 0 && prefix.Length > 0)
                {
                    length = (length - prefix.Length);
                }
                if (!string.IsNullOrEmpty(length.ToString()))
                {
                    bookFormat = String.Format("{0}{1," + length + ":D" + length + "} ", prefix, pageNo);
                }
            }
            return bookFormat;
        }
        private int SaveGiftVoucherTransfer(Models.GiftVoucherTransfer GVTransferNote)
        {
            int res1 = 0;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            RIT.HMS.Domain.Transactions.InvGiftVoucherTransferNoteHeader invGiftVoucherTransfer = new RIT.HMS.Domain.Transactions.InvGiftVoucherTransferNoteHeader();
            RIT.HMS.Domain.Transactions.InvGiftVoucherTransferNoteHeader invGiftVoucherTransferResult = new RIT.HMS.Domain.Transactions.InvGiftVoucherTransferNoteHeader();
            RIT.HMS.Domain.Transactions.InvGiftVoucherTransferNoteDetail invGiftVoucherTransferDetails = new RIT.HMS.Domain.Transactions.InvGiftVoucherTransferNoteDetail();
            List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetInvGiftVoucherBookcodeByCode(GVTransferNote.BookCode).ToList();

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
                invGiftVoucherTransfer.CompanyID = companyid;
               // invGiftVoucherTransfer.LocationID = GVTransferNote.LocationAhidden;
                invGiftVoucherTransfer.DocumentID = GVTransferNote.DocumentID;
                invGiftVoucherTransfer.DocumentNo = GetGVDocNo_;
                invGiftVoucherTransfer.DocumentDate = DateTime.Now;
               // invGiftVoucherTransfer.SupplierID = GVTransferNote.SupplierID;
                invGiftVoucherTransfer.GiftVoucherAmount = GVTransferNote.GiftVoucherValue;
                invGiftVoucherTransfer.GiftVoucherPercentage = GVTransferNote.GiftVoucherPercentage;
               // invGiftVoucherTransfer.GrossAmount = GVTransferNote.GrossAmount;
               // invGiftVoucherTransfer.DiscountAmount = GVTransferNote.DiscountAmount;
               // invGiftVoucherTransfer.DiscountPercentage = GVTransferNote.DiscountPercentage;
               // invGiftVoucherTransfer.OtherCharges = GVTransferNote.OtherCharges;
               // invGiftVoucherTransfer.TaxAmount1 = GVTransferNote.TaxAmount1;
               // invGiftVoucherTransfer.TaxAmount2 = GVTransferNote.TaxAmount2;
               // invGiftVoucherTransfer.TaxAmount3 = GVTransferNote.TaxAmount3;
               // invGiftVoucherTransfer.TaxAmount4 = GVTransferNote.TaxAmount4;
               // invGiftVoucherTransfer.TaxAmount5 = GVTransferNote.TaxAmount5;
               // invGiftVoucherTransfer.TaxAmount = GVTransferNote.TaxAmount;
               // invGiftVoucherTransfer.CreditLimit = GVTransferNote.CreditLimit;
               // invGiftVoucherTransfer.CreditPeriod = GVTransferNote.CreditPeriod;
               // invGiftVoucherTransfer.ChequeLimit = GVTransferNote.ChequeLimit;
               // invGiftVoucherTransfer.ChequePeriod = GVTransferNote.ChequePeriod;
              //  invGiftVoucherTransfer.GiftVoucherQty = GVTransferNote.GiftVoucherQty;
              //  invGiftVoucherTransfer.PaymentTermID = GVTransferNote.PaymentTermhidden;
                invGiftVoucherTransfer.VoucherType = GVTransferNote.VoucherType;
               // invGiftVoucherTransfer.DocumentStatus = GVTransferNote.DocumentStatus;
                invGiftVoucherTransfer.GroupOfCompanyID = GVTransferNote.GroupOfCompanyID;
                invGiftVoucherTransfer.CreatedUser = GVTransferNote.CreatedUser;
                invGiftVoucherTransfer.CreatedDate = DateTime.Now;
                invGiftVoucherTransfer.ModifiedUser = GVTransferNote.ModifiedUser;
                invGiftVoucherTransfer.ModifiedDate = DateTime.Now;
               // invGiftVoucherTransfer.PartyInvoiceDate = DateTime.Now;
                invGiftVoucherTransfer.DataTransfer = 1;
               // invGiftVoucherTransfer.PaymentPeriod = GVTransferNote.PaymentPeriod;
               // invGiftVoucherTransfer.DispatchDate = DateTime.Now;
               // invGiftVoucherTransfer.GiftVoucherPurchaseHeaderID = 1;
               // invGiftVoucherTransfer.Remark = GVTransferNote.Remark;
               // invGiftVoucherTransfer.ReferenceNo = GVTransferNote.ReferenceNo;
                if (GVTransferNote.GiftVoucherValue != 0)
                {
                    var res = _billGiftVTransfer.SaveGiftVoucherTransfer(invGiftVoucherTransfer);
                    res1 = res;
                }
                else
                {
                    res1 = 0;
                }
                if (res1 != 0)
                {
                    ViewBag.Message = "3";
                    invGiftVoucherTransferResult = _billGiftVTransfer.GetGiftvoucherTranferHeaderID();
                    List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> giftvoucherMasterBook = new List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster>();
                    giftvoucherMasterBook = _billGiftVoucherGroup.GetInvGiftVoucherMasterByBookCodeID(giftvoucherBook[0].InvGiftVoucherBookCodeID, GVTransferNote.PageCount).ToList();
                    if (giftvoucherMasterBook.Count != 0)
                    {
                        for (int y = 0; y < giftvoucherMasterBook.Count; y++)
                        {
                          //  invGiftVoucherTransferDetails.InvGiftVoucherPurchaseDetailID = 0;
                          //  invGiftVoucherTransferDetails.invGiftVoucherTransferID = invGiftVoucherTransferResult.invGiftVoucherTransferID;
                            // invGiftVoucherTransferDetails.CompanyID = invGiftVoucherTransferResult.CompanyID;
                            // invGiftVoucherTransferDetails.LocationId = invGiftVoucherTransferResult.LocationId;
                            invGiftVoucherTransferDetails.DocumentID = invGiftVoucherTransferResult.DocumentID;
                            invGiftVoucherTransferDetails.DocumentDate = invGiftVoucherTransferResult.DocumentDate;
                            invGiftVoucherTransferDetails.LineNo = y + 1;
                          //  invGiftVoucherTransferDetails.InvGiftVoucherMasterID = giftvoucherMasterBook[y].InvGiftVoucherMasterID;
                          //  invGiftVoucherTransferDetails.NumberOfCount = GVTransferNote.GiftVoucherQty;
                          //  invGiftVoucherTransferDetails.VoucherAmount = GVTransferNote.GiftVoucherAmount;
                            invGiftVoucherTransferDetails.VoucherType = GVTransferNote.VoucherType;
                            // invGiftVoucherTransferDetails.IsPurchase = false;
                          //  invGiftVoucherTransferDetails.DocumentStatus = GVTransferNote.DocumentStatus;
                            invGiftVoucherTransferDetails.GroupOfCompanyID = GVTransferNote.GroupOfCompanyID;
                            invGiftVoucherTransferDetails.CreatedUser = GVTransferNote.CreatedUser;
                            invGiftVoucherTransferDetails.CreatedDate = DateTime.Now;
                            invGiftVoucherTransferDetails.ModifiedUser = GVTransferNote.ModifiedUser;
                            invGiftVoucherTransferDetails.ModifiedDate = DateTime.Now;
                            // invGiftVoucherTransferDetails.DataTransfer = GVTransferNote.DataTransfer;
                            var DetailsSaveresult = _billGiftVTransfer.SaveGiftVoucherTransferDetails(invGiftVoucherTransferDetails);
                        }
                    }
                    InvGiftVoucherDocumentNumber giftvoucherDocNo = new InvGiftVoucherDocumentNumber();
                    giftvoucherDocNo.DocumentId = 3;
                    giftvoucherDocNo.DocumentName = "GiftVoucherPO";
                    giftvoucherDocNo.DocumentNo = GetGVDocNo_;
                    giftvoucherDocNo.GroupOfCompanyID = GVTransferNote.GroupOfCompanyID;
                    giftvoucherDocNo.CompanyID = companyid;
                   // giftvoucherDocNo.LocationId = GVTransferNote.LocationAhidden;
                    var SerultGVDocNo = _billGiftVPO.SaveGiftVoucherDocNo(giftvoucherDocNo);
                }
                else
                {//fail
                    ViewBag.Message = "0";
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return res1;

        }
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Delete(int id)
        {
            return View();
        }
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        [HttpGet]
        public JsonResult GetGiftvoucherGroup()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<InvGiftVoucherGroup> giftvouchergroupss = _billGiftVoucherGroup.GetGroups(companyid).ToList();
            giftvouchergroupcode = giftvouchergroupss[0].GiftVoucherGroupCode;
            return Json(JsonConvert.SerializeObject(giftvouchergroupss, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetGiftvoucherGRN()
        {
            //BLL_GiftVoucherGoodReceiveNote _billGiftVGRN;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<invGiftVoucherPurchaseHeaders> giftvoucherPurchaseHeader = _billGiftVGRN.GetGiftVoucherPH().ToList();
            giftvoucherGRN = giftvoucherPurchaseHeader[0].DocumentNo;
            return Json(JsonConvert.SerializeObject(giftvoucherPurchaseHeader, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }    
    }
}
