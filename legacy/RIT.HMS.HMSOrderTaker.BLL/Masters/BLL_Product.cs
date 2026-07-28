using RIT.HMS.HMSOrderTaker.Data;
using RIT.HMS.HMSOrderTaker.Domain;
using RIT.HMS.HMSOrderTaker.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.BLL.Masters
{
    public class BLL_Product
    {
        private UnitOfWork<SmartLinkEntities> unitOfWork;
        public BLL_Product()
        {
            unitOfWork = new UnitOfWork<SmartLinkEntities>();

        }

        public IEnumerable<DTO_Product> GetProductsByDeptCatId(long deptid, long catid)
        {
            try
            {
                var products = unitOfWork.Tbl_Product.Get(g => g.IsDelete == false && g.IsActive == true && g.DepartmentId == deptid &&
                                                                    g.CategoryId == catid && g.IsRowMaterial == false).OrderBy(g => g.ProductCode);


                List<DTO_Product> objproducts = new List<DTO_Product>();
                foreach (var prd in products)
                {
                    DTO_Product objproduct = new DTO_Product()
                    {
                        ProductId = prd.ProductId,
                        ProductCode = prd.ProductCode,
                        ProductName = prd.ProductName,
                        ProductImage = prd.ProductImage,
                        ProductImageName = prd.ProductImageName,
                        ProductImageType = prd.ProductImageType,
                       
                    };
                    objproducts.Add(objproduct);

                }

                return objproducts;


            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public DTO_Product GetProductsByProductId(long productid)
        {
            try
            {
                var product = unitOfWork.Tbl_Product.Get(g => g.ProductId==productid).FirstOrDefault();
            
               
                    DTO_Product objproduct = new DTO_Product()
                    {
                        ProductId = product.ProductId,
                        ProductCode = product.ProductCode,
                        ProductName = product.ProductName,
                        ProductImage = product.ProductImage,
                        ProductImageName = product.ProductImageName,
                        ProductImageType = product.ProductImageType,
                    };

                return objproduct;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public DTO_Product GetProductsByProductIdServingUnitId(long productid,int servingunitid,int companyid,int locationid)
        {
            try
            {
                var product = (from psu in unitOfWork.Tbl_ProductServingUnit.Get(g => g.ProductId == productid &&
                                                                   g.CompanyID == companyid && g.LocationId == locationid &&
                                                                   g.ProductServingUnitId == servingunitid) join
                                p in unitOfWork.Tbl_Product.Get(p => p.ProductId == productid) on psu.ProductId equals p.ProductId
                                select  new DTO_Product
                                {
                                    ProductId = p.ProductId,
                                    ProductCode = p.ProductCode,
                                    ProductName = p.ProductName,
                                    ProductImage = p.ProductImage,
                                    ProductImageName = p.ProductImageName,
                                    ProductImageType = p.ProductImageType,
                                    CostPrice=psu.CostPrice,
                                    SellingPrice=psu.SellingPrice
                                }).FirstOrDefault();


                //DTO_Product objproduct = new DTO_Product()
                //{
                //    ProductId = product.ProductId,
                //    ProductCode = product.ProductCode,
                //    ProductName = product.ProductName,
                //    ProductImage = product.ProductImage,
                //    ProductImageName = product.ProductImageName,
                //    ProductImageType = product.ProductImageType,
                //};

                return product;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public  List<DTO_Product> GetProductServingUnitsByProductIdLocationID(long productid,int locationid)
        {
            try
            {
                var product = unitOfWork.Tbl_ProductServingUnit.Get(g => g.ProductId == productid && g.LocationId==locationid).ToList();
                List<DTO_Product> servingunits = new List<DTO_Product>();
                foreach (var p in product)
                {
                    DTO_Product objproduct = new DTO_Product()
                    {
                        ProductId = (Int32)p.ProductId,
                        ProductServingUnitId= (Int32)p.ProductServingUnitId,
                        ServingUnit=p.ServingUnit,                     
                    };
                    servingunits.Add(objproduct);
                }
                return servingunits;
            }
            catch (Exception ex)
            {

                throw;
            }
        }





       
    }
}
