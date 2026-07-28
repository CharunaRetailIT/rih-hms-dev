using RIT.HMS.BLL.MasterData;
using RIT.HMS.Data;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.TransactionData
{
    public class BLL_StockAdjustment
    {
        private readonly UnitOfWork _unitofwork;
        private readonly BLL_Location _blllocation;
        private readonly BLL_Product _bllproduct;
        public BLL_StockAdjustment()
        {
            _unitofwork = new UnitOfWork();
            _blllocation = new BLL_Location();
            _bllproduct = new BLL_Product();
        }
        public BLL_StockAdjustment(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
            _blllocation = new BLL_Location(connection);
            _bllproduct = new BLL_Product(connection);
        }

        public IEnumerable<StockAdjustmentType> GetTypes()
        {
            try
            {
                IEnumerable<StockAdjustmentType> types = _unitofwork.StockAdjustmentTypeRepository.Get().OrderBy(e => e.IsActive == true);
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

        public IEnumerable<StockAdjustmentType> GetTypesById(int Id)
        {
            try
            {
                IEnumerable<StockAdjustmentType> types = _unitofwork.StockAdjustmentTypeRepository.Get(g => g.AdjustmentTypeId == Id).OrderBy(e => e.IsActive == true);
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

        public IEnumerable<StockAdjustmentType> GetTypesByBaseType(string BType)
        {
            try
            {
                IEnumerable<StockAdjustmentType> types = _unitofwork.StockAdjustmentTypeRepository.Get(g => g.BaseType == BType).OrderBy(e => e.IsActive == true);
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

        public IEnumerable<StockAdjustmentHeader> GetDocNoByLocId(long locid,int companyid)
        {
            try
            {
                IEnumerable<StockAdjustmentHeader> docs = _unitofwork.StockAdjustmentHeaderRepository.Get(e => e.StockLocationId == locid && e.CompanyID== companyid)
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

        public List<StockAdjustmentViewModel> GetProductDetails(long productid, long locid, decimal adjStock, string type,int companyid)
        {
            try
            {
                var det = _unitofwork.ProductStockMasterRepository.Get(e => e.ProductId == productid && e.LocationId == locid && e.CompanyID== companyid).ToList();
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
                        s.NewStock = item.Stock - adjStock;

                    }
                    else if (type == "Override")
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

        public StockAdjustmentHeader GetLastInitDate(int companyid,int locationid)
        {
            var lasdate = _unitofwork.StockAdjustmentHeaderRepository.Get(c => c.CompanyID == companyid 
                                                                                && c.StockLocationId == locationid && c.DocumentId==15
                                                                                ).OrderByDescending(c => c.CreatedDate).FirstOrDefault();
            return lasdate;
            //if (lasdate != null)
            //{
            //    return lasdate.CreatedDate;
            //}
            //else
            //{
            //    return DateTime.Now;
            //}
        }

        public List<StockAdjustmentViewModel> GetProductInitDetails(long productid, long locid, decimal adjStock, string type, int companyid)
        {
            try
            {
                var det = _unitofwork.ProductStockMasterRepository.Get(e => e.ProductId == productid && e.LocationId == locid && e.CompanyID == companyid).ToList();
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
                    s.NewStock = adjStock;

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
            //using (var dbtransaction = context.Database.BeginTransaction())
            _unitofwork.CreateTransaction();
            {
                try
                {
                    _unitofwork.StockAdjustmentHeaderRepository.Insert(header);

                    if (_unitofwork.Save() == 1)
                    {
                        foreach (var detail in header.StockAdjDetail)
                        {
                            detail.StockAdjustmentHeaderId = header.StockAdjustmentHeaderId;

                            var productstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId &&
                                                                            s.LocationId == header.StockLocationId 
                                                                            && s.CompanyID==header.CompanyID).First();

                            detail.AvgCost = productstock.AvgCost;
                            
                            productstock.Stock = detail.NewStock;
                            _unitofwork.StockAdjustmentDetailRepository.Insert(detail);
                        }

                        _unitofwork.Save();
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
                    return false;
                }
            }
        }

        public bool SubmitStockAdjustment_(StockAdjustmentHeader header)
        {
            //using (var dbtransaction = context.Database.BeginTransaction())
            _unitofwork.CreateTransaction();
            {
                try
                {
                    _unitofwork.StockAdjustmentHeaderRepository.Insert(header);

                    if (_unitofwork.Save() == 1)
                    {
                        foreach (var detail in header.StockAdjDetail)
                        {
                           detail.StockAdjustmentHeaderId = header.StockAdjustmentHeaderId;

                            var productstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId &&
                                                                            s.LocationId == header.StockLocationId
                                                                            && s.CompanyID == header.CompanyID && s.IsActive == true).First();


                            productstock.Stock = detail.NewStock;
                            _unitofwork.StockAdjustmentDetailRepository.Insert(detail);
                        }

                        _unitofwork.Save();
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
                    return false;
                }
            }
        }

        public bool StockAdjustmentExcelSubmit(DataTable excelStock, int companyId)
        {
            //using (var dbtransaction = context.Database.BeginTransaction())
            _unitofwork.CreateTransaction();
            {
                try
                {
                    List<StockAdjustmentDetail> StockAdjDetailList = new List<StockAdjustmentDetail>();
                    foreach (DataRow row in excelStock.Rows)
                    {
                        StockAdjustmentDetail stockAdjustmentDtl = new StockAdjustmentDetail();
                        stockAdjustmentDtl.ProductCode = row["ProductCode"].ToString();
                        stockAdjustmentDtl.ProductName = row["ProductName"].ToString();
                        stockAdjustmentDtl.NewStock = Convert.ToDecimal(row["NewStock"]);
                    }

                       // _unitofwork.StockAdjustmentHeaderRepository.Insert(header);

                    if (_unitofwork.Save() == 1)
                    {
                        foreach (var detail in StockAdjDetailList)
                        {
                            //detail.StockAdjustmentHeaderId = header.StockAdjustmentHeaderId;

                            //var productstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId &&
                            //                                                s.LocationId == header.StockLocationId
                            //                                                && s.CompanyID == header.CompanyID).First();

                            //detail.AvgCost = productstock.AvgCost;

                            //productstock.Stock = detail.NewStock;
                            _unitofwork.StockAdjustmentDetailRepository.Insert(detail);
                        }

                        _unitofwork.Save();
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
                    return false;
                }
            }
        }

        public List<StockAdjustmentHeader> GetInitializations(int companyid,int locationid,int status)
        {
            return _unitofwork.StockAdjustmentHeaderRepository.Get(filter:s=>s.CompanyID==companyid 
                                                                                && s.LocationId==locationid && s.DocumentStatus==status).ToList();
        }

        public StockAdjustmentHeader GetInitializationById(int id,int companyid, int locationid)
        {
            StockAdjustmentHeader header = new StockAdjustmentHeader();
            header =_unitofwork.StockAdjustmentHeaderRepository.Get(filter: s => s.CompanyID == companyid
                                                                                 && s.LocationId == locationid 
                                                                                 && s.StockAdjustmentHeaderId == id).FirstOrDefault();
            header.StockAdjDetail = _unitofwork.StockAdjustmentDetailRepository.Get(filter:d=>d.StockAdjustmentHeaderId == id).ToList();
            return header;
        }


        public StockMovementViewModel StockMovementReport(StockMovementViewModel vmmovement)
        {
            //var sysproducts = _unitofwork.ProductRepository.Get(p => p.IsActive == true 
            //   && p.CompanyID == vmmovement.CompanyId).OrderBy(p => p.ProductCode).ToList();
            if(vmmovement != null &&    vmmovement.Details != null)
            {
                vmmovement.Details.Clear();
            }

            var sysproducts = (from h in _unitofwork.ProductRepository.Get(p => p.IsActive == true && p.IsDelete == false  
                                                                    && p.CompanyID == vmmovement.CompanyId).OrderBy(p => p.ProductCode)
                               join d in _unitofwork.ProductStockMasterRepository.Get(d => d.LocationId == vmmovement.LocationId && d.CompanyID==vmmovement.CompanyId) on h.ProductId equals d.ProductId
                               select new
                               {
                                   ProductId = h.ProductId,
                                   ProductName = h.ProductName,
                                   ProductCode = h.ProductCode,
                                   Rate=d.CostPrice
                               }).OrderBy(p => p.ProductId).ToList()
                               ;



            List<StockAdjustmentHeader> adjheaders = new List<StockAdjustmentHeader>();
            adjheaders = _unitofwork.StockAdjustmentHeaderRepository.Get(a => a.CompanyID == vmmovement.CompanyId
                                                                              && a.LocationId == vmmovement.LocationId && a.DocumentId == 15
                                                                              && a.DocumentStatus == 2
                                                                              && DbFunctions.TruncateTime(a.CreatedDate) <= DbFunctions.TruncateTime(vmmovement.DateFrom)
                                                                            //  && DbFunctions.TruncateTime(a.CreatedDate) <= DbFunctions.TruncateTime(vmmovement.DateTo)
                                                                             ).OrderByDescending(a => a.CreatedDate).ToList();


            StockAdjustmentHeader beforedatefrom = new StockAdjustmentHeader();
            beforedatefrom = _unitofwork.StockAdjustmentHeaderRepository.Get(a => a.CompanyID == vmmovement.CompanyId
                                                                              && a.LocationId == vmmovement.LocationId && a.DocumentId == 15
                                                                              && a.DocumentStatus == 2                                                                              
                                                                              && DbFunctions.TruncateTime(a.CreatedDate) < DbFunctions.TruncateTime(vmmovement.DateFrom)
                                                                             ).OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            if (beforedatefrom != null)
            {
                adjheaders.Add(beforedatefrom);
            }

            // remove FromDates's record
            List<StockAdjustmentHeader> adjswithouttoday = new List<StockAdjustmentHeader>();
            foreach (var adj in adjheaders)
            {
                var date = adj.CreatedDate;
               if (date.ToShortDateString() != vmmovement.DateFrom.ToShortDateString())
                {
                    adj.StockAdjDetail = _unitofwork.StockAdjustmentDetailRepository.Get(d => d.StockAdjustmentHeaderId == adj.StockAdjustmentHeaderId
                                                                                              && d.BaseType == "SpecialStockInit").ToList();
                    adjswithouttoday.Add(adj);
                }
            }

            var grns = _unitofwork.PurchaseHeaderRepository.Get(h => h.CompanyID == vmmovement.CompanyId
                       && h.GRNLocationId == vmmovement.LocationId && h.DocumentStatus == 3 && h.DocumentID == 4
                       && DbFunctions.TruncateTime(h.GRNDate) >= DbFunctions.TruncateTime(vmmovement.DateFrom)
                       && DbFunctions.TruncateTime(h.GRNDate) <= DbFunctions.TruncateTime(vmmovement.DateTo)
                        );

            foreach (var h in grns)
            {
                h.GRNDetail = _unitofwork.PurchaseDetailRepository.Get(d=>d.PurchaseHeaderID==h.PurchaseHeaderId).ToList();
            }


            //prns
            var prns = _unitofwork.PurchaseHeaderRepository.Get(h => h.CompanyID == vmmovement.CompanyId
                      && h.GRNLocationId == vmmovement.LocationId && h.DocumentStatus == 3 && h.DocumentID == 6
                      && DbFunctions.TruncateTime(h.GRNDate) >= DbFunctions.TruncateTime(vmmovement.DateFrom)
                      && DbFunctions.TruncateTime(h.GRNDate) <= DbFunctions.TruncateTime(vmmovement.DateTo)
                       );

            foreach (var h in prns)
            {
                h.GRNDetail = _unitofwork.PurchaseDetailRepository.Get(d => d.PurchaseHeaderID == h.PurchaseHeaderId).ToList();
            }

            // adjustments
            var adjustments = _unitofwork.StockAdjustmentHeaderRepository.Get(h => h.CompanyID == vmmovement.CompanyId
                     && h.StockLocationId == vmmovement.LocationId  && h.DocumentId == 8 
                     && DbFunctions.TruncateTime(h.CreatedDate) >= DbFunctions.TruncateTime(vmmovement.DateFrom)
                     && DbFunctions.TruncateTime(h.CreatedDate) <= DbFunctions.TruncateTime(vmmovement.DateTo)
                      ).ToList();

            foreach (var h in adjustments)
            {
                //h.StockAdjDetail = _unitofwork.StockAdjustmentDetailRepository.Get(d => d.StockAdjustmentHeaderId == h.StockAdjustmentHeaderId && d.BaseType == "Add").ToList();
                h.StockAdjDetail = _unitofwork.StockAdjustmentDetailRepository.Get(d => d.StockAdjustmentHeaderId == h.StockAdjustmentHeaderId).ToList();
            }

            // transfer in 

            var transferins = _unitofwork.TransferNoteHeaderRepository.Get(h => h.CompanyID == vmmovement.CompanyId
                    && h.ToLocationId == vmmovement.LocationId && h.DocumentId == 5 && h.DocumentStatus==3
                    && DbFunctions.TruncateTime(h.TOGDate) >= DbFunctions.TruncateTime(vmmovement.DateFrom)
                    && DbFunctions.TruncateTime(h.TOGDate) <= DbFunctions.TruncateTime(vmmovement.DateTo)
                     ).ToList();

            foreach (var h in transferins)
            {
                h.TOGDetail = _unitofwork.TransferNoteDetailRepository.Get(d => d.TransferNoteHeaderId == h.TransferNoteHeaderId).ToList();
            }

            // transfer out
            #region Created Transferout 2023-02-23

            var transferouts = _unitofwork.TransferNoteHeaderRepository.Get(h => h.CompanyID == vmmovement.CompanyId
                 && h.FromLocationId == vmmovement.LocationId && h.DocumentId == 5 && h.DocumentStatus == 3
                 && DbFunctions.TruncateTime(h.TOGDate) >= DbFunctions.TruncateTime(vmmovement.DateFrom)
                 && DbFunctions.TruncateTime(h.TOGDate) <= DbFunctions.TruncateTime(vmmovement.DateTo)
                  ).ToList();

            foreach (var h in transferouts)
            {
                h.TOGDetail = _unitofwork.TransferNoteDetailRepository.Get(d => d.TransferNoteHeaderId == h.TransferNoteHeaderId).ToList();
            }

            #endregion

            // opening balance today 
            var openingbalancetoday = _unitofwork.StockAdjustmentHeaderRepository.Get(a => a.CompanyID == vmmovement.CompanyId
                                                                               && a.LocationId == vmmovement.LocationId && a.DocumentId == 15
                                                                               && a.DocumentStatus == 2
                                                                               && DbFunctions.TruncateTime(a.CreatedDate) >= DbFunctions.TruncateTime(vmmovement.DateTo)
                                                                               && DbFunctions.TruncateTime(a.CreatedDate) <= DbFunctions.TruncateTime(vmmovement.DateTo)
                                                                             ).OrderByDescending(a => a.CreatedDate).ToList();
            foreach (var h in openingbalancetoday)
            {
                h.StockAdjDetail = _unitofwork.StockAdjustmentDetailRepository.Get(d => d.StockAdjustmentHeaderId == h.StockAdjustmentHeaderId).ToList();
            }

            #region Get Sales In 2023-02-23

            var SalesIn = _unitofwork.TransactionDetRepository.Get(a => a.LocationID == vmmovement.LocationId && a.DocumentID == 1
                                                                                
                                                                               && DbFunctions.TruncateTime(a.RecDate) >= DbFunctions.TruncateTime(vmmovement.DateFrom)
                                                                               && DbFunctions.TruncateTime(a.RecDate) <= DbFunctions.TruncateTime(vmmovement.DateTo)
                                                                             ).OrderByDescending(a => a.RecDate).ToList();
            foreach (var h in SalesIn)
            {
                h.SalesIN = _unitofwork.TransactionDetRepository.Get(d => d.TransactionDetID == h.TransactionDetID).ToList();
            }
 
            
            #endregion

            #region Get Sales Out 2023-02-23

            var SalesOut = _unitofwork.TransactionDetRepository.Get(a => a.LocationID == vmmovement.LocationId && a.DocumentID == 2

                                                                               && DbFunctions.TruncateTime(a.RecDate) >= DbFunctions.TruncateTime(vmmovement.DateFrom)
                                                                               && DbFunctions.TruncateTime(a.RecDate) <= DbFunctions.TruncateTime(vmmovement.DateTo)
                                                                             ).OrderByDescending(a => a.RecDate).ToList();
            foreach (var h in SalesOut)
            {
                h.SalesOut = _unitofwork.TransactionDetRepository.Get(d => d.TransactionDetID == h.TransactionDetID).ToList();
            }


            #endregion

            foreach (var product in sysproducts)
            {
                StockMovementViewModel.Detail movement = new StockMovementViewModel.Detail();
                movement.ProductId = product.ProductId;
                movement.ProductCode = product.ProductCode;
                movement.ProductDesc = product.ProductName;

                //set opening balance
                foreach (var o in adjswithouttoday)
                {
                    foreach (var d in o.StockAdjDetail)
                    {
                        if (d.ProductId == product.ProductId)
                        {
                            var vm = vmmovement.Details.Where(v => v.ProductId == product.ProductId).FirstOrDefault();
                            if (vm==null)
                            {
                                movement.OpeningBalance = d.NewStock;
                               // break;
                            }                           
                        }                        
                    }           
                }
                //set grns
                foreach (var g in grns)
                {
                    foreach (var d in g.GRNDetail)
                    {
                        if (d.ProductID == product.ProductId)
                        {
                            movement.GRNQty += d.GRNQuantity;
                        }
                    }
                }

                //set prns
                foreach (var g in prns)
                {
                    foreach (var d in g.GRNDetail)
                    {
                        if (d.ProductID == product.ProductId)
                        {
                            movement.ReturnQty += d.OrderQty;
                        }
                    }
                }
                // adjustments
                foreach (var g in adjustments)
                {
                    foreach (var d in g.StockAdjDetail)
                    {
                        if (d.ProductId == product.ProductId)
                        {
                            #region 2023-02-23 Get Overwrite stock true minus to Stock Adjuestment Add and Redeuse.

                            movement.StockAdjustment += d.NewStock - d.CurrentStock;

                            #endregion

                            #region Comment Old Coding 2023-02-23
                            //if (d.BaseType == "Add")
                            //{
                            //    movement.StockAdjustment += d.AdjustStock;
                            //}
                            //else if (d.BaseType == "Reduce")
                            //{
                            //    movement.StockAdjustment -= d.AdjustStock;

                            //}
                        //----------------------------------------------------//
                            //else if(d.BaseType== "Override")
                            //{
                            //    movement.StockAdjustment = d.NewStock;
                            //}

                            //  movement.BaseType = d.BaseType;
                            #endregion

                        }
                    }
                }

                // transfer in
                foreach (var g in transferins)
                {
                    foreach (var d in g.TOGDetail)
                    {
                        if (d.ProductId == product.ProductId)
                        {
                            movement.TransferIn += d.OrderQty;
                        }
                    }
                }

                #region Create Transferout Foreach 2023-02-23

                // transfer Out
                foreach (var g in transferouts)
                {
                    foreach (var d in g.TOGDetail)
                    {
                        if (d.ProductId == product.ProductId)
                        {
                            movement.TransferOut += d.OrderQty;
                        }
                    }
                }

                #endregion

                #region Create Sales IN Foreach 2023-02-23

                foreach (var g in SalesIn)
                {
                    foreach (var d in g.SalesIN)
                    {
                        if (d.ProductID == product.ProductId)
                        {
                            movement.SalesInQty += d.Qty;
                        }
                    }
                }

                #endregion

                #region Create SalesOut Foreach 2023-02-23

                foreach (var g in SalesOut)
                {
                    foreach (var d in g.SalesOut)
                    {
                        if (d.ProductID == product.ProductId)
                        {
                            movement.SalesOutQty += d.Qty;
                        }
                    }
                }

                #endregion

                // Opening Balance Today
                foreach (var g in openingbalancetoday)
                {
                    foreach (var d in g.StockAdjDetail)
                    {
                        if (d.ProductId == product.ProductId)
                        {
                            movement.OpeningBalanceToDate += d.NewStock;
                        }
                    }
                }
                //var stockmaster = _unitofwork.ProductStockMasterRepository.Get(p => p.ProductId == product.ProductId 
                //                                                                && p.LocationId == vmmovement.LocationId 
                //                                                                && p.CompanyID == vmmovement.CompanyId).FirstOrDefault();
                //var stockmaster = _unitofwork.ProductStockMasterRepository.GetById(product.ProductId);

                //if (stockmaster != null)
                //{
                //    movement.Rate = stockmaster.CostPrice;
                //}
                movement.Rate = product.Rate;
                vmmovement.Details.Add(movement);

            }

            foreach( var item in vmmovement.Details)
            {

                decimal soldqty = item.SalesInQty - item.SalesOutQty;
                item.SoldQty = soldqty;
                item.TotalQty = item.OpeningBalance + item.TotalQty + item.GRNQty - item.ReturnQty + item.StockAdjustment + item.TransferIn - item.TransferOut - item.SalesInQty + item.SalesOutQty;
                item.TotalAmount = soldqty * item.Rate;
            }




            return vmmovement;         
        }

        public StockMovementViewModel RPTStockMovement(StockMovementViewModel vmmovement)
        {
            DateTime yesterday = DateTime.Now.AddDays(-1);

            var sysproducts = (from h in _unitofwork.StockAdjustmentHeaderRepository.Get(h=>h.CompanyID== vmmovement.CompanyId && h.DocumentStatus==2 && h.DocumentId==15
                               && h.LocationId== vmmovement.LocationId
                               && DbFunctions.TruncateTime(h.CreatedDate)==DbFunctions.TruncateTime(yesterday)
                               )
                               join d in _unitofwork.StockAdjustmentDetailRepository.Get(d=>d.BaseType== "SpecialStockInit") on h.StockAdjustmentHeaderId equals d.StockAdjustmentHeaderId                              
                               select new
                               {
                                   ProductId = d.ProductId,
                                   ProductName = d.ProductName,
                                   ProductCode = d.ProductCode,

                               }).ToList();

            foreach (var product in sysproducts)
            {
                StockMovementViewModel.Detail movement = new StockMovementViewModel.Detail();
                var stockmaster= _unitofwork.ProductStockMasterRepository.Get(p => p.ProductId == product.ProductId
                                                                                     && p.CompanyID == vmmovement.CompanyId
                                                                                     && p.LocationId == vmmovement.LocationId).FirstOrDefault();
                movement.OpeningBalance = stockmaster.Stock;
                movement.Rate = stockmaster.CostPrice;
                movement.ProductDesc = product.ProductName;
                vmmovement.Details.Add(movement);

            }
       
            return vmmovement;
        }

        public Int64 SubmitStockInitialization(StockAdjustmentHeader header)
        {
           
            _unitofwork.CreateTransaction();
            {
                try
                {
                    //if (!header.IsExists)
                    //{
                    //    //check for selected day approved or not
                    //    var selecteddayheader = _unitofwork.StockAdjustmentHeaderRepository.Get(filter: t => t.DocumentId == 15
                    //                                                              && t.CompanyID == header.CompanyID
                    //                                                              && t.StockLocationId == header.StockLocationId
                    //                                                              && t.CreatedDate == header.CreatedDate
                    //                                                              && t.DocumentStatus==2).FirstOrDefault();

                    //   if (selecteddayheader != null && selecteddayheader.DocumentStatus == 2)
                    //   {
                    //        // var adjindb = _unitofwork.StockAdjustmentHeaderRepository.GetById(selecteddayheader.StockAdjustmentHeaderId);
                    //        // header.StockAdjustmentHeaderId = selecteddayheader.StockAdjustmentHeaderId;
                    //        // _unitofwork.StockAdjustmentHeaderRepository.Update(adjindb);
                    //       // header.StockAdjustmentHeaderId = selecteddayheader.StockAdjustmentHeaderId;
                    //        var selecteddaydetail = _unitofwork.StockAdjustmentDetailRepository.Get(d => d.StockAdjustmentHeaderId == selecteddayheader.StockAdjustmentHeaderId).ToList();

                    //        int detaillength = header.StockAdjDetail.Count();
                    //        int selecteddaylength = selecteddaydetail.Count();
                    //        if (detaillength >= selecteddaylength)
                    //        {
                    //            foreach (var hd in header.StockAdjDetail)
                    //            {
                    //                if (selecteddaydetail.Select(p => p.ProductId).Contains(hd.ProductId))
                    //                {
                    //                    var exsist = selecteddaydetail.Where(d=>d.ProductId==hd.ProductId).First();

                    //                    exsist.AdjustStock += hd.AdjustStock;
                    //                    exsist.NewStock += hd.NewStock;
                    //                    exsist.CostPrice = hd.CostPrice;
                    //                    exsist.SellingPrice = hd.SellingPrice;
                    //                    _unitofwork.StockAdjustmentDetailRepository.Update(exsist);
                    //                }
                    //                else
                    //                {
                    //                    StockAdjustmentDetail dettosave = new StockAdjustmentDetail();
                    //                    dettosave.StockAdjustmentDetailId = selecteddaydetail.First().StockAdjustmentDetailId;
                    //                    dettosave.StockAdjustmentHeaderId = selecteddaydetail.First().StockAdjustmentHeaderId;
                    //                    dettosave.ProductId = hd.ProductId;
                    //                    dettosave.CurrentStock = hd.CurrentStock;
                    //                    dettosave.AdjustStock = hd.AdjustStock;
                    //                    dettosave.NewStock = hd.NewStock;
                    //                    dettosave.CostPrice = hd.CostPrice;
                    //                    dettosave.SellingPrice = hd.SellingPrice;
                    //                    dettosave.AvgCost = hd.AvgCost;
                    //                    dettosave.ProductName = hd.ProductName;
                    //                    dettosave.BaseType = hd.BaseType;
                    //                    dettosave.Reason = hd.Reason;
                    //                    _unitofwork.StockAdjustmentDetailRepository.Insert(dettosave);

                    //                }
                    //            }
                    //        }
                    //        else if (selecteddaylength>=detaillength)
                    //        {
                    //            foreach (var sd in selecteddaydetail)
                    //            {
                    //                if (header.StockAdjDetail.Select(p => p.ProductId).Contains(sd.ProductId))
                    //                {
                    //                    var exsist = header.StockAdjDetail.Where(d => d.ProductId == sd.ProductId).FirstOrDefault();

                    //                    exsist.AdjustStock += sd.AdjustStock;
                    //                    exsist.NewStock += sd.NewStock;
                    //                    exsist.CostPrice += sd.CostPrice;
                    //                    exsist.SellingPrice += sd.SellingPrice;

                    //                    var edb = _unitofwork.StockAdjustmentDetailRepository.Get(s=>s.StockAdjustmentDetailId==sd.StockAdjustmentDetailId
                    //                    && s.ProductId==exsist.ProductId).FirstOrDefault();

                    //                    edb.AdjustStock = exsist.AdjustStock;
                    //                    edb.NewStock = exsist.NewStock;
                    //                    edb.CostPrice = exsist.CostPrice;
                    //                    edb.SellingPrice = exsist.SellingPrice;

                    //                    _unitofwork.StockAdjustmentDetailRepository.Update(edb);
                    //                    _unitofwork.Save();
                    //                }
                    //                else
                    //                {
                    //                    if (header.StockAdjDetail.Count() != 0)
                    //                    {
                    //                        StockAdjustmentDetail dettosave = new StockAdjustmentDetail();
                    //                        // dettosave.StockAdjustmentDetailId = selecteddaydetail.First().StockAdjustmentDetailId;
                    //                        dettosave.StockAdjustmentHeaderId = selecteddaydetail.First().StockAdjustmentHeaderId;

                    //                        var toinsert = header.StockAdjDetail.ToList().First();

                    //                        dettosave.ProductId = toinsert.ProductId;
                    //                        dettosave.CurrentStock = toinsert.CurrentStock;
                    //                        dettosave.AdjustStock = toinsert.AdjustStock;
                    //                        dettosave.NewStock = toinsert.NewStock;
                    //                        dettosave.CostPrice = toinsert.CostPrice;
                    //                        dettosave.SellingPrice = toinsert.SellingPrice;
                    //                        dettosave.AvgCost = toinsert.AvgCost;
                    //                        dettosave.ProductName = toinsert.ProductName;
                    //                        dettosave.BaseType = toinsert.BaseType;
                    //                        dettosave.Reason = toinsert.Reason;

                    //                        header.StockAdjDetail.Remove(toinsert);
                    //                        _unitofwork.StockAdjustmentDetailRepository.Insert(dettosave);
                    //                    }
                    //                }
                    //            }
                    //        }


                    //        // header.StockAdjustmentHeaderId = selecteddayheader.StockAdjustmentHeaderId;
                    //        _unitofwork.Save();
                    //        _unitofwork.Commit();
                    //        return selecteddayheader.StockAdjustmentHeaderId;
                    //    }
                    //   else
                    //   {
                    //        _unitofwork.StockAdjustmentHeaderRepository.Insert(header);
                    //        if (_unitofwork.Save() == 1)
                    //        {
                    //            if (header.DocumentStatus == 2)
                    //            {
                    //                var productsto0stock = _unitofwork.ProductStockMasterRepository.Get(filter: s => s.CompanyID == header.CompanyID
                    //                                                         && s.LocationId == header.LocationId).ToList();
                    //                productsto0stock.ForEach(p => { p.Stock = 0; });
                    //            }
                    //            foreach (var detail in header.StockAdjDetail)
                    //            {
                    //                detail.StockAdjustmentHeaderId = header.StockAdjustmentHeaderId;

                    //                var productstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId &&
                    //                                                                s.LocationId == header.StockLocationId
                    //                                                                && s.CompanyID == header.CompanyID).First();
                    //                detail.AvgCost = productstock.AvgCost;
                    //                if (header.DocumentStatus == 2)
                    //                {
                    //                    productstock.Stock = detail.NewStock;
                    //                }
                    //                _unitofwork.StockAdjustmentDetailRepository.Insert(detail);
                    //            }

                    //            _unitofwork.Save();
                    //            _unitofwork.Commit();
                    //            return header.StockAdjustmentHeaderId;
                    //        }
                    //        else
                    //        {
                    //            _unitofwork.Rollback();
                    //            return 0;
                    //        }
                    //    }
                    //    // end exists for same day
                    //}
                    //else   // update 
                    //{
                    //    var adjindb = _unitofwork.StockAdjustmentHeaderRepository.GetById(header.StockAdjustmentHeaderId);
                    //   // toupdate = header;
                    //    _unitofwork.StockAdjustmentHeaderRepository.UpdateBySet(adjindb, header);
                    //  //  _unitofwork.StockAdjustmentHeaderRepository.Update(toupdate);
                    //    if (_unitofwork.Save() == 1)
                    //    {
                    //        if (header.DocumentStatus == 2)
                    //        {
                    //            var productsto0stock = _unitofwork.ProductStockMasterRepository.Get(filter: s => s.CompanyID == header.CompanyID
                    //                                                     && s.LocationId == header.LocationId).ToList();
                    //            productsto0stock.ForEach(p => { p.Stock = 0; });
                    //        }

                    //        _unitofwork.StockAdjustmentDetailRepository.DeleteRange(
                    //            _unitofwork.StockAdjustmentDetailRepository.Get(d=>d.StockAdjustmentHeaderId==header.StockAdjustmentHeaderId).ToList()
                    //            );
                    //        foreach (var detail in header.StockAdjDetail)
                    //        {
                    //            detail.StockAdjustmentHeaderId = header.StockAdjustmentHeaderId;

                    //            var productstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId &&
                    //                                                            s.LocationId == header.StockLocationId
                    //                                                            && s.CompanyID == header.CompanyID).First();
                    //            detail.AvgCost = productstock.AvgCost;
                    //            if (header.DocumentStatus == 2)
                    //            {
                    //                productstock.Stock = detail.NewStock;
                    //            }
                    //            _unitofwork.StockAdjustmentDetailRepository.Insert(detail);
                    //        }

                    //        _unitofwork.Save();
                    //        _unitofwork.Commit();
                    //        return header.StockAdjustmentHeaderId;
                    //    }
                    //    else
                    //    {
                    //        _unitofwork.Rollback();
                    //        return 0;
                    //    }
                    //}
                    if (!header.IsExists)
                    {
                        
                            _unitofwork.StockAdjustmentHeaderRepository.Insert(header);
                            if (_unitofwork.Save() == 1)
                            {
                                if (header.DocumentStatus == 2)
                                {
                                    var productsto0stock = _unitofwork.ProductStockMasterRepository.Get(filter: s => s.CompanyID == header.CompanyID
                                                                             && s.LocationId == header.LocationId).ToList();
                                    productsto0stock.ForEach(p => { p.Stock = 0; });
                                }
                                foreach (var detail in header.StockAdjDetail)
                                {
                                    detail.StockAdjustmentHeaderId = header.StockAdjustmentHeaderId;

                                    var productstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId &&
                                                                                    s.LocationId == header.StockLocationId
                                                                                    && s.CompanyID == header.CompanyID).First();
                                    detail.AvgCost = productstock.AvgCost;
                                    if (header.DocumentStatus == 2)
                                    {
                                        productstock.Stock = detail.NewStock;
                                    }
                                    _unitofwork.StockAdjustmentDetailRepository.Insert(detail);
                                }

                                _unitofwork.Save();
                                _unitofwork.Commit();
                                return header.StockAdjustmentHeaderId;
                            }
                            else
                            {
                                _unitofwork.Rollback();
                                return 0;
                            }
                        // end exists for same day
                    }
                    else   // update 
                    {
                        var adjindb = _unitofwork.StockAdjustmentHeaderRepository.GetById(header.StockAdjustmentHeaderId);
                        // toupdate = header;
                        _unitofwork.StockAdjustmentHeaderRepository.UpdateBySet(adjindb, header);
                        //  _unitofwork.StockAdjustmentHeaderRepository.Update(toupdate);
                        if (_unitofwork.Save() == 1)
                        {
                            if (header.DocumentStatus == 2)
                            {
                                var productsto0stock = _unitofwork.ProductStockMasterRepository.Get(filter: s => s.CompanyID == header.CompanyID
                                                                         && s.LocationId == header.LocationId).ToList();
                                productsto0stock.ForEach(p => { p.Stock = 0; });
                            }

                            _unitofwork.StockAdjustmentDetailRepository.DeleteRange(
                                _unitofwork.StockAdjustmentDetailRepository.Get(d => d.StockAdjustmentHeaderId == header.StockAdjustmentHeaderId).ToList()
                                );
                            foreach (var detail in header.StockAdjDetail)
                            {
                                detail.StockAdjustmentHeaderId = header.StockAdjustmentHeaderId;

                                var productstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId &&
                                                                                s.LocationId == header.StockLocationId
                                                                                && s.CompanyID == header.CompanyID).First();
                                detail.AvgCost = productstock.AvgCost;
                                if (header.DocumentStatus == 2)
                                {
                                    productstock.Stock = detail.NewStock;
                                }
                                _unitofwork.StockAdjustmentDetailRepository.Insert(detail);
                            }

                            _unitofwork.Save();
                            _unitofwork.Commit();
                            return header.StockAdjustmentHeaderId;
                        }
                        else
                        {
                            _unitofwork.Rollback();
                            return 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _unitofwork.Rollback();
                    return 0;
                }
            }
        }

        public List<StockAdjustmentReportViewModel> StockAdjustmentReport(long locid, long docid, DateTime frmdate, DateTime todate,int companyid)
        {
            List<StockAdjustmentReportViewModel> reportdata = new List<StockAdjustmentReportViewModel>();
            List<StockAdjustmentHeader> dbheader = new List<StockAdjustmentHeader>();

            if (locid == 0 || docid == 0)
            {
                dbheader = _unitofwork.StockAdjustmentHeaderRepository.Get(s => DbFunctions.TruncateTime(s.CreatedDate) >= frmdate.Date && DbFunctions.TruncateTime(s.CreatedDate) <= todate.Date
                                                               && s.StockLocationId == locid && s.CompanyID==companyid && s.DocumentId==8
                                                               ).ToList();
            }
            else if (locid != 0 && docid != 0)
            {
                dbheader = _unitofwork.StockAdjustmentHeaderRepository.Get(s => DbFunctions.TruncateTime(s.CreatedDate) >= frmdate.Date && DbFunctions.TruncateTime(s.CreatedDate) <= todate.Date &&
                                                                            s.StockLocationId == locid && s.StockAdjustmentHeaderId == docid && s.CompanyID == companyid && s.DocumentId == 8
                                                               ).ToList();
            }
            else if (locid != 0 && docid == 0)
            {
                dbheader = _unitofwork.StockAdjustmentHeaderRepository.Get(s => s.StockLocationId == locid &&
                                                                 DbFunctions.TruncateTime(s.CreatedDate) >= frmdate.Date && DbFunctions.TruncateTime(s.CreatedDate) <= todate.Date && s.CompanyID == companyid && s.DocumentId == 8
                                                               ).ToList();
            }

            foreach (var header in dbheader)
            {
                StockAdjustmentReportViewModel vm = new StockAdjustmentReportViewModel();
                var stocklocation = _blllocation.GetLocationById(header.StockLocationId);
                if(stocklocation!=null)
                vm.Location = stocklocation.LocationName;
                vm.DocumentDate = header.CreatedDate.ToShortDateString();
                vm.DocumentNo = header.DocumentNo;
                vm.Remark = header.Remark;
                
                foreach (var s in _unitofwork.StockAdjustmentDetailRepository.Get(r => r.StockAdjustmentHeaderId == header.StockAdjustmentHeaderId))
                {
                    StockAdjustmentReportViewModel.ReportDetail det = new StockAdjustmentReportViewModel.ReportDetail();
                    var prd = _bllproduct.GetProductById(s.ProductId);
                    det.ProductId = s.ProductId;
                    if (prd != null)
                    {
                        det.ProductCode = prd.ProductCode;
                        det.ProductName = prd.ProductName;
                    }
                    else
                    {
                        det.ProductCode = "N/A";
                        det.ProductName = "N/A";
                    }
                    det.CurrentStock = s.CurrentStock;
                    det.AdjustStock = s.AdjustStock;
                    det.NewStock = s.NewStock;
                    det.AvgCost = s.AvgCost;
                    det.BaseType = s.BaseType;
                    det.Reason = s.Reason;
                    //costvalue = s.CostPrice * s.AdjustStock;
                    det.CostPrice = s.CostPrice;
                    det.costvalue= s.CostPrice * s.AdjustStock;
                    det.TotalCostValue += s.CostPrice * s.AdjustStock;
                    vm.Detail.Add(det);
                }
                
                reportdata.Add(vm);
            }



            return reportdata;
        }

        public StockAdjustmentHeader GetHeaderById(int id)
        {
            return _unitofwork.StockAdjustmentHeaderRepository.GetById(id);
        }

        public List<StockAdjustmentDetail> GetDetailByHeaderId(long id)
        {
            return _unitofwork.StockAdjustmentDetailRepository.Get(d=>d.StockAdjustmentHeaderId==id).ToList();
        }


    }
}
