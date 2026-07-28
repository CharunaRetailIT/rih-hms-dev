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
    public class PaymentTermController : Controller
    {
        BLL_PaymentTerm _bllPaymentTerms;
        public PaymentTermController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllPaymentTerms = new BLL_PaymentTerm(cn);

        }
        public ActionResult Create()
        {

            return View();
        }

        [Authorize(Roles = "TaxesAndPayments")]
        public ActionResult Index()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            return View(_bllPaymentTerms.GetAllPaymentTerms(companyid));
        }
        [Authorize(Roles = "TaxesAndPayments")]
        public ActionResult Edit(long id)
        {
            //  CompanyService companyreporsitory = new CompanyService();
            var payterm = _bllPaymentTerms.GetPaymentTermById(id);
            ViewBag.PaymenttermId = payterm.PaymenttermId;
            return View(payterm);
        }

        [Authorize(Roles = "TaxesAndPayments")]
        [HttpPost]
        public ActionResult Edit(PaymentTerm paymentterm)
        {
            if (!ModelState.IsValid)
            {
                @ViewBag.PaymenttermId = paymentterm.PaymenttermId;
                return View(paymentterm);

            }
            //if (paymentterm.IsActive == true && paymentmethod.IsDelete == true)
            //{
            //    @ViewBag.PaymentMethodId = paymentmethod.PaymentMethodId;
            //    @ViewBag.Message = "4";
            //    return View(paymentmethod);
            //}

            @ViewBag.PaymenttermId = paymentterm.PaymenttermId;
            paymentterm.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            paymentterm.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (_bllPaymentTerms.UpdatePaymentMethod(paymentterm) == 1)
            {
                ViewBag.Message = "1";
                return View(new PaymentTerm());
            }
            else
            {
                @ViewBag.PaymenttermId = paymentterm.PaymenttermId;
                ViewBag.Message = "0";
                return View(paymentterm);
            }
        }

        [Authorize(Roles = "TaxesAndPayments")]
        [HttpPost]
        public ActionResult Create(PaymentTerm payterm)
        {
            if (!ModelState.IsValid)
            {
                @ViewBag.PaymenttermId = payterm.PaymenttermId;
                return View("Create", payterm);
            }
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            payterm.CompanyID = companyid;
            payterm.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString()); 
            var existscom = _bllPaymentTerms.GetPaymentMethodByCode(payterm.PaymentTermCode,companyid);
            if (existscom != null)
            {
                ViewBag.Message = "3";
                @ViewBag.PaymenttermId = payterm.PaymenttermId;
                return View("Create", payterm);
            }

            ViewBag.PaymentTermCode = payterm.PaymentTermCode;

            if (_bllPaymentTerms.SavePaymentMethod(payterm) == 1)
            {
                ViewBag.Message = "1";
                return View("Create", new PaymentTerm());
            }
            else
            {
                @ViewBag.PaymenttermId = payterm.PaymenttermId;
                ViewBag.Message = "2";
                return View("Create", payterm);
            }

        }
    }
}