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
   public class BLL_BillValueBasedGetYProductPromotion
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_BillValueBasedGetYProductPromotion()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_BillValueBasedGetYProductPromotion(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public List<Product> GetProducts(int companyid)
        {
            var sysproducts = _unitofwork.ProductRepository.Get().Select(p => new
            {
                p.ProductCode,
                p.ProductId,
                p.ProductName,
                p.IsActive,
                p.IsRowMaterial,
                p.CompanyID
            }).Where(p => p.IsActive == true && p.IsRowMaterial == false && p.CompanyID==companyid).ToList().OrderBy(p => p.ProductCode);

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

        public List<ProductServingUnit> GetServingUnits(long id,int companyid)
        {
            var servingunits = _unitofwork.ProductServingUnitRepository.Get().Select(s => new
            {
                s.ProductServingUnitId,
                s.ServingUnit,
                s.ProductId,
                s.SellingPrice,
                s.CompanyID
            }).Where(p => p.ProductId == id && p.CompanyID==companyid).ToList().OrderBy(p => p.ServingUnit);

            List<ProductServingUnit> producservingunits = new List<ProductServingUnit>();
            foreach (var p in servingunits)
            {
                ProductServingUnit su = new ProductServingUnit();

                su.ProductId = p.ProductId;
                su.ServingUnit = p.ServingUnit;
                su.ProductServingUnitId = p.ProductServingUnitId;
                su.SellingPrice = p.SellingPrice;


                producservingunits.Add(su);
            }

            if (producservingunits != null)
            {
                return producservingunits;
            }
            else
                return null;
        }

        public ProductStockMasterViewModel GetProductDetailsById(long id, int servingunitid,int companyid)
        {

            var items = (
                           from p in _unitofwork.ProductRepository.Get()
                           join ps in _unitofwork.ProductServingUnitRepository.Get() on p.ProductId equals ps.ProductId
                           join u in _unitofwork.UnitOfMeasureRepository.Get() on p.PurchasingUnit equals u.UnitOfMeasureId
                           where ps.ProductId == id && ps.ProductServingUnitId == servingunitid && p.CompanyID==companyid
                           && ps.CompanyID==companyid && u.CompanyID==companyid
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

                    foreach (var p in promotions)
                    {

                        InvPromoBillValueBasedGetYProduct promotion = new InvPromoBillValueBasedGetYProduct();
                        promotion.InvPromotionMasterId = p.InvPromotionMasterId;
                        promotion.ProductId = p.ProductId;
                        promotion.ProductCode = p.ProductCode;
                        promotion.ProductName = p.ProductName;
                        promotion.ServingUnitId = p.ServingUnitId;
                        promotion.BuyUnitOfMeasureId = p.UOMId;
                        promotion.Rate = p.SellingPrice;
                        promotion.Qty = p.Quantity;
                        promotion.ValueFrom = p.ValueFrom;
                        promotion.ValueTo = p.ValueTo;
                       
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


                        _unitofwork.PromoBillValueBasedGetYProductRepository.Insert(promotion);

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

        public List<InvBillValueDiscount> GetAllBillValueDiscounts(int companyid)
        {
            return _unitofwork.InvBillValueDiscountRepository.Get(i => i.CompanyID == companyid).ToList();
        }

        public InvBillValueDiscount GetBillValueDiscountsById(int companyid,int id)
        {
            return _unitofwork.InvBillValueDiscountRepository.Get(i => i.CompanyID == companyid && i.InvBillValueDiscountId==id).FirstOrDefault();
        }
        public int SubmitBillValueDiscountPromotion(InvBillValueDiscount invbillbaluediscount)
        {

            _unitofwork.CreateTransaction();
            {
                try
                {
                    if (invbillbaluediscount.IsExists == false)
                    {
                        _unitofwork.InvBillValueDiscountRepository.Insert(invbillbaluediscount);
                    }
                    else
                    {
                        var dbval = _unitofwork.InvBillValueDiscountRepository.GetById(invbillbaluediscount.InvBillValueDiscountId);
                        invbillbaluediscount.CreatedDate = dbval.CreatedDate;
                        invbillbaluediscount.CreatedUser = dbval.CreatedUser;

                        _unitofwork.InvBillValueDiscountRepository.UpdateBySet(dbval,invbillbaluediscount);
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

    }
}
