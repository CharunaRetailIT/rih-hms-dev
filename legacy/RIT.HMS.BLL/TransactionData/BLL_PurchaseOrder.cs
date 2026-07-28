using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using RIT.HMS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.Reports;
using RIT.HMS.Domain.Transactions;
using System.Data.Entity;

namespace RIT.HMS.BLL.TransactionData
{
    public class BLL_PurchaseOrder
    {
        private readonly BLL_Location _blllocation;
        private readonly BLL_Product _bllproduct;
        private readonly UnitOfWork _unitofwork;
        private readonly BLL_DocStatus _bllstatus;
        public BLL_PurchaseOrder()
        {
            _blllocation = new BLL_Location();
            _bllproduct = new BLL_Product();
            _unitofwork = new UnitOfWork();
            _bllstatus = new BLL_DocStatus();

        }
        public BLL_PurchaseOrder(string connectionname)
        {
            _blllocation = new BLL_Location(connectionname);
            _bllproduct = new BLL_Product(connectionname);
            _unitofwork = new UnitOfWork(connectionname);
            _bllstatus = new BLL_DocStatus(connectionname);

        }

        public IEnumerable<PaymentMethod> GetActivePaymentMethods(Int32 compid)
        {
            try
            {
                IEnumerable<PaymentMethod> paymentmethods = _unitofwork.PaymentMethodRepository.Get(c => c.IsActive == true && c.CompanyID == compid).OrderBy(g => g.PaymentMethodName);
                //if (paymentmethods != null)
                //{
                //    return paymentmethods;
                //}
                //else
                //    return null;

                return paymentmethods == null ? null : paymentmethods;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<ProductStockMaster> GetLocationProducts(long locid, int companyid)
        {
            try
            {
                IEnumerable<ProductStockMaster> locproducts = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == locid && p.IsActive == true
                && p.IsDelete == false && p.CompanyID == companyid);
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
        public IEnumerable<ProductStockMaster> GetLocationProductNames(long locid, int companyid)
        {
            try
            {
                var sysproducts = (from p in _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == locid && p.IsActive == true && p.IsDelete == false && p.CompanyID == companyid)
                                   join pp in _unitofwork.ProductRepository.Get() on p.ProductId equals pp.ProductId
                                   join u in _unitofwork.UnitOfMeasureRepository.Get() on pp.PurchasingUnit equals u.UnitOfMeasureId
                                   where pp.IsActive==true && pp.IsDelete==false
                                   //  where p.LocationId == locid && p.IsActive == true && p.IsDelete == false && p.CompanyID==companyid
                                   select new
                                   {
                                       ProductId = p.ProductId,
                                       ProductName = p.ProductName,
                                       ProductCode = p.ProductCode,
                                       UOM = u.UnitOfMeasureName
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
        public IEnumerable<PaymentTerm> GetActivePaymentterms(Int32 compid)
        {
            try
            {
                IEnumerable<PaymentTerm> paymentterms = _unitofwork.PaymentTermRepository.Get(c => c.IsDelete == false && c.CompanyID == compid).OrderBy(g => g.PaymentTermCode);
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
        public IEnumerable<PurchaseOrderHeader> GetAllPos(int companyid)
        {
            try
            {
                IEnumerable<PurchaseOrderHeader> pos = _unitofwork.PurchaseOrderHeaderRepository.Get(p => p.CompanyID == companyid).OrderByDescending(g => g.DocumentDate);
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
        public IEnumerable<PurchaseOrderHeader> GetNewPos(int companyid)
        {
            try
            {

                IEnumerable<PurchaseOrderHeader> pos = _unitofwork.PurchaseOrderHeaderRepository.Get(p => p.IsGRN == false && p.IsTempPO == false && p.CompanyID == companyid).OrderByDescending(g => g.DocumentDate);
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

        public IEnumerable<PurchaseOrderHeader> GetAllPosCurrentMonth(int companyid)
        {
            try
            {
                // IEnumerable<PurchaseOrderHeader> pos = _unitofwork.PurchaseOrderHeaderRepository.Get(p => p.IsTempPO == true).OrderBy(g => g.DocumentDate);
                var today = DateTime.Today;
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

                IEnumerable<PurchaseOrderHeader> pos = _unitofwork.PurchaseOrderHeaderRepository
                    .Get(p => p.CompanyID == companyid
                              && p.DocumentDate >= firstDayOfMonth
                              && p.DocumentDate <= today)
                    .OrderByDescending(g => g.DocumentDate);
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


        public IEnumerable<PurchaseOrderHeader> GetAllPoswithDateRange(int companyid,DateTime FromDate,DateTime ToDate)
        {
            try
            {
                // IEnumerable<PurchaseOrderHeader> pos = _unitofwork.PurchaseOrderHeaderRepository.Get(p => p.IsTempPO == true).OrderBy(g => g.DocumentDate);
                //var today = DateTime.Today;
                //var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                DateTime toDatePlusOne = ToDate.AddDays(1);


                IEnumerable<PurchaseOrderHeader> pos = _unitofwork.PurchaseOrderHeaderRepository
                    .Get(p => p.CompanyID == companyid
                              && p.DocumentDate >= FromDate
                              && p.DocumentDate < toDatePlusOne)
                    .OrderByDescending(g => g.DocumentDate);
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








        public IEnumerable<PurchaseOrderHeader> GetAllTempPos(int companyid)
        {
            try
            {
                // IEnumerable<PurchaseOrderHeader> pos = _unitofwork.PurchaseOrderHeaderRepository.Get(p => p.IsTempPO == true).OrderBy(g => g.DocumentDate);
                IEnumerable<PurchaseOrderHeader> pos = _unitofwork.PurchaseOrderHeaderRepository.Get(p => p.CompanyID == companyid).OrderByDescending(g => g.DocumentDate);
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
        public IEnumerable<PurchaseOrderHeader> GetSavedPosBySupplierId(long id, int companyid)
        {
            try
            {
                IEnumerable<PurchaseOrderHeader> pos = _unitofwork.PurchaseOrderHeaderRepository.Get(p =>
                                                        p.IsTempPO == false && p.IsGRN == false && p.SupplierId == id && p.CompanyID == companyid)
                                                        .OrderBy(g => g.DocumentDate);
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
        public IEnumerable<PurchaseOrderHeader> GetSavedAllPosBySupplierId(long id, int companyid)
        {
            try
            {
                //IEnumerable<PurchaseOrderHeader> pos = _unitofwork.PurchaseOrderHeaderRepository.Get(p =>
                //                                        p.IsTempPO == false && p.SupplierId == id && p.CompanyID == companyid)
                //                                        .OrderBy(g => g.DocumentDate);

                var pos = from p in _unitofwork.PurchaseOrderHeaderRepository.Get() // or GetQueryable()
                          join pd in _unitofwork.PurchaseOrderDetailRepository.Get() // or GetQueryable()
                          on p.PurchaseOrderHeaderId equals pd.PurchaseOrderHeaderId
                          where p.SupplierId == id
                                && p.IsTempPO == false
                                && p.CompanyID == companyid
                                && pd.OrderQty > pd.GRNQuantity
                          orderby p.DocumentDate
                          select new
                          {
                              PurchaseOrderHeader = p,
                              PurchaseOrderDetail = pd
                          };

                //var purchaseOrderHeaders = pos.Select(result => result.PurchaseOrderHeader).ToList();
                var purchaseOrderHeaders = pos
    .Select(result => result.PurchaseOrderHeader)
    .Distinct() // This will work if Equals and GetHashCode are overridden
    .ToList();




                if (purchaseOrderHeaders != null)
                {
                    return purchaseOrderHeaders;
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
                IEnumerable<PurchaseOrderDetail> pos = _unitofwork.PurchaseOrderDetailRepository.Get(g => g.PurchaseOrderHeaderId == id).OrderBy(g => g.PurchaseOrderHeaderId);
                //if (pos != null)
                //{
                //    return pos;
                //}
                //else
                //    return null;
                return pos == null ? null : pos;

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
                var po = _unitofwork.PurchaseOrderHeaderRepository.Get(g => g.PurchaseOrderHeaderId == id).FirstOrDefault();
                return po == null ? null : po;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<PurchaseOrderDetail> GetPOIdByQtys(long id)
        {
            try
            {
                var po = _unitofwork.PurchaseOrderDetailRepository.Get(g => g.PurchaseOrderHeaderId == id).ToList();
                return po == null ? null : po;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<PurchaseOrderDetail> GetPODetById(long id)
        {
            try
            {
                // List<PurchaseOrderDetail> podet = _unitofwork.PurchaseOrderDetailRepository.Get(p => p.PurchaseOrderHeaderId == id).OrderBy(g => g.LineNo).ToList();




                List<PurchaseOrderDetail> podet = (from pod in _unitofwork.PurchaseOrderDetailRepository.Get(p => p.PurchaseOrderHeaderId == id)
                                                   join psm in _unitofwork.ProductRepository.Get()
                                                   on pod.ProductId equals psm.ProductId
                                                   where psm.IsActive == true && psm.IsDelete == false
                                                   orderby pod.LineNo
                                                   select pod).ToList();

                return podet ?? null;
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
        public ProductStockMaster GetProductDetails(long prodid, long polocid, int companyid)
        {
            ProductStockMaster detailslist = new ProductStockMaster();


            if (_unitofwork.ProductStockMasterRepository.Get(p => p.ProductId == prodid && p.LocationId == polocid && p.CompanyID == companyid).Any(ps => ps.ProductId == prodid && ps.LocationId == polocid && ps.CompanyID == companyid))
            {
                detailslist = _unitofwork.ProductStockMasterRepository.Get(p => p.ProductId == prodid && p.LocationId == polocid && p.CompanyID == companyid).First();

                return detailslist;
            }
            else
            {
                return detailslist;
            }



        }

        public List<POViewModel> GetProductTaxes1(long prodid, long polocid, int companyid)
        {
            List<POViewModel> povwmodel = new List<POViewModel>();


            if (CheckProductTaxes(prodid, companyid))
            {


                var taxproduct = (
                        from p in _unitofwork.ProductRepository.Get()
                        join pt in _unitofwork.ProductTaxRepository.Get() on p.ProductId equals pt.ProductId
                        join ps in _unitofwork.ProductStockMasterRepository.Get() on p.ProductId equals ps.ProductId
                        join tx in _unitofwork.TaxRepository.Get() on pt.TaxId equals tx.TaxId

                        where p.ProductId == prodid && ps.LocationId == polocid && p.CompanyID == companyid && ps.CompanyID == companyid
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
                        from ps in _unitofwork.ProductStockMasterRepository.Get()
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
        public List<POViewModel> GetProductTaxes(long prodid, long polocid, int companyid)
        {
            List<POViewModel> povwmodel = new List<POViewModel>();


            if (CheckProductTaxes(prodid, companyid))
            {


                var taxproduct = (
                        from p in _unitofwork.ProductRepository.Get()
                        join pt in _unitofwork.ProductTaxRepository.Get() on p.ProductId equals pt.ProductId
                        join ps in _unitofwork.ProductStockMasterRepository.Get() on p.ProductId equals ps.ProductId
                        join tx in _unitofwork.TaxRepository.Get() on pt.TaxId equals tx.TaxId

                        where p.ProductId == prodid && ps.LocationId == polocid && p.CompanyID == companyid && ps.CompanyID == companyid
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
                        from ps in _unitofwork.ProductStockMasterRepository.Get()
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
        public List<POViewModel> GetReOrderLevelExceededProductBySupplierId1(long supplierid, long locid, int companyid)
        {
            List<POViewModel> supqtylist = new List<POViewModel>();

            var supproduct = (
                            from pp in _unitofwork.SupplierProductRepository.Get()
                            join ps in _unitofwork.ProductStockMasterRepository.Get() on pp.ProductId equals ps.ProductId
                            join p in _unitofwork.ProductRepository.Get() on pp.ProductId equals p.ProductId
                            // join pd in _unitofwork.PurchaseOrderDetailRepository.Get() on p.ProductId equals pd.ProductId
                            // join ph in _unitofwork.PurchaseOrderHeaderRepository.Get() on pd.PurchaseOrderHeaderId equals ph.PurchaseOrderHeaderId
                            where (ps.Stock) <= ps.ReOrderLevel
                             //&& ps.ReOrderQuantity != 0
                             && ps.LocationId == locid && p.CompanyID == companyid && ps.CompanyID == companyid && pp.CompanyID == companyid
                            //&& ph.IsGRN==false && ph.IsTempPO==false
                            // && pp.SupplierId == supplierid                        
                            orderby ps.ProductId
                            select new
                            {
                                ProductId = pp.ProductId,
                                SupplierId = pp.SupplierId,
                                CostPrice = ps.CostPrice,
                                SellingPrice = ps.SellingPrice,
                                ReOrderQty = ps.ReOrderQuantity,
                                ProductName = p.ProductName,
                                CurrentStock = ps.Stock
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

            return supqtylist;
        }
        public List<POViewModel> GetReOrderLevelExceededProductBySupplierId(long supplierid, long locid, int companyid)
        {
            List<POViewModel> supqtylist = new List<POViewModel>();


            var supproduct = (
                            from pp in _unitofwork.SupplierProductRepository.Get()
                            join ps in _unitofwork.ProductStockMasterRepository.Get() on pp.ProductId equals ps.ProductId
                            join p in _unitofwork.ProductRepository.Get() on pp.ProductId equals p.ProductId
                            where pp.SupplierId == supplierid &&
                            (ps.Stock
                            // +_unitofwork.PurchaseOrderDetailRepository.Get(pd=>pd.IsGRN==false && pd.ProductId==p.ProductId).Sum(pd=>pd.OrderQty)
                            ) <= ps.ReOrderLevel
                            && ps.LocationId == locid && ps.ReOrderQuantity != 0 && p.CompanyID == companyid && ps.CompanyID == companyid
                            && pp.CompanyID == companyid
                            orderby ps.ProductId
                            select new
                            {
                                ProductId = pp.ProductId,
                                SupplierId = pp.SupplierId,
                                ReOrderQty = ps.ReOrderQuantity,
                                ProductName = p.ProductName,
                                CostPrice = ps.CostPrice,
                                SellingPrice = ps.CostPrice,
                                Stock = ps.Stock,
                                ROL = ps.ReOrderLevel,
                                ROQ = ps.ReOrderQuantity
                            }
                        ).ToList();

            foreach (var sup in supproduct)
            {
                var vm = new POViewModel();
                vm.SupplierId = sup.SupplierId;
                vm.ReOrderQty = sup.ReOrderQty;
                vm.ItemId = sup.ProductId;
                vm.ItemDesc = sup.ProductName;
                vm.CostPrice = sup.CostPrice;
                vm.SellingPrice = sup.SellingPrice;
                vm.POStock = GetPOStockByProductId(sup.ProductId, companyid);
                vm.CurrentStock = sup.Stock;
                vm.ROL = sup.ROL;
                vm.ROQ = sup.ROQ;
                supqtylist.Add(vm);
            }


            return supqtylist;
        }

        public List<POViewModel> GetRequestNoteItemsBySupplierId(long supplierid, long requestnoteid, int companyid)
        {
            List<POViewModel> supqtylist = new List<POViewModel>();


            var supproduct = (
                            from pp in _unitofwork.SupplierProductRepository.Get()
                                //join ps in _unitofwork.ProductStockMasterRepository.Get() on pp.ProductId equals ps.ProductId
                            join rnd in _unitofwork.RequestNoteAccptanceDetailRepository.Get() on pp.ProductId equals rnd.ProductId
                            join p in _unitofwork.ProductRepository.Get() on rnd.ProductId equals p.ProductId
                            where pp.SupplierId == supplierid && rnd.RequestNoteAccptanceHeaderId == requestnoteid
                                    && p.CompanyID == companyid && pp.CompanyID == companyid
                            //&&
                            //(ps.Stock

                            //) <= ps.ReOrderLevel
                            //&& ps.LocationId == locid && ps.ReOrderQuantity != 0
                            orderby rnd.ProductId
                            select new
                            {
                                ProductId = rnd.ProductId,
                                SupplierId = pp.SupplierId,
                                ReOrderQty = rnd.IssueQty,
                                ProductName = p.ProductName,
                                CostPrice = rnd.CostPrice,
                                SellingPrice = rnd.SellingPrice,
                                Stock = 0,
                                ROL = 0,
                                ROQ = 0
                            }
                        ).ToList();

            foreach (var sup in supproduct)
            {
                var vm = new POViewModel();
                vm.SupplierId = Convert.ToInt32(sup.SupplierId);
                vm.ReOrderQty = sup.ReOrderQty;
                vm.ItemId = sup.ProductId;
                vm.ItemDesc = sup.ProductName;
                vm.CostPrice = sup.CostPrice;
                vm.SellingPrice = sup.SellingPrice;
                vm.POStock = GetPOStockByProductId(Convert.ToInt32(sup.ProductId), companyid);
                vm.CurrentStock = sup.Stock;
                vm.ROL = sup.ROL;
                vm.ROQ = sup.ROQ;
                supqtylist.Add(vm);
            }


            return supqtylist;
        }


        public decimal GetPOStockByProductId(int productid, int companyid)
        {
            var poq = (from ph in _unitofwork.PurchaseOrderHeaderRepository.Get()
                       join pd in _unitofwork.PurchaseOrderDetailRepository.Get()
                       on ph.PurchaseOrderHeaderId equals pd.PurchaseOrderHeaderId
                       where (ph.DocumentStatus == 1 || ph.DocumentStatus == 2 || ph.DocumentStatus == 3 || ph.DocumentStatus == 6)
                       && ph.CompanyID == companyid
                       && pd.ProductId == productid
                       && pd.OrderQty != pd.GRNQuantity
                       select new
                       {
                           POQty = pd.OrderQty
                       }
                     ).ToList();
            decimal qty = poq.Sum(p => p.POQty);
            return qty;
        }
        public List<POViewModel> GetProductTaxesBySupplierProductId(long supplierproductid, long polocationid, int companyid)
        {
            List<POViewModel> povwmodel = new List<POViewModel>();
            if (CheckProductTaxes(supplierproductid, companyid))
            {

                var taxproduct = (
                        from p in _unitofwork.ProductRepository.Get()
                        join pt in _unitofwork.ProductTaxRepository.Get() on p.ProductId equals pt.ProductId
                        join ps in _unitofwork.ProductStockMasterRepository.Get() on p.ProductId equals ps.ProductId
                        join tx in _unitofwork.TaxRepository.Get() on pt.TaxId equals tx.TaxId

                        where p.ProductId == supplierproductid && ps.Stock <= ps.ReOrderLevel
                        && ps.LocationId == polocationid
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
                var product = (from p in _unitofwork.ProductRepository.Get()
                               join ps in _unitofwork.ProductStockMasterRepository.Get() on p.ProductId equals ps.ProductId
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
                        ReOrderQty = prdt.ReOrderQty
                    };
                    povwmodel.Add(vvm);
                }
            }
            return povwmodel;


        }
        public bool SavePurchaseOrder(PurchaseOrderHeader poheader)
        {
            _unitofwork.CreateTransaction();

            try
            {
                _unitofwork.PurchaseOrderHeaderRepository.Insert(poheader);

                if (_unitofwork.Save() == 1)
                {

                    int idx = 1;
                    foreach (var detail in poheader.PODetail)
                    {
                        var podet = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId &&
                                                                                      s.LocationId == poheader.POLocationId && s.CompanyID == poheader.CompanyID);


                      



                        detail.PurchaseOrderHeaderId = poheader.PurchaseOrderHeaderId;
                        if (podet.First().StockCode == null)
                        {
                            decimal maxDecimal183 = 999999999999999.999M;
                            decimal minDecimal183 = -999999999999999.999M;

                            decimal avgcost = podet.First().AvgCost;
                            avgcost = Math.Round(avgcost, 2, MidpointRounding.AwayFromZero);
                            if (avgcost >= minDecimal183 && avgcost <= maxDecimal183)
                            {
                                podet.First().AvgCost = avgcost;

                            }
                            else
                            {
                                podet.First().AvgCost = podet.First().CostPrice;



                            }


                            detail.StockCode = "1";
                        }
                        else
                        {
                            detail.StockCode = podet.First().StockCode;


                            decimal maxDecimal183 = 999999999999999.999M;
                            decimal minDecimal183 = -999999999999999.999M;

                            decimal avgcost = podet.First().AvgCost;
                            avgcost = Math.Round(avgcost, 2, MidpointRounding.AwayFromZero);
                            if (avgcost >= minDecimal183 && avgcost <= maxDecimal183)
                            {
                                podet.First().AvgCost = avgcost;
                               
                            }
                            else
                            {
                                podet.First().AvgCost = podet.First().CostPrice;
                               


                            }

                            

                            
                        }

                        detail.LineNo = idx;
                        idx += 1;


                       



                        _unitofwork.PurchaseOrderDetailRepository.Insert(detail);

                        if (!poheader.IsTempPO)
                        {
                            var supplierproduct = _unitofwork.SupplierProductRepository.Get(s => s.SupplierId == poheader.SupplierId
                                                                                        && s.ProductId == detail.ProductId && s.CompanyID == poheader.CompanyID).FirstOrDefault();
                            if (supplierproduct != null)
                            {
                                supplierproduct.LastCostPrice = detail.CostPrice;
                                _unitofwork.SupplierProductRepository.Update(supplierproduct);
                            }

                            //should apply a config
                            //var productstockmaster = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId
                            //                                            && s.LocationId == poheader.POLocationId).FirstOrDefault();
                            //productstockmaster.CostPrice = detail.CostPrice;
                            //_unitofwork.ProductStockMasterRepository.Update(productstockmaster);

                            // var productmaster = _unitofwork.ProductRepository.Get(s => s.ProductId == detail.ProductId).FirstOrDefault();
                            //_unitofwork.ProductRepository.Update(productmaster);

                        }
                    }

                    if (_unitofwork.Save() < poheader.PODetail.Count() || !CheckPurchaseOrderQty(poheader.DocumentNo))
                    {
                        _unitofwork.Rollback();
                        return false;
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

        public bool CheckPurchaseOrderQty(string docNo)
        {

            long poh = _unitofwork.PurchaseOrderHeaderRepository
                                  .Get(s => s.DocumentNo == docNo)
                                  .Select(s => s.PurchaseOrderHeaderId)
                                  .FirstOrDefault();

            decimal qty = _unitofwork.PurchaseOrderDetailRepository
                         .Get(s => s.PurchaseOrderHeaderId == poh)
                         .Select(s => s.OrderQty)
                         .FirstOrDefault();

            if(qty != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SavedPurchaseOrderHeader(PurchaseOrderHeader poheader)
        {
            try
            {
                _unitofwork.CreateTransaction();
                _unitofwork.PurchaseOrderHeaderRepository.Insert(poheader);
                _unitofwork.Save();
                _unitofwork.Commit();

                return true;
            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return false;

            }
        }

        public bool SavePurchaseOrderDetail(PurchaseOrderDetail podetails)
        {
            try
            {
                _unitofwork.CreateTransaction();
                _unitofwork.PurchaseOrderDetailRepository.Insert(podetails);
                _unitofwork.Save();
                _unitofwork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return false;

            }
        }
        public bool UpdatePurchaseOrderDetail(bool IsPoTransfer, string ReqDocNo, int POLocationId, int companyid, int ProductID)
        {
            try
            {
                _unitofwork.CreateTransaction();
                var PoTransferheader = _unitofwork.RequestNoteHeaderRepository.Get(r => r.DocumentNo == ReqDocNo && r.ToLocationId == POLocationId && r.CompanyId == companyid).FirstOrDefault();
                var PoAcceptanceTransferheader = _unitofwork.RequestNoteAccptanceHeaderRepository.Get(r => r.DocumentNo == ReqDocNo && r.ToLocationId == POLocationId && r.CompanyId == companyid).FirstOrDefault();

                if (PoTransferheader != null)
                {
                    PoTransferheader.IsPoTransfer = IsPoTransfer;
                    PoTransferheader.RequestType = "Request Note based PO";
                    _unitofwork.RequestNoteHeaderRepository.Update(PoTransferheader);
                }
                if (PoAcceptanceTransferheader != null)
                {
                    PoAcceptanceTransferheader.RequestType = "Request Note based PO";

                    _unitofwork.RequestNoteAccptanceHeaderRepository.Update(PoAcceptanceTransferheader);
                }
                var PoTransferdetail = _unitofwork.RequestNoteDetailRepository.Get(r => r.ProductId == ProductID && r.RequestnoteHeaderId == PoTransferheader.RequestnoteHeaderId).FirstOrDefault();

                if (PoTransferdetail != null)
                {

                    PoTransferdetail.IsPoTransfer = true;
                    _unitofwork.RequestNoteDetailRepository.Update(PoTransferdetail);
                }

                _unitofwork.Save();
                _unitofwork.Commit();

                return true;
            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return false;

            }
        }
        public bool SaveInvRequestNotePOTransaction(InvRequestNotePOTransaction invrequestnotepotransactions)
        {
            try
            {
                _unitofwork.CreateTransaction();
                _unitofwork.InvRequestNotePOTransaction.Insert(invrequestnotepotransactions);
                _unitofwork.Save();
                _unitofwork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return false;

            }
        }

        public bool UpdateInvRequestNotePOTransactions(long POHeaderID,long ProductID,long PODetailID)           
        {
            try
            {
                var RequestNotePOTransaction = _unitofwork.InvRequestNotePOTransaction.Get(r => r.PurchaseOrderHeaderID == POHeaderID && r.ProductID == ProductID).ToList();
                if (RequestNotePOTransaction != null)
                {

                    _unitofwork.CreateTransaction();
                    foreach (var transaction in RequestNotePOTransaction)
                    {
                        transaction.PurchaseOrderDetailID = PODetailID; // Update each transaction
                    }                  
                   
                    _unitofwork.Save();
                    _unitofwork.Commit();
                    return true;
                }

            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return false;

            }

            return true;


        }


    public bool UpdatePurchaseOrderForAveragePrices( long POHeaderID , decimal TotCostValue,decimal TotSelleingValue,decimal TotGrossValue)
        {
            try
            {
                var Poheader = _unitofwork.PurchaseOrderHeaderRepository.Get(r => r.PurchaseOrderHeaderId == POHeaderID).FirstOrDefault();

            
                if (Poheader != null)
           
                {

                    _unitofwork.CreateTransaction();
                    Poheader.TotCostPrice = TotCostValue;
                    Poheader.TotSellingPrice = TotSelleingValue;
                    Poheader.GrossAmount = TotGrossValue;
                    Poheader.NetAmount = TotGrossValue;
                    _unitofwork.PurchaseOrderHeaderRepository.Update(Poheader);
                    _unitofwork.Save();
                    _unitofwork.Commit();
                    return true;
                }
           
            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return false;

            }

            return true;
        }






        public bool SavePurchaseOrderHeader(PurchaseOrderHeader poheader, List<PurchaseOrderDetail> podetail,List<InvRequestNotePOTransaction> RequestNotePOTransaction)
        {
            _unitofwork.CreateTransaction();

            try
            {
                _unitofwork.PurchaseOrderHeaderRepository.Insert(poheader);

                int idx = 1;
                RequestNoteHeader requestnoteheader = new RequestNoteHeader();
                RequestNoteDetail requestnotedetail = new RequestNoteDetail();
             

                    foreach (var detail in podetail)
                {
                    var podet = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId &&
                                                                                  s.LocationId == detail.LocationID && s.CompanyID == detail.CompanyID);

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
                    _unitofwork.PurchaseOrderDetailRepository.Insert(detail);


       


                    if (!detail.IsTempPO)
                    {
                        var supplierproduct = _unitofwork.SupplierProductRepository.Get(s => s.SupplierId == detail.SupplierId
                                                                                    && s.ProductId == detail.ProductId && s.CompanyID == detail.CompanyID).FirstOrDefault();
                        if (supplierproduct != null)
                        {
                            supplierproduct.LastCostPrice = detail.CostPrice;
                            _unitofwork.SupplierProductRepository.Update(supplierproduct);
                        }

                        var PoTransferheader = _unitofwork.RequestNoteHeaderRepository.Get(r => r.DocumentNo == poheader.ReqDocNo && r.ToLocationId == poheader.POLocationId && r.CompanyId == poheader.CompanyID).FirstOrDefault();

                        if (PoTransferheader != null)
                        {
                            PoTransferheader.IsPoTransfer = true;
                            _unitofwork.RequestNoteHeaderRepository.Update(PoTransferheader);
                        }

                        var PoTransferdetail = _unitofwork.RequestNoteDetailRepository.Get(r => r.ProductId == detail.ProductId && r.RequestnoteHeaderId == PoTransferheader.RequestnoteHeaderId).FirstOrDefault();

                        if (PoTransferdetail != null)
                        {
                            PoTransferdetail.IsPoTransfer = true;
                            _unitofwork.RequestNoteDetailRepository.Update(PoTransferdetail);
                        }

                    }


                }

                foreach (var RequestNotePOTr in RequestNotePOTransaction)
                {
                    // RequestNotePOTr.PurchaseOrderDetailID = detail.PurchaseOrderDetailId;

                    _unitofwork.InvRequestNotePOTransaction.Insert(RequestNotePOTr);
                }


                if (_unitofwork.Save() < podetail.Count())
                {
                    _unitofwork.Rollback();
                    return false;
                }

                //if (_unitofwork.Save() < RequestNotePOTransaction.Count())
                //{
                //    _unitofwork.Rollback();
                //    return false;
                //}
                _unitofwork.Commit();

                return true;
            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return false;

            }

        }

        public bool SavePurchaseOrderDetail(List<PurchaseOrderDetail> podetail)
        {
            _unitofwork.CreateTransaction();

            try
            {
                
           
                    int idx = 1;
                    foreach (var detail in podetail)
                    {
                        var podet = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId &&
                                                                                      s.LocationId == detail.LocationID && s.CompanyID == detail.CompanyID);

                        detail.PurchaseOrderHeaderId = detail.PurchaseOrderHeaderId;
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
                        _unitofwork.PurchaseOrderDetailRepository.Insert(detail);

                        if (!detail.IsTempPO)
                        {
                            var supplierproduct = _unitofwork.SupplierProductRepository.Get(s => s.SupplierId == detail.SupplierId
                                                                                        && s.ProductId == detail.ProductId && s.CompanyID == detail.CompanyID).FirstOrDefault();
                            if (supplierproduct != null)
                            {
                                supplierproduct.LastCostPrice = detail.CostPrice;
                                _unitofwork.SupplierProductRepository.Update(supplierproduct);
                            }

                   
                        }

                    }

                    if (_unitofwork.Save() < podetail.Count())
                    {
                        _unitofwork.Rollback();
                        return false;
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

        public long DeletePODetail(long poheaderid)
        {
            var res = 0;
            try
            {
                _unitofwork.PurchaseOrderDetailRepository.DeleteRange(_unitofwork.PurchaseOrderDetailRepository.Get(x => x.PurchaseOrderHeaderId == poheaderid));
                res = _unitofwork.Save();


            }
            catch (Exception)
            {

                throw;
            }
            return res;
        }
        public bool EditPo(PurchaseOrderHeader poheader)
        {
            _unitofwork.CreateTransaction();
                try
                {
                    _unitofwork.PurchaseOrderHeaderRepository.Update(poheader);
                    if (_unitofwork.Save() == 1)
                    {
                        DeletePODetail(poheader.PurchaseOrderHeaderId);
                        int idx = 1;
                        foreach (var detail in poheader.PODetail)
                        {
                            var podet = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId && s.LocationId == poheader.POLocationId 
                                                                                     && s.CompanyID==poheader.CompanyID);

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

                            _unitofwork.PurchaseOrderDetailRepository.Insert(detail);
                            if (!poheader.IsTempPO)
                            {
                                var supplierproduct = _unitofwork.SupplierProductRepository.Get(s => s.SupplierId == poheader.SupplierId
                                                                                            && s.ProductId == detail.ProductId && s.CompanyID== poheader.CompanyID).FirstOrDefault();
                                if (supplierproduct != null)
                                {
                                    supplierproduct.LastCostPrice = detail.CostPrice;
                                    _unitofwork.SupplierProductRepository.Update(supplierproduct);
                                }

                            }


                    }
                      
                        if (_unitofwork.Save() < poheader.PODetail.Count)
                        {
                            _unitofwork.Rollback();
                            return false;
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
        public List<PurchaseOrderHeader> GetPOSummaryReport(long locid, long docid, DateTime from, DateTime to)
        {
            try
            {
                List<PurchaseOrderHeader> purchaseorderheader = new List<PurchaseOrderHeader>();

                if (locid != 0 && docid != 0)
                {
                    purchaseorderheader = _unitofwork.PurchaseOrderHeaderRepository.Get(r => r.PurchaseOrderHeaderId == docid && r.POLocationId == locid).
                                                              OrderBy(c => c.DocumentNo).ToList();
                }
                else if (locid != 0 && docid == 0)
                {
                    purchaseorderheader = _unitofwork.PurchaseOrderHeaderRepository.Get(
                        r => r.POLocationId == locid
                        && DbFunctions.TruncateTime(r.PODate) >= DbFunctions.TruncateTime(from) && DbFunctions.TruncateTime(r.PODate) <= DbFunctions.TruncateTime(to)
                        ).OrderBy(c => c.DocumentNo).ToList();
                }
                else if (locid == 0 && docid != 0)
                {
                    purchaseorderheader = _unitofwork.PurchaseOrderHeaderRepository.Get
                        (r => r.POLocationId == docid).OrderBy(c => c.DocumentNo).ToList();
                }
                else if (locid == 0 && docid == 0)
                {
                   
                    purchaseorderheader = _unitofwork.PurchaseOrderHeaderRepository.Get(r => DbFunctions.TruncateTime (r.PODate) >= DbFunctions.TruncateTime(from) 
                    && DbFunctions.TruncateTime(r.PODate) <= DbFunctions.TruncateTime(to)).OrderBy(c => c.DocumentNo).OrderBy(d => d.POLocationId).ToList();

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
        public IEnumerable<PurchaseOrderHeader> GetDocNoByLocId(long locid,int companyid)
        {
            try
            {
                IEnumerable<PurchaseOrderHeader> docs = _unitofwork.PurchaseOrderHeaderRepository.Get(e => e.POLocationId == locid && e.CompanyID==companyid).OrderBy(k => k.DocumentNo);


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
        public List<PODetailViewModel> GetPODetailReport(long locid, long docid, DateTime frmdate, DateTime todate,int companyid)
        {
            List<PODetailViewModel> reportdata = new List<PODetailViewModel>();
            List<PurchaseOrderHeader> dbheader = new List<PurchaseOrderHeader>();

            var poheader = _unitofwork.PurchaseOrderHeaderRepository.Get(p=>p.CompanyID==companyid);

            if (locid == 0 || docid == 0)
            {
                dbheader = poheader.Where(s => s.PODate.Date >= frmdate.Date && s.PODate.Date <= todate.Date).ToList();
            }
            else if (locid != 0 && docid != 0)
            {
                dbheader = poheader.Where(s => s.POLocationId == locid && s.PurchaseOrderHeaderId == docid).ToList();
            }
            else if (locid != 0 && docid == 0)
            {
                dbheader = poheader.Where(s => s.POLocationId == locid && s.PODate.Date >= frmdate.Date && s.PODate.Date <= todate.Date).ToList();
            }

            foreach (var header in dbheader)
            {
                PODetailViewModel vm = new PODetailViewModel();
                vm.Location = _unitofwork.LocationRepository.GetById(header.POLocationId).LocationName;
                vm.DocumentDate = header.PODate.ToShortDateString();
                vm.DocumentNo = header.DocumentNo;
                vm.Remark = header.Remark;
                vm.Status = _bllstatus.GetDocStatusById(header.DocumentStatus).Description;
                foreach (var s in _unitofwork.PurchaseOrderDetailRepository.Get(r => r.PurchaseOrderHeaderId == header.PurchaseOrderHeaderId))
                {
                    PODetailViewModel.ReportDetail det = new PODetailViewModel.ReportDetail();
                    var prd = _unitofwork.ProductRepository.GetById(s.ProductId);
                    det.ProductId = s.ProductId;
                    if (prd != null)
                    {
                        det.ProductName = prd.ProductName;
                        det.ProductCode = prd.ProductCode;
                    }
                    det.OrderQty = s.OrderQty;
                    det.FreeQty = s.FreeQty;
                    det.CostPrice = s.CostPrice;
                    det.SellingPrice = s.SellingPrice;
                    det.CostValue = s.CostValue;
                   
                    vm.Detail.Add(det);
                }

                reportdata.Add(vm);
            }

            return reportdata;
        }
        public IEnumerable<PurchaseOrderHeader> GetTodayPos(DateTime date,int companyid)
        {
            try
            {

                IEnumerable<PurchaseOrderHeader> pos = _unitofwork.PurchaseOrderHeaderRepository.Get(p => DbFunctions.TruncateTime(p.CreatedDate) == date.Date && p.CompanyID==companyid).OrderByDescending(g => g.DocumentDate);
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
        public IEnumerable<PurchaseOrderHeader> GetThisweekPos(DateTime fromdate,DateTime date,int companyid)
        {
            try
            {

                IEnumerable<PurchaseOrderHeader> pos = _unitofwork.PurchaseOrderHeaderRepository.Get(p => p.IsTempPO == false
                                                              &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) >= fromdate.Date
                                                             &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) <= date.Date && p.CompanyID== companyid).OrderByDescending(g => g.DocumentDate);
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
        public IEnumerable<ProductStockMaster> GetSupplierProductsNames(long supplierid,int companyid)
        {
            try
            {

                var sysproducts = (from p in _unitofwork.SupplierProductRepository.Get()
                                   join pp in _unitofwork.ProductRepository.Get() on p.ProductId equals pp.ProductId
                                   join u in _unitofwork.UnitOfMeasureRepository.Get() on
                                   pp.PurchasingUnit equals u.UnitOfMeasureId
                                   where p.SupplierId == supplierid && pp.IsActive == true && pp.IsDelete == false
                                   && p.CompanyID==companyid && pp.CompanyID==companyid && pp.CompanyID==companyid



                                   orderby pp.ProductCode
                                   select new
                                   {
                                       ProductId = p.ProductId,
                                       ProductName = pp.ProductName,
                                       ProductCode = pp.ProductCode,
                                       UOM = u.UnitOfMeasureName
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
        public int ActiveInactivePO(long poid,bool status,string modifiedby)
        {
            var dbpo = _unitofwork.PurchaseOrderHeaderRepository.GetById(poid);
            dbpo.IsActive = status;
            dbpo.ModifiedDate = DateTime.Now;
            dbpo.ModifiedUser = modifiedby;
            _unitofwork.PurchaseOrderHeaderRepository.Update(dbpo);
            return _unitofwork.Save();

        }

    }
}
