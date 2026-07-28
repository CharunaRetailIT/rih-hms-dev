using HospitalityManagement.Models;
using HospitalityManagement.Models.Promotions;
using HospitalityManagement.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service.Promotion
{
    public class ItemToItemPromotionService
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
            }).Where(p => p.IsActive == true && p.IsRowMaterial == false).ToList().OrderBy(p=>p.ProductCode);

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

        public ProductStockMasterViewModel GetProductDetailsById(long id,int servingunitid)
        {
           
            var items = (
                           from p in context.Product
                           join ps in context.ProductServingUnit on p.ProductId equals ps.ProductId
                           join u in context.UnitOfMeasure on p.PurchasingUnit equals u.UnitOfMeasureId
                           where ps.ProductId == id && ps.ProductServingUnitId==servingunitid                         
                           orderby p.ProductName
                           select new
                           {
                               ProductId = p.ProductId,
                               ProductCode=p.ProductCode,
                               ProductName = p.ProductName,
                               Cost = ps.CostPrice,
                               Selling = ps.SellingPrice,
                               UOM=u.UnitOfMeasureName,
                               UOMId=p.PurchasingUnit,
                               ServingUnit=ps.ServingUnit,
                               ServingUnitId=ps.ProductServingUnitId
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
                    vm.UOMId=items.UOMId;
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
                    int promotionmasterid = promotions.FirstOrDefault().PromotionMasterId;
                    //var exists = context.InvPromotionDetailsBuyXProduct.Any(p => p.InvPromotionMasterId == promotionmasterid);
                    //if (exists)
                    //{
                    //    context.InvPromotionDetailsBuyXProduct.RemoveRange(context.InvPromotionDetailsBuyXProduct.Where(p => p.InvPromotionMasterId == promotionmasterid));
                    //}

                    int maxgroupid = 0;
                    var a = context.InvPromotionDetailsBuyXProduct.ToList();
                    if (a.Count!=0)
                    {
                        maxgroupid = context.InvPromotionDetailsBuyXProduct.Max(g => g.GroupId);
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

                        } else if (p.DiscountType == "Prc")
                        {
                            promotion.DiscountAmount = 0;
                            promotion.DiscountPercentage = p.DiscountAmt;

                        } else if (p.DiscountType=="N")
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
                        promotion.GroupId = maxgroupid;
                        context.InvPromotionDetailsBuyXProduct.Add(promotion);

                    }

                    var res =  context.SaveChanges();
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

        public List<InvPromotionMaster> GetPromotionMasters()
        {
            return context.InvPromotionMaster.Where(p => p.IsDelete == false ).ToList();
        }

        public InvPromotionMaster GetPromotionMasterById(int id)
        {
            return context.InvPromotionMaster.Where(p => p.IsDelete == false && p.InvPromotionMasterID==id).FirstOrDefault();

           // var promotion=
        }

        public List<ProductStockMasterViewModel> MyProperty(int promotionmasterid)
        {
            var items = (
                          from p in context.InvPromotionDetailsBuyXProduct
                          join pm in context.InvPromotionMaster on p.InvPromotionMasterId equals pm.InvPromotionMasterID
                          join pp in context.Product on p.ProductId equals pp.ProductId
                          where p.InvPromotionMasterId == promotionmasterid 
                          //&& p.ProductType==2
                          orderby pp.ProductName
                          select new
                          {
                              ProductId = p.ProductId,
                              ProductCode = pp.ProductCode,
                              ProductName = pp.ProductName,
                              Cost = 0,
                              Selling = p.Rate,
                              Qty = p.Qty,
                              DiscountAmount = p.DiscountAmount,
                              DiscountPrc=p.DiscountPercentage,
                              UOM = context.UnitOfMeasure.Where(u => u.UnitOfMeasureId == pp.PurchasingUnit).FirstOrDefault().UnitOfMeasureName,
                              UOMId = p.BuyUnitOfMeasureId,
                              ServingUnit = context.ProductServingUnit.Where(p => p.ProductServingUnitId == p.ProductServingUnitId).FirstOrDefault().ServingUnit,
                              ServingUnitId = p.ServingUnitId,
                              PromotionMasterId=p.InvPromotionMasterId,
                              PromotionTypeId=pm.PromotionTypeID,
                              PromotionName=pm.PromotionName,
                              ProductType=p.ProductType
                            
                          }
                      ).ToList();

            List<ProductStockMasterViewModel> vmm = new List<ProductStockMasterViewModel>();

            if (items != null && items.Count!=0)
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
                    if (vm.DiscountAmt != 0)
                    {
                        vm.DiscountAmt = i.DiscountAmount;
                        vm.DiscountType = "Amt";

                    }else if(vm.DiscountPrc !=0)
                    {
                        vm.DiscountAmt = i.DiscountPrc;
                        vm.DiscountType = "Prc";
                    }
                    vmm.Add(vm);
                }
                return vmm;
            }
            else if (items.Count == 0)
            {
                
                    ProductStockMasterViewModel vm = new ProductStockMasterViewModel();
                    var pm = context.InvPromotionMaster.Where(p => p.InvPromotionMasterID == promotionmasterid).FirstOrDefault();
                    vm.PromotionMasterId = (int)pm.InvPromotionMasterID;
                    vm.PromotionTypeId = pm.PromotionTypeID;
                    vm.PromotionName = pm.PromotionName;                  
                    vmm.Add(vm);
              
                    return vmm;

            }
            else
                return null;
        }

    }
}