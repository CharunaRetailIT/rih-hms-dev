//using HospitalityManagement.Models;
//using HospitalityManagement.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using OfficeOpenXml;
using System.IO;
using System.Data;
using OfficeOpenXml.Style;
using System.Drawing;
namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class SupplierGroupController : Controller
    {

        BLL_SupplierGroup _bllsupplierGroup;
        BLL_Location _location; 
        public SupplierGroupController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllsupplierGroup = new BLL_SupplierGroup(cn);
            _location = new BLL_Location(cn);
        }

        [Authorize(Roles = "SupGroupCreatee")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Clear()
        {

            return View("Edit");
        }

        [Authorize(Roles = "SupGroupEdit")]
        public ActionResult Edit(long id)
        {
          
            var suppliergroup = _bllsupplierGroup.GetSupplierGroupById(id);
            ViewBag.SupplierGroupID = suppliergroup.SupplierGroupID;
            return View(suppliergroup);
        }

        [Authorize(Roles = "SupGroupEdit")]
        [HttpPost]
        public ActionResult Edit(SupplierGroup suppliergroups)
        {
          
            var suppliergroup = _bllsupplierGroup.GetSupplierGroupById(suppliergroups.SupplierGroupID);
            suppliergroup.SupplierGroupCode = suppliergroups.SupplierGroupCode;
            suppliergroup.SupplierGroupName = suppliergroups.SupplierGroupName;
            suppliergroup.Remark = suppliergroups.Remark;
            suppliergroup.IsDelete = suppliergroups.IsDelete;
            suppliergroup.ModifiedDate = DateTime.UtcNow;
            suppliergroup.ModifiedUser = Session["loggeduser"].ToString();
            suppliergroup.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            @ViewBag.SupGroupCode = suppliergroup.SupplierGroupCode;

            if (suppliergroup.IsDelete == true && _bllsupplierGroup.SupplierGroupIsUsing(suppliergroup.SupplierGroupID))
            {
                    ViewBag.Message = "3";
                    return View();
            }
            
            if (_bllsupplierGroup.UpdateSupplierGroup(suppliergroup) == 1)
            {

                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "2";
            }

            return View();
        }

        [Authorize(Roles = "SupGroupView")]
        public ActionResult ViewSupplierGroups()
        {
       
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var suppliergroups = _bllsupplierGroup.GetSupplierGroups(companyid).OrderBy(c => c.SupplierGroupCode);

            suppliergroups.ToList().ForEach(c =>
            {

                //    c.UserName = userreporsitory.GetSysUserMasterID(c.SysUserMasterID).GroupOfCompanyName;
            });

            return View(suppliergroups);
        }


        [Authorize(Roles = "SupGroupView")]
        public void HMSSupplierGroups()
        {

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var suppliergroups = _bllsupplierGroup.GetSupplierGroups(companyid).OrderBy(c => c.SupplierGroupCode);

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("SuplierGroups");
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

            Sheet.Cells[6, 2].Value = "Suplier Group Report";
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
            Sheet.Cells[8, 1].Value = "Supplier Group Code";
            Sheet.Cells[8, 2].Value = "Supplier Group Name";
            Sheet.Cells[8, 3].Value = "Remark";
            Sheet.Cells[8, 4].Value = "Is Delete";   

            //Sheet.Cells["A1"].Value = "SupplierGroupCode";
            //Sheet.Cells["B1"].Value = "SupplierGroupName";
            //Sheet.Cells["C1"].Value = "Remark";
            //Sheet.Cells["D1"].Value = "IsDelete";
           

            int row = 9;
            foreach (var item in suppliergroups)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.SupplierGroupCode;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.SupplierGroupName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.Remark;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.IsDelete;          
                row++;
            }
            #region Header Bold
            Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

            Sheet.Cells[8, 1, 8, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[8, 1, 8, 4].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

            var table = Sheet.Cells[8, 1, 8,4];
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
            Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSSupplierGroups.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                Ep.SaveAs(MyMemoryStream);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }



        }



        [Authorize(Roles = "SupGroupCreatee")]
        [HttpPost]
        public ActionResult Create(SupplierGroup suppliergroups)
        {
            suppliergroups.CreatedUser = Session["loggeduser"].ToString();
            suppliergroups.CreatedDate = DateTime.UtcNow;
            suppliergroups.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());

            if (!ModelState.IsValid)
            {
                return View("Index", suppliergroups);
            }

      

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            suppliergroups.CompanyID = companyid;
            var existssupgroup = _bllsupplierGroup.GetSupGroupByCode(suppliergroups.SupplierGroupCode,companyid);
            if (existssupgroup != null)
            {
                ViewBag.Message = "3";
                return View("Index", suppliergroups);
            }
            if (_bllsupplierGroup.SaveSupplierGroup(suppliergroups) == 1)
            {
                @ViewBag.Message = "1";
                //suppliergroups = null;
            }
            else
            {
                @ViewBag.Message = "2";
            }


            ViewBag.SupGroupCode = suppliergroups.SupplierGroupCode;
            return View("Index", suppliergroups);

        }


        [HttpGet]
        public JsonResult GetActiveSupplierGroups()
        {
          

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var suppliergroups = _bllsupplierGroup.GetSupplierGroups(companyid);
            return Json(JsonConvert.SerializeObject(suppliergroups, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

	}
}