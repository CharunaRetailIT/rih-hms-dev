
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using RIT.HMS.BLL.Common;
using RIT.HMS.Domain.Common;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class SupplierController : Controller
    {
        private readonly BLL_Common _bllcommon;
        BLL_Supplier _bllsupplier;
        BLL_SupplierGroup _bllsupplierGroup;
        BLL_Location _location; 
        public SupplierController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcommon = new BLL_Common(cn);
            _bllsupplier = new BLL_Supplier(cn);
            _bllsupplierGroup = new BLL_SupplierGroup(cn);
            _location = new BLL_Location(cn);
        }

        // GET: /Supplier/
        [Authorize(Roles = "SupView")]
        public ActionResult ViewSuppliers()
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var exists = _bllsupplier.GetSuppliers(companyid);
            try
            {
                exists.ToList().ForEach(c =>
                {
                    if (c.SupplierGroupID == 0)
                    {
                        c.Remark = "";
                    }
                    else
                    {
                        c.Remark = _bllsupplierGroup.GetSupplierGroupById(c.SupplierGroupID).SupplierGroupName;
                    }

                    if (c.SupplierTypeID == 0)
                    {
                        c.SuppliedProducts = "";
                    }
                    else
                    {
                        c.SuppliedProducts = _bllsupplier.GetSupplierTypesById(c.SupplierTypeID).SupplierTypeName;
                    }
                    if(c.SupplierTitle == "0")
                    {
                        c.SupplierTitle = "-";
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

        [Authorize(Roles = "SupView")]
        public void HMSSupplier()
        {

            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var exists = _bllsupplier.GetSuppliers(companyid);

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Suppliers");

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

            Sheet.Cells[6, 2].Value = "Supplier Report";
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

            Sheet.Cells[8, 1].Value = "Supplier Code";
            Sheet.Cells[8, 2].Value = "Supplier Title";
            Sheet.Cells[8, 3].Value = "Supplier Name";
            Sheet.Cells[8, 4].Value = "Contact No";
            Sheet.Cells[8, 5].Value = "Email";
            Sheet.Cells[8, 6].Value = "Credit Limit";
            Sheet.Cells[8, 7].Value = "Cheque Limit";
            Sheet.Cells[8, 8].Value = "Is Active";
            Sheet.Cells[8, 9].Value = "Supplier Type";
            Sheet.Cells[8, 10].Value = "Supplier Group";


            try
            {
                exists.ToList().ForEach(c =>
                {
                    if (c.SupplierGroupID == 0)
                    {
                        c.Remark = "";
                    }
                    else
                    {
                        c.Remark = _bllsupplierGroup.GetSupplierGroupById(c.SupplierGroupID).SupplierGroupName;
                    }

                    if (c.SupplierTypeID == 0)
                    {
                        c.SuppliedProducts = "";
                    }
                    else
                    {
                        c.SuppliedProducts = _bllsupplier.GetSupplierTypesById(c.SupplierTypeID).SupplierTypeName;
                    }
                    if (c.SupplierTitle == "0")
                    {
                        c.SupplierTitle = "-";
                    }
                    if (c.Email == "")
                    {
                        c.Email = "-";
                    }
                });




                int row = 9;
                foreach (var item in exists)
                {

                    Sheet.Cells[row, 1].Value = item.SupplierCode;
                    Sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 2].Value = item.SupplierTitle;
                    Sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 3].Value = item.SupplierName;
                    Sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 4].Value = item.BillingTelephone;
                    Sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 5].Value = item.Email;
                    Sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 6].Value = item.CreditLimit;
                    Sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 7].Value = item.ChequeLimit;
                    Sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    if (item.IsBlocked == true)
                    {
                        Sheet.Cells[row, 8].Value = "Yes";
                        Sheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    else
                    {
                        Sheet.Cells[row, 8].Value = "No";
                        Sheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    Sheet.Cells[row, 9].Value = item.SuppliedProducts;
                    Sheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 10].Value = item.Remark;
                    Sheet.Cells[row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

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

                //ExcelPackage Ep = new ExcelPackage();
                //ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("SuplierGroups");
                //Sheet.Cells["A1"].Value = "SupplierCode";
                //Sheet.Cells["B1"].Value = "SupplierName";
                //Sheet.Cells["C1"].Value = "Type";
                //Sheet.Cells["D1"].Value = "Group";


                //int row = 2;
                //foreach (var item in exists)
                //{
                //    Sheet.Cells[string.Format("A{0}", row)].Value = item.SupplierCode;
                //    Sheet.Cells[string.Format("B{0}", row)].Value = item.SupplierName;
                //    Sheet.Cells[string.Format("C{0}", row)].Value = item.SuppliedProducts;
                //    Sheet.Cells[string.Format("D{0}", row)].Value = item.Remark;


                //    row++;
                //}

                Sheet.Cells["A:AZ"].AutoFitColumns();
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSSupplier.xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    Ep.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }




            }
            catch (NullReferenceException)
            {
                
            }
        }

        [Authorize(Roles = "SupCreatee")]
        public ActionResult Index()
        {
            Supplier sup = new Supplier();
            int docid = _bllcommon.GetDcumentId("Supplier", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
             var docnum = _bllcommon.GetDocumentNumber("Supplier", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
          //  var docnum = _bllcommon.GetDocumentNo("Supplier", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid,false);
            sup.SupplierCode = docnum;

            @ViewBag.sel = "0";
            return View("Index", sup);
        }

        [Authorize(Roles = "SupEdit")]
        public ActionResult Edit(long id)
        {
           
            var exists = _bllsupplier.GetSupplierById(id);
            @ViewBag.SupplierTypeID = exists.SupplierTypeID;
            @ViewBag.FileName = exists.SupplierPictureName;
            @ViewBag.SupplierGroupID = exists.SupplierGroupID;
            @ViewBag.PaymenttermId = exists.PaymentTermID;
            @ViewBag.PaymentMethodId = exists.PaymentMethod;
            @ViewBag.sel = "0";
            if (exists.SupplierTitle == "Mr")
            {
                @ViewBag.sel = "1";
            }
            else if (exists.SupplierTitle == "Mrs")
            {
                @ViewBag.sel = "2";
            }
            else if (exists.SupplierTitle == "Ms")
            {
                @ViewBag.sel = "3";
            }
            else if (exists.SupplierTitle == "Miss")
            {
                @ViewBag.sel = "4";
            }
            else if (exists.SupplierTitle == "Dr")
            {
                @ViewBag.sel = "5";
            }
            else if (exists.SupplierTitle == "Rev")
            {
                @ViewBag.sel = "6";
            }
            else if (exists.SupplierTitle == "Company")
            {
                @ViewBag.sel = "7";
            }

            return View(exists);
        }
        [Authorize(Roles = "SupEdit")]
        [HttpPost]
        public ActionResult Edit(Supplier sup)
        {
           

            @ViewBag.SupplierTypeID = sup.SupplierTypeID;
            @ViewBag.FileName = sup.SupplierPictureName;
            @ViewBag.SupplierGroupID = sup.SupplierGroupID;
            @ViewBag.PaymenttermId = sup.PaymentTermID;
            @ViewBag.PaymentMethodId = sup.PaymentMethod;
            @ViewBag.sel = "0";
            if (sup.SupplierTitle == "Mr")
            {
                @ViewBag.sel = "1";
            }
            else if (sup.SupplierTitle == "Mrs")
            {
                @ViewBag.sel = "2";
            }
            else if (sup.SupplierTitle == "Ms")
            {
                @ViewBag.sel = "3";
            }
            else if (sup.SupplierTitle == "Miss")
            {
                @ViewBag.sel = "4";
            }
            else if (sup.SupplierTitle == "Dr")
            {
                @ViewBag.sel = "5";
            }
            else if (sup.SupplierTitle == "Rev")
            {
                @ViewBag.sel = "6";
            }
            else if (sup.SupplierTitle == "Company")
            {
                @ViewBag.sel = "7";
            }
            //Added by pavi on 2019-12-01 
            /*------------------Start-------------------------*/
            if (!ModelState.IsValid)
            {
                //@ViewBag.SupplierGroupID = sup.SupplierGroupID;
                //@ViewBag.SupplierCode = sup.SupplierCode;

                return View(sup);
            }

            //@ViewBag.SupplierGroupID = sup.SupplierGroupID;
            //@ViewBag.SupplierCode = sup.SupplierCode;

            /*------------------End-------------------------*/
            sup.ModifiedUser = Session["loggeduser"].ToString();
            sup.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (_bllsupplier.UpdateSupplier(sup) != 0)
            {
                @ViewBag.Message = "1";
            }
            else
            {
                @ViewBag.Message = "2";
            }

            //Commented by pavi (There is no any place using Viewbag.CustCode)
            //@ViewBag.CustCode = sup.SupplierCode;

            return View(sup);
        }

        [Authorize(Roles = "SupCreatee")]
        [HttpPost]
        public ActionResult Create(Supplier sup)
        {
            sup.CreatedUser = Session["loggeduser"].ToString();
            sup.CreatedDate = DateTime.Now;
            sup.DataTransfer = 1;
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            sup.CompanyID = companyid;
            sup.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());

            if (!ModelState.IsValid)
            {
                ViewBag.SupplierTypeID = sup.SupplierID;
                ViewBag.SupplierGroupID = sup.SupplierGroupID;
                ViewBag.PaymentMethodId = sup.PaymentMethod;
                ViewBag.PaymenttermId = sup.PaymentTermID;
                ViewBag.SupplierTypeID = sup.SupplierTypeID;
                @ViewBag.sel = "0";
                if (sup.SupplierTitle == "Mr")
                {
                    @ViewBag.sel = "1";
                }
                else if (sup.SupplierTitle == "Mrs")
                {
                    @ViewBag.sel = "2";
                }
                else if (sup.SupplierTitle == "Ms")
                {
                    @ViewBag.sel = "3";
                }
                else if (sup.SupplierTitle == "Miss")
                {
                    @ViewBag.sel = "4";
                }
                else if (sup.SupplierTitle == "Dr")
                {
                    @ViewBag.sel = "5";
                }
                else if (sup.SupplierTitle == "Rev")
                {
                    @ViewBag.sel = "6";
                }
                else if (sup.SupplierTitle == "Company")
                {
                    @ViewBag.sel = "7";
                }
                return View("Index", sup);
            }

            if (sup.Photograph != null)
            {
                byte[] photo;
                using (BinaryReader br = new BinaryReader(sup.Photograph.InputStream))
                {
                    photo = br.ReadBytes(sup.Photograph.ContentLength);
                    sup.SupplierPicture = photo;
                    sup.SupplierPictureName = sup.Photograph.FileName;
                    sup.SupplierPictureType = sup.Photograph.ContentType;
                }
            }
          
            var existsemp = _bllsupplier.GetSupplierByCode(sup.SupplierCode,companyid);
            if (existsemp != null)
            {
                ViewBag.Message = "3";
                return View("Index", sup);
            }

            if (_bllsupplier.SaveSupplier(sup) == 1)
            {
                ModelState.Clear();
                ViewBag.Message = "1";
                @ViewBag.sel = "0";
                Supplier newSup = new Supplier();
                int docid = _bllcommon.GetDcumentId("Supplier", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllcommon.GetDocumentNo("Supplier", Convert.ToInt32(Session["loggeduserlocId"]), "1", docid, false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                newSup.SupplierCode = docnum;
                return View("Index", newSup);
            }
            else
            {
                ViewBag.Message = "2";
                ViewBag.SupCode = sup.SupplierCode;
                return View("Index", sup);
            }
        }

        [HttpPost]
        public ActionResult Gender(Supplier gender)
        {

            return View(gender);
        }

        [HttpGet]
        public JsonResult GetActiveSupplierTypes()
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var supplierTypes = _bllsupplier.GetSupplierTypes(companyid);
            return Json(JsonConvert.SerializeObject(supplierTypes, Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
            JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSuppliers()
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var suppliers = _bllsupplier.GetActiveSuppliers(companyid);
            return Json(JsonConvert.SerializeObject(suppliers, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        //Added by Pavithra on 2019-12-01
        [HttpGet]
        public JsonResult CheckSupplierCode(string code)
        {
           
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var sup = _bllsupplier.FindByCode(code,companyid);
            return new JsonResult { Data = sup, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}