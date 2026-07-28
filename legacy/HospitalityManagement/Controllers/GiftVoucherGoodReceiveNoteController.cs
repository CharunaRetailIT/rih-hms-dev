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
    public class GiftVoucherGoodReceiveNoteController : Controller
    {
        BLL_GiftVoucherGoodReceiveNote _billGiftVoucherGoodReceiveNote;
        BLL_Location _blllocation;
        BLL_GiftVoucherGroup _billGiftVoucherGroup;
        BLL_GiftVoucherPO _GiftVoucherPO;
        BLL_GiftVoucherGroup _bllGVGroup;
        private InvGiftVoucherGroup existingInvGiftVoucherGroup;
        BLL_GiftVoucherPO _billGiftVoucherPO;
        private string giftvouchergroupcode = string.Empty;
        private string PurchaseOrderID = string.Empty;
        BLL_GiftVoucherPO _billGiftVPO;
        BLL_GiftVoucherGoodReceiveNote _billGiftVGRN;
        public GiftVoucherGoodReceiveNoteController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _billGiftVoucherGoodReceiveNote = new BLL_GiftVoucherGoodReceiveNote(cn);
            _blllocation = new BLL_Location(cn);
            _billGiftVoucherGroup = new BLL_GiftVoucherGroup(cn);
            _billGiftVPO = new BLL_GiftVoucherPO(cn);
            _billGiftVGRN = new BLL_GiftVoucherGoodReceiveNote(cn);
            _billGiftVoucherPO = new BLL_GiftVoucherPO(cn);
            _GiftVoucherPO = new BLL_GiftVoucherPO(cn);
            _bllGVGroup = new BLL_GiftVoucherGroup(cn);
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Details(int id)
        {
            return View();
        }
        // POST: GiftVoucherPurchaseOrder/Create

        public ActionResult SaveDetails(Models.GiftVoucherGoodReceiveNote GVPurchaseOrderHead)

        {
            try
            {

                var ff = SaveGiftVoucherGRN(GVPurchaseOrderHead);

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
                return View("~/Views/GiftVoucherGoodReceiveNote/Create.cshtml");
            }
            catch
            {
                return View("~/Views/GiftVoucherGoodReceiveNote/Create.cshtml");
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
        public ActionResult HMSGVGenarate(Models.GiftVoucherGoodReceiveNote invGVMaster)
        {
            int currentBookNo = 0;
            int StartingNo = invGVMaster.StartingNo;
            int VouNo = invGVMaster.StartingNo;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            int res1 = 0; 
            var ff = GenerateGiftVoucherMaster(invGVMaster);
            if (ff.Count == 0)
            {
                ViewBag.Message = "0";
            }
            else if (ff[0].BlockedUnitID == 4)
            {
                ViewBag.Message = "4";
            }
            else
            {
                res1 = SaveGiftVoucherGRN(invGVMaster);
                ViewBag.Message = "5";
                InvGiftVoucherBookCodeA objInvGiftVoucherBookCodeA = new InvGiftVoucherBookCodeA();
                List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetInvGiftVoucherBookcodeByCode(invGVMaster.BookCode1).ToList();
                objInvGiftVoucherBookCodeA.giftvoucherBook = giftvoucherBook;
                existingInvGiftVoucherGroup = _billGiftVoucherGroup.GetInvGiftVoucherGroupByCode(invGVMaster.GiftVoucherGroupCode);
                List<Models.GiftVoucherGoodReceiveNote> itemList = new List<Models.GiftVoucherGoodReceiveNote>();
                string GetGVDocNo_ = "";
                List<InvGiftVoucherDocumentNumber> resDocNo = _billGiftVPO.GetnewVoucherDocNo(companyid).ToList();
                //if (resDocNo != null && resDocNo.Count > 0)
                //{
                //    var GetGVDocNo = resDocNo[0].DocumentNo;
                //    GetGVDocNo_ = GetGVDocNo;
                //    // Extract the prefix and numerical portion of the code
                //    string prefix = GetGVDocNo.Substring(0, 1); // "G"
                //    string numericalPart = GetGVDocNo.Substring(1); // "0011"
                //    int numericalValue = int.Parse(numericalPart);
                //    numericalValue++;
                //    // Format the numerical value back into the desired format
                //    GetGVDocNo_ = prefix + numericalValue.ToString("D11"); // "G0012"
                //}
                //else
                //{
                //    GetGVDocNo_ = "T" + "00000000001";
                //}
                //ViewBag.GVDocumentNo = GetGVDocNo_;

                currentBookNo = invGVMaster.VoucherSerialNo;

                int y = 0;
                if (invGVMaster.PageCount != 0)
                {
                    for (int x = 1; x <= invGVMaster.PageCount; x = x + y)
                    {
                        for (y = 0; y < invGVMaster.PageCount; y++)
                        {
                            Models.GiftVoucherGoodReceiveNote invGVMaster_ = new Models.GiftVoucherGoodReceiveNote();
                            invGVMaster_.InvGiftVoucherBookCodeID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].InvGiftVoucherBookCodeID;
                            invGVMaster_.InvGiftVoucherGroupID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].InvGiftVoucherGroupID;
                            invGVMaster_.LocationAhidden = objInvGiftVoucherBookCodeA.giftvoucherBook[0].LocationId;
                            invGVMaster_.VoucherNo = GetBookCodeFormat(objInvGiftVoucherBookCodeA.giftvoucherBook[0].BookPrefix, objInvGiftVoucherBookCodeA.giftvoucherBook[0].SerialLength, VouNo);
                            invGVMaster_.VoucherNoSerial = currentBookNo;
                            invGVMaster_.VoucherSerial = GetBookCodeFormat(objInvGiftVoucherBookCodeA.giftvoucherBook[0].BookPrefix, invGVMaster.SerialLength, StartingNo);
                            invGVMaster_.VoucherSerialNo = StartingNo;
                            invGVMaster_.GiftVoucherValue = invGVMaster.GiftVoucherValue;
                            invGVMaster_.GiftVoucherPercentage = invGVMaster.GiftVoucherPercentage;
                            invGVMaster_.VoucherCount = invGVMaster.VoucherCount;
                            invGVMaster_.VoucherPrefix = invGVMaster.VoucherPrefix;
                            invGVMaster_.SerialLength = invGVMaster.SerialLength;
                            invGVMaster_.StartingNo = StartingNo;
                            invGVMaster_.PageCount = invGVMaster.PageCount;
                            invGVMaster_.VoucherType = 1;
                            invGVMaster_.VoucherStatus = 0;
                            invGVMaster_.IsDelete = false;
                            invGVMaster_.ToLocationID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].LocationId;

                            StartingNo = StartingNo + 1;
                            itemList.Add(invGVMaster_);
                        }
                        currentBookNo = currentBookNo + 1;
                    }
                    ViewBag.ItemList = itemList;                  
                    ViewBag.GroupID = invGVMaster.GiftVoucherGroupCode;
                    ViewBag.BookID = invGVMaster.BookCode;
                    invGVMaster.DocumentNo = GetGVDocNo_;
                }
            }
            return View("~/Views/GiftVoucherGoodReceiveNote/Create.cshtml", invGVMaster);
        }
        public ActionResult Create()
        {
            string GetGVDocNo_ = "";
            HospitalityManagement.Models.GiftVoucherGoodReceiveNote invGiftVoucherGoodReceiveNote = new HospitalityManagement.Models.GiftVoucherGoodReceiveNote();
            List<InvGiftVoucherDocumentNumber> res = _billGiftVPO.GetnewVoucherDocNo("GRN").ToList();
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
            invGiftVoucherGoodReceiveNote.DocumentNo = GetGVDocNo_;
            return View(invGiftVoucherGoodReceiveNote);
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
            return View("~/Views/GiftVoucherGoodReceiveNote/Create.cshtml");
        }
        private List<Models.GiftVoucherGoodReceiveNote> GenerateGiftVoucherMaster(Models.GiftVoucherGoodReceiveNote invGVMaster)
        {
            int currentBookNo = 0;
            int StartingNo = invGVMaster.StartingNo;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            int VouNo = invGVMaster.StartingNo;
            InvGiftVoucherBookCodeA objInvGiftVoucherBookCodeA = new InvGiftVoucherBookCodeA();
            List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetInvGiftVoucherBookcodeByCode(invGVMaster.BookCode1).ToList();
            var book = _billGiftVoucherGroup.CheckBookCode(giftvoucherBook[0].InvGiftVoucherBookCodeID);
            objInvGiftVoucherBookCodeA.giftvoucherBook = giftvoucherBook;

            //existingInvGiftVoucherBookCode = _billGiftVoucherGroup.GetInvGiftVoucherBookcodeByCode(invGVMaster.BookCode);
            existingInvGiftVoucherGroup = _billGiftVoucherGroup.GetInvGiftVoucherGroupByCode(invGVMaster.GiftVoucherGroupCode);
            List<Models.GiftVoucherGoodReceiveNote> itemList = new List<Models.GiftVoucherGoodReceiveNote>();
            //InvGiftVoucherMasterTemp itemListtemp = new InvGiftVoucherMasterTemp();
            RIT.HMS.Domain.Transactions.InvGiftVoucherMaster DBOInvGiftvoucherMaster = new RIT.HMS.Domain.Transactions.InvGiftVoucherMaster();
            try
            {
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

                

                if (book == null)
                {
                    currentBookNo = invGVMaster.VoucherSerialNo;
                    int y = 0;
                    if (invGVMaster.PageCount != 0)
                    {
                        for (int x = 1; x <= invGVMaster.PageCount; x = x + y)
                        {
                            for (y = 0; y < invGVMaster.PageCount; y++)
                            {
                                DBOInvGiftvoucherMaster.InvGiftVoucherBookCodeID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].InvGiftVoucherBookCodeID;

                                DBOInvGiftvoucherMaster.InvGiftVoucherBookCodeID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].InvGiftVoucherBookCodeID;
                                DBOInvGiftvoucherMaster.InvGiftVoucherGroupID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].InvGiftVoucherGroupID;
                                DBOInvGiftvoucherMaster.CompanyID = companyid;
                                DBOInvGiftvoucherMaster.LocationId = objInvGiftVoucherBookCodeA.giftvoucherBook[0].LocationId;
                                DBOInvGiftvoucherMaster.VoucherNo = GetBookCodeFormat(objInvGiftVoucherBookCodeA.giftvoucherBook[0].BookPrefix, objInvGiftVoucherBookCodeA.giftvoucherBook[0].SerialLength, VouNo);
                                DBOInvGiftvoucherMaster.VoucherNoSerial = currentBookNo;
                                DBOInvGiftvoucherMaster.VoucherPrefix = giftvoucherBook[0].BookPrefix;
                                DBOInvGiftvoucherMaster.SerialLength = invGVMaster.SerialLength;
                                DBOInvGiftvoucherMaster.GiftVoucherValue = invGVMaster.GiftVoucherValue;
                                DBOInvGiftvoucherMaster.GiftVoucherPercentage = invGVMaster.GiftVoucherPercentage;
                                DBOInvGiftvoucherMaster.StartingNo = StartingNo;
                                DBOInvGiftvoucherMaster.VoucherCount = invGVMaster.VoucherCount;
                                DBOInvGiftvoucherMaster.PageCount = invGVMaster.PageCount;
                                DBOInvGiftvoucherMaster.VoucherSerial = GetBookCodeFormat(invGVMaster.VoucherPrefix, invGVMaster.SerialLength, StartingNo);
                                DBOInvGiftvoucherMaster.VoucherSerialNo = StartingNo;
                                //DBOInvGiftvoucherMaster.VoucherType = 1;
                                DBOInvGiftvoucherMaster.VoucherType = objInvGiftVoucherBookCodeA.giftvoucherBook[0].VoucherType;
                                DBOInvGiftvoucherMaster.VoucherStatus = 0;
                                DBOInvGiftvoucherMaster.ToLocationID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].LocationId;
                                DBOInvGiftvoucherMaster.SoldLocationID = 0;
                                DBOInvGiftvoucherMaster.SoldCashierID = 0;
                                DBOInvGiftvoucherMaster.SoldReceiptNo = "";
                                DBOInvGiftvoucherMaster.SoldUnitID = 0;
                                DBOInvGiftvoucherMaster.SoldZNo = 0;
                                DBOInvGiftvoucherMaster.SoldDate = DateTime.Now;
                                DBOInvGiftvoucherMaster.RedeemedLocationID = 0;
                                DBOInvGiftvoucherMaster.RedeemedCashierID = 0;
                                DBOInvGiftvoucherMaster.RedeemedReceiptNo = "";
                                DBOInvGiftvoucherMaster.RedeemedUnitID = 0;
                                DBOInvGiftvoucherMaster.RedeemedZNo = 0;
                                DBOInvGiftvoucherMaster.RedeemedDate = DateTime.Now;
                                DBOInvGiftvoucherMaster.IsBarcodePrinted = false;
                                DBOInvGiftvoucherMaster.IsDelete = false;
                                DBOInvGiftvoucherMaster.GroupOfCompanyID = companyid;
                                DBOInvGiftvoucherMaster.CreatedUser = Session["loggeduser"].ToString();
                                DBOInvGiftvoucherMaster.CreatedDate = DateTime.Now;
                                DBOInvGiftvoucherMaster.ModifiedUser = Session["loggeduser"].ToString();
                                DBOInvGiftvoucherMaster.ModifiedDate = DateTime.Now;
                                DBOInvGiftvoucherMaster.DataTransfer = 0;
                                DBOInvGiftvoucherMaster.IsTemporaryBlocked = false;
                                DBOInvGiftvoucherMaster.BlockedLocationID = 0;
                                DBOInvGiftvoucherMaster.BlockedCashierID = false;
                                DBOInvGiftvoucherMaster.BlockedUnitID = 0;
                                DBOInvGiftvoucherMaster.BlockedDate = DateTime.Now;

                                StartingNo = StartingNo + 1;

                                var res = _billGiftVoucherGroup.SaveGiftVoucherMaster(DBOInvGiftvoucherMaster);
                            }
                            currentBookNo = currentBookNo + 1;
                        }

                    }

                }
                else
                {
                    if (book.InvGiftVoucherBookCodeID != giftvoucherBook[0].InvGiftVoucherBookCodeID)
                    {
                        currentBookNo = invGVMaster.VoucherSerialNo;
                        int y = 0;
                        if (invGVMaster.PageCount != 0)
                        {
                            for (int x = 1; x <= invGVMaster.PageCount; x = x + y)
                            {
                                for (y = 0; y < invGVMaster.PageCount; y++)
                                {
                                    DBOInvGiftvoucherMaster.InvGiftVoucherBookCodeID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].InvGiftVoucherBookCodeID;
                                    DBOInvGiftvoucherMaster.InvGiftVoucherGroupID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].InvGiftVoucherGroupID;
                                    DBOInvGiftvoucherMaster.CompanyID = companyid;
                                    DBOInvGiftvoucherMaster.LocationId = objInvGiftVoucherBookCodeA.giftvoucherBook[0].LocationId;
                                    DBOInvGiftvoucherMaster.VoucherNo = GetBookCodeFormat(objInvGiftVoucherBookCodeA.giftvoucherBook[0].BookPrefix, objInvGiftVoucherBookCodeA.giftvoucherBook[0].SerialLength, VouNo);
                                    DBOInvGiftvoucherMaster.VoucherNoSerial = currentBookNo;
                                    DBOInvGiftvoucherMaster.VoucherPrefix = giftvoucherBook[0].BookPrefix;
                                    DBOInvGiftvoucherMaster.SerialLength = invGVMaster.SerialLength;
                                    DBOInvGiftvoucherMaster.GiftVoucherValue = invGVMaster.GiftVoucherValue;
                                    DBOInvGiftvoucherMaster.GiftVoucherPercentage = invGVMaster.GiftVoucherPercentage;
                                    DBOInvGiftvoucherMaster.StartingNo = StartingNo;
                                    DBOInvGiftvoucherMaster.VoucherCount = invGVMaster.VoucherCount;
                                    DBOInvGiftvoucherMaster.PageCount = invGVMaster.PageCount;
                                    DBOInvGiftvoucherMaster.VoucherSerial = GetBookCodeFormat(invGVMaster.VoucherPrefix, invGVMaster.SerialLength, StartingNo);
                                    DBOInvGiftvoucherMaster.VoucherSerialNo = StartingNo;
                                    //DBOInvGiftvoucherMaster.VoucherType = 1;
                                    DBOInvGiftvoucherMaster.VoucherType = objInvGiftVoucherBookCodeA.giftvoucherBook[0].VoucherType;
                                    DBOInvGiftvoucherMaster.VoucherStatus = 0;
                                    DBOInvGiftvoucherMaster.ToLocationID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].LocationId;
                                    DBOInvGiftvoucherMaster.SoldLocationID = 0;
                                    DBOInvGiftvoucherMaster.SoldCashierID = 0;
                                    DBOInvGiftvoucherMaster.SoldReceiptNo = "";
                                    DBOInvGiftvoucherMaster.SoldUnitID = 0;
                                    DBOInvGiftvoucherMaster.SoldZNo = 0;
                                    DBOInvGiftvoucherMaster.SoldDate = DateTime.Now;
                                    DBOInvGiftvoucherMaster.RedeemedLocationID = 0;
                                    DBOInvGiftvoucherMaster.RedeemedCashierID = 0;
                                    DBOInvGiftvoucherMaster.RedeemedReceiptNo = "";
                                    DBOInvGiftvoucherMaster.RedeemedUnitID = 0;
                                    DBOInvGiftvoucherMaster.RedeemedZNo = 0;
                                    DBOInvGiftvoucherMaster.RedeemedDate = DateTime.Now;
                                    DBOInvGiftvoucherMaster.IsBarcodePrinted = false;
                                    DBOInvGiftvoucherMaster.IsDelete = false;
                                    DBOInvGiftvoucherMaster.GroupOfCompanyID = companyid;
                                    DBOInvGiftvoucherMaster.CreatedUser = Session["loggeduser"].ToString();
                                    DBOInvGiftvoucherMaster.CreatedDate = DateTime.Now;
                                    DBOInvGiftvoucherMaster.ModifiedUser = Session["loggeduser"].ToString();
                                    DBOInvGiftvoucherMaster.ModifiedDate = DateTime.Now;
                                    DBOInvGiftvoucherMaster.DataTransfer = 0;
                                    DBOInvGiftvoucherMaster.IsTemporaryBlocked = false;
                                    DBOInvGiftvoucherMaster.BlockedLocationID = 0;
                                    DBOInvGiftvoucherMaster.BlockedCashierID = false;
                                    DBOInvGiftvoucherMaster.BlockedUnitID = 0;
                                    DBOInvGiftvoucherMaster.BlockedDate = DateTime.Now;

                                    StartingNo = StartingNo + 1;
                                    var res = _billGiftVoucherGroup.SaveGiftVoucherMaster(DBOInvGiftvoucherMaster);
                                }
                                currentBookNo = currentBookNo + 1;
                            }

                        }
                    }
                    else
                    {
                        invGVMaster.BlockedUnitID = 4;//already exists
                        itemList.Add(invGVMaster);
                    }
                }
                GetNewGiftVoucherDocumentNo();
                itemList.Add(invGVMaster);
                return itemList;
            }
            catch (Exception ex)
            {
                return itemList;
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
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderHeader> giftvoucherPOH = _GiftVoucherPO.GetPOHeadsDetailsbyHeaderID(DocumentNO).ToList(); //invGVMaster.PurchaseOrderNo).ToList();
                POHNO = Convert.ToInt32(giftvoucherPOH[0].InvGiftVoucherPurchaseOrderHeaderID);
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderDetail> giftvoucherPOD = _GiftVoucherPO.GetPODetails(POHNO).ToList();
                RIT.HMS.Domain.Transactions.InvGiftVoucherMaster DBOInvGiftvoucherMaster = new RIT.HMS.Domain.Transactions.InvGiftVoucherMaster();
                //Gift Voucher Master Id
                GiftVoucherMasterID = Convert.ToInt32(giftvoucherPOD[0].InvGiftVoucherMasterID);
                //Get Book Code
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> GiftVoucherMaster = _GiftVoucherPO.GetGVMasterDetails(GiftVoucherMasterID).ToList();               
                List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetgvBookDetailsByID(companyid, Convert.ToInt32(GiftVoucherMaster[0].InvGiftVoucherBookCodeID)).ToList();
                InvGiftVoucherBookCodeA objInvGiftVoucherBookCodeA = new InvGiftVoucherBookCodeA();
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherGroup> GiftVoucherGroup = _bllGVGroup.GetGroupCodeName(GiftVoucherMaster[0].InvGiftVoucherGroupID).ToList();
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
        [HttpGet]
        public JsonResult GetViewGRNVoucherDetails(long DocumentNO)
        {
            int POHNO = 0;
            int GiftVoucherMasterID = 0;
            List<Models.GiftVoucherGoodReceiveNote> itemList = new List<Models.GiftVoucherGoodReceiveNote>();
            try
            {
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderHeader> giftvoucherPOH = _GiftVoucherPO.GetPOHeadsDetails(DocumentNO).ToList(); //invGVMaster.PurchaseOrderNo).ToList();
                POHNO = Convert.ToInt32(giftvoucherPOH[0].InvGiftVoucherPurchaseOrderHeaderID);
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderDetail> giftvoucherPOD = _GiftVoucherPO.GetPODetails(POHNO).ToList();
                RIT.HMS.Domain.Transactions.InvGiftVoucherMaster DBOInvGiftvoucherMaster = new RIT.HMS.Domain.Transactions.InvGiftVoucherMaster();
                //Gift Voucher Master Id
                GiftVoucherMasterID = Convert.ToInt32(giftvoucherPOD[0].InvGiftVoucherMasterID);
                //Get Book Code
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> GiftVoucherMaster = _GiftVoucherPO.GetGVMasterDetails(GiftVoucherMasterID).ToList();
                int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetgvBookDetailsByID(companyid, Convert.ToInt32(GiftVoucherMaster[0].InvGiftVoucherBookCodeID)).ToList();
                InvGiftVoucherBookCodeA objInvGiftVoucherBookCodeA = new InvGiftVoucherBookCodeA();
                objInvGiftVoucherBookCodeA.giftvoucherBook = giftvoucherBook;
                int y = 0;
                if (giftvoucherPOD.Count != 0)
                {
                    for (y = 0; y < giftvoucherPOD.Count; y++)
                    {
                        GiftVoucherMasterID = Convert.ToInt32(giftvoucherPOD[y].InvGiftVoucherMasterID);
                        List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> GiftVoucherMaster1 = _GiftVoucherPO.GetGVMasterDetails(GiftVoucherMasterID).ToList();
                        DBOInvGiftvoucherMaster.VoucherNo = GiftVoucherMaster1[0].VoucherNo;
                        DBOInvGiftvoucherMaster.VoucherSerial = GiftVoucherMaster1[0].VoucherSerial;
                    }
                }
                return Json(JsonConvert.SerializeObject(objInvGiftVoucherBookCodeA, Formatting.None, new JsonSerializerSettings
                { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //ViewGRNDetails
        private List<Models.GiftVoucherGoodReceiveNote> ViewGRN(Models.GiftVoucherGoodReceiveNote invGVMaster)
        {
            string GetGVDocNo_ = "";
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<Models.GiftVoucherGoodReceiveNote> itemList = new List<Models.GiftVoucherGoodReceiveNote>();
            try
            {
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
                int POHNO = 0;
                long a = 3;
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderHeader> giftvoucherPOH = _GiftVoucherPO.GetPOHeadsDetails(a).ToList(); //invGVMaster.PurchaseOrderNo).ToList();
                POHNO = Convert.ToInt32(giftvoucherPOH[0].InvGiftVoucherPurchaseOrderHeaderID);
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderDetail> giftvoucherPOD = _GiftVoucherPO.GetPODetails(POHNO).ToList();
                RIT.HMS.Domain.Transactions.InvGiftVoucherMaster DBOInvGiftvoucherMaster = new RIT.HMS.Domain.Transactions.InvGiftVoucherMaster();

                int y = 0;
                int GiftVoucherMasterID = 0;
                if (giftvoucherPOD.Count != 0)
                {
                    for (y = 0; y < giftvoucherPOD.Count; y++)
                    {
                        GiftVoucherMasterID = Convert.ToInt32(giftvoucherPOD[y].InvGiftVoucherMasterID);
                        List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> GiftVoucherMaster = _GiftVoucherPO.GetGVMasterDetails(GiftVoucherMasterID).ToList();
                        invGVMaster.VoucherNo = GiftVoucherMaster[0].VoucherNo;
                        invGVMaster.VoucherSerial = GiftVoucherMaster[0].VoucherSerial;
                        itemList.Add(invGVMaster);

                    }
                }

                itemList.Add(invGVMaster);
                return itemList;
            }
            catch (Exception ex)
            {
                return itemList;
            }
        }
        [HttpPost]
        public ActionResult ViewGRNDetails(Models.GiftVoucherGoodReceiveNote invgiftvoucherMaster)
        {
            try
            {
                var ff = ViewGRN(invgiftvoucherMaster);
                if (ff.Count == 0)
                {
                    ViewBag.Message = "0";
                }
                else if (ff[0].BlockedUnitID == 4)
                {
                    ViewBag.Message = "4";
                }
                else
                {
                    ViewBag.Message = "3";
                }
                return View("~/Views/GiftVoucherGoodReceiveNote/Create.cshtml", invgiftvoucherMaster);
            }
            catch
            {
                ViewBag.Message = "0";
                return View("~/Views/GiftVoucherGoodReceiveNote/Create.cshtml");
                //return View(invgiftvoucherMaster);
            }
        }

        private int CreateVoucher(Models.GiftVoucherGoodReceiveNote invgiftvoucherMaster)
        {
            int res1 = 0;
            try
            {
                var ff = GenerateGiftVoucherMaster(invgiftvoucherMaster);
                if (ff.Count == 0)
                {
                    res1 = 0;
                }
                else if (ff[0].BlockedUnitID == 4)
                {
                    res1 = 4;
                }
                else
                {
                     res1 = 3;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return res1;
        }
        private int SaveGiftVoucherGRN(Models.GiftVoucherGoodReceiveNote GVGoodReceiveNote)
        {
            int res1 = 0;
            try
            {
                if (GVGoodReceiveNote.DocumentNo == null)
                {
                    res1 = SaveGVGRNwithOUT_PO(GVGoodReceiveNote);
                }
                else
                {
                    res1 = SaveGVGRNwithPO(GVGoodReceiveNote);
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return res1;
        }
        private int SaveGVGRNwithOUT_PO(Models.GiftVoucherGoodReceiveNote GVGoodReceiveNote)
        {            
            int GiftVoucherMasterID = 0;
            int res1 = 0;
            long BookID = 0;
            RIT.HMS.Domain.Transactions.invGiftVoucherPurchaseHeaders invGiftVoucherPurchaseHeader = new RIT.HMS.Domain.Transactions.invGiftVoucherPurchaseHeaders();
            RIT.HMS.Domain.Transactions.invGiftVoucherPurchaseHeaders invGiftVoucherGRNResult = new RIT.HMS.Domain.Transactions.invGiftVoucherPurchaseHeaders();
            RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseDetails invGiftVoucherPurchaseDetails = new RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseDetails();
            try
            {
                //List<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderHeader> giftvoucherPOH = _GiftVoucherPO.GetPOHeadsDetails(GVGoodReceiveNote.PurchaseOrderID).ToList(); //invGVMaster.PurchaseOrderNo).ToList();
                //POHNO = Convert.ToInt32(giftvoucherPOH[0].InvGiftVoucherPurchaseOrderHeaderID);
                //List<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderDetail> giftvoucherPOD = _GiftVoucherPO.GetPODetails(POHNO).ToList();
                //RIT.HMS.Domain.Transactions.InvGiftVoucherMaster DBOInvGiftvoucherMaster = new RIT.HMS.Domain.Transactions.InvGiftVoucherMaster();
                //Gift Voucher Master Id
                //GiftVoucherMasterID = Convert.ToInt32(giftvoucherPOD[0].InvGiftVoucherMasterID);
                //Get Book Code
                //List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> GiftVoucherMaster = _GiftVoucherPO.GetGVMasterDetails(GiftVoucherMasterID).ToList();
                int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                //List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetgvBookDetailsByID(companyid, Convert.ToInt32(GiftVoucherMaster[0].InvGiftVoucherBookCodeID)).ToList();
                //InvGiftVoucherBookCodeA objInvGiftVoucherBookCodeA = new InvGiftVoucherBookCodeA();
                //objInvGiftVoucherBookCodeA.giftvoucherBook = giftvoucherBook;
                List<InvGiftVoucherBookCode> giftvoucherBook1 = _billGiftVoucherGroup.GetInvGiftVoucherBookcodeByCode(GVGoodReceiveNote.BookCode1).ToList(); string GetGVDocNo_ = "";
                List<InvGiftVoucherDocumentNumber> resDocNo = _billGiftVPO.GetnewVoucherDocNo(companyid).ToList();
                BookID = giftvoucherBook1[0].InvGiftVoucherBookCodeID;
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

                invGiftVoucherPurchaseHeader.CompanyID = companyid;
                invGiftVoucherPurchaseHeader.LocationID = GVGoodReceiveNote.LocationAhidden;
                invGiftVoucherPurchaseHeader.DocumentID = GVGoodReceiveNote.DocumentID;
                invGiftVoucherPurchaseHeader.DocumentNo = GetGVDocNo_;
                invGiftVoucherPurchaseHeader.DocumentDate = DateTime.Now;
                invGiftVoucherPurchaseHeader.SupplierID = GVGoodReceiveNote.SupplierID;
                invGiftVoucherPurchaseHeader.GiftVoucherAmount = GVGoodReceiveNote.GiftVoucherValue;
                invGiftVoucherPurchaseHeader.GiftVoucherPercentage = GVGoodReceiveNote.GiftVoucherPercentage;
                invGiftVoucherPurchaseHeader.GrossAmount = GVGoodReceiveNote.GrossAmount;
                invGiftVoucherPurchaseHeader.DiscountAmount = GVGoodReceiveNote.DiscountAmount;
                invGiftVoucherPurchaseHeader.DiscountPercentage = GVGoodReceiveNote.DiscountPercentage;
                invGiftVoucherPurchaseHeader.OtherCharges = GVGoodReceiveNote.OtherCharges;
                invGiftVoucherPurchaseHeader.TaxAmount1 = GVGoodReceiveNote.TaxAmount1;
                invGiftVoucherPurchaseHeader.TaxAmount2 = GVGoodReceiveNote.TaxAmount2;
                invGiftVoucherPurchaseHeader.TaxAmount3 = GVGoodReceiveNote.TaxAmount3;
                invGiftVoucherPurchaseHeader.TaxAmount4 = GVGoodReceiveNote.TaxAmount4;
                invGiftVoucherPurchaseHeader.TaxAmount5 = GVGoodReceiveNote.TaxAmount5;
                invGiftVoucherPurchaseHeader.TaxAmount = GVGoodReceiveNote.TaxAmount;
                invGiftVoucherPurchaseHeader.NetAmount = GVGoodReceiveNote.NetAmount;
                invGiftVoucherPurchaseHeader.CreditLimit = GVGoodReceiveNote.CreditLimit;
                invGiftVoucherPurchaseHeader.CreditPeriod = GVGoodReceiveNote.CreditPeriod;
                invGiftVoucherPurchaseHeader.ChequeLimit = GVGoodReceiveNote.ChequeLimit;
                invGiftVoucherPurchaseHeader.ChequePeriod = GVGoodReceiveNote.ChequePeriod;
                invGiftVoucherPurchaseHeader.GiftVoucherQty = GVGoodReceiveNote.GiftVoucherQty;
                invGiftVoucherPurchaseHeader.PaymentTermID = GVGoodReceiveNote.PaymentTermID;
                invGiftVoucherPurchaseHeader.VoucherType = GVGoodReceiveNote.VoucherType;
                invGiftVoucherPurchaseHeader.DocumentStatus = GVGoodReceiveNote.DocumentStatus;
                invGiftVoucherPurchaseHeader.GroupOfCompanyID = GVGoodReceiveNote.GroupOfCompanyID;
                invGiftVoucherPurchaseHeader.CreatedUser = GVGoodReceiveNote.CreatedUser;
                invGiftVoucherPurchaseHeader.CreatedDate = DateTime.Now;
                invGiftVoucherPurchaseHeader.ModifiedUser = GVGoodReceiveNote.ModifiedUser;
                invGiftVoucherPurchaseHeader.ModifiedDate = DateTime.Now;
                invGiftVoucherPurchaseHeader.PartyInvoiceDate = DateTime.Now;
                invGiftVoucherPurchaseHeader.DataTransfer = 1;
                invGiftVoucherPurchaseHeader.PaymentPeriod = GVGoodReceiveNote.PaymentPeriod;
                invGiftVoucherPurchaseHeader.DispatchDate = DateTime.Now;
                invGiftVoucherPurchaseHeader.GiftVoucherPurchaseHeaderID = 1;
                invGiftVoucherPurchaseHeader.Remark = GVGoodReceiveNote.Remark;
                invGiftVoucherPurchaseHeader.ReferenceNo = GVGoodReceiveNote.ReferenceNo;

                if (GVGoodReceiveNote.GiftVoucherValue != 0)
                {
                    var res = _billGiftVGRN.SaveGiftVoucherGRN(invGiftVoucherPurchaseHeader);
                    res1 = res;
                }
                else
                {
                    res1 = 0;
                }
                if (res1 != 0)
                {
                    ViewBag.Message = "3";
                    invGiftVoucherGRNResult = _billGiftVGRN.GetPurchaseHeaderID(GetGVDocNo_);
                    List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> giftvoucherMasterBook = new List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster>();
                    giftvoucherMasterBook = _billGiftVoucherGroup.GetInvGiftVoucherMasterByBookCodeID(BookID, GVGoodReceiveNote.GiftVoucherQty).ToList();
                    if (giftvoucherMasterBook.Count != 0)
                    {
                        for (int y = 0; y < giftvoucherMasterBook.Count; y++)
                        {
                            //invGiftVoucherPurchaseDetails.InvGiftVoucherPurchaseDetailID = 0;
                            invGiftVoucherPurchaseDetails.InvGiftVoucherPurchaseHeaderID = invGiftVoucherGRNResult.InvGiftVoucherPurchaseHeaderID;
                            invGiftVoucherPurchaseDetails.CompanyID = invGiftVoucherGRNResult.CompanyID;
                            //invGiftVoucherPurchaseDetails.LocationId = invGiftVoucherGRNResult.LocationId;
                            invGiftVoucherPurchaseDetails.DocumentID = invGiftVoucherGRNResult.DocumentID;
                            invGiftVoucherPurchaseDetails.DocumentDate = invGiftVoucherGRNResult.DocumentDate;
                            invGiftVoucherPurchaseDetails.LineNo = y + 1;
                            invGiftVoucherPurchaseDetails.InvGiftVoucherMasterID = giftvoucherMasterBook[y].InvGiftVoucherMasterID;
                            invGiftVoucherPurchaseDetails.NumberOfCount = GVGoodReceiveNote.GiftVoucherQty;
                            invGiftVoucherPurchaseDetails.VoucherAmount = GVGoodReceiveNote.GiftVoucherAmount;
                            invGiftVoucherPurchaseDetails.VoucherType = GVGoodReceiveNote.VoucherType;
                            // invGiftVoucherPurchaseDetails.IsPurchase = false;
                            invGiftVoucherPurchaseDetails.DocumentStatus = GVGoodReceiveNote.DocumentStatus;
                            invGiftVoucherPurchaseDetails.GroupOfCompanyID = GVGoodReceiveNote.GroupOfCompanyID;
                            invGiftVoucherPurchaseDetails.CreatedUser = GVGoodReceiveNote.CreatedUser;
                            invGiftVoucherPurchaseDetails.CreatedDate = DateTime.Now;
                            invGiftVoucherPurchaseDetails.ModifiedUser = GVGoodReceiveNote.ModifiedUser;
                            invGiftVoucherPurchaseDetails.ModifiedDate = DateTime.Now;
                            // invGiftVoucherPurchaseDetails.DataTransfer = GVGoodReceiveNote.DataTransfer;
                            var DetailsSaveresult = _billGiftVGRN.SaveGiftVoucherDetails(invGiftVoucherPurchaseDetails);
                        }
                    }
                    InvGiftVoucherDocumentNumber giftvoucherDocNo = new InvGiftVoucherDocumentNumber();
                    giftvoucherDocNo.DocumentId = 3;
                    giftvoucherDocNo.DocumentName = "GiftVoucherGRN";
                    giftvoucherDocNo.DocumentNo = GetGVDocNo_;
                    giftvoucherDocNo.GroupOfCompanyID = GVGoodReceiveNote.GroupOfCompanyID;
                    giftvoucherDocNo.CompanyID = companyid;
                    giftvoucherDocNo.LocationId = GVGoodReceiveNote.LocationAhidden;
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
        private int SaveGVGRNwithPO(Models.GiftVoucherGoodReceiveNote GVGoodReceiveNote)
        {            
            int POHNO = 0;
            int GiftVoucherMasterID = 0;
            int res1 = 0;
            RIT.HMS.Domain.Transactions.invGiftVoucherPurchaseHeaders invGiftVoucherPurchaseHeader = new RIT.HMS.Domain.Transactions.invGiftVoucherPurchaseHeaders();
            RIT.HMS.Domain.Transactions.invGiftVoucherPurchaseHeaders invGiftVoucherGRNResult = new RIT.HMS.Domain.Transactions.invGiftVoucherPurchaseHeaders();
            RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseDetails invGiftVoucherPurchaseDetails = new RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseDetails();
            try
            {
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderHeader> giftvoucherPOH = _GiftVoucherPO.GetPOHeadsDetails(GVGoodReceiveNote.PurchaseOrderID).ToList(); //invGVMaster.PurchaseOrderNo).ToList();
                POHNO = Convert.ToInt32(giftvoucherPOH[0].InvGiftVoucherPurchaseOrderHeaderID);
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderDetail> giftvoucherPOD = _GiftVoucherPO.GetPODetails(POHNO).ToList();
                RIT.HMS.Domain.Transactions.InvGiftVoucherMaster DBOInvGiftvoucherMaster = new RIT.HMS.Domain.Transactions.InvGiftVoucherMaster();
                //Gift Voucher Master Id
                GiftVoucherMasterID = Convert.ToInt32(giftvoucherPOD[0].InvGiftVoucherMasterID);
                //Get Book Code
                List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> GiftVoucherMaster = _GiftVoucherPO.GetGVMasterDetails(GiftVoucherMasterID).ToList();
                int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetgvBookDetailsByID(companyid, Convert.ToInt32(GiftVoucherMaster[0].InvGiftVoucherBookCodeID)).ToList();
                InvGiftVoucherBookCodeA objInvGiftVoucherBookCodeA = new InvGiftVoucherBookCodeA();
                objInvGiftVoucherBookCodeA.giftvoucherBook = giftvoucherBook;
                List<InvGiftVoucherBookCode> giftvoucherBook1 = _billGiftVoucherGroup.GetInvGiftVoucherBookcodeByCode(GVGoodReceiveNote.BookCode).ToList();                string GetGVDocNo_ = "";
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

                invGiftVoucherPurchaseHeader.CompanyID = companyid;
                invGiftVoucherPurchaseHeader.LocationID = GVGoodReceiveNote.LocationAhidden;
                invGiftVoucherPurchaseHeader.DocumentID = GVGoodReceiveNote.DocumentID;
                invGiftVoucherPurchaseHeader.DocumentNo = GetGVDocNo_;
                invGiftVoucherPurchaseHeader.DocumentDate = DateTime.Now;
                invGiftVoucherPurchaseHeader.SupplierID = GVGoodReceiveNote.SupplierID;
                invGiftVoucherPurchaseHeader.GiftVoucherAmount = giftvoucherPOH[0].GiftVoucherAmount;
                invGiftVoucherPurchaseHeader.GiftVoucherPercentage = giftvoucherPOH[0].GiftVoucherPercentage;
                invGiftVoucherPurchaseHeader.GrossAmount = giftvoucherPOH[0].GrossAmount;
                invGiftVoucherPurchaseHeader.DiscountAmount = giftvoucherPOH[0].DiscountAmount;
                invGiftVoucherPurchaseHeader.DiscountPercentage = giftvoucherPOH[0].DiscountPercentage;
                invGiftVoucherPurchaseHeader.OtherCharges = giftvoucherPOH[0].OtherCharges;
                invGiftVoucherPurchaseHeader.TaxAmount1 = giftvoucherPOH[0].TaxAmount1;
                invGiftVoucherPurchaseHeader.TaxAmount2 = giftvoucherPOH[0].TaxAmount2;
                invGiftVoucherPurchaseHeader.TaxAmount3 = giftvoucherPOH[0].TaxAmount3;
                invGiftVoucherPurchaseHeader.TaxAmount4 = giftvoucherPOH[0].TaxAmount4;
                invGiftVoucherPurchaseHeader.TaxAmount5 = giftvoucherPOH[0].TaxAmount5;
                invGiftVoucherPurchaseHeader.TaxAmount = giftvoucherPOH[0].TaxAmount;
                invGiftVoucherPurchaseHeader.NetAmount = giftvoucherPOH[0].NetAmount;
                invGiftVoucherPurchaseHeader.CreditLimit = giftvoucherPOH[0].CreditLimit;
                invGiftVoucherPurchaseHeader.CreditPeriod = giftvoucherPOH[0].CreditPeriod;
                invGiftVoucherPurchaseHeader.ChequeLimit = giftvoucherPOH[0].ChequeLimit;
                invGiftVoucherPurchaseHeader.ChequePeriod = giftvoucherPOH[0].ChequePeriod;
                invGiftVoucherPurchaseHeader.GiftVoucherQty = giftvoucherPOH[0].GiftVoucherQty;
                invGiftVoucherPurchaseHeader.PaymentTermID = giftvoucherPOH[0].PaymentTermID;
                invGiftVoucherPurchaseHeader.VoucherType = giftvoucherPOH[0].VoucherType;
                invGiftVoucherPurchaseHeader.DocumentStatus = giftvoucherPOH[0].DocumentStatus;
                invGiftVoucherPurchaseHeader.GroupOfCompanyID = giftvoucherPOH[0].GroupOfCompanyID;
                invGiftVoucherPurchaseHeader.CreatedUser = GVGoodReceiveNote.CreatedUser;
                invGiftVoucherPurchaseHeader.CreatedDate = DateTime.Now;
                invGiftVoucherPurchaseHeader.ModifiedUser = GVGoodReceiveNote.ModifiedUser;
                invGiftVoucherPurchaseHeader.ModifiedDate = DateTime.Now;
                invGiftVoucherPurchaseHeader.PartyInvoiceDate = DateTime.Now;
                invGiftVoucherPurchaseHeader.DataTransfer = 1;
                invGiftVoucherPurchaseHeader.PaymentPeriod = GVGoodReceiveNote.PaymentPeriod;
                invGiftVoucherPurchaseHeader.DispatchDate = DateTime.Now;
                invGiftVoucherPurchaseHeader.GiftVoucherPurchaseHeaderID = 1;
                invGiftVoucherPurchaseHeader.Remark = GVGoodReceiveNote.Remark;
                invGiftVoucherPurchaseHeader.ReferenceNo = GVGoodReceiveNote.ReferenceNo;

                if (giftvoucherPOH[0].NetAmount != 0)
                {
                    var res = _billGiftVGRN.SaveGiftVoucherGRN(invGiftVoucherPurchaseHeader);
                    res1 = res;
                }
                else
                {
                    res1 = 0;
                }
                if (res1 != 0)
                {
                    ViewBag.Message = "3";
                    invGiftVoucherGRNResult = _billGiftVGRN.GetPurchaseHeaderID(GetGVDocNo_);
                    List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> giftvoucherMasterBook = new List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster>();
                    giftvoucherMasterBook = _billGiftVoucherGroup.GetInvGiftVoucherMasterByBookCodeID(giftvoucherBook[0].InvGiftVoucherBookCodeID, GVGoodReceiveNote.GiftVoucherQty).ToList();
                    if (giftvoucherMasterBook.Count != 0)
                    {
                        for (int y = 0; y < giftvoucherMasterBook.Count; y++)
                        {
                            //invGiftVoucherPurchaseDetails.InvGiftVoucherPurchaseDetailID = 0;
                            invGiftVoucherPurchaseDetails.InvGiftVoucherPurchaseHeaderID = invGiftVoucherGRNResult.InvGiftVoucherPurchaseHeaderID;
                            invGiftVoucherPurchaseDetails.CompanyID = invGiftVoucherGRNResult.CompanyID;
                            //invGiftVoucherPurchaseDetails.LocationId = invGiftVoucherGRNResult.LocationId;
                            invGiftVoucherPurchaseDetails.DocumentID = invGiftVoucherGRNResult.DocumentID;
                            invGiftVoucherPurchaseDetails.DocumentDate = invGiftVoucherGRNResult.DocumentDate;
                            invGiftVoucherPurchaseDetails.LineNo = y + 1;
                            invGiftVoucherPurchaseDetails.InvGiftVoucherMasterID = giftvoucherMasterBook[y].InvGiftVoucherMasterID;
                            invGiftVoucherPurchaseDetails.NumberOfCount = GVGoodReceiveNote.GiftVoucherQty;
                            invGiftVoucherPurchaseDetails.VoucherAmount = GVGoodReceiveNote.GiftVoucherAmount;
                            invGiftVoucherPurchaseDetails.VoucherType = GVGoodReceiveNote.VoucherType;
                            // invGiftVoucherPurchaseDetails.IsPurchase = false;
                            invGiftVoucherPurchaseDetails.DocumentStatus = GVGoodReceiveNote.DocumentStatus;
                            invGiftVoucherPurchaseDetails.GroupOfCompanyID = GVGoodReceiveNote.GroupOfCompanyID;
                            invGiftVoucherPurchaseDetails.CreatedUser = GVGoodReceiveNote.CreatedUser;
                            invGiftVoucherPurchaseDetails.CreatedDate = DateTime.Now;
                            invGiftVoucherPurchaseDetails.ModifiedUser = GVGoodReceiveNote.ModifiedUser;
                            invGiftVoucherPurchaseDetails.ModifiedDate = DateTime.Now;
                            // invGiftVoucherPurchaseDetails.DataTransfer = GVGoodReceiveNote.DataTransfer;
                            var DetailsSaveresult = _billGiftVGRN.SaveGiftVoucherDetails(invGiftVoucherPurchaseDetails);
                        }
                    }
                    InvGiftVoucherDocumentNumber giftvoucherDocNo = new InvGiftVoucherDocumentNumber();
                    giftvoucherDocNo.DocumentId = 3;
                    giftvoucherDocNo.DocumentName = "GiftVoucherGRN";
                    giftvoucherDocNo.DocumentNo = GetGVDocNo_;
                    giftvoucherDocNo.GroupOfCompanyID = GVGoodReceiveNote.GroupOfCompanyID;
                    giftvoucherDocNo.CompanyID = companyid;
                    giftvoucherDocNo.LocationId = GVGoodReceiveNote.LocationAhidden;
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
        public JsonResult GetGiftvoucherPurchaseOrderload()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<RIT.HMS.Domain.Transactions.InvGiftVoucherPurchaseOrderHeader> giftvoucherPO = _billGiftVoucherPO.GetPOHeads(companyid).ToList();
            PurchaseOrderID = giftvoucherPO[0].DocumentNo;
            return Json(JsonConvert.SerializeObject(giftvoucherPO, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
    }
}
