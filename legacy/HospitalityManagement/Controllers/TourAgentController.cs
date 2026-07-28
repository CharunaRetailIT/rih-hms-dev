using RIT.HMS.BLL.MasterData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.Domain;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using Newtonsoft.Json;
using RIT.HMS.BLL.Configurations;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class TourAgentController : Controller
    {
        BLL_TourAgent _TourAgent;
        BLL_Location _location;
        private AppManager _appmanager;
        private readonly BLL_Configuration _bllconfiguration;
        public TourAgentController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _TourAgent = new BLL_TourAgent(cn);
            _location = new BLL_Location(cn);
            _appmanager = new AppManager(cn);
            _bllconfiguration = new BLL_Configuration(cn);

        }

        // GET: TourAgent
     //   [Authorize(Roles = "TourAgentView")]
        public ActionResult ViewTourAgents()
        {

            int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var exists = _TourAgent.GetTourAgents(compayid);
            return View(exists);
        }
      //  [Authorize(Roles = "TourAgentCreate")]
        public ActionResult Create()
        {

            @ViewBag.TourAgentStatus = "Other";
            return View(new TourAgent());
        }

     //   [Authorize(Roles = "TourAgentCreate")]
        [HttpPost]
        public ActionResult Create(TourAgent touragt)
        {
            touragt.CreatedUser = Session["loggeduser"].ToString();
            touragt.CreatedDate = DateTime.Now;
            touragt.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            touragt.DataTransfer = 1;
            touragt.ModifiedUser = Session["loggeduser"].ToString();
            touragt.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var existscust = _TourAgent.GetTourAgentByCode(touragt.AgentCode, companyid);
            ViewBag.AgentTCode = touragt.AgentCode;

            if (existscust != null)
            {
                ViewBag.Message = "3";
                return View(touragt);
            }

            if (_TourAgent.SaveTourAgent(touragt) != 0)
            {
                ViewBag.Message = "1";
                ModelState.Clear();
                return View(new TourAgent());
            }
            else
            {
                @ViewBag.TourAgentCompanyID = touragt.TourAgentCompanyID;
                ViewBag.Message = "2";
                return View(touragt);
            }
            
        }

      //  [Authorize(Roles = "TourAgentEdit")]
        [HttpPost]
        public ActionResult Edit(TourAgent touragt)
        {
            @ViewBag.TourAgentCompanyID = touragt.TourAgentCompanyID;
            if (touragt.IsActive == false)
            {
                @ViewBag.TourAgentCompanyID = touragt.TourAgentCompanyID;
                @ViewBag.Message = "4";
                return View(touragt);
            }

            var exists = _TourAgent.GetTourAgentById(touragt.TourAgentID);

            exists.TourAgentName = touragt.TourAgentName;
            exists.BillingAddress1 = touragt.BillingAddress1;
            exists.BillingAddress2 = touragt.BillingAddress2;
            exists.BillingAddress3 = touragt.BillingAddress3;
            exists.TourAgentTitle = touragt.TourAgentTitle;
            exists.NIC = touragt.NIC;
            exists.Mobile = touragt.Mobile;
            exists.Email = touragt.Email;
            exists.IsActive = touragt.IsActive;
            exists.ModifiedDate = DateTime.Now;
            exists.ModifiedUser = Session["loggeduser"].ToString();
            exists.TourPercentage = touragt.TourPercentage;
            exists.TourAmount = touragt.TourAmount;
            exists.Remarks = touragt.Remarks;
            exists.TourAgentCompanyCode = touragt.TourAgentCompanyCode;  // HAVE TO CHECK IT MORE
            exists.TourAgentCompanyID = touragt.TourAgentCompanyID;
            exists.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (_TourAgent.UpdateTourAgent(exists) > 0)
            {
                @ViewBag.Message = "1";
            }
            else
            {
                @ViewBag.Message = "2";
            }
            @ViewBag.TourAgentCode = exists.AgentCode;
       

            return View(touragt);

        }

     //   [Authorize(Roles = "TourAgentEdit")]
        public ActionResult Edit(long id)
        {
            
            var exists = _TourAgent.GetTourAgentById(id);
            @ViewBag.TourAgentCompanyID = exists.TourAgentCompanyID;

            if (exists.TourAgentTitle == "Mr")
            {
                @ViewBag.sel = "0";
            }
            else if (exists.TourAgentTitle == "Mrs")
            {
                @ViewBag.sel = "1";
            }
            else if (exists.TourAgentTitle == "Ms")
            {
                @ViewBag.sel = "2";
            }
            else if (exists.TourAgentTitle == "Miss")
            {
                @ViewBag.sel = "3";
            }
            else if (exists.TourAgentTitle == "Dr")
            {
                @ViewBag.sel = "4";
            }
            else if (exists.TourAgentTitle == "Rev")
            {
                @ViewBag.sel = "5";
            }
      
            return View(exists);
        }

       // [Authorize(Roles = "TourAgentView")]
        public void HMSTourAgent()
        {
            using (ExcelPackage pck = new ExcelPackage())
            {

                int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                var exists = _TourAgent.GetTourAgents(compayid);


                ExcelPackage Ep = new ExcelPackage();
                ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("TourAgent");

                string compName, Address1, Address2, Address3, Tele, Fax, website;
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

                Sheet.Cells[6, 2].Value = "Customer Report";
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

                Sheet.Cells[8, 1].Value = "Tour Agent Code";
                Sheet.Cells[8, 2].Value = "Tour Agent Title";
                Sheet.Cells[8, 3].Value = "Tour Agent Name";
                Sheet.Cells[8, 4].Value = "Tour Agent Address";
                Sheet.Cells[8, 5].Value = "Contact No";
                Sheet.Cells[8, 6].Value = "Email";
                Sheet.Cells[8, 7].Value = "Amount";
                Sheet.Cells[8, 8].Value = "Percentage";
                Sheet.Cells[8, 9].Value = "Is Active";

                int row = 9;
                foreach (var item in exists)
                {

                    Sheet.Cells[row, 1].Value = item.AgentCode;
                    Sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 2].Value = item.TourAgentTitle;
                    Sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 3].Value = item.TourAgentName;
                    Sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 4].Value = (item.BillingAddress1 + item.BillingAddress2 + item.BillingAddress3);
                    Sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 5].Value = item.Mobile;
                    Sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 6].Value = item.Email;
                    Sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    Sheet.Cells[row, 7].Value = item.TourAmount;
                    Sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 8].Value = item.TourPercentage;
                    Sheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    if (item.IsActive == true)
                    {
                        Sheet.Cells[row, 9].Value = "Yes";
                        Sheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    else
                    {
                        Sheet.Cells[row, 9].Value = "No";
                        Sheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                 
                    row++;
                }


                #region
                System.Drawing.Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

                Sheet.Cells[8, 1, 8, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells[8, 1, 8, 9].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                var table = Sheet.Cells[8, 1, 8, 9];
                table.Style.Border.Top.Style =
                table.Style.Border.Left.Style =
                table.Style.Border.Right.Style =
                table.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                table.Style.Font.Bold = true;
                table.Style.Font.Name = "Calibri";
                table.AutoFitColumns();

                #endregion

                Sheet.Cells["A:AZ"].AutoFitColumns();
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSTourAgent.xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    Ep.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        [HttpGet]
        public JsonResult GetActiveTourAgentCompany()
        {

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var TourAgCom = _TourAgent.GetActiveTourAgentCompany(companyid);
            return Json(JsonConvert.SerializeObject(TourAgCom, Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
            JsonRequestBehavior.AllowGet);
        }
    }
}