using Newtonsoft.Json;
using RIT.HMS.BLL.Common;
using RIT.HMS.BLL.Configurations;
using RIT.HMS.BLL.Loyalty;
using RIT.HMS.Domain.Loyalty;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HospitalityManagement.Controllers.Loyalty
{
    [SessionTimeout]
    public class CardNumberGenerationController : Controller
    {
        private readonly BLL_Configuration _bllconfiguration;
        private readonly BLL_CardNumberGeneration _bllcardnogeneration;
        private readonly BLL_CardMaster _bllcardmaster;
        private AppManager _appmanager;
        private readonly BLL_Common _bllCommon;

        public CardNumberGenerationController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
           _bllconfiguration = new BLL_Configuration(cn);
           _bllcardnogeneration = new BLL_CardNumberGeneration(cn);
           _bllcardmaster = new BLL_CardMaster(cn);
           _appmanager = new AppManager(cn);
           _bllCommon = new BLL_Common(cn);
        }
        public ActionResult CardNumberGeneration()
        {

            if (!_appmanager.SetPermissions(14, Session["loggeduserempcode"].ToString(), "LoyaltyNoGenerate"))
            {
                @ViewBag.Permissions = "No user permissions to Generate/Issue Loyalty Card Numbers";
                return View("~/Views/Account/AccessDenied.cshtml");
            }

            int docid = _bllCommon.GetDcumentId("LCN", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            var docnum = _bllCommon.GetDocumentNo("LCN",
                                                   Convert.ToInt32(Session["loggeduserlocId"]),
                                                   "1",
                                                   docid,
                                                   true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

            LoyaltyCardGenerationHeader loyaltycardgenheader = new LoyaltyCardGenerationHeader();
            loyaltycardgenheader.DocNumber = docnum;
            var defaultsettings = _bllcardnogeneration.GetDefaultParams(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            loyaltycardgenheader.CardLength = defaultsettings.CardNoLength;
            loyaltycardgenheader.CardStartingNo = Convert.ToInt32(defaultsettings.CardStartingNo);
            loyaltycardgenheader.EncodeStartingNo = Convert.ToInt32(defaultsettings.EncodeStartingNo);
           

            Session["CGWizadUI"] = _bllconfiguration.GetConfiguration("UIWZ", Convert.ToInt32(Session["loggedusercompanyId"].ToString())).ConfigurationOn;
            return View("~/Views/Loyalty/CardNumberGeneration.cshtml", loyaltycardgenheader);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CardNumberGeneration(LoyaltyCardGenerationHeader cardnogenheader)
        {
            cardnogenheader.LocationId = cardnogenheader.GenLocationId;
            cardnogenheader.CreatedDate = DateTime.Now;
            cardnogenheader.CreatedUser = Session["loggeduser"].ToString();
          //  bool complete = false;
            var errors = ModelState.Values.SelectMany(e => e.Errors);
            if (cardnogenheader.LoyaltyCardGenerationDetail.Count() == 0)
            {
                ViewBag.Message = "2";
                ViewBag.CardMasterId = cardnogenheader.CardMasterId;
                ViewBag.GenLocationId = cardnogenheader.GenLocationId;
                return View("~/Views/Loyalty/CardNumberGeneration.cshtml", cardnogenheader);
            }
            if (ModelState.IsValid)
            {

                int docid = _bllCommon.GetDcumentId("LCN", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum = _bllCommon.GetDocumentNo("LCN",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                        "1",
                                                         docid,
                                                         false, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                cardnogenheader.DocNumber = docnum;
                cardnogenheader.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                if (_bllcardnogeneration.SaveCardNumbers(cardnogenheader))
                {                   
                    ModelState.Clear();
                    ViewBag.Message = "1";
                    int docid1 = _bllCommon.GetDcumentId("LCN", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    var docnum1 = _bllCommon.GetDocumentNo("LCN",
                                                            Convert.ToInt32(Session["loggeduserlocId"]),
                                                            "1",
                                                             docid1,
                                                             true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
               
                    LoyaltyCardGenerationHeader newheader = new LoyaltyCardGenerationHeader();
                    newheader.DocNumber = docnum1;
                    var defaultsettings = _bllcardnogeneration.GetDefaultParams(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    newheader.CardLength = defaultsettings.CardNoLength;
                    newheader.CardStartingNo = Convert.ToInt32(defaultsettings.CardStartingNo);
                    newheader.EncodeStartingNo = Convert.ToInt32(defaultsettings.EncodeStartingNo);
                    newheader.LoyaltyCardGenerationHeaderId = cardnogenheader.LoyaltyCardGenerationHeaderId;               
                    return View("~/Views/Loyalty/CardNumberGeneration.cshtml",newheader);                 
                }
                else
                {
                    int docid1 = _bllCommon.GetDcumentId("LCN", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    var docnum1 = _bllCommon.GetDocumentNo("LCN",
                                                            Convert.ToInt32(Session["loggeduserlocId"]),
                                                            "1",
                                                             docid1,
                                                             true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                    cardnogenheader.DocNumber = docnum1;
                    ViewBag.Message = "3";
                    ViewBag.CardMasterId = cardnogenheader.CardMasterId;
                    ViewBag.GenLocationId = cardnogenheader.GenLocationId;
                    return View("~/Views/Loyalty/CardNumberGeneration.cshtml", cardnogenheader);
                }
        
            }
            else
            {
                int docid1 = _bllCommon.GetDcumentId("LCN", Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                var docnum1 = _bllCommon.GetDocumentNo("LCN",
                                                        Convert.ToInt32(Session["loggeduserlocId"]),
                                                        "1",
                                                         docid1,
                                                         true, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
                cardnogenheader.DocNumber = docnum1;
                ViewBag.Message = "3";
                ViewBag.CardMasterId = cardnogenheader.CardMasterId;
                ViewBag.GenLocationId = cardnogenheader.GenLocationId;
                return View("~/Views/Loyalty/CardNumberGeneration.cshtml",cardnogenheader);
            }

        }

        //--------------------- Expoters-------------------------------------------

        [HttpPost]
        [ValidateInput(false)]
        public FileResult Export(string GridHtml)
        {
            return File(Encoding.ASCII.GetBytes(GridHtml), "application/vnd.ms-excel", "CardNumbers.xls");
        }


        public ActionResult ExportToExcel(int id)
        {
            
            var gv = new GridView();
          //  "=CHAR(048)&"
            var data= _bllcardnogeneration.GetCardNoDetailByHeaderId(id);
           
            data.ToList().ForEach(e=>{ e.EncodeNo = (e.EncodeNo).ToString(); });
            var data1 = data.Select(s => new { s.CardNo, s.EncodeNo });
            gv.DataSource = data1;
            gv.DataBind();           

            StringWriter objStringWriter = new  StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            byte[] binddata = Encoding.ASCII.GetBytes(objStringWriter.ToString());
            return File(binddata, "application/ms-excel", "CardNumbers.xls");
           
        }

        // --------------------Loaders-------------------------------------------

        [HttpGet]
        public JsonResult GetCardGenParams(int locid)
        {
            var parms = _bllcardnogeneration.GetParams(locid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return new JsonResult
            {
                Data = parms,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public JsonResult GenerateCardNumbers(int qty,int CardStartingNo,int EncodeStartingNo,int CardNoLength)
        {

            //var cardnumbers = _bllcardnogeneration.GenerateCardNumbers(qty, locprefx, CardStartingNo, EncodeStartingNo, CardNoLength);
            var cardnumbers = _bllcardnogeneration.GenerateCardNumbers(qty, CardStartingNo, EncodeStartingNo, CardNoLength);
            return Json(JsonConvert.SerializeObject(cardnumbers, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SelectCardNumbers(string cardnofrom,string cardnoto,int locid)
        {

            var cardnumbers = _bllcardnogeneration.SelectCardNumbers(cardnofrom,cardnoto,locid, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(cardnumbers, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ValidateCardNumber(string cardnumber)
        {
            var isvalid = _bllcardnogeneration.ValidateCardNumber(cardnumber);
            return new JsonResult
            {
                Data = isvalid,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

    }
}