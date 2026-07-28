using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.Domain;
using RIT.HMS.BLL.MasterData;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class PaymentMethodController : Controller
    {
        BLL_PaymentMethod _bllPaymentMethods;

        public PaymentMethodController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllPaymentMethods = new BLL_PaymentMethod(cn);
        }

        public ActionResult Create()
        {

            return View();
        }

        [Authorize(Roles = "TaxesAndPayments")]
        public ActionResult Index()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            return View(_bllPaymentMethods.GetAllPaymentMethods(companyid));
        }
        [Authorize(Roles = "TaxesAndPayments")]
        public ActionResult Edit(long id)
        {
            //  CompanyService companyreporsitory = new CompanyService();
            var paymethod = _bllPaymentMethods.GetPaymentMethodById(id);
            ViewBag.PaymentMethodId = paymethod.PaymentMethodId;
            return View(paymethod);
        }

        [Authorize(Roles = "TaxesAndPayments")]
        [HttpPost]
        public ActionResult Edit(PaymentMethod paymentmethod)
        {
            if (!ModelState.IsValid)
            {
                @ViewBag.PaymentMethodId = paymentmethod.PaymentMethodId;
                return View(paymentmethod);

            }
            if (paymentmethod.IsActive == true && paymentmethod.IsDelete == true)
            {
                @ViewBag.PaymentMethodId = paymentmethod.PaymentMethodId;
                @ViewBag.Message = "4";
                return View(paymentmethod);
            }
            if (paymentmethod.IsReceiptType == false && paymentmethod.IsPaymentType == false)
            {
                @ViewBag.PaymentMethodId = paymentmethod.PaymentMethodId;
                @ViewBag.Message = "5";
                return View(paymentmethod);
            }

            @ViewBag.PaymentMethodId = paymentmethod.PaymentMethodId;
            paymentmethod.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            paymentmethod.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (_bllPaymentMethods.UpdatePaymentMethod(paymentmethod) == 1)
            {
                ViewBag.Message = "1";
                return View(new PaymentMethod());
            }
            else
            {
                @ViewBag.PaymentMethodId = paymentmethod.PaymentMethodId;
                ViewBag.Message = "0";
                return View(paymentmethod);
            }
        }

        [Authorize(Roles = "TaxesAndPayments")]
        [HttpPost]
        public ActionResult Create(PaymentMethod paymethod)
        {
            if (!ModelState.IsValid)
            {
                @ViewBag.PaymentMethodId = paymethod.PaymentMethodId;
                return View("Create", paymethod);
            }
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            paymethod.CompanyID = companyid;
            paymethod.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            var existscom = _bllPaymentMethods.GetPaymentMethodByCode(paymethod.PaymentMethodCode,companyid);
            if (existscom != null)
            {
                ViewBag.Message = "3";
                @ViewBag.PaymentMethodId = paymethod.PaymentMethodId;
                return View("Create", paymethod);
            }
            if (paymethod.IsReceiptType == false && paymethod.IsPaymentType == false)
            {
                @ViewBag.PaymentMethodId = paymethod.PaymentMethodId;
                @ViewBag.Message = "5";
                return View(paymethod);
            }
            ViewBag.PaymentMethodCode = paymethod.PaymentMethodCode;

            if (_bllPaymentMethods.SavePaymentMethod(paymethod) == 1)
            {
                ViewBag.Message = "1";
                return View("Create", new PaymentMethod());
            }
            else
            {
                @ViewBag.PaymentMethodId = paymethod.PaymentMethodId;
                ViewBag.Message = "2";
                return View("Create", paymethod);
            }

        }
    }
}