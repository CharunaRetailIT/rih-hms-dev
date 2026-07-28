using HospitalityManagement.Models;
using HospitalityManagement.Models.Reports;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service.Reports
{
    public class ReportsService
    {
        ApplicationDbContext context = new ApplicationDbContext();




        public IEnumerable<ReportCategory> GetRptCategories()
        {
            try
            {
                IEnumerable<ReportCategory> rptcat = context.ReportCategory.OrderBy(r => r.ReportCategoryCode);
                if (rptcat != null)
                {
                    return rptcat;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ReportCategory GetRptCategoryById(long id)
        {
            try
            {
                ReportCategory rptcat = context.ReportCategory.Where(r => r.ReportCategoryId == id).FirstOrDefault();
                if (rptcat != null)
                {
                    return rptcat;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ReportInfo> GetRptInfoIdByRptCatId(long catid)
        {
            try
            {
                IEnumerable<ReportInfo> rptinfo = context.ReportInfo.Where(r => r.ReportCategoryId == catid)
                                                                                        .OrderBy(k => k.OrderId);


                if (rptinfo != null)
                {
                    return rptinfo;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }



        public IEnumerable<ReportInfo> GetURLByReportId(long rptid)
        {
            List<ReportInfo> reportdata = new List<ReportInfo>();

            try
            {
                IEnumerable<ReportInfo> docs = context.ReportInfo.Where(e => e.ReportInfoId == rptid)
                                                                                        .OrderBy(k => k.ReportURL);


                if (docs != null)
                {
                    return docs;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }



        public List<ReportInfo> GetReportURL(long rptcatid, long rptid)
        {
            try
            {
                List<ReportInfo> rptinfo = new List<ReportInfo>();

                if (rptcatid != 0 && rptid != 0)
                {
                    rptinfo = context.ReportInfo.Where(r => r.ReportInfoId == rptid && r.ReportCategoryId == rptcatid).
                                                              OrderBy(c => c.ReportURL).ToList();
                }
               

                if (rptinfo != null)
                {
                    return rptinfo;
                }

                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<Models.ViewModels.Reports.SalesRegisterViewModel> GetSalesRegistryReport(string exectype,
          DateTime execfromdate, DateTime exectodate, bool execIsAsAtDate, DateTime execfromtime, DateTime exectotime,
          int execlocf, int execloct, int exedept, int execcat, int execsubcat, int execustomer)
        {

            var type = new SqlParameter("@Type", exectype);
            var fromDate = new SqlParameter("@FromDate", execfromdate);
            var toDate = new SqlParameter("@ToDate", exectodate);
            var isatdate = new SqlParameter("@IsAsAtDate", execIsAsAtDate);
            var fromTime = new SqlParameter("@FromTime", execfromtime);
            var toTime = new SqlParameter("@ToTime", exectotime);
            var locationF = new SqlParameter("@LocationF", execlocf);
            var locationT = new SqlParameter("@LocationT", execloct);
            var department = new SqlParameter("@Department", exedept);
            var category = new SqlParameter("@Category", execcat);
            var subCategory = new SqlParameter("@SubCategory", execsubcat);
            var customer = new SqlParameter("@Customer", execustomer);
            //var Receipt = new SqlParameter("@Receipt", execreceipt);


            return context.Database.SqlQuery<Models.ViewModels.Reports.SalesRegisterViewModel>("[dbo].[sp_rpt_SalesRegistry] @Type, @FromDate, @ToDate, @IsAsAtDate, @FromTime,@ToTime,  @LocationF, @LocationT, @Department, @SubCategory, @Customer", type, fromDate, toDate, isatdate, fromTime, toTime, locationF, locationT, department, category, subCategory, customer).ToList();

        }


        public List<Models.ViewModels.Reports.DailySalesViewMdel.SalesData> GetDailySalesReport(DateTime date,int location)
        {

            var locationid = new SqlParameter("@LocationId", location);
            var reportdate = new SqlParameter("@Date", date);
            return context.Database.SqlQuery<Models.ViewModels.Reports.DailySalesViewMdel.SalesData>("[dbo].[SP_DailySales] @Date, @LocationId", reportdate, locationid).ToList();

        }

        //public List<Models.ViewModels.Reports.SalesRegisterViewModel> GetSalesRegistryReportAsAtDate(string exectype,
        //   DateTime execfromdate, DateTime exectodate, bool execIsAsAtDate, DateTime execfromtime, DateTime exectotime,
        //   int execlocf, int execloct, int exedept, int execcat, int execsubcat, int execustomer)
        //{

        //    var type = new SqlParameter("@Type", exectype);
        //    var fromDate = new SqlParameter("@FromDate", execfromdate);
        //    var toDate = new SqlParameter("@ToDate", exectodate);
        //    var isatdate = new SqlParameter("@IsAsAtDate", execIsAsAtDate);
        //    var fromTime = new SqlParameter("@FromTime", execfromtime);
        //    var toTime = new SqlParameter("@ToTime", exectotime);
        //    var locationF = new SqlParameter("@LocationF", execlocf);
        //    var locationT = new SqlParameter("@LocationT", execloct);
        //    var department = new SqlParameter("@Department", exedept);
        //    var category = new SqlParameter("@Category", execcat);
        //    var subCategory = new SqlParameter("@SubCategory", execsubcat);
        //    var customer = new SqlParameter("@Customer", execustomer);
        //    //var Receipt = new SqlParameter("@Receipt", execreceipt);


        //    return context.Database.SqlQuery<Models.ViewModels.Reports.SalesRegisterViewModel>("[dbo].[sp_rpt_SalesRegistry] @Type, @FromDate, @ToDate, @IsAsAtDate, @FromTime,@ToTime,  @LocationF, @LocationT, @Department, @SubCategory, @Customer", type, fromDate, toDate, isatdate, fromTime, toTime, locationF, locationT, department, category, subCategory, customer).ToList();

        //}


        //public List<Models.ViewModels.Reports.SalesRegisterViewModel> GetSalesRegistryReport(string exectype,
        //    DateTime execfromdate, DateTime exectodate, DateTime execfromtime, DateTime exectotime,
        //    int execlocf, int execloct, int exedept, int execcat, int execsubcat, int execustomer)
        //{

        //    var type = new SqlParameter("@Type", exectype);
        //    var fromDate = new SqlParameter("@FromDate", execfromdate);
        //    var toDate = new SqlParameter("@ToDate", exectodate);
        //    var fromTime = new SqlParameter("@FromTime", execfromtime);
        //    var toTime = new SqlParameter("@ToTime", exectotime);
        //    var locationF = new SqlParameter("@LocationF", execlocf);
        //    var locationT = new SqlParameter("@LocationT", execloct);
        //    var department = new SqlParameter("@Department", exedept);
        //    var category = new SqlParameter("@Category", execcat);
        //    var subCategory = new SqlParameter("@SubCategory", execsubcat);
        //    var customer = new SqlParameter("@Customer", execustomer);
        //    //var Receipt = new SqlParameter("@Receipt", execreceipt);


        //    return context.Database.SqlQuery<Models.ViewModels.Reports.SalesRegisterViewModel>("[dbo].[sp_rpt_SalesRegistry] @Type, @FromDate, @ToDate,@FromTime,@ToTime,  @LocationF, @LocationT, @Department, @SubCategory, @Customer", type, fromDate, toDate, fromTime, toTime, locationF, locationT, department, category, subCategory, customer).ToList();

        //}

    }
}