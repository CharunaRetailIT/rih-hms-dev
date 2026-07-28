using HospitalityManagement.Models;
using HospitalityManagement.Models.Transactions;
using HospitalityManagement.Models.ViewModels;
using HospitalityManagement.Models.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service.Transactions
{
    public class ProductionNoteService
    {
        LocationService _locationService = new LocationService();
        ProductService _productService = new ProductService();

        ApplicationDbContext context = new ApplicationDbContext();
        public List<ProductionViewModel> GetRecepieByProductId(long productid, long locationid,
                                                                decimal qty,long servingunitid)
        {
            try
            {
                List<ProductionViewModel> vvm = new List<ProductionViewModel>();
                ProductService productService = new ProductService();
                var receipe = (
                           from r in context.Receipe
                           join ps in context.ProductStockMaster on r.MaterialId equals ps.ProductId
                           join p in context.Product on ps.ProductId equals p.ProductId
                           join u in context.UnitOfMeasure on p.PurchasingUnit equals u.UnitOfMeasureId 
                           where r.ProductId == productid && ps.LocationId == locationid && 
                           r.ProductServingUnitId== servingunitid && r.ProductQty==1

                           orderby ps.ProductName
                           select new
                           {
                               MaterialId = r.MaterialId,
                               MaterialName = ps.ProductName,
                               MaterialCost = ps.CostPrice * qty,
                               MaterialSelling = ps.SellingPrice * qty,
                               MaterialQuantity = r.Quantity * qty,
                               MaterialCode = ps.ProductCode,
                               MaterialDbStock = ps.Stock,
                               LocationId = ps.LocationId,
                               UnitOfMeasureId=p.PurchasingUnit,
                               UnitOfMeasureName=u.UnitOfMeasureName
                           }
                       ).ToList();

                        foreach (var mat in receipe)
                        {
                            ProductionViewModel vm = new ProductionViewModel();
                            vm.MaterialId = mat.MaterialId;
                            vm.MaterialName = mat.MaterialName;
                            vm.MaterialCostPrice = mat.MaterialCost;
                            vm.MaterialSellingPrice = mat.MaterialSelling;
                            vm.MaterialQuantity = mat.MaterialQuantity;
                            vm.MaterialDbStock = mat.MaterialDbStock;
                            vm.MaterialCode = mat.MaterialCode;
                            vm.LocationId = mat.LocationId;
                            vm.MaterialUOMId = mat.UnitOfMeasureId;
                            vm.MaterialUOMName = mat.UnitOfMeasureName;
                            vvm.Add(vm);
                            
                        }

                var product = (

                          from ps in context.ProductStockMaster
                          join p in  context.Product on ps.ProductId equals p.ProductId
                          join u in context.UnitOfMeasure on p.PurchasingUnit equals u.UnitOfMeasureId
                          where ps.ProductId==productid && ps.LocationId==locationid
                           
                           select new
                           {
                               ProductId = ps.ProductId,
                               ProductName = ps.ProductName,
                               ProductCost = ps.CostPrice,
                               ProductSelling = ps.SellingPrice,
                               ProductQuantity = ps.Stock,
                               ProductCode = ps.ProductCode,
                               LocationId= ps.LocationId,
                               UnitOfMeasureId = p.PurchasingUnit,
                               UnitOfMeasureName = u.UnitOfMeasureName
                           }
                       ).ToList();

                        foreach (var prd in product)
                        {
                            ProductionViewModel vp = new ProductionViewModel();
                            vp.ProductId = prd.ProductId;
                            vp.ProductName = prd.ProductName;
                            vp.ProductCostPrice =prd.ProductCost;
                            vp.ProductSellingPrice = prd.ProductSelling;
                          //vp.ProductQuantity = prd.ProductQuantity;
                            vp.ProductQuantity = prd.ProductQuantity;
                            vp.ProductCode = prd.ProductCode;
                            vp.LocationId = prd.LocationId;
                            vp.ProductUOMId = prd.UnitOfMeasureId;
                            vp.ProductUOMName = prd.UnitOfMeasureName;
                            vvm.Add(vp);
                        }
                return vvm;

            }
            catch (Exception)
            { 
                throw;
            }
        }

        public List<ProductionViewModel> GetDefinedRecepieByProductId(long productid, long locationid,
                                                              decimal qty, long servingunitid)
        {
            try
            {
                List<ProductionViewModel> vvm = new List<ProductionViewModel>();
                ProductService productService = new ProductService();

                var dbrecipe = (
                           from r in context.Receipe
                           join ps in context.ProductStockMaster on r.MaterialId equals ps.ProductId
                           join p in context.Product on ps.ProductId equals p.ProductId
                           join u in context.UnitOfMeasure on p.PurchasingUnit equals u.UnitOfMeasureId
                           where r.ProductId == productid && ps.LocationId == locationid &&
                           r.ProductServingUnitId == servingunitid
                           && r.ProductQty == qty
                           orderby ps.ProductName
                           select new
                           {
                               MaterialId = r.MaterialId,
                               MaterialName = ps.ProductName,
                               MaterialCost = ps.CostPrice,
                               MaterialSelling = ps.SellingPrice,
                               MaterialQuantity = r.Quantity,
                               MaterialCode = ps.ProductCode,
                               MaterialDbStock = ps.Stock,
                               LocationId = ps.LocationId,
                               UnitOfMeasureId = p.PurchasingUnit,
                               UnitOfMeasureName = u.UnitOfMeasureName
                           }
                           ).ToList();


                var receipe = (
                           from r in context.Receipe
                           join ps in context.ProductStockMaster on r.MaterialId equals ps.ProductId
                           join p in context.Product on ps.ProductId equals p.ProductId
                           join u in context.UnitOfMeasure on p.PurchasingUnit equals u.UnitOfMeasureId
                           where r.ProductId == productid && ps.LocationId == locationid &&
                           r.ProductServingUnitId == servingunitid 
                           //&& r.ProductQty == qty
                           orderby ps.ProductName
                           select new
                           {
                               MaterialId = r.MaterialId,
                               MaterialName = ps.ProductName,
                               MaterialCost = ps.CostPrice*qty,
                               MaterialSelling = ps.SellingPrice*qty,
                               MaterialQuantity = r.Quantity*qty,
                               MaterialCode = ps.ProductCode,
                               MaterialDbStock = ps.Stock,
                               LocationId = ps.LocationId,
                               UnitOfMeasureId = p.PurchasingUnit,
                               UnitOfMeasureName = u.UnitOfMeasureName
                           }
                       ).ToList();

                if (dbrecipe.Count() == 0)
                {
                    foreach (var mat in receipe)
                    {
                        ProductionViewModel vm = new ProductionViewModel();
                        vm.MaterialId = mat.MaterialId;
                        vm.MaterialName = mat.MaterialName;
                        vm.MaterialCostPrice = mat.MaterialCost;
                        vm.MaterialSellingPrice = mat.MaterialSelling;
                        vm.MaterialQuantity = mat.MaterialQuantity;
                        vm.MaterialDbStock = mat.MaterialDbStock;
                        vm.MaterialCode = mat.MaterialCode;
                        vm.LocationId = mat.LocationId;
                        vm.MaterialUOMId = mat.UnitOfMeasureId;
                        vm.MaterialUOMName = mat.UnitOfMeasureName;
                        vvm.Add(vm);

                    }
                }
                else if (dbrecipe.Count != 0)
                {
                    foreach (var mat in dbrecipe)
                    {
                        ProductionViewModel vm = new ProductionViewModel();
                        vm.MaterialId = mat.MaterialId;
                        vm.MaterialName = mat.MaterialName;
                        vm.MaterialCostPrice = mat.MaterialCost;
                        vm.MaterialSellingPrice = mat.MaterialSelling;
                        vm.MaterialQuantity = mat.MaterialQuantity;
                        vm.MaterialDbStock = mat.MaterialDbStock;
                        vm.MaterialCode = mat.MaterialCode;
                        vm.LocationId = mat.LocationId;
                        vm.MaterialUOMId = mat.UnitOfMeasureId;
                        vm.MaterialUOMName = mat.UnitOfMeasureName;
                        vvm.Add(vm);

                    }
                }

                var product = (

                          from ps in context.ProductStockMaster
                          join p in context.Product on ps.ProductId equals p.ProductId
                          // join u in context.UnitOfMeasure on p.PurchasingUnit equals u.UnitOfMeasureId
                          where ps.ProductId == productid && ps.LocationId == locationid

                          select new
                          {
                              ProductId = ps.ProductId,
                              ProductName = ps.ProductName,
                              ProductCost = ps.CostPrice,
                              ProductSelling = ps.SellingPrice,
                              ProductQuantity = ps.Stock,
                              ProductCode = ps.ProductCode,
                              LocationId = ps.LocationId,
                              UnitOfMeasureId = p.PurchasingUnit,
                              //  UnitOfMeasureName = u.UnitOfMeasureName
                              UnitOfMeasureName = ""
                          }
                       ).ToList();

                foreach (var prd in product)
                {
                    ProductionViewModel vp = new ProductionViewModel();
                    vp.ProductId = prd.ProductId;
                    vp.ProductName = prd.ProductName;
                    vp.ProductCostPrice = prd.ProductCost;
                    vp.ProductSellingPrice = prd.ProductSelling;
                    //vp.ProductQuantity = prd.ProductQuantity;
                    vp.ProductQuantity = prd.ProductQuantity;
                    vp.ProductCode = prd.ProductCode;
                    vp.LocationId = prd.LocationId;
                    vp.ProductUOMId = prd.UnitOfMeasureId;
                    vp.ProductUOMName = prd.UnitOfMeasureName;
                    vvm.Add(vp);
                }
                return vvm;

            }
            catch (Exception)
            {
                throw;
            }
        }


        public List<Receipe> CheckReceipe(long productid, long locationid,
                                                              decimal qty, long servingunitid)
        {
            try
            {
                List<Receipe> receipe = context.Receipe.Where(r=>r.ProductServingUnitId== servingunitid &&
                                                                    r.ProductId== productid && 
                                                                    r.ProductQty== qty).ToList();
                if (receipe != null)
                {
                    return receipe;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<ProductionNoteHeader> GetActiveProductions()
        {
            try
            {
                IEnumerable<ProductionNoteHeader> productions = context.ProductionNoteHeader.OrderBy(g => g.CreatedDate);
                if (productions != null)
                {
                    return productions;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<ProductionNoteDetail> GetProductionNoteDetailByHeaderId(long id)
        {
            try
            {
                IEnumerable<ProductionNoteDetail> productions = context.ProductionNoteDetail.Where(g => g.ProductionNoteHeaderId==id
                                                                                                   && g.ProductQty!=0);
                if (productions != null)
                {
                    return productions;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ProductionNoteHeader GetActiveProductionsById(long id)
        {
            try
            {
                ProductionNoteHeader productions = context.ProductionNoteHeader.
                                                                Where(p=>p.ProductionNoteHeaderId==id).
                                                                OrderBy(g => g.CreatedDate).FirstOrDefault();
                if (productions != null)
                {
                    return productions;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<ProductionNoteDetail> GetActiveProductionDetById(long id)
        {
            try
            {
                List<ProductionNoteDetail> productionsdet = context.ProductionNoteDetail.Where(p => p.ProductionNoteHeaderId == id).ToList();
                                                                
                if (productionsdet != null)
                {
                    return productionsdet;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public bool SubmitProduction(ProductionNoteHeader prdheader)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {
                   
                    context.ProductionNoteHeader.Add(prdheader);

                    if (context.SaveChanges() == 1)
                    {
                       
                        foreach (var detail in prdheader.ProductionDetail)
                        {
                            var materialstock = context.ProductStockMaster.Where(s=>s.ProductId==detail.MaterialId && 
                                                                            s.LocationId==prdheader.ProductionLocId).First();
                            materialstock.Stock -= detail.MaterialQty;
                            materialstock.DocumentNo = prdheader.DocumentNo;
                            materialstock.LastUpdatedDate = DateTime.Now;
                            ProductionNoteDetail pdetail = new ProductionNoteDetail();
                            pdetail.ProductionNoteHeaderId = prdheader.ProductionNoteHeaderId;
                            pdetail.MaterialId = detail.MaterialId;
                            pdetail.MaterialName = detail.MaterialName;
                            pdetail.MaterialQty = detail.MaterialQty;
                            pdetail.SellingPrice = detail.SellingPrice;
                            pdetail.CostPrice = detail.CostPrice;
                            pdetail.AvgCost = materialstock.AvgCost;
                            
                            context.ProductionNoteDetail.Add(pdetail);
                        }
                        //if (context.SaveChanges() == prdheader.ProductionDetail.Count)
                        //{
                            context.SaveChanges();
                            var productstock = context.ProductStockMaster.Where(s => s.ProductId == prdheader.ProductId &&
                                                                              s.LocationId == prdheader.ProductionLocId).First();


                            productstock.Stock += prdheader.ProductQty;
                            productstock.DocumentNo = prdheader.DocumentNo;
                            productstock.LastUpdatedDate = DateTime.Now;




                            if (context.SaveChanges() == 1)
                            {
                                dbtransaction.Commit();
                            }
                            else
                            {
                                dbtransaction.Rollback();
                                return false;
                            }
                        //}
                        //else
                        //{
                        //    dbtransaction.Rollback();
                        //    return false;
                        //}

                        
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


        public bool SubmitMaltipleProduction(ProductionNoteHeader prdheader)
         {
            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {

                    context.ProductionNoteHeader.Add(prdheader);

                    if (context.SaveChanges() == 1)
                    {

                        foreach (var detail in prdheader.ProductionDetail)
                        {
                            var materialstock = context.ProductStockMaster.Where(s => s.ProductId == detail.MaterialId &&
                                                                            s.LocationId == prdheader.ProductionLocId).First();
                            materialstock.Stock -= detail.MaterialQty;
                            materialstock.DocumentNo = prdheader.DocumentNo;
                            materialstock.LastUpdatedDate = DateTime.Now;
                            ProductionNoteDetail pdetail = new ProductionNoteDetail();
                            pdetail.ProductionNoteHeaderId = prdheader.ProductionNoteHeaderId;
                            pdetail.MaterialId = detail.MaterialId;
                            pdetail.MaterialName = detail.MaterialName;
                            pdetail.MaterialQty = detail.MaterialQty;
                            pdetail.SellingPrice = detail.SellingPrice;
                            pdetail.CostPrice = detail.CostPrice;
                            pdetail.AvgCost = materialstock.AvgCost;
                            pdetail.ProductId = detail.ProductId;
                            pdetail.ProductQty = detail.ProductQty;
                            pdetail.ProductCostPrice = detail.ProductCostPrice;
                            pdetail.ProductSellingPrice = detail.ProductSellingPrice;
                            context.ProductionNoteDetail.Add(pdetail);
                          
                            if (detail.ProductQty != 0)
                            {
                                var productstock = context.ProductStockMaster.Where(s => s.ProductId == detail.ProductId &&
                                                                                    s.LocationId == prdheader.ProductionLocId).First();


                                productstock.Stock += detail.ProductQty;
                                productstock.DocumentNo = prdheader.DocumentNo;
                                productstock.LastUpdatedDate = DateTime.Now;
                            }
                           

                        }

                        context.SaveChanges();
                        dbtransaction.Commit();
                        return true;
                        


                    }
                    else
                    {
                        dbtransaction.Rollback();
                        return false;
                    }

                   
                }
                catch (Exception)
                {
                    dbtransaction.Rollback();
                    return false;

                }
            }

        }
        
        public int DeleteProductionDetailByHeaderId(long id)
        {
            try
            {
                context.ProductionNoteDetail.RemoveRange(context.ProductionNoteDetail.Where(x => x.ProductionNoteHeaderId == id));
                var res = context.SaveChanges();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool EditProduction(ProductionNoteHeader prdheader, ProductionNoteHeader oldprdheader)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {
                    if (DeleteProductionDetailByHeaderId(prdheader.ProductionNoteHeaderId)!=0)
                    {


                        var newproduction = GetActiveProductionsById(prdheader.ProductionNoteHeaderId);
                        //  dbproduction.ProductionDetail = GetActiveProductionDetById(prdheader.ProductionNoteHeaderId);
                        var productstock = context.ProductStockMaster.Where(s => s.ProductId == prdheader.ProductId &&
                                                                             s.LocationId == prdheader.ProductionLocId).First();
                        productstock.Stock -= oldprdheader.ProductQty;
                        productstock.Stock += prdheader.ProductQty;
                      


                        newproduction.ProductId = prdheader.ProductId;
                        newproduction.DocumentNo = prdheader.DocumentNo;
                        newproduction.ProductQty = prdheader.ProductQty;
                        newproduction.ProductCostPrice = prdheader.ProductCostPrice;
                        newproduction.ProductSellingPrice = prdheader.ProductSellingPrice;
                        newproduction.IsTempPN = newproduction.IsTempPN;
                        newproduction.Remark = prdheader.Remark;
                        newproduction.ProductionLocId = newproduction.ProductionLocId;
                        newproduction.ModifiedUser = "1";
                        newproduction.ModifiedDate = DateTime.Now;

                        
                        //if (context.SaveChanges() == 1)
                        //{

                        foreach (var detail in prdheader.ProductionDetail)
                            {
                                var materialstock = context.ProductStockMaster.Where(s => s.ProductId == detail.MaterialId &&
                                                                                s.LocationId == prdheader.ProductionLocId).First();

                                var itemToaddstock = oldprdheader.ProductionDetail.Single(r => r.MaterialId == detail.MaterialId);
                                if (itemToaddstock != null)
                                    materialstock.Stock += itemToaddstock.MaterialQty;
                                materialstock.Stock -= detail.MaterialQty;
                                ProductionNoteDetail pdetail = new ProductionNoteDetail();
                                pdetail.ProductionNoteHeaderId = prdheader.ProductionNoteHeaderId;
                                pdetail.MaterialId = detail.MaterialId;
                                pdetail.MaterialName = detail.MaterialName;
                                pdetail.MaterialQty = detail.MaterialQty;
                                pdetail.SellingPrice = detail.SellingPrice;
                                pdetail.CostPrice = detail.CostPrice;
                                context.ProductionNoteDetail.Add(pdetail);
                            }

                           // context.SaveChanges();
                            //var productstock = context.ProductStockMaster.Where(s => s.ProductId == prdheader.ProductId &&
                            //                                                  s.LocationId == prdheader.ProductionLocId).First();
                            //productstock.Stock += prdheader.ProductQty;
                            //productstock.Stock -= oldprdheader.ProductQty;

                            if (context.SaveChanges() != 0)
                            {
                                dbtransaction.Commit();
                                return true;
                            }
                            else
                            {
                                dbtransaction.Rollback();
                                return false;
                            }

                        //}
                        //else
                        //{
                        //    dbtransaction.Rollback();
                        //    return false;
                        //}
                    }
                    else
                    {
                        dbtransaction.Rollback();
                        return false;
                    }
                   
                }
                catch (Exception ex)
                {
                    dbtransaction.Rollback();
                    return false;

                }
            }

        }

        public IEnumerable<ProductionNoteHeader> GetPeoductionsByLocId(long locid,DateTime datefrom,DateTime dateto)
        {
            DateTime date1 = datefrom.Date;
            DateTime date2 = dateto.Date;

            return context.ProductionNoteHeader.Where(p=>p.LocationId==locid &&
                                                         DbFunctions.TruncateTime(p.CreatedDate)>=DbFunctions.TruncateTime(date1.Date) &&
                                                         DbFunctions.TruncateTime(p.CreatedDate)<= DbFunctions.TruncateTime(date2.Date)
                                                         );

           
        }

        public List<ProductionReportViewModel> GetProductions(long locid, DateTime datefrom, DateTime dateto)
        {
            DateTime date1 = datefrom.Date;
            DateTime date2 = dateto.Date.AddDays(1);
            List<ProductionReportViewModel> reportdata = new List<ProductionReportViewModel>();

          


           var  productionhead = ( 
                         from ph in context.ProductionNoteHeader
                         join pd in context.ProductionNoteDetail on ph.ProductionNoteHeaderId equals pd.ProductionNoteHeaderId
                         join p in context.Product on pd.ProductId equals p.ProductId 
                         join l in context.SysLocations on ph.LocationId equals l.SysLocationID                        
                         where  pd.ProductQty != 0
                         && ph.CreatedDate >= date1 && ph.CreatedDate <= date2
                         && ph.ProductionLocId==locid
                         orderby ph.DocumentNo 
                         select new
                         {
                             HeaderId=ph.ProductionNoteHeaderId,                       
                             Location = l.LocationName,
                             DocNo=ph.DocumentNo,
                             Date=ph.CreatedDate

                            
                         }
                     ).ToList().Distinct();




            foreach (var item in productionhead)
            {
               var vm = new ProductionReportViewModel();
               
                vm.HeaderId = item.HeaderId;
                vm.Date = item.Date.ToShortDateString();
                vm.DocNo = item.DocNo;
                vm.Location = item.Location;  

                foreach (var s in context.ProductionNoteDetail.Where(r => r.ProductionNoteHeaderId == item.HeaderId && r.ProductQty !=0))
                {
                    ProductionReportViewModel.ProductionDetail det = new ProductionReportViewModel.ProductionDetail();
                    det.ProductId = s.ProductId;
                    det.ProductName = _productService.GetProductById(det.ProductId).ProductName;
                    det.ProductQuantity = s.ProductQty;
                    det.ProductCostPrice = s.ProductCostPrice;
                    det.ProductSellingPrice = s.ProductSellingPrice;
                    vm.ProductionDetailReport.Add(det);

                }

                
                reportdata.Add(vm);



            }

            return reportdata;


        }
    }
}