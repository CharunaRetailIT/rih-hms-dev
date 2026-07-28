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
using System.Data.Entity;

namespace HospitalityManagement.Service.Transactions
{
    public class PurchaseOrderService
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
        public IEnumerable<ProductStockMaster> GetLocationProducts(long locid)
        {
            try
            {
                IEnumerable<ProductStockMaster> locproducts = context.ProductStockMaster.Where(p => p.LocationId == locid && p.IsActive==true && p.IsDelete==false);
                if (locproducts != null)
                {
                    return locproducts;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ProductStockMaster> GetLocationProductNames(long locid)
        {
            try
            {
                //IEnumerable<ProductStockMaster> locproducts = context.ProductStockMaster.Where(p => p.LocationId == locid && p.IsActive == true && p.IsDelete == false);
                //if (locproducts != null)
                //{
                //    return locproducts;
                //}
                //else
                //    return null;

                //var sysproducts = context.ProductStockMaster.Select(p => new { p.ProductId, p.ProductCode, p.ProductName, p.IsActive, p.IsDelete,p.LocationId}).Where(g => g.IsDelete == false &&
                //                                                            g.IsActive == true && g.IsDelete == false && g.LocationId==locid ).OrderBy(g => g.ProductCode);


                var sysproducts = (from p in context.ProductStockMaster                            
                            join pp in context.Product on p.ProductId equals pp.ProductId
                            join u in context.UnitOfMeasure on
                            pp.PurchasingUnit equals u.UnitOfMeasureId
                            where p.LocationId == locid && p.IsActive == true && p.IsDelete == false
                            select new
                            {
                                ProductId = p.ProductId,
                                ProductName = p.ProductName,
                                ProductCode=p.ProductCode,
                                UOM=u.UnitOfMeasureName
                            }).ToList();

                List<ProductStockMaster> products = new List<ProductStockMaster>();
                foreach (var p in sysproducts)
                {
                    ProductStockMaster prd = new ProductStockMaster();
                    prd.ProductId = p.ProductId;
                    prd.ProductCode = p.ProductCode;
                    prd.ProductName = p.ProductName;
                    prd.UOMDesc = p.UOM;
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
        public IEnumerable<PaymentTerm> GetActivePaymentterms()
        {
            try
            {
                IEnumerable<PaymentTerm> paymentterms = context.PaymentTerm.OrderBy(g => g.PaymentTermCode);
                if (paymentterms != null)
                {
                    return paymentterms;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<PurchaseOrderHeader> GetAllPos()
        {
            try
            {
              //  IEnumerable<PurchaseOrderHeader> pos = context.PurchaseOrderHeader.Where(p=>p.IsGRN==false).OrderByDescending(g => g.DocumentDate);
                IEnumerable<PurchaseOrderHeader> pos = context.PurchaseOrderHeader.OrderByDescending(g => g.DocumentDate);
                if (pos != null)
                {
                    return pos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<PurchaseOrderHeader> GetAllTempPos()
        {
            try
            {
                IEnumerable<PurchaseOrderHeader> pos = context.PurchaseOrderHeader.Where(p => p.IsTempPO == true).OrderBy(g => g.DocumentDate);
                if (pos != null)
                {
                    return pos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<PurchaseOrderDetail> GetAllPOsById(long id)
        {
            try
            {
                IEnumerable<PurchaseOrderDetail> pos = context.PurchaseOrderDetail.OrderBy(g =>g.PurchaseOrderHeaderId==id);
                if (pos != null)
                {
                    return pos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public PurchaseOrderHeader GetPOById(long id)
        {
            try
            {
                var po = context.PurchaseOrderHeader.FirstOrDefault(g => g.PurchaseOrderHeaderId == id);
                return po ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public IEnumerable<PurchaseOrderDetail> GetPODetById(long id)
        {
            try
            {
                IEnumerable<PurchaseOrderDetail> podet = context.PurchaseOrderDetail.Where(p=>p.PurchaseOrderHeaderId==id).OrderBy(g => g.LineNo);
                return podet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public bool CheckProductTaxes(long id)
        {
            bool IsTaxes = context.ProductTax.Any(pt => pt.ProductId==id);

            return IsTaxes;

        }

        public ProductStockMaster GetProductDetails(long prodid, long polocid)
        {
            ProductStockMaster detailslist = new ProductStockMaster();
            using (ApplicationDbContext db = new ApplicationDbContext())
            {

                if (db.ProductStockMaster.Any(ps => ps.ProductId == prodid && ps.LocationId == polocid))
                {
                    detailslist = db.ProductStockMaster.Where(p => p.ProductId == prodid && p.LocationId == polocid).First();

                    return detailslist;
                }
                else
                {
                    return detailslist;                   
                }
            }

           
        }
        public List<POViewModel> GetProductTaxes1(long prodid, long polocid)
        {
            List<POViewModel> povwmodel = new List<POViewModel>();

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                if (CheckProductTaxes(prodid))
                {


                    var taxproduct = (
                            from p in db.Product
                            join pt in db.ProductTax on p.ProductId equals pt.ProductId
                            join ps in db.ProductStockMaster on p.ProductId equals ps.ProductId
                            join tx in db.Taxes on pt.TaxId equals tx.TaxId

                            where p.ProductId == prodid && ps.LocationId == polocid
                            orderby pt.ProductTaxId
                            select new
                            {
                                ProductId = p.ProductId,
                                Cost = ps.CostPrice,
                                Selling = ps.SellingPrice,
                                Discounts = ps.DiscountPrc,
                                TaxPrc = tx.TaxPercentage,
                                TaxAmt = p.SellingPrice * (tx.TaxPercentage / 100),
                                newselling = (p.SellingPrice + (p.SellingPrice * (tx.TaxPercentage / 100))),
                                TaxId = tx.TaxId,
                                TaxOnTax = tx.IsTaxOnTax
                            }
                        ).ToList();


                    foreach (var tax in taxproduct)
                    {
                        var vvm = new POViewModel();
                        vvm.ItemId = tax.ProductId;
                        vvm.SellingPrice = tax.Selling;
                        vvm.CostPrice = tax.Cost;
                        vvm.DiscountPrc = tax.Discounts;
                        vvm.TaxPrc = decimal.Round(tax.TaxPrc, 2, MidpointRounding.AwayFromZero);
                        vvm.TaxAmount = decimal.Round(tax.TaxAmt, 2, MidpointRounding.AwayFromZero);
                        vvm.IsTaxOnTax = tax.TaxOnTax;
                        vvm.TaxId = tax.TaxId;
                        povwmodel.Add(vvm);
                    }
                }
                else
                {
                    var product = (
                            from ps in db.ProductStockMaster
                            where ps.ProductId == prodid && ps.LocationId == polocid
                            //from p in db.Product
                            //where p.ProductId == prodid
                            //orderby p.ProductId

                            select new
                            {
                                ProductId = ps.ProductId,
                                Cost = ps.CostPrice,
                                Selling = ps.SellingPrice,
                                Discounts = ps.DiscountPrc,
                                TaxPrc = 0,
                                TaxAmt = 0,
                                newselling = 0,
                                TaxId = 0,
                                TaxOnTax = false
                            }
                        ).ToList();


                    foreach (var prd in product)
                    {
                        var vvm = new POViewModel
                        {
                            ItemId = prd.ProductId,
                            SellingPrice = prd.Selling,
                            CostPrice = prd.Cost,
                            TaxPrc = 0,
                            TaxAmount = 0,
                            IsTaxOnTax = false,
                            TaxId = 0
                        };
                        povwmodel.Add(vvm);
                    }



                }

                return povwmodel;

            }

        }

        public List<POViewModel> GetProductTaxes(long prodid,long polocid)
        {
            List<POViewModel> povwmodel=new List<POViewModel>();

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                if (CheckProductTaxes(prodid))
                {


                    var taxproduct = (
                            from p in db.Product
                            join pt in db.ProductTax on p.ProductId equals pt.ProductId
                            join ps in db.ProductStockMaster on p.ProductId equals ps.ProductId
                            join tx in db.Taxes on pt.TaxId equals tx.TaxId
                            
                            where p.ProductId == prodid && ps.LocationId==polocid
                            orderby pt.ProductTaxId
                            select new
                            {
                                ProductId = p.ProductId,
                                Cost = ps.CostPrice,
                                Selling = ps.SellingPrice,
                                Discounts = ps.DiscountPrc,
                                TaxPrc = tx.TaxPercentage,
                                TaxAmt = p.SellingPrice * (tx.TaxPercentage / 100),
                                newselling = (p.SellingPrice + (p.SellingPrice * (tx.TaxPercentage / 100))),
                                TaxId = tx.TaxId,
                                TaxOnTax = tx.IsTaxOnTax
                            }
                        ).ToList();


                    foreach (var tax in taxproduct)
                    {
                        var vvm = new POViewModel();
                        vvm.ItemId = tax.ProductId;
                        vvm.SellingPrice = tax.Selling;
                        vvm.CostPrice = tax.Cost;
                        vvm.DiscountPrc = tax.Discounts;
                        vvm.TaxPrc = decimal.Round(tax.TaxPrc, 2, MidpointRounding.AwayFromZero);
                        vvm.TaxAmount = decimal.Round(tax.TaxAmt, 2, MidpointRounding.AwayFromZero);
                        vvm.IsTaxOnTax = tax.TaxOnTax;
                        vvm.TaxId = tax.TaxId;
                        povwmodel.Add(vvm);
                    }
                }
                else
                {
                    var product = (
                            from ps in db.ProductStockMaster
                            where ps.ProductId==prodid && ps.LocationId==polocid
                            //from p in db.Product
                            //where p.ProductId == prodid
                            //orderby p.ProductId

                            select new
                            {
                                ProductId = ps.ProductId,
                                Cost = ps.CostPrice,
                                Selling = ps.SellingPrice,
                                Discounts = ps.DiscountPrc,
                                TaxPrc = 0,
                                TaxAmt = 0,
                                newselling = 0,
                                TaxId = 0,
                                TaxOnTax =false
                            }
                        ).ToList();


                    foreach (var prd in product)
                    {
                        var vvm = new POViewModel
                        {
                            ItemId = prd.ProductId,
                            SellingPrice = prd.Selling,
                            CostPrice = prd.Cost,
                            TaxPrc = 0 ,
                            TaxAmount = 0 ,
                            IsTaxOnTax = false,
                            TaxId = 0
                        };
                        povwmodel.Add(vvm);
                    }



                }

                return povwmodel;

            }
            
        }

        public List<POViewModel> GetReOrderLevelExceededProductBySupplierId1(long supplierid, long locid)
        {
            List<POViewModel> supqtylist = new List<POViewModel>();

            using (ApplicationDbContext db = new ApplicationDbContext())
            {

                var supproduct = (
                                from pp in db.SupplierProduct
                                join ps in db.ProductStockMaster on pp.ProductId equals ps.ProductId
                                join p in db.Product on pp.ProductId equals p.ProductId
                                where ps.Stock <= ps.ReOrderLevel && ps.ReOrderQuantity!=0
                                && ps.LocationId == locid
                                && pp.SupplierId == supplierid 
                                orderby ps.ProductId
                                select new
                                {
                                    ProductId = pp.ProductId,
                                    SupplierId = pp.SupplierId,
                                    CostPrice = ps.CostPrice,
                                    SellingPrice = ps.SellingPrice,
                                    ReOrderQty = ps.ReOrderQuantity,
                                    ProductName = p.ProductName
                                }
                            ).ToList();

                foreach (var sup in supproduct)
                {
                    var vm = new POViewModel();
                    vm.SupplierId = sup.SupplierId;
                    vm.CostPrice = sup.CostPrice;
                    vm.SellingPrice = sup.SellingPrice;
                    vm.ReOrderQty = sup.ReOrderQty;
                    vm.ItemId = sup.ProductId;
                    vm.ItemDesc = sup.ProductName;
                    supqtylist.Add(vm);
                }

            }

            return supqtylist;
        }

        public List<POViewModel> GetReOrderLevelExceededProductBySupplierId(long supplierid,long locid)
        {
            List<POViewModel> supqtylist = new List<POViewModel>();

            using (ApplicationDbContext db = new ApplicationDbContext())
            {

                var supproduct = (
                                from pp in db.SupplierProduct
                                join ps in db.ProductStockMaster on pp.ProductId equals ps.ProductId
                                join p in db.Product on pp.ProductId equals p.ProductId
                                where pp.SupplierId == supplierid && ps.Stock <= ps.ReOrderLevel
                                && ps.LocationId==locid
                                orderby ps.ProductId
                                select new
                                {
                                    ProductId = pp.ProductId,
                                    SupplierId = pp.SupplierId,
                                    ReOrderQty = ps.ReOrderQuantity,
                                    ProductName = p.ProductName
                                }
                            ).ToList();

                foreach (var sup in supproduct)
                {
                    var vm = new POViewModel();
                    vm.SupplierId = sup.SupplierId;
                    vm.ReOrderQty = sup.ReOrderQty;
                    vm.ItemId = sup.ProductId;
                    vm.ItemDesc = sup.ProductName;
                    supqtylist.Add(vm);
                }
              
            }

            return supqtylist;
        }

        public List<POViewModel> GetProductTaxesBySupplierProductId(long supplierproductid,long polocationid)
        {
            List<POViewModel> povwmodel = new List<POViewModel>();

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
               
                    if (CheckProductTaxes(supplierproductid))
                    {


                        var taxproduct = (
                                from p in db.Product
                                join pt in db.ProductTax on p.ProductId equals pt.ProductId
                                join ps in db.ProductStockMaster on p.ProductId equals ps.ProductId
                                join tx in db.Taxes on pt.TaxId equals tx.TaxId

                                where p.ProductId == supplierproductid && ps.Stock <= ps.ReOrderLevel
                                && ps.LocationId==polocationid
                                orderby pt.ProductTaxId
                                select new
                                {
                                    ProductId = p.ProductId,
                                    Cost = ps.CostPrice,
                                    Selling = ps.SellingPrice,
                                    Discounts = ps.DiscountPrc,
                                    TaxPrc = tx.TaxPercentage,
                                    TaxAmt = p.SellingPrice * (tx.TaxPercentage / 100),
                                    newselling = (p.SellingPrice + (p.SellingPrice * (tx.TaxPercentage / 100))),
                                    TaxId = tx.TaxId,
                                    TaxOnTax = tx.IsTaxOnTax,
                                    ReOrderQty = ps.ReOrderQuantity
                                }
                            ).ToList();


                        foreach (var tax in taxproduct)
                        {
                            var vvm = new POViewModel();
                            vvm.ItemId = tax.ProductId;
                            vvm.SellingPrice = tax.Selling;
                            vvm.CostPrice = tax.Cost;
                            vvm.DiscountPrc = tax.Discounts;
                            vvm.TaxPrc = decimal.Round(tax.TaxPrc, 2, MidpointRounding.AwayFromZero);
                            vvm.TaxAmount = decimal.Round(tax.TaxAmt, 2, MidpointRounding.AwayFromZero);
                            vvm.IsTaxOnTax = tax.TaxOnTax;
                            vvm.TaxId = tax.TaxId;
                            vvm.ReOrderQty = tax.ReOrderQty;
                            povwmodel.Add(vvm);
                        }
                    }
                    else
                    {
                        var product = (from p in db.Product
                                       join ps in db.ProductStockMaster on p.ProductId equals ps.ProductId
                                       where p.ProductId == supplierproductid && ps.Stock <= ps.ReOrderLevel
                                       orderby p.ProductId
                                       select new
                                       {
                                           ProductId = p.ProductId,
                                           Cost = ps.CostPrice,
                                           Selling = ps.SellingPrice,
                                           Discounts = p.DepartmentId,
                                           TaxPrc = 0,
                                           TaxAmt = 0,
                                           newselling = 0,
                                           TaxId = 0,
                                           TaxOnTax = false,
                                           ReOrderQty = ps.ReOrderQuantity
                                       }
                            ).ToList();


                        foreach (var prdt in product)
                        {
                            var vvm = new POViewModel
                            {
                                ItemId = prdt.ProductId,
                                SellingPrice = prdt.Selling,
                                CostPrice = prdt.Cost,
                                TaxPrc = 0,
                                TaxAmount = 0,
                                IsTaxOnTax = false,
                                TaxId = 0,
                                ReOrderQty=prdt.ReOrderQty
                            };
                            povwmodel.Add(vvm);
                        }



                    }
                 

                return povwmodel;
            }

        }

        public bool SavePurchaseOrder(PurchaseOrderHeader poheader)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {
                    context.PurchaseOrderHeader.Add(poheader);
                  
                    if (context.SaveChanges() == 1)
                    {
                        
                        int idx = 1;
                        foreach (var detail in poheader.PODetail)
                        {
                            var podet = context.ProductStockMaster.Where(s=>s.ProductId==detail.ProductId && s.LocationId==poheader.POLocationId);
                            
                            detail.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                            if (podet.First().StockCode == null)
                            {
                                detail.StockCode = "1";
                            }
                            else
                            {
                                detail.StockCode = podet.First().StockCode;
                            }
                           
                           
                            detail.LineNo = idx;
                            idx += 1;
                            context.PurchaseOrderDetail.Add(detail);
                            
                        }

                        if (context.SaveChanges() != poheader.PODetail.Count())
                        {
                            dbtransaction.Rollback();
                            return false;
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

        public long DeletePODetail(long poheaderid)
        {
            var res = 0;
            try
            {
                context.PurchaseOrderDetail.RemoveRange(context.PurchaseOrderDetail.Where(x => x.PurchaseOrderHeaderId == poheaderid));
                res  = context.SaveChanges();
              

            }
            catch (Exception)
            {

                throw; 
            }
            return res;
        }
        public bool EditPo(PurchaseOrderHeader poheader)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {
                   
                    if (context.SaveChanges() == 1)
                    {
                        DeletePODetail(poheader.PurchaseOrderHeaderId);
                        int idx = 1;
                        foreach (var detail in poheader.PODetail)
                        {
                            var podet = context.ProductStockMaster.Where(s => s.ProductId == detail.ProductId && s.LocationId ==poheader.POLocationId);

                            detail.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                            if (podet.First().StockCode == null)
                            {
                                detail.StockCode = "1";
                            }
                            else
                            {
                                detail.StockCode = podet.First().StockCode;
                            }


                            detail.LineNo = idx;
                            idx += 1;

                            context.PurchaseOrderDetail.Add(detail);
                           

                        }

                       // context.Entry(accountInDb).CurrentValues.SetValues(grnheader);
                        if (context.SaveChanges() == 0)
                        {
                            dbtransaction.Rollback();
                            return false;
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

        public List<PurchaseOrderHeader> GetPOSummaryReport(long locid, long docid, DateTime from, DateTime to)
        {
            try
            {
                List<PurchaseOrderHeader> purchaseorderheader = new List<PurchaseOrderHeader>();

                if (locid != 0 && docid != 0)
                {
                    purchaseorderheader = context.PurchaseOrderHeader.Where(r => r.PurchaseOrderHeaderId == docid && r.DeliveryLocationId == locid).
                                                              OrderBy(c => c.DocumentNo).OrderBy(d => d.DeliveryLocationId).ToList();
                }
                else if (locid != 0 && docid == 0)
                {
                    purchaseorderheader = context.PurchaseOrderHeader.Where(r => r.DeliveryLocationId == locid).
                                                               OrderBy(c => c.DocumentNo).OrderBy(d => d.DeliveryLocationId).ToList();
                }
                else if (locid == 0 && docid != 0)
                {
                    purchaseorderheader = context.PurchaseOrderHeader.Where(r => r.PurchaseOrderHeaderId == docid).
                                                              OrderBy(c => c.DocumentNo).OrderBy(d => d.DeliveryLocationId).ToList();
                }
                else if (locid == 0 && docid == 0)
                {
                    //purchaseorderheader = context.PurchaseOrderHeader.
                    //                                           OrderBy(c => c.DocumentNo).OrderBy(d => d.DeliveryLocationId).ToList();

                    DateTime frmdate = from.Date;
                    DateTime todate = to.Date;
                  

                    purchaseorderheader = context.PurchaseOrderHeader.Where(r => r.DocumentDate >= frmdate && r.DocumentDate <= todate
                        ).
                        OrderBy(c => c.DocumentNo).OrderBy(d => d.POLocationId).ToList();
                   
                }

                if (purchaseorderheader != null)
                {
                    return purchaseorderheader;
                }

                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        //public List<PurchaseOrderDetail> GetPODetailReport(long pohid, long productid)
        //{
        //    try
        //    {
        //        List<PurchaseOrderDetail> purchaseorderdetail = new List<PurchaseOrderDetail>();

        //        if (pohid != 0)
        //        {
        //            purchaseorderdetail = context.PurchaseOrderDetail.Where(r => r.PurchaseOrderHeaderId == pohid).
        //                                                      OrderBy(c => c.ProductId).OrderBy(d => d.PurchaseOrderHeaderId).ToList();
        //        }
                
        //        else if (pohid == 0)
        //        {
        //            purchaseorderdetail = context.PurchaseOrderDetail.Where(r => r.PurchaseOrderHeaderId == pohid).
        //                                                      OrderBy(c => c.ProductId).OrderBy(d => d.PurchaseOrderHeaderId).ToList();
        //        }
                

        //        if (purchaseorderdetail != null)
        //        {
        //            return purchaseorderdetail;
        //        }

        //        else
        //            return null;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}

        public IEnumerable<PurchaseOrderHeader> GetDocNoByLocId(long locid)
        {
            try
            {
                IEnumerable<PurchaseOrderHeader> docs = context.PurchaseOrderHeader.Where(e => e.POLocationId == locid)
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
        public List<PODetailViewModel> GetPODetailReport(long locid, long docid, DateTime frmdate, DateTime todate)
        {
            List<PODetailViewModel> reportdata = new List<PODetailViewModel>();
            List<PurchaseOrderHeader> dbheader = new List<PurchaseOrderHeader>();

            if (locid == 0 || docid == 0)
            {
                dbheader = context.PurchaseOrderHeader.Where(s => s.CreatedDate >= frmdate.Date && s.CreatedDate <= todate.Date 
                                                               ).ToList();
            }
            else if (locid != 0 && docid != 0)
            {
                dbheader = context.PurchaseOrderHeader.Where(s => s.POLocationId == locid && s.PurchaseOrderHeaderId == docid 
                                                               ).ToList();
            }
            else if (locid != 0 && docid == 0)
            {
                dbheader = context.PurchaseOrderHeader.Where(s => s.POLocationId == locid &&
                                                                s.CreatedDate >= frmdate.Date && s.CreatedDate <= todate.Date 
                                                               ).ToList();
            }

            foreach (var header in dbheader)
            {
                PODetailViewModel vm = new PODetailViewModel();
                vm.Location = _locservice.GetLocationById(header.POLocationId).LocationName;
                vm.DocumentDate = header.CreatedDate.ToShortDateString();
                vm.DocumentNo = header.DocumentNo;
                vm.Remark = header.Remark;

                foreach (var s in context.PurchaseOrderDetail.Where(r => r.PurchaseOrderHeaderId == header.PurchaseOrderHeaderId))
                {
                    PODetailViewModel.ReportDetail det = new PODetailViewModel.ReportDetail();
                    det.ProductId = s.ProductId;
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