using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Promotions;
using RIT.HMS.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.Promotions
{
    public class BLL_ItemToItemPromotion
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_ItemToItemPromotion()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_ItemToItemPromotion(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }

        public List<Product> GetProducts(int companyid)
        {
            var sysproducts = _unitofwork.ProductRepository.Get(p => p.IsActive == true && p.IsRowMaterial == false && p.CompanyID == companyid).Select(p => new
            {
                p.ProductCode,
                p.ProductId,
                p.ProductName,
                p.IsActive,
                p.IsRowMaterial,
                p.CompanyID,
            }).ToList().OrderBy(p => p.ProductCode);

            List<Product> products = new List<Product>();
            foreach (var p in sysproducts)
            {
                Product prd = new Product();
                prd.ProductId = p.ProductId;
                prd.ProductCode = p.ProductCode;
                prd.ProductName = p.ProductName;
                prd.IsActive = p.IsActive;
                prd.IsRowMaterial = p.IsRowMaterial;

                products.Add(prd);
            }

            if (products != null)
            {
                return products;
            }
            else
                return null;
        }


        public List<ProductServingUnit> GetServingUnits(long id)
        {
            var servingunits = _unitofwork.ProductServingUnitRepository.Get(p => p.ProductId == id).Select(s => new
            {
                s.ProductServingUnitId,
                s.ServingUnit,
                s.ProductId,
                s.SellingPrice,
                s.CompanyID,
                s.LocationId
            }).ToList().OrderBy(p => p.ServingUnit);

            List<ProductServingUnit> producservingunits = new List<ProductServingUnit>();
            foreach (var p in servingunits)
            {
                ProductServingUnit su = new ProductServingUnit();

                su.ProductId = p.ProductId;
                su.ServingUnit = p.ServingUnit;
                su.ProductServingUnitId = p.ProductServingUnitId;
                su.SellingPrice = p.SellingPrice;
                su.LocationId = p.LocationId;

                producservingunits.Add(su);
            }

            if (producservingunits != null)
            {
                return producservingunits;
            }
            else
                return null;
        }

        public ProductStockMasterViewModel GetComboProductDetailsByID(long id)
        {
            var items = (from p in _unitofwork.PromotionDetailsBuyXProductRepository.Get(p => p.ProductId == id)
                        join ps in _unitofwork.PromotionRepository.Get() on p.InvPromotionMasterId equals ps.InvPromotionMasterID
                        select new
                        {
                            ProductId = p.ProductId,
                            EndDate = ps.EndDate,
                            StartDate = ps.StartDate
                        }
                        ).FirstOrDefault();

            if (items != null)
            {

                ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                vm.ProductId = items.ProductId;
                vm.EndDate = Convert.ToDateTime(items.EndDate);
                vm.StartDate = Convert.ToDateTime(items.StartDate);


                return vm;
            }
            else
                return null;
        }

        public ProductStockMasterViewModel GetComboProductDetailsByID1(long id)
        {
            var items = (from p in _unitofwork.InvPromotionDetailsProductDisRepository.Get(p => p.ProductID == id)
                         join ps in _unitofwork.PromotionRepository.Get() on p.InvPromotionMasterID equals ps.InvPromotionMasterID
                         select new
                         {
                             ProductId = p.ProductID,
                             EndDate = ps.EndDate,
                             StartDate = ps.StartDate
                         }
                        ).FirstOrDefault();

            if (items != null)
            {

                ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                vm.ProductId = items.ProductId;
                vm.EndDate = Convert.ToDateTime(items.EndDate);
                vm.StartDate = Convert.ToDateTime(items.StartDate);


                return vm;
            }
            else
                return null;
        }

        public ProductStockMasterViewModel GetComboProductDetailsByID2(long id)
        {
            var items = (from p in _unitofwork.InvPromoBillValueBasedGetYProduct.Get(p => p.ProductId == id)
                         join ps in _unitofwork.PromotionRepository.Get() on p.InvPromotionMasterId equals ps.InvPromotionMasterID
                         select new
                         {
                             ProductId = p.ProductId,
                             EndDate = ps.EndDate,
                             StartDate = ps.StartDate
                         }
                        ).FirstOrDefault();

            if (items != null)
            {

                ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                vm.ProductId = items.ProductId;
                vm.EndDate = Convert.ToDateTime(items.EndDate);
                vm.StartDate = Convert.ToDateTime(items.StartDate);


                return vm;
            }
            else
                return null;
        }
        public ProductStockMasterViewModel GetProductDetailsById(long id, int servingunitid)
        {

            var items = (
                           from p in _unitofwork.ProductRepository.Get(p=>p.ProductId==id)
                           join ps in _unitofwork.ProductServingUnitRepository.Get(ps=>ps.ProductId == id && ps.ProductServingUnitId == servingunitid) on p.ProductId equals ps.ProductId
                           join u in _unitofwork.UnitOfMeasureRepository.Get() on p.PurchasingUnit equals u.UnitOfMeasureId
                         //  where
                           orderby p.ProductName
                           select new
                           {
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductName = p.ProductName,
                               Cost = ps.CostPrice,
                               Selling = ps.SellingPrice,
                               UOM = u.UnitOfMeasureName,
                               UOMId = p.PurchasingUnit,
                               ServingUnit = ps.ServingUnit,
                               ServingUnitId = ps.ProductServingUnitId
                           }
                       ).FirstOrDefault();


            if (items != null)
            {

                ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                vm.ProductId = items.ProductId;
                vm.ProductCode = items.ProductCode;
                vm.ProductName = items.ProductName;
                vm.SellingPrice = items.Selling;
                vm.UOM = items.UOM;
                vm.UOMId = items.UOMId;
                vm.ServingUnit = items.ServingUnit;
                vm.ServingUnitId = (int)items.ServingUnitId;

                return vm;
            }
            else
                return null;


        }

        public ProductStockMasterViewModel GetComboPackProductDetailsById(long id, int servingunitid)
        {

            var items = (
                           from p in _unitofwork.ProductRepository.Get(p => p.ProductId == id)
                           join ps in _unitofwork.ProductServingUnitRepository.Get(ps => ps.ProductId == id && ps.ProductServingUnitId == servingunitid) on p.ProductId equals ps.ProductId
                           join u in _unitofwork.UnitOfMeasureRepository.Get() on p.PurchasingUnit equals u.UnitOfMeasureId
                           //  where
                           orderby p.ProductName
                           select new
                           {
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductName = p.ProductName,
                               Cost = ps.CostPrice,
                               Selling = ps.SellingPrice,
                               UOM = u.UnitOfMeasureName,
                               UOMId = p.PurchasingUnit,
                               ServingUnit = ps.ServingUnit,
                               ServingUnitId = ps.ProductServingUnitId
                           }
                       ).FirstOrDefault();


            if (items != null)
            {

                ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                vm.ProductId = items.ProductId;
                vm.ProductCode = items.ProductCode;
                vm.ProductName = items.ProductName;
                vm.SellingPrice = items.Selling;
                vm.UOM = items.UOM;
                vm.UOMId = items.UOMId;
                vm.ServingUnit = items.ServingUnit;
                vm.ServingUnitId = (int)items.ServingUnitId;

                return vm;
            }
            else
                return null;


        }

        public int SubmitPromotion(List<ProductStockMasterViewModel> promotions,int companyid)
        {
            _unitofwork.CreateTransaction();
            {
                try
                {
                    int promotionmasterid = promotions.FirstOrDefault().PromotionMasterId;
                    //var exists = context.InvPromotionDetailsBuyXProduct.Any(p => p.InvPromotionMasterId == promotionmasterid);
                    //if (exists)
                    //{
                    //    context.InvPromotionDetailsBuyXProduct.RemoveRange(context.InvPromotionDetailsBuyXProduct.Where(p => p.InvPromotionMasterId == promotionmasterid));
                    //}

                    int maxgroupid = 0;
                    var a = _unitofwork.PromotionDetailsBuyXProductRepository.Get().ToList();
                    if (a.Count != 0)
                    {
                        maxgroupid = _unitofwork.PromotionDetailsBuyXProductRepository.Get().Max(g => g.GroupId);
                    }
                    maxgroupid += 1;

                    foreach (var p in promotions)
                    {

                        InvPromotionDetailsBuyXProduct promotion = new InvPromotionDetailsBuyXProduct();
                        promotion.InvPromotionMasterId = promotionmasterid;
                        promotion.ProductId = p.ProductId;
                        promotion.ProductCode = p.ProductCode;
                        promotion.ProductName = p.ProductName;
                        promotion.ServingUnitId = p.ServingUnitId;
                        promotion.BuyUnitOfMeasureId = p.UOMId;
                        promotion.Rate = p.SellingPrice;
                        promotion.Qty = p.Quantity;

                        if (p.DiscountType == "Amt")
                        {
                            promotion.DiscountAmount = p.DiscountAmt;
                            promotion.DiscountPercentage = 0;

                        }
                        else if (p.DiscountType == "Prc")
                        {
                            promotion.DiscountAmount = 0;
                            promotion.DiscountPercentage = p.DiscountAmt;

                        }
                        else if (p.DiscountType == "N")
                        {
                            promotion.DiscountAmount = 0;
                            promotion.DiscountPercentage = 0;
                        }
                        promotion.ProductType = p.PromotionItemType;
                        promotion.GroupOfCompanyID = 1;
                        promotion.CompanyID = companyid;
                        promotion.LocationId = 1;
                        promotion.CreatedUser = "";
                        promotion.CreatedDate = DateTime.Now;
                        promotion.ModifiedDate = DateTime.Now;
                        promotion.ModifiedUser = "";
                        promotion.DataTransfer = 0;
                        promotion.Points = 0;
                        promotion.GroupId = maxgroupid;
                        _unitofwork.PromotionDetailsBuyXProductRepository.Insert(promotion);

                    }

                    var res = _unitofwork.Save();
                    _unitofwork.Commit();
                    return res;
                }
                catch (Exception e)
                {
                    _unitofwork.Rollback();
                    return 0;
                }
            }


        }

        public List<InvPromotionMaster> GetPromotionMasters(int companyid)
        {
            return _unitofwork.PromotionRepository.Get(p => p.IsDelete == false && p.CompanyID==companyid).ToList();
        }

        public List<InvPromotionMaster> GetComboPromotionMasters(int companyid)
        {
            return _unitofwork.PromotionRepository.Get(p => p.IsActive == true && p.IsDelete == false && p.CompanyID == companyid).ToList();
        }

        public InvPromotionMaster GetPromotionMasterById(int id)
        {
            return _unitofwork.PromotionRepository.Get(p => p.IsDelete == false && p.InvPromotionMasterID == id).FirstOrDefault();

            // var promotion=
        }

        public InvComboPackBundleItemPrice GetComboPackBundleItemById(int id)
        {
            try
            { 
                var items = (from p in _unitofwork.InvBundleItemPriceRepositorys.Get(p => p.PromotionMasterId == id)
                             select new
                             {
                                 PromotionMasterId = p.PromotionMasterId
                             }
                            ).FirstOrDefault();

                if (items != null)
                {

                    InvComboPackBundleItemPrice vm = new InvComboPackBundleItemPrice();
                    vm.PromotionMasterId = items.PromotionMasterId;


                    return vm;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            // var promotion=
        }
        public ProductStockMasterViewModel MyProperty(int promotionmasterid, int companyid)
        {
           
                ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                if (promotionmasterid != 0)
                {
                    var pm = _unitofwork.PromotionRepository.Get(p => p.InvPromotionMasterID == promotionmasterid && p.CompanyID==companyid).FirstOrDefault();
                    vm.PromotionMasterId = (int)pm.InvPromotionMasterID;
                    vm.PromotionTypeId = pm.PromotionTypeID;
                    vm.PromotionName = pm.PromotionName;
                    vm.LocationId = pm.LocationID;
                }
                return vm;

       
        }

        public List<ProductStockMasterViewModel> GetPromotionItems(int promotionmasterid, int companyid)
        {
            var items = (
                          from p in _unitofwork.PromotionDetailsBuyXProductRepository.GetAsNoTracking(p => p.InvPromotionMasterId == promotionmasterid && p.CompanyID == companyid)
                          join pm in _unitofwork.PromotionRepository.GetAsNoTracking(pm => pm.CompanyID == companyid && pm.InvPromotionMasterID == promotionmasterid) on p.InvPromotionMasterId equals pm.InvPromotionMasterID
                          join pp in _unitofwork.ProductRepository.GetAsNoTracking(pp => pp.CompanyID == companyid).Select(
                                                                        pp => new
                                                                            {
                                                                                pp.ProductId,
                                                                                pp.ProductCode,
                                                                                pp.ProductName,
                                                                                pp.PurchasingUnit,                                                                         
                                                                            }
                                                                        ) on p.ProductId equals pp.ProductId
                          join u in _unitofwork.UnitOfMeasureRepository.GetAsNoTracking(u => u.CompanyID == companyid) on pp.PurchasingUnit equals u.UnitOfMeasureId
                          join s in _unitofwork.ProductServingUnitRepository.GetAsNoTracking(s => s.CompanyID == companyid) on p.ServingUnitId equals s.ProductServingUnitId                     
                          select new
                          {
                              ProductId = p.ProductId,
                              ProductCode = pp.ProductCode,
                              ProductName = pp.ProductName,
                              Cost = 0,
                              Selling = p.Rate,
                              Qty = p.Qty,
                              DiscountAmount = p.DiscountAmount,
                              DiscountPrc = p.DiscountPercentage,
                              UOM = u.UnitOfMeasureName,
                              UOMId = p.BuyUnitOfMeasureId,
                              ServingUnit = s.ServingUnit,
                              ServingUnitId = p.ServingUnitId,
                              PromotionMasterId = p.InvPromotionMasterId,
                              PromotionTypeId = pm.PromotionTypeID,
                              PromotionName = pm.PromotionName,
                              ProductType = p.ProductType,
                              GroupId= p.GroupId

                          }
                      );
          
            List<ProductStockMasterViewModel> vmm = new List<ProductStockMasterViewModel>();
            if (items != null && items.ToList().Count != 0)
            {
                
                foreach (var i in items)
                {
                    ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                    vm.ProductId = i.ProductId;
                    vm.ProductCode = i.ProductCode;
                    vm.ProductName = i.ProductName;
                    vm.SellingPrice = i.Selling;
                    vm.UOM = i.UOM;
                    vm.UOMId = i.UOMId;
                    vm.Quantity = i.Qty;
                    vm.ServingUnit = i.ServingUnit;
                    vm.ServingUnitId = (int)i.ServingUnitId;
                    vm.PromotionMasterId = (int)i.PromotionMasterId;
                    vm.PromotionTypeId = i.PromotionTypeId;
                    vm.PromotionName = i.PromotionName;
                    vm.DiscountAmt = i.DiscountAmount;
                    vm.DiscountPrc = i.DiscountPrc;
                    vm.ProductType = i.ProductType;
                    vm.GroupId=i.GroupId;
                    if (vm.DiscountAmt != 0)
                    {
                        vm.DiscountAmt = i.DiscountAmount;
                        vm.DiscountType = "Amt";

                    }
                    else if (vm.DiscountPrc != 0)
                    {
                        vm.DiscountAmt = i.DiscountPrc;
                        vm.DiscountType = "Prc";
                    }
                    vmm.Add(vm);
                }
                return vmm;
            }
            else if (items.ToList().Count == 0)
            {

                ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                if (promotionmasterid != 0)
                {
                    var pm = _unitofwork.PromotionRepository.Get(p => p.InvPromotionMasterID == promotionmasterid && p.CompanyID == companyid).FirstOrDefault();
                    vm.PromotionMasterId = (int)pm.InvPromotionMasterID;
                    vm.PromotionTypeId = pm.PromotionTypeID;
                    vm.PromotionName = pm.PromotionName;
                    vmm.Add(vm);
                }
                return vmm;
            } else
                    return null;
        }

        public int RemovePromotionItemGroup(InvPromotionDetailsBuyXProduct itemgrouptoremove)
        {
            _unitofwork.CreateTransaction();
            try
            {
                var itemgroup = _unitofwork.PromotionDetailsBuyXProductRepository.Get(x=>x.GroupId==itemgrouptoremove.GroupId 
                && x.InvPromotionMasterId==itemgrouptoremove.InvPromotionMasterId && x.CompanyID==itemgrouptoremove.CompanyID);
                 _unitofwork.PromotionDetailsBuyXProductRepository.DeleteRange(itemgroup);
                 _unitofwork.Save();

                _unitofwork.Commit();
                return 1;
            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return 0;
            }
           
        }
    }
}
