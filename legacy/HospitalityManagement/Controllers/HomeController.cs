using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.BLL.TransactionData;
using System.Globalization;
using System.Threading.Tasks;

namespace HospitalityManagement.Controllers
{
    [Authorize]
    [SessionTimeout]
    public class HomeController : Controller
    {
       private readonly BLL_Dashboard _dashBoardService;
       private readonly BLL_Location _locationService;
       private readonly BLL_Customer _customerService;
       private readonly BLL_CateringMood _cartreringmoods;

        public HomeController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _dashBoardService = new BLL_Dashboard(cn);
            _locationService = new BLL_Location(cn);
            _customerService = new BLL_Customer(cn);
            _cartreringmoods = new BLL_CateringMood(cn);
      }

        [Authorize(Roles ="Home")]
        public void AdvancedDashboard()
        {
            var ss = @Session["DURL"].ToString();
            Response.Redirect(Session["DURL"].ToString());
        }
        [Authorize(Roles = "Home")]
        public ActionResult Index()
        {

                int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());            
                // belongs to old dashboard version 
               DashboardViewModel dvm = new DashboardViewModel();
             //   dvm.POCount = _dashBoardService.GetPOCountToday(1, companyid);   // has to pass user  id   
             //   dvm.ProductionCount = _dashBoardService.GetProductionCountToday(1, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
             //   dvm.TOGCount = _dashBoardService.GetTOGCountToday(1, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));

              //  dvm.POCountThisWeek = _dashBoardService.GetPOCountThisWeek(1, companyid);
              //  dvm.ProductionCountThisWeek = _dashBoardService.GetProductionCountThisWeek(1, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
              //  dvm.TOGCountThisWeek = _dashBoardService.GetTOGCountThisWeek(1, Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            // belongs to old dashboard version 

            List<SelectListItem> LocationId = new List<SelectListItem>();
            SelectListItem defaultloc = new SelectListItem();
            defaultloc.Text = "-- All Locations --";
            defaultloc.Value ="0";
            LocationId.Add(defaultloc);
           
            foreach (var loc in _locationService.GetActiveLocations(companyid))
            {
                SelectListItem dbloc = new SelectListItem();
                dbloc.Text = loc.LocationName;
                dbloc.Value = loc.SysLocationID.ToString();
                LocationId.Add(dbloc);
            }
            ViewBag.LocationId = LocationId;

            List<SelectListItem> CustomerGroupIds = new List<SelectListItem>();
            SelectListItem CustomerGroupId = new SelectListItem();
            CustomerGroupId.Text = "-- All Customer Groups --";
            CustomerGroupId.Value = "0";
            CustomerGroupIds.Add(CustomerGroupId);

            foreach (var cat in _customerService.GetActiveCustomerCategories(companyid))
            {
                SelectListItem dbcat = new SelectListItem();
                dbcat.Text = cat.CustomerCategoryName;
                dbcat.Value = cat.CustomerCategoryID.ToString();
                CustomerGroupIds.Add(dbcat);
            }

            ViewBag.CustomerGroupIds = CustomerGroupIds;

            List<SelectListItem> CateringMoods = new List<SelectListItem>();
            SelectListItem CateringMood = new SelectListItem();
            CateringMood.Text = "-- All Customer Groups --";
            CateringMood.Value = "0";
            CateringMoods.Add(CateringMood);
            foreach (var dbmode in _cartreringmoods.GetByCateringMoods(Convert.ToInt32(Session["loggedusercompanyId"].ToString())))
            {
                SelectListItem mode = new SelectListItem();
                mode.Text = dbmode.CateringMoodName;
                mode.Value = dbmode.CateringMoodID.ToString();
                CateringMoods.Add(mode);
            }

