using HospitalityManagement.Models.Transactions;
using HospitalityManagement.Service;
using HospitalityManagement.Service.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Transactions
{
    [SessionTimeout]
    public class PartialGRNController : Controller
    {

        private CommonService commonService = new CommonService();
        private PurchaseService purchaseService = new PurchaseService();
        private PurchaseOrderService _purchaseorderservice = new PurchaseOrderService();
        private ProductService _productservice = new ProductService();
        private readonly TaxService _taxService = new TaxService();
        private readonly LocationService _locationService = new LocationService();
        private readonly CompanyService _companyService = new CompanyService();
        private readonly SupplierService _supplierService = new SupplierService();

        // GET: PartialGRN
        public ActionResult Index()
        {
            var pheader = new PurchaseHeader();
        
            int docid = commonService.GetDcumentId("GRN");
            var docnum = commonService.GetDocumentNo("GRN",
                                                    Convert.ToInt32(Session["loggeduserlocId"]),
                                                    "1",
                                                     docid,
                                                     true);

            pheader.DocumentNo = docnum;
            pheader.GRNDate = DateTime.Now;
            return View("~/Views/Transactions/PartialGRN/PartialGRN.cshtml", pheader);
        }
    }
}