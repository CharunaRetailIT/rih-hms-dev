using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Web.Security;
using System.Web.Script.Serialization;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain.Accounts;
using RIT.HMS.Domain;
using RIT.HMS.BLL.Configurations;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using RIT.HMS.Domain.Common;
using RIT.HMS.BLL.Common;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;

namespace HospitalityManagement.Controllers
{
    // [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private BLL_Configuration _bllconfiguration = null;
        private BLL_Company _bllcompany = null;
        private BLL_SysConfiguration _bllconfigurations = null;
        private BLL_ConnectionManager _bllcompanymanager = null;

        private  BLL_Location _blllocation = null;
        public AccountController()
        {
            //var mode = ConfigurationManager.AppSettings["SubscriptionMode"];
            //if (mode == "OFF")
            //{

            //    ConnectionManager.CurrentConnectionName = "HMS_Default";
            //}
            //else if(mode=="ON")
            //{
            //    ConnectionManager.CurrentConnectionName = "HMSLoginManager";
            //}

            //_bllcompanymanager = new BLL_ConnectionManager();               
            //_bllcompany= new BLL_Company();
            //_bllconfigurations = new BLL_SysConfiguration();

        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            LoginViewModel loginvwmodel = new LoginViewModel();
            loginvwmodel.Authorized = true;
            ViewBag.sn = HttpContext.Server.MachineName;
            return View(loginvwmodel);

            //var servername = HttpContext.Server.MachineName;
            //    servername = "SERVER1";

            //var sysname = _bllconfigurations.GetServerName();
            //var dburl = _bllconfiguration.GetConfiguration("DURL", 1);
            //if (dburl != null)
            //{
            //    Session["DURL"] = dburl.ConfigurationDescription;
            //}
            //else
            //{
            //    Session["DURL"] = "";
            //}

            //// check license 
            //if (servername != sysname)
            //{
            //    ModelState.AddModelError("Password", "Unauthorized License Key Detected..!");
            //    loginvwmodel.Authorized = false;
            //}
            //else
            //{
            //    loginvwmodel.Authorized = true;
            //}

            //   return View(loginvwmodel);

        }

