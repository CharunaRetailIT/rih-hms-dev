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
    public class BLL_TransferNote
    {

        private readonly UnitOfWork _unitofwork;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_Location _blllocation;
        private readonly BLL_DocStatus _blldocstatus;
        public BLL_TransferNote()
        {
            _unitofwork = new UnitOfWork();
            _bllproduct = new BLL_Product();
            _blllocation = new BLL_Location();
            _blldocstatus = new BLL_DocStatus();
            _unitofwork = new UnitOfWork();
        }

        public BLL_TransferNote(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
            _bllproduct = new BLL_Product(connectionname);
            _blllocation = new BLL_Location(connectionname);
            _blldocstatus = new BLL_DocStatus(connectionname);
        }
        public IEnumerable<PaymentMethod> GetActivePaymentMethods(int companyid)
        {
            try
            {
                IEnumerable<PaymentMethod> paymentmethods = _unitofwork.PaymentMethodRepository.Get(t => t.CompanyID == companyid).OrderBy(g => g.PaymentMethodName);
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

        public TransferNoteHeader GetBaseDocNo(long Id)
        {
            try
            {
                TransferNoteHeader basedocno = _unitofwork.TransferNoteHeaderRepository.Get(d => d.TransferNoteHeaderId == Id).FirstOrDefault();
                return basedocno == null ? null : basedocno;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public IEnumerable<TransferNoteHeader> GetAllTOGsCurrentDate(int companyid, long locationID, bool IsHeadOffice)
        {
            try
            {
                var today = DateTime.Today.AddDays(1);
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

                if (IsHeadOffice)
                {

                    IEnumerable<TransferNoteHeader> togs = _unitofwork.TransferNoteHeaderRepository.Get(t => t.CompanyID == companyid 
                    && t.DocumentDate >= firstDayOfMonth  && t.DocumentDate < today).OrderByDescending(g => g.DocumentDate);
                    if (togs != null)
                    {
                        return togs;
                    }
                }
                else
                {
                    IEnumerable<TransferNoteHeader> togs = _unitofwork.TransferNoteHeaderRepository.Get(t => t.CompanyID == companyid && t.ToLocationId == locationID && t.DocumentDate >= firstDayOfMonth
              && t.DocumentDate < today).OrderByDescending(g => g.DocumentDate);
                    if (togs != null)
                    {
                        return togs;
                    }

                }
                return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<TransferNoteHeader> GetAllTOGs(int companyid, long locationID, bool IsHeadOffice)
        {
            try
            {
                
                if (IsHeadOffice)
                {

                    IEnumerable<TransferNoteHeader> togs = _unitofwork.TransferNoteHeaderRepository.Get(t => t.CompanyID == companyid).OrderByDescending(g => g.DocumentDate);
                    if (togs != null)
                    {
                        return togs;
                    }
                }
                else 
                {
                    IEnumerable<TransferNoteHeader> togs = _unitofwork.TransferNoteHeaderRepository.Get(t => t.CompanyID == companyid && t.ToLocationId==locationID).OrderByDescending(g => g.DocumentDate);
                    if (togs != null)
                    {
                        return togs;
                    }

                }
                return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public IEnumerable<TransferNoteHeader> GetAllTOGswithDateRange(int companyid, long locationID, bool IsHeadOffice,DateTime FromDate,DateTime ToDate)
        {
            try
            {
                var Enddate = DateTime.Today.AddDays(1);
                if (IsHeadOffice)
                {

                    IEnumerable<TransferNoteHeader> togs = _unitofwork.TransferNoteHeaderRepository.Get(t => t.CompanyID == companyid && t.DocumentDate >= FromDate && t.DocumentDate < Enddate).OrderByDescending(g => g.DocumentDate);
                    if (togs != null)
                    {
                        return togs;
                    }
                }
                else
                {
                    IEnumerable<TransferNoteHeader> togs = _unitofwork.TransferNoteHeaderRepository.Get(t => t.CompanyID == companyid && t.ToLocationId == locationID && t.DocumentDate >= FromDate && t.DocumentDate < Enddate).OrderByDescending(g => g.DocumentDate);
                    if (togs != null)
                    {
                        return togs;
                    }

                }
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
                IEnumerable<TransferNoteDetail> togs = _unitofwork.TransferNoteDetailRepository.Get().OrderBy(g => g.TransferNoteHeaderId == id);
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
                var tog = _unitofwork.TransferNoteHeaderRepository.Get().FirstOrDefault(g => g.TransferNoteHeaderId == id);
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
                //IEnumerable<TransferNoteDetail> togdet = _unitofwork.TransferNoteDetailRepository.Get(p => p.TransferNoteHeaderId == id).OrderBy(g => g.LineNo);
                //return togdet ?? null;

                IEnumerable<TransferNoteDetail> togdet = (from pod in _unitofwork.TransferNoteDetailRepository.Get(p => p.TransferNoteHeaderId == id)
                                                   join psm in _unitofwork.ProductRepository.Get()
                                                   on pod.ProductId equals psm.ProductId
                                                   where psm.IsActive == true && psm.IsDelete == false
                                                   orderby pod.LineNo
                                                   select pod).ToList();
                return togdet ?? null;


            }
            catch (Exception)
            {

                throw;
            }
        }
        public ProductStockMaster GetTOGProductById(long id, long locid, int companyid)
        {
            try
            {
                ProductStockMaster togdet = _unitofwork.ProductStockMasterRepository.Get(p => p.ProductId == id && p.LocationId == locid && p.CompanyID == companyid && p.Stock > 0 && p.CostPrice > 0).OrderBy(g => g.ProductId).First();


                return togdet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ProductStockMaster GetPOProductId(long id, long locid, int companyid)
        {
            try
            {
                ProductStockMaster togdet = _unitofwork.ProductStockMasterRepository.Get(p => p.ProductId == id && p.LocationId == locid && p.CompanyID == companyid && p.CostPrice > 0).OrderBy(g => g.ProductId).First();


                return togdet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }




        public bool CheckProductTaxes(long id, int companyid)
        {
            bool IsTaxes = _unitofwork.ProductTaxRepository.Get().Any(pt => pt.ProductId == id && pt.CompanyID == companyid);

            return IsTaxes;

        }

        public bool CheckProductExistsAtLoc(string foo,long id, long fromloc, long toloc, int companyid)
        {
            try
            {
                if (foo == "PO")
                {
                    //return _unitofwork.ProductStockMasterRepository.Get().Any(l => l.ProductId == id &&
                    //                                                        (l.LocationId == fromloc || l.LocationId == toloc) 
                    //                                                        && l.CompanyID== companyid);
                    return _unitofwork.ProductStockMasterRepository.Get(l => l.ProductId == id &&
                                                                        (l.LocationId == toloc || l.LocationId == fromloc) //
                                                                        && l.CompanyID == companyid).Any();
                }
                else
                {
                    return _unitofwork.ProductStockMasterRepository.Get(l => l.ProductId == id &&
                                                                       (l.LocationId == toloc || l.LocationId == fromloc) //
                                                                       && l.CompanyID == companyid && l.Stock > 0).Any();
                }
            }
            catch (Exception)
            {

                throw;

            }
        }


        public bool SaveTransferNote(TransferNoteHeader togheader)
        {
            _unitofwork.CreateTransaction();
            {

                try
                {
                    togheader.CostCentreId = togheader.FromLocationId;
                    _unitofwork.TransferNoteHeaderRepository.Insert(togheader);

                    if (togheader.TOGType == "RequestNoteBased")
                    {
                        var rqheader = _unitofwork.RequestNoteAccptanceHeaderRepository.GetById(togheader.RequestNoteId);
                        rqheader.IsTOG = true;


                    }

                    if (_unitofwork.Save() > 0)
                    {

                        int idx = 1;
                        foreach (var detail in togheader.TOGDetail)
                        {


                            if(detail.ReqNoteCreatedDate.Year <2000)
                            {

                                detail.ReqNoteCreatedDate = DateTime.Parse("1999-09-29 00:00:00.000");
                            }
                            var formlocstock = _unitofwork.ProductStockMasterRepository.Get(from => from.ProductId == detail.ProductId &&
                                                    from.LocationId == togheader.FromLocationId && from.CompanyID == togheader.CompanyID).First();

                            detail.TransferNoteHeaderId = togheader.TransferNoteHeaderId;
                            detail.StockCode = formlocstock.StockCode;


                            var grndetail = _unitofwork.PurchaseDetailRepository.Get(d => d.PurchaseHeaderID == togheader.GRNId
                                                                        && d.ProductID == detail.ProductId).FirstOrDefault();

                            //if part added by pavithra
                            if (grndetail != null && togheader.IsTempTOG == false && togheader.DocumentStatus == 3)
                            {
                                grndetail.TOGQty += detail.OrderQty;
                            }

                            // request note detail IsTOG Update

                            var rqdetail = _unitofwork.RequestNoteAccptanceDetailRepository.Get(r => r.RequestNoteAccptanceHeaderId == togheader.RequestNoteId
                            && r.ProductId == detail.ProductId
                            ).FirstOrDefault();

                            if (rqdetail != null)
                            {
                                rqdetail.IsTOG = true;
                            }

                            // only for temporary
                            detail.BatchExpiryDate = DateTime.Now;
                            detail.BatchNo = "";
                            detail.IsBatch = false;
                            detail.PackId = 1;
                            detail.SerialNo = "1";
                            detail.UnitOfMeasureId = 1;
                            //

                            detail.LineNo = togheader.TOGDetail.IndexOf(detail) + 1;
                            idx += 1;
                            _unitofwork.TransferNoteDetailRepository.Insert(detail);

                            if (_unitofwork.Save() != 0)
                            {
                                if (togheader.IsTempTOG == false && togheader.DocumentStatus == 3)
                                {
                                    if (formlocstock.ProductId == detail.ProductId
                                      && formlocstock.LocationId == togheader.FromLocationId)
                                    {

                                        formlocstock.Stock -= detail.OrderQty;
                                        formlocstock.DocumentNo = togheader.DocumentNo;
                                        formlocstock.LastUpdatedDate = DateTime.Now;


                                        var tolocstock = _unitofwork.ProductStockMasterRepository.Get(to => to.ProductId == detail.ProductId &&
                                                                                           to.LocationId == togheader.ToLocationId).First();
                                        tolocstock.Stock += detail.OrderQty;
                                        tolocstock.DocumentNo = togheader.DocumentNo;
                                        tolocstock.LastUpdatedDate = DateTime.Now;

                                        _unitofwork.Save();

                                    }
                                }
                            }

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


        public bool CheckIfExist(long fromloc, long toloc, long productid, int companyid)
        {
            bool exists = true;
            var frm = _unitofwork.ProductStockMasterRepository.Get().Any(ps => ps.LocationId == fromloc && ps.ProductId == productid && ps.CompanyID == companyid);
            var to = _unitofwork.ProductStockMasterRepository.Get().Any(ps => ps.LocationId == toloc && ps.ProductId == productid && ps.CompanyID == companyid);
            if (!frm || !to)
            {
                exists = false;
            }
            return exists;
        }

        public decimal CheckFromLocStock(long fromloc, long productid, int companyid)
        {
            if (_unitofwork.ProductStockMasterRepository.Get().Any(p => p.ProductId == productid && p.LocationId == fromloc && p.CompanyID == companyid))
            {
                return _unitofwork.ProductStockMasterRepository.Get(ps => ps.LocationId == fromloc && ps.ProductId == productid && ps.CompanyID == companyid).First().Stock;
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
                _unitofwork.TransferNoteDetailRepository.DeleteRange(_unitofwork.TransferNoteDetailRepository.Get(x => x.TransferNoteHeaderId == id));
                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public bool UpdateTOG(TransferNoteHeader tog,long POHeaderID)
        {
            _unitofwork.CreateTransaction();
            {
                try
                {
                    //if (tog.TOGType == "RequestNoteBased")
                    //{
                    //    var rqheader = _unitofwork.RequestNoteAccptanceHeaderRepository.GetById(tog.ReferenceDocumentId);
                    //    rqheader.IsTOG = true;
                    //    _unitofwork.Save();
                    //}

                    if (_unitofwork.Save() > 0)
                    {

                        //List<TransferNoteDetail> td = new List<TransferNoteDetail>();
                        //td = tog.TOGDetail;

                        DeleteItemsById(tog.TransferNoteHeaderId);

                        //  tog.TOGDetail= td;
                        //update 
                        if (tog.TOGDetail.Count > 0)
                        {

                            int idx = 1;

                            bool isGRNBasedOnRequestNote = false;
                            foreach (var togdetail in tog.TOGDetail)
                            {
                                

                                if (tog.IsTempTOG == false && tog.DocumentStatus == 3)
                                {

                                    var RequestNotePOTransaction = _unitofwork.InvRequestNotePOTransaction.Get(r => r.PurchaseOrderHeaderID == POHeaderID && r.FromLocationID == tog.ToLocationId && r.ProductID == togdetail.ProductId  && r.RequestNoteHeaderID == togdetail.RequestNoteHeaderID).ToList();

                                    if (RequestNotePOTransaction != null)
                                    {
                                      
                                        isGRNBasedOnRequestNote = true;
                                        foreach (var transaction in RequestNotePOTransaction)
                                        {
                                            togdetail.ReqNoteCreatedDate = transaction.ReqNoteCreatedDate;
                                            transaction.IssueQtY = transaction.IssueQtY + togdetail.OrderQty; // Update each transaction
                                            transaction.BalanceQtY = transaction.QTY - transaction.IssueQtY;
                                        }

                                        _unitofwork.Save();

                                    }
                                }



                                if(togdetail.ReqNoteCreatedDate.Year <2000)
                                {

                                    togdetail.ReqNoteCreatedDate = DateTime.Parse("1999-09-29 00:00:00.000");
                                }

                                if(togdetail.RequestNoteDocumentNo==null)
                                {
                                    togdetail.RequestNoteDocumentNo = "";
                                }
                                togdetail.TransferNoteHeaderId = tog.TransferNoteHeaderId;
                                var formlocstock = _unitofwork.ProductStockMasterRepository.Get(from => from.ProductId == togdetail.ProductId &&
                                                    from.LocationId == tog.FromLocationId && tog.CompanyID == tog.CompanyID).First();

                                togdetail.TransferNoteHeaderId = tog.TransferNoteHeaderId;
                                togdetail.StockCode = formlocstock.StockCode;


                                // request note detail IsTOG Update

                                //var rqdetail = _unitofwork.RequestNoteAccptanceDetailRepository.Get(r => r.RequestNoteAccptanceHeaderId == tog.RequestNoteId
                                //&& r.ProductId == togdetail.ProductId
                                //).FirstOrDefault();

                                //if (rqdetail != null)
                                //{
                                //    rqdetail.IsTOG = true;
                                //}

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
                                _unitofwork.TransferNoteDetailRepository.Insert(togdetail);


                                if (tog.IsTempTOG == false && tog.DocumentStatus == 3)
                                {
                                    var GRNDetail = _unitofwork.PurchaseHeaderRepository.Get(s => s.DocumentNo == tog.GRNNo && s.CompanyID == tog.CompanyID).FirstOrDefault();
                                    if(GRNDetail!= null)
                                        {
                                        var SelectGRN_Details = _unitofwork.PurchaseDetailRepository.Get(r => r.PurchaseHeaderID == GRNDetail.PurchaseHeaderId && r.ProductID == togdetail.ProductId).ToList();


                                        foreach (PurchaseDetail PD in SelectGRN_Details)
                                        {
                                            PD.TOGQty = PD.TOGQty + togdetail.OrderQty;
                                        }

                                        _unitofwork.Save();
                                    }
                                }


                                var GRNDetails = _unitofwork.PurchaseHeaderRepository.Get(s => s.DocumentNo == tog.GRNNo && s.CompanyID == tog.CompanyID).FirstOrDefault();
                                if (GRNDetails != null)
                                {
                                    if (isGRNBasedOnRequestNote)
                                    {
                                        var GRN_Details = _unitofwork.PurchaseDetailRepository.Get(s => s.PurchaseHeaderID == GRNDetails.PurchaseHeaderId ).ToList();


                                      




                                        if (GRN_Details.Sum(X=> X.GRNQuantity )  <= GRN_Details.Sum(X => X.TOGQty)    )
                                        {
                                            GRNDetails.IsTOGTransfer = true;
                                        }


                                    }
                                    else
                                    {
                                        GRNDetails.IsTOGTransfer = true;
                                    }
                                    _unitofwork.PurchaseHeaderRepository.Update(GRNDetails);
                                }

                                //if (GRNDetails.POID != 0)
                                //{
                                //    var PODetails = _unitofwork.PurchaseOrderHeaderRepository.Get(s => s.PurchaseOrderHeaderId == GRNDetails.POID && s.CompanyID == tog.CompanyID).FirstOrDefault();


                                //    var RequestNotePO = _unitofwork.InvRequestNotePOTransaction.Get(r => r.PurchaseOrderDocumentNo == PODetails.DocumentNo && r.LocationID == PODetails.POLocationId && r.ProductID == togdetail.ProductId).FirstOrDefault();

                                //    RequestNotePO.IssueQtY = togdetail.OrderQty;
                                //    RequestNotePO.BalanceQtY = RequestNotePO.IssueQtY - togdetail.OrderQty;
                                //    _unitofwork.InvRequestNotePOTransaction.Update(RequestNotePO);

                                //}
                                //else
                                //{

                                    //foreach (var item in RequestNotePO)
                                    //{
                                    //    foreach (var item1 in tog.TOGDetail)
                                    //    {
                                    //        if (item1.ProductId == item.ProductID)
                                    //        {
                                    //            item.IssueQtY = item1.TOGQty;
                                    //            _unitofwork.InvRequestNotePOTransaction.Update(RequestNotePO);
                                    //        }
                                    //    }

                                    //}
                               // }

                                if (_unitofwork.Save() != 0)
                                {
                                    if (tog.IsTempTOG == false && tog.DocumentStatus == 3)
                                    {
                                        if (formlocstock.ProductId == togdetail.ProductId && formlocstock.LocationId == tog.FromLocationId)
                                        {

                                            formlocstock.Stock -= togdetail.OrderQty;
                                            formlocstock.DocumentNo = tog.DocumentNo;
                                            formlocstock.LastUpdatedDate = DateTime.Now;


                                            var tolocstock = _unitofwork.ProductStockMasterRepository.Get(to => to.ProductId == togdetail.ProductId &&
                                                                                               to.LocationId == tog.ToLocationId && to.CompanyID == tog.CompanyID).First();

                                            decimal cqty = tolocstock.Stock;

                                            tolocstock.Stock += togdetail.OrderQty;
                                            decimal maxDecimal183 = 999999999999999.999M;
                                            decimal minDecimal183 = -999999999999999.999M;

                                            // update avg cost 
                                            decimal newqty = togdetail.OrderQty;
                                            decimal crrqty = cqty;
                                            if (togdetail.OrderQty > 0)
                                            {
                                                decimal unitcost = togdetail.CostPrice / togdetail.OrderQty;

                                                decimal a = (newqty * unitcost);
                                                decimal b = (tolocstock.AvgCost * crrqty);
                                                if (crrqty < 0) { crrqty = 1; }
                                                decimal c = (newqty + crrqty);
                                                decimal d = a + b;
                                                decimal avgcost = d / c;
                                                if (avgcost >= minDecimal183 && avgcost <= maxDecimal183)
                                                {
                                                    tolocstock.AvgCost = avgcost;
                                                }
                                                else
                                                {
                                                    tolocstock.AvgCost = togdetail.CostPrice;
                                                }
                                            }
                                            else
                                            {

                                            }
                                            // end update avg cost


                                            tolocstock.DocumentNo = tog.DocumentNo;
                                            tolocstock.LastUpdatedDate = DateTime.Now;
                                            _unitofwork.Save();
                                        }
                                    }
                                }

                            }

                        }

                      

                        _unitofwork.Commit();
                        return true;
                    }
                    else
                    {
                        _unitofwork.Rollback();
                        return false;

                    }

                }

                catch (Exception ex)
                {
                    _unitofwork.Rollback();
                    throw;
                }

            }

        }

        public List<TransferNoteHeader> GetTOGSummaryReport(long locid, long docid, DateTime from, DateTime to)
        {
            try
            {
                DateTime frmdate = from.Date;
                DateTime todate = to.Date;

                List<TransferNoteHeader> transfernoteheader = new List<TransferNoteHeader>();

                if (locid != 0 && docid != 0)
                {
                    transfernoteheader = _unitofwork.TransferNoteHeaderRepository.Get(r => r.TransferNoteHeaderId == docid && r.FromLocationId == locid).
                                                              OrderBy(c => c.DocumentNo).OrderBy(d => d.FromLocationId).ToList();
                }
                else if (locid != 0 && docid == 0)
                {
                    transfernoteheader = _unitofwork.TransferNoteHeaderRepository.Get(r => r.FromLocationId == locid
                      && DbFunctions.TruncateTime(r.TOGDate) >= DbFunctions.TruncateTime(frmdate) && DbFunctions.TruncateTime(r.TOGDate) <= DbFunctions.TruncateTime(todate)).OrderBy(c => c.DocumentNo).OrderBy(d => d.FromLocationId).ToList();
                }
                else if (locid == 0 && docid != 0)
                {
                    transfernoteheader = _unitofwork.TransferNoteHeaderRepository.Get(r => r.TransferNoteHeaderId == docid).
                                                              OrderBy(c => c.DocumentNo).OrderBy(d => d.FromLocationId).ToList();
                }
                else if (locid == 0 && docid == 0)
                {

                    transfernoteheader = _unitofwork.TransferNoteHeaderRepository.Get(r => DbFunctions.TruncateTime(r.TOGDate) >= DbFunctions.TruncateTime(frmdate) && DbFunctions.TruncateTime(r.TOGDate) <= DbFunctions.TruncateTime(todate)
                    ).OrderBy(c => c.DocumentNo).OrderBy(d => d.FromLocationId).ToList();

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
        public IEnumerable<TransferNoteHeader> GetDocNoByLocId(long locid, long tolocid, int companyid)
        {
            try
            {
                IEnumerable<TransferNoteHeader> docs = _unitofwork.TransferNoteHeaderRepository.Get(e => (locid == 0 || e.FromLocationId == locid) && (tolocid == 0 || e.ToLocationId == tolocid) && e.CompanyID == companyid)
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
        public List<TOGDetailViewModel> GetTOGDetailReport(long locid, long tolocid, long docid, DateTime frmdate, DateTime todate)
        {
            List<TOGDetailViewModel> reportdata = new List<TOGDetailViewModel>();
            List<TransferNoteHeader> dbheader = new List<TransferNoteHeader>();

            if (locid == 0 && docid == 0)
            {
                dbheader = _unitofwork.TransferNoteHeaderRepository.Get(s => (tolocid == 0 || s.ToLocationId == tolocid) && DbFunctions.TruncateTime(s.TOGDate) >= DbFunctions.TruncateTime(frmdate) && DbFunctions.TruncateTime(s.TOGDate) <= DbFunctions.TruncateTime(todate)
                                                               ).ToList();
            }
            else if (locid != 0 && docid != 0)
            {
                dbheader = _unitofwork.TransferNoteHeaderRepository.Get(s => s.FromLocationId == locid && (tolocid == 0 || s.ToLocationId == tolocid) && s.TransferNoteHeaderId == docid
                                                               ).ToList();
            }
            else if (locid != 0 && docid == 0)
            {
                dbheader = _unitofwork.TransferNoteHeaderRepository.Get(s => s.FromLocationId == locid && (tolocid == 0 || s.ToLocationId == tolocid) &&
                                                               DbFunctions.TruncateTime(s.TOGDate) >= DbFunctions.TruncateTime(frmdate)

                                                               ).ToList();

                // && DbFunctions.TruncateTime(s.TOGDate) <= DbFunctions.TruncateTime(todate)
            }

            foreach (var header in dbheader)
            {
                TOGDetailViewModel vm = new TOGDetailViewModel();
                var frmloc = _blllocation.GetLocationById(header.FromLocationId);
                if (frmloc != null)
                    vm.Location = frmloc.LocationName;
                var toloc = _blllocation.GetLocationById(header.ToLocationId);
                if (toloc != null)
                    vm.ToLocation = toloc.LocationName;
                vm.DocumentDate = header.TOGDate.ToShortDateString();
                vm.DocumentNo = header.DocumentNo;
                vm.Remark = header.Remark;
                var docstatus = _blldocstatus.GetDocStatusById(header.DocumentStatus);
                if (docstatus != null)
                {
                    vm.Status = docstatus.Description;
                }

                foreach (var s in _unitofwork.TransferNoteDetailRepository.Get(r => r.TransferNoteHeaderId == header.TransferNoteHeaderId))
                {
                    TOGDetailViewModel.ReportDetail det = new TOGDetailViewModel.ReportDetail();
                    det.ProductId = s.ProductId;
                    var prd = _bllproduct.GetProductById(det.ProductId);
                    if (prd != null)
                    {
                        det.ProductName = prd.ProductName;
                        det.ProductCode = prd.ProductCode;
                    }
                    det.OrderQty = s.OrderQty;
                    det.CostPrice = s.CostPrice;
                    det.SellingPrice = s.SellingPrice;
                    vm.Detail.Add(det);

                }

                reportdata.Add(vm);
            }

            return reportdata;
        }
        public int CheckToLocation(long productid, long tolocid, int companyid)
        {
            if (_unitofwork.ProductStockMasterRepository.Get().Any(p => p.ProductId == productid && p.LocationId == tolocid && p.CompanyID == companyid))
            {
                return 1;
            }
            else
            {
                return 0;
            }


        }
        public IEnumerable<TransferNoteHeader> GetTOGsThisWeek(DateTime fromdate, DateTime todate, int companyid)
        {
            try
            {
                IEnumerable<TransferNoteHeader> togs = _unitofwork.TransferNoteHeaderRepository.Get(p => p.IsTempTOG == false
                                                             &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) >= fromdate.Date
                                                             &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) <= todate.Date && p.CompanyID == companyid).OrderBy(g => g.DocumentDate);
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
        public IEnumerable<TransferNoteHeader> GetTOGsToday(DateTime date, int companyid)
        {
            try
            {
                IEnumerable<TransferNoteHeader> togs = _unitofwork.TransferNoteHeaderRepository.Get(p => p.IsTempTOG == false
                                                             &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) >= date.Date && p.CompanyID == companyid
                                                             ).OrderBy(g => g.DocumentDate);
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




    }
}
