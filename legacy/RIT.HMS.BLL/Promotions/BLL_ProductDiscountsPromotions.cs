using RIT.HMS.Data;
using RIT.HMS.Domain.Promotions;
using RIT.HMS.Domain.ViewModels.Promotions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.Promotions
{
    public class BLL_ProductDiscountsPromotions
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_ProductDiscountsPromotions()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_ProductDiscountsPromotions(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public int SubmitProductDiscounts(Int32 deptid, Int32 catid, Int32 subcatid, Int32 productid, long servingunitid,
                                         decimal FromQty, decimal ToQty, decimal Rate,long Points, decimal DiscountAmt,
                                         decimal DiscountPrc, int promomasterid,int companyid)
        {
            var products = (from p in _unitofwork.ProductRepository.Get(p=>p.IsActive == true && p.IsDelete == false)
                            join psu in _unitofwork.ProductServingUnitRepository.Get()
                            on p.ProductId equals psu.ProductId
                            where p.CompanyID==companyid && psu.CompanyID==companyid
                            //where (p.DepartmentId = deptid || p.CategoryId=catid || p.SubCategoryId=subcatid || p.ProductId=productid)
                            select new
                            {
                                ProductId = p.ProductId,
                                ServingUnitId = psu.ProductServingUnitId,
                                DepartmentId = p.DepartmentId,
                                CategoryId = p.CategoryId,
                                SubCategoryId = p.SubCategoryId,
                                UnitOfMeasureId = p.PurchasingUnit
                            }
                          ).ToList();

            List<InvPromotionDetailsProductDis> productdiscounts= new List<InvPromotionDetailsProductDis>();                  
          
                foreach (var p in products)
                {
                    InvPromotionDetailsProductDis productdiscount = new InvPromotionDetailsProductDis();
                    productdiscount.ProductID = p.ProductId;
                    productdiscount.ServingUnitId = Convert.ToInt32(p.ServingUnitId);
                    productdiscount.InvPromotionMasterID = promomasterid;
                    productdiscount.UnitOfMeasureID = p.UnitOfMeasureId;
                    productdiscount.CompanyID = companyid;
                    productdiscount.LocationID = 1;
                    productdiscount.Rate = Rate ;
                    productdiscount.FromQty = FromQty;
                    productdiscount.ToQty = ToQty;
                    productdiscount.DiscountPercentage = DiscountPrc;
                    productdiscount.DiscountAmount = DiscountAmt;
                    productdiscount.Points = Points;

                    productdiscount.DepartmentId = p.DepartmentId;
                    productdiscount.CategoryId = p.CategoryId;
                    productdiscount.SubCategoryId = p.SubCategoryId;
                    productdiscount.ProductID = p.ProductId;

                    productdiscounts.Add(productdiscount);

                        if (_unitofwork.InvPromotionDetailsProductDisRepository.Get(d=>d.ProductID==p.ProductId &&
                                                                        d.ServingUnitId== servingunitid).Any())
                       {
                        _unitofwork.InvPromotionDetailsProductDisRepository.DeleteRange(_unitofwork.InvPromotionDetailsProductDisRepository.Get(d => d.ProductID == p.ProductId &&
                                                                        d.ServingUnitId == servingunitid));

                       }

                }

          //  int k = 0;

            var sss = productdiscounts.Where(d => d.DepartmentId == deptid && d.CategoryId == catid && d.SubCategoryId == subcatid).ToList();

            if (deptid != 0 && catid == 0 && subcatid == 0 && productid == 0)
            {             
                _unitofwork.InvPromotionDetailsProductDisRepository.BulkInsert(productdiscounts.Where(d => d.DepartmentId == deptid).ToList());
            }
            else if (deptid != 0 && catid != 0 && subcatid == 0 && productid == 0)
            {
                _unitofwork.InvPromotionDetailsProductDisRepository.BulkInsert(productdiscounts.Where(d => d.DepartmentId == deptid && d.CategoryId == catid).ToList());
            }
            else if (deptid != 0 && catid != 0 && subcatid != 0 && productid == 0)
            {
                _unitofwork.InvPromotionDetailsProductDisRepository.BulkInsert(productdiscounts.Where(d =>  d.DepartmentId == deptid && d.CategoryId == catid && d.SubCategoryId == subcatid).ToList());
            }
            else if (deptid != 0 && catid != 0 && subcatid != 0 && productid != 0)
            {
                _unitofwork.InvPromotionDetailsProductDisRepository.BulkInsert(productdiscounts.Where(d => d.DepartmentId == deptid && d.CategoryId == catid && d.SubCategoryId == subcatid && d.ProductID == productid).ToList());
            }
            return _unitofwork.Save();

        }

        public List<VMProductDiscounts> ProductDiscounts(int companyid)
        {
            var res = (from pd in _unitofwork.InvPromotionDetailsProductDisRepository.Get()
                       join p in _unitofwork.ProductRepository.Get() on pd.ProductID equals p.ProductId
                       join s in _unitofwork.ProductServingUnitRepository.Get() on pd.ServingUnitId equals s.ProductServingUnitId
                       join u in _unitofwork.UnitOfMeasureRepository.Get() on p.PurchasingUnit equals u.UnitOfMeasureId
                       where p.CompanyID== companyid && s.CompanyID==companyid && u.CompanyID==companyid
                       orderby p.ProductCode
                       select new
                       {
                           pd.InvPromotionDetailsProductDisId,
                           pd.InvPromotionMasterID,
                           pd.CompanyID,
                           pd.LocationID,
                           pd.ProductID,
                           pd.ServingUnitId,
                           pd.UnitOfMeasureID,
                           pd.Rate,
                           pd.FromQty,
                           pd.ToQty,
                           pd.Points,
                           pd.DiscountPercentage,
                           pd.DiscountAmount,
                           p.ProductName,
                           s.ServingUnit,
                           u.UnitOfMeasureName,
                           p.PurchasingUnit,
                           p.ProductCode
                       }
                    ).ToList();

            List<VMProductDiscounts> productdiscounts = new List<VMProductDiscounts>();
            foreach (var p in res)
            {
                VMProductDiscounts productdiscount = new VMProductDiscounts();
                productdiscount.InvPromotionDetailsProductDisId = p.InvPromotionDetailsProductDisId;
                productdiscount.InvPromotionMasterID = p.InvPromotionDetailsProductDisId;
                productdiscount.CompanyID = p.CompanyID;
                productdiscount.LocationID = p.LocationID;
                productdiscount.ProductID = p.ProductID;
                productdiscount.ProductCode = p.ProductCode;
                productdiscount.ServingUnitId = p.ServingUnitId;
                productdiscount.UnitOfMeasureID = p.UnitOfMeasureID;
                productdiscount.Rate = p.Rate;
                productdiscount.FromQty = p.FromQty;
                productdiscount.ToQty = p.ToQty;
                productdiscount.Points = p.Points;
                productdiscount.DiscountPercentage = p.DiscountPercentage;
                productdiscount.DiscountAmount = p.DiscountAmount;
                productdiscount.ProductName = p.ProductName;
                productdiscount.ServingUnit = p.ServingUnit;
                productdiscount.UOM = p.UnitOfMeasureName;
                productdiscounts.Add(productdiscount);

            }

            return productdiscounts == null ? null : productdiscounts;

        }

        public int RemoveProductDiscount(long rowid)
        {
            _unitofwork.InvPromotionDetailsProductDisRepository.Delete(rowid);
           return  _unitofwork.Save();
        }

    }
 }
