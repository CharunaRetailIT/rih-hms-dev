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
    public class GiftVoucherCancelController : Controller
    {
        BLL_Location _blllocation;
        BLL_GiftVoucherGroup _billGiftVoucherGroup;
        BLL_GiftVoucherCancel _billGiftVoucherCancel;
        //List<InvGiftVoucherMasterTemp> invGiftVoucherMastersTemp = new List<InvGiftVoucherMasterTemp>();
        private InvGiftVoucherGroup existingInvGiftVoucherGroup;
        private string giftvouchergroupcode = string.Empty;
        public GiftVoucherCancelController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _blllocation = new BLL_Location(cn);
            _billGiftVoucherGroup = new BLL_GiftVoucherGroup(cn);
            _billGiftVoucherCancel = new BLL_GiftVoucherCancel(cn);
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
            return View(new Models.Transactions.InvGiftVoucherCancel());
        }        
        [HttpGet]
        public ActionResult ViewVoucher(string VoucherNo)
        {
            List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> res1 = _billGiftVoucherCancel.GetGiftVoucherDetails(VoucherNo).ToList();
            if (res1.Count == 0 && VoucherNo!=null)
            {
                ViewBag.Message = "1";
                return View("~/Views/GiftVoucherCancel/ViewCancelVoucher.cshtml", res1);
            }
            else
            {
                return View("~/Views/GiftVoucherCancel/ViewCancelVoucher.cshtml", res1);
            }
        }
        public ActionResult EditOLD(RIT.HMS.Domain.Transactions.InvGiftVoucherCancel invgiftVoucherCancel)
        {
            List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> res1 = _billGiftVoucherCancel.GetGiftVoucherDetails("").ToList();
            try
            {
                invgiftVoucherCancel.ModifiedDate = DateTime.Now;
                invgiftVoucherCancel.ModifiedUser = Session["loggeduser"].ToString();
                invgiftVoucherCancel.CreatedDate = DateTime.Now;
                invgiftVoucherCancel.CreatedUser = Session["loggeduser"].ToString();
                var res = _billGiftVoucherCancel.SaveGiftVoucherCancel(invgiftVoucherCancel);
                if (res != null && res != 0)
                {
                    ViewBag.Message = "3";
                    return View();
                }
                else
                {//fail
                    ViewBag.Message = "0";
                    //return View(invgiftVoucherCancel);
                    return View("~/Views/GiftVoucherCancel/ViewCancelVoucher.cshtml", res1);
                }
                return View("~/Views/GiftVoucherCancel/ViewCancelVoucher.cshtml", res1);
            }
            catch (Exception ex)
            {
                return View("~/Views/GiftVoucherCancel/ViewCancelVoucher.cshtml", res1);
            }
        }
        public ActionResult Edit(string VoucherPrefix)
        {
            RIT.HMS.Domain.Transactions.InvGiftVoucherCancel invgiftVoucherCancel = new RIT.HMS.Domain.Transactions.InvGiftVoucherCancel();
            List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> res1 = _billGiftVoucherCancel.GetGiftVoucherDetails("VoucherPrefix").ToList();
            try
            {               
                invgiftVoucherCancel.VoucherNo = VoucherPrefix;
                invgiftVoucherCancel.ModifiedDate = DateTime.Now;
                invgiftVoucherCancel.ModifiedUser = Session["loggeduser"].ToString();
                invgiftVoucherCancel.CreatedDate = DateTime.Now;
                invgiftVoucherCancel.CreatedUser = Session["loggeduser"].ToString();
                var res = _billGiftVoucherCancel.SaveGiftVoucherCancel(invgiftVoucherCancel);
                if (res != null && res != 0)
                {
                    ViewBag.Message = "3";
                    return View("~/Views/GiftVoucherCancel/ViewCancelVoucher.cshtml", res1);
                }
                else
                {//fail
                    ViewBag.Message = "0";
                    return View(invgiftVoucherCancel);
                }
                return View("~/Views/GiftVoucherCancel/ViewCancelVoucher.cshtml", res1);
            }
            catch (Exception ex)
            {
                return View("~/Views/GiftVoucherCancel/ViewCancelVoucher.cshtml", res1);
            }
        }
        class ErrorMsg
        {
          public  string  Message { get; set; }

        }

        public JsonResult ViewVoucherDetails(string VoucherNo)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<RIT.HMS.Domain.Transactions.InvGiftVoucherMaster> res1 = _billGiftVoucherCancel.GetGiftVoucherDetails(VoucherNo).ToList();
            RIT.HMS.Domain.Transactions.InvGiftVoucherMaster obj = new RIT.HMS.Domain.Transactions.InvGiftVoucherMaster();

            ErrorMsg objErrorMsg = new ErrorMsg();
            objErrorMsg.Message = "No Records Found";
            if (res1 != null && res1.Count > 0)
            {
                obj.InvGiftVoucherMasterID = res1[0].InvGiftVoucherMasterID;
                obj.GiftVoucherGroupCode = res1[0].GiftVoucherGroupCode;
                obj.BookCode = res1[0].BookCode;
                obj.GiftVoucherValue = res1[0].GiftVoucherValue;
                obj.VoucherNo = res1[0].VoucherNo;
                res1.Add(obj);
                return new JsonResult
                {
                    Data = res1.FirstOrDefault(),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {  
                return new JsonResult
                {
                    Data = objErrorMsg,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
        public JsonResult CancelGiftVoucher(string VoucherNo)
        {
            RIT.HMS.Domain.Transactions.InvGiftVoucherCancel obj1 = new RIT.HMS.Domain.Transactions.InvGiftVoucherCancel();
            obj1.ModifiedDate = DateTime.Now;
            obj1.ModifiedUser = Session["loggeduser"].ToString();
            obj1.CreatedDate = DateTime.Now;
            obj1.CreatedUser = Session["loggeduser"].ToString();
            obj1.VoucherNo = VoucherNo;
            obj1.IsCancel = true;
            obj1.Remark = "Voucher Cancel";

            var res = _billGiftVoucherCancel.SaveGiftVoucherCancel(obj1);
            if (res != 0)
            { // If Sucess 
                return new JsonResult
                {
                    Data = "Sucessfully Gift Voucher Cancelled",
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {//fail
                return new JsonResult
                {
                    Data = "Gift Voucher Cancel UnSucessfull",
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
    }
}

