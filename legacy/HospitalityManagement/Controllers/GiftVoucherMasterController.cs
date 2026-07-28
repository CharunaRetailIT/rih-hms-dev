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
    public class GiftVoucherMasterController : Controller
    {
        BLL_Location _blllocation;
        BLL_GiftVoucherGroup _billGiftVoucherGroup;
        //List<InvGiftVoucherMasterTemp> invGiftVoucherMastersTemp = new List<InvGiftVoucherMasterTemp>();
        private InvGiftVoucherGroup existingInvGiftVoucherGroup;
        private InvGiftVoucherBookCode existingInvGiftVoucherBookCode;
        private string giftvouchergroupcode = string.Empty;
        public GiftVoucherMasterController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _blllocation = new BLL_Location(cn);
            _billGiftVoucherGroup = new BLL_GiftVoucherGroup(cn);
        }
        // GET: GiftVoucherMaster
        public ActionResult Index()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            return View();
        }
        // GET: GiftVoucherMaster/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: GiftVoucherMaster/Create
        public ActionResult Create()
        {

            return View(new Models.Transactions.InvGiftVoucherMaster());
        }

        // POST: GiftVoucherMaster/Create
        [HttpPost]
        public ActionResult Create(Models.Transactions.InvGiftVoucherMaster invgiftvoucherMaster)
        {
            try
            {
                // TODO: Add insert logic here
                var ff = GenerateGiftVoucherMaster(invgiftvoucherMaster);

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



                // return RedirectToAction("~/Views/GiftVoucherMaster/Create.cshtml");
                return View();
            }
            catch
            {
                //return View("~/Views/GiftVoucherMaster/Create.cshtml");
                ViewBag.Message = "0";
                return View(invgiftvoucherMaster);
            }
        }

        // GET: GiftVoucherMaster/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: GiftVoucherMaster/Edit/5
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

        // GET: GiftVoucherMaster/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: GiftVoucherMaster/Delete/5
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
        //public string  HMSGVGenarate()
        public ActionResult HMSGVGenarate(Models.Transactions.InvGiftVoucherMaster invGVMaster)
        {
            int currentBookNo = 0;
            int StartingNo = invGVMaster.StartingNo;
            int VouNo = invGVMaster.StartingNo;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            InvGiftVoucherBookCodeA objInvGiftVoucherBookCodeA = new InvGiftVoucherBookCodeA();

            List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetInvGiftVoucherBookcodeByCode(invGVMaster.BookCode).ToList();

            objInvGiftVoucherBookCodeA.giftvoucherBook = giftvoucherBook;

            existingInvGiftVoucherGroup = _billGiftVoucherGroup.GetInvGiftVoucherGroupByCode(invGVMaster.GiftVoucherGroupCode);
            List<Models.Transactions.InvGiftVoucherMaster> itemList = new List<Models.Transactions.InvGiftVoucherMaster>();

            currentBookNo = invGVMaster.VoucherSerialNo;
            int y = 0;
            if (invGVMaster.PageCount != 0)
            {
                for (int x = 1; x <= invGVMaster.VoucherCount; x = x + y)
                {
                    for (y = 0; y < invGVMaster.PageCount; y++)
                    {
                        Models.Transactions.InvGiftVoucherMaster invGVMaster_ = new Models.Transactions.InvGiftVoucherMaster();
                        invGVMaster_.InvGiftVoucherBookCodeID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].InvGiftVoucherBookCodeID;
                        invGVMaster_.InvGiftVoucherGroupID = existingInvGiftVoucherGroup.InvGiftVoucherGroupID;
                        invGVMaster_.CompanyID = companyid;
                        invGVMaster_.LocationId = objInvGiftVoucherBookCodeA.giftvoucherBook[0].LocationId;
                        invGVMaster_.VoucherNo = GetBookCodeFormat(objInvGiftVoucherBookCodeA.giftvoucherBook[0].BookPrefix, objInvGiftVoucherBookCodeA.giftvoucherBook[0].SerialLength, VouNo);
                        invGVMaster_.VoucherNoSerial = currentBookNo;
                        invGVMaster_.VoucherSerial = GetBookCodeFormat(invGVMaster.VoucherPrefix, invGVMaster.SerialLength, StartingNo);
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
                        //InvGiftVoucherMasterTemp tmpvoumaster = new InvGiftVoucherMasterTemp();
                        //tmpvoumaster.GVMasTempList[0].BookCode = invGVMaster_.BookCode;
                        //invGVMaster_.GVBookcodes[y].BookCode = invGVMaster.BookCode;
                        //invGVMaster_.GVGroups[y].GiftVoucherGroupCode = invGVMaster.GiftVoucherGroupCode;
                        itemList.Add(invGVMaster_);
                    }
                    currentBookNo = currentBookNo + 1;
                }

                ViewBag.ItemList = itemList;
                ViewBag.GroupID = invGVMaster.GiftVoucherGroupCode;
                ViewBag.BookID = invGVMaster.BookCode;

            }

            return View("~/Views/GiftVoucherMaster/Create.cshtml", invGVMaster);
            //return PartialView("~/Views/GiftVoucherMaster/Create.cshtml", invGVMaster);
            //return View(invGiftVoucherMastersTemp);


        }

        private List<Models.Transactions.InvGiftVoucherMaster> GenerateGiftVoucherMaster(Models.Transactions.InvGiftVoucherMaster invGVMaster)
        {
            int currentBookNo = 0;
            int StartingNo = invGVMaster.StartingNo;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            int VouNo = invGVMaster.StartingNo;
            InvGiftVoucherBookCodeA objInvGiftVoucherBookCodeA = new InvGiftVoucherBookCodeA();
            List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetInvGiftVoucherBookcodeByCode(invGVMaster.BookCodehidden).ToList();

            objInvGiftVoucherBookCodeA.giftvoucherBook = giftvoucherBook;

            //existingInvGiftVoucherBookCode = _billGiftVoucherGroup.GetInvGiftVoucherBookcodeByCode(invGVMaster.BookCode);
            existingInvGiftVoucherGroup = _billGiftVoucherGroup.GetInvGiftVoucherGroupByCode(invGVMaster.GiftVoucherGroupCodehidden);
            List<Models.Transactions.InvGiftVoucherMaster> itemList = new List<Models.Transactions.InvGiftVoucherMaster>();
            //InvGiftVoucherMasterTemp itemListtemp = new InvGiftVoucherMasterTemp();
            RIT.HMS.Domain.Transactions.InvGiftVoucherMaster DBOInvGiftvoucherMaster = new RIT.HMS.Domain.Transactions.InvGiftVoucherMaster();
            try
            {
                var book = _billGiftVoucherGroup.CheckBookCode(giftvoucherBook[0].InvGiftVoucherBookCodeID);

                if (book == null)
                {
                    currentBookNo = invGVMaster.VoucherSerialNo;
                    int y = 0;
                    if (invGVMaster.PageCount != 0)
                    {
                        for (int x = 1; x <= invGVMaster.VoucherCount; x = x + y)
                        {
                            for (y = 0; y < invGVMaster.PageCount; y++)
                            {
                                DBOInvGiftvoucherMaster.InvGiftVoucherBookCodeID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].InvGiftVoucherBookCodeID;

                                DBOInvGiftvoucherMaster.InvGiftVoucherBookCodeID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].InvGiftVoucherBookCodeID;
                                DBOInvGiftvoucherMaster.InvGiftVoucherGroupID = existingInvGiftVoucherGroup.InvGiftVoucherGroupID;
                                DBOInvGiftvoucherMaster.CompanyID = companyid;
                                DBOInvGiftvoucherMaster.LocationId = objInvGiftVoucherBookCodeA.giftvoucherBook[0].LocationId;
                                DBOInvGiftvoucherMaster.VoucherNo = GetBookCodeFormat(objInvGiftVoucherBookCodeA.giftvoucherBook[0].BookPrefix, objInvGiftVoucherBookCodeA.giftvoucherBook[0].SerialLength, VouNo);
                                DBOInvGiftvoucherMaster.VoucherNoSerial = currentBookNo;
                                DBOInvGiftvoucherMaster.VoucherPrefix = invGVMaster.VoucherPrefix;
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
                            for (int x = 1; x <= invGVMaster.VoucherCount; x = x + y)
                            {
                                for (y = 0; y < invGVMaster.PageCount; y++)
                                {
                                    DBOInvGiftvoucherMaster.InvGiftVoucherBookCodeID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].InvGiftVoucherBookCodeID;
                                    DBOInvGiftvoucherMaster.InvGiftVoucherGroupID = objInvGiftVoucherBookCodeA.giftvoucherBook[0].InvGiftVoucherGroupID;
                                    DBOInvGiftvoucherMaster.CompanyID = companyid;
                                    DBOInvGiftvoucherMaster.LocationId = objInvGiftVoucherBookCodeA.giftvoucherBook[0].LocationId;
                                    DBOInvGiftvoucherMaster.VoucherNo = GetBookCodeFormat(objInvGiftVoucherBookCodeA.giftvoucherBook[0].BookPrefix, objInvGiftVoucherBookCodeA.giftvoucherBook[0].SerialLength, VouNo);
                                    DBOInvGiftvoucherMaster.VoucherNoSerial = currentBookNo;
                                    DBOInvGiftvoucherMaster.VoucherPrefix = invGVMaster.VoucherPrefix;
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


                itemList.Add(invGVMaster);
                return itemList;
            }
            catch (Exception ex)
            {
                return itemList;
            }
        }

        //private List<InvGiftVoucherMasterTemp> GenerateGiftVoucherBook(string prefix, int bookCodeLength, int startingNo, int noOfPagesOnBook, int noOfVouchers)
        //{
        //    int //bookNo = 0,
        //           currentBookNo = 0;

        //    if (invGiftVoucherMastersTemp == null)
        //    { invGiftVoucherMastersTemp = new List<InvGiftVoucherMasterTemp>(); }

        //    invGiftVoucherMastersTemp.Clear();
        //    existingInvGiftVoucherGroup = _billGiftVoucherGroup.GetInvGiftVoucherGroupByCode();

        //}



        [HttpGet]
        public JsonResult GetGiftvoucherGroupCodeby()
        {
            //  LocationService reporsitory = new LocationService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //var locations = _blllocation.GetActiveLocations(companyid);
            //var giftvouchergroups = _billGiftVoucherGroup.GetGroups(companyid);
            List<InvGiftVoucherGroup> giftvouchergroupss = _billGiftVoucherGroup.GetGroups(companyid).ToList();
            giftvouchergroupcode = giftvouchergroupss[0].GiftVoucherGroupCode;
            return Json(JsonConvert.SerializeObject(giftvouchergroupss, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetGiftvoucherGroup()
        {
            //  LocationService reporsitory = new LocationService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //var locations = _blllocation.GetActiveLocations(companyid);
            //var giftvouchergroups = _billGiftVoucherGroup.GetGroups(companyid);
            List<InvGiftVoucherGroup> giftvouchergroupss = _billGiftVoucherGroup.GetGroups(companyid).ToList();
            giftvouchergroupcode = giftvouchergroupss[0].GiftVoucherGroupCode;
            return Json(JsonConvert.SerializeObject(giftvouchergroupss, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetGiftvoucherBook()
        {
            //  LocationService reporsitory = new LocationService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //var locations = _blllocation.GetActiveLocations(companyid);
            var giftvoucherBook = _billGiftVoucherGroup.GetgvBooks(companyid);
            return Json(JsonConvert.SerializeObject(giftvoucherBook, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetGiftvoucherBookDetails(string GvBookCode)
        {//InvGiftVoucherBookCode
            //  LocationService reporsitory = new LocationService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //var locations = _blllocation.GetActiveLocations(companyid);
            //var giftvoucherBook = _billGiftVoucherGroup.GetgvBookDetails(companyid,GvBookCode);

            InvGiftVoucherBookCodeA objInvGiftVoucherBookCodeA = new InvGiftVoucherBookCodeA();
            List<InvGiftVoucherBookCode> giftvoucherBook = _billGiftVoucherGroup.GetgvBookDetails(companyid, GvBookCode).ToList();
            objInvGiftVoucherBookCodeA.giftvoucherBook = giftvoucherBook;

            if (giftvoucherBook != null && giftvoucherBook.Count > 0)
            {
                string prefix = giftvoucherBook[0].BookPrefix;
                int length = giftvoucherBook[0].SerialLength;
                int startingNo = giftvoucherBook[0].StartingNo;
                objInvGiftVoucherBookCodeA.SerialFormat = GetBookCodeFormat(prefix, length, startingNo);
                //string bookFormat = "";

                //if (length > 0)
                //{
                //    length = (length - prefix.Length);
                //}

                //if (!string.IsNullOrEmpty(length.ToString()))
                //{
                //    bookFormat = String.Format("{0}{1," + length + ":D" + length + "} ", prefix, startingNo);
                //}
                //objInvGiftVoucherBookCodeA.SerialFormat = bookFormat;
                //giftvoucherBook[0].SerialFormat = bookFormat;
            }



            return Json(JsonConvert.SerializeObject(objInvGiftVoucherBookCodeA, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        public string GetBookCodeFormat(string prefix, int length, int pageNo)
        {
            string bookFormat = "";
            if(prefix!=null && length>0)
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
    }
}
