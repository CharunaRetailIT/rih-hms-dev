using OfficeOpenXml;
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.Configurations;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.BLL.Reports;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers.Transactions
{
    public class StockCountController : Controller
    {
        private readonly BLL_Common _bllcommon;      
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_Department _blldepartment;
        private readonly AppManager _appmanager;    
        private readonly BLL_Location _blllocation;
        private readonly BLL_StockCount _bllstockcount;
      

        public StockCountController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcommon = new BLL_Common(cn);        
            _bllconfiguration = new BLL_Configuration(cn);
            _bllproduct = new BLL_Product(cn);
            _blldepartment = new BLL_Department(cn);
            _appmanager = new AppManager(cn);          
            _blllocation = new BLL_Location(cn);
            _bllstockcount = new BLL_StockCount(cn);

        }
        public ActionResult StartJob()
        {
            // check permissions
            if (!_appmanager.SetPermissions(16, Session["loggeduserempcode"].ToString(), "StockCount"))
            {
                @ViewBag.Permissions = "No user permissions to Perform Stock Count";
                return View("~/Views/Account/AccessDenied.cshtml");
            }
            //end check permissions


            JobHeader jobheader = new JobHeader();
            jobheader.Messanger = 0; // no message
            List<SelectListItem> Departments = new List<SelectListItem>();
            SelectListItem defaultdept = new SelectListItem();
            defaultdept.Text = "-- All Departments --";
            defaultdept.Value = "0";
            Departments.Add(defaultdept);
            var depts = _blldepartment.GetActiveDepartments(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var dept in depts)
            {
                SelectListItem deptitem = new SelectListItem();
                deptitem.Text = (dept.DepartmentCode + "-" + dept.DepartmentName);
                deptitem.Value = dept.RstDepartmentID.ToString();
                Departments.Add(deptitem);
            }
            jobheader.Departments = Departments;

            List<SelectListItem> Locations = new List<SelectListItem>();
            SelectListItem defaultlocation = new SelectListItem();
            defaultlocation.Text = "-- Select A Location --";
            defaultlocation.Value = "0";
            Locations.Add(defaultlocation);
            var locations = _blllocation.GetActiveLocations(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var loc in locations)
            {
                SelectListItem locitem = new SelectListItem();
                locitem.Text = (loc.LocationCode + "-" + loc.LocationName);
                locitem.Value = loc.SysLocationID.ToString();
                Locations.Add(locitem);
            }
            jobheader.Locations = Locations;


            jobheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"]);           
            jobheader.JobDate = DateTime.Now;

            var currentjob = _bllstockcount.CurrentJob(Convert.ToInt32(Session["loggedusercompanyId"]));
            if (currentjob == null)
            {

                int docid = _bllcommon.GetDcumentId("Stock Count", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentNo("Stock Count",
                                                      Convert.ToInt32(Session["loggeduserlocId"]),
                                                      "1",
                                                      docid,
                                                      false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

                jobheader.JobNumber = docnum;
            }
            else
            {
                jobheader.JobNumber = currentjob.JobNumber;
            }
            return View("~/Views/Transactions/StockCount/StockCount.cshtml", jobheader);
        }

        [HttpPost]
        public ActionResult StartJob(JobHeader jobheader)
        {
            jobheader.Messanger = 0; // no message
            jobheader.Locations = GetLocationAndDepartmentDropdowns().Item1;
            jobheader.Departments = GetLocationAndDepartmentDropdowns().Item2;

            
           
            if (jobheader.LocationId == 0)
            {
                ModelState.AddModelError("LocationId", "Select a Location");
                return View("~/Views/Transactions/StockCount/StockCount.cshtml", jobheader);
            }

            jobheader.CreatedUser = Convert.ToString(Session["loggeduser"]);
            jobheader.StartTime = DateTime.Now;

            var jobitems = _bllstockcount.CurrentStock(jobheader).JobItem;
            jobheader.JobItem = jobitems;
            if (jobheader != null)
            {
                jobheader.Messanger = 2;
            }
            else
            {
                jobheader.Messanger = 1;
            }

            return View("~/Views/Transactions/StockCount/StockCount.cshtml", jobheader);
        }


        [HttpPost]
        public ActionResult UploadFile(JobHeader jobheader)
        {

            
            jobheader.Messanger = 0; // no message
            jobheader.ModifiedDate = DateTime.Now;
            jobheader.ModifiedUser= Convert.ToString(Session["loggeduser"]);
            jobheader.Locations = GetLocationAndDepartmentDropdowns().Item1;
            jobheader.Departments = GetLocationAndDepartmentDropdowns().Item2;
           
            jobheader.CreatedUser = Convert.ToString(Session["loggeduser"]);
            HttpPostedFileBase file = Request.Files["ProductUploadFile"];

            if (file.ContentLength == 0)
            {
                return View("~/Views/Transactions/StockCount/StockCount.cshtml", jobheader);
            }


            JobHeader newjobheader = new JobHeader();
         

            if (Request != null)
            {
                List<JobItem> excelitems = new List<JobItem>();
               

              //  HttpPostedFileBase file = Request.Files["ProductUploadFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                    using (var package = new ExcelPackage(file.InputStream))
                    {

                        var sheets = package.Workbook.Worksheets;
                        foreach (var currentSheet in sheets)
                        {
                            var worksheet = currentSheet;
                            int colcount = 0;
                            int rowcount = 0;
                            if (worksheet.Dimension != null)
                            {
                                colcount = worksheet.Dimension.End.Column;
                                rowcount = worksheet.Dimension.End.Row;
                            }
                            for (int rowIterator = 2; rowIterator <= rowcount; rowIterator++)
                            {
                                JobItem excelitem = new JobItem();

                                excelitem.ProductId = Convert.ToInt32(worksheet.Cells[rowIterator, 2].Value);
                                excelitem.ProductCode = Convert.ToString(worksheet.Cells[rowIterator, 3].Value);
                                excelitem.SystemQty = Convert.ToDecimal(worksheet.Cells[rowIterator, 5].Value);
                                excelitem.PhysicalQty = Convert.ToDecimal(worksheet.Cells[rowIterator, 6].Value);

                                jobheader.JobItem.Add(excelitem);

                            }

                        }
                    }

                   
                    newjobheader = _bllstockcount.UploadFile(jobheader);
                  
                    newjobheader.Locations = GetLocationAndDepartmentDropdowns().Item1;
                    newjobheader.Departments = GetLocationAndDepartmentDropdowns().Item2;

                }
            }

            if (newjobheader.Messanger == 4)
            {
                newjobheader.Message = "File Uploaded Sucessfully !";
                string sss = ConfigurationManager.AppSettings["ExcelPath"].ToString();
                string path = Server.MapPath(sss);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                file.SaveAs(path + Path.GetFileName(file.FileName + DateTime.Now.ToString() + Convert.ToString(Session["loggeduser"])));


            }
            else 
            if (newjobheader.Messanger == 5)
            {
                newjobheader.Message = "Failed to upload file !";
            }
            return View("~/Views/Transactions/StockCount/StockCount.cshtml", newjobheader);
        }

        [HttpPost]
        public ActionResult ConfirmJob(JobHeader jobheader)
        {
            jobheader.Messanger = 0; // no message
            jobheader.ModifiedDate = DateTime.Now;
            jobheader.ModifiedUser = Convert.ToString(Session["loggeduser"]);
            jobheader.Locations = GetLocationAndDepartmentDropdowns().Item1;
            jobheader.Departments = GetLocationAndDepartmentDropdowns().Item2;

            jobheader.CreatedUser = Convert.ToString(Session["loggeduser"]);

            JobHeader newjobheader = new JobHeader();

            newjobheader = _bllstockcount.ConfirmJob(jobheader);
            newjobheader.Locations = GetLocationAndDepartmentDropdowns().Item1;
            newjobheader.Departments = GetLocationAndDepartmentDropdowns().Item2;

            if (newjobheader.Messanger == 6)
            {
                ModelState.Clear();
                newjobheader.Locations = GetLocationAndDepartmentDropdowns().Item1;
                newjobheader.Departments = GetLocationAndDepartmentDropdowns().Item2;
                int docid = _bllcommon.GetDcumentId("Stock Count", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentNo("Stock Count",
                                                      Convert.ToInt32(Session["loggeduserlocId"]),
                                                      "1",
                                                      docid,
                                                      false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                newjobheader.JobNumber = docnum;
                newjobheader.Message = "Job"+ newjobheader .JobNumber+ " Confirmed Sucessfully !";

            }
            else
            if (newjobheader.Messanger == 7)
            {
                newjobheader.Message = "Failed to Confirm Job" + newjobheader.JobNumber + "!";
            }
            return View("~/Views/Transactions/StockCount/StockCount.cshtml", newjobheader);
        }

        [HttpPost]
        public ActionResult ViewJob(JobHeader jobheader)
        {
            jobheader.Locations = GetLocationAndDepartmentDropdowns().Item1;
            jobheader.Departments = GetLocationAndDepartmentDropdowns().Item2;
            jobheader.Messanger = 0; // no message
           // var job = _bllstockcount.ViewJob(jobheader);
            var jobitems = _bllstockcount.ViewJob(jobheader).JobItem;
            jobheader.JobItem = jobitems;
            jobheader.Messanger = 0;

            return View("~/Views/Transactions/StockCount/StockCount.cshtml", jobheader);
        }

        private Tuple<List<SelectListItem>, List<SelectListItem>> GetLocationAndDepartmentDropdowns()
        {
            List<SelectListItem> Departments = new List<SelectListItem>();
            SelectListItem defaultdept = new SelectListItem();
            defaultdept.Text = "-- All Departments --";
            defaultdept.Value = "0";
            Departments.Add(defaultdept);
            var depts = _blldepartment.GetActiveDepartments(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var dept in depts)
            {
                SelectListItem deptitem = new SelectListItem();
                deptitem.Text = (dept.DepartmentCode + "-" + dept.DepartmentName);
                deptitem.Value = dept.RstDepartmentID.ToString();
                Departments.Add(deptitem);
            }
           

            List<SelectListItem> Locations = new List<SelectListItem>();
            SelectListItem defaultlocation = new SelectListItem();
            defaultlocation.Text = "-- Select A Location --";
            defaultlocation.Value = "0";
            Locations.Add(defaultlocation);
            var locations = _blllocation.GetActiveLocations(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            foreach (var loc in locations)
            {
                SelectListItem locitem = new SelectListItem();
                locitem.Text = (loc.LocationCode + "-" + loc.LocationName);
                locitem.Value = loc.SysLocationID.ToString();
                Locations.Add(locitem);
            }

            return Tuple.Create(Locations,Departments);
        }
    }
}