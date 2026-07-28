using RIT.HMS.BLL.MasterData;
using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;


namespace RIT.HMS.BLL.TransactionData
{
    public class BLL_ProductionNote
    {
        private readonly BLL_Location _blllocation;
        private readonly BLL_Product _bllproduct;
        private readonly UnitOfWork _unitofwork;

        public BLL_ProductionNote()
        {
            _blllocation = new BLL_Location();
            _bllproduct = new BLL_Product();
            _unitofwork = new UnitOfWork();
        }
        public BLL_ProductionNote(string connectionstring)
        {
            _blllocation = new BLL_Location(connectionstring);
            _bllproduct = new BLL_Product(connectionstring);
            _unitofwork = new UnitOfWork(connectionstring);
        }
        public List<ProductionViewModel> GetRecepieByProductId(long productid, long locationid,
                                                              decimal qty, long servingunitid,int companyid)
        {
            try
            {
                List<ProductionViewModel> vvm = new List<ProductionViewModel>();

                var receipe = (
                           from r in _unitofwork.ReceipeRepository.Get()
                           join ps in _unitofwork.ProductStockMasterRepository.Get() on r.MaterialId equals ps.ProductId
                           join p in _unitofwork.ProductRepository.Get() on ps.ProductId equals p.ProductId
                           join u in _unitofwork.UnitOfMeasureRepository.Get() on p.PurchasingUnit equals u.UnitOfMeasureId
                           where r.ProductId == productid && ps.LocationId == locationid &&
                           r.ProductServingUnitId == servingunitid && r.ProductQty == 1 && p.CompanyID== companyid && ps.CompanyID== companyid &&r.CompanyID== companyid && u.CompanyID== companyid

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
                               UnitOfMeasureId = p.PurchasingUnit,
                               UnitOfMeasureName = u.UnitOfMeasureName
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
                    vm.ProductQuantity = 1;
                    vvm.Add(vm);

                }

                var product = (

                          from ps in _unitofwork.ProductStockMasterRepository.Get()
                          join p in _unitofwork.ProductRepository.Get() on ps.ProductId equals p.ProductId
                          join u in _unitofwork.UnitOfMeasureRepository.Get() on p.PurchasingUnit equals u.UnitOfMeasureId
                          where ps.ProductId == productid && ps.LocationId == locationid
                          && p.CompanyID == companyid && ps.CompanyID == companyid && u.CompanyID == companyid

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
                              UnitOfMeasureName = u.UnitOfMeasureName
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
                    vp.ProductQuantity = 1;
                    vvm.Add(vp);
                }
                return vvm;

            }
            catch (Exception)
            {
                throw;
            }
        }


