using RIT.HMS.BLL.MasterData;
using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.TransactionData
{
    public class BLL_PurchaseReturn
    {

        private readonly UnitOfWork _unitofwork;
        private readonly BLL_Location _blllocation;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_PurchaseOrder _bllpurchaseorder;
        private readonly BLL_DocStatus _blldocstatus;

        public BLL_PurchaseReturn()
        {
            _unitofwork = new UnitOfWork();
            _blllocation = new BLL_Location();
            _bllproduct = new BLL_Product();
            _bllpurchaseorder = new BLL_PurchaseOrder();
            _blldocstatus = new BLL_DocStatus();
        }
        public BLL_PurchaseReturn(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
            _blllocation = new BLL_Location(connection);
            _bllproduct = new BLL_Product(connection);
            _bllpurchaseorder = new BLL_PurchaseOrder(connection);
            _blldocstatus = new BLL_DocStatus(connection);
        }
        public IEnumerable<PaymentMethod> GetActivePaymentMethods(int companyid)
        {
            try
            {
                IEnumerable<PaymentMethod> paymentmethods = _unitofwork.PaymentMethodRepository.Get(p=>p.CompanyID== companyid).OrderBy(g => g.PaymentMethodName);
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
        public IEnumerable<PurchaseHeader> GetAllPRNs(int companyid)
        {
            try
            {
                //IEnumerable<PurchaseHeader> prns = _unitofwork.PurchaseHeaderRepository.Get(r => (r.DocumentStatus !=3 && r.DocumentStatus!=5) && r.DocumentID==6).OrderBy(g => g.DocumentDate);
                IEnumerable<PurchaseHeader> prns = _unitofwork.PurchaseHeaderRepository.Get(r => r.DocumentID == 6 && r.CompanyID==companyid).OrderBy(g => g.DocumentDate);
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


        public IEnumerable<PurchaseHeader> FilterAllPRNs(int companyid)
        {
            try
            {
                IEnumerable<PurchaseHeader> prns = _unitofwork.PurchaseHeaderRepository.Get(p=>p.DocumentID==6 && p.CompanyID== companyid).OrderBy(g => g.DocumentDate);
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

        public IEnumerable<PurchaseHeader> GetAllActivePRNs(int companyid)
        {
            try
            {
                IEnumerable<PurchaseHeader> prn = _unitofwork.PurchaseHeaderRepository.Get(g => g.IsGRN == false && g.IsTempPRN == false && g.CompanyID== companyid).OrderBy(g => g.DocumentDate);

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

        public IEnumerable<PurchaseHeader> GetDocNoByLocId(long locid,int companyid)
        {
            try
            {
                IEnumerable<PurchaseHeader> docs = _unitofwork.PurchaseHeaderRepository.Get(e => e.GRNLocationId == locid && e.CompanyID== companyid)
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

        public IEnumerable<PaymentMethod> GetPaymentMethods(int companyid)
        {
            try
            {
                IEnumerable<PaymentMethod> pm = _unitofwork.PaymentMethodRepository.Get(p=>p.CompanyID== companyid).OrderBy(g => g.PaymentMethodName);
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

        public IEnumerable<PaymentTerm> GetPaymentterms(int companyid)
        {
            try
            {
                IEnumerable<PaymentTerm> pt = _unitofwork.PaymentTermRepository.Get(c => c.IsDelete == false && c.CompanyID== companyid).OrderBy(g => g.PaymentTermCode);
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
                var prn = _unitofwork.PurchaseHeaderRepository.Get().FirstOrDefault(p => p.PurchaseHeaderId == id);
                return prn ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseDetail> GetPRNDetById(long id)
        {

            try
            {
                //IEnumerable<PurchaseDetail> prndet = _unitofwork.PurchaseDetailRepository.Get(p => p.PurchaseHeaderID == id).OrderBy(g => g.LineNo);


                IEnumerable<PurchaseDetail> prndet = (from p in _unitofwork.PurchaseDetailRepository.Get(p => p.PurchaseHeaderID == id)
                                                      join pp in _unitofwork.ProductRepository.Get() on p.ProductID equals pp.ProductId
                                                      where pp.IsActive == true && pp.IsDelete == false
                                                      orderby p.LineNo
                                                      select p).ToList();





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

                IEnumerable<PurchaseDetail> prndet = _unitofwork.PurchaseDetailRepository.Get(p => p.PurchaseHeaderID == id
                    && p.DocumentDate >= frmdate && p.DocumentDate <= to

                    ).OrderBy(g => g.LineNo);
                return prndet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool CheckProductTaxes(long id,int companyid)
        {
            bool IsTaxes = _unitofwork.ProductTaxRepository.Get().Any(pt => pt.ProductId == id && pt.CompanyID== companyid);

            return IsTaxes;

        }

        public PurchaseHeader GetBaseDocNo(long GRNId)
        {
            try
            {
                PurchaseHeader basedocno = _unitofwork.PurchaseHeaderRepository.Get(d => d.PurchaseHeaderId == GRNId).FirstOrDefault();
                return basedocno == null ? null : basedocno;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public bool SavePRN(PurchaseHeader prnheader)
        {
            _unitofwork.CreateTransaction();
            {

                try
                {
                    prnheader.CostCentreID = Convert.ToInt32(prnheader.GRNLocationId);
                    if (prnheader.PRNType == "GRNBased")
                    {
                        var grnheader = _unitofwork.PurchaseHeaderRepository.GetById(prnheader.GRNId);
                        grnheader.IsPRN = true;
                      
                      //  _unitofwork.PurchaseHeaderRepository.Update(grnheader);
                    }
                        _unitofwork.PurchaseHeaderRepository.Insert(prnheader);

                    if (_unitofwork.Save() > 0)
                    {

                        int idx = 1;
                        foreach (var detail in prnheader.GRNDetail)
                        {
                            var productstockmaster = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductID && 
                                                                                                  s.LocationId == prnheader.GRNLocationId && s.CompanyID==prnheader.CompanyID);
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
                            //  detail.IsPRN = true;



                            if (prnheader.PRNType == "GRNBased")
                            {
                                detail.BalanceQty = detail.GRNQuantity - detail.OrderQty;
                                var grndetail = _unitofwork.PurchaseDetailRepository.Get(p => p.PurchaseHeaderID == prnheader.GRNId && p.ProductID == detail.ProductID).FirstOrDefault();
                                grndetail.BalanceQty = detail.GRNQuantity - detail.OrderQty;
                                grndetail.IsPRN = true;
                                grndetail.PRNQuantity = grndetail.PRNQuantity + detail.OrderQty;
                                if (grndetail.GRNQuantity > grndetail.PRNQuantity)
                                {

                                    detail.IsPRN = false;
                                    var grnheader = _unitofwork.PurchaseHeaderRepository.GetById(prnheader.GRNId);
                                    grnheader.IsPRN = false;
                                    grndetail.IsPRN = false;
                                    _unitofwork.PurchaseHeaderRepository.Update(grnheader);
                                    // _unitofwork.PurchaseOrderDetailRepository.Update(grndetail);
                                }

                            }
                            else
                            {
                                detail.IsPRN = true;
                            }

                            //detail.BalanceQty = (detail.OrderQty - detail.GRNQuantity);
                            //if (detail.OrderQty > detail.GRNQuantity)
                            //{
                            //    var poheader = _unitofwork.PurchaseHeaderRepository.GetById(prnheader.GRNId);
                            //    poheader.is = false;
                            //    _unitofwork.PurchaseOrderHeaderRepository.Update(poheader);
                            //}

                            detail.DiscountAmount = 0;
                            detail.DiscountPercentage = 0;

                            //Added by pavithra
                            if (detail.DiscountType == "Prc")
                            {

                                detail.DiscountPercentage = detail.Discount;
                                detail.DiscountAmount = 0;
                            }

                            if (detail.DiscountType == "Amt")
                            {
                                detail.DiscountAmount = detail.Discount;
                                detail.DiscountPercentage = 0;
                            }



                                _unitofwork.PurchaseDetailRepository.Insert(detail);
                            if (_unitofwork.Save() !=0)
                            {
                                if (prnheader.IsTempPRN == false && prnheader.DocumentStatus==3)
                                {
                                    foreach (var ps in productstockmaster)
                                    {
                                        if (ps.ProductId == detail.ProductID)
                                        {
                                           // if (ps.Stock > 0)
                                            {
                                                ps.Stock -= detail.FreeQty + detail.OrderQty;
                                                ps.DocumentNo = detail.DocumentNo;
                                                ps.LastUpdatedDate = DateTime.Now;
                                                _unitofwork.Save();
                                            }
                                        }
                                    }
                                }
                            }
   
                        }

                        if (prnheader.PRNType == "GRNBased")
                        {
                            var grndetail = _unitofwork.PurchaseDetailRepository.Get(p => p.PurchaseHeaderID == prnheader.GRNId);

                            var prnheaders = _unitofwork.PurchaseHeaderRepository.Get(g => g.GRNId == prnheader.GRNId).ToList();
                            int detailcount = 0;
                            List<long> prnproductsp = new List<long>();
                            foreach (var pdet in prnheaders)
                            {
                                detailcount += _unitofwork.PurchaseDetailRepository.Get(p => p.PurchaseHeaderID == pdet.PurchaseHeaderId).Count();
                            }

                            //  var grndetailproducts = grndetail.Where(p => p.OrderQty > p.PRNQuantity).Select(p => p.ProductID).ToList();
                            int grnpcount = grndetail.Select(p => p.ProductID).ToList().Count();
                            var grndetailproducts = grndetail.Where(p => p.OrderQty==p.PRNQuantity).Select(p => p.ProductID).ToList();
                            var grnheader = _unitofwork.PurchaseHeaderRepository.GetById(prnheader.GRNId);
                            //if (grndetailproducts.Count() != 0)
                            if (grndetailproducts.Count() != grnpcount)
                            {

                                grnheader.IsPRN = false;

                            }
                            else
                            {
                                grnheader.IsPRN = true;
                            }

                            _unitofwork.PurchaseHeaderRepository.Update(grnheader);
                            _unitofwork.Save();

                        }

                        _unitofwork.Commit();
                    }
                    else
                    {
                        _unitofwork.Rollback();
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    _unitofwork.Rollback();
                    return false;

                }
            }
        }

        public long DeleteGRNDetail(long grnheaderid)
        {
            var res = 0;
            try
            {
                //Commented below and added new line
                //_unitofwork.PurchaseDetailRepository.Delete(_unitofwork.PurchaseDetailRepository.Get(x => x.PurchaseHeaderID == grnheaderid));
                _unitofwork.PurchaseDetailRepository.DeleteRange(_unitofwork.PurchaseDetailRepository.Get(x => x.PurchaseHeaderID == grnheaderid));
                res = _unitofwork.Save();


            }
            catch (Exception ex)
            {

                throw ex;
            }
            return res;
        }

        public bool UpdatePRN(PurchaseHeader prnheader)
        {
           // ApplicationDbContext db = new ApplicationDbContext();
            _unitofwork.CreateTransaction();
            {

                try
                {
                    var prnInDb = _unitofwork.PurchaseHeaderRepository.Get().Single(a => a.PurchaseHeaderId == prnheader.PurchaseHeaderId);
                    prnheader.CreatedUser = prnInDb.CreatedUser;
                    prnheader.CreatedDate = prnInDb.CreatedDate;
                    prnheader.DocumentID = prnInDb.DocumentID;
                    prnheader.CurrencyID = prnInDb.CurrencyID;
                    prnheader.LocationId = prnInDb.LocationId;
                    prnheader.CompanyID = prnInDb.CompanyID;
                    // Update the properties
                    // _unitofwork.Entry(prnInDb).CurrentValues.SetValues(prnheader);
                    _unitofwork.PurchaseHeaderRepository.UpdateBySet(prnInDb, prnheader);


                    if (_unitofwork.Save() == 1)
                    {

                        DeleteGRNDetail(prnheader.PurchaseHeaderId);
                        int idx = 1;
                        foreach (var detail in prnheader.GRNDetail)
                        {
                            var productstockmaster = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductID && s.LocationId == prnheader.GRNLocationId && s.CompanyID==prnheader.CompanyID);
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

                            _unitofwork.PurchaseDetailRepository.Insert(detail);

                            if (_unitofwork.Save() == 1)
                            {
                                if (prnheader.IsTempPRN == false && prnheader.DocumentStatus==3)
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
                                                _unitofwork.Save();
                                            }

                                        }

                                    }
                                }

                            }

                        }

                    }

                    _unitofwork.Commit();
                    return true;


                }
                catch (Exception ex)
                {

                    _unitofwork.Rollback();
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
                    purchaseheader = _unitofwork.PurchaseHeaderRepository.Get(r => r.PurchaseHeaderId == docid && r.GRNLocationId == locid).
                                                              OrderBy(c => c.DocumentNo).ToList();
                }
                else if (locid != 0 && docid == 0)
                {
                    purchaseheader = _unitofwork.PurchaseHeaderRepository.Get(r => r.GRNLocationId == locid && DbFunctions.TruncateTime(r.GRNDate) >= DbFunctions.TruncateTime(from) && DbFunctions.TruncateTime(r.GRNDate) <= DbFunctions.TruncateTime(to)).
                                                               OrderBy(c => c.DocumentNo).ToList();
                }
                else if (locid == 0 && docid != 0)
                {
                    purchaseheader = _unitofwork.PurchaseHeaderRepository.Get(r => r.PurchaseHeaderId == docid && DbFunctions.TruncateTime(r.GRNDate) >= DbFunctions.TruncateTime(from) && DbFunctions.TruncateTime(r.GRNDate) <= DbFunctions.TruncateTime(to)).
                                                              OrderBy(c => c.DocumentNo).ToList();
                }
                else if (locid == 0 && docid == 0)
                {
                    purchaseheader = _unitofwork.PurchaseHeaderRepository.Get(r =>DbFunctions.TruncateTime(r.GRNDate) >= DbFunctions.TruncateTime(from) && DbFunctions.TruncateTime(r.GRNDate) <= DbFunctions.TruncateTime(to)).
                        OrderBy(c => c.DocumentNo).ToList();
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

            if (locid == 0 && docid == 0)
            {
                dbheader = _unitofwork.PurchaseHeaderRepository.Get(s => DbFunctions.TruncateTime(s.GRNDate) >= DbFunctions.TruncateTime(frmdate) && DbFunctions.TruncateTime(s.GRNDate) <= DbFunctions.TruncateTime(todate) && s.DocumentID==6
                                                               ).ToList();
            }
            else if (locid != 0 && docid != 0)
            {
                dbheader = _unitofwork.PurchaseHeaderRepository.Get(s => s.GRNLocationId == locid && s.PurchaseHeaderId == docid
                                                               ).ToList();
            }
            else if (locid != 0 && docid == 0)
            {
                dbheader = _unitofwork.PurchaseHeaderRepository.Get(s => s.GRNLocationId == locid &&
                                                                DbFunctions.TruncateTime(s.GRNDate) >= DbFunctions.TruncateTime(frmdate) && DbFunctions.TruncateTime(s.GRNDate) <= DbFunctions.TruncateTime(todate) && s.DocumentID == 6
                                                               ).ToList();
            }

            foreach (var header in dbheader)
            {
                PRNDetailViewModel vm = new PRNDetailViewModel();
                vm.Location = _blllocation.GetLocationById(header.GRNLocationId).LocationName;
                vm.DocumentDate = header.GRNDate.ToShortDateString();
                vm.DocumentNo = header.DocumentNo;
                vm.Remark = header.Remark;
                var docstatus = _blldocstatus.GetDocStatusById(header.DocumentStatus);
                if (docstatus != null)
                {
                    vm.Status = docstatus.Description;
                 }
                foreach (var s in _unitofwork.PurchaseDetailRepository.Get(r => r.PurchaseHeaderID == header.PurchaseHeaderId))
                {
                    PRNDetailViewModel.ReportDetail det = new PRNDetailViewModel.ReportDetail();
                    det.ProductId = s.ProductID;
                    var prd = _bllproduct.GetProductById(det.ProductId);
                    if (prd != null)
                    {
                        det.ProductName = prd.ProductName;
                        det.ProductCode = prd.ProductCode;
                    }
                    det.OrderQty = s.OrderQty;
                    det.FreeQty = s.FreeQty;
                    det.CostPrice = s.CostPrice;
                    det.SellingPrice = s.SellingPrice;
                    det.DiscountAmount = s.DiscountAmount;
                    det.DiscountPrc = s.DiscountPercentage;
                    vm.Detail.Add(det);

                }

                reportdata.Add(vm);
            }

            return reportdata;
        }

        

    }
}
