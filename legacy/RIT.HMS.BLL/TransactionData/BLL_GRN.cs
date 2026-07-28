using RIT.HMS.BLL.MasterData;
using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Entity;
using RIT.HMS.Domain.ViewModels;

namespace RIT.HMS.BLL.TransactionData
{
    public class BLL_GRN
    {
        private readonly UnitOfWork _unitofwork;
        private readonly BLL_Location _blllocation;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_PurchaseOrder _bllpurchaseorder;
        private readonly BLL_DocStatus _blldocstatus;

        public BLL_GRN()
        {
            _unitofwork = new UnitOfWork();
            _blllocation = new BLL_Location();
            _bllproduct = new BLL_Product();
            _bllpurchaseorder = new BLL_PurchaseOrder();
            _blldocstatus = new BLL_DocStatus();
        }
        public BLL_GRN(string connection)
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
                IEnumerable<PaymentMethod> paymentmethods = _unitofwork.PaymentMethodRepository.Get(p=>p.CompanyID== companyid).OrderBy(g=>g.PaymentMethodName);         
                return paymentmethods == null ? null : paymentmethods;
               
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<PurchaseHeader> GetAllGRNs(int companyid)
        {
            try
            {
                //IEnumerable<PurchaseHeader> grns = _unitofwork.PurchaseHeaderRepository.Get(r => (r.IsTempGRN == true || 
                //(r.IsTempGRN==false && r.IsGRN== false) 
                //|| (r.DocumentStatus!=3 && r.DocumentStatus!=0)
                //)&& r.CompanyID== companyid).OrderBy(g => g.DocumentDate);

                IEnumerable<PurchaseHeader> grns = _unitofwork.PurchaseHeaderRepository.Get().OrderBy(g => g.DocumentDate);

                return grns == null ? null : grns;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseHeader> GetAllGRNsToCurrentMonth(int companyid)
        {
            try
            {
               // IEnumerable<PurchaseHeader> grns = _unitofwork.PurchaseHeaderRepository.Get(r => r.DocumentID == 4 && r.CompanyID == companyid).OrderByDescending(g => g.DocumentDate);
                //  IEnumerable<PurchaseHeader> grns = _unitofwork.PurchaseHeaderRepository.Get(r => r.IsGRN == false).OrderBy(g => g.DocumentDate);

                var today = DateTime.Today.AddDays(1);
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

                IEnumerable<PurchaseHeader> grns = _unitofwork.PurchaseHeaderRepository
                    .Get(p => p.CompanyID == companyid && p.DocumentID == 4
                              && p.DocumentDate >= firstDayOfMonth
                              && p.DocumentDate < today   )
                    .OrderByDescending(g => g.DocumentDate);






                return grns == null ? null : grns;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        
        public IEnumerable<PurchaseHeader> GetAllGRNswithDateRange(int companyid, DateTime FromDate, DateTime dateto)
        {
            try
            {
                // IEnumerable<PurchaseHeader> grns = _unitofwork.PurchaseHeaderRepository.Get(r => r.DocumentID == 4 && r.CompanyID == companyid).OrderByDescending(g => g.DocumentDate);
                //  IEnumerable<PurchaseHeader> grns = _unitofwork.PurchaseHeaderRepository.Get(r => r.IsGRN == false).OrderBy(g => g.DocumentDate);

                //var today = DateTime.Today;
                //var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                DateTime toDatePlusOne = dateto.AddDays(1);
                IEnumerable<PurchaseHeader> grns = _unitofwork.PurchaseHeaderRepository
                    .Get(p => p.CompanyID == companyid && p.DocumentID == 4
                              && p.DocumentDate >= FromDate
                              && p.DocumentDate < toDatePlusOne)
                    .OrderByDescending(g => g.DocumentDate);






                return grns == null ? null : grns;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseHeader> GetAllGRNsToFilter(int companyid)
        {
            try
            {
                IEnumerable<PurchaseHeader> grns = _unitofwork.PurchaseHeaderRepository.Get(r =>  r.DocumentID==4 && r.CompanyID== companyid).OrderByDescending(g => g.DocumentDate);
                //  IEnumerable<PurchaseHeader> grns = _unitofwork.PurchaseHeaderRepository.Get(r => r.IsGRN == false).OrderBy(g => g.DocumentDate);
                return grns == null ? null : grns;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseHeader> GetAllTempGRNs(int companyid)
        {
            try
            {
                IEnumerable<PurchaseHeader> grns =  _unitofwork.PurchaseHeaderRepository.Get(r => r.IsTempGRN == true && r.CompanyID== companyid
                                                    && r.IsGRN == true).OrderBy(g => g.DocumentDate);

                return grns == null ? null : grns;
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
                IEnumerable<PaymentMethod> pm =_unitofwork.PaymentMethodRepository.Get(p=>p.CompanyID== companyid).OrderBy(g => g.PaymentMethodName);
                return pm == null ? null : pm;
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
                return pt == null ? null : pt;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public PurchaseHeader GetGRNById(long id)
        {
            try
            {
                var grn = _unitofwork.PurchaseHeaderRepository.GetById(id);
                return grn ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public PurchaseHeader GetGRNByDocNo(string docno,int companyid)
        {
            try
            {
                var grn = _unitofwork.PurchaseHeaderRepository.Get(g=>g.DocumentNo==docno && g.CompanyID== companyid).FirstOrDefault();
                return grn ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public List<PurchaseDetail> GetGRNDet(long id)
        {
            try
            {
                try
                {
                    var po = _unitofwork.PurchaseDetailRepository.Get(g => g.PurchaseHeaderID == id).ToList();
                    return po ?? null;
                }
                catch (Exception)
                {

                    throw;
                } 
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseDetail> GetGRNDetById(long id)
        {
            try
            {

                //IEnumerable<PurchaseDetail> grndet = _unitofwork.PurchaseDetailRepository.Get(p => p.PurchaseHeaderID == id
                //&& p.IsPRN==false
                //).OrderBy(g => g.LineNo);

                IEnumerable<PurchaseDetail> grndet = (from pod in _unitofwork.PurchaseDetailRepository.Get(p => p.PurchaseHeaderID == id)
                                                   join psm in _unitofwork.ProductRepository.Get()
                                                   on pod.ProductID equals psm.ProductId
                                                   where psm.IsActive == true && psm.IsDelete == false && pod.IsPRN == false
                                                      orderby pod.LineNo
                                                   select pod).ToList();




                return grndet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseDetail> GetGRNDetReportById(long id, DateTime from, DateTime to)
        {
            try
            {
                DateTime frmdate = from.Date;
                DateTime todate = to.Date;

                IEnumerable<PurchaseDetail> grndet = _unitofwork.PurchaseDetailRepository.Get(p => p.PurchaseHeaderID == id
                    && p.DocumentDate >= frmdate && p.DocumentDate <= to).OrderBy(g => g.LineNo);
                return grndet ?? null;
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
        private int UpdatePOStatus(long id)
        {
            try
            {
                                      
                var dbpoheader =_unitofwork.PurchaseOrderHeaderRepository.GetById(id);       
                dbpoheader.IsGRN = true;
                dbpoheader.IsTempPO = false;

                _unitofwork.PurchaseOrderHeaderRepository.Update(dbpoheader);
                return _unitofwork.Save();
            }
            catch (Exception e)
            {
                return 0;
            }

        }
        public bool SaveGRN(PurchaseHeader grnheader)
        {
                _unitofwork.CreateTransaction();

                try
                {
                    if (grnheader.GRNType == "POBased")
                    {

                        UpdatePOStatus(grnheader.POID);
                    }

                    grnheader.CostCentreID = Convert.ToInt32(grnheader.GRNLocationId);
                   _unitofwork.PurchaseHeaderRepository.Insert(grnheader);

                    if (_unitofwork.Save() == 1)
                    {
                     
                        foreach (var detail in grnheader.GRNDetail)
                        {
                            var productstockmaster = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductID
                                                                                        && s.LocationId == grnheader.GRNLocationId
                                                                                        ).FirstOrDefault();

                            if (grnheader.GRNType == "POBased")
                            {
                                var dbpo = _unitofwork.PurchaseOrderDetailRepository.Get(p => p.PurchaseOrderHeaderId == grnheader.POID
                                                                         && p.ProductId == detail.ProductID).FirstOrDefault();

                                    dbpo.GRNQuantity = detail.GRNQuantity + dbpo.GRNQuantity;
                                    //dbpo.GRNQuantity = detail.GRNQuantity;
                                    dbpo.BalanceQty = (dbpo.OrderQty - dbpo.GRNQuantity);
                                    dbpo.IsGRN = true;

                                    var poheader = _unitofwork.PurchaseOrderHeaderRepository.GetById(grnheader.POID);
                                    if (dbpo.OrderQty > dbpo.GRNQuantity)
                                    {
                                        dbpo.IsGRN = false;
                                        poheader.IsGRN = false;
                                    }
                                    //else
                                    //{
                                    //    dbpo.IsGRN = true;
                                    //    poheader.IsGRN = true;
                                    //}

                            _unitofwork.PurchaseOrderHeaderRepository.Update(poheader);
                            }

                            // calculates avg cost
                            decimal avaragecost = 0;
                            if (detail.GRNQuantity != 0)
                            {
                            
                            decimal maxDecimal183 = 999999999999999.999M;
                            decimal minDecimal183 = -999999999999999.999M;

                            decimal newqty = detail.GRNQuantity;
                                decimal crrqty = productstockmaster.Stock;
                                decimal unitcost = detail.CostValue / detail.GRNQuantity;
                                decimal a = (newqty * unitcost);
                                decimal b = (productstockmaster.AvgCost * crrqty);
                                if (crrqty < 0) { crrqty = 1; }
                                decimal c = (newqty + crrqty + detail.FreeQty);
                                decimal d = a + b;
                                decimal avgcost = d / c;
                            avgcost = Math.Round(avgcost, 2, MidpointRounding.AwayFromZero);
                            if (avgcost >= minDecimal183 && avgcost <= maxDecimal183)
                            {
                                detail.AvgCost = avgcost;
                                avaragecost = avgcost;
                            }
                            else
                            {

                                detail.AvgCost = detail.CostPrice;
                                
                                avaragecost = detail.CostPrice;
                                
                            }


                                
                            }
                            else
                            {
                                detail.AvgCost = 0;
                                avaragecost = 0;
                            }

                            detail.PurchaseHeaderID = grnheader.PurchaseHeaderId;
                            detail.StockCode = productstockmaster.StockCode;
                            detail.LineNo = grnheader.GRNDetail.IndexOf(detail) + 1;
                            //  idx += 1;
                            detail.BatchNo = "0";
                            // detail.ExpiryDate = DateTime.Now;
                            detail.SerialNo = "0";
                            detail.DocumentID = grnheader.DocumentID;
                            detail.DocumentNo = grnheader.DocumentNo;
                            detail.ProductRemark = "";
                            detail.DocumentDate = DateTime.Now;
                            detail.CostCentreID = Convert.ToInt32(grnheader.CostCentreID);
                            
                            detail.GrossAmount = detail.CostValue;
                            detail.NetAmount = (detail.CostValue - detail.DiscountAmount) + detail.TotalTax;

                             detail.DiscountAmount = 0;
                             detail.DiscountPercentage = 0;

                        //Added by pavithra
                        if (detail.DiscountType == "Prc")
                            {
                            //detail.DiscountAmount = ((detail.CostPrice * detail.GRNQuantity) * detail.Discount) / 100;
                            //detail.DiscountPercentage = detail.Discount;
                            detail.DiscountPercentage = ((detail.CostPrice * detail.GRNQuantity) * detail.Discount) / 100;
                            detail.DiscountAmount = 0;
                            }

                            if(detail.DiscountType == "Amt")
                            {
                                detail.DiscountAmount = detail.Discount;
                                detail.DiscountPercentage = 0;
                            }
                            //else
                            //{
                            //    detail.DiscountAmount = 0;
                            //    detail.DiscountPercentage = 0;
                            //}
                        
                            _unitofwork.PurchaseDetailRepository.Insert(detail);
                        //var sss = _unitofwork.Save();

                        if (_unitofwork.Save() != 0)
                        {
                            if (grnheader.IsTempGRN == false && grnheader.DocumentStatus==3)
                            {

                                if (productstockmaster.ProductId == detail.ProductID)
                                {

                                    if (productstockmaster.Stock > 0)
                                    {
                                        PriceLevel pl = new PriceLevel();
                                        pl.ProductId = detail.ProductID;

                                        pl.CostPrice = detail.CostPrice;
                                        pl.SellingPrice = detail.SellingPrice;
                                        pl.Qty = detail.GRNQuantity + detail.FreeQty;

                                        pl.CreatedUser = grnheader.CreatedUser;

                                        pl.CreatedDate = DateTime.Now;
                                        pl.ModifiedDate = DateTime.Now;
                                        pl.LocationId = detail.CostCentreID;
                                        pl.DocumentId = Convert.ToInt32(detail.PurchaseHeaderID);

                                        _unitofwork.PriceLevelRepository.Insert(pl);
                                        if (grnheader.IsTempGRN == false)
                                        {
                                            _unitofwork.Save();
                                        }

                                    }


                                    productstockmaster.AvgCost = avaragecost;
                                    productstockmaster.Stock += detail.FreeQty + detail.GRNQuantity;
                                    if (detail.GRNQuantity != 0)
                                    {
                                        //Below line commented and new line added by pavithra to correct the stockmaster price saving issue
                                        //productstockmaster.SellingPrice = detail.SellingPrice / detail.GRNQuantity;
                                        productstockmaster.SellingPrice = detail.SellingPrice;

                                        productstockmaster.CostPrice = detail.CostPrice;
                                    }
                                    else
                                    {
                                        productstockmaster.SellingPrice = 0;
                                        productstockmaster.CostPrice = 0;
                                    }


                                    /// detail.GRNQuantity;                                               
                                    productstockmaster.DocumentNo = detail.DocumentNo;
                                    productstockmaster.LastUpdatedDate = DateTime.Now;
                                    if (grnheader.IsTempGRN == false)
                                    {
                                        _unitofwork.Save();
                                    }

                                }

                                // Update Recipes
                               // if (_bllproduct.GetProductById(detail.ProductID).IsCostOnReceipe)
                               // {
                                    UpdateReceipes(productstockmaster.AvgCost, detail.ProductID, grnheader.GRNLocationId,grnheader.CompanyID);
                               // }

                            }

                        }
                        else
                        {
                            _unitofwork.Rollback();
                            return false;
                        }

                     }
                        if (grnheader.GRNType == "POBased")
                        {

                        var podetail = _unitofwork.PurchaseOrderDetailRepository.Get(p=>p.PurchaseOrderHeaderId==grnheader.POID);
                        //var grndetail = _unitofwork.PurchaseDetailRepository.Get(g=>g.PurchaseHeaderID==grnheader.PurchaseHeaderId);

                        var grnheaders = _unitofwork.PurchaseHeaderRepository.Get(g => g.POID == grnheader.POID).ToList();
                        int detailcount = 0;
                        List<long> grnp=new List<long>();
                        foreach (var pdet in grnheaders )
                        {
                            var grndetail = _unitofwork.PurchaseDetailRepository.Get(p => p.PurchaseHeaderID == pdet.PurchaseHeaderId);
                            detailcount += grndetail.Count();
                            grnp.AddRange(grndetail.Select(g=>g.ProductID));
                        }

                        var  podetailproducts = podetail.Where(p=>p.OrderQty>p.GRNQuantity).Select(p=>p.ProductId).ToList();                        
                        var poheader = _unitofwork.PurchaseOrderHeaderRepository.GetById(grnheader.POID);
                 
                        //grnp.Distinct().Count() !=
                        if (podetailproducts.Count() != 0)
                        {
                            //   poheader.IsGRN = false;
                            poheader.IsGRN = true;

                        }
                        else
                        {
                            //poheader.IsGRN = true;
                            poheader.IsGRN = false;
                        }

                        _unitofwork.PurchaseOrderHeaderRepository.Update(poheader);
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

        private void UpdateReceipes(decimal avgcost, decimal materialid, long locationid , int companyid)
        {


            var menustoupdatecost = _unitofwork.ProductRepository.Get(p => p.IsRowMaterial == false && p.IsCostOnReceipe == true && 
                                                                p.LocationId == locationid && p.CompanyID == companyid).ToList();



            if (menustoupdatecost.Count > 0)
            {
                foreach (var m in menustoupdatecost)
                {

                    var recipes = _unitofwork.ReceipeRepository.Get(r => r.ProductId == m.ProductId && r.CompanyID==companyid && r.LocationId==locationid).
                                    Select(p => new { p.ProductId, p.ProductServingUnitId }).ToList();
                    var recipe = recipes.Distinct();

                    foreach (var r in recipe)
                    {
                       
                        var recipetoupdate = _unitofwork.ReceipeRepository.Get(re=>re.ProductId==r.ProductId && 
                                                                                    re.ProductServingUnitId==r.ProductServingUnitId &&
                                                                                    re.MaterialId==materialid &&
                                                                                    re.CompanyID ==companyid && 
                                                                                    re.LocationId== locationid
                                                                                    ).ToList();
                       
                        foreach (var re in recipetoupdate)
                        {
                            var  mat = _unitofwork.ProductRepository.GetById(re.MaterialId);
                            var  stockmaster = _unitofwork.ProductStockMasterRepository.Get(s=>s.ProductId==re.MaterialId 
                                                                                                && s.LocationId==locationid && 
                                                                                                s.CompanyID==companyid).FirstOrDefault();
                            bool isnoeffect = false;
                            if (mat != null)
                            {
                                isnoeffect = mat.IsNoEffectCostforMenu;
                                if (mat.WeightPerUnit != 0)
                                {
                                    stockmaster.SubUnitValue = _unitofwork.UnitConversionRepository.GetById(mat.WeightPerUnit).SubUnitValue;
                                }
                                if (stockmaster.SubUnitValue == 0)
                                    stockmaster.SubUnitValue = 1;
                            }
                            if (isnoeffect == false)
                            {
                                re.CostPrice = (stockmaster.AvgCost / stockmaster.SubUnitValue) * re.Quantity;

                                _unitofwork.ReceipeRepository.Update(re);

                                var recipeindb = _unitofwork.ReceipeRepository.Get(k => k.ProductId == r.ProductId && k.ProductServingUnitId==re.ProductServingUnitId && 
                                                                                                        k.CompanyID==companyid && k.LocationId==locationid).ToList();

                                var servingunit = _unitofwork.ProductServingUnitRepository.GetById(re.ProductServingUnitId);
                                servingunit.CostPrice = recipeindb.Sum(c => c.CostPrice);
                                _unitofwork.ProductServingUnitRepository.Update(servingunit);

                                _unitofwork.Save();
                            }

                        }
                    }


                //    int rrr = 0;
                    //////////////////////////
                                     
                }

            }

        }

        public long DeleteGRNDetail(long grnheaderid)
        {
            var res = 0;
            try
            {
                _unitofwork.PurchaseDetailRepository.DeleteRange(_unitofwork.PurchaseDetailRepository.Get(x => x.PurchaseHeaderID == grnheaderid));
                res = _unitofwork.Save();


            }
            catch (Exception)
            {

                throw;
            }
            return res;
        }

        public bool UpdateGRNForNewCancellation(PurchaseHeader grnheader)
        {
            _unitofwork.CreateTransaction();


            try
            {
                var grnInDb = _unitofwork.PurchaseHeaderRepository.GetById(grnheader.PurchaseHeaderId);
                grnheader.CreatedUser = grnInDb.CreatedUser;
                grnheader.CreatedDate = grnInDb.CreatedDate;
                grnheader.DocumentID = grnInDb.DocumentID;
                grnheader.CurrencyID = grnInDb.CurrencyID;
                grnheader.CompanyID = grnInDb.CompanyID;
                grnheader.LocationId = grnInDb.LocationId;
                // Update the properties
                _unitofwork.PurchaseHeaderRepository.UpdateBySet(grnInDb, grnheader);
                //   context.Entry(grnInDb).CurrentValues.SetValues(grnheader);

                if (_unitofwork.Save() == 1)
                {

                    DeleteGRNDetail(grnheader.PurchaseHeaderId);
                    int idx = 1;
                    foreach (var detail in grnheader.GRNDetail)
                    {
                        var productstockmaster = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductID
                                                                                        && s.LocationId == grnheader.GRNLocationId).ToList();
                        if (grnInDb.GRNType == "POBased")
                        {

                            var dbpo = _unitofwork.PurchaseOrderDetailRepository.Get(p => p.PurchaseOrderHeaderId == grnheader.POID
                                                               && p.ProductId == detail.ProductID).FirstOrDefault();
                            //   dbpo.GRNQuantity = detail.GRNQuantity + dbpo.GRNQuantity;
                            //  dbpo.BalanceQty = (detail.OrderQty - detail.GRNQuantity);

                        }


                        detail.PurchaseHeaderID = grnheader.PurchaseHeaderId;
                        detail.StockCode = productstockmaster.First().StockCode;

                        detail.LineNo = idx;
                        idx += 1;
                        detail.BatchNo = "0";
                        //  detail.ExpiryDate = DateTime.Now;
                        detail.SerialNo = "0";
                        detail.DocumentID = grnheader.DocumentID;
                        detail.DocumentNo = grnheader.DocumentNo;
                        detail.ProductRemark = "";
                        detail.DocumentDate = DateTime.Now;
                        detail.CostCentreID = grnheader.LocationId;

                        //Added by pavithra
                        if (detail.DiscountType == "Prc")
                        {
                            detail.DiscountPercentage = ((detail.CostPrice * detail.GRNQuantity) * detail.Discount) / 100;
                            detail.DiscountAmount = 0;
                        }
                        else if (detail.DiscountType == "Amt")
                        {
                            detail.DiscountPercentage = 0;
                            detail.DiscountAmount = detail.Discount;
                        }
                        else
                        {
                            detail.DiscountAmount = 0;
                            detail.DiscountPercentage = 0;
                        }

                        _unitofwork.PurchaseDetailRepository.Insert(detail);
                        //  var rrr=_unitofwork.Save();
                        // if (_unitofwork.Save() == 1)
                        if (_unitofwork.Save() != 0)
                        {    
                            // update  recipe cost here... 
                            if (grnheader.IsTempGRN == false && grnheader.DocumentStatus == 3)
                            {
                                UpdateReceipes(detail.AvgCost, detail.ProductID, grnheader.GRNLocationId, grnheader.CompanyID);
                            }

                        }
                        else
                        {
                            _unitofwork.Rollback();
                            return false;
                        }

                    }

                    if (grnheader.GRNType == "POBased")
                    {
                        var podetails = _unitofwork.PurchaseOrderDetailRepository.Get(p => p.PurchaseOrderHeaderId == grnheader.POID);
                        var grndetail = _unitofwork.PurchaseDetailRepository.Get(g => g.PurchaseHeaderID == grnheader.PurchaseHeaderId);
                       
                            foreach (var podetail in podetails)
                            {
                                podetail.GRNQuantity = 0; // Update Field1 with the new value
                                podetail.IsGRN = false; // Update Field2 with the new value
                            }
                        _unitofwork.Save();
                        var poheader = _unitofwork.PurchaseOrderHeaderRepository.GetById(grnheader.POID);
                            poheader.IsGRN = false;
                            _unitofwork.PurchaseOrderHeaderRepository.Update(poheader);
                            _unitofwork.Save();
                        
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
                return false;
            }

        }


        public bool UpdateGRN(PurchaseHeader grnheader)
        {
            _unitofwork.CreateTransaction();
        

                try
                {
                    var grnInDb = _unitofwork.PurchaseHeaderRepository.GetById(grnheader.PurchaseHeaderId);
                    grnheader.CreatedUser = grnInDb.CreatedUser;
                    grnheader.CreatedDate = grnInDb.CreatedDate;
                    grnheader.DocumentID = grnInDb.DocumentID;
                    grnheader.CurrencyID = grnInDb.CurrencyID;
                    grnheader.CompanyID = grnInDb.CompanyID;
                    grnheader.LocationId = grnInDb.LocationId;
                // Update the properties
                    _unitofwork.PurchaseHeaderRepository.UpdateBySet(grnInDb, grnheader);
                //   context.Entry(grnInDb).CurrentValues.SetValues(grnheader);

                if (_unitofwork.Save() == 1)
                {

                    DeleteGRNDetail(grnheader.PurchaseHeaderId);
                    int idx = 1;
                    foreach (var detail in grnheader.GRNDetail)
                    {
                        var productstockmaster = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductID
                                                                                        && s.LocationId == grnheader.GRNLocationId).ToList();
                        if (grnInDb.GRNType == "POBased")
                        {

                            var dbpo = _unitofwork.PurchaseOrderDetailRepository.Get(p => p.PurchaseOrderHeaderId == grnheader.POID
                                                               && p.ProductId == detail.ProductID).FirstOrDefault();
                         //   dbpo.GRNQuantity = detail.GRNQuantity + dbpo.GRNQuantity;
                          //  dbpo.BalanceQty = (detail.OrderQty - detail.GRNQuantity);

                        }


                        detail.PurchaseHeaderID = grnheader.PurchaseHeaderId;
                        detail.StockCode = productstockmaster.First().StockCode;

                        detail.LineNo = idx;
                        idx += 1;
                        detail.BatchNo = "0";
                      //  detail.ExpiryDate = DateTime.Now;
                        detail.SerialNo = "0";
                        detail.DocumentID = grnheader.DocumentID;
                        detail.DocumentNo = grnheader.DocumentNo;
                        detail.ProductRemark = "";
                        detail.DocumentDate = DateTime.Now;
                        detail.CostCentreID = grnheader.LocationId;

                        //Added by pavithra
                        if (detail.DiscountType == "Prc")
                        {
                            detail.DiscountPercentage = ((detail.CostPrice * detail.GRNQuantity) * detail.Discount) / 100;
                            detail.DiscountAmount = 0;
                        }
                        else if (detail.DiscountType == "Amt")
                        {
                            detail.DiscountPercentage = 0;
                            detail.DiscountAmount = detail.Discount;
                        }
                        else
                        {
                            detail.DiscountAmount = 0;
                            detail.DiscountPercentage = 0;
                        }

                        _unitofwork.PurchaseDetailRepository.Insert(detail);
                        //  var rrr=_unitofwork.Save();
                        // if (_unitofwork.Save() == 1)
                        if (_unitofwork.Save() != 0)
                        {

                            foreach (var ps in productstockmaster)
                            {
                                if (ps.ProductId == detail.ProductID)
                                {

                                    if (ps.Stock > 0)
                                    {
                                        PriceLevel pl = new PriceLevel();
                                        pl.ProductId = detail.ProductID;

                                        //pl.CostPrice = detail.CostPrice / detail.OrderQty;
                                        //pl.SellingPrice = detail.SellingPrice / detail.OrderQty;
                                        //pl.Qty = detail.OrderQty + detail.FreeQty;


                                        if (detail.GRNQuantity != 0)
                                        {
                                            //pl.CostPrice = detail.CostPrice / detail.GRNQuantity;
                                            //pl.SellingPrice = detail.SellingPrice / detail.GRNQuantity;
                                            pl.CostPrice = detail.CostPrice;
                                            pl.SellingPrice = detail.SellingPrice;

                                        }
                                        else
                                        {
                                            pl.CostPrice = 0;
                                            pl.SellingPrice = 0;

                                        }

                                        pl.Qty = detail.GRNQuantity + detail.FreeQty;
                                        pl.CreatedUser = grnheader.CreatedUser;
                                        pl.CreatedDate = DateTime.Now;
                                        pl.ModifiedDate = DateTime.Now;
                                        pl.LocationId = detail.CostCentreID;
                                        pl.DocumentId = Convert.ToInt32(detail.PurchaseHeaderID);
                                        _unitofwork.PriceLevelRepository.Insert(pl);
                                        _unitofwork.Save();
                                    }

                                    //ps.Stock += detail.FreeQty + detail.OrderQty;
                                    //ps.SellingPrice = detail.SellingPrice / detail.OrderQty;
                                    //ps.CostPrice = detail.CostPrice / detail.OrderQty;
                                    decimal currentstock = ps.Stock;
                                    if (grnheader.DocumentStatus == 3)
                                    {
                                        ps.Stock += detail.FreeQty + detail.GRNQuantity;
                                        ps.CostPrice = detail.CostPrice;
                                                                              
                                    }
                                    else
                                    {
                                        ps.Stock = ps.Stock;
                                    }

                                    decimal averagecost = 0;
                                    if (detail.GRNQuantity != 0)
                                    {

                                        decimal maxDecimal183 = 999999999999999.999M;
                                        decimal minDecimal183 = -999999999999999.999M;


                                        decimal newqty = detail.GRNQuantity;
                                        decimal crrqty = currentstock;
                                        decimal unitcost = detail.CostValue / detail.GRNQuantity;
                                        decimal a = (newqty * unitcost);
                                        decimal b = (ps.AvgCost * crrqty);
                                        if (crrqty < 0) { crrqty = 1; }
                                        decimal c = (newqty + crrqty + detail.FreeQty);
                                        decimal d = a + b;
                                        decimal avgcost = d / c;


                                        avgcost = Math.Round(avgcost, 2, MidpointRounding.AwayFromZero);
                                        if (avgcost >= minDecimal183 && avgcost <= maxDecimal183)
                                        {
                                            detail.AvgCost = avgcost;
                                            ps.AvgCost = avgcost;
                                        }
                                        else
                                        {
                                            detail.AvgCost = detail.CostPrice;
                                            ps.AvgCost = detail.CostPrice;


                                            
                                        }


                                        
                                    }



                                    //if (detail.GRNQuantity != 0)
                                    //{
                                    //    ps.SellingPrice = detail.SellingPrice / detail.GRNQuantity;
                                    //    ps.CostPrice = detail.CostPrice / detail.GRNQuantity;

                                    //}
                                    //else
                                    //{
                                    //    ps.CostPrice = 0;
                                    //    ps.SellingPrice = 0;

                                    //}

                                    ps.DocumentNo = detail.DocumentNo;
                                    ps.LastUpdatedDate = DateTime.Now;
                                    _unitofwork.Save();

                                }

                            }

                            // update  recipe cost here... 
                            if (grnheader.IsTempGRN == false && grnheader.DocumentStatus == 3)
                            {
                                UpdateReceipes(detail.AvgCost, detail.ProductID, grnheader.GRNLocationId, grnheader.CompanyID);
                            }

                        }
                        else
                        {
                            _unitofwork.Rollback();
                            return false;
                        }

                    }

                    if (grnheader.GRNType == "POBased")
                    {
                        var podetail = _unitofwork.PurchaseOrderDetailRepository.Get(p => p.PurchaseOrderHeaderId == grnheader.POID);
                        var grndetail = _unitofwork.PurchaseDetailRepository.Get(g => g.PurchaseHeaderID == grnheader.PurchaseHeaderId);
                        if (podetail.Count() != grndetail.Count())
                        {
                            var poheader = _unitofwork.PurchaseOrderHeaderRepository.GetById(grnheader.POID);
                            poheader.IsGRN = false;
                            _unitofwork.PurchaseOrderHeaderRepository.Update(poheader);
                            _unitofwork.Save();
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
                  return false;
             }
            
        }

        public IEnumerable<PurchaseHeader> GetGRNByLocId(long locid,int companyid)
        {
            try
            {
                IEnumerable<PurchaseHeader> grn = _unitofwork.PurchaseHeaderRepository.Get(p => p.GRNLocationId == locid && p.DocumentStatus == 1 && p.IsGRN == true && p.CompanyID== companyid).OrderBy(g => g.DocumentNo);
                return grn ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseHeader> GetGRNByLocIdNew(long locid,int companyid)
        {

            //Select(p => new { p.ProductId, p.ProductCode, p.ProductName, p.IsActive, p.IsDelete, p.IsRowMaterial }).Where(g => g.IsDelete == false &&
         //   g.IsActive == true)
            try
            {
                var grn = _unitofwork.PurchaseHeaderRepository.Get(p => p.GRNLocationId == locid && p.DocumentStatus == 1 && p.IsGRN == true && p.CompanyID== companyid).Select(p => new { p.PurchaseHeaderId,p.DocumentNo,p.DocumentStatus ,p.IsGRN})
                    .OrderBy(g => g.DocumentNo);

                List<PurchaseHeader> grns = new List<PurchaseHeader>();
                foreach (var p in grn)
                {
                    PurchaseHeader prd = new PurchaseHeader();
                    prd.PurchaseHeaderId = p.PurchaseHeaderId;
                    prd.DocumentNo = p.DocumentNo;
                    grns.Add(prd);
                }


                return grns ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseDetail> GetAllGRNProducts(long id)
        {
            try
            {
                IEnumerable<PurchaseDetail> det = _unitofwork.PurchaseDetailRepository.Get(r => r.PurchaseHeaderID == id).OrderBy(g => g.LineNo);
                if (det != null)
                {
                    return det;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseDetail> GetAllGRNProductsByDocNumber(string docnum,int companyid)
        {
            try
            {

                var grnheader = _unitofwork.PurchaseHeaderRepository.Get(p => p.DocumentNo == docnum && p.CompanyID== companyid).FirstOrDefault();
                if (grnheader.IsPRN==false)
                {
                   // IEnumerable<PurchaseDetail> det = _unitofwork.PurchaseDetailRepository.Get(r => r.DocumentNo == docnum).OrderBy(g => g.LineNo);


                    IEnumerable<PurchaseDetail> det = (from pod in _unitofwork.PurchaseDetailRepository.Get(r => r.DocumentNo == docnum)
                                                       join psm in _unitofwork.ProductRepository.Get()
                                                       on pod.ProductID equals psm.ProductId
                                                       where psm.IsActive == true && psm.IsDelete == false
                                                       orderby pod.LineNo
                                                       select pod).ToList();
                    if (det != null)
                    {
                        return det;
                    }
                    else
                        return null;
                }
                else
                {
                    List<PurchaseDetail> det = new List<PurchaseDetail>();
                    return det;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }




        public List<InvRequestNotePOTransaction> GetRequestNoteDetailsforGRN(long POHeaderID, int locationID)
        {
            try
            {
               // var RequestNotePOTransaction = _unitofwork.InvRequestNotePOTransaction.Get(r => r.PurchaseOrderHeaderID == POHeaderID && r.FromLocationID == locationID).ToList();


                var RequestNotePOTransaction = (from pod in _unitofwork.InvRequestNotePOTransaction.Get(r => r.PurchaseOrderHeaderID == POHeaderID && r.FromLocationID == locationID)
                                                   join psm in _unitofwork.ProductRepository.Get()
                                                   on pod.ProductID equals psm.ProductId
                                                   where psm.IsActive == true && psm.IsDelete == false
                                                   
                                                   select pod).ToList();


                return RequestNotePOTransaction;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<InvRequestNotePOTransaction> GetInvRequestNotePOTransactionbyID(long POHeaderID)
        {
            try
            {
                var RequestNotePOTransaction = _unitofwork.InvRequestNotePOTransaction.Get(r => r.PurchaseOrderHeaderID == POHeaderID ).ToList();

                return RequestNotePOTransaction;

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public List<VMInvRequestNotePOTransactions> GetRequestNoteDetailsforPO(long POHeaderID)
        {
            try
            {



                var ReqItems = (from pot in _unitofwork.InvRequestNotePOTransaction.Get()
                                join rh in _unitofwork.RequestNoteHeaderRepository.Get() on pot.RequestNoteHeaderID equals rh.RequestnoteHeaderId
                                join p in _unitofwork.ProductRepository.Get() on pot.ProductID equals p.ProductId
                                join l in _unitofwork.LocationRepository.Get() on pot.FromLocationID equals l.SysLocationID
                                join PD in _unitofwork.PurchaseOrderDetailRepository.Get() on pot.PurchaseOrderHeaderID equals PD.PurchaseOrderHeaderId

                                where pot.ProductID == PD.ProductId && pot.PurchaseOrderHeaderID == POHeaderID && p.IsActive == true && p.IsDelete == false
                                select new
                                {
                                    rh.DocumentDate,
                                    rh.DocumentNo,
                                    l.LocationCode,
                                    l.LocationName,
                                    p.ProductCode,
                                    p.ProductName,
                                    pot.QTY,
                                    pot.IssueQtY,
                                    pot.BalanceQtY,
                                    rh.Remark
                                }).ToList();


                // rh.Remark
                List<VMInvRequestNotePOTransactions> products = new List<VMInvRequestNotePOTransactions>();
                foreach (var p in ReqItems)
                {
                    VMInvRequestNotePOTransactions prd = new VMInvRequestNotePOTransactions();
                    prd.RequestNoteDate = p.DocumentDate;
                    prd.RequestNoteNo = p.DocumentNo;
                    prd.ReqLocationCode = p.LocationCode;
                    prd.ReqLocationName = p.LocationName;
                    prd.ProductCode = p.ProductCode;
                    prd.ProductDesp = p.ProductName;
                    prd.RequestedQty = p.QTY;
                    prd.IssueQtY = p.IssueQtY;
                    prd.BalanceQtY = p.BalanceQtY;
                      prd.Remark = p.Remark;
                    //prd.Remark = "";
                    products.Add(prd);
                }
                      

                return products;

            }
            catch (Exception ex)
            {

                throw;
            }
        }





        public IEnumerable<RequestNoteAcceptanceDetail> GetRequestNoteData(long id)
        {
            try
            {
                IEnumerable<RequestNoteAcceptanceDetail> det = _unitofwork.RequestNoteAccptanceDetailRepository.Get(r =>
                                                            r.RequestNoteAccptanceHeaderId == id).OrderBy(g => g.LineNo);
                if (det != null)
                {
                    return det;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseHeader> GetAllActiveGRNs(int companyid)
        {
            try
            {
                IEnumerable<PurchaseHeader> grn = _unitofwork.PurchaseHeaderRepository.Get(g => g.IsGRN == true && g.CompanyID== companyid).OrderBy(g => g.DocumentDate);
                return grn ?? null;
               
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseHeader> GetLocWiseGRNs(long locid,int companyid)
        {
            try
            {
                IEnumerable<PurchaseHeader> prndet = _unitofwork.PurchaseHeaderRepository.Get(p => p.GRNLocationId == locid && p.IsGRN == true && p.CompanyID== companyid && p.IsTOGTransfer== false && p.DocumentStatus== 3).OrderBy(p => p.PurchaseHeaderId);


                return prndet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

      


        public List<PurchaseHeader> GetGRNSummaryReport(long locid, long docid, DateTime from, DateTime to, int supplierId, int companyid)
        {
            try
            {
                DateTime frmdate = from.Date;
                DateTime todate = to.Date;

                var query = _unitofwork.PurchaseHeaderRepository.Get(
                    r => r.GRNDate >= frmdate &&
                         r.GRNDate <= todate &&
                         r.CompanyID == companyid);

                if (locid != 0)
                    query = query.Where(r => r.GRNLocationId == locid);

                if (docid != 0)
                    query = query.Where(r => r.PurchaseHeaderId == docid);

                if (supplierId != 0)
                    query = query.Where(r => r.SupplierID == supplierId);

                return query
                    .OrderBy(r => r.GRNLocationId)
                    .ThenBy(r => r.DocumentNo)
                    .ToList();
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
                IEnumerable<PurchaseHeader> docs = _unitofwork.PurchaseHeaderRepository.Get(e => e.GRNLocationId == locid && e.CompanyID== companyid && e.DocumentID==4)
                                                                                        .OrderBy(k => k.DocumentNo);

                int count = docs.Count();


                return docs ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        //public List<GRNDetailViewModel> GetGRNDetailReport(long locid, long docid, DateTime frmdate, DateTime todate, int supplierId)
        //{
        //    List<GRNDetailViewModel> reportdata = new List<GRNDetailViewModel>();
        //    List<PurchaseHeader> dbheader = new List<PurchaseHeader>();

        //    if (locid == 0 || docid == 0)
        //    {
        //        dbheader = _unitofwork.PurchaseHeaderRepository.Get(s => s.GRNDate >= frmdate.Date && s.GRNDate <= todate.Date && s.DocumentID==4
        //                                                       ).ToList();
        //    }
        //    else if (locid != 0 && docid != 0)
        //    {
        //        dbheader = _unitofwork.PurchaseHeaderRepository.Get(s => s.GRNLocationId == locid && s.PurchaseHeaderId == docid && s.DocumentID == 4
        //                                                       ).ToList();
        //    }
        //    else if (locid != 0 && docid == 0)
        //    {
        //        dbheader = _unitofwork.PurchaseHeaderRepository.Get(s => s.GRNLocationId == locid &&  s.DocumentID == 4 &&
        //                                                        s.GRNDate >= frmdate.Date && s.GRNDate <= todate.Date && s.IsGRN
        //                                                       ).ToList();
        //    }

        //    foreach (var header in dbheader)
        //    {
        //        GRNDetailViewModel vm = new GRNDetailViewModel();
        //        vm.Location = _blllocation.GetLocationById(header.GRNLocationId).LocationName;
        //        vm.DocumentDate = header.GRNDate.ToShortDateString();
        //        vm.DocumentNo = header.DocumentNo;
        //        vm.Remark = header.Remark;
        //        var status = _blldocstatus.GetDocStatusById(header.DocumentStatus);
        //        if(status!=null)
        //        vm.Status = status.Description;

        //        foreach (var s in _unitofwork.PurchaseDetailRepository.Get(r => r.PurchaseHeaderID == header.PurchaseHeaderId))
        //        {
        //            GRNDetailViewModel.ReportDetail det = new GRNDetailViewModel.ReportDetail();
        //            det.ProductId = s.ProductID;                    
        //            det.OrderQty = s.OrderQty;
        //            det.FreeQty = s.FreeQty;
        //            det.CostPrice = s.CostValue;
        //            det.SellingPrice = s.SellingPrice;
        //            var prd=_bllproduct.GetProductById(det.ProductId);
        //            if (prd != null)
        //            {
        //                det.ProductCode = prd.ProductCode;
        //                det.ProductName = prd.ProductName;

        //            }
        //            det.DiscountAmt = s.DiscountAmount;
        //            det.DiscountPrc = s.DiscountPercentage;
        //            vm.Detail.Add(det);

        //        }

        //        reportdata.Add(vm);
        //    }

        //    return reportdata;
        //}


        //Added by pavithra on 2019-12-05

        public List<GRNDetailViewModel> GetGRNDetailReport(long locid, long docid, DateTime frmdate, DateTime todate, int supplierId)
        {
            try
            {
                // Base query
                var query = _unitofwork.PurchaseHeaderRepository.Get(s =>
                    s.GRNDate >= frmdate.Date &&
                    s.GRNDate <= todate.Date &&
                    s.DocumentID == 4);

                // Apply filters conditionally
                if (locid != 0)
                    query = query.Where(s => s.GRNLocationId == locid);

                if (docid != 0)
                    query = query.Where(s => s.PurchaseHeaderId == docid);

                if (supplierId != 0)
                    query = query.Where(s => s.SupplierID == supplierId);

                // Fetch data
                var dbheader = query.ToList();

                var reportData = new List<GRNDetailViewModel>();

                foreach (var header in dbheader)
                {
                    var vm = new GRNDetailViewModel
                    {
                        Location = _blllocation.GetLocationById(header.GRNLocationId)?.LocationName,
                        DocumentDate = header.GRNDate.ToShortDateString(),
                        DocumentNo = header.DocumentNo,
                        Remark = header.Remark,
                        Status = _blldocstatus.GetDocStatusById(header.DocumentStatus)?.Description,
                        Detail = new List<GRNDetailViewModel.ReportDetail>()
                    };

                    // Get details for each header
                    var details = _unitofwork.PurchaseDetailRepository.Get(r => r.PurchaseHeaderID == header.PurchaseHeaderId).ToList();

                    foreach (var s in details)
                    {
                        var prd = _bllproduct.GetProductById(s.ProductID);

                        vm.Detail.Add(new GRNDetailViewModel.ReportDetail
                        {
                            ProductId = s.ProductID,
                            ProductCode = prd?.ProductCode,
                            ProductName = prd?.ProductName,
                            OrderQty = s.OrderQty,
                            FreeQty = s.FreeQty,
                            CostPrice = s.CostValue,
                            SellingPrice = s.SellingPrice,
                            DiscountAmt = s.DiscountAmount,
                            DiscountPrc = s.DiscountPercentage
                        });
                    }

                    reportData.Add(vm);
                }

                return reportData;
            }
            catch
            {
                throw;
            }
        }


        public IEnumerable<Product> GetProductsBySupllier(long SuppID,int companyid)
        {
            try
            {//load all items for selected supplier
                var sysproducts = (from p in _unitofwork.ProductRepository.Get()
                                   join pp in _unitofwork.SupplierProductRepository.Get() on p.ProductId equals pp.ProductId
                                   where p.IsActive == true && p.IsDelete == false && pp.SupplierId == SuppID 
                                   //&& p.IsRowMaterial == true
                                   && p.CompanyID== companyid && pp.CompanyID== companyid
                                   select new
                                   {
                                       ProductId = p.ProductId,
                                       ProductName = p.ProductName,
                                       ProductCode = p.ProductCode
                                   }).ToList();

                List<Product> products = new List<Product>();
                foreach (var p in sysproducts)
                {
                    Product prd = new Product();
                    prd.ProductId = p.ProductId;
                    prd.ProductCode = p.ProductCode;
                    prd.ProductName = p.ProductName;
                    products.Add(prd);
                }

                if (products != null)
                {
                    return products;
                }
                else
                    return null;


            }
            catch (Exception ex)
            {

                throw;
            }
        }

        //Added by pavithra on 2019-12-10
        public IEnumerable<Product> GetLocationWiseProductDetailsByID(int itemID, int locationID,int companyid)
        {
            try
            {
                var sysproducts = (from p in _unitofwork.ProductRepository.Get()
                                   where p.IsActive == true && p.IsDelete == false && p.ProductId == itemID 
                                   && p.CompanyID== companyid
                                   //&& p.LocationId == locationID
                                   select new
                                   {
                                       p.ProductId,
                                       p.ProductName,
                                       p.ProductCode,
                                       p.MaximumDiscount,
                                       p.MaximumDiscountPercentage,
                                   }).ToList();

                List<Product> products = new List<Product>();
                foreach (var p in sysproducts)
                {
                    Product prd = new Product();
                    prd.ProductId = p.ProductId;
                    prd.ProductCode = p.ProductCode;
                    prd.ProductName = p.ProductName;
                    prd.MaximumDiscount = p.MaximumDiscount;
                    prd.MaximumDiscountPercentage = p.MaximumDiscountPercentage;
                    products.Add(prd);
                }

                if (products != null)
                {
                    return products;
                }
                else
                    return null;


            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseHeader> GetSupWiseGRNs(long supID,int companyid)
        {
            try
            {
                IEnumerable<PurchaseHeader> prndet = _unitofwork.PurchaseHeaderRepository.Get(p => p.SupplierID == supID && p.IsGRN == true && p.CompanyID== companyid).OrderBy(p => p.PurchaseHeaderId);


                return prndet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }



        public List<dynamic> GetSupWiseGRNsDropDown(long supID, int companyid)
        {
            try
            {
                // Fetch the data and project it into a dynamic type
                var prndet = _unitofwork.PurchaseHeaderRepository
                    .Get(p => p.SupplierID == supID && p.IsGRN == true && p.CompanyID == companyid)
                    .OrderBy(p => p.PurchaseHeaderId)
                    .Select(p => new { p.PurchaseHeaderId, p.DocumentNo,p.DocumentStatus,p.IsPRN,p.IsTempGRN })
                    .Where(g => g.DocumentStatus == 3 && g.IsPRN == false && g.IsTempGRN == false)
                    .ToList()
                    .Cast<dynamic>()  // Explicitly cast each item to dynamic
                    .ToList();

                return prndet;
            }
            catch (Exception)
            {
                // Handle or log the error
                throw;
            }
        }
    }
}