        [AllowAnonymous]
        [SessionTimeout]
        public ActionResult LogOut(string returnUrl)
        {

            var mode1 = ConfigurationManager.AppSettings["SubscriptionMode"];
            if (mode1 == "OFF")
            {

                ConnectionManager.CurrentConnectionName = "HMS_Default";
                Session["CurrentConnectionName"] = "HMS_Default";
            }
            else if (mode1 == "ON")
            {
                ConnectionManager.CurrentConnectionName = "HMSLoginManager";
                Session["CurrentConnectionName"] = "HMSLoginManager";
            }

            _bllcompanymanager = new BLL_ConnectionManager(Convert.ToString(Session["CurrentConnectionName"]));
            _bllcompany = new BLL_Company(Convert.ToString(Session["CurrentConnectionName"]));
            _bllconfigurations = new BLL_SysConfiguration(Convert.ToString(Session["CurrentConnectionName"]));



            LoginViewModel model = new LoginViewModel();
            model.Email = Session["loggeduser"].ToString();
            model.Password = Session["loggeduserpw"].ToString();


            var mode = ConfigurationManager.AppSettings["SubscriptionMode"];
            string actualdb = "";
            if (mode == "ON")
            {
                actualdb = _bllcompanymanager.GetActualConnectionName(model.Email);
                ConnectionManager.CurrentConnectionName = actualdb;
                Session["CurrentConnectionName"] = actualdb;
            }
            else if (mode == "OFF")
            {
                actualdb = ConnectionManager.CurrentConnectionName;
                actualdb = Session["CurrentConnectionName"].ToString();
            }




            // UserServise Repository = new UserServise();
            // var actualdb = _bllcompanymanager.GetActualConnectionName(model.Email);
            // ConnectionManager.CurrentConnectionName = actualdb;
            BLL_UserMaster _bllusermaster = new BLL_UserMaster(actualdb);

            SysUserMaster sysUserMaster = _bllusermaster.GetUserDetails(model, Convert.ToInt32(Session["loggeduserlocId"]));
            string Rols = _bllusermaster.GetUserRolesByUsername(sysUserMaster);

            FormsAuthentication.SetAuthCookie(model.Email, false);
            var authTicket = new FormsAuthenticationTicket(1, "", DateTime.Now,
                                                                  DateTime.Now.AddDays(-1), false, Rols);

            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

            HttpContext.Response.Cookies.Add(authCookie);

            ViewBag.ReturnUrl = returnUrl;
            Session.Clear();
            return RedirectToAction("Login", "Account");

        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public void SetPermissionCookie(int typeid, string username)
        {
            var actualdb = _bllcompanymanager.GetActualConnectionName(username);
            ConnectionManager.CurrentConnectionName = actualdb;
            BLL_UserMaster _bllusermaster = new BLL_UserMaster(actualdb);
            var permissions = _bllusermaster.GetUserPermissionsTypeId(typeid, username);
            if (permissions != null)
            {
                // LoginViewModel model = new LoginViewModel();
                //   model.Email = Session["loggeduser"].ToString();
                //  model.Password = Session["loggeduserpw"].ToString();                         
                // SysUserMaster sysUserMaster = _bllusermaster.GetUserDetails(model, Convert.ToInt32(Session["loggeduserlocId"]));
                //   string Rols = _bllusermaster.GetUserRolesByUsername(sysUserMaster);

                FormsAuthentication.SetAuthCookie(username, false);
                var authTicket = new FormsAuthenticationTicket(1, "", DateTime.Now,
                                                                      DateTime.Now.AddDays(-1), false, permissions);

                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                // var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);


                HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket));

                var authTicket1 = new FormsAuthenticationTicket(1, username, DateTime.Now,
                                                                   DateTime.Now.AddDays(1), false,
                                                                   permissions,
                                                                   FormsAuthentication.FormsCookieName
                                                                   );

                string encryptedTicket1 = FormsAuthentication.Encrypt(authTicket1);
                //  authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket1);
                HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket1));


                //string[] allCookies = Request.Cookies.AllKeys;
                //foreach (string cookie in allCookies)
                //{
                //  var sss =   Response.Cookies[cookie].Value;
                //}


                //   var sss = Response.Cookies[".ASPXAUTH"].Value;
                //  FormsAuthentication.SetAuthCookie(username, false);
                //HttpContext.Response.Cookies.Remove(".ASPXAUTH");
                //if (HttpContext.Response.Cookies.AllKeys.Contains(".ASPXAUTH"))
                //{
                //     bool k = true;
                //     HttpContext.Response.Cookies.Clear();
                //    HttpContext.Response.Cookies.Remove(".ASPXAUTH");
                // }
                // else
                // {

                //   FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, username, DateTime.Now,
                //    DateTime.Now.AddDays(1), false, permissions.Trim(),
                //   FormsAuthentication.FormsCookiePath);

                //string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                //HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);               
                //HttpContext.Response.Cookies.Add(authCookie);

                //  HttpCookie userInfo = new HttpCookie("userpermissions");
                //   userInfo["PermissionList"] = permissions;

                //  userInfo.Expires.Add(new TimeSpan(24, 0, 0));
                //  Response.Cookies.Add(userInfo);

                //  }
                // HttpContext.Response.AppendCookie(authCookie);

            }

        }



        //
        // POST: /Account/Login


        //public ActionResult Login1(LoginViewModel model, string returnUrl)
        //{

        //    if (!ModelState.IsValid)
        //    {
        //        ModelState.Clear();
        //        if (string.IsNullOrEmpty(model.Email))
        //        {
        //            ModelState.AddModelError("Email", "Username Required");
        //        }
        //        else if (string.IsNullOrEmpty(model.Password))
        //        {
        //            ModelState.AddModelError("Password", "Password Required");
        //        }
        //        model.Authorized = true;
        //        return View(model);
        //    }


        //    var actualdb = _bllcompanymanager.GetActualConnectionName(model.Email);
        //    ConnectionManager.CurrentConnectionName = actualdb;
        //    BLL_UserMaster _bllusermaster = new BLL_UserMaster();
        //    SysUserMaster sysUserMaster = _bllusermaster.GetUserDetails(model, 0);


        //    if (sysUserMaster != null)
        //    {
        //        string Rols = _bllusermaster.GetUserRolesByUsername(sysUserMaster);
        //        if (Rols != "")
        //        {

        //            FormsAuthentication.SetAuthCookie(model.Email, false);

        //            var authTicket = new FormsAuthenticationTicket(1, model.Email, DateTime.Now,
        //                                                            DateTime.Now.AddDays(1), false,
        //                                                            Rols.Trim(),
        //                                                            FormsAuthentication.FormsCookieName
        //                                                            );



        //            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
        //            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
        //            HttpContext.Response.Cookies.Add(authCookie);

        //            var sss = Response.Cookies[".ASPXAUTH"].Value;


        //            //  SetPermissionCookie(2, sysUserMaster.EmployeeCode);
        //            Session["loggeduserid"] = sysUserMaster.SysUserMasterID;
        //            Session["loggeduser"] = sysUserMaster.UserName;
        //            Session["loggeduserempcode"] = sysUserMaster.EmployeeCode;
        //            Session["loggeduserpw"] = sysUserMaster.Password;
        //            Session["loggeduserlocId"] = sysUserMaster.LocationId;
        //            Session["loggedusergroupId"] = sysUserMaster.UserGroupID;
        //            Session["loggedusercompanyId"] = sysUserMaster.CompanyID;
        //            Session["loggedusergorupofcompanyId"] = sysUserMaster.GroupOfCompanyID;
        //            Session["loggedusercompany"] = _bllcompany.GetCompanyById(sysUserMaster.CompanyID).CompanyName;

        //            //  var ddd = Session["loggeduser"].ToString();
        //            ViewBag.LoggedUser = (Session["loggeduser"].ToString());
        //            return RedirectToAction("Index", "Home");

        //        }
        //        else
        //        {
        //            ModelState.AddModelError("Password", "Invalid login attempt.");
        //            model.Email = string.Empty;
        //            model.Password = string.Empty;
        //            model.Authorized = true;
        //            return View(model);
        //        }
        //    }
        //    else
        //    {
        //        ModelState.AddModelError("Password", "Invalid login attempt.");
        //        model.Email = string.Empty;
        //        model.Password = string.Empty;
        //        model.Authorized = true;
        //        return View(model);
        //    }
        //}


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {

            var mode1 = ConfigurationManager.AppSettings["SubscriptionMode"];
            if (mode1 == "OFF")
            {

                ConnectionManager.CurrentConnectionName = "HMS_Default";
                Session["CurrentConnectionName"] = "HMS_Default";
            }
            else if (mode1 == "ON")
            {
                ConnectionManager.CurrentConnectionName = "HMSLoginManager";
                Session["CurrentConnectionName"] = "HMSLoginManager";
            }
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllcompanymanager = new BLL_ConnectionManager(Convert.ToString(Session["CurrentConnectionName"]));
            _bllcompany = new BLL_Company(Convert.ToString(Session["CurrentConnectionName"]));
            _bllconfigurations = new BLL_SysConfiguration(Convert.ToString(Session["CurrentConnectionName"]));

            _blllocation = new BLL_Location(cn);
            //  MvcApplication.MyProperty

            try
            {

                if (!ModelState.IsValid)
                {
                    ModelState.Clear();
                    if (string.IsNullOrEmpty(model.Email))
                    {
                        ModelState.AddModelError("Email", "Username Required");
                    }
                    else if (string.IsNullOrEmpty(model.Password))
                    {
                        ModelState.AddModelError("Password", "Password Required");
                    }
                    model.Authorized = true;
                    return View(model);
                }

                var mode = ConfigurationManager.AppSettings["SubscriptionMode"];
                string actualdb = "";
                if (mode == "ON")
                {
                    actualdb = _bllcompanymanager.GetActualConnectionName(model.Email);
                    ConnectionManager.CurrentConnectionName = actualdb;
                    Session["CurrentConnectionName"] = actualdb;

                }
                else if (mode == "OFF")
                {
                    actualdb = ConnectionManager.CurrentConnectionName;
                    actualdb = Convert.ToString(Session["CurrentConnectionName"]);
                }


                var servername = HttpContext.Server.MachineName;
                // servername = "SERVER";
                // servername = "VMI522335";
                BLL_SysConfiguration bllsysconfiguration = new BLL_SysConfiguration(actualdb);
                var sysname = bllsysconfiguration.GetServerName();

                if (servername == servername)
                 {
                    BLL_UserMaster _bllusermaster = new BLL_UserMaster(actualdb);
                    SysUserMaster sysUserMaster = _bllusermaster.GetUserDetails(model, 0);
                    if (sysUserMaster != null)
                    {
                        string Rols = _bllusermaster.GetUserRolesByUsername(sysUserMaster);
                        if (Rols != "")
                        {
                            BLL_Company company = new BLL_Company(actualdb);
                            string CompanyName = company.GetCompanyById(sysUserMaster.CompanyID).CompanyName;
                            Session["loggedusercompany"] = CompanyName;

                            // New Lisence Validation Start
                            if (!CheckExpiryFile(CompanyName))
                            {
                                model.Email = string.Empty;
                                model.Password = string.Empty;
                                model.Authorized = true;
                                return View(model);
                            }
                            // / New Lisence Validation End

                            FormsAuthentication.SetAuthCookie(model.Email, false);

                            var authTicket = new FormsAuthenticationTicket(1, model.Email, DateTime.Now,
                                                                            DateTime.Now.AddDays(1), false,
                                                                            Rols.Trim(),
                                                                            FormsAuthentication.FormsCookieName
                                                                            );

                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                            HttpContext.Response.Cookies.Add(authCookie);

                            var sss = Response.Cookies[".ASPXAUTH"].Value;

                            //  SetPermissionCookie(2, sysUserMaster.EmployeeCode);
                            Session["loggeduserid"] = sysUserMaster.SysUserMasterID;
                            Session["loggeduser"] = sysUserMaster.UserName;
                            Session["loggeduserempcode"] = sysUserMaster.EmployeeCode;
                            Session["loggeduserpw"] = sysUserMaster.Password;
                            Session["loggeduserlocId"] = sysUserMaster.LocationId;
                            Session["loggedusergroupId"] = sysUserMaster.UserGroupID;
                            Session["loggedusercompanyId"] = sysUserMaster.CompanyID;
                            Session["loggedusergorupofcompanyId"] = sysUserMaster.GroupOfCompanyID;

                            var lc= _blllocation.GetLocationById(sysUserMaster.LocationId);

                            if (lc.IsHeadOffice)
                                Session["IsHeadOffice"] = true;
                            else
                                Session["IsHeadOffice"] = false;



                            BLL_Configuration bllconfiguration = new BLL_Configuration(actualdb);
                            var dburl = bllconfiguration.GetConfiguration("DURL", sysUserMaster.CompanyID);
                            if (dburl != null)
                            {
                                Session["DURL"] = dburl.ConfigurationDescription;
                            }
                            else
                            {
                                Session["DURL"] = "";
                            }
                            MvcApplication.MyProperty = actualdb;
                            ViewBag.LoggedUser = (Session["loggeduser"].ToString());
                            return RedirectToAction("Index", "Home");

                        }
                        else
                        {
                            ModelState.AddModelError("Password", "Invalid login attempt.");
                            model.Email = string.Empty;
                            model.Password = string.Empty;
                            model.Authorized = true;
                            return View(model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "Invalid login attempt.");
                        model.Email = string.Empty;
                        model.Password = string.Empty;
                        model.Authorized = true;
                        return View(model);
                    }

                }
                else
                {
                    ModelState.AddModelError("Password", "Unauthorized License Key Detected..!");
                    model.Email = string.Empty;
                    model.Password = string.Empty;
                    model.Authorized = true;
                    return View(model);
                }

            }
          
            catch (Exception e)
            {
                ModelState.AddModelError("Password", e.Message);
                model.Email = string.Empty;
                model.Password = string.Empty;
                model.Authorized = true;
                return View(model);
            }
        }

        #region Check Expiry File
        //Check Expiry File
        private bool CheckExpiryFile(string strLoggedCompanyName)
        {
            try
            {
                bool Access = false;
                string[] HideInfo;
                string strFilePath = Server.MapPath(@"~/ACTIVATIONKEY.dbf");
                bool isExis = false;
                if (System.IO.File.Exists(strFilePath))
                {
                    HideInfo = ReadFile(strFilePath).Split(';');

                    long DiffDays;
                    int DaysToEnd;
                    int _DefDays;
                    string macAddresses = "";
                    string strSoftwareName = HideInfo[0];                       // Software Name - "HMS"
                    string strIdentifier = HideInfo[1];                         // Identifier - 333
                    DateTime dt = new DateTime(Convert.ToInt64(HideInfo[2]));   // Date of Activated
                    DaysToEnd = Convert.ToInt32(HideInfo[3]);                   // TrialDays
                    string strMACAdd = HideInfo[4];                             // MAC Address
                    string strCompanyName = HideInfo[5];                        // Company Name
                    if (macAddresses == "")
                    {
                        macAddresses = GetMACAddress1(strMACAdd);
                    }
                    if (strSoftwareName == "HMS" && strIdentifier == "333" && strLoggedCompanyName == strCompanyName && macAddresses == strMACAdd)
                    {
                        if (DaysToEnd <= 0)
                        {
                            Access = true;
                            if (dt.Date > DateTime.Now.Date)
                            {
                                ModelState.AddModelError("Password", "INVALID SYSTEM ACTIVATION CONFIGURATION. \r\nPLEASE CONTACT THE RIT SUPPORT TEAM FOR ASSISTANCE.");
                                return false;
                            }
                        }
                        else
                        {
                            TimeSpan diff1 = DateTime.Now.Date - dt.Date;
                            DiffDays = diff1.Days;
                            DiffDays = Math.Abs(DiffDays);

                            _DefDays = DaysToEnd - Convert.ToInt32(DiffDays);
                            if (dt.Date > DateTime.Now.Date)
                            {
                                ModelState.AddModelError("Password", "INVALID SYSTEM ACTIVATION CONFIGURATION. \r\nPLEASE CONTACT THE RIT SUPPORT TEAM FOR ASSISTANCE.");
                                return false;
                            }
                            if (_DefDays > 0)
                            {
                                Access = true;
                                if (_DefDays <= 7)
                                {
                                    ModelState.AddModelError("Password", "TRIAL PERIOD REMAINING: " + _DefDays + " Day(s)");
                                    TempData["LicenseMessage"] = "TRIAL PERIOD REMAINING: " + _DefDays + " Day(s)";
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("Password", "TRIAL PERIOD EXPIRED");
                                return false;
                            }
                        }
                    }
                    if (Access)
                    {
                        return true;
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "INVALID SYSTEM ACTIVATION CONFIGURATION. \r\nPLEASE CONTACT THE RIT SUPPORT TEAM FOR ASSISTANCE.");
                        return false;
                    }
                }
                else
                {
                    ModelState.AddModelError("Password", "SYSTEM ACTIVATION CONFIGURATION MISSING. \r\nPLEASE CONTACT THE RIT SUPPORT TEAM FOR ASSISTANCE.");
                    return false;
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("Password", "UNAUTHORIZED LICENSE KEY DETECTED..!");
                return false;
            }
        }

        public static string GetMACAddress1(string strMAC)
        {
            System.Management.ManagementObjectSearcher objMOS = new System.Management.ManagementObjectSearcher("Select * FROM Win32_NetworkAdapterConfiguration");
            System.Management.ManagementObjectCollection objMOC = objMOS.Get();
            string macAddress = String.Empty;
            foreach (System.Management.ManagementObject objMO in objMOC)
            {
                object tempMacAddrObj = objMO["MacAddress"];

                if (tempMacAddrObj == null) //Skip objects without a MACAddress
                {
                    continue;
                }
                if (macAddress == String.Empty) // only return MAC Address from first card that has a MAC Address
                {
                    macAddress = tempMacAddrObj.ToString();
                    if (strMAC == macAddress.Replace(":", ""))
                    {
                        break;
                    }
                    macAddress = String.Empty;
                }
                objMO.Dispose();
            }
            macAddress = macAddress.Replace(":", "");
            return macAddress;
        }

        // LEGACY-SECRET-SCRUBBED: the original 24-byte TripleDES key and zero IV were
        // hardcoded here, making the licence file ACTIVATIONKEY.dbf trivially decryptable
        // by anyone with source access. Keys removed; runtime now loads from environment.
        // The v2 cloud rewrite replaces this entire licence-key flow with subscription-
        // based auth via Microsoft Entra External ID. See /SECURITY.md and
        // /docs/v2-multi-tenancy.md.
        public static byte[] key = LoadLicenceKeyOrFail("HMS_LICENCE_KEY");          // 24 bytes, base64 in env
        private static byte[] iv  = LoadLicenceKeyOrFail("HMS_LICENCE_IV");          // 8 bytes,  base64 in env

        private static byte[] LoadLicenceKeyOrFail(string envName)
        {
            var b64 = Environment.GetEnvironmentVariable(envName);
            if (string.IsNullOrEmpty(b64))
            {
                // Legacy local-dev fallback: 24/8 zero bytes. Will not decrypt real
                // ACTIVATIONKEY.dbf files. Prod must set the env var.
                return envName.EndsWith("_IV") ? new byte[8] : new byte[24];
            }
            return Convert.FromBase64String(b64);
        }

        public static string ReadFile(string FilePath)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(FilePath);
            if (fi.Exists == false)
                return string.Empty;

            System.IO.FileStream fin = new System.IO.FileStream(FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.Security.Cryptography.TripleDES tdes = new System.Security.Cryptography.TripleDESCryptoServiceProvider();
            System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(fin, tdes.CreateDecryptor(key, iv), System.Security.Cryptography.CryptoStreamMode.Read);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            int ch;
            for (int i = 0; i < fin.Length; i++)
            {
                ch = cs.ReadByte();
                if (ch == 0)
                    break;
                SB.Append(Convert.ToChar(ch));
            }

            cs.Close();
            fin.Close();
            return SB.ToString();
        }
        #endregion

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel
            {
                Provider = provider,
                ReturnUrl = returnUrl,
                RememberMe = rememberMe
            });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            //if (ModelState.IsValid)
            //{
            //    var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            //    var result = await UserManager.CreateAsync(user, model.Password);
            //    if (result.Succeeded)
            //    {
            //        await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

            //        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            //        // Send an email with this link
            //        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            //        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
            //        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

            //        return RedirectToAction("Index", "Home");
            //    }
            //    AddErrors(result);
            //}

            //// If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                //    return RedirectToAction("Index", "Manage");
                //}

                //if (ModelState.IsValid)
                //{
                //    // Get the information about the user from the external login provider
                //    var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                //    if (info == null)
                //    {
                //        return View("ExternalLoginFailure");
                //    }
                //    var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                //    var result = await UserManager.CreateAsync(user);
                //    if (result.Succeeded)
                //    {
                //        result = await UserManager.AddLoginAsync(user.Id, info.Login);
                //        if (result.Succeeded)
                //        {
                //            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                //            return RedirectToLocal(returnUrl);
                //        }
                //    }
                //    AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Home(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            @ViewBag.Permissions = "Invalid User Permissions";
            return View("~/Views/Account/AccessDenied.cshtml");
            // return View("~/Views/Account/Login.cshtml");
            // return RedirectToAction("Index", "Home");
        }
        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}