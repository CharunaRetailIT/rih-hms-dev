using System;
//using System.Activities.Expressions;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using HospitalityManagement.Models;
using HospitalityManagement.Models.Transactions;
using HospitalityManagement.Models.ViewModels;
using HospitalityManagement.Models.ViewModels.Reports;

namespace HospitalityManagement.Service.Transactions
{
    public class PurchaseReturnService
    {


        ApplicationDbContext context = new ApplicationDbContext();
        private readonly LocationService _locservice = new LocationService();
        private readonly ProductService _productservice = new ProductService();
        public IEnumerable<PaymentMethod> GetActivePaymentMethods()
        {
            try
            {
                IEnumerable<PaymentMethod> paymentmethods = context.PaymentMethod.OrderBy(g => g.PaymentMethodName);
                if (paymentmethods != null)
                {
                    return paymentmethods;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<PurchaseHeader> GetAllPRNs()
        {
            try
            {
                IEnumerable<PurchaseHeader> prns = context.PurchaseHeader.Where(r => r.IsTempPRN == true).OrderBy(g => g.DocumentDate);
                if (prns != null)
                {
                    return prns;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseHeader> GetAllActivePRNs()
        {
            try
            {
                IEnumerable<PurchaseHeader> prn = context.PurchaseHeader.Where(g => g.IsGRN == false && g.IsTempPRN == false).OrderBy(g => g.DocumentDate);

                if (prn != null)
                {
                    return prn;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }




        public IEnumerable<PurchaseHeader> GetDocNoByLocId(long locid)
        {
            try
            {
                IEnumerable<PurchaseHeader> docs = context.PurchaseHeader.Where(e => e.GRNLocationId == locid && e.IsGRN == false && e.IsTempPRN == false)
                                                                                        .OrderBy(k => k.DocumentNo);


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


        public IEnumerable<PaymentMethod> GetPaymentMethods()
        {
            try
            {
                IEnumerable<PaymentMethod> pm = context.PaymentMethod.OrderBy(g => g.PaymentMethodName);
                if (pm != null)
                {
                    return pm;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<PaymentTerm> GetPaymentterms()
        {
            try
            {
                IEnumerable<PaymentTerm> pt = context.PaymentTerm.OrderBy(g => g.PaymentTermName);
                if (pt != null)
                {
                    return pt;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public PurchaseHeader GetPRNById(long id)
        {
            try
            {
                var prn = context.PurchaseHeader.FirstOrDefault(p => p.PurchaseHeaderId == id);
                return prn ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseDetail> GetPRNDetById(long id)
        {

            //var sss = (from pd in context.PurchaseDetail join
            //           p in context.Product on pd.ProductID equals p.ProductId
            //           where pd.PurchaseHeaderID==id
            //           orderby pd.LineNo
            //           select new
            //           {
            //               ProductId = pd.ProductID,
            //               ProductName = p.ProductName,
            //               qty = ps.CostPrice,
            //               Selling = ps.SellingPrice,
            //               Discounts = ps.DiscountPrc,
            //               Stock = ps.Stock
            //           }
            //           ).ToList();
            // List<PurchaseDetail> pd = new List<PurchaseDetail>();




            // return sss.ToList<PurchaseDetail>();

            try
            {
                IEnumerable<PurchaseDetail> prndet = context.PurchaseDetail.Where(p => p.PurchaseHeaderID == id).OrderBy(g => g.LineNo);
                return prndet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseDetail> GetPRNDetReportById(long id, DateTime from, DateTime to)
        {
            try
            {
                DateTime frmdate = from.Date;
                DateTime todate = to.Date;

                IEnumerable<PurchaseDetail> prndet = context.PurchaseDetail.Where(p => p.PurchaseHeaderID == id
                    && p.DocumentDate >= frmdate && p.DocumentDate <= to

                    ).OrderBy(g => g.LineNo);
                return prndet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        } 

        public bool CheckProductTaxes(long id)
        {
            bool IsTaxes = context.ProductTax.Any(pt => pt.ProductId == id);

            return IsTaxes;

        }


       
        public bool SavePRN(PurchaseHeader prnheader)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {
                    prnheader.CostCentreID = Convert.ToInt32(prnheader.GRNLocationId);
                    context.PurchaseHeader.Add(prnheader);

                    if (context.SaveChanges() == 1)
                    {

                        int idx = 1;
                        foreach (var detail in prnheader.GRNDetail)
                        {
                            var productstockmaster = context.ProductStockMaster.Where(s => s.ProductId == detail.ProductID && s.LocationId == 9);


                            detail.PurchaseHeaderID = prnheader.PurchaseHeaderId;
                            detail.StockCode = productstockmaster.First().StockCode;

                            detail.LineNo = idx;
                            idx += 1;
                            detail.BatchNo = "0";
                            detail.ExpiryDate = DateTime.Now;
                            detail.SerialNo = "0";
                            detail.DocumentID = prnheader.DocumentID;
                            detail.DocumentNo = prnheader.DocumentNo;
                            detail.ProductRemark = "";
                            detail.DocumentDate = DateTime.Now;
                            detail.CostCentreID = Convert.ToInt32(prnheader.CostCentreID);


                            context.PurchaseDetail.Add(detail);

                            if (context.SaveChanges() == 1)
                            {
                                    foreach (var ps in productstockmaster)
                                    {
                                        if (ps.ProductId == detail.ProductID)
                                        {

                                            if (ps.Stock > 0)
                                            {

                                                ps.Stock -= detail.FreeQty + detail.OrderQty;
                                                ps.DocumentNo = detail.DocumentNo;
                                                ps.LastUpdatedDate = DateTime.Now;


                                                context.SaveChanges();
                                            
                                            
                                            }

                                        }

                                    }
                            }


                        }

                        dbtransaction.Commit();
                    }
                    else
                    {
                        dbtransaction.Rollback();
                        return false;
                    }

                    return true;
                }
                catch (Exception)
                {
                    dbtransaction.Rollback();
                    return false;

                }
            }
        }

        public long DeleteGRNDetail(long grnheaderid)
        {
            var res = 0;
            try
            {
                context.PurchaseDetail.RemoveRange(context.PurchaseDetail.Where(x => x.PurchaseHeaderID == grnheaderid));
                res = context.SaveChanges();


            }
            catch (Exception)
            {

                throw;
            }
            return res;
        }

        public bool UpdatePRN(PurchaseHeader prnheader)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {
                    var prnInDb = context.PurchaseHeader.Single(a => a.PurchaseHeaderId == prnheader.PurchaseHeaderId);
                    prnheader.CreatedUser = prnInDb.CreatedUser;
                    prnheader.CreatedDate = prnInDb.CreatedDate;
                    prnheader.DocumentID = prnInDb.DocumentID;
                    prnheader.CurrencyID = prnInDb.CurrencyID;
                    // Update the properties
                    context.Entry(prnInDb).CurrentValues.SetValues(prnheader);
                    if (context.SaveChanges() == 1)
                    {

                        DeleteGRNDetail(prnheader.PurchaseHeaderId);
                        int idx = 1;
                        foreach (var detail in prnheader.GRNDetail)
                        {
                            var productstockmaster = context.ProductStockMaster.Where(s => s.ProductId == detail.ProductID && s.LocationId == 9);


                            detail.PurchaseHeaderID = prnheader.PurchaseHeaderId;
                            detail.StockCode = productstockmaster.First().StockCode;

                            detail.LineNo = idx;
                            idx += 1;
                            detail.BatchNo = "0";
                            detail.ExpiryDate = DateTime.Now;
                            detail.SerialNo = "0";
                            detail.DocumentID = prnheader.DocumentID;
                            detail.DocumentNo = prnheader.DocumentNo;
                            detail.ProductRemark = "";
                            detail.DocumentDate = DateTime.Now;
                            detail.CostCentreID = prnheader.LocationId;

                            context.PurchaseDetail.Add(detail);

                            if (context.SaveChanges() == 1)
                            {

                                foreach (var ps in productstockmaster)
                                {
                                    if (ps.ProductId == detail.ProductID)
                                    {

                                        if (ps.Stock > 0)
                                        {
                                            ps.Stock -= detail.FreeQty + detail.OrderQty;
                                            ps.DocumentNo = detail.DocumentNo;
                                            ps.LastUpdatedDate = DateTime.Now;


                                            context.SaveChanges();
                                        }

                                       

                                    }

                                }

                            }

                        }

                    }



                    dbtransaction.Commit();
                    return true;


                }
                catch (Exception ex)
                {

                    dbtransaction.Rollback();
                    return false;
                }
            }
        }


        public List<PurchaseHeader> GetPRNSummaryReport(long locid, long docid, DateTime from, DateTime to)
        {
            try
            {
                List<PurchaseHeader> purchaseheader = new List<PurchaseHeader>();

                if (locid != 0 && docid != 0)
                {
                    purchaseheader = context.PurchaseHeader.Where(r => r.PurchaseHeaderId == docid && r.GRNLocationId == locid && r.IsGRN == false && r.IsTempPRN == false).
                                                              OrderBy(c => c.DocumentNo).OrderBy(d => d.GRNLocationId).ToList();
                }
                else if (locid != 0 && docid == 0)
                {
                    purchaseheader = context.PurchaseHeader.Where(r => r.GRNLocationId == locid && r.IsGRN == false && r.IsTempPRN == false).
                                                               OrderBy(c => c.DocumentNo).OrderBy(d => d.GRNLocationId).ToList();
                }
                else if (locid == 0 && docid != 0)
                {
                    purchaseheader = context.PurchaseHeader.Where(r => r.PurchaseHeaderId == docid && r.IsGRN == false && r.IsTempPRN == false).
                                                              OrderBy(c => c.DocumentNo).OrderBy(d => d.GRNLocationId).ToList();
                }
                else if (locid == 0 && docid == 0)
                {

                    DateTime frmdate = from.Date;
                    DateTime todate = to.Date;

                    purchaseheader = context.PurchaseHeader.Where(r => r.IsGRN == false && r.IsTempPRN == false
                        && r.DocumentDate >= frmdate
                        && r.DocumentDate <= todate
                        ).
                        OrderBy(c => c.DocumentNo).OrderBy(d => d.GRNLocationId).ToList();


                }

                if (purchaseheader != null)
                {
                    return purchaseheader;
                }

                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }



      



        public List<PRNDetailViewModel> GetPRNDetailReport(long locid, long docid, DateTime frmdate, DateTime todate)
        {
            List<PRNDetailViewModel> reportdata = new List<PRNDetailViewModel>();
            List<PurchaseHeader> dbheader = new List<PurchaseHeader>();

            if (locid == 0 || docid == 0)
            {
                dbheader = context.PurchaseHeader.Where(s => s.CreatedDate >= frmdate.Date && s.CreatedDate <= todate.Date && s.IsGRN == false && s.IsTempPRN == false
                                                               ).ToList();
            }
            else if (locid != 0 && docid != 0)
            {
                dbheader = context.PurchaseHeader.Where(s => s.GRNLocationId == locid && s.PurchaseHeaderId == docid && s.IsGRN == false && s.IsTempPRN == false
                                                               ).ToList();
            }
            else if (locid != 0 && docid == 0)
            {
                dbheader = context.PurchaseHeader.Where(s => s.GRNLocationId == locid &&
                                                                s.CreatedDate >= frmdate.Date && s.CreatedDate <= todate.Date && s.IsGRN == false && s.IsTempPRN == false
                                                               ).ToList();
            }

            foreach (var header in dbheader)
            {
                PRNDetailViewModel vm = new PRNDetailViewModel();
                vm.Location = _locservice.GetLocationById(header.GRNLocationId).LocationName;
                vm.DocumentDate = header.CreatedDate.ToShortDateString();
                vm.DocumentNo = header.DocumentNo;
                vm.Remark = header.Remark;

                foreach (var s in context.PurchaseDetail.Where(r => r.PurchaseHeaderID == header.PurchaseHeaderId))
                {
                    PRNDetailViewModel.ReportDetail det = new PRNDetailViewModel.ReportDetail();
                    det.ProductId = s.ProductID;
                    det.ProductName = _productservice.GetProductById(det.ProductId).ProductName; 
                    det.OrderQty = s.OrderQty;
                    det.FreeQty = s.FreeQty;
                    det.CostPrice = s.CostPrice;
                    det.SellingPrice = s.SellingPrice;
                    vm.Detail.Add(det);

                }

                reportdata.Add(vm);
            }

            return reportdata;
        }









    }
}