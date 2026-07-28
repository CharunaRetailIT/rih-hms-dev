using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.Domain;
using RIT.HMS.BLL.MasterData;
using OfficeOpenXml;
using System.IO;
using System.Data;
using OfficeOpenXml.Style;
using System.Drawing;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]  

    public class ServingUnitController : Controller
    {
        // GET: ServingUnit
        BLL_ServingUnit _bllServingUnits;
        BLL_Location _location; 

        public ServingUnitController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllServingUnits = new BLL_ServingUnit(cn);
            _location = new BLL_Location(cn);
        }

        public ActionResult Create()
        {

            return View();
        }

        [Authorize(Roles = "ServingUnits")]
        public ActionResult Index()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            return View(_bllServingUnits.GetAllServingUnits(companyid));
        }

        [Authorize(Roles = "ServingUnits")]
        public void HMSServingUnits()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var servingunits = _bllServingUnits.GetAllServingUnits(companyid);

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("ServingUnits");
            //---------------2023/03/27 ----------Tharaka---------------
            string compName, Address1, Address2, Address3, Tele, Fax, website = "";
            compName = _location.GetCompanyDetails().CompanyName;
            Address1 = _location.GetCompanyDetails().Address1;
            Address2 = _location.GetCompanyDetails().Address2;
            Address3 = _location.GetCompanyDetails().Address3;
            Tele = _location.GetCompanyDetails().Telephone;
            Fax = _location.GetCompanyDetails().Fax;
            website = _location.GetCompanyDetails().Website;
            #region Headings

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

            Sheet.Cells[6, 2].Value = "Serving Unit Report";
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
            Sheet.Cells[8, 1].Value = "Serving Unit";
            Sheet.Cells[8, 2].Value = "Is Active";
            
            //Sheet.Cells["A1"].Value = "ServingUnit";
            //Sheet.Cells["B1"].Value = "IsActive";    
            int row = 9;
            foreach (var item in servingunits)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.ServingUnitName;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.IsActive;
                row++;
            }
            #region Header Bold
            Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");
            Sheet.Cells[8, 1, 8, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[8, 1, 8, 2].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);
            var table = Sheet.Cells[8, 1, 8, 2];
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
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSServingUnits.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

        }
        [Authorize(Roles = "ServingUnits")]
        public ActionResult Edit(long id)
        {
            //  CompanyService companyreporsitory = new CompanyService();
            var servingunit = _bllServingUnits.GetServingUnitById(id);
            ViewBag.PaymentMethodId = servingunit.ServingUnitId;
            return View(servingunit);
        }

        [Authorize(Roles = "ServingUnits")]
        [HttpPost]
        public ActionResult Edit(ServingUnit servingunit)
        {
            servingunit.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            servingunit.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            if (!ModelState.IsValid)
            {
                @ViewBag.PaymentMethodId = servingunit.ServingUnitId;
                return View(servingunit);

            }
            if (servingunit.IsActive == true && servingunit.IsDelete == true)
            {
                @ViewBag.PaymentMethodId = servingunit.ServingUnitId;
                @ViewBag.Message = "4";
                return View(servingunit);
            }

            @ViewBag.PaymentMethodId = servingunit.ServingUnitId;

            if (_bllServingUnits.UpdateServingUnit(servingunit) == 1)
            {
                ViewBag.Message = "1";
                return View(new ServingUnit());
            }
            else
            {
                @ViewBag.PaymentMethodId = servingunit.ServingUnitId;
                ViewBag.Message = "0";
                return View(servingunit);
            }
        }

        [Authorize(Roles = "ServingUnits")]
        [HttpPost]
        public ActionResult Create(ServingUnit servingunit)
        {
            servingunit.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            servingunit.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            if (!ModelState.IsValid)
            {
                @ViewBag.PaymentMethodId = servingunit.ServingUnitId;
                return View("Create", servingunit);
            }
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var existscom = _bllServingUnits.GetServingUnitByName(servingunit.ServingUnitName,companyid);
            if (existscom != null)
            {
                ViewBag.Message = "3";
                @ViewBag.PaymentMethodId = servingunit.ServingUnitId;
                return View("Create", servingunit);
            }

     ////       ViewBag.PaymentMethodCode = servingunit.PaymentMethodCode;

            if (_bllServingUnits.SaveServingUnit(servingunit) == 1)
            {
                ViewBag.Message = "1";
                return View("Create", new ServingUnit());
            }
            else
            {
                @ViewBag.PaymentMethodId = servingunit.ServingUnitId;
                ViewBag.Message = "2";
                return View("Create", servingunit);
            }

        }
    }
}