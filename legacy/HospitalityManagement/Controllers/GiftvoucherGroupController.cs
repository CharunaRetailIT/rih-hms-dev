using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    public class GiftvoucherGroupController : Controller
    {
        BLL_GiftVoucherGroup _billGiftVoucherGroup;
        // GET: GiftvoucherGroup
        public GiftvoucherGroupController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _billGiftVoucherGroup = new BLL_GiftVoucherGroup(cn);


        }
        public ActionResult Index()
        {
            return View();
        }

        // GET: GiftvoucherGroup/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: GiftvoucherGroup/Create
        public ActionResult Create()
        {
            GetNewGiftVoucherGroupCode();
            return View();
        }

        // POST: GiftvoucherGroup/Create
        [HttpPost]
        public ActionResult Create(InvGiftVoucherGroup invgiftVoucherGroup)
        {
            try
            {
                invgiftVoucherGroup.ModifiedDate = DateTime.Now;
                invgiftVoucherGroup.ModifiedUser = Session["loggeduser"].ToString();
                invgiftVoucherGroup.CreatedDate = DateTime.Now;
                invgiftVoucherGroup.CreatedUser = Session["loggeduser"].ToString();
                invgiftVoucherGroup.LocationName = "ABC";
                invgiftVoucherGroup.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());

                // TODO: Add insert logic here
                var res = _billGiftVoucherGroup.SaveGiftVoucherGroup(invgiftVoucherGroup);

                if (res != null && res != 0)
                {
                    ViewBag.Message = "3";
                    return View();

                }
                else
                {//fail
                    ViewBag.Message = "0";
                    return View(invgiftVoucherGroup);
                }
                //return View(invgiftVoucherGroup);
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: GiftvoucherGroup/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
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
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //[HttpGet]
        public ActionResult GetNewGiftVoucherGroupCode()
        {
            string GetGVgrupCode_ = "";
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<InvGiftVoucherGroup> res = _billGiftVoucherGroup.GetnewVoucherGroupID(companyid).ToList();

            if (res != null && res.Count > 0)
            {
                var GetGVgrupCode = res[0].GiftVoucherGroupCode;
                GetGVgrupCode_ = GetGVgrupCode;

                // Extract the prefix and numerical portion of the code
                string prefix = GetGVgrupCode.Substring(0, 1); // "G"
                string numericalPart = GetGVgrupCode.Substring(1); // "0011"
                int numericalValue = int.Parse(numericalPart);
                numericalValue++;
                // Format the numerical value back into the desired format
                GetGVgrupCode_ = prefix + numericalValue.ToString("D4"); // "G0012"

            }
            else
            {
                GetGVgrupCode_ = "G" + "0001";
            }
            ViewBag.GVgroupCode = GetGVgrupCode_;
            return View("~/Views/GiftvoucherGroup/Create.cshtml");
        }
        public ActionResult ViewAllGroups()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<InvGiftVoucherGroup> giftvouchergroupss = _billGiftVoucherGroup.GetGroups(companyid).ToList();
            ViewBag.GVGroupAll = giftvouchergroupss;
            return View("~/Views/GiftvoucherGroup/Create.cshtml");
        }
    }
}