        public List<ProductionViewModel> GetDefinedRecepieForProductions(long productid, long locationid,
                                                      decimal qty, long servingunitid, int companyid)
        {
            try
            {
                List<ProductionViewModel> vvm = new List<ProductionViewModel>();


                var dbrecipe = (
                           from r in _unitofwork.ReceipeRepository.Get()
                           join ps in _unitofwork.ProductStockMasterRepository.Get() on r.MaterialId equals ps.ProductId
                           join p in _unitofwork.ProductRepository.Get() on ps.ProductId equals p.ProductId
                           join u in _unitofwork.UnitOfMeasureRepository.Get() on p.PurchasingUnit equals u.UnitOfMeasureId
                           where r.ProductId == productid && ps.LocationId == locationid &&
                           r.ProductServingUnitId == servingunitid
                           && r.ProductQty == qty
                           && p.CompanyID == companyid && ps.CompanyID == companyid && r.CompanyID == companyid && u.CompanyID == companyid

                           orderby ps.ProductName
                           select new
                           {
                               MaterialId = r.MaterialId,
                               MaterialName = ps.ProductName,
                               //MaterialCost = ps.CostPrice,
                               //MaterialSelling = ps.SellingPrice,
                               MaterialCost = r.CostPrice,
                               MaterialSelling = r.SellingPrice,
                               MaterialQuantity = r.Quantity,
                               MaterialCode = ps.ProductCode,
                               MaterialDbStock = ps.Stock,
                               LocationId = ps.LocationId,
                               UnitOfMeasureId = p.PurchasingUnit,
                               UnitOfMeasureName = u.UnitOfMeasureName,
                               WeightPerUnit = p.WeightPerUnit,
                               ProductQty = r.ProductQty,
                               ProductName = p.ProductName
                           }
                           ).ToList();


                var receipe = (
                           from r in _unitofwork.ReceipeRepository.Get()
                           join ps in _unitofwork.ProductStockMasterRepository.Get() on r.MaterialId equals ps.ProductId
                           join p in _unitofwork.ProductRepository.Get() on ps.ProductId equals p.ProductId
                           join u in _unitofwork.UnitOfMeasureRepository.Get() on p.PurchasingUnit equals u.UnitOfMeasureId
                           where r.ProductId == productid && ps.LocationId == locationid &&
                           r.ProductServingUnitId == servingunitid
                           && r.ProductQty == 1
                           && p.CompanyID == companyid && ps.CompanyID == companyid && r.CompanyID == companyid && u.CompanyID == companyid

                           orderby ps.ProductName
                           select new
                           {
                               MaterialId = r.MaterialId,
                               MaterialName = ps.ProductName,

                               MaterialCost = r.CostPrice * qty,
                               MaterialSelling = r.SellingPrice * qty,
                               MaterialQuantity = r.Quantity * qty,

                               MaterialCode = ps.ProductCode,
                               MaterialDbStock = ps.Stock,
                               LocationId = ps.LocationId,
                               UnitOfMeasureId = p.PurchasingUnit,
                               WeightPerUnit = p.WeightPerUnit,
                               UnitOfMeasureName = u.UnitOfMeasureName,
                               ProductName = r.ProductName,
                               ProductQty = qty
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
                       
                        vm.MaterialDbStock = mat.MaterialDbStock;
                        vm.MaterialCode = mat.MaterialCode;
                        vm.LocationId = mat.LocationId;
                        vm.MaterialUOMId = mat.UnitOfMeasureId;
                        vm.MaterialUOMName = mat.UnitOfMeasureName;
                        vm.ProductQuantity = mat.ProductQty;
                        vm.SubUnitId = mat.WeightPerUnit;
                        vm.ProductName = mat.ProductName;
                        var subunit = _unitofwork.UnitConversionRepository.GetById(mat.WeightPerUnit);
                        if (subunit != null)
                        {
                            vm.SubUnitValue = subunit.SubUnitValue;
                            vm.SubUnitName = subunit.SubUnitSymbol;
                            vm.MaterialQuantity = mat.MaterialQuantity/vm.SubUnitValue;
                        }
                        else
                        {
                            vm.SubUnitValue = 1;
                            vm.SubUnitName = subunit.SubUnitSymbol;
                        }
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
                        vm.SubUnitId = mat.WeightPerUnit;
                        vm.ProductQuantity = mat.ProductQty;
                        vm.SubUnitId = mat.WeightPerUnit;
                        vm.ProductName = mat.ProductName;
                        var subunit = _unitofwork.UnitConversionRepository.GetById(mat.WeightPerUnit);
                        if (subunit != null)
                        {
                            vm.SubUnitValue = subunit.SubUnitValue;
                            vm.SubUnitName = subunit.SubUnitSymbol;
                            vm.MaterialQuantity = vm.MaterialQuantity / vm.SubUnitValue;

                            vm.MaterialCostPrice = mat.MaterialCost ;
                            vm.MaterialSellingPrice = mat.MaterialSelling;
                        }
                        else
                        {
                            vm.SubUnitValue = 1;
                            vm.SubUnitName = "n/a";
                        }
                        vvm.Add(vm);

                    }
                }

                var product = (

                          from ps in _unitofwork.ProductStockMasterRepository.Get()
                          join p in _unitofwork.ProductRepository.Get() on ps.ProductId equals p.ProductId
                          // join u in context.UnitOfMeasure on p.PurchasingUnit equals u.UnitOfMeasureId
                          where ps.ProductId == productid && ps.LocationId == locationid
                          && p.CompanyID == companyid && ps.CompanyID == companyid

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
                              WaightPerUnit = p.WeightPerUnit,

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
                    var subunit = _unitofwork.UnitConversionRepository.GetById(prd.WaightPerUnit);
                    if (subunit != null)
                    {
                        vp.SubUnitValue = subunit.SubUnitValue;
                        vp.SubUnitName = subunit.SubUnitSymbol;
                    }
                    else
                    {
                        vp.SubUnitValue = 1;
                        vp.SubUnitName = "n/a";
                    }
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
                                                              decimal qty, long servingunitid,int companyid)
        {
            try
            {
                List<ProductionViewModel> vvm = new List<ProductionViewModel>();


                var dbrecipe = (
                           from r in _unitofwork.ReceipeRepository.Get(r=> r.ProductId == productid && r.ProductServingUnitId == servingunitid && r.ProductQty == qty && r.CompanyID == companyid)
                           join ps in _unitofwork.ProductStockMasterRepository.Get(ps => ps.LocationId == locationid && ps.CompanyID == companyid) on r.MaterialId equals ps.ProductId
                           join p in _unitofwork.ProductRepository.Get(p=> p.CompanyID == companyid) on ps.ProductId equals p.ProductId
                           join u in _unitofwork.UnitOfMeasureRepository.Get(u=>u.CompanyID == companyid) on p.PurchasingUnit equals u.UnitOfMeasureId
                          // where 
                         //     r.ProductId == productid 
                         //  && ps.LocationId == locationid 
                         //  && r.ProductServingUnitId == servingunitid
                           //&& r.ProductQty == qty
                        //   && p.CompanyID == companyid
                          // && ps.CompanyID == companyid 
                          // && r.CompanyID == companyid 
                          // && u.CompanyID == companyid

                           orderby ps.ProductName
                           select new
                           {
                               MaterialId = r.MaterialId,
                               MaterialName = ps.ProductName,
                               //MaterialCost = ps.CostPrice,
                               //MaterialSelling = ps.SellingPrice,
                               MaterialCost = r.CostPrice,
                               MaterialSelling = r.SellingPrice,
                               MaterialQuantity = r.Quantity,
                               MaterialCode = ps.ProductCode,
                               MaterialDbStock = ps.Stock,
                               LocationId = ps.LocationId,
                               UnitOfMeasureId = p.PurchasingUnit,
                               UnitOfMeasureName = u.UnitOfMeasureName,
                               WeightPerUnit = p.WeightPerUnit,
                               ProductQty = r.ProductQty,
                               ProductName=p.ProductName
                           }
                           ).ToList();


                var receipe = (
                           from r in _unitofwork.ReceipeRepository.Get(r=> r.ProductId == productid && r.ProductServingUnitId == servingunitid && r.ProductQty == 1 && r.CompanyID == companyid)
                           join ps in _unitofwork.ProductStockMasterRepository.Get(ps=> ps.LocationId == locationid && ps.CompanyID == companyid) on r.MaterialId equals ps.ProductId
                           join p in _unitofwork.ProductRepository.Get(p => p.CompanyID == companyid) on ps.ProductId equals p.ProductId
                           join u in _unitofwork.UnitOfMeasureRepository.Get(u => u.CompanyID == companyid) on p.PurchasingUnit equals u.UnitOfMeasureId
                          // where 
                          // r.ProductId == productid && ps.LocationId == locationid &&
                          // r.ProductServingUnitId == servingunitid
                          // && r.ProductQty == 1
                         //  && p.CompanyID == companyid 
                          // && ps.CompanyID == companyid 
                          // && r.CompanyID == companyid 
                          // && u.CompanyID == companyid

                           orderby ps.ProductName
                           select new
                           {
                               MaterialId = r.MaterialId,
                               MaterialName = ps.ProductName,
                               MaterialCost = r.CostPrice * qty,
                               MaterialSelling = r.SellingPrice * qty,
                               MaterialQuantity = r.Quantity * qty,
                               MaterialCode = ps.ProductCode,
                               MaterialDbStock = ps.Stock,
                               LocationId = ps.LocationId,
                               UnitOfMeasureId = p.PurchasingUnit,
                               WeightPerUnit = p.WeightPerUnit,
                               UnitOfMeasureName = u.UnitOfMeasureName,
                               ProductName = r.ProductName,
                               ProductQty = qty
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
                        vm.ProductQuantity = mat.ProductQty;
                        vm.SubUnitId = mat.WeightPerUnit;
                        vm.ProductName = mat.ProductName;
                        var subunit= _unitofwork.UnitConversionRepository.GetById(mat.WeightPerUnit);
                        if (subunit != null)
                        {
                            vm.SubUnitValue = subunit.SubUnitValue;
                            vm.SubUnitName = subunit.SubUnitSymbol;
                        }
                        else
                        {
                            vm.SubUnitValue = 1;
                            vm.SubUnitName = subunit.SubUnitSymbol;
                        }
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
                        vm.SubUnitId = mat.WeightPerUnit;
                        vm.ProductQuantity = mat.ProductQty;
                        vm.SubUnitId = mat.WeightPerUnit;
                        vm.ProductName = mat.ProductName;
                        var subunit = _unitofwork.UnitConversionRepository.GetById(mat.WeightPerUnit);
                        if (subunit != null)
                        {
                            vm.SubUnitValue = subunit.SubUnitValue;
                            vm.SubUnitName = subunit.SubUnitSymbol;
                        }
                        else
                        {
                            vm.SubUnitValue = 1;
                            vm.SubUnitName = "n/a";
                        }
                        vvm.Add(vm);

                    }
                }

                var product = (

                          from ps in _unitofwork.ProductStockMasterRepository.Get(ps=> ps.ProductId == productid && ps.LocationId == locationid && ps.CompanyID == companyid)
                          join p in _unitofwork.ProductRepository.Get(p=> p.CompanyID == companyid) on ps.ProductId equals p.ProductId
                          // join u in context.UnitOfMeasure on p.PurchasingUnit equals u.UnitOfMeasureId

                         // where ps.ProductId == productid && ps.LocationId == locationid
                         // && p.CompanyID == companyid && ps.CompanyID == companyid

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
                              WaightPerUnit = p.WeightPerUnit,
                             
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
                    var subunit = _unitofwork.UnitConversionRepository.GetById(prd.WaightPerUnit);
                    if (subunit != null)
                    {
                        vp.SubUnitValue = subunit.SubUnitValue;
                        vp.SubUnitName = subunit.SubUnitSymbol;
                    }
                    else
                    {
                        vp.SubUnitValue = 1;
                        vp.SubUnitName = "n/a";
                    }
                    vvm.Add(vp);
                }
                return vvm;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Receipe> CheckReceipe(long productid, long locationid,decimal qty, long servingunitid,int companyid)
        {
            try
            {
                List<Receipe> receipe = _unitofwork.ReceipeRepository.Get(r => r.ProductServingUnitId == servingunitid &&
                                                                     r.ProductId == productid &&
                                                                     r.ProductQty == qty && r.CompanyID==companyid).ToList();
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


        public IEnumerable<ProductionNoteHeader> GetActiveProductions(int companyid)
        {
            try
            {
                IEnumerable<ProductionNoteHeader> productions = _unitofwork.ProductionNoteHeaderRepository.Get(i=>i.CompanyID==companyid).OrderBy(g => g.CreatedDate);
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
                IEnumerable<ProductionNoteDetail> productions = _unitofwork.ProductionNoteDetailRepository.Get(g => g.ProductionNoteHeaderId == id
                                                                                                   && g.ProductQty != 0);
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

        public IEnumerable<ProductionNoteDetail> GetProductionNoteDetail()
        {
            try
            {
                IEnumerable<ProductionNoteDetail> productions = _unitofwork.ProductionNoteDetailRepository.Get();
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
                ProductionNoteHeader productions = _unitofwork.ProductionNoteHeaderRepository.Get(p => p.ProductionNoteHeaderId == id).
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
                List<ProductionNoteDetail> productionsdet = _unitofwork.ProductionNoteDetailRepository.Get(p => p.ProductionNoteHeaderId == id).ToList();

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
            _unitofwork.CreateTransaction();

            try
            {

                _unitofwork.ProductionNoteHeaderRepository.Insert(prdheader);

                if (_unitofwork.Save() == 1)
                {

                    foreach (var detail in prdheader.ProductionDetail)
                    {
                        var materialstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.MaterialId &&
                                                                        s.LocationId == prdheader.ProductionLocId && s.CompanyID==prdheader.CompanyID).First();
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

                        _unitofwork.ProductionNoteDetailRepository.Insert(pdetail);
                    }
                    //if (context.SaveChanges() == prdheader.ProductionDetail.Count)
                    //{
                    _unitofwork.Save();
                    var productstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == prdheader.ProductId &&
                                                                      s.LocationId == prdheader.ProductionLocId && s.CompanyID==prdheader.CompanyID).First();


                    productstock.Stock += prdheader.ProductQty;
                    productstock.DocumentNo = prdheader.DocumentNo;
                    productstock.LastUpdatedDate = DateTime.Now;




                    if (_unitofwork.Save() == 1)
                    {
                        _unitofwork.Commit();
                    }
                    else
                    {
                        _unitofwork.Rollback();
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
                    _unitofwork.Rollback();
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                _unitofwork.Rollback();
                return false;

            }


        }

        public bool SubmitMaltipleProduction(ProductionNoteHeader prdheader)
        {
            _unitofwork.CreateTransaction();

            try
            {

                _unitofwork.ProductionNoteHeaderRepository.Insert(prdheader);

                if (_unitofwork.Save() == 1)
                {

                    foreach (var detail in prdheader.ProductionDetail)
                    {
                        var materialstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.MaterialId &&
                                                                        s.LocationId == prdheader.ProductionLocId && s.CompanyID==prdheader.CompanyID).First();

                        // var purchasingunit = _unitofwork.ProductRepository.GetById(detail.MaterialId).PurchasingUnit;
                        // var subunitvalue = _unitofwork.UnitConversionRepository.Get(u=>u.UnitOfMeasureId== purchasingunit).FirstOrDefault().SubUnitValue;

                        // materialstock.Stock -= detail.MaterialQty/ subunitvalue;
                        materialstock.Stock -= detail.MaterialQty * detail.ActualQty;
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
                        pdetail.ServingUnitId = detail.ServingUnitId;
                        pdetail.ActualQty = detail.ActualQty;
                        _unitofwork.ProductionNoteDetailRepository.Insert(pdetail);

                        if (detail.ProductQty != 0)
                        {
                            var productstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.ProductId &&
                                                                                s.LocationId == prdheader.ProductionLocId && s.CompanyID==prdheader.CompanyID).First();


                            // productstock.Stock += detail.ProductQty;
                            productstock.Stock += detail.ActualQty;
                            productstock.DocumentNo = prdheader.DocumentNo;
                            productstock.LastUpdatedDate = DateTime.Now;
                        }


                    }


                    if (prdheader.Mode == "RequestBased")
                    {
                       // var docnum = _unitofwork.RequestNoteHeaderRepository.GetById(prdheader.RequestNoteId).DocumentNo;
                        var reqheader = _unitofwork.RequestNoteAccptanceHeaderRepository.GetById(prdheader.RequestNoteId);
                        reqheader.IsProductionComplete = true;
                        _unitofwork.RequestNoteAccptanceHeaderRepository.Update(reqheader);
                    }

                    _unitofwork.Save();
                    _unitofwork.Commit();
                    return true;



                }
                else
                {
                    _unitofwork.Rollback();
                    return false;
                }


            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return false;

            }


        }

        public int DeleteProductionDetailByHeaderId(long id)
        {
            try
            {
                _unitofwork.ProductionNoteDetailRepository.DeleteRange(_unitofwork.ProductionNoteDetailRepository.Get(x => x.ProductionNoteHeaderId == id));
                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool EditProduction(ProductionNoteHeader prdheader, ProductionNoteHeader oldprdheader)
        {
            _unitofwork.CreateTransaction();

            try
            {
                if (DeleteProductionDetailByHeaderId(prdheader.ProductionNoteHeaderId) != 0)
                {


                    var newproduction = GetActiveProductionsById(prdheader.ProductionNoteHeaderId);
                    //  dbproduction.ProductionDetail = GetActiveProductionDetById(prdheader.ProductionNoteHeaderId);
                    var productstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == prdheader.ProductId &&
                                                                         s.LocationId == prdheader.ProductionLocId && s.CompanyID==prdheader.CompanyID).First();
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
                        var materialstock = _unitofwork.ProductStockMasterRepository.Get(s => s.ProductId == detail.MaterialId &&
                                                                        s.LocationId == prdheader.ProductionLocId && s.CompanyID==prdheader.CompanyID).First();

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
                        _unitofwork.ProductionNoteDetailRepository.Insert(pdetail);
                    }

                    // context.SaveChanges();
                    //var productstock = context.ProductStockMaster.Where(s => s.ProductId == prdheader.ProductId &&
                    //                                                  s.LocationId == prdheader.ProductionLocId).First();
                    //productstock.Stock += prdheader.ProductQty;
                    //productstock.Stock -= oldprdheader.ProductQty;

                    if (_unitofwork.Save() != 0)
                    {
                        _unitofwork.Commit();
                        return true;
                    }
                    else
                    {
                        _unitofwork.Rollback();
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
                    _unitofwork.Rollback();
                    return false;
                }

            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return false;

            }


        }

        public IEnumerable<ProductionNoteHeader> GetPeoductionsByLocId(long locid, DateTime datefrom, DateTime dateto,int companyid)
        {
            

            return _unitofwork.ProductionNoteHeaderRepository.Get(p => p.ProductionLocId == locid &&
                                                         DbFunctions.TruncateTime(p.CreatedDate) >= DbFunctions.TruncateTime(datefrom) &&
                                                         DbFunctions.TruncateTime(p.CreatedDate) <= DbFunctions.TruncateTime(dateto) &&
                                                         p.CompanyID==companyid
                                                         );
        }

        public List<ProductionReportViewModel> GetProductions(long locid, DateTime datefrom, DateTime dateto,long docid)
        {
            
            List<ProductionReportViewModel> reportdata = new List<ProductionReportViewModel>();

           List<ProductionNoteHeader> production = new List<ProductionNoteHeader>();
            if (locid == 0 && docid == 0)
            {
                production = _unitofwork.ProductionNoteHeaderRepository.Get(p => DbFunctions.TruncateTime(p.CreatedDate) >= DbFunctions.TruncateTime(datefrom)
                            && DbFunctions.TruncateTime(p.CreatedDate) <= DbFunctions.TruncateTime(dateto)).ToList();
            }
            else if (locid != 0 && docid == 0)
            {
                production = _unitofwork.ProductionNoteHeaderRepository.Get(p => DbFunctions.TruncateTime(p.CreatedDate) >= DbFunctions.TruncateTime(datefrom)
                           && DbFunctions.TruncateTime(p.CreatedDate) <= DbFunctions.TruncateTime(dateto) && p.ProductionLocId == locid).ToList();
            }
            else if (locid != 0 && docid != 0)
            {
                production = _unitofwork.ProductionNoteHeaderRepository.Get(p => p.ProductionLocId == locid 
                                                                            && p.ProductionNoteHeaderId == docid).ToList();
            } 
               
            foreach (var item in production)
            {
                var vm = new ProductionReportViewModel();

                vm.HeaderId = item.ProductionNoteHeaderId;
                vm.Date = item.CreatedDate.ToShortDateString();
                vm.DocNo = item.DocumentNo;
                var loc = _blllocation.GetLocationById(item.LocationId);
                if (loc != null)
                {
                    vm.Location = loc.LocationName;
                    vm.LocationId = loc.SysLocationID;
                }

                foreach (var s in _unitofwork.ProductionNoteDetailRepository.Get(r => r.ProductionNoteHeaderId == item.ProductionNoteHeaderId && r.ProductQty != 0))
                {
                    ProductionReportViewModel.ProductionDetail det = new ProductionReportViewModel.ProductionDetail();
                    det.ProductId = s.ProductId;
                    var prd = _unitofwork.ProductRepository.GetById(det.ProductId);
                    if (prd != null)
                    {
                        det.ProductName = prd.ProductName;
                        det.ProductCode = prd.ProductCode;
                    }
                    det.ProductQuantity = s.ProductQty;
                    det.ProductCostPrice = s.ProductCostPrice;
                    det.ProductSellingPrice = s.ProductSellingPrice;
                    vm.ProductionDetailReport.Add(det);

                }
                reportdata.Add(vm);
            }

            return reportdata;
        }

        public IEnumerable<ProductionNoteHeader> GetActiveProductionsToday(DateTime date,int companyid)
        {
            try
            {
                //IEnumerable<ProductionNoteHeader> productions = _unitofwork.ProductionNoteHeaderRepository.Get(p => p.IsTempPN == false
                //                                             &&
                //                                             DbFunctions.TruncateTime(p.CreatedDate) == date.Date
                //                                             ).OrderBy(g => g.CreatedDate);
                var sss = _unitofwork.ProductionNoteHeaderRepository.Get(p=>p.CompanyID==companyid);
                IEnumerable<ProductionNoteHeader> productions = _unitofwork.ProductionNoteHeaderRepository.Get(p=>p.CompanyID==companyid);
                if (productions != null)
                {
                    return productions;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ProductionNoteHeader> GetActiveProductionsThisWeek(DateTime fromdate, DateTime todate,int companyid)
        {
            try
            {
                IEnumerable<ProductionNoteHeader> productions = _unitofwork.ProductionNoteHeaderRepository.Get(p => p.IsTempPN == false
                                                             &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) >= fromdate.Date
                                                             &&
                                                              DbFunctions.TruncateTime(p.CreatedDate) <= todate.Date
                                                             && p.CompanyID==companyid).OrderBy(g => g.CreatedDate);
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


        public List<ProductionReportViewModel> GetProductionsThisweek(DateTime datefrom, DateTime dateto,int companyid)
        {
            DateTime date1 = datefrom.Date;
            DateTime date2 = dateto.Date.AddDays(1);
            List<ProductionReportViewModel> reportdata = new List<ProductionReportViewModel>();
            
            var productionhead = (
                          from ph in _unitofwork.ProductionNoteHeaderRepository.Get(ph=>ph.CreatedDate >= date1 && ph.CreatedDate <= date2 && ph.CompanyID == companyid)
                          join pd in _unitofwork.ProductionNoteDetailRepository.Get(pd=>pd.ProductQty != 0) on ph.ProductionNoteHeaderId equals pd.ProductionNoteHeaderId
                          join p in _unitofwork.ProductRepository.Get(p=> p.CompanyID == companyid) on pd.ProductId equals p.ProductId
                          join l in _unitofwork.LocationRepository.Get(l=> l.CompanyID == companyid) on ph.LocationId equals l.SysLocationID
                         // where 
                         //  && 
                          //  && ph.ProductionLocId == locid
                          orderby ph.DocumentNo
                          select new
                          {
                              HeaderId = ph.ProductionNoteHeaderId,
                              Location = l.LocationName,
                              DocNo = ph.DocumentNo,
                              Date = ph.CreatedDate
                          }
                      ).ToList().Distinct();

            foreach (var item in productionhead)
            {
                var vm = new ProductionReportViewModel();
                vm.HeaderId = item.HeaderId;
                vm.Date = item.Date.ToShortDateString();
                vm.DocNo = item.DocNo;
                vm.Location = item.Location;

                foreach (var s in _unitofwork.ProductionNoteDetailRepository.Get(r => r.ProductionNoteHeaderId == item.HeaderId && r.ProductQty != 0))
                {
                    ProductionReportViewModel.ProductionDetail det = new ProductionReportViewModel.ProductionDetail();
                    det.ProductId = s.ProductId;
                    det.ProductName = _unitofwork.ProductRepository.GetById(det.ProductId).ProductName;
                    det.ProductQuantity = s.ProductQty;
                    det.ProductCostPrice = s.ProductCostPrice;
                    det.ProductSellingPrice = s.ProductSellingPrice;
                    vm.ProductionDetailReport.Add(det);
                }

                reportdata.Add(vm);
            }

            return reportdata;
        }

        public List<ProductionReportViewModel> GetProductionsToday(DateTime datefrom,int companyid)
        {
            DateTime date1 = datefrom.Date;
            //   DateTime date2 = dateto.Date.AddDays(1);
            List<ProductionReportViewModel> reportdata = new List<ProductionReportViewModel>();
            var productionhead = (
                          from ph in _unitofwork.ProductionNoteHeaderRepository.Get(ph=> ph.CreatedDate >= date1 && ph.CompanyID == companyid)
                          join pd in _unitofwork.ProductionNoteDetailRepository.Get(pd=> pd.ProductQty != 0) on ph.ProductionNoteHeaderId equals pd.ProductionNoteHeaderId
                          join p in _unitofwork.ProductRepository.Get(p=>p.CompanyID == companyid) on pd.ProductId equals p.ProductId
                          join l in _unitofwork.LocationRepository.Get(l=>l.CompanyID == companyid) on ph.LocationId equals l.SysLocationID
                         // where                       
                          //  && ph.ProductionLocId == locid
                          orderby ph.DocumentNo
                          select new
                          {
                              HeaderId = ph.ProductionNoteHeaderId,
                              Location = l.LocationName,
                              DocNo = ph.DocumentNo,
                              Date = ph.CreatedDate
                          }
                      ).ToList().Distinct();

            foreach (var item in productionhead)
            {
                var vm = new ProductionReportViewModel();
                vm.HeaderId = item.HeaderId;
                vm.Date = item.Date.ToShortDateString();
                vm.DocNo = item.DocNo;
                vm.Location = item.Location;
                foreach (var s in _unitofwork.ProductionNoteDetailRepository.Get(r => r.ProductionNoteHeaderId == item.HeaderId && r.ProductQty != 0))
                {
                    ProductionReportViewModel.ProductionDetail det = new ProductionReportViewModel.ProductionDetail();
                    det.ProductId = s.ProductId;
                    det.ProductName = _unitofwork.ProductRepository.GetById(det.ProductId).ProductName;
                    det.ProductQuantity = s.ProductQty;
                    det.ProductCostPrice = s.ProductCostPrice;
                    det.ProductSellingPrice = s.ProductSellingPrice;
                    vm.ProductionDetailReport.Add(det);

                }
                reportdata.Add(vm);
            }

            return reportdata;
        }

        public IEnumerable<ProductionNoteHeader> ProductionTest(int companyid)
        {
            try
            {
                //IEnumerable<ProductionNoteHeader> productions = _unitofwork.ProductionNoteHeaderRepository.Get(p => p.IsTempPN == false
                //                                             &&
                //                                             DbFunctions.TruncateTime(p.CreatedDate) == date.Date
                //                                             ).OrderBy(g => g.CreatedDate);
                var p = _unitofwork.ProductRepository.Get(a=>a.CompanyID==companyid);
                var g = _unitofwork.PurchaseHeaderRepository.Get(a => a.CompanyID == companyid);
                var s = _unitofwork.ProductionNoteHeaderRepository.Get(a => a.CompanyID == companyid).FirstOrDefault();
                var sss = _unitofwork.ProductionNoteHeaderRepository.Get(a => a.CompanyID == companyid);
                IEnumerable<ProductionNoteHeader> productions = _unitofwork.ProductionNoteHeaderRepository.Get(a => a.CompanyID == companyid);
                if (productions != null)
                {
                    return productions;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


    }
}
