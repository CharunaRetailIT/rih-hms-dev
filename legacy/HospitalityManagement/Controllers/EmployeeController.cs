//using HospitalityManagement.Models;
//using HospitalityManagement.Service;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.Domain;
using RIT.HMS.BLL.Common;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using RIT.HMS.BLL.Configurations;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class EmployeeController : Controller
    {
        private readonly BLL_Common _bllcommon;
        BLL_Employee _employee;
        BLL_Location _location;
        BLL_EmployeeGroup _employeeGroup;
        BLL_UserGroup _blluserGroup;
        BLL_Configuration _bllconfiguration;
        public EmployeeController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcommon = new BLL_Common(cn);
            _employee = new BLL_Employee(cn);
            _location = new BLL_Location(cn);
            _employeeGroup = new BLL_EmployeeGroup(cn);
            _blluserGroup = new BLL_UserGroup(cn);
            _bllconfiguration = new BLL_Configuration(cn);
        }


        // GET: /Employee/
        [Authorize(Roles = "EmpView")]
        public ActionResult ViewEmployees()
        {
            
           Session["DisableEmpoyeeMandoryField"] = _bllconfiguration.GetConfiguration("DisableEmpoyeeMandoryField", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var exists = _employee.GetEmployees(companyid);

            foreach (var x in exists)
            {
                if (Convert.ToBoolean(Session["DisableEmpoyeeMandoryField"]))
                    x.EnableEmpoyeeMandoryField = false;
                else
                    x.EnableEmpoyeeMandoryField = true;
            }


            try
            {
                exists.ToList().ForEach(c =>
               {
                   if (c.EpfNo == "")
                   {
                       c.EpfNo = "-";
                   }
                    
                   if(c.Email == "")
                   {
                       c.Email = "-";
                   }
               });
                return View(exists);
            }
             catch (NullReferenceException ex)
             {
                 return View(exists);
             }
        }

        [Authorize(Roles = "EmpView")]
        public void HMSEmployees()
        {

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var exists = _employee.GetEmployees(companyid);

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Employees");

            string compName, Address1, Address2, Address3, Tele, Fax, website, ReportHead = "";
            compName = _location.GetCompanyDetails().CompanyName;
            Address1 = _location.GetCompanyDetails().Address1;
            Address2 = _location.GetCompanyDetails().Address2;
            Address3 = _location.GetCompanyDetails().Address3;
            Tele = _location.GetCompanyDetails().Telephone;
            Fax = _location.GetCompanyDetails().Fax;
            website = _location.GetCompanyDetails().Website;


            #region

            Sheet.Cells[1, 2].Value = compName;
            Sheet.Cells[1, 2, 3, 12].Merge = true;
            Sheet.Cells[1, 2, 3, 12].Style.Font.Size = 12;
            Sheet.Cells[1, 2, 3, 12].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;

            Sheet.Cells[4, 2].Value = Address1 + " " + Address2 + " " + Address3;
            ;
            Sheet.Cells[4, 2, 4, 12].Merge = true;
            Sheet.Cells[4, 2, 4, 12].Style.Font.Size = 10;

            Sheet.Cells[5, 2].Value = "Tel:- " + Tele + " / " + ",  Fax:- " + Fax + ",  Web Site:- " + website;
            Sheet.Cells[5, 2, 5, 12].Merge = true;
            Sheet.Cells[5, 2, 5, 12].Style.Font.Size = 10;

            Sheet.Cells[6, 2].Value = "Employee Report";
            Sheet.Cells[6, 2, 6, 12].Merge = true;
            Sheet.Cells[6, 2, 6, 12].Style.Font.Size = 12;

            var businessUnitDetail = Sheet.Cells[1, 2, 6, 12];
            businessUnitDetail.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            businessUnitDetail.Style.Font.Bold = true;
            businessUnitDetail.Style.Font.Name = "Calibri";

            #endregion


            #region Print Detail Box

            var printDetailBox = Sheet.Cells[3, 13, 5, 14];
            printDetailBox.Style.Font.Size = 8;
            Sheet.Cells[3, 13, 5, 13].Style.Font.Bold = true;

            Sheet.Cells[3, 13].Value = "Date";
            Sheet.Cells[4, 13].Value = "Time";
            Sheet.Cells[5, 13].Value = "Req. By";

            Sheet.Cells[3, 14].Value = DateTime.Now.Date.ToShortDateString();
            Sheet.Cells[4, 14].Value = DateTime.Now.ToString("h:mm tt");

            if (Session["loggeduser"] != null)
                Sheet.Cells[5, 14].Value = Session["loggeduser"].ToString();
            else
                Sheet.Cells[5, 14].Value = " ";


            #endregion 

            Sheet.Cells[8, 1].Value = "Employee Code";
            Sheet.Cells[8, 2].Value = "EpfNo";
            Sheet.Cells[8, 3].Value = "Title";
            Sheet.Cells[8, 4].Value = "Name";
            Sheet.Cells[8, 5].Value = "Address";
            Sheet.Cells[8, 6].Value = "Email";
            Sheet.Cells[8, 7].Value = "NIC";
            Sheet.Cells[8, 8].Value = "Designation";
            Sheet.Cells[8, 9].Value = "Contact No";
            Sheet.Cells[8, 10].Value = "Is Active";

            int row = 9;
            foreach (var item in exists)
            {

                Sheet.Cells[row, 1].Value = item.EmployeeCode;
                Sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                if (item.EpfNo == "")
                {
                    Sheet.Cells[row, 2].Value = "-";
                    Sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                else
                {
                    Sheet.Cells[row, 2].Value = item.EpfNo;
                    Sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }

                Sheet.Cells[row, 3].Value = item.EmployeeTitle;
                Sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                Sheet.Cells[row, 4].Value = item.EmployeeName;
                Sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                Sheet.Cells[row, 5].Value = (item.Address1 + item.Address2 + item.Address3);
                Sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                if (item.Email == "null")
                {
                    Sheet.Cells[row, 6].Value = "-";
                    Sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                else
                {
                    Sheet.Cells[row, 6].Value = item.Email;
                    Sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                Sheet.Cells[row, 7].Value = item.NIC;
                Sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                Sheet.Cells[row, 8].Value = item.Designation;
                Sheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                Sheet.Cells[row, 9].Value = item.Mobile;
                Sheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                if (item.IsActive == true)
                {
                    Sheet.Cells[row, 10].Value = "Yes";
                    Sheet.Cells[row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                else
                {
                    Sheet.Cells[row, 10].Value = "No";
                    Sheet.Cells[row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
           
                row++;
            }
            #region
            Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

            Sheet.Cells[8, 1, 8, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[8, 1, 8, 10].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

            var table = Sheet.Cells[8, 1, 8, 10];
            table.Style.Border.Top.Style =
            table.Style.Border.Left.Style =
            table.Style.Border.Right.Style =
            table.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            table.Style.Font.Bold = true;
            table.Style.Font.Name = "Calibri";
            table.AutoFitColumns();

            #endregion


            //Sheet.Cells["A1"].Value = "EmployeeCode";
            //Sheet.Cells["B1"].Value = "EpfNo";
            //Sheet.Cells["C1"].Value = "Title";
            //Sheet.Cells["D1"].Value = "Name";
            //Sheet.Cells["E1"].Value = "Designation";
            //Sheet.Cells["F1"].Value = "ContactNo";
            //Sheet.Cells["G1"].Value = "IsActive";
           


            //int row = 2;
            //foreach (var item in exists)
            //{
            //    Sheet.Cells[string.Format("A{0}", row)].Value = item.EmployeeCode;
            //    Sheet.Cells[string.Format("B{0}", row)].Value = item.EpfNo;
            //    Sheet.Cells[string.Format("C{0}", row)].Value = item.EmployeeTitle;
            //    Sheet.Cells[string.Format("D{0}", row)].Value = item.EmployeeName;
            //    Sheet.Cells[string.Format("E{0}", row)].Value = item.Designation;
            //    Sheet.Cells[string.Format("F{0}", row)].Value = item.Telephone;
            //    Sheet.Cells[string.Format("G{0}", row)].Value = item.IsActive;
            

            //    row++;
            //}

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSEmployees.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }


        }

        [Authorize(Roles = "EmpCreatee")]
        public ActionResult Index()
        {
            Session["DisableEmpoyeeMandoryField"] = _bllconfiguration.GetConfiguration("DisableEmpoyeeMandoryField", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            Employee employee = new Employee();
            int docid = _bllcommon.GetDcumentId("Employee", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllcommon.GetDocumentNo("Employee", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid,false ,Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            employee.EmployeeCode = docnum;
            if (Convert.ToBoolean(Session["DisableEmpoyeeMandoryField"]))
                employee.EnableEmpoyeeMandoryField = false;
            else
                employee.EnableEmpoyeeMandoryField = true;

            employee.Address1 = "";
            employee.Address2 = "";
            employee.Address3 = "";
            employee.NIC = "";
            employee.Mobile = "";
            employee.EpfNo = "";
            employee.DOB =  Convert.ToDateTime("1999-09-29 00:00:00.000");

            var posusergroups = _blluserGroup.GetActivePOSUserGroups();
          


            @ViewBag.Designation = posusergroups.Select(X=>X.POSUserGroupId).First();
            return View("Index", employee);
        }

        [Authorize(Roles = "EmpEdit")]
        public ActionResult Edit(long id)
        {
            Session["DisableEmpoyeeMandoryField"] = _bllconfiguration.GetConfiguration("DisableEmpoyeeMandoryField", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            bool xer = Convert.ToBoolean(Session["DisableEmpoyeeMandoryField"]);
            var exists = _employee.GetEmployeeById(id);

            var posusergroups = _blluserGroup.GetActivePOSUserGroups();
            var Designation = posusergroups.Where(x => x.POSUserGroupName == exists.Designation).FirstOrDefault();

            if (Designation!=null)
            {
                exists.Designation = Designation.POSUserGroupId.ToString();
            }
            

            if (Convert.ToBoolean(Session["DisableEmpoyeeMandoryField"]))
                exists.EnableEmpoyeeMandoryField = false;
            else
                exists.EnableEmpoyeeMandoryField = true;







            @ViewBag.FileName = exists.EmployeePictureName;
            @ViewBag.DepartmentID = exists.DepartmentID;
            @ViewBag.EmployeeGroupID = exists.EmployeeGroupID;
            @ViewBag.LocationId = exists.LocationId;
            @ViewBag.Designation = exists.Designation;


            if (exists.EmployeeTitle == "Mr")
            {
                @ViewBag.sel = "0";
            }
            else if (exists.EmployeeTitle == "Mrs")
            {
                @ViewBag.sel = "1";
            }
            else if (exists.EmployeeTitle == "Ms")
            {
                @ViewBag.sel = "2";
            }
            else if (exists.EmployeeTitle == "Miss")
            {
                @ViewBag.sel = "3";
            }
            else if (exists.EmployeeTitle == "Dr")
            {
                @ViewBag.sel = "4";
            }
            else if (exists.EmployeeTitle == "Rev")
            {
                @ViewBag.sel = "5";
            }

            return View(exists);
        }

        [Authorize(Roles = "EmpEdit")]
        [HttpPost]
        public ActionResult Edit(Employee emp)
        {
            Session["DisableEmpoyeeMandoryField"] = _bllconfiguration.GetConfiguration("DisableEmpoyeeMandoryField", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            @ViewBag.DepartmentID = emp.DepartmentID;
            @ViewBag.LocationId = emp.LocationId;
            @ViewBag.EmployeeGroupID = emp.EmployeeGroupID;
            @ViewBag.sel = emp.EmployeeTitle;
            @ViewBag.Designation = emp.Designation;

            

            var posusergroups = _blluserGroup.GetActivePOSUserGroups();
            var Designation = posusergroups.Where(x => x.POSUserGroupId == int.Parse(emp.Designation)).FirstOrDefault();

           






            var exists = _employee.GetEmployeeById(emp.EmployeeID);
            if(Convert.ToBoolean(Session["DisableEmpoyeeMandoryField"]))
            exists.EnableEmpoyeeMandoryField =   false;
            else
                exists.EnableEmpoyeeMandoryField = true;

            exists.EmployeeName = emp.EmployeeName;
            exists.Address1 = emp.Address1 ?? "";
            exists.Address2 = emp.Address2 ?? "";
            exists.Address3 = emp.Address3 ?? "";
            exists.DOB = emp.DOB;
            exists.Designation = Designation.POSUserGroupName;
            exists.LocationId = emp.LocationId;
            exists.DepartmentID = emp.DepartmentID;
            exists.EmployeeGroupID = emp.EmployeeGroupID;
            exists.NIC = emp.NIC ?? "";
            exists.Passport = emp.Passport;
            exists.EmployeeTitle = emp.EmployeeTitle;
            exists.Telephone = emp.Telephone;
            exists.Mobile = emp.Mobile ?? "";
            exists.Email = emp.Email;
            exists.IsActive = emp.IsActive;
            exists.IsDelete = emp.IsDelete;
            exists.ModifiedUser = Session["loggeduser"].ToString();
            exists.ModifiedDate = DateTime.Now;
           
            exists.EpfNo = emp.EpfNo ?? "";
           

            exists.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (emp.LocationId == 0)
            {
                ModelState.AddModelError("LocationId", "Please select Location !");
                return View(emp);
            }
            if (emp.EnableEmpoyeeMandoryField &&_employee.CheckEpfNo(emp.EpfNo, emp.EmployeeCode) == 0)
            {
                ModelState.AddModelError("EpfNo", "EPF No already exsists !");
                return View(emp);
            }
            if (emp.IsActive == true && emp.IsDelete == true)
            {
                @ViewBag.Message = "4";
                return View(emp);
            }
            if (emp.Photograph != null)
            {
                byte[] newlogo;
                using (BinaryReader br = new BinaryReader(emp.Photograph.InputStream))
                {
                    newlogo = br.ReadBytes(emp.Photograph.ContentLength);
                    emp.EmployeePicture = newlogo;
                    emp.EmployeePictureName = emp.Photograph.FileName;
                    emp.EmployeePictureType = emp.Photograph.ContentType;
                }

                if (emp.EmployeePictureName != exists.EmployeePictureName)
                {
                    byte[] pic;
                    using (BinaryReader br = new BinaryReader(emp.Photograph.InputStream))
                    {
                        pic = br.ReadBytes(emp.Photograph.ContentLength);
                        exists.EmployeePicture = pic;
                        exists.EmployeePictureName = emp.Photograph.FileName;
                        exists.EmployeePictureType = emp.Photograph.ContentType;
                    }
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var err in errors)
            {
                var e = err;
            }
            if (_employee.UpdateEmployee(exists) == 1)
            {
                @ViewBag.Message = "1";
            }
            else
            {
                @ViewBag.Message = "2";
            }
            @ViewBag.CustCode = exists.EmployeeCode;
            return View(emp);
        }

        [Authorize(Roles = "EmpCreatee")]
        [HttpPost]
        public ActionResult Create(Employee emp)
        {
            Session["DisableEmpoyeeMandoryField"] = _bllconfiguration.GetConfiguration("DisableEmpoyeeMandoryField", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            if (Convert.ToBoolean(Session["DisableEmpoyeeMandoryField"]))
                emp.EnableEmpoyeeMandoryField = false;
            else
                emp.EnableEmpoyeeMandoryField = true;


            @ViewBag.DepartmentID = emp.DepartmentID;
            @ViewBag.EmployeeGroupID = emp.EmployeeGroupID;
            @ViewBag.LocationId = emp.LocationId;
            Common common = new Common();
            if (emp.Photograph != null && common.CheckImageType(emp.Photograph.ContentType) == false)
            {
                ModelState.AddModelError("Photograph", "Only an Image required !");
                return View("Index", emp);
            }
            emp.CreatedUser = Session["loggeduser"].ToString();
            emp.CreatedDate = DateTime.Now;
            //   emp.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());

            //Added by pavi on 2019-12-02
            emp.IsActive = true;

            emp.Address1 = emp.Address1 ?? "";
            emp.Address2 = emp.Address2 ?? "";
            emp.Address3 = emp.Address3 ?? "";
            emp.NIC = emp.NIC ?? "";
            emp.Mobile = emp.Mobile ?? "";
            emp.EpfNo = emp.EpfNo ?? "";


            //emp.DataTransfer = 1;
            if (!ModelState.IsValid)
            {

                var errors = ModelState.Values.SelectMany(v => v.Errors);

                // Log errors or process them
                foreach (var error in errors)
                {
                    // Error.ErrorMessage contains the error message
                    Console.WriteLine(error.ErrorMessage);
                }


                return View("Index", emp);
            }
            if (emp.LocationId == 0)
            {
                ModelState.AddModelError("LocationId", "Please select Location !");
                return View("Index", emp);
            }
            if ( emp.EnableEmpoyeeMandoryField &&   _employee.CheckEpfNo(emp.EpfNo, emp.EmployeeCode) == 0)
            {
                ModelState.AddModelError("EpfNo", "Please enter valid EPF No !");
                return View("Index", emp);
            }
            if (emp.Photograph != null)
            {
                byte[] photo;
                using (BinaryReader br = new BinaryReader(emp.Photograph.InputStream))
                {
                    photo = br.ReadBytes(emp.Photograph.ContentLength);
                    emp.EmployeePicture = photo;
                    emp.EmployeePictureName = emp.Photograph.FileName;
                    emp.EmployeePictureType = emp.Photograph.ContentType;
                }
            }

            




            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            emp.CompanyID = companyid;
            var existsemp = _employee.GetEmployeeByCode(emp.EmployeeCode, companyid);
            if (existsemp != null)
            {
                ViewBag.Message = "3";
                return View("Index");
            }
            var posusergroups = _blluserGroup.GetActivePOSUserGroups();
            var Designation = posusergroups.Where(x => x.POSUserGroupId == int.Parse(emp.Designation)).FirstOrDefault();
            if(Designation!=null)
            emp.Designation = Designation.POSUserGroupName;

            if (_employee.SaveEmployee(emp) == 1)
            {
                ModelState.Clear();
                ViewBag.Message = "1";
                @ViewBag.DepartmentID = "0";
                @ViewBag.EmployeeGroupID = "0";
                Employee newEmployee = new Employee();
                int docid = _bllcommon.GetDcumentId("Employee", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentNo("Employee", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                newEmployee.EmployeeCode = docnum;
                return View("Index", newEmployee);
            }
            else
            {
                ViewBag.Message = "2"; ViewBag.EmpCode = emp.EmployeeCode;
                return View("Index");
            }
        }

        [HttpPost]
        public ActionResult Gender(Employee gender)
        {
            return View(gender);
        }

        [HttpGet]
        public JsonResult GetActiveEmployees()
        {
            Session["DisableEmpoyeeMandoryField"] = _bllconfiguration.GetConfiguration("DisableEmpoyeeMandoryField", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var employees = _employee.GetActiveEmployees(companyid);
            return Json(JsonConvert.SerializeObject(employees, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }
    }
}