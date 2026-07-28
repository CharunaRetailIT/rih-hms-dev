using System;
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
    public class TransferNoteService
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
        public IEnumerable<TransferNoteHeader> GetAllTOGs()
        {
            try
            {
                IEnumerable<TransferNoteHeader> togs = context.TransferNoteHeader.Where(r => r.IsTempTOG == true).OrderBy(g => g.DocumentDate);
                if (togs != null)
                {
                    return togs;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<TransferNoteDetail> GetAllTOGsById(long id)
        {
            try
            {
                IEnumerable<TransferNoteDetail> togs = context.TransferNoteDetail.OrderBy(g => g.TransferNoteHeaderId == id);
                if (togs != null)
                {
                    return togs;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public TransferNoteHeader GetTOGById(long id)
        {
            try
            {
                var tog = context.TransferNoteHeader.FirstOrDefault(g => g.TransferNoteHeaderId == id);
                return tog ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<TransferNoteDetail> GetTOGDetById(long id)
        {
            try
            {
                IEnumerable<TransferNoteDetail> togdet = context.TransferNoteDetail.Where(p => p.TransferNoteHeaderId == id).OrderBy(g => g.LineNo);
                return togdet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public ProductStockMaster GetTOGProductById(long id,long locid)
        {
            try
            {
                ProductStockMaster togdet = context.ProductStockMaster.Where(p => p.ProductId == id && p.LocationId == locid).OrderBy(g => g.ProductId).First();


                return togdet ?? null;
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

        public bool CheckProductExistsAtLoc(long id, long fromloc,long toloc)
        {
            try
            {
              return  context.ProductStockMaster.Any(l => l.ProductId == id && 
                                                     (l.LocationId==fromloc || l.LocationId==toloc)
                                                     );

            }
            catch (Exception)
            {
                
                throw;
             
            }
        }


        public bool SaveTransferNote(TransferNoteHeader togheader)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {
                    togheader.CostCentreId = togheader.FromLocationId;
                    context.TransferNoteHeader.Add(togheader);

                    if (context.SaveChanges() == 1)
                    {

                        int idx = 1;
                        foreach (var detail in togheader.TOGDetail)
                        {
                            var formlocstock = context.ProductStockMaster.Where(from => from.ProductId == detail.ProductId &&
                                                    from.LocationId == togheader.FromLocationId).First();

                            detail.TransferNoteHeaderId = togheader.TransferNoteHeaderId;
                            detail.StockCode = formlocstock.StockCode;

                            var grndetail = context.PurchaseDetail.Where(d=>d.PurchaseHeaderID==togheader.GRNId 
                                                                        && d.ProductID==detail.ProductId).FirstOrDefault();

                            grndetail.TOGQty += detail.OrderQty;


                            // only for temporary
                            detail.BatchExpiryDate = DateTime.Now;
                            detail.BatchNo = "";
                            detail.IsBatch = false;
                            detail.PackId = 1;
                            detail.SerialNo = "1";
                            detail.UnitOfMeasureId = 1;
                            //

                            detail.LineNo = togheader.TOGDetail.IndexOf(detail)+1;
                            idx += 1;
                            context.TransferNoteDetail.Add(detail);

                            if (context.SaveChanges() == 1)
                            {
                                
                                    if (formlocstock.ProductId == detail.ProductId && formlocstock.LocationId == togheader.FromLocationId)
                                    {

                                        formlocstock.Stock -= detail.OrderQty;
                                        formlocstock.DocumentNo = togheader.DocumentNo;
                                        formlocstock.LastUpdatedDate = DateTime.Now;
                                       

                                        var tolocstock = context.ProductStockMaster.Where(to=>to.ProductId==detail.ProductId && 
                                                                                           to.LocationId==togheader.ToLocationId).First();
                                        tolocstock.Stock += detail.OrderQty;
                                        tolocstock.DocumentNo = togheader.DocumentNo;
                                        tolocstock.LastUpdatedDate = DateTime.Now;
                                        context.SaveChanges();
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
                catch (Exception ex)
                {
                    dbtransaction.Rollback();
                    return false;

                }
            }
        }

        //public int UpdateTOG(TransferNoteHeader tog)
        //{
        //    try
        //    {

        //        int res = context.SaveChanges();
        //        return res;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}


        public bool CheckIfExist(long fromloc,long toloc,long productid)
        {
            bool exists = true;
            var frm = context.ProductStockMaster.Any(ps => ps.LocationId == fromloc && ps.ProductId == productid);
            var to = context.ProductStockMaster.Any(ps => ps.LocationId == toloc && ps.ProductId == productid);
            if (!frm || !to)
            {
                exists = false;
            }
            return exists;
        }

        public decimal CheckFromLocStock(long fromloc,long productid)
        {
            if (context.ProductStockMaster.Any(p => p.ProductId == productid && p.LocationId == fromloc))
            {
                return context.ProductStockMaster.Where(ps => ps.LocationId == fromloc && ps.ProductId == productid).First().Stock;
            }
            else
            {
                return 0;
            }
           
        }

        public int DeleteItemsById(long id)
        {
            try
            {
                context.TransferNoteDetail.RemoveRange(context.TransferNoteDetail.Where(x => x.TransferNoteHeaderId == id));
                var res = context.SaveChanges();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }




        public bool UpdateTOG(TransferNoteHeader tog)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {
                try
                {
                    if (context.SaveChanges() == 1)
                    {

                        //List<TransferNoteDetail> td = new List<TransferNoteDetail>();
                        //td = tog.TOGDetail;

                        DeleteItemsById(tog.TransferNoteHeaderId);

                      //  tog.TOGDetail= td;
                        //update 
                        if (tog.TOGDetail.Count > 0)
                        {
                           
                            int idx = 1;
                            foreach (var togdetail in tog.TOGDetail)
                            {
                                togdetail.TransferNoteHeaderId = tog.TransferNoteHeaderId;
                                var formlocstock = context.ProductStockMaster.Where(from => from.ProductId == togdetail.ProductId &&
                                                    from.LocationId == tog.FromLocationId).First();

                                togdetail.TransferNoteHeaderId = tog.TransferNoteHeaderId;
                                togdetail.StockCode = formlocstock.StockCode;

                                // only for temporary
                                togdetail.BatchExpiryDate = DateTime.Now;
                                togdetail.BatchNo = "";
                                togdetail.IsBatch = false;
                                togdetail.PackId = 1;
                                togdetail.SerialNo = "1";
                                togdetail.UnitOfMeasureId = 1;
                                //

                                togdetail.LineNo = idx;
                                idx += 1;
                                context.TransferNoteDetail.Add(togdetail);

                                if (context.SaveChanges() == 1)
                                {

                                    if (formlocstock.ProductId == togdetail.ProductId && formlocstock.LocationId == tog.FromLocationId)
                                    {

                                        formlocstock.Stock -= togdetail.OrderQty;
                                        formlocstock.DocumentNo = tog.DocumentNo;
                                        formlocstock.LastUpdatedDate = DateTime.Now;
                                        //context.SaveChanges();

                                        var tolocstock = context.ProductStockMaster.Where(to => to.ProductId == togdetail.ProductId &&
                                                                                           to.LocationId == tog.ToLocationId).First();
                                        tolocstock.Stock += togdetail.OrderQty;
                                        tolocstock.DocumentNo = tog.DocumentNo;
                                        tolocstock.LastUpdatedDate = DateTime.Now;
                                        context.SaveChanges();
                                    }

                                }

                            }

                            //if (context.SaveChanges() != tog.TOGDetail.Count)
                            //{
                            //    dbtransaction.Rollback();
                            //    return false;
                            //}


                        }
                     
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
                    throw;
                }

            }

        }



        public List<TransferNoteHeader> GetTOGSummaryReport(long locid, long docid, DateTime from, DateTime to)
        {
            try
            {
                List<TransferNoteHeader> transfernoteheader = new List<TransferNoteHeader>();

                if (locid != 0 && docid != 0)
                {
                    transfernoteheader = context.TransferNoteHeader.Where(r => r.TransferNoteHeaderId == docid && r.LocationId == locid).
                                                              OrderBy(c => c.DocumentNo).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid != 0 && docid == 0)
                {
                    transfernoteheader = context.TransferNoteHeader.Where(r => r.LocationId == locid).
                                                               OrderBy(c => c.DocumentNo).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid == 0 && docid != 0)
                {
                    transfernoteheader = context.TransferNoteHeader.Where(r => r.TransferNoteHeaderId == docid).
                                                              OrderBy(c => c.DocumentNo).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid == 0 && docid == 0)
                {
                    DateTime frmdate = from.Date;
                    DateTime todate = to.Date;

                    transfernoteheader = context.TransferNoteHeader.Where(r => r.DocumentDate >= frmdate && r.DocumentDate <= todate
                        ).
                        OrderBy(c => c.DocumentNo).OrderBy(d => d.LocationId).ToList();

                }

                if (transfernoteheader != null)
                {
                    return transfernoteheader;
                }

                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<TransferNoteHeader> GetDocNoByLocId(long locid)
        {
            try
            {
                IEnumerable<TransferNoteHeader> docs = context.TransferNoteHeader.Where(e => e.LocationId == locid)
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



        public List<TOGDetailViewModel> GetTOGDetailReport(long locid, long docid, DateTime frmdate, DateTime todate)
        {
            List<TOGDetailViewModel> reportdata = new List<TOGDetailViewModel>();
            List<TransferNoteHeader> dbheader = new List<TransferNoteHeader>();

            if (locid == 0 || docid == 0)
            {
                dbheader = context.TransferNoteHeader.Where(s => s.CreatedDate >= frmdate.Date && s.CreatedDate <= todate.Date
                                                               ).ToList();
            }
            else if (locid != 0 && docid != 0)
            {
                dbheader = context.TransferNoteHeader.Where(s => s.LocationId == locid && s.TransferNoteHeaderId == docid
                                                               ).ToList();
            }
            else if (locid != 0 && docid == 0)
            {
                dbheader = context.TransferNoteHeader.Where(s => s.LocationId == locid &&
                                                                s.CreatedDate >= frmdate.Date && s.CreatedDate <= todate.Date
                                                               ).ToList();
            }

            foreach (var header in dbheader)
            {
                TOGDetailViewModel vm = new TOGDetailViewModel();
                vm.Location = _locservice.GetLocationById(header.LocationId).LocationName;
                vm.ToLocation = _locservice.GetLocationById(header.ToLocationId).LocationName;
                vm.DocumentDate = header.CreatedDate.ToShortDateString();
                vm.DocumentNo = header.DocumentNo;
                vm.Remark = header.Remark;

                foreach (var s in context.TransferNoteDetail.Where(r => r.TransferNoteHeaderId == header.TransferNoteHeaderId))
                {
                    TOGDetailViewModel.ReportDetail det = new TOGDetailViewModel.ReportDetail();
                    det.ProductId = s.ProductId;
                    det.ProductName = _productservice.GetProductById(det.ProductId).ProductName;
                    det.OrderQty = s.OrderQty;
                    det.CostPrice = s.CostPrice;
                    det.SellingPrice = s.SellingPrice;
                    vm.Detail.Add(det);

                }

                reportdata.Add(vm);
            }

            return reportdata;
        }


        public int CheckToLocation(long productid,long tolocid)
        {
            if (context.ProductStockMaster.Any(p => p.ProductId == productid && p.LocationId == tolocid))
            {
                return 1;
            }
            else
            {
                return 0;
            }

           
        }

    }
}