            ViewBag.CateringMoods = CateringMoods;
            return View(dvm);      
        }

        [WebMethod]
        public async Task<JsonResult> RevenueAndCost(int locationid,DateTime datefrom,DateTime dateto,
                                                    int customergroupid,string selectiontypeid)
        {
            // Create parameters on the main thread (lightweight operation)
            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.CustomerGroupId = customergroupid;
            parms.DepartmentId = 0;
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (selectiontypeid == "Daily")
            {
                parms.TypeId = 1;
            }
            else if (selectiontypeid == "Weekly") { parms.TypeId = 2; }
            else if (selectiontypeid == "Monthly") { parms.TypeId = 3; }

            // Execute the database operation on a separate thread
            var results = await Task.Run(() =>
            {
                List<DashboardViewModel.RevenueVsCost> listrevenueandcost = new List<DashboardViewModel.RevenueVsCost>();

                foreach (var reve in _dashBoardService.ExecRevenueAndCostSP(parms))
                {
                    listrevenueandcost.Add(new DashboardViewModel.RevenueVsCost
                    {
                        recdate = reve.recdate,
                        Nett = reve.Nett,
                        Cost = reve.Cost,
                        day = reve.recdate
                    });
                }
                return listrevenueandcost;
            });
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [WebMethod]
        public async Task<ActionResult> RevenueAndCostRowData(
    int locationid,
    DateTime datefrom,
    DateTime dateto,
    int customergroupid,
    string selectiontypeid,
    int deptid)
        {
            // Create parameters on the main thread (lightweight operation)
            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.CustomerGroupId = customergroupid;
            parms.DepartmentId = deptid;
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (selectiontypeid == "Daily")
            {
                parms.TypeId = 1;
            }
            else if (selectiontypeid == "Weekly") { parms.TypeId = 2; }
            else if (selectiontypeid == "Monthly") { parms.TypeId = 3; }

            // Execute the database operation and processing on a separate thread
            var listrevenueandcost = await Task.Run(() =>
            {
                var results = new List<DashboardViewModel.RevenueVsCost>();

                foreach (var reve in _dashBoardService.ExecRevenueAndCostSP(parms))
                {
                    results.Add(new DashboardViewModel.RevenueVsCost
                    {
                        recdate = reve.recdate,
                        Nett = reve.Nett,
                        Cost = reve.Cost,
                        day = reve.recdate
                    });
                }
                return results;
            });

            return View("~/Views/Home/DashBoardRowData.cshtml", listrevenueandcost);
        }

        [WebMethod]
        public async Task<JsonResult> OrderTypesVariation(int locationid, DateTime datefrom, DateTime dateto,
                                        int customergroupid, string selectiontypeid)
        {
            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.CustomerGroupId = customergroupid;
            parms.DepartmentId = 0;
            parms.CompanyId= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (selectiontypeid == "Daily")
            {
                parms.TypeId = 1;
            }
            else if (selectiontypeid == "Weekly") { parms.TypeId = 2; }
            else if (selectiontypeid == "Monthly") { parms.TypeId = 3; }

            // Execute the database operation on a background thread
            var listordertypevariation = await Task.Run(() =>
            {
                var result = new List<DashboardViewModel.OrderTypesVariation>();

                foreach (var order in _dashBoardService.ExecNumberOfOrdersSp(parms))
                {
                    result.Add(new DashboardViewModel.OrderTypesVariation
                    {
                        recdate = order.recdate,
                        KOTCount = order.KOTCount,
                        BOTCount = order.BOTCount,
                        NoneCount = order.NoneCount
                    });
                }
                return result;
            });

            return Json(listordertypevariation, JsonRequestBehavior.AllowGet);

        }

        [WebMethod]
        public async Task<JsonResult> OrderTypesWiseProducts(int locationid, DateTime datefrom, DateTime dateto,
                                        int customergroupid, string ordertypeid,string charttypeid)
        {
            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.CustomerGroupId = customergroupid;
            parms.DepartmentId = 0;
            parms.TypeId = 0;
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (charttypeid == "Daily")
            {
                parms.TypeId = 1;
            }
            else if (charttypeid == "Weekly") { parms.TypeId = 2; }
            else if (charttypeid == "Monthly") { parms.TypeId = 3; }



            if (ordertypeid == "KOT")
            {
                parms.OrderTypeId = 1;
            }
            else if (ordertypeid == "BOT") { parms.OrderTypeId = 2; }
            else if (ordertypeid == "None") { parms.OrderTypeId = 3; }

            // Execute both database operations in parallel
            var ordersTask = Task.Run(() => _dashBoardService.ExecNumberOfOrdersSp(parms));
            var productsTask = Task.Run(() => _dashBoardService.ExecOrderTypeWiseProductSalesSp(parms));

            await Task.WhenAll(ordersTask, productsTask);

            var orders = ordersTask.Result;
            var allProducts = productsTask.Result;

            // Process results
            var listordertypevariation = new List<DashboardViewModel.OrderTypesVariation>();
            foreach (var order in orders)
            {
                var ordertypes = new DashboardViewModel.OrderTypesVariation
                {
                    recdate = order.recdate,
                    KOTCount = order.KOTCount,
                    BOTCount = order.BOTCount,
                    NoneCount = order.NoneCount,
                    Products = allProducts.Where(p => p.recdate == order.recdate).ToList()
                };
                listordertypevariation.Add(ordertypes);
            }

            return Json(listordertypevariation, JsonRequestBehavior.AllowGet);

        }

        [WebMethod]
        public async Task<JsonResult> GetValueBasedProducts(int locationid, DateTime datefrom, DateTime dateto,
                                     int customergroupid, string ordertypeid, string charttypeid,string valueid)
        {
            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.CustomerGroupId = customergroupid;
            parms.DepartmentId = 0;
            parms.TypeId = 0;

            if (charttypeid == "Daily")
            {
                parms.TypeId = 1;
            }
            else if (charttypeid == "Weekly") { parms.TypeId = 2; }
            else if (charttypeid == "Monthly") { parms.TypeId = 3; }

            if (ordertypeid == "KOT")
            {
                parms.OrderTypeId = 1;
            }
            else if (ordertypeid == "BOT") { parms.OrderTypeId = 2; }
            else if (ordertypeid == "None") { parms.OrderTypeId = 3; }
            else if (ordertypeid == "All") { parms.OrderTypeId = 0; }

            var productlist = await Task.Run(() =>
            {
                var products = _dashBoardService.ExecOrderTypeWiseProductSalesSp(parms)
                                  .Where(p => p.recdate == valueid)
                                  .ToList();

                // Add valueid to each product
                products.ForEach(p => { p.Value = valueid; });

                return products;
            });

            return Json(productlist, JsonRequestBehavior.AllowGet);

        }

        [WebMethod]
        public async Task<JsonResult> OrderTypeBreakdown(int locationid, DateTime datefrom, DateTime dateto,
                                    int customergroupid)
        {
            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.CustomerGroupId = customergroupid;
            parms.DepartmentId = 0;
            parms.TypeId = 0;
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //if (charttypeid == "Daily")
            //{
            //    parms.TypeId = 1;
            //}
            //else if (charttypeid == "Weekly") { parms.TypeId = 2; }
            //else if (charttypeid == "Monthly") { parms.TypeId = 3; }

            var datalist = await Task.Run(() =>
            {
                return _dashBoardService.ExecOrderTypeWiseSales(parms).ToList();
            });

            return Json(datalist, JsonRequestBehavior.AllowGet);

        }

        [WebMethod]
        public async Task<JsonResult> DeptOrderTypeWiseSales(int locationid, DateTime datefrom, DateTime dateto,
                                   int customergroupid,int  cateringmodeid)
        {
            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.CustomerGroupId = customergroupid;
            parms.DepartmentId = 0;
            parms.TypeId = 0;
            parms.CateringModeId = cateringmodeid;
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            // var datalist = _dashBoardService.DeptOrderTypeWiseSales(parms).ToList();

            var departmentOrders = await Task.Run(() => _dashBoardService.DeptOrderTypeWiseSales(parms));

            // Process each order in parallel
            var listdiptordertypesales = new List<DashboardViewModel.DeptOrderTypeWiseSales>();

            var tasks = departmentOrders.Select(async order =>
            {
                var deptParms = new DashboardViewModel.DashboardParms
                {
                    FromDate = parms.FromDate,
                    ToDate = parms.ToDate,
                    LocationId = parms.LocationId,
                    CustomerGroupId = parms.CustomerGroupId,
                    DepartmentId = 2, // Hardcoded as in original
                    TypeId = parms.TypeId,
                    CateringModeId = parms.CateringModeId,
                    CompanyId = parms.CompanyId,
                    OrderTypeId = order.OrderTypeId // Include order type for product filtering
                };

                var products = await Task.Run(() => _dashBoardService.ProductWiseSalesByDept(deptParms));

                return new DashboardViewModel.DeptOrderTypeWiseSales
                {
                    DeptId = order.DeptId,
                    DeptName = order.DeptName,
                    OrderTypeId = order.OrderTypeId,
                    OrderType = order.OrderType,
                    Nett = order.Nett,
                    Products = products.ToList()
                };
            });

            listdiptordertypesales.AddRange(await Task.WhenAll(tasks));

            return Json(listdiptordertypesales, JsonRequestBehavior.AllowGet);
        }

        [WebMethod]
        public async Task<JsonResult> DeptWiseSale(int locationid, DateTime datefrom, DateTime dateto,
                                 int customergroupid,string recordcount)
        {
            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.CustomerGroupId = customergroupid;
            parms.DepartmentId = 0;
            parms.TypeId = 0;
            parms.CateringModeId = 0;
            parms.RecordCount = Convert.ToInt16(recordcount);
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            var departmentSales = await Task.Run(() => _dashBoardService.DeptWiseSales(parms));

            // Process each department in parallel
            var salesTasks = departmentSales.Select(async order =>
            {
                var deptParms = new DashboardViewModel.DashboardParms
                {
                    FromDate = parms.FromDate,
                    ToDate = parms.ToDate,
                    LocationId = parms.LocationId,
                    CustomerGroupId = parms.CustomerGroupId,
                    DepartmentId = 2, // Hardcoded as per original
                    TypeId = parms.TypeId,
                    CateringModeId = parms.CateringModeId,
                    RecordCount = parms.RecordCount,
                    CompanyId = parms.CompanyId,
                    OrderTypeId = order.OrderTypeId // Preserve order type
                };

                var products = await Task.Run(() => _dashBoardService.DeptWiseProductsSale(deptParms));

                return new DashboardViewModel.DeptOrderTypeWiseSales
                {
                    DeptId = order.DeptId,
                    DeptName = order.DeptName,
                    OrderTypeId = order.OrderTypeId,
                    OrderType = order.OrderType,
                    Nett = order.Nett,
                    Products = products.ToList()
                };
            });

            // Wait for all department processing to complete
            var listdiptordertypesales = (await Task.WhenAll(salesTasks)).ToList();

            return Json(listdiptordertypesales, JsonRequestBehavior.AllowGet);

        }

        [WebMethod]
        public async Task<JsonResult> WaistageSummary(int locationid, DateTime datefrom, DateTime dateto,
                                string charttype, string ordertype)
        {
            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.CustomerGroupId = 0;
            parms.DepartmentId = 0;
            parms.TypeId = 0;
            parms.CateringModeId = 0;
            parms.RecordCount =0;
            parms.CompanyId= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            if (charttype == "Daily")
            {
                parms.TypeId = 1;
            }
            else if (charttype == "Weekly") { parms.TypeId = 2; }
            else if (charttype == "Monthly") { parms.TypeId = 3; }

            if (ordertype == "KOT")
            {
                parms.OrderTypeId = 1;
            }
            else if (ordertype == "BOT") { parms.OrderTypeId = 2; }
            else if (ordertype == "None") { parms.OrderTypeId = 3; }
            else if (ordertype == "All") { parms.OrderTypeId = 0; }

            var summaries = await _dashBoardService.WaistageSummary(parms);
            var allDetails = await _dashBoardService.WaistageDetail(parms);

            // Process results
            var waistageList = summaries.Select(order => new DashboardViewModel.Waistage
            {
                recdate = order.recdate,
                Nett = order.Nett,
                Products = allDetails
                          .Where(d => d.recdate == order.recdate)
                          .ToList()
            }).ToList();

            return Json(waistageList, JsonRequestBehavior.AllowGet);
        }

        [WebMethod]
        public async Task<JsonResult> HourlySales(int locationid, DateTime datefrom, DateTime dateto,
                            string timefrom, string timeto)
        {
       
            TimeSpan time1 = TimeSpan.Parse(timefrom);
            TimeSpan time2 = TimeSpan.Parse(timeto);

            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.TimeFrom = time1;
            parms.TimeTo = time2;
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            var hourlySalesData = await Task.Run(() => _dashBoardService.HourlySales(parms));

            return Json(hourlySalesData, JsonRequestBehavior.AllowGet);

        }


        [WebMethod]
        public async Task<JsonResult> HourlySalesTabular(int locationid, DateTime datefrom)
        {
            // taking first monday of the week
            // DateTime userdate = Convert.ToDateTime("17/08/2020");
            DateTime userdate = datefrom.Date;
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var diff = userdate.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            var actualfirstdate = userdate.AddDays(-diff).Date;
            DateTime returndate;
            returndate = actualfirstdate.AddDays(1);
            if (userdate < returndate)
            {
                returndate = userdate.AddDays(-6).Date;
            }

            var finalval = returndate;
            //

            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = finalval;         
            parms.LocationId = locationid;
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            var hourlyData = await Task.Run(() =>
            {
                var data = _dashBoardService.HourlySalesTabular(parms);

                // Process data on background thread
                data.ForEach(d =>
                {
                    d.ActualDate = finalval.Date.ToShortDateString();
                    d.ActualDay = finalval.DayOfWeek.ToString();
                });

                return data;
            });

            return Json(hourlyData, JsonRequestBehavior.AllowGet);

        }

        [WebMethod]
        public async Task<JsonResult> TimeConsumption(int locationid, DateTime datefrom, DateTime dateto,
                                           string timefrom, string timeto,int mode)
        {


            TimeSpan time1 = TimeSpan.Parse(timefrom);
            TimeSpan time2 = TimeSpan.Parse(timeto);

            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.TimeFrom = time1;
            parms.TimeTo = time2;
            parms.CateringModeId = mode;
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            var timeConsumptionData = await Task.Run(() => _dashBoardService.TimeConsumption(parms));

            return Json(timeConsumptionData, JsonRequestBehavior.AllowGet);

        }


        [WebMethod]
        public async Task<JsonResult> FoodCost(int locationid, DateTime datefrom, DateTime dateto)
        {
            
            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            parms.TypeId = 0;

            var foodCostData = await Task.Run(() => _dashBoardService.FoodCost(parms));

            return Json(foodCostData, JsonRequestBehavior.AllowGet);
        }

        [WebMethod]
        public async Task<JsonResult> DeptWiseFoodCost(int locationid, DateTime datefrom, DateTime dateto)
        {

            DashboardViewModel.DashboardParms parms = new DashboardViewModel.DashboardParms();
            parms.FromDate = datefrom;
            parms.ToDate = dateto;
            parms.LocationId = locationid;
            parms.CompanyId = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            parms.TypeId = 1;

            var deptFoodCostData = await Task.Run(() => _dashBoardService.FoodCost(parms));

            return Json(deptFoodCostData, JsonRequestBehavior.AllowGet);
        }

        [WebMethod]
        public async Task<JsonResult> Top10Productions(long id)
        {
            long locationId = Convert.ToInt64(Session["loggeduserlocId"]);

            // Execute database operation and processing on background thread
            var top10list = await Task.Run(() =>
            {
                var results = new List<DashboardViewModel.Top10Productions>();

                foreach (var prd in _dashBoardService.GetTop10Productions(locationId))
                {
                    results.Add(new DashboardViewModel.Top10Productions
                    {
                        ProductName = prd.ProductName,
                        ProductCount = prd.ProductCount
                    });
                }
                return results;
            });

            return Json(top10list, JsonRequestBehavior.AllowGet);

        }

        [Authorize(Roles = "About")]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [Authorize(Roles = "Admins")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}