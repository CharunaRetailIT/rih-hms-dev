using OfficeOpenXml;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml.Style;
using System.IO;
namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class TourAgentCompanyController : Controller
    {
        BLL_TourAgentCompany _TourAgentCompany;
        BLL_Location _location;
        public TourAgentCompanyController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _TourAgentCompany = new BLL_TourAgentCompany(cn);
            _location = new BLL_Location(cn);
        }
        // GET: TourAgentCompany
      //  [Authorize(Roles = "TourAgentComView")]
        public ActionResult ViewTourAgentCompany()
        {

            int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var exists = _TourAgentCompany.GetTourAgentCompany(compayid);
            return View(exists);
        }

    //    [Authorize(Roles = "TourAgentComCreate")]
        public ActionResult Create()
        {

            @ViewBag.TourAgentComStatus = "Other";
            return View(new TourAgentCompany());
        }

     //   [Authorize(Roles = "TourAgentComCreate")]
        [HttpPost]
        public ActionResult Create(TourAgentCompany touragtcomp)
        {
            touragtcomp.CreatedUser = Session["loggeduser"].ToString();
            touragtcomp.CreatedDate = DateTime.Now;
            touragtcomp.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            touragtcomp.DataTransfer = 1;
            touragtcomp.ModifiedUser = Session["loggeduser"].ToString();
            touragtcomp.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var existscust = _TourAgentCompany.GetTourAgentCompanyByCode(touragtcomp.TourAgentCompanyCode, companyid);
            ViewBag.AgentTCode = touragtcomp.TourAgentCompanyCode;

            if (existscust != null)
            {
                ViewBag.Message = "3";
                return View(touragtcomp);
            }

            if (_TourAgentCompany.SaveTourAgentCompany(touragtcomp) != 0)
            {
                ViewBag.Message = "1";
                ModelState.Clear();
                return View(new TourAgentCompany());
            }
            else
            {
                ViewBag.Message = "2";
                return View(touragtcomp);
            }
        }


     //   [Authorize(Roles = "TourAgentComEdit")]
        [HttpPost]
        public ActionResult Edit(TourAgentCompany touragtCom)
        {
            if (touragtCom.IsActive == false)
            {
                @ViewBag.Message = "4";
                return View(touragtCom);
            }

            var exists = _TourAgentCompany.GetTourAgentCompanyById(touragtCom.TourAgentCompanyID);

            exists.TourAgentCompanyName = touragtCom.TourAgentCompanyName;
            exists.Address1 = touragtCom.Address1;
            exists.Address2 = touragtCom.Address2;
            exists.Mobile = touragtCom.Mobile;
            exists.Telephone = touragtCom.Telephone;
            exists.FaxNo = touragtCom.FaxNo;
            exists.Email = touragtCom.Email;
            exists.WebAddress = touragtCom.WebAddress;
            exists.ContactPerson = touragtCom.ContactPerson;
            exists.CommissionAmount = touragtCom.CommissionAmount;
            exists.IsActive = touragtCom.IsActive;
            exists.ModifiedDate = DateTime.Now;
            exists.ModifiedUser = Session["loggeduser"].ToString();
            exists.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            exists.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());

            if (_TourAgentCompany.UpdateTourAgentCompany(exists) > 0)
            {
                @ViewBag.Message = "1";
            }
            else
            {
                @ViewBag.Message = "2";
            }
            @ViewBag.TourAgentCompanyCode = exists.TourAgentCompanyCode;


            return View(touragtCom);

        }

     //   [Authorize(Roles = "TourAgentComEdit")]
        public ActionResult Edit(long id)
        {

            var exists = _TourAgentCompany.GetTourAgentCompanyById(id);

            return View(exists);
        }
     //   [Authorize(Roles = "TourAgentComView")]
        public void HMSTourAgentCompany()
        {
            using (ExcelPackage pck = new ExcelPackage())
            {

                int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                var exists = _TourAgentCompany.GetTourAgentCompany(compayid);


                ExcelPackage Ep = new ExcelPackage();
                ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("TourAgentCompany");

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

                Sheet.Cells[8, 1].Value = "Tour Agent Company Code";
                Sheet.Cells[8, 2].Value = "Tour Agent Company Name";
                Sheet.Cells[8, 3].Value = "Tour Agent Company Address";
                Sheet.Cells[8, 4].Value = "Contact No";
                Sheet.Cells[8, 5].Value = "Email";
                Sheet.Cells[8, 6].Value = "Amount";
                Sheet.Cells[8, 7].Value = "Is Active";

                int row = 9;
                foreach (var item in exists)
                {

                    Sheet.Cells[row, 1].Value = item.TourAgentCompanyCode;
                    Sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 2].Value = item.TourAgentCompanyName;
                    Sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 3].Value = (item.Address1 + item.Address2);
                    Sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 4].Value = item.Mobile;
                    Sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 5].Value = item.Email;
                    Sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    Sheet.Cells[row, 6].Value = item.CommissionAmount;
                    Sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    if (item.IsActive == true)
                    {
                        Sheet.Cells[row, 7].Value = "Yes";
                        Sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    else
                    {
                        Sheet.Cells[row, 7].Value = "No";
                        Sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }

                    row++;
                }


                #region
                System.Drawing.Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

                Sheet.Cells[8, 1, 8, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells[8, 1, 8, 7].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                var table = Sheet.Cells[8, 1, 8, 7];
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
                Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSTourAgentCompany.xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    Ep.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }
    }
}