using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Data;
using RIT.HMS.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RIT.HMS.BLL.MasterData
{
    public class BLL_Dashboard
    {

        //    ApplicationDbContext context = new ApplicationDbContext();
        BLL_PurchaseOrder _bllorder;
        BLL_ProductionNote _bllproductionnote;
        BLL_TransferNote _blltransfernote;
        BLL_Product _bllproduct;
        private readonly UnitOfWork _unitofwork;

        public BLL_Dashboard()
        {
            _unitofwork = new UnitOfWork();
            _bllorder = new BLL_PurchaseOrder();
            _bllproductionnote = new BLL_ProductionNote();
            _blltransfernote = new BLL_TransferNote();
            _bllproduct = new BLL_Product();
        }
        public BLL_Dashboard(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
            _bllorder = new BLL_PurchaseOrder(connectionname);
            _bllproductionnote = new BLL_ProductionNote(connectionname);
            _blltransfernote = new BLL_TransferNote(connectionname);
            _bllproduct = new BLL_Product(connectionname);
        }
        public int GetPOCountToday(int loggeduserid,int companyid)
        {
            try
            {
                DateTime date = DateTime.Today.Date;
                var pos = _bllorder.GetTodayPos(date, companyid).ToList();

                if (pos == null)
                {
                    return 0;
                }
                else
                {
                    return pos.Count();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public int GetPOCountThisWeek(int loggeduserid,int companyid)
        {
            try
            {

                DateTime fromdate = DateTime.Today.Date.AddDays(-7);
                DateTime todate = DateTime.Today.Date;
                var pos = _bllorder.GetThisweekPos(fromdate,todate, companyid).ToList();

                if (pos == null)
                {
                    return 0;
                }
                else
                {
                    return pos.Count();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public int GetProductionCountToday(int loggeduserid,int companyid)
        {
            try
            {
                DateTime date = DateTime.Today.Date;
                var production = _bllproductionnote.GetProductionsToday(date, companyid).ToList();
                if (production == null)
                {
                    return 0;
                }
                else
                {
                    return production.Count();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public int GetProductionCountThisWeek(int loggeduserid,int companyid)
        {
            try
            {
                DateTime fromdate = DateTime.Today.Date.AddDays(-7);
                DateTime todate = DateTime.Today.Date;
                var production = _bllproductionnote.GetProductionsThisweek(fromdate, todate, companyid).ToList();
                if (production == null)
                {
                    return 0;
                }
                else
                {
                    return production.Count();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public int GetTOGCountThisWeek(int loggeduserid,int companyid)
        {
            try
            {
                DateTime fromdate = DateTime.Today.Date.AddDays(-7);
                DateTime todate = DateTime.Today.Date;
                var togs = _blltransfernote.GetTOGsThisWeek(fromdate,todate,companyid).ToList();
                if (togs == null)
                {
                    return 0;
                }
                else
                {
                    return togs.Count();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public int GetTOGCountToday(int loggeduserid,int companyid)
        {
            try
            {
                DateTime date = DateTime.Today.Date;
                var togs = _blltransfernote.GetTOGsToday(date,companyid).ToList();
                if (togs == null)
                {
                    return 0;
                }
                else
                {
                    return togs.Count();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public List<DashboardViewModel.Top10Productions> GetTop10Productions(long locationid)
        {
            try
            {


                var productions = (from p in _bllproductionnote.GetProductionNoteDetail()
                                   where p.ProductId != 0
                                   group p by p.ProductId into groupedtable

                                   select new
                                   {
                                       ProductId = groupedtable.Key,
                                       Qty = groupedtable.Sum(s => s.ProductQty)

                                   }).ToList();
                var orderedlist = productions.OrderByDescending(p => p.Qty).ToList();
                var tolist = orderedlist.Take(10);

                //.OrderByDescending(k=>k.ProductQty).ToList();

                List<DashboardViewModel.Top10Productions> top10list = new List<DashboardViewModel.Top10Productions>();
                //  ProductService _porductService = new ProductService();


                foreach (var prd in tolist)
                {

                    DashboardViewModel.Top10Productions newprd = new DashboardViewModel.Top10Productions();

                    var ext = _bllproduct.GetProductById(prd.ProductId);
                    if (ext != null)
                    {
                        newprd.ProductName = ext.ProductName;
                        newprd.ProductCount = prd.Qty;
                        top10list.Add(newprd);
                    }
                }

                return top10list;



            }
            catch (Exception ex)
            {

                throw;
            }
        }

        // version 2.0.0

        public List<DashboardViewModel.RevenueVsCost> ExecRevenueAndCostSP(DashboardViewModel.DashboardParms parms)
        {

           
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.RevenueVsCost>("[dbo].[SP_DB_SalesNRevenue] @FromDate, @ToDate,@LocationID,@ChartType,@DeptID,@CustCatID,@CompanyID",
                     new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                     new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
                     new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                     new SqlParameter("@ChartType", SqlDbType.Int) { Value = Convert.ToInt32(parms.TypeId) },
                     new SqlParameter("@DeptID", SqlDbType.Int) { Value = Convert.ToInt32(parms.DepartmentId) },
                     new SqlParameter("@CustCatID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CustomerGroupId) },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CompanyId) }
                    ).ToList();
            return result;
        }

        public List<DashboardViewModel.OrderTypesVariation> ExecNumberOfOrdersSp(DashboardViewModel.DashboardParms parms)
        {


            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.OrderTypesVariation>("[dbo].[SP_DB_NumberOfOrders] @FromDate, @ToDate,@LocationID,@ChartType,@CustCatID,@CompanyID",
                     new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                     new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
                     new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                     new SqlParameter("@ChartType", SqlDbType.Int) { Value = Convert.ToInt32(parms.TypeId) },                    
                     new SqlParameter("@CustCatID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CustomerGroupId) },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CompanyId) }
                    ).ToList();
            return result;
        }


        public List<DashboardViewModel.Products> ExecOrderTypeWiseProductSalesSp(DashboardViewModel.DashboardParms parms)
        {


            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.Products>("[dbo].[SP_DB_OrderTypeWiseProductSales] @FromDate, @ToDate,@LocationID,@ChartType,@CustCatID,@OrderType,@CompanyID",
                     new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                     new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
                     new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                     new SqlParameter("@ChartType", SqlDbType.Int) { Value = Convert.ToInt32(parms.TypeId) },
                     new SqlParameter("@OrderType", SqlDbType.Int) { Value = Convert.ToInt32(parms.OrderTypeId) },
                     new SqlParameter("@CustCatID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CustomerGroupId) },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CompanyId) }
                    ).ToList();
            return result;
        }

        public List<DashboardViewModel.OrderTypeBreakdown> ExecOrderTypeWiseSales(DashboardViewModel.DashboardParms parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.OrderTypeBreakdown>("[dbo].[SP_DB_OrderTypeWiseSales] @FromDate, @ToDate,@LocationID,@DeptID,@CustCatID,@CompanyID",
                     new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                     new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
                     new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                     new SqlParameter("@DeptID", SqlDbType.Int) { Value = Convert.ToInt32(parms.DepartmentId) },
                     new SqlParameter("@CustCatID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CustomerGroupId) },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CompanyId) }
                    ).ToList();
            return result;
        }

        public List<DashboardViewModel.DeptOrderTypeWiseSales> DeptOrderTypeWiseSales(DashboardViewModel.DashboardParms parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.DeptOrderTypeWiseSales>("[dbo].[SP_DB_DeptOrderTypeWiseSales] @FromDate, @ToDate,@LocationID,@DeptID,@CustCatID,@CateModeID,@CompanyID",
                     new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                     new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
                     new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                     new SqlParameter("@DeptID", SqlDbType.Int) { Value = Convert.ToInt32(parms.DepartmentId) },
                     new SqlParameter("@CustCatID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CustomerGroupId) },
                     new SqlParameter("@CateModeID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CateringModeId) },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CompanyId) }
                    ).ToList();
            return result;
        }

        public List<DashboardViewModel.Products> ProductWiseSalesByDept(DashboardViewModel.DashboardParms parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.Products>("[dbo].[SP_DB_ProductWiseSalesByDept] @FromDate, @ToDate,@LocationID,@DeptID,@CustCatID,@CompanyID",
                     new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                     new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
                     new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                     new SqlParameter("@DeptID", SqlDbType.Int) { Value = Convert.ToInt32(parms.DepartmentId) },
                     new SqlParameter("@CustCatID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CustomerGroupId) },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CompanyId) }
                    ).ToList();
            return result;
        }

        public List<DashboardViewModel.DeptOrderTypeWiseSales> DeptWiseSales(DashboardViewModel.DashboardParms parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.DeptOrderTypeWiseSales>("[dbo].[SP_DB_DepartmentWiseSales] @FromDate, @ToDate,@LocationID,@CustCatID,@CompanyID",
                     new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                     new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
                     new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },                 
                     new SqlParameter("@CustCatID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CustomerGroupId) },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CompanyId) }
                    ).ToList();
            return result;
        }


        public List<DashboardViewModel.Products> DeptWiseProductsSale(DashboardViewModel.DashboardParms parms)
        {
            //try
            //{
            //    var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.Products>("[dbo].[SP_DB_MostOrderedProductWiseSales] @FromDate, @ToDate,@LocationID,@CustCatID,@TopCount,@CompanyID",
            //             new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
            //             new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
            //             new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
            //             new SqlParameter("@CustCatID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CustomerGroupId) },
            //             new SqlParameter("@TopCount", SqlDbType.Int) { Value = Convert.ToInt32(parms.RecordCount) },
            //             new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CompanyId) }
            //            ).ToList();
            //    return result;
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}

            try
            {

               




                var result = _unitofwork
                    .RevenueAndCostRepository
                    .SQLQuery<DashboardViewModel.Products>(
                        "[dbo].[SP_DB_MostOrderedProductWiseSales] " +
                        "@FromDate, @ToDate, @LocationID, @CustCatID, @TopCount, @CompanyID",

                        new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate },
                        new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate },
                        new SqlParameter("@LocationID", SqlDbType.Int) { Value = parms.LocationId },
                        new SqlParameter("@CustCatID", SqlDbType.Int) { Value = parms.CustomerGroupId },
                        new SqlParameter("@TopCount", SqlDbType.Int) { Value = parms.RecordCount },
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = parms.CompanyId }
                    )
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }




        }

        public async Task<List<DashboardViewModel.Waistage>> WaistageSummary(DashboardViewModel.DashboardParms parms)
        {
            return await _unitofwork.RevenueAndCostRepository.SQLQueryAsync<DashboardViewModel.Waistage>(
                "[dbo].[SP_DB_WastageSummary] @FromDate, @ToDate,@LocationID,@ChartType,@OrderType,@CompanyID",
                new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
                new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                new SqlParameter("@ChartType", SqlDbType.Int) { Value = Convert.ToInt32(parms.TypeId) },
                new SqlParameter("@OrderType", SqlDbType.Int) { Value = Convert.ToInt32(parms.OrderTypeId) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CompanyId) }
            );
        }


        public async Task<List<DashboardViewModel.Products>> WaistageDetail(DashboardViewModel.DashboardParms parms)
        {
            return await _unitofwork.RevenueAndCostRepository.SQLQueryAsync<DashboardViewModel.Products>(
               "[dbo].[SP_DB_WastageDetail] @FromDate, @ToDate, @LocationID, @ChartType, @OrderType, @CompanyID",
               new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate },
               new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate },
               new SqlParameter("@LocationID", SqlDbType.Int) { Value = parms.LocationId },
               new SqlParameter("@ChartType", SqlDbType.Int) { Value = parms.TypeId },
               new SqlParameter("@OrderType", SqlDbType.Int) { Value = parms.OrderTypeId },
               new SqlParameter("@CompanyID", SqlDbType.Int) { Value = parms.CompanyId }
           );
            //var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.Products>("[dbo].[SP_DB_WastageDetail] @FromDate, @ToDate,@LocationID,@ChartType,@OrderType,@CompanyID",
            //         new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
            //         new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
            //         new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
            //         new SqlParameter("@ChartType", SqlDbType.Int) { Value = Convert.ToInt32(parms.TypeId) },
            //         new SqlParameter("@OrderType", SqlDbType.Int) { Value = Convert.ToInt32(parms.OrderTypeId) },
            //         new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CompanyId) }

            //        ).ToList();
            //return result;
        }

        public List<DashboardViewModel.HourlySales> HourlySales(DashboardViewModel.DashboardParms parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.HourlySales>("[dbo].[SP_DB_HourlySales] @LocationID,@FromDate,@FromTime ,@ToDate ,@ToTime,@CompanyID",
                    new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                    new SqlParameter("@FromTime", SqlDbType.Time) { Value = parms.TimeFrom },
                    new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
                    new SqlParameter("@ToTime", SqlDbType.Time) { Value = parms.TimeTo },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = parms.CompanyId }
                    ).ToList();
            return result;
        }


        public List<DashboardViewModel.OrderTimeConsumption> TimeConsumption(DashboardViewModel.DashboardParms parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.OrderTimeConsumption>("[dbo].[SP_DB_OrderWiseTimeConsumption] @LocationID,@FromDate,@FromTime ,@ToDate ,@ToTime,@CateModeID,@CompanyID",
                    new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                    new SqlParameter("@FromTime", SqlDbType.Time) { Value = parms.TimeFrom },
                    new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
                    new SqlParameter("@ToTime", SqlDbType.Time) { Value = parms.TimeTo },
                    new SqlParameter("@CateModeID", SqlDbType.Int) { Value = parms.CateringModeId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = parms.CompanyId }

                    ).ToList();
            return result;
        }

        public List<DashboardViewModel.FoodCost> FoodCost(DashboardViewModel.DashboardParms parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.FoodCost>("[dbo].[SP_DB_FoodCostEstimate] @FromDate,@ToDate,@CompanyID,@LocationID,@IsDeptWise",                   
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = Convert.ToDateTime(parms.FromDate).ToShortDateString() },                   
                    new SqlParameter("@ToDate", SqlDbType.DateTime) { Value =Convert.ToDateTime(parms.ToDate).ToShortDateString() },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = parms.CompanyId },
                    new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                    new SqlParameter("@IsDeptWise", SqlDbType.Int) { Value = parms.TypeId }
                    ).ToList();
            return result;
        }

        public List<DashboardViewModel.HourlySalesTabular> HourlySalesTabular(DashboardViewModel.DashboardParms parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DashboardViewModel.HourlySalesTabular>("[dbo].[SP_DB_HourlySalesTable] @LocationID,@FromDate,@CompanyID",
                    new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = parms.CompanyId }
                    ).ToList();
            return result;
        }
    }
}
