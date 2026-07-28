
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using RIT.HMS.Domain.ViewModels.Reports;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using RIT.HMS.BLL.Loyalty;
using System.Security.Cryptography;
using System.Text;
using RIT.HMS.BLL.Configurations;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RIT.HMS.BLL.Reports;
using System.Drawing;
using Aspose;
using Aspose.Pdf; 
using System.Data;
using System.Reflection;
using Aspose.Pdf.Text;
namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class CustomerController : Controller

    {
        
        BLL_Customer _customer;
        BLL_Location _location;      
        BLL_CardNumberGeneration  _bllCardNoGen;
        private AppManager _appmanager;
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_Reports _bllreports;
        public CustomerController()
        {
            
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _customer = new BLL_Customer(cn);
            _location = new BLL_Location(cn);
            _bllCardNoGen = new BLL_CardNumberGeneration(cn);
            _appmanager = new AppManager(cn);
            _bllconfiguration = new BLL_Configuration(cn);
            _bllreports = new BLL_Reports(cn);
        }

        // GET: Customer

        [Authorize(Roles = "CusView")]      
        public ActionResult ViewCustomers()
        {
                              
            int compayid= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var exists = _customer.GetCustomers(compayid);
            return View(exists);
        }

        [Authorize(Roles = "CusView")]
        public void HMSCustomers()
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
               
                int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                var exists = _customer.GetCustomers(compayid); 


                ExcelPackage Ep = new ExcelPackage();
                ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Customers");

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

                Sheet.Cells[8, 1].Value = "Customer Code";
                Sheet.Cells[8, 2].Value = "Customer Title";
                Sheet.Cells[8, 3].Value = "Customer Name";
                Sheet.Cells[8, 4].Value = "Customer Address";
                Sheet.Cells[8, 5].Value = "Contact No";
                Sheet.Cells[8, 6].Value = "Email"; 
                Sheet.Cells[8, 7].Value = "Is Active";
                Sheet.Cells[8, 8].Value = "Sender Preference";

                int row = 9; 
                foreach (var item in exists)  
                {

                    Sheet.Cells[row, 1].Value = item.CustomerCode; 
                    Sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 2].Value = item.CustomerTitle;
                    Sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 3].Value = item.CustomerName;
                    Sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 4].Value = (item.BillingAddress1 + item.BillingAddress2 + item.BillingAddress3);
                    Sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 5].Value = item.Mobile;
                    Sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 6].Value = item.Email;
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
                    Sheet.Cells[row, 8].Value = item.SenderPreference;
                    Sheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    row++;
                }


                #region
                System.Drawing.Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

                Sheet.Cells[8, 1, 8, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells[8, 1, 8, 8].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);
 
                var table = Sheet.Cells[8, 1, 8, 8];
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
                Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSCustomers.xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    Ep.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        [Authorize(Roles = "CusView")]
        public void HMSCustomerDetails()
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                RptCustomerDetailViewModel objRpt=new RptCustomerDetailViewModel();
                if (TempData["CustomerDetailsReport"]!=null)                
                    objRpt = (RptCustomerDetailViewModel)TempData["CustomerDetailsReport"];
               


                int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                var exists = _customer.GetCustomerDetailReport(objRpt.LocationId, objRpt.CustomerId);
                //var Location = _location.GetLocationById(objRpt.LocationId).LocationName;

                ExcelPackage Ep = new ExcelPackage();
                ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("CustomerDetails");


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
                Sheet.Cells[8, 1].Value = "Location Name";
                Sheet.Cells[8, 2].Value = "Customer Code";
                Sheet.Cells[8, 3].Value = "Customer Title";
                Sheet.Cells[8, 4].Value = "Customer Name";
                Sheet.Cells[8, 5].Value = "Customer Address";
                Sheet.Cells[8, 6].Value = "Contact No";
                Sheet.Cells[8, 7].Value = "Email";
                Sheet.Cells[8, 8].Value = "NIC No"; 

                int row = 9;
                 

                foreach (var item in exists)
                {
                    Sheet.Cells[row, 1].Value = _location.GetLocationById(item.LocationId).LocationName; 
                    Sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 2].Value = item.CustomerCode;
                    Sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 3].Value = item.CustomerTitle;
                    Sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 4].Value = item.CustomerName;
                    Sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 5].Value = (item.BillingAddress1 + item.BillingAddress2 + item.BillingAddress3);
                    Sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 6].Value = item.Mobile;
                    Sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 7].Value = item.Email;
                    Sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    Sheet.Cells[row, 8].Value = item.NIC;
                    Sheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; 
                    row++;
                }


                #region
                System.Drawing.Color colFromHexHeading = System.Drawing.ColorTranslator.FromHtml("#919089");

                Sheet.Cells[8, 1, 8, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells[8, 1, 8, 8].Style.Fill.BackgroundColor.SetColor(colFromHexHeading);

                var table = Sheet.Cells[8, 1, 8, 8];
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
                Response.AddHeader("content-disposition", "attachment: attachment;filename=HMSCustomers.xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    Ep.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        [Authorize(Roles = "CusView")]
        public FileContentResult HMSCustomerDetailsPdf()
        {

            var document = new Document
            {
                PageInfo = new PageInfo { Margin=new MarginInfo(28,28,28,40)}
            };

            var pdfpage = document.Pages.Add();
            string LocationName = "";
   
           
             string compName, Address1, Address2, Address3, Tele, Fax, website, ReportHead = "";
                compName = _location.GetCompanyDetails().CompanyName;
                Address1 = _location.GetCompanyDetails().Address1;
                Address2 = _location.GetCompanyDetails().Address2;
                Address3 = _location.GetCompanyDetails().Address3;
                Tele = _location.GetCompanyDetails().Telephone;
                Fax = _location.GetCompanyDetails().Fax;
                website = _location.GetCompanyDetails().Website;

                //var header = new TextFragment("Customer Details Report");
                //header.TextState.Font = FontRepository.FindFont("Arial");
                //header.TextState.FontSize = 10;
                //header.HorizontalAlignment = HorizontalAlignment.Center;
                //header.Position = new Position(130, 720);
                //pdfpage.Paragraphs.Add(header);

                //var header1 = new TextFragment(compName);
                //header1.TextState.Font = FontRepository.FindFont("Arial");
                //header1.TextState.FontSize = 10;
                //header1.HorizontalAlignment = HorizontalAlignment.Left;
                //header1.Position = new Position(130, 720);
                //pdfpage.Paragraphs.Add(header1);

                //var header2 = new TextFragment(Address1);
                //header2.TextState.Font = FontRepository.FindFont("Arial");
                //header2.TextState.FontSize = 10;
                //header2.HorizontalAlignment = HorizontalAlignment.Left;
                //header2.Position = new Position(130, 720);
                //pdfpage.Paragraphs.Add(header2);

                //var header3 = new TextFragment(Address2);
                //header3.TextState.Font = FontRepository.FindFont("Arial");
                //header3.TextState.FontSize = 10;
                //header3.HorizontalAlignment = HorizontalAlignment.Left;
                //header3.Position = new Position(130, 720);
                //pdfpage.Paragraphs.Add(header3);

                //var header4 = new TextFragment(Address3);
                //header4.TextState.Font = FontRepository.FindFont("Arial");
                //header4.TextState.FontSize = 10;
                //header4.HorizontalAlignment = HorizontalAlignment.Left;
                //header4.Position = new Position(130, 720);
                //pdfpage.Paragraphs.Add(header4);

                //var header5 = new TextFragment(Tele);
                //header5.TextState.Font = FontRepository.FindFont("Arial");
                //header5.TextState.FontSize = 10;
                //header5.HorizontalAlignment = HorizontalAlignment.Left;
                //header5.Position = new Position(130, 720);
                //pdfpage.Paragraphs.Add(header5);

                //var header6 = new TextFragment(Fax);
                //header6.TextState.Font = FontRepository.FindFont("Arial");
                //header6.TextState.FontSize = 10;
                //header6.HorizontalAlignment = HorizontalAlignment.Left;
                //header6.Position = new Position(130, 720);
                //pdfpage.Paragraphs.Add(header6);

                pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(compName));
                pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(Address1));
                pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(Address2));
                pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(Address3));
                pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(Tele));
                pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(Fax));
                if (website != null)
                {
                    pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(website));
                }
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));
 


            var header7 = new TextFragment("Customer Details Report");
            header7.TextState.Font = FontRepository.FindFont("Arial");
            header7.TextState.FontSize = 12;
            header7.HorizontalAlignment = HorizontalAlignment.Center;
            header7.Position = new Position(130, 720);
            pdfpage.Paragraphs.Add(header7);
             
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));
            pdfpage.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment(""));



            //Aspose.Pdf.Text.TextFragment fragment = new TextFragment("Hello World....");

            Aspose.Pdf.Table table = new Aspose.Pdf.Table
            {
                ColumnWidths = "12% 15% 12% 15% 15% 15% 15%",
                DefaultCellPadding = new MarginInfo(10,6,10,6),
                Border = new BorderInfo(BorderSide.All, .5f, Aspose.Pdf.Color.Black),
                DefaultCellBorder = new BorderInfo(BorderSide.All, .2f, Aspose.Pdf.Color.Black),
            };


            RptCustomerDetailViewModel objRpt = new RptCustomerDetailViewModel();
            if (TempData["CustomerDetailsReport"] != null)
                objRpt = (RptCustomerDetailViewModel)TempData["CustomerDetailsReport"];

            List<Customer> GetCust = new List<Customer>();

            int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            GetCust = _customer.GetCustomerDetailReport(objRpt.LocationId, objRpt.CustomerId);
            //var Location = _location.GetLocationById(objRpt.LocationId).LocationName;


                  ListtoDataTableConverter converter = new ListtoDataTableConverter();
                  DataTable dt = converter.ToDataTable(GetCust);


                 table.ImportDataTable(dt, true, 0, 0);
                 document.Pages[1].Paragraphs.Add(table);


                using (var strment = new MemoryStream())
                {
                    document.Save(strment);
                    return new FileContentResult(strment.ToArray(), "application/pdf")
                    {
                        FileDownloadName = "Cuntryname.pdf"
                    };

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

                dataTable.Columns.Add("Customer Code", typeof(System.String));
                dataTable.Columns.Add("Customer Title", typeof(System.String));
                dataTable.Columns.Add("Customer Name", typeof(System.String));
                dataTable.Columns.Add("Customer Address", typeof(System.String));
                dataTable.Columns.Add("Contact No", typeof(System.String));
                dataTable.Columns.Add("Email", typeof(System.String));
                dataTable.Columns.Add("NIC No", typeof(System.String));
                foreach (T item in items)
                {
                    var values = new object[Props.Length];
                    for (int i = 0; i < Props.Length; i++)
                    {
                        dr = dataTable.NewRow();
                        //inserting property values to datatable rows
                        ///values[i] = Props[i].GetValue(item, null);
                        dr[0] = Props[1].GetValue(item, null);
                        dr[1] = Props[2].GetValue(item, null);
                        dr[2] = Props[3].GetValue(item, null);
                        dr[3] = Props[6].GetValue(item, null);
                        dr[4] = Props[13].GetValue(item, null);
                        dr[5] = Props[15].GetValue(item, null);
                        dr[6] = Props[10].GetValue(item, null);
                    }
                    dataTable.Rows.Add(dr);
                }
                //put a breakpoint here and check datatable
                return dataTable;
            }
        }

        [Authorize(Roles = "CusCreatee")]
        public ActionResult Create()
        {

            @ViewBag.CustomerStatus = "Other";
            return View(new Customer());
        }

        [Authorize(Roles = "CusEdit")]
        public ActionResult Edit(long id)
        {
            //CustomerService custservice = new CustomerService();
            var exists = _customer.GetCustomerById(id);
            @ViewBag.CustomerStatus = exists.CustomerStatus;
            @ViewBag.CustomerCategoryId = exists.CustomerCategoryId;
            @ViewBag.FileName = exists.CustomerPictureName;
            @ViewBag.SpecialDayType = exists.SpecialDayType;
            @ViewBag.CivilStatus = exists.CivilStatus; 

            if (exists.CustomerTitle == "Mr")
            {
                @ViewBag.sel = "0";
            }
            else if (exists.CustomerTitle == "Mrs")
            {
                @ViewBag.sel = "1";
            }
            else if (exists.CustomerTitle == "Ms")
            {
                @ViewBag.sel = "2";
            }
            else if (exists.CustomerTitle == "Miss")
            {
                @ViewBag.sel = "3";
            }
            else if (exists.CustomerTitle == "Dr")
            {
                @ViewBag.sel = "4";
            }
            else if (exists.CustomerTitle == "Rev")
            {
                @ViewBag.sel = "5";
            }

            else if (exists.CustomerTitle == "Company" || exists.CustomerTitle == "COMP")
            {
                @ViewBag.sel = "6";
                exists.CustomerTitle = "Company";
            }
            

            if (exists.SenderPreference == 0)
            {
                @ViewBag.sel = "None";
            }
            else if (exists.SenderPreference == 1)
            {
                @ViewBag.sel = "SMS";
            }
            else if (exists.SenderPreference == 2)
            {
                @ViewBag.sel = "Email";
            }
            else if (exists.SenderPreference == 3)
            {
                @ViewBag.sel = "Email & SMS";
            }
            else if (exists.SenderPreference == 4)
            {
                @ViewBag.sel = "Only Print";
            }





            if (exists.WeddingAnniversary != null)
            {
                DateTime sss = (DateTime)exists.WeddingAnniversary;
                if (sss.ToShortDateString() == DateTime.Now.Date.ToShortDateString())
                {
                    ModelState.AddModelError("WeddingAnniversary", "Wedding Anniversary is " + sss.ToShortDateString() + " (Today)");
                }
            }
            // ModelState.AddModelError("CustomerStatus",exists.CustomerStatus);
            @ViewBag.CustomerStatus = exists.CustomerStatus;

            var card = _bllCardNoGen.GetCardNumberByCustomerId(exists.CustomerID, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            if (card != null)
            {
                exists.CardNumber = card.CardNo;
                exists.NameOnCard = card.NameOnCard;
            }
            return View(exists);
        }
        [Authorize(Roles = "CusEdit")]
        [HttpPost]
        public ActionResult Edit(Customer cust)
        {
            Common common = new Common();
            @ViewBag.CustomerStatus = cust.CustomerStatus;
            @ViewBag.CustomerCategoryId = cust.CustomerCategoryId;
            @ViewBag.FileName = cust.CustomerPictureName;
            ModelState.AddModelError("CustomerStatus", cust.CustomerStatus);

            if (cust.Photograph != null && common.CheckImageType(cust.Photograph.ContentType) == false)
            {
                ModelState.AddModelError("Photograph", "Only an Image required");
                return View(cust);
            }
            if (cust.IsActive == true && cust.IsDelete == true)
            {
                ViewBag.CustomerStatus = cust.CustomerStatus;
                ViewBag.CustomerCategoryId = cust.CustomerCategoryId;
                @ViewBag.Message = "4";
                return View(cust);
            }
          
            var exists = _customer.GetCustomerById(cust.CustomerID);

            exists.FirstName = cust.FirstName;
            exists.LastName = cust.LastName;
            exists.CustomerName = cust.FirstName + " " + cust.LastName;

            //exists.CustomerName = cust.CustomerName;
            exists.BillingAddress1 = cust.BillingAddress1;
            exists.BillingAddress2 = cust.BillingAddress2;
            exists.BillingAddress3 = cust.BillingAddress3;
            exists.DOB = cust.DOB;
            exists.WeddingAnniversary = cust.WeddingAnniversary;
            exists.Profession = cust.Profession;
            //  exists.CustomerType = cust.CustomerType;
            exists.CustomerStatus = cust.CustomerStatus;
            exists.CustomerCategoryId = cust.CustomerCategoryId;
            exists.NIC = cust.NIC;
            exists.Passport = cust.Passport;
            exists.CustomerTitle = cust.CustomerTitle;
            exists.Telephone = cust.Telephone;
            exists.Mobile = cust.Mobile;
            exists.Email = cust.Email;
            exists.VehicleNo = cust.VehicleNo;
            exists.IsActive = cust.IsActive;
            exists.IsActiveForLoyalty = cust.IsActiveForLoyalty;
            exists.IsDelete = cust.IsDelete;
            exists.ModifiedDate = DateTime.Now;
            exists.ModifiedUser = Session["loggeduser"].ToString();
            exists.Fax = cust.Fax;
            exists.CreditLimit = cust.CreditLimit;
            exists.Outstanding = cust.Outstanding;
            exists.EPFNo = cust.EPFNo;
            exists.MembershipCardNo = cust.MembershipCardNo;
            exists.Remarks = cust.Remarks;
            exists.CustomerStatus = cust.CustomerStatus;
            exists.Gender = cust.Gender;
            exists.ReferenceNo1 = cust.ReferenceNo1;
            exists.ReferenceNo2 = cust.ReferenceNo2;
            exists.Age = cust.Age;
            exists.Religion = cust.Religion;
            exists.Race = cust.Race;
            exists.LandMark = cust.LandMark;
            exists.District = cust.District;
            exists.Organization = cust.Organization;
            exists.WorkAddres1 = cust.WorkAddres1;
            exists.WorkAddres2 = cust.WorkAddres2;
            exists.WorkAddres3 = cust.WorkAddres3;
            exists.WorkEmail = cust.WorkEmail;
            exists.WorkTelephone = cust.WorkTelephone;
            exists.WorkMobile = cust.WorkMobile;
            exists.WorkFax = cust.WorkFax;
            exists.SpouseName = cust.SpouseName;
            exists.CivilStatus = cust.CivilStatus;
            exists.SpouseDateOfBirth = cust.SpouseDateOfBirth;
            exists.DeliverTo = cust.DeliverTo;
            exists.DeliverToAddress = cust.DeliverToAddress;
            exists.Country = cust.Country;
            exists.CustomerSince = cust.CustomerSince;
            exists.SpecialDayType = cust.SpecialDayType;
            exists.SendUpdatesViaEmail = cust.SendUpdatesViaEmail;
            exists.SendUpdatesViaSms = cust.SendUpdatesViaSms;
            exists.IsRegByPOS = cust.IsRegByPOS;
            exists.CardNumber = cust.CardNumber;
            exists.NameOnCard = cust.NameOnCard;
            exists.ExpiryDate = cust.ExpiryDate;
            exists.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            exists.SenderPreference = cust.SenderPreference; 
            //   exists.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            if (cust.Photograph != null)
            {
                byte[] newlogo;
                using (BinaryReader br = new BinaryReader(cust.Photograph.InputStream))
                {
                    newlogo = br.ReadBytes(cust.Photograph.ContentLength);
                    cust.CustomerPicture = newlogo;
                    cust.CustomerPictureName = cust.Photograph.FileName;
                    cust.CustomerPictureType = cust.Photograph.ContentType;
                }

                if (cust.CustomerPictureName != exists.CustomerPictureName)
                {
                    byte[] pic;
                    using (BinaryReader br = new BinaryReader(cust.Photograph.InputStream))
                    {
                        pic = br.ReadBytes(cust.Photograph.ContentLength);
                        exists.CustomerPicture = pic;
                        exists.CustomerPictureName = cust.Photograph.FileName;
                        exists.CustomerPictureType = cust.Photograph.ContentType;
                    }
                }
            }
            @ViewBag.CustomerStatus = exists.CustomerStatus;
            if (cust.IsActiveForLoyalty && String.IsNullOrEmpty(cust.CardNumber) || (cust.IsActiveForLoyalty && String.IsNullOrEmpty(cust.NameOnCard)))
            {
                ViewBag.Message = "5";
                @ViewBag.CustomerCategoryId = cust.CustomerCategoryId;
                return View(cust);
            }
            if (cust.IsActiveForLoyalty && String.IsNullOrEmpty(cust.CardNumber) == false && cust.IsCardValid == false)
            {
                ViewBag.Message = "6";
                @ViewBag.CustomerCategoryId = cust.CustomerCategoryId;
                return View(cust);
            }
            if (_customer.UpdateCustomer(exists) > 0)
            {
                @ViewBag.Message = "1";
            }
            else
            {
                @ViewBag.Message = "2";
            }
            @ViewBag.CustCode = exists.CustomerCode;
            return View(cust);

         
        }

        [Authorize(Roles = "CusCreatee")]
        [HttpPost]
        public ActionResult Create(Customer cust)
        {
            //cust.NIC = cust.CustomerNIC;

            Common common = new Common();

            if (cust.Photograph != null && common.CheckImageType(cust.Photograph.ContentType) == false)
            {
                ModelState.AddModelError("Photograph", "Only an Image required");
                return View(cust);
            }

            cust.FirstName = cust.FirstName;
            cust.LastName = cust.LastName;
            cust.CustomerName = cust.FirstName + " " + cust.LastName;
            cust.CreatedUser = Session["loggeduser"].ToString();
            cust.CreatedDate = DateTime.Now;
            cust.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            cust.DataTransfer = 1;
            cust.ModifiedUser = Session["loggeduser"].ToString();
            //Added by pavithra on 2019-11-30
            cust.IsActive = true;
            cust.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            @ViewBag.CustomerStatus = cust.CustomerStatus;
            //if (cust.WeddingAnniversary.ToShortDateString() == "01/01/0001")
            //{
            //    cust.WeddingAnniversary = Convert.ToDateTime("01/01/0001");
            //}

            if (!ModelState.IsValid)
            {

                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var err in errors)
                {
                    var k = err;
                }

                @ViewBag.CustomerCategoryId = cust.CustomerCategoryId;
                // var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(cust);
            }

            if (cust.IsActiveForLoyalty && (String.IsNullOrEmpty(cust.CardNumber) ||
                                            String.IsNullOrEmpty(cust.NameOnCard))
                )
            {
                ViewBag.Message = "5";
                @ViewBag.CustomerCategoryId = cust.CustomerCategoryId;
                return View(cust);
            }
            if (cust.IsActiveForLoyalty && String.IsNullOrEmpty(cust.CardNumber) == false && cust.IsCardValid == false)
            {
                ViewBag.Message = "6";
                @ViewBag.CustomerCategoryId = cust.CustomerCategoryId;
                return View(cust);
            }

            if (cust.Photograph != null)
            {
                byte[] photo;
                using (BinaryReader br = new BinaryReader(cust.Photograph.InputStream))
                {
                    photo = br.ReadBytes(cust.Photograph.ContentLength);
                    cust.CustomerPicture = photo;
                    cust.CustomerPictureName = cust.Photograph.FileName;
                    cust.CustomerPictureType = cust.Photograph.ContentType;
                }
            }
          
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var existscust = _customer.GetCustomerByCode(cust.CustomerCode, companyid);
            ViewBag.CustCode = cust.CustomerCode;

            if (existscust != null)
            {
                ViewBag.Message = "3";
                return View(cust);
            }
            var errors1 = ModelState.Values.SelectMany(v => v.Errors);
            // cust.Religion = 1;
            if (_customer.SaveCustomer(cust) != 0)
            {
                ViewBag.Message = "1";
                @ViewBag.CustomerStatus = "Other";
                ModelState.Clear();
                return View(new Customer());
            }
            else
            {
                @ViewBag.CustomerCategoryId = cust.CustomerCategoryId;
                ViewBag.Message = "2";
                return View(cust);
            }


        }

        [HttpGet]
        public JsonResult GetCustomerByLocId(long locid)
        {
            var types = _customer.GetCustomerByLocId(locid);
            return Json(JsonConvert.SerializeObject(types, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCustomers()
        {
            int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var types = _customer.GetCustomers(compayid);
            return Json(JsonConvert.SerializeObject(types, Formatting.None, new
                JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Reports")]
        public ActionResult RPTCustomerDetails()
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTCustomerDetails"))
                {
                    @ViewBag.Permissions = "No user permissions to View Customer Details Report";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }

            RptCustomerDetailViewModel vvm = new RptCustomerDetailViewModel();
            return View("~/Views/Reports/Customer/RPTCustomerDetails.cshtml", vvm);
        }

        


        [Authorize(Roles = "Reports")]
        [HttpPost]
        public ActionResult RPTCustomerDetails(RptCustomerDetailViewModel vvm)
        {
            if (_bllconfiguration.GetConfiguration("UReports", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn)
            {

                if (!_appmanager.CheckReportPermissions(999, Session["loggeduserempcode"].ToString(), "RPTCustomerDetails"))
                {
                    @ViewBag.Permissions = "No user permissions to View Customer Details Report";
                    return View("~/Views/Account/AccessDenied.cshtml");
                }

            }


            var customers = _customer.GetCustomerDetailReport(vvm.LocationId, vvm.CustomerId);
            List<CustomerDetailViewModel> customermodel = new List<CustomerDetailViewModel>();
            foreach (var s in customers)
            {
                CustomerDetailViewModel v = new CustomerDetailViewModel();
                v.CustomerID = s.CustomerID;
                v.CustomerCode = s.CustomerCode;
                v.Location = _location.GetLocationById(s.LocationId).LocationName;
                v.CustomerTitle = s.CustomerTitle;
                v.CustomerName = s.CustomerName;
                v.LocationId = _location.GetLocationById(s.LocationId).SysLocationID;
                
              //  v.CustomerType = s.CustomerType;
                v.Address = s.BillingAddress1+","+s.BillingAddress2+","+s.BillingAddress3+".";
                v.NIC = s.NIC;
                v.Mobile = s.Mobile;
                v.Email = s.Email;
                v.SenderPreference = Convert.ToString(s.SenderPreference);
                
                customermodel.Add(v);
            }



            if (customermodel.Count > 0)
            {
                if (vvm.LocationId == 0 && vvm.CustomerId == 0) { @ViewBag.ReportSummary = "All Customers at All Locations"; }
                if (vvm.LocationId != 0 && vvm.CustomerId == 0) { @ViewBag.ReportSummary = "All Customers at Location: " + customermodel.First().Location; }
                if (vvm.LocationId == 0 && vvm.CustomerId != 0) { @ViewBag.ReportSummary = "Customer: " + customermodel.First().CustomerName + " in every location"; }
                if (vvm.LocationId != 0 && vvm.CustomerId != 0) { @ViewBag.ReportSummary = "Customer: " + customermodel.First().CustomerName + " in Location : " + customermodel.First().Location; }
            }
            else
            {
                @ViewBag.ReportSummary = "No existing customers in this location";
            }
            vvm.customermodel = customermodel;

            TempData["CustomerDetailsReport"] = vvm;

            return View("~/Views/Reports/Customer/RPTCustomerDetails.cshtml", vvm);
        }



        //------------------------------------------------------

        [HttpGet]
        public JsonResult CheckCustomerCode(string code)
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var cus = _customer.FindByCode(code, companyid);
            return new JsonResult { Data = cus, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
           
        }

        [HttpGet]
        public JsonResult GetActiveCustomerCategories()
        {
          
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var cuscat = _customer.GetActiveCustomerCategories(companyid);
            return Json(JsonConvert.SerializeObject(cuscat, Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
            JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CivilStates()
        {
            var civilstatus = Enum.GetValues(typeof(RIT.HMS.Domain.MasterEnums.CivilStatus)).
                                                Cast<RIT.HMS.Domain.MasterEnums.CivilStatus>().Select(x => x.ToString()).ToList();
         
            List<RIT.HMS.Domain.MasterEnums.Ennums> enumvals = new List<RIT.HMS.Domain.MasterEnums.Ennums>();
            int k = Enum.GetValues(typeof(RIT.HMS.Domain.MasterEnums.CivilStatus)).Length;
            for (int i = 1; i <= k; i++)
            {
                RIT.HMS.Domain.MasterEnums.Ennums e = new RIT.HMS.Domain.MasterEnums.Ennums();
                e.Value = i;
                e.Name = (Enum.GetName(typeof(RIT.HMS.Domain.MasterEnums.CivilStatus), i)).ToString();
                enumvals.Add(e);
            }

            return Json(JsonConvert.SerializeObject(enumvals, Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
            JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSpecialDays()
        {
            var specialdays = Enum.GetValues(typeof(RIT.HMS.Domain.MasterEnums.SpecialDay)).
                                                Cast<RIT.HMS.Domain.MasterEnums.SpecialDay>();
            List<RIT.HMS.Domain.MasterEnums.Ennums> enumvals = new List<RIT.HMS.Domain.MasterEnums.Ennums>();           
            int k = Enum.GetValues(typeof(RIT.HMS.Domain.MasterEnums.SpecialDay)).Length;
            for (int i=1; i<= k;i++)
            {
                RIT.HMS.Domain.MasterEnums.Ennums e = new RIT.HMS.Domain.MasterEnums.Ennums();
                e.Value = i;
                e.Name = (Enum.GetName(typeof(RIT.HMS.Domain.MasterEnums.SpecialDay), i)).ToString();               
                enumvals.Add(e);
            }                    
            return Json(JsonConvert.SerializeObject(enumvals, Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
            JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetActiveCustomerStatuses()
        {
            var customerstatus = Enum.GetValues(typeof(RIT.HMS.Domain.MasterEnums.EnumCustomerStatus)).
                                                Cast<RIT.HMS.Domain.MasterEnums.EnumCustomerStatus>().Select(x => x.ToString()).ToList();          
            return Json(JsonConvert.SerializeObject(customerstatus, Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
            JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "CusCreatee")]
        public ActionResult CustomerWisePrices()
        {

            return View(_customer.GetCustomerPrices());
        }

        public ActionResult SubmitCustomerPrices(List<CustomerDiscount> customerdiscountlist)
        {
            customerdiscountlist.ForEach( c =>
                    {
                        c.CustomerCode = _customer.GetCustomerById(c.CustomerId).CustomerCode;
                        c.CreatedUser = Session["loggeduser"].ToString();
                        c.ModifiedDate = DateTime.Now;
                        c.LocationId = Convert.ToInt32(Session["loggeduserlocId"]);
                        c.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"]); 
                    }
            );          
            return new JsonResult { Data = _customer.SaveCustomerDiscounts(customerdiscountlist),
                                    JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public JsonResult CustomerPrices(long customerid)
        {
            var customerprices =_customer.GetCustomerPricesByCustomerId(customerid);

            return Json(JsonConvert.SerializeObject(customerprices, Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
            JsonRequestBehavior.AllowGet);
            //return new JavaScriptSerializer().Serialize(customerprices);
            //return JsonConvert.SerializeObject(customerprices);
        }

        [HttpGet]
        public JsonResult RemoveCustomerDiscount(long customerid,long productid)
        {

            var responsemsg = _customer.RemoveCustomerDiscounts(customerid,productid);
            return Json(JsonConvert.SerializeObject(responsemsg, Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
            JsonRequestBehavior.AllowGet);
        
        }

        [HttpGet]
        public JsonResult GetReligions()
        {
            var religions = _customer.ReferanceTypes("7");

            return Json(JsonConvert.SerializeObject(religions, Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
            JsonRequestBehavior.AllowGet);
       
        }
        [HttpGet]
        public JsonResult GetRaces()
        {
            var races = _customer.ReferanceTypes("29");

            return Json(JsonConvert.SerializeObject(races, Formatting.None,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
            JsonRequestBehavior.AllowGet);

        }
    }
}