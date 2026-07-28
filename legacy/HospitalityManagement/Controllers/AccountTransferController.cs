using Aspose.Pdf;
using Aspose.Pdf.Text;
using HospitalityManagement.Models;
using RIT.HMS.BLL.Configurations;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.Reports;
using RIT.HMS.Domain.Journal;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    public class AccountTransferController : Controller
    {
        private readonly BLL_Configuration _bllconfiguration;
        private AppManager _appmanager;
        private readonly BLL_Reports _bllreports;
        Boolean _success = false;
        Boolean _GlobIsGLDataProcessed = false;
        BLL_Location _location;
        BLL_Reports _ImportJurDetReport;

        public AccountTransferController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();

            _bllconfiguration = new BLL_Configuration(cn);
            _appmanager = new AppManager(cn);
            _bllreports = new BLL_Reports(cn);
            _location = new BLL_Location(cn);
            _ImportJurDetReport = new BLL_Reports(cn);
        }


        // GET: AccountTransfer
        public ActionResult Index()
        {
            //return View();
            return View("~/Views/AccountTransfer/GLDataTransfering.cshtml", new AccountDataTransfer());

        }
        // [HttpPost]
        public ActionResult GLDataTransfering(AccountDataTransfer AccDataTrans)
        {
            return View("~/Views/AccountTransfer/GLDataTransfering.cshtml", new AccountDataTransfer());
            // return View();

        }
        [Authorize(Roles = "Reports")]
        public ActionResult ProcessTransaction(AccountDataTransfer AccDataTransfering)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (_bllconfiguration.GetConfiguration("UReports", companyid).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "AccountTransfer"))
                {
                    @ViewBag.Permissions = "No user permissions to View Daily Sales";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            AccountDataTransfer accountDataTransfer = new AccountDataTransfer();
            @ViewBag.StartDate = DateTime.Now.ToShortDateString();
            @ViewBag.EndDate = DateTime.Now.ToShortDateString();
            accountDataTransfer.StartDate = System.DateTime.Now;
            accountDataTransfer.EndDate = System.DateTime.Now;
            AccountDataTransfer AccDataTrans = new AccountDataTransfer();
            AccDataTrans.StartDate = AccDataTransfering.StartDate;
            AccDataTrans.EndDate = AccDataTransfering.EndDate;
           // @ViewBag.LocationId = AccDataTransfering.LocationID;

            if (AccDataTransfering.StartDate.Date.ToShortDateString() == "01/01/0001")
            {
                @ViewBag.ShowAlertDate = 0;
            }
            //if (AccDataTransfering.EndDate.Date.ToShortDateString() == "01/01/0001")
            //{
            //    @ViewBag.ShowAlertDate = 0;
            //}
            //if (AccDataTransfering.StartDate > AccDataTransfering.EndDate)
            //{
            //    @ViewBag.ShowAlertWrongDatePeriod = 0;
            //}

            if (AccDataTransfering.IsValueApply == false)
            {
                @ViewBag.ShowAlert = 0;
            }
            
            if (AccDataTransfering.StartDate.Date.ToShortDateString() != "01/01/0001")
            {
                if (AccDataTransfering.IsValueApply == true)
                {
                    //AccDataTrans.ImportJournalDetList
                    _success = _bllreports.GetAuditTrailDataReport(AccDataTransfering.StartDate, AccDataTransfering.StartDate);

                    if (_success ==true)
                    {
                        @ViewBag.ShowStatusMsg = 1;
                        _GlobIsGLDataProcessed = true;

                        HMSAuditTrailPdf(AccDataTransfering);

                    }
                    else
                    {
                        @ViewBag.ShowStatusMsg = 0;
                        _GlobIsGLDataProcessed = false;
                    }

                }
            }
                return View("~/Views/AccountTransfer/GLDataTransfering.cshtml", AccDataTrans);


        }
        public ActionResult AccountLink(AccountDataTransfer AccDataTransfering)
        {
            Boolean _AccountLink = false;
            List<ImportJournalDetails> GetImportJournalDet = new List<ImportJournalDetails>();
            ListtoDataTableConverter converter = new ListtoDataTableConverter();

            GetImportJournalDet = _ImportJurDetReport.GetImportJournalDetails(AccDataTransfering.StartDate.Date, AccDataTransfering.StartDate.Date);
            DataTable dt = converter.ToDataTable(GetImportJournalDet);

            if (dt!=null && dt.Rows.Count>0)
            {
                _GlobIsGLDataProcessed = true;
            }
            else
            {
                _GlobIsGLDataProcessed = false;
            }


            if (_GlobIsGLDataProcessed == true)
            {
                _AccountLink = _bllreports.GLTransferingHMStoAccount(AccDataTransfering.StartDate, AccDataTransfering.StartDate);
                if (_AccountLink==true)
                {
                    @ViewBag.ShowMsgAccLink = 1;
                }
                else
                {
                    @ViewBag.ShowMsgAccLink = 0;
                }
                

            }
            else
            {
                @ViewBag.ShowMsgAccLink = 2;
            }

            return View("~/Views/AccountTransfer/GLDataTransfering.cshtml", new AccountDataTransfer());

        }

        public FileContentResult HMSAuditTrailPdf(AccountDataTransfer AccDataTransfering)
        {
            try
            {

            var document = new Document
            {
                PageInfo = new PageInfo { Margin = new MarginInfo(28, 28, 28, 40) }
            };

            var pdfpage = document.Pages.Add();
            string LocationName = "";


            string compName, Address1, Address2, Address3, Tele;
            compName = _location.GetCompanyDetails().CompanyName;
            Address1 = _location.GetCompanyDetails().Address1;
            Address2 = _location.GetCompanyDetails().Address2;
            Address3 = _location.GetCompanyDetails().Address3;
            Tele = _location.GetCompanyDetails().Telephone;
          
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(compName));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(Address1));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(Address2));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(Address3));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(Tele));
            ////pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(Fax));
            ////if (website != null)
            ////{
            ////    pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(website));
            ////}
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));



            var header7 = new TextFragment("Audit Trail Rerpot");
            header7.TextState.Font = FontRepository.FindFont("Arial");
            header7.TextState.FontSize = 12;
            header7.TextState.FontStyle = FontStyles.Bold;
            header7.HorizontalAlignment = HorizontalAlignment.Center;
            header7.Position = new Position(130, 720);
            pdfpage.Paragraphs.Add(header7);

            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));


            Aspose.Pdf.Table table = new Aspose.Pdf.Table
            {
                ColumnWidths = "16% 52% 16% 16%",
                DefaultCellPadding = new MarginInfo(10, 6, 10, 6),
                Border = new BorderInfo(BorderSide.All, .5f, Aspose.Pdf.Color.Black),
                DefaultCellBorder = new BorderInfo(BorderSide.All, .2f, Aspose.Pdf.Color.Black),
            };

           
            ////RptCustomerDetailViewModel objRpt = new RptCustomerDetailViewModel();
            ////if (TempData["ImportJournalDetailsReport"] != null)
            ////    objRpt = (RptCustomerDetailViewModel)TempData["ImportJournalDetailsReport"];

            List<ImportJournalDetails> GetImportJournalDetails = new List<ImportJournalDetails>();

            int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            GetImportJournalDetails = _ImportJurDetReport.GetImportJournalDetailReport(AccDataTransfering.StartDate.Date, AccDataTransfering.StartDate.Date);
            //var Location = _location.GetLocationById(objRpt.LocationId).LocationName;


            ListtoDataTableConverter converter = new ListtoDataTableConverter();
            DataTable dt = converter.ToDataTable(GetImportJournalDetails);


            table.ImportDataTable(dt, true, 0, 0);
            document.Pages[1].Paragraphs.Add(table);


            using (var strment = new MemoryStream())
            {
                document.Save(strment);
                return new FileContentResult(strment.ToArray(), "application/pdf")
                {
                    FileDownloadName = "AuditTrailReport.pdf"
                };

            }

                // Response.AppendHeader("Content-Disposition", "attachment; filename=AuditTrailReport.pdf");

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public class ListtoDataTableConverter
        {
            public DataTable ToDataTable<T>(List<T> items)
            {
                DataTable dataTable = new DataTable(typeof(T).Name);
                DataRow dr = dataTable.NewRow();
                //Get all the properties
                PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                //foreach (PropertyInfo prop in Props)
                //{
                //    //Setting column names as Property names
                //    dataTable.Columns.Add(prop.Name);
                //}

                dataTable.Columns.Add("ACODE", typeof(System.String));
                dataTable.Columns.Add("DESCRIPTION", typeof(System.String));
                dataTable.Columns.Add("DR", typeof(System.String));
                dataTable.Columns.Add("CR", typeof(System.String));
                foreach (T item in items)
                {
                    var values = new object[Props.Length];
                    for (int i = 0; i < Props.Length; i++)
                    {
                        dr = dataTable.NewRow();
                        //inserting property values to datatable rows
                        ///values[i] = Props[i].GetValue(item, null);
                        dr[0] = Props[8].GetValue(item, null);
                        dr[1] = Props[11].GetValue(item, null);

                        if (Props[10].GetValue(item, null).ToString()=="D")
                        {
                            dr[2] = Props[12].GetValue(item, null);
                            dr[3] = 0;
                        }
                        
                        if (Props[10].GetValue(item, null).ToString() == "C")
                        {
                            dr[3] = Props[12].GetValue(item, null);
                            dr[2] = 0;
                        }
                        
                        //dr[2] = Props[10].GetValue(item, null);
                        //dr[3] = Props[10].GetValue(item, null);
                    }
                    dataTable.Rows.Add(dr);
                }
                //put a breakpoint here and check datatable
                return dataTable;
            }
        }
    }
}