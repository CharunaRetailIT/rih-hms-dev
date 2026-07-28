using HospitalityManagement.Models;
using HospitalityManagement.Models.Transactions;
using HospitalityManagement.Models.ViewModels;
using HospitalityManagement.Models.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service.Transactions
{
    public class StockAdjustmentService
    {
        ApplicationDbContext context = new ApplicationDbContext();
        private readonly LocationService _locservice = new LocationService();
        public IEnumerable<StockAdjustmentType> GetTypes()
        {
            try
            {
                IEnumerable<StockAdjustmentType> types = context.StockAdjustmentType.OrderBy(e => e.IsActive==true);
                if (types != null)
                {
                    return types;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public IEnumerable<StockAdjustmentHeader> GetDocNoByLocId(long locid)
        {
            try
            {
                IEnumerable<StockAdjustmentHeader> docs = context.StockAdjustmentHeader.Where(e => e.StockLocationId == locid)
                                                                                        .OrderBy(k=>k.DocumentNo);


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

        public List<StockAdjustmentViewModel> GetProductDetails(long productid,long locid,decimal adjStock,string type)
        {
            try
            {
                var det = context.ProductStockMaster.Where(e => e.ProductId == productid && e.LocationId==locid).ToList();
                List<StockAdjustmentViewModel> vvm = new List<StockAdjustmentViewModel>(); 
                foreach (var item in det)
                {
                    StockAdjustmentViewModel s = new StockAdjustmentViewModel();
                    s.ProductId = item.ProductId;
                    s.ProductName = item.ProductName;
                    s.CurrentStock = item.Stock;
                    s.AdjustStock = adjStock;
                    s.SellingPrice = item.SellingPrice;
                    s.CostPrice = item.CostPrice;
                    s.AvgCost = item.AvgCost;
                    if (type == "Add")
                    {
                        s.NewStock = item.Stock + adjStock;
                    }
                    else if (type == "Reduce")
                    {
                        s.NewStock = item.Stock-adjStock;

                    } else if (type == "Override")
                    {
                        s.NewStock = adjStock;
                    }

                    vvm.Add(s);

                }


                if (det != null)
                {
                    return vvm;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public bool SubmitStockAdjustment(StockAdjustmentHeader header)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {

                    context.StockAdjustmentHeader.Add(header);

                    if (context.SaveChanges() == 1)
                    {

                        foreach (var detail in header.StockAdjDetail)
                        {

                            detail.StockAdjustmentHeaderId = header.StockAdjustmentHeaderId;

                            var productstock = context.ProductStockMaster.Where(s => s.ProductId == detail.ProductId &&
                                                                            s.LocationId == header.StockLocationId).First();
                            
                            detail.AvgCost = productstock.AvgCost;
                            context.StockAdjustmentDetail.Add(detail);

                          

                            productstock.Stock = detail.NewStock;

                        }

                        context.SaveChanges();                       
                        dbtransaction.Commit();
                        return true;

                    }
                    else
                    {
                        dbtransaction.Rollback();
                        return false;
                    }

                    
                }
                catch (Exception ex)
                {
                    dbtransaction.Rollback();
                    return false;

                }
            }

        }

        public List<StockAdjustmentReportViewModel> StockAdjustmentReport(long locid,long docid,DateTime frmdate,DateTime todate)
        {
            List<StockAdjustmentReportViewModel> reportdata = new List<StockAdjustmentReportViewModel>();
            List<StockAdjustmentHeader> dbheader = new List<StockAdjustmentHeader>();

            if (locid == 0 || docid == 0)
            {
                dbheader = context.StockAdjustmentHeader.Where(s => s.CreatedDate >= frmdate.Date && s.CreatedDate <= todate.Date
                                                               ).ToList();
            }
            else if(locid!=0 && docid!=0)
            {
                dbheader = context.StockAdjustmentHeader.Where(s => s.StockLocationId == locid && s.StockAdjustmentHeaderId == docid
                                                               ).ToList();
            }
            else if (locid != 0 && docid == 0)
            {
                dbheader = context.StockAdjustmentHeader.Where(s => s.StockLocationId == locid &&
                                                                s.CreatedDate >= frmdate.Date && s.CreatedDate <= todate.Date
                                                               ).ToList();
            }

            foreach (var header in dbheader)
            {
                StockAdjustmentReportViewModel vm = new StockAdjustmentReportViewModel();
                vm.Location = _locservice.GetLocationById(header.StockLocationId).LocationName;
                vm.DocumentDate = header.CreatedDate.ToShortDateString();
                vm.DocumentNo = header.DocumentNo;
                vm.Remark = header.Remark;

                foreach(var s in context.StockAdjustmentDetail.Where(r => r.StockAdjustmentHeaderId == header.StockAdjustmentHeaderId))
                {
                    StockAdjustmentReportViewModel.ReportDetail det = new StockAdjustmentReportViewModel.ReportDetail();
                    det.ProductId = s.ProductId;
                    det.ProductName = s.ProductName;
                    det.CurrentStock = s.CurrentStock;
                    det.AdjustStock = s.AdjustStock;
                    det.NewStock = s.NewStock;
                    det.AvgCost = s.AvgCost;
                    det.BaseType = s.BaseType;
                    det.Reason = s.Reason;
                    vm.Detail.Add(det);
                }

                reportdata.Add(vm);
            }



            return reportdata;
        }


    }
}