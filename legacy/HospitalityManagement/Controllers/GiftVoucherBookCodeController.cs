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
    public class GiftvoucherBookCodeController : Controller
    {
        BLL_GiftVoucherBookCode _billGiftVoucherBookCode;
        BLL_Location _blllocation;
        BLL_GiftVoucherGroup _billGiftVoucherGroup;
        //List<InvGiftVoucherMasterTemp> invGiftVoucherMastersTemp = new List<InvGiftVoucherMasterTemp>();
        //private InvGiftVoucherGroup existingInvGiftVoucherGroup;
        //private InvGiftVoucherBookCode existingInvGiftVoucherBookCode;
        private string giftvouchergroupcode = string.Empty;
        public GiftvoucherBookCodeController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _billGiftVoucherBookCode = new BLL_GiftVoucherBookCode(cn);
            _blllocation = new BLL_Location(cn);
            _billGiftVoucherGroup = new BLL_GiftVoucherGroup(cn);
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
            int bookcodecount = 0;
            string firstChar = "";
            string serialBook = "";
            int serialBookNew = 0;
            InvGiftVoucherBookCode invgiftVoucherBookCode = new InvGiftVoucherBookCode();            
            List<InvGiftVoucherBookCode> res = _billGiftVoucherBookCode.GetOldBookCode().ToList();
            if (res != null && res.Count > 0)
            {
                bookcodecount = res.Count;
                var GetBookCode = res[bookcodecount - 1].BookCode;
                int stringcount = GetBookCode.Length;
                firstChar = GetBookCode.Substring(0, 1);
                serialBook = GetBookCode.Substring(1, (stringcount - 1));
                serialBookNew = Convert.ToInt32(serialBook) + 1;
                invgiftVoucherBookCode.BookCode =  firstChar + Convert.ToString(serialBookNew);
            }
            else
            {
                invgiftVoucherBookCode.BookCode = "B1000001";
            }
            invgiftVoucherBookCode.SerialLength = 8;
            invgiftVoucherBookCode.VoucherType = 1;
            return View(invgiftVoucherBookCode);
        }
        [Authorize(Roles = "CusView")]
        public ActionResult ViewBookCode()
        {

            int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var exists = _billGiftVoucherBookCode.GetGiftVoucherBookCode();
            return View("~/Views/GiftVoucherBookCode/ViewVoucherBook.cshtml", exists);
        }

        // GET: GiftvoucherGroup/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(InvGiftVoucherBookCode invgiftVoucherBookCode)
        {
            try
            {
                invgiftVoucherBookCode.ModifiedDate = DateTime.Now;
                invgiftVoucherBookCode.ModifiedUser = Session["loggeduser"].ToString();
                invgiftVoucherBookCode.CreatedDate = DateTime.Now;
                invgiftVoucherBookCode.CreatedUser = Session["loggeduser"].ToString();
                invgiftVoucherBookCode.LocationName = "TEST";
                if (invgiftVoucherBookCode.GiftVoucherPercentage > 0)
                {
                    invgiftVoucherBookCode.VoucherType = 2;
                }
                else
                {
                    invgiftVoucherBookCode.VoucherType = 1;
                }
                // TODO: Add insert logic here
                var res = _billGiftVoucherBookCode.SaveGiftVoucherBookGenarator(invgiftVoucherBookCode);
                if (res != null && res != 0)
                {
                    ViewBag.Message = "3";
                    ModelState.Clear();
                    return View();
                }
                else
                {//fail
                    ViewBag.Message = "0";
                    return View(invgiftVoucherBookCode);
                }
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }
        // POST: GiftvoucherGroup/Edit/5
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

        // GET: GiftvoucherGroup/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: GiftvoucherGroup/Delete/5
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
            if (giftvouchergroupss.Count > 0)
            {
                giftvouchergroupcode = giftvouchergroupss[0].GiftVoucherGroupCode;
            }
            return Json(JsonConvert.SerializeObject(giftvouchergroupss, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }        
    }
}
