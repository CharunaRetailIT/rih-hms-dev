using HospitalityManagement.Controllers;
using HospitalityManagement.Models;
using HospitalityManagement.Models.Promotions;
using HospitalityManagement.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalityManagement.Service.Promotion
{
    class BillValueBasedGetYProductPromotionService
    {
        
        ApplicationDbContext context = new ApplicationDbContext();
        public List<Product> GetProducts()
        {
            var sysproducts = context.Product.Select(p => new
            {
                p.ProductCode,
                p.ProductId,
                p.ProductName,
                p.IsActive,
                p.IsRowMaterial
            }).Where(p => p.IsActive == true && p.IsRowMaterial == false).ToList().OrderBy(p => p.ProductCode);

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
            var servingunits = context.ProductServingUnit.Select(s => new
            {
                s.ProductServingUnitId,
                s.ServingUnit,
                s.ProductId,
                s.SellingPrice

            }).Where(p => p.ProductId == id).ToList().OrderBy(p => p.ServingUnit);

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

        public ProductStockMasterViewModel GetProductDetailsById(long id, int servingunitid)
        {

            var items = (
                           from p in context.Product
                           join ps in context.ProductServingUnit on p.ProductId equals ps.ProductId
                           join u in context.UnitOfMeasure on p.PurchasingUnit equals u.UnitOfMeasureId
                           where ps.ProductId == id && ps.ProductServingUnitId == servingunitid
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

        public int SubmitPromotion(List<ProductStockMasterViewModel> promotions)
        {
          
            using (var dbtransaction = context.Database.BeginTransaction())
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
                        promotion.CompanyID = 1;
                        promotion.LocationId = 1;
                        promotion.CreatedUser = "";
                        promotion.CreatedDate = DateTime.Now;
                        promotion.ModifiedDate = DateTime.Now;
                        promotion.ModifiedUser = "";
                        promotion.DataTransfer = 0;
                        promotion.Points = 0;
                       

                        context.InvPromoBillValueBasedGetYProduct.Add(promotion);

                    }

                    var res = context.SaveChanges();
                    dbtransaction.Commit();
                    return res;
                }
                catch (Exception e)
                {
                    dbtransaction.Rollback();
                    return 0;
                }
            }


        }



    }
}
