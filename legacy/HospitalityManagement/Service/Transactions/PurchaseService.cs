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
    public class PurchaseService
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
        public IEnumerable<PurchaseHeader> GetAllGRNs()
        {
            try
            {
                IEnumerable<PurchaseHeader> grns = context.PurchaseHeader.Where(r=>r.IsTempGRN==true).OrderBy(g => g.DocumentDate);
                if (grns != null)
                {
                    return grns;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseHeader> GetAllTempGRNs()
        {
            try
            {
                IEnumerable<PurchaseHeader> grns = context.PurchaseHeader.Where(r => r.IsTempGRN == true && r.IsGRN==true).OrderBy(g => g.DocumentDate);
                if (grns != null)
                {
                    return grns;
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

        public PurchaseHeader GetGRNById(long id)
        {
            try
            {
                var grn = context.PurchaseHeader.FirstOrDefault(g => g.PurchaseHeaderId == id);
                return grn ?? null;
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
               
                IEnumerable<PurchaseDetail> grndet = context.PurchaseDetail.Where(p => p.PurchaseHeaderID == id).OrderBy(g => g.LineNo);
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

                IEnumerable<PurchaseDetail> grndet = context.PurchaseDetail.Where(p => p.PurchaseHeaderID == id
                    && p.DocumentDate >= frmdate && p.DocumentDate <= to).OrderBy(g => g.LineNo);
                return grndet ?? null;
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

        //public List<POViewModel> GetProductTaxes(long prodid)
        //{
        //    List<POViewModel> povwmodel = new List<POViewModel>();

        //    using (ApplicationDbContext db = new ApplicationDbContext())
        //    {
        //        if (CheckProductTaxes(prodid))
        //        {


        //            var taxproduct = (
        //                    from p in db.Product
        //                    join pt in db.ProductTax on p.ProductId equals pt.ProductId
        //                    join ps in db.ProductStockMaster on p.ProductId equals ps.ProductId
        //                    join tx in db.Taxes on pt.TaxId equals tx.TaxID

        //                    where p.ProductId == prodid
        //                    orderby pt.ProductTaxId
        //                    select new
        //                    {
        //                        ProductId = p.ProductId,
        //                        Cost = ps.CostPrice,
        //                        Selling = ps.SellingPrice,
        //                        Discounts = ps.DiscountPrc,
        //                        TaxPrc = tx.EffectivePercentage,
        //                        TaxAmt = p.SellingPrice * (tx.EffectivePercentage / 100),
        //                        newselling = (p.SellingPrice + (p.SellingPrice * (tx.EffectivePercentage / 100))),
        //                        TaxId = tx.TaxID,
        //                        TaxOnTax = tx.IsTaxOnTax
        //                    }
        //                ).ToList();


        //            foreach (var tax in taxproduct)
        //            {
        //                var vvm = new POViewModel();
        //                vvm.ItemId = tax.ProductId;
        //                vvm.SellingPrice = tax.Selling;
        //                vvm.CostPrice = tax.Cost;
        //                vvm.DiscountPrc = tax.Discounts;
        //                vvm.TaxPrc = decimal.Round(tax.TaxPrc, 2, MidpointRounding.AwayFromZero);
        //                vvm.TaxAmount = decimal.Round(tax.TaxAmt, 2, MidpointRounding.AwayFromZero);
        //                vvm.IsTaxOnTax = tax.TaxOnTax;
        //                vvm.TaxId = tax.TaxId;
        //                povwmodel.Add(vvm);
        //            }
        //        }
        //        else
        //        {
        //            var product = (from p in db.Product

        //                           where p.ProductId == prodid
        //                           orderby p.ProductId
        //                           select new
        //                           {
        //                               ProductId = p.ProductId,
        //                               Cost = p.CostPrice,
        //                               Selling = p.SellingPrice,
        //                               Discounts = p.DepartmentId,
        //                               TaxPrc = 0,
        //                               TaxAmt = 0,
        //                               newselling = 0,
        //                               TaxId = 0,
        //                               TaxOnTax = false
        //                           }
        //                ).ToList();


        //            foreach (var prd in product)
        //            {
        //                var vvm = new POViewModel
        //                {
        //                    ItemId = prd.ProductId,
        //                    SellingPrice = prd.Selling,
        //                    CostPrice = prd.Cost,
        //                    TaxPrc = 0,
        //                    TaxAmount = 0,
        //                    IsTaxOnTax = false,
        //                    TaxId = 0
        //                };
        //                povwmodel.Add(vvm);
        //            }



        //        }

        //        return povwmodel;

        //    }

        //}


        private int UpdatePOStatus(long id)
        {
            try
            {
                PurchaseOrderService poService = new PurchaseOrderService();
                // PurchaseOrderHeader poheaher = new PurchaseOrderHeader();
                //poheaher = poService.GetPOById(docid);
                //poheaher.IsGRN = true;

                // var poheaher = new PurchaseOrderHeader() { IsGRN = true };
                var dbpoheader = context.PurchaseOrderHeader.Find(id);
                //context.PurchaseOrderHeader.Attach(poheaher);
                dbpoheader.IsGRN = true;
                dbpoheader.IsTempPO = false;
                //  context.Entry(poheaher).Property(x => x.IsGRN).IsModified = true;


                var sss = context.SaveChanges();
                return sss;
            }
            catch (Exception e)
            {
                return 0;
            }

        }

        public bool SaveGRN (PurchaseHeader grnheader)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {
                 
                try
                {
                    if (grnheader.GRNType == "POBased")
                    {
                        UpdatePOStatus(grnheader.POID);
                    }

                    grnheader.CostCentreID = Convert.ToInt32(grnheader.GRNLocationId);
                    context.PurchaseHeader.Add(grnheader);

                    if (context.SaveChanges() == 1)
                    {
                       
                      //  int idx = 1;
                        foreach (var detail in grnheader.GRNDetail)
                        {
                            var productstockmaster = context.ProductStockMaster.Where(s => s.ProductId == detail.ProductID 
                                                                                        && s.LocationId == grnheader.GRNLocationId
                                                                                        ).FirstOrDefault();

                         

                            if (grnheader.GRNType == "POBased")
                            {
                                var dbpo = context.PurchaseOrderDetail.Where(p => p.PurchaseOrderHeaderId == grnheader.POID
                                                                         && p.ProductId == detail.ProductID).FirstOrDefault();
                                dbpo.GRNQuantity = detail.GRNQuantity + dbpo.GRNQuantity;
                                dbpo.BalanceQty = (dbpo.OrderQty - dbpo.GRNQuantity);
                            }
                          



                            // calculates avg cost
                            decimal avaragecost = 0;
                            if (detail.GRNQuantity != 0)
                            {
                                decimal newqty = detail.GRNQuantity;
                                decimal crrqty = productstockmaster.Stock;                              
                                decimal unitcost = detail.CostValue / detail.GRNQuantity;
                                decimal a = (newqty * unitcost);
                                decimal b = (productstockmaster.AvgCost * crrqty);
                                decimal c = (newqty + crrqty);
                                decimal d = a + b;
                                decimal avgcost = d / c;
                                detail.AvgCost = avgcost;
                                avaragecost = avgcost;
                            }
                            else
                            {
                                detail.AvgCost = 0;
                                avaragecost = 0;
                            }

                            detail.PurchaseHeaderID = grnheader.PurchaseHeaderId;
                            detail.StockCode = productstockmaster.StockCode;
                            detail.LineNo = grnheader.GRNDetail.IndexOf(detail)+1;
                          //  idx += 1;
                            detail.BatchNo = "0";
                            detail.ExpiryDate = DateTime.Now;
                            detail.SerialNo = "0";
                            detail.DocumentID = grnheader.DocumentID;
                            detail.DocumentNo = grnheader.DocumentNo;
                            detail.ProductRemark ="";
                            detail.DocumentDate = DateTime.Now;
                            detail.CostCentreID = Convert.ToInt32(grnheader.CostCentreID);
                            detail.DiscountAmount = detail.DiscountAmount;
                            detail.GrossAmount = detail.CostValue;
                            detail.NetAmount = (detail.CostValue - detail.DiscountAmount) + detail.TotalTax;

                            context.PurchaseDetail.Add(detail);


                            if (context.SaveChanges() == 1)
                            {
                                if (grnheader.IsTempGRN == false)
                                {

                                    //foreach (var ps in productstockmaster)
                                    //{
                                        if (productstockmaster.ProductId == detail.ProductID)
                                        {

                                            if (productstockmaster.Stock > 0)
                                            {
                                                PriceLevel pl = new PriceLevel();
                                                pl.ProductId = detail.ProductID;

                                                //pl.CostPrice = detail.CostPrice / detail.OrderQty;
                                                //pl.SellingPrice = detail.SellingPrice / detail.OrderQty;
                                                //pl.Qty = detail.OrderQty + detail.FreeQty;

                                                //pl.CostPrice = detail.CostPrice / detail.GRNQuantity;
                                                //pl.SellingPrice = detail.SellingPrice / detail.GRNQuantity;

                                                pl.CostPrice = detail.CostPrice;
                                                pl.SellingPrice = detail.SellingPrice;
                                                pl.Qty = detail.GRNQuantity + detail.FreeQty;

                                                pl.CreatedUser = grnheader.CreatedUser;

                                                pl.CreatedDate = DateTime.Now;
                                                pl.ModifiedDate = DateTime.Now;
                                                pl.LocationId = detail.CostCentreID;
                                                pl.DocumentId = Convert.ToInt32(detail.PurchaseHeaderID);

                                                context.PriceLevel.Add(pl);
                                                context.SaveChanges();

                                            }

                                           
                                                productstockmaster.AvgCost = avaragecost;

                                                //productstockmaster.Stock += detail.FreeQty + detail.OrderQty;
                                                //productstockmaster.SellingPrice = detail.SellingPrice / detail.OrderQty;
                                                //productstockmaster.CostPrice = detail.CostPrice / detail.OrderQty;

                                                productstockmaster.Stock += detail.FreeQty + detail.GRNQuantity;
                                                if (detail.GRNQuantity != 0)
                                                {
                                                    productstockmaster.SellingPrice = detail.SellingPrice / detail.GRNQuantity;
                                                    productstockmaster.CostPrice = detail.CostPrice;
                                                }
                                                else {
                                                    productstockmaster.SellingPrice = 0;
                                                    productstockmaster.CostPrice = 0;
                                                }
                                               
                                               
                                                /// detail.GRNQuantity;                                               
                                                productstockmaster.DocumentNo = detail.DocumentNo;
                                                productstockmaster.LastUpdatedDate = DateTime.Now;

                                                context.SaveChanges();

                                        }


                                                UpdateReceipes(avaragecost, detail.ProductID,grnheader.GRNLocationId);

                                  //  }

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

        private void UpdateReceipes(decimal avgcost, decimal materialid,long locationid)
        {

            var receipes = context.Receipe.Where(r => r.MaterialId == materialid && r.LocationId==locationid).ToList();

            if (receipes.Count>0)
            {
                foreach (var r in receipes)
                {
                    var productstockmaster = context.ProductStockMaster.Where(s => s.ProductId == r.MaterialId && 
                                                                                    s.LocationId == locationid).FirstOrDefault();

                    r.CostPrice = r.Quantity * productstockmaster.CostPrice;
                    r.SellingPrice = r.Quantity * productstockmaster.SellingPrice;

                   var servingunit = context.Receipe.Where(s => s.ProductServingUnitId == r.ProductServingUnitId).ToList();

                    var totcostonreceipe = servingunit.Sum(cost=>cost.CostPrice);
                    var totsellingonreceipe = servingunit.Sum(selling => selling.SellingPrice);

                    //var servingunittoupdate = context.ProductServingUnit.Where(s => s.ProductServingUnitId == r.ProductServingUnitId).FirstOrDefault();
                    //servingunittoupdate.CostPrice = totcostonreceipe;
                    //servingunittoupdate.SellingPrice = totsellingonreceipe;

                    context.SaveChanges();

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

        public bool UpdateGRN(PurchaseHeader grnheader)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            using (var dbtransaction = context.Database.BeginTransaction())
            {

                   try
                    {
                        var grnInDb = context.PurchaseHeader.Single(a => a.PurchaseHeaderId == grnheader.PurchaseHeaderId);
                        grnheader.CreatedUser = grnInDb.CreatedUser;
                        grnheader.CreatedDate = grnInDb.CreatedDate;
                        grnheader.DocumentID = grnInDb.DocumentID;
                        grnheader.CurrencyID = grnInDb.CurrencyID;
                        // Update the properties
                        context.Entry(grnInDb).CurrentValues.SetValues(grnheader);

                      if (context.SaveChanges() == 1)
                       {

                                     DeleteGRNDetail(grnheader.PurchaseHeaderId);
                                     int idx = 1;
                                    foreach (var detail in grnheader.GRNDetail)
                                    {
                                        var productstockmaster = context.ProductStockMaster.Where(s => s.ProductId == detail.ProductID 
                                                                                                        && s.LocationId == grnheader.GRNLocationId).ToList();


                                        var dbpo = context.PurchaseOrderDetail.Where(p => p.PurchaseOrderHeaderId == grnheader.POID
                                                                           && p.ProductId == detail.ProductID).FirstOrDefault();

                                        dbpo.GRNQuantity = detail.GRNQuantity + dbpo.GRNQuantity;
                                        dbpo.BalanceQty = (detail.OrderQty - detail.GRNQuantity);

                                        detail.PurchaseHeaderID = grnheader.PurchaseHeaderId;
                                        detail.StockCode = productstockmaster.First().StockCode;

                                        detail.LineNo = idx;
                                        idx += 1;
                                        detail.BatchNo = "0";
                                        detail.ExpiryDate = DateTime.Now;
                                        detail.SerialNo = "0";
                                        detail.DocumentID = grnheader.DocumentID;
                                        detail.DocumentNo = grnheader.DocumentNo;
                                        detail.ProductRemark = "";
                                        detail.DocumentDate = DateTime.Now;
                                        detail.CostCentreID = grnheader.LocationId;


                                        context.PurchaseDetail.Add(detail);

                                        if (context.SaveChanges() == 1)
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
                                                            pl.CostPrice = detail.CostPrice / detail.GRNQuantity;
                                                            pl.SellingPrice = detail.SellingPrice / detail.GRNQuantity;

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
                                                        context.PriceLevel.Add(pl);
                                                        context.SaveChanges();
                                                    }

                                                //ps.Stock += detail.FreeQty + detail.OrderQty;
                                                //ps.SellingPrice = detail.SellingPrice / detail.OrderQty;
                                                //ps.CostPrice = detail.CostPrice / detail.OrderQty;

                                                    ps.Stock += detail.FreeQty + detail.GRNQuantity;

                                                 
                                                    if (detail.GRNQuantity != 0)
                                                    {
                                                        ps.SellingPrice = detail.SellingPrice / detail.GRNQuantity;
                                                        ps.CostPrice = detail.CostPrice / detail.GRNQuantity;

                                                    }
                                                    else
                                                    {
                                                        ps.CostPrice = 0;
                                                        ps.SellingPrice = 0;

                                                    }

                                                    ps.DocumentNo = detail.DocumentNo;
                                                    ps.LastUpdatedDate = DateTime.Now;
                                                    context.SaveChanges();

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

        public IEnumerable<PurchaseHeader> GetGRNByLocId(long locid)
        {
            try
            {
                IEnumerable<PurchaseHeader> grn = context.PurchaseHeader.Where(p => p.GRNLocationId == locid).OrderBy(g => g.DocumentNo);
                return grn ?? null;
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
                IEnumerable<PurchaseDetail>  det= context.PurchaseDetail.Where(r => r.PurchaseHeaderID ==id ).OrderBy(g => g.LineNo);
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

        public IEnumerable<RequestNoteAcceptanceDetail> GetRequestNoteData(long id)
        {
            try
            {
                IEnumerable<RequestNoteAcceptanceDetail> det = context.RequestNoteAcceptanceDetail.Where(r => 
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

        public IEnumerable<PurchaseHeader> GetAllActiveGRNs()
        {
            try
            {
                IEnumerable<PurchaseHeader> grn = context.PurchaseHeader.Where(g => g.IsGRN == true).OrderBy(g => g.DocumentDate);

                if (grn != null)
                {
                    return grn;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<PurchaseHeader> GetLocWiseGRNs(long locid)
        {
            try
            {
                IEnumerable<PurchaseHeader> prndet = context.PurchaseHeader.Where(p => p.GRNLocationId == locid && p.IsGRN == true).OrderBy(p => p.PurchaseHeaderId);


                return prndet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<PurchaseHeader> GetGRNSummaryReport(long locid, long docid,DateTime from ,DateTime to)
        {
            try
            {
                List<PurchaseHeader> purchaseheader = new List<PurchaseHeader>();

                if (locid != 0 && docid != 0)
                {
                    purchaseheader = context.PurchaseHeader.Where(r => r.PurchaseHeaderId == docid && r.GRNLocationId == locid && r.IsGRN==true && r.IsTempGRN ==false ).
                                                              OrderBy(c => c.DocumentNo).OrderBy(d => d.GRNLocationId).ToList();
                }
                else if (locid != 0 && docid == 0)
                {
                    purchaseheader = context.PurchaseHeader.Where(r => r.GRNLocationId == locid && r.IsGRN == true && r.IsTempGRN == false).
                                                               OrderBy(c => c.DocumentNo).OrderBy(d => d.GRNLocationId).ToList();
                }
                else if (locid == 0 && docid != 0)
                {
                    purchaseheader = context.PurchaseHeader.Where(r => r.PurchaseHeaderId == docid && r.IsGRN == true && r.IsTempGRN == false).
                                                              OrderBy(c => c.DocumentNo).OrderBy(d => d.GRNLocationId).ToList();
                   
                }
                else if (locid == 0 && docid == 0)
                {
                    DateTime frmdate = from.Date;
                    DateTime todate = to.Date;

                    purchaseheader = context.PurchaseHeader.Where(r => r.IsGRN == true && r.IsTempGRN == false
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

        public IEnumerable<PurchaseHeader> GetDocNoByLocId(long locid)
        {
            try
            {
                IEnumerable<PurchaseHeader> docs = context.PurchaseHeader.Where(e => e.GRNLocationId == locid && e.IsGRN == true && e.IsTempGRN == false)
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

        public List<GRNDetailViewModel> GetGRNDetailReport(long locid, long docid, DateTime frmdate, DateTime todate)
        {
            List<GRNDetailViewModel> reportdata = new List<GRNDetailViewModel>();
            List<PurchaseHeader> dbheader = new List<PurchaseHeader>();

            if (locid == 0 || docid == 0)
            {
                dbheader = context.PurchaseHeader.Where(s => s.CreatedDate >= frmdate.Date && s.CreatedDate <= todate.Date && s.IsGRN == true && s.IsTempGRN == false
                                                               ).ToList();
            }
            else if (locid != 0 && docid != 0)
            {
                dbheader = context.PurchaseHeader.Where(s => s.GRNLocationId == locid && s.PurchaseHeaderId == docid && s.IsGRN == true && s.IsTempGRN == false
                                                               ).ToList();
            }
            else if (locid != 0 && docid == 0)
            {
                dbheader = context.PurchaseHeader.Where(s => s.GRNLocationId == locid &&
                                                                s.CreatedDate >= frmdate.Date && s.CreatedDate <= todate.Date && s.IsGRN == true && s.IsTempGRN == false
                                                               ).ToList();
            }

            foreach (var header in dbheader)
            {
                GRNDetailViewModel vm = new GRNDetailViewModel();
                vm.Location = _locservice.GetLocationById(header.GRNLocationId).LocationName;
                vm.DocumentDate = header.CreatedDate.ToShortDateString();
                vm.DocumentNo = header.DocumentNo;
                vm.Remark = header.Remark;

                foreach (var s in context.PurchaseDetail.Where(r => r.PurchaseHeaderID == header.PurchaseHeaderId))
                {
                    GRNDetailViewModel.ReportDetail det = new GRNDetailViewModel.ReportDetail();
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