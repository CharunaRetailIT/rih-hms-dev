using HospitalityManagement.Models.Transactions;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    public class GiftVoucherBarcodeController : Controller
    {
        BLL_GiftVoucherGoodReceiveNote _billGiftVoucherGoodReceiveNote;
        BLL_Location _blllocation;
        BLL_GiftVoucherGroup _billGiftVoucherGroup;
        BLL_GiftVoucherPO _billGiftVPO;
        private string giftvouchergroupcode = string.Empty;
        public GiftVoucherBarcodeController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _billGiftVoucherGoodReceiveNote = new BLL_GiftVoucherGoodReceiveNote(cn);
            _blllocation = new BLL_Location(cn);
            _billGiftVoucherGroup = new BLL_GiftVoucherGroup(cn);
            _billGiftVPO = new BLL_GiftVoucherPO(cn);
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Details(int id)
        {
            return View();
        }
        public ActionResult Create()
        {
            string GetGVDocNo_ = "";
            HospitalityManagement.Models.GiftVoucherBarcode invGiftVoucherBarcode = new HospitalityManagement.Models.GiftVoucherBarcode();
            List<InvGiftVoucherDocumentNumber> res = _billGiftVPO.GetnewVoucherDocNo("GVT").ToList();

            if (res != null && res.Count > 0)
            {
                var GetGVDocNo = res[0].DocumentNo;
                GetGVDocNo_ = GetGVDocNo;

                // Extract the prefix and numerical portion of the code
                string prefix = GetGVDocNo.Substring(0, 1); // "G"
                string numericalPart = GetGVDocNo.Substring(1); // "0011"
                int numericalValue = int.Parse(numericalPart);
                numericalValue++;
                // Format the numerical value back into the desired format
                GetGVDocNo_ = prefix + numericalValue.ToString("D11"); // "G0012"
            }
            else
            {
                GetGVDocNo_ = "T" + "00000000001";
            }
            invGiftVoucherBarcode.DocumentNo = GetGVDocNo_;
            return View(invGiftVoucherBarcode);
        }
        
        [Authorize(Roles = "CusView")]
        public ActionResult ViewBookCode()
        {

            int compayid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //var exists = _billGiftVoucherBookCode.GetGiftVoucherBookCode();
            //return View("~/Views/GiftVoucherBookCode/ViewVoucherBook.cshtml", exists);
            return View();
        }
        public ActionResult Edit(int id)
        {
            return View();
        }
        //private bool PrintBarCode()
        //{
        //    try
        //    {
        //        LookUpReferenceService lookUpReferenceService = new LookUpReferenceService();
        //        SystemConfig systemConfig = new SystemConfig();
        //        CommonService commonDetails = new CommonService();
        //        ReferenceType referenceType = new ReferenceType();
        //        StreamWriter m_streamWriter;
        //        string @barcodeTextPath, @appPath, @destinationPath;
        //        bool blnLocalCopy = false, folderExists = false;
        //        string txtFileName = "", exeFileName = "", tagFileName = "", sourceFile = "", destFile = "";
        //        txtFileName = "GVbar.txt";
        //        exeFileName = "Barcode.exe";
        //        referenceType = lookUpReferenceService.GetLookUpReferenceByValue(((int)LookUpReference.GiftVoucherTagType).ToString(), cmbTag.Text.Trim());
        //        if (referenceType != null)
        //        {
        //            tagFileName = string.Concat(referenceType.LookupValue, ".lbx");
        //        }
        //        @appPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "GVBarCode");
        //        systemConfig = commonDetails.GetSystemInfo(1);
        //        if (systemConfig != null)
        //        {
        //            @barcodeTextPath = @systemConfig.GVBarcodeTextPath; //@"C:\Barcode";
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //        @destinationPath = @barcodeTextPath;
        //        folderExists = Directory.Exists(@destinationPath);
        //        if (!folderExists)
        //        {
        //            folderExists = true;
        //            blnLocalCopy = Common.CopyDirectory(@appPath, @destinationPath, true);
        //        }
        //        if (folderExists)
        //        {
        //            FileStream fileStream = new FileStream(@destinationPath + @"\\" + txtFileName, FileMode.Create);
        //            m_streamWriter = new StreamWriter(fileStream);
        //            foreach (InvGiftVoucherBarcodeDetailTemp invBarcodeDetailTemp in invBarcodeDetailTempList)
        //            {
        //                for (int count = 0; count < invBarcodeDetailTemp.Qty; count = count + 1)
        //                {
        //                    //string strSellingPrice = (string.Format("{0:#0.##}", invBarcodeDetailTemp.SellingPrice));                            

        //                    m_streamWriter.WriteLine(invBarcodeDetailTemp.InvGiftVoucherMasterID + "," +
        //                                                invBarcodeDetailTemp.BookCode + "," +
        //                                                invBarcodeDetailTemp.BookName + ", " +
        //                        //invBarcodeDetailTemp.BarCode + ", " +
        //                                                invBarcodeDetailTemp.VoucherType + "," +
        //                                                invBarcodeDetailTemp.VoucherSerial + "," +
        //                                                invBarcodeDetailTemp.VoucherAmount  // + "," + 
        //                        //invBarcodeDetailTemp.DocumentDate.ToString("ddMMyy") + "," +
        //                        //(!strSellingPrice.Contains('.') ? string.Concat(strSellingPrice, "/-") : string.Format("{0:#0.00}", invBarcodeDetailTemp.SellingPrice)) + "," +
        //                                                );
        //                }
        //            }
        //            m_streamWriter.Close();
        //            fileStream.Close();
        //            if (File.Exists(@destinationPath + @"\\" + txtFileName))
        //            {
        //                Process.Start(@destinationPath + @"\\" + tagFileName);
        //            }
        //            string Bookcode = "";
        //            Bookcode = dgvSerialDetails.Rows[0].Cells["BookCode"].Value.ToString();
        //            InvGiftVoucherGroupService giftvoucherservice = new InvGiftVoucherGroupService();
        //            return giftvoucherservice.ProcessGiftvoucherBarcodePrintUpdate(Common.LoggedLocationID, Bookcode);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteLog(ex, MethodInfo.GetCurrentMethod().Name.ToString(), this.Name, Logger.logtype.ErrorLog, Common.LoggedLocationID);
        //    }
        //    return true;
        //}
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Delete(int id)
        {
            return View();
        }
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        [HttpGet]
        public JsonResult GetGiftvoucherGroup()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            List<InvGiftVoucherGroup> giftvouchergroupss = _billGiftVoucherGroup.GetGroups(companyid).ToList();
            giftvouchergroupcode = giftvouchergroupss[0].GiftVoucherGroupCode;
            return Json(JsonConvert.SerializeObject(giftvouchergroupss, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }        
    }
}
