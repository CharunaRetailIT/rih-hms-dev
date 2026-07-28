using RIT.HMS.BLL.Common;
using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Logs;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.DataUpload;
using RIT.HMS.Domain.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_Receipe
    {
        private readonly UnitOfWork _unitofwork;
        private readonly BLL_Product _bllProduct;
        public BLL_Receipe()
        {
            _unitofwork = new UnitOfWork();
            _bllProduct = new BLL_Product();
        }

        public BLL_Receipe(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
            _bllProduct = new BLL_Product(connectionname);
        }

        public int SaveReceipe(Receipe receipe)
        {
            try
            {
                if (receipe.ApplyForAllLocation == false)
                {

                    var dbsu = _unitofwork.ProductServingUnitRepository.Get(sunit => sunit.CompanyID == receipe.CompanyID
                                                                            && sunit.LocationId == receipe.LocationId
                                                                            && sunit.ProductId == receipe.ProductId
                                                                            && sunit.ServingUnit == receipe.ServingUnitName).FirstOrDefault();
                    if (dbsu != null)
                    {
                        var existsreceipe = _unitofwork.ReceipeRepository.Get(r => r.ProductId == receipe.ProductId &&
                                                           r.ProductServingUnitId == dbsu.ProductServingUnitId &&
                                                           r.ProductQty == receipe.ProductQty &&
                                                           r.LocationId == receipe.LocationId &&
                                                           r.CompanyID == receipe.CompanyID &&
                                                           r.GroupOfCompanyID == receipe.GroupOfCompanyID).ToList();
                        if (existsreceipe.Count != 0)
                        {
                            _unitofwork.ReceipeRepository.DeleteRange(_unitofwork.ReceipeRepository.Get(x => x.ProductId == receipe.ProductId &&
                                                                    x.ProductServingUnitId == dbsu.ProductServingUnitId &&
                                                                    x.ProductQty == receipe.ProductQty &&
                                                                    x.LocationId == receipe.LocationId &&
                                                                    x.CompanyID == receipe.CompanyID &&
                                                                    x.GroupOfCompanyID == receipe.GroupOfCompanyID
                                                              ));
                        }

                        foreach (var r in receipe.Receipes)
                        {
                            Receipe newres = new Receipe();
                            newres.ProductId = receipe.ProductId;
                            newres.ProductServingUnitId = receipe.ProductServingUnitId;
                            newres.MaterialId = r.MaterialId;
                            newres.Quantity = r.Quantity;
                            newres.CostPrice = r.CostPrice;
                            newres.SellingPrice = r.SellingPrice;

                            newres.LocationId = receipe.LocationId;
                            newres.CreatedDate = DateTime.Now;
                            newres.DataTransfer = 0;
                            newres.ModifiedDate = DateTime.Now;
                            newres.CompanyID = receipe.CompanyID;
                            newres.GroupOfCompanyID = receipe.GroupOfCompanyID;
                            newres.CreatedUser = receipe.CreatedUser;
                            newres.ModifiedUser = receipe.CreatedUser;
                            newres.ProductQty = receipe.ProductQty;
                            newres.IsActive = r.IsActive;

                            _unitofwork.ReceipeRepository.Insert(newres);
                            LOGReceipe lgrecipe = new LOGReceipe();
                            var mappedres = HMSExtensions.MatchAndMap(newres, lgrecipe);
                            mappedres.SourceId = Convert.ToInt32(newres.ReceipeId);
                            _unitofwork.LOGReceipe.Insert(mappedres);

                        }

                        dbsu.SellingPrice = receipe.TotSellingPrice;
                        dbsu.CostPrice = receipe.TotCostPrice;
                        dbsu.ModifiedDate = DateTime.Now;
                        // su.LocationId = receipe.LocationId;
                        dbsu.ModifiedUser = receipe.ModifiedUser;
                        dbsu.DataTransfer = 0;
                        dbsu.CompanyID = receipe.CompanyID;
                        dbsu.GroupOfCompanyID = receipe.GroupOfCompanyID;

                        LOGProductServingUnit lgprdunits = new LOGProductServingUnit();
                        var mappedsu = HMSExtensions.MatchAndMap(dbsu, lgprdunits);
                        mappedsu.SourceId = Convert.ToInt32(dbsu.ProductServingUnitId);
                        _unitofwork.LOGProductServingUnit.Insert(mappedsu);

                    }
                    else
                    {

                        ProductServingUnit newproductserevingunit = new ProductServingUnit();
                        newproductserevingunit.ProductId = receipe.ProductId;
                        newproductserevingunit.ServingUnit = receipe.ServingUnitName;
                        newproductserevingunit.SellingPrice = receipe.TotSellingPrice;
                        newproductserevingunit.CostPrice = receipe.TotCostPrice;
                        newproductserevingunit.CreatedDate = DateTime.Now;
                        newproductserevingunit.LocationId = receipe.LocationId;
                        newproductserevingunit.CreatedUser = receipe.CreatedUser;
                        newproductserevingunit.DataTransfer = 0;
                        newproductserevingunit.CompanyID = receipe.CompanyID;
                        newproductserevingunit.GroupOfCompanyID = receipe.GroupOfCompanyID;

                        _unitofwork.ProductServingUnitRepository.Insert(newproductserevingunit);
                        _unitofwork.Save();

                        var existsreceipe = _unitofwork.ReceipeRepository.Get(r => r.ProductId == receipe.ProductId &&
                                                           r.ProductServingUnitId == newproductserevingunit.ProductServingUnitId &&
                                                           r.ProductQty == receipe.ProductQty &&
                                                           r.LocationId == receipe.LocationId &&
                                                           r.CompanyID == receipe.CompanyID &&
                                                           r.GroupOfCompanyID == receipe.GroupOfCompanyID).ToList();
                        if (existsreceipe.Count != 0)
                        {
                            _unitofwork.ReceipeRepository.DeleteRange(_unitofwork.ReceipeRepository.Get(x => x.ProductId == receipe.ProductId &&
                                                                    x.ProductServingUnitId == newproductserevingunit.ProductServingUnitId &&
                                                                    x.ProductQty == receipe.ProductQty &&
                                                                    x.LocationId == receipe.LocationId &&
                                                                    x.CompanyID == receipe.CompanyID &&
                                                                    x.GroupOfCompanyID == receipe.GroupOfCompanyID
                                                              ));
                        }

                        foreach (var r in receipe.Receipes)
                        {
                            Receipe newres = new Receipe();
                            newres.ProductId = receipe.ProductId;
                            newres.ProductServingUnitId = newproductserevingunit.ProductServingUnitId;
                            newres.MaterialId = r.MaterialId;
                            newres.Quantity = r.Quantity;
                            newres.CostPrice = r.CostPrice;
                            newres.SellingPrice = r.SellingPrice;

                            newres.LocationId = receipe.LocationId;
                            newres.CreatedDate = DateTime.Now;
                            newres.DataTransfer = 0;
                            newres.ModifiedDate = DateTime.Now;
                            newres.CompanyID = receipe.CompanyID;
                            newres.GroupOfCompanyID = receipe.GroupOfCompanyID;
                            newres.CreatedUser = receipe.CreatedUser;
                            newres.ModifiedUser = receipe.ModifiedUser;
                            newres.ProductQty = receipe.ProductQty;
                            newres.IsActive = r.IsActive;

                            _unitofwork.ReceipeRepository.Insert(newres);
                            LOGReceipe lgrecipe = new LOGReceipe();
                            var mappedres = HMSExtensions.MatchAndMap(newres, lgrecipe);
                            mappedres.SourceId = Convert.ToInt32(newres.ReceipeId);
                            _unitofwork.LOGReceipe.Insert(mappedres);

                        }

                        LOGProductServingUnit lgprdunits = new LOGProductServingUnit();
                        var mappedsu = HMSExtensions.MatchAndMap(newproductserevingunit, lgprdunits);
                        mappedsu.SourceId = Convert.ToInt32(newproductserevingunit.ProductServingUnitId);
                        _unitofwork.LOGProductServingUnit.Insert(mappedsu);
                    }



                }
                else
                {

                    var locations = _unitofwork.LocationRepository.Get(l => l.IsActive == true &&
                                                                            l.IsDelete == false
                                                                            && l.IsShowRoom == true
                                                                            && l.CompanyID == receipe.CompanyID
                                                                            ).ToList();

                    foreach (var l in locations)
                    {

                        var existsu = _unitofwork.ProductServingUnitRepository.Get(r => r.ProductId == receipe.ProductId &&
                                                            r.ServingUnit == receipe.ServingUnitName &&
                                                            r.LocationId == l.SysLocationID &&
                                                            r.CompanyID == receipe.CompanyID &&
                                                            r.GroupOfCompanyID == receipe.GroupOfCompanyID).FirstOrDefault();

                        if (existsu != null)
                        {

                            existsu.SellingPrice = receipe.TotSellingPrice;
                            existsu.CostPrice = receipe.TotCostPrice;
                            existsu.ModifiedDate = DateTime.Now;
                            existsu.ModifiedUser = receipe.ModifiedUser;
                            existsu.DataTransfer = 0;
                            existsu.CompanyID = receipe.CompanyID;
                            existsu.GroupOfCompanyID = receipe.GroupOfCompanyID;

                            var existsreceipe = _unitofwork.ReceipeRepository.Get(r => r.ProductId == receipe.ProductId &&
                                                            r.ProductServingUnitId == existsu.ProductServingUnitId &&
                                                            r.ProductQty == receipe.ProductQty &&
                                                            r.LocationId == l.SysLocationID &&
                                                            r.CompanyID == receipe.CompanyID &&
                                                            r.GroupOfCompanyID == receipe.GroupOfCompanyID).ToList();


                            if (existsreceipe.Count != 0)
                            {
                                _unitofwork.ReceipeRepository.DeleteRange(_unitofwork.ReceipeRepository.Get(x => x.ProductId == receipe.ProductId &&
                                                                        x.ProductServingUnitId == existsu.ProductServingUnitId &&
                                                                        x.ProductQty == receipe.ProductQty &&
                                                                        x.LocationId == l.SysLocationID &&
                                                                        x.CompanyID == receipe.CompanyID &&
                                                                        x.GroupOfCompanyID == receipe.GroupOfCompanyID
                                                                        ));
                            }

                            receipe.Receipes.ForEach(lr =>
                            {
                                var locationprices = _bllProduct.GetReceipeDetails(l.SysLocationID, lr.MaterialId, lr.Quantity,
                                                                                   Convert.ToDecimal(lr.UnitConvertion), receipe.CompanyID);
                                lr.CostPrice = locationprices.CostPrice;
                                // lr.SellingPrice = locationprices.SellingPrice;
                            }
                            );
                            existsu.CostPrice = receipe.Receipes.Sum(s => s.CostPrice);
                            existsu.CostPrice = existsu.CostPrice / receipe.ProductQty;
                            //  existsu.SellingPrice = receipe.Receipes.Sum(s => s.SellingPrice);

                            foreach (var r in receipe.Receipes)
                            {
                                Receipe newres = new Receipe();
                                newres.ProductId = receipe.ProductId;
                                newres.ProductServingUnitId = existsu.ProductServingUnitId;

                                newres.MaterialId = r.MaterialId;
                                newres.Quantity = r.Quantity;

                                newres.CostPrice = r.CostPrice;
                                newres.SellingPrice = r.SellingPrice;

                                newres.LocationId = l.SysLocationID;
                                newres.CreatedDate = DateTime.Now;
                                newres.DataTransfer = 0;
                                newres.ModifiedDate = DateTime.Now;
                                newres.CompanyID = receipe.CompanyID;
                                newres.GroupOfCompanyID = receipe.GroupOfCompanyID;
                                newres.ModifiedUser = receipe.ModifiedUser;
                                newres.CreatedUser = receipe.CreatedUser;
                                newres.ProductQty = receipe.ProductQty;
                                newres.IsActive = r.IsActive;

                                _unitofwork.ReceipeRepository.Insert(newres);
                                LOGReceipe lgrecipe = new LOGReceipe();
                                var mappedres = HMSExtensions.MatchAndMap(newres, lgrecipe);
                                mappedres.SourceId = Convert.ToInt32(newres.ReceipeId);
                                _unitofwork.LOGReceipe.Insert(mappedres);

                            }

                            LOGProductServingUnit lgprdunits1 = new LOGProductServingUnit();
                            var mappedsu1 = HMSExtensions.MatchAndMap(existsu, lgprdunits1);
                            mappedsu1.SourceId = Convert.ToInt32(existsu.ProductServingUnitId);
                            _unitofwork.LOGProductServingUnit.Insert(mappedsu1);

                        }
                        else
                        {

                            receipe.Receipes.ForEach(lr =>
                            {
                                var locationprices = _bllProduct.GetReceipeDetails(l.SysLocationID, lr.MaterialId, lr.Quantity,
                                                                                   Convert.ToDecimal(lr.UnitConvertion), receipe.CompanyID);
                                lr.CostPrice = locationprices.CostPrice;
                                //   lr.SellingPrice = locationprices.SellingPrice;
                            }
                            );


                            ProductServingUnit su = new ProductServingUnit();
                            su.ProductId = receipe.ProductId;
                            su.ServingUnit = receipe.ServingUnitName;
                            su.SellingPrice = receipe.TotSellingPrice;
                            su.CostPrice = (receipe.Receipes.Sum(s => s.CostPrice)) / receipe.ProductQty;
                            su.LocationId = l.SysLocationID;
                            su.ModifiedUser = receipe.ModifiedUser;
                            su.DataTransfer = 0;
                            su.CompanyID = receipe.CompanyID;
                            su.GroupOfCompanyID = receipe.GroupOfCompanyID;
                            su.CreatedDate = DateTime.Now;
                            su.CreatedUser = receipe.CreatedUser;
                            su.ModifiedUser = receipe.ModifiedUser;
                            su.ModifiedDate = DateTime.Now;
                            su.DataTransfer = 0;
                            _unitofwork.ProductServingUnitRepository.Insert(su);
                            _unitofwork.Save();


                            var existsreceipe = _unitofwork.ReceipeRepository.Get(r => r.ProductId == receipe.ProductId &&
                                                            r.ProductServingUnitId == su.ProductServingUnitId &&
                                                            r.ProductQty == receipe.ProductQty &&
                                                            r.LocationId == l.SysLocationID &&
                                                            r.CompanyID == receipe.CompanyID &&
                                                            r.GroupOfCompanyID == receipe.GroupOfCompanyID).ToList();


                            if (existsreceipe.Count != 0)
                            {
                                _unitofwork.ReceipeRepository.DeleteRange(_unitofwork.ReceipeRepository.Get(x => x.ProductId == receipe.ProductId &&
                                                                        x.ProductServingUnitId == su.ProductServingUnitId &&
                                                                        x.ProductQty == receipe.ProductQty &&
                                                                        x.LocationId == l.SysLocationID &&
                                                                        x.CompanyID == receipe.CompanyID &&
                                                                        x.GroupOfCompanyID == receipe.GroupOfCompanyID
                                                                        ));
                            }

                            foreach (var r in receipe.Receipes)
                            {
                                Receipe newres1 = new Receipe();
                                newres1.ProductId = receipe.ProductId;
                                newres1.ProductServingUnitId = su.ProductServingUnitId;

                                newres1.MaterialId = r.MaterialId;
                                newres1.Quantity = r.Quantity;
                                newres1.CostPrice = r.CostPrice;
                                newres1.SellingPrice = r.SellingPrice;

                                newres1.LocationId = l.SysLocationID;
                                newres1.CreatedDate = DateTime.Now;
                                newres1.DataTransfer = 0;
                                newres1.ModifiedDate = DateTime.Now;
                                newres1.CompanyID = receipe.CompanyID;
                                newres1.GroupOfCompanyID = receipe.GroupOfCompanyID;
                                newres1.CreatedUser = receipe.CreatedUser;
                                newres1.CreatedDate = receipe.CreatedDate;
                                newres1.ProductQty = receipe.ProductQty;
                                newres1.IsActive = r.IsActive;

                                _unitofwork.ReceipeRepository.Insert(newres1);

                                LOGReceipe lgrecipe = new LOGReceipe();
                                var mappedres = HMSExtensions.MatchAndMap(newres1, lgrecipe);
                                mappedres.SourceId = Convert.ToInt32(newres1.ReceipeId);
                                _unitofwork.LOGReceipe.Insert(mappedres);

                            }

                            LOGProductServingUnit lgprdunits = new LOGProductServingUnit();
                            var mappedsu = HMSExtensions.MatchAndMap(su, lgprdunits);
                            mappedsu.SourceId = Convert.ToInt32(su.ProductServingUnitId);
                            _unitofwork.LOGProductServingUnit.Insert(mappedsu);
                        }
                    }



                }
                int res = _unitofwork.Save();
                return res;


            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int RemoveReceipe(Receipe receipe)
        {
            _unitofwork.ReceipeRepository.Delete(_unitofwork.ReceipeRepository.Get(x => x.ProductId == receipe.ProductId &&
                                                              x.ProductServingUnitId == receipe.ProductServingUnitId && x.ProductQty == receipe.ProductQty));
            var res = _unitofwork.Save();
            return res;
        }

        public long ServingUnitId(long prdid, string serv)
        {
            return _unitofwork.ProductServingUnitRepository.Get(p => p.ServingUnit == serv && p.ProductId == prdid).First().ProductServingUnitId;
        }

        public Boolean CheckReceipesExists(long ProductId, long ProductServingUnitId, decimal quantity)
        {
            try
            {
                return _unitofwork.ReceipeRepository.Get().Any(g => g.ProductId == ProductId && g.ProductServingUnitId == ProductServingUnitId && g.ProductQty == quantity);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<Receipe> CheckReceipesExist(string ProductCode, string ProductServingUnit, Int32 compid)
        {
            try
            {
                var receipeitems = (
                          from p in _unitofwork.ReceipeRepository.Get()
                          join ps in _unitofwork.ProductServingUnitRepository.Get() on p.ProductId equals ps.ProductId
                          join pm in _unitofwork.ProductRepository.Get() on p.ProductId equals pm.ProductId
                          where ps.ProductServingUnitId == p.ProductServingUnitId && pm.IsActive == true && pm.IsDelete == false
                                && pm.ProductCode == ProductCode
                                && ps.ServingUnit == ProductServingUnit && pm.CompanyID == compid
                          select new
                          {
                              ProductId = p.ProductId,
                              ProductDesc = pm.ProductName,
                              ServingUnit = ps.ServingUnit,
                              ProductQty = p.ProductQty,
                              Cost = ps.CostPrice,
                              Selling = ps.SellingPrice,
                              Code = pm.ProductCode,
                              ServingUnitId = ps.ProductServingUnitId,
                              CreatedDate = Convert.ToDateTime(p.CreatedDate.ToShortDateString())
                          }
                      ).Distinct().ToList();

                List<Receipe> receipes = new List<Receipe>();
                foreach (var r in receipeitems)
                {
                    Receipe res = new Receipe();

                    res.ProductId = r.ProductId;
                    res.ProductName = r.ProductDesc + '(' + r.Code + ')';
                    res.ServingUnitName = r.ServingUnit;
                    res.ProductQty = r.ProductQty;
                    res.TotCostPrice = r.Cost;
                    res.TotSellingPrice = r.Selling;
                    res.ProductCode = r.Code;
                    res.ProductServingUnitId = r.ServingUnitId;
                    res.CreatedDate = r.CreatedDate;
                    receipes.Add(res);
                }

                return receipes;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IEnumerable<Receipe> CheckReceipesExistByProductCode(string ProductCode, Int32 compid)
        {
            try
            {
                var receipeitems = (
                          from p in _unitofwork.ReceipeRepository.Get()
                          join ps in _unitofwork.ProductServingUnitRepository.Get() on p.ProductId equals ps.ProductId
                          join pm in _unitofwork.ProductRepository.Get() on p.ProductId equals pm.ProductId
                          where ps.ProductServingUnitId == p.ProductServingUnitId && pm.IsActive == true && pm.IsDelete == false
                                && pm.ProductCode == ProductCode && pm.CompanyID == compid
                          select new
                          {
                              ProductId = p.ProductId,
                              ProductDesc = pm.ProductName,
                              ServingUnit = ps.ServingUnit,
                              ProductQty = p.ProductQty,
                              Cost = ps.CostPrice,
                              Selling = ps.SellingPrice,
                              Code = pm.ProductCode,
                              ServingUnitId = ps.ProductServingUnitId,
                              CreatedDate = Convert.ToDateTime(p.CreatedDate.ToShortDateString())
                          }
                      ).Distinct().ToList();

                List<Receipe> receipes = new List<Receipe>();
                foreach (var r in receipeitems)
                {
                    Receipe res = new Receipe();

                    res.ProductId = r.ProductId;
                    res.ProductName = r.ProductDesc + '(' + r.Code + ')';
                    res.ServingUnitName = r.ServingUnit;
                    res.ProductQty = r.ProductQty;
                    res.TotCostPrice = r.Cost;
                    res.TotSellingPrice = r.Selling;
                    res.ProductCode = r.Code;
                    res.ProductServingUnitId = r.ServingUnitId;
                    res.CreatedDate = r.CreatedDate;
                    receipes.Add(res);
                }

                return receipes;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IEnumerable<Receipe> GetReceipes(Int32 compid)
        {
            try
            {
                var receipeitems = (
                          from p in _unitofwork.ReceipeRepository.Get(p => p.CompanyID == compid)
                          join ps in _unitofwork.ProductServingUnitRepository.Get(ps => ps.CompanyID == compid) on new { p.ProductId, p.ProductServingUnitId } equals new { ps.ProductId, ps.ProductServingUnitId }
                          join pm in _unitofwork.ProductRepository.Get(pm => pm.IsActive == true && pm.IsDelete == false && pm.CompanyID == compid) on p.ProductId equals pm.ProductId
                          join l in _unitofwork.LocationRepository.Get(l => l.CompanyID == compid) on ps.LocationId equals l.SysLocationID
                          where ps.ProductId == p.ProductId
                          //&& ps.CostPrice != 0

                          select new
                          {
                              ProductId = p.ProductId,
                              ProductDesc = pm.ProductName,
                              ServingUnit = ps.ServingUnit,
                              ProductQty = p.ProductQty,
                              Cost = ps.CostPrice,
                              Selling = ps.SellingPrice,
                              Code = pm.ProductCode,
                              ServingUnitId = ps.ProductServingUnitId,
                              LocationId = ps.LocationId,
                              LocationName = l.LocationName,
                              CreatedDate = Convert.ToDateTime(p.CreatedDate.ToShortDateString()),
                              IsActive = p.IsActive
                          }
                      ).Distinct().ToList();

                List<Receipe> receipes = new List<Receipe>();
                foreach (var r in receipeitems)
                {
                    Receipe res = new Receipe();

                    res.ProductId = r.ProductId;
                    res.ProductName = r.ProductDesc + '(' + r.Code + ')';
                    res.ServingUnitName = r.ServingUnit;
                    res.ProductQty = r.ProductQty;
                    res.TotCostPrice = r.Cost;
                    res.TotSellingPrice = r.Selling;
                    res.ProductCode = r.Code;
                    res.ProductServingUnitId = r.ServingUnitId;
                    res.CreatedDate = r.CreatedDate;
                    res.LocationId = r.LocationId;
                    res.LocationName = r.LocationName;
                    res.IsActive = r.IsActive;
                    receipes.Add(res);
                }

                return receipes;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<ReceipeViewModel> GetItems(long productid, long unitid, long locid, long compid, long gcompid, decimal proqty)
        {
            try
            {
                var sysrowmaterials = (from p in _unitofwork.ProductRepository.Get(p => p.IsDelete == false && p.IsActive == true && p.CompanyID == compid)
                                       join u in _unitofwork.UnitConversionRepository.Get() on p.WeightPerUnit equals u.UnitConversionId
                                       join r in _unitofwork.ReceipeRepository.Get(r => r.ProductId == productid && r.ProductServingUnitId == unitid && r.ProductQty == proqty && r.LocationId == locid) on p.ProductId equals r.MaterialId
                                       join psu in _unitofwork.ProductServingUnitRepository.Get(psu => psu.CompanyID == compid) on r.ProductServingUnitId equals psu.ProductServingUnitId
                                       //  where r.ProductId == productid && r.ProductServingUnitId == unitid && r.ProductQty == proqty
                                       select new
                                       {
                                           p.ProductName,
                                           p.ProductId,
                                           r.MaterialId,
                                           r.Quantity,
                                           r.UOM,
                                           r.CostPrice,
                                           u.SubUnitValue,
                                           r.LocationId,
                                       }).OrderBy(g => g.ProductName).ToList();

                List<ReceipeViewModel> receipes = new List<ReceipeViewModel>();
                foreach (var r in sysrowmaterials)
                {
                    ReceipeViewModel res = new ReceipeViewModel();

                    res.ProductId = r.ProductId;
                    res.MaterialId = r.MaterialId;
                    res.Quantity = r.Quantity;
                    res.UOM = r.UOM;
                    res.CostPrice = r.CostPrice;
                    res.SellingPrice = r.SubUnitValue;
                    res.LocationId = r.LocationId;
                    receipes.Add(res);
                }

                return receipes;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<ReceipeViewModel> GetReceipeByProductId(long productid)
        {
            try
            {
                List<ReceipeViewModel> receipes = new List<ReceipeViewModel>();
                foreach (var r in _unitofwork.ReceipeRepository.Get(p => p.ProductId == productid))
                {
                    ReceipeViewModel res = new ReceipeViewModel();
                    res.ProductId = r.ProductId;
                    res.MaterialId = r.MaterialId;
                    res.Quantity = r.Quantity;
                    res.UOM = r.UOM;
                    receipes.Add(res);

                }

                return receipes;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ProductServingUnit GetServingUnit(long suid)
        {
            return _unitofwork.ProductServingUnitRepository.Get(p => p.ProductServingUnitId == suid).FirstOrDefault();
        }

        public List<ProductServingUnit> GetServingUnitByPrductId(long prdid)
        {
            return _unitofwork.ProductServingUnitRepository.Get(p => p.ProductId == prdid).ToList();
        }

        public List<Receipe> GetReceipeReport(long locid, long productid)
        {
            try
            {
                List<Receipe> receipe = new List<Receipe>();

                if (locid != 0 && productid != 0)
                {
                    receipe = _unitofwork.ReceipeRepository.Get(r => r.ProductId == productid && r.LocationId == locid).
                                         OrderBy(c => c.MaterialId).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid != 0 && productid == 0)
                {
                    receipe = _unitofwork.ReceipeRepository.Get(r => r.LocationId == locid).
                                         OrderBy(c => c.MaterialId).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid == 0 && productid != 0)
                {
                    receipe = _unitofwork.ReceipeRepository.Get(r => r.ProductId == productid).
                                         OrderBy(c => c.MaterialId).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid == 0 && productid == 0)
                {
                    receipe = _unitofwork.ReceipeRepository.Get().OrderBy(c => c.MaterialId).OrderBy(d => d.LocationId).ToList();
                }

                if (receipe != null)
                {
                    return receipe;
                }

                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateRecipe(int productid, decimal recipeqty, int servingunitid, int companyid, int locationid)
        {
            var dbrecipe = _unitofwork.ReceipeRepository.Get(r => r.ProductId == productid && r.ProductQty == recipeqty
                                                                && r.ProductServingUnitId == servingunitid
                                                                && r.LocationId == locationid && r.CompanyID == companyid).ToList();
            foreach (var r in dbrecipe)
            {
                var materil = _unitofwork.ProductStockMasterRepository.Get(p => p.ProductId == r.MaterialId &&
                                                                        p.LocationId == locationid && p.CompanyID == companyid
                                                                        ).FirstOrDefault();

                materil.SubUnitValue = _unitofwork.UnitConversionRepository.GetById(_unitofwork.ProductRepository.GetById(r.MaterialId).WeightPerUnit).SubUnitValue;
                if (materil.SubUnitValue == 0)
                    materil.SubUnitValue = 1;

                r.CostPrice = (materil.AvgCost / materil.SubUnitValue) * r.Quantity;
                _unitofwork.ReceipeRepository.Update(r);

                var recipe = _unitofwork.ReceipeRepository.Get(k => k.ProductId == r.ProductId && k.ProductServingUnitId == r.ProductServingUnitId).ToList();
                var servingunit = _unitofwork.ProductServingUnitRepository.GetById(r.ProductServingUnitId);
                servingunit.CostPrice = recipe.Sum(c => c.CostPrice);
                servingunit.CreatedUser = r.CreatedUser;
                servingunit.ModifiedUser = r.ModifiedUser;
                servingunit.ModifiedDate = r.ModifiedDate;
                _unitofwork.ProductServingUnitRepository.Update(servingunit);

                _unitofwork.Save();

            }


            return dbrecipe.Count();
        }

        public int UpdateAllRecipes(int companyid, int locationid)
        {
            var dbrecipe = _unitofwork.ReceipeRepository.Get(r =>
                //r.ProductId == productid && r.ProductQty == recipeqty
                //   && r.ProductServingUnitId == servingunitid
                                                              r.LocationId == locationid && r.CompanyID == companyid).ToList();
            foreach (var r in dbrecipe)
            {
                var materil = _unitofwork.ProductStockMasterRepository.Get(p => p.ProductId == r.MaterialId &&
                                                                        p.LocationId == locationid && p.CompanyID == companyid
                                                                        ).FirstOrDefault();

                materil.SubUnitValue = _unitofwork.UnitConversionRepository.GetById(_unitofwork.ProductRepository.GetById(r.MaterialId).WeightPerUnit).SubUnitValue;
                if (materil.SubUnitValue == 0)
                    materil.SubUnitValue = 1;

                r.CostPrice = (materil.AvgCost / materil.SubUnitValue) * r.Quantity;
                _unitofwork.ReceipeRepository.Update(r);

                var recipe = _unitofwork.ReceipeRepository.Get(k => k.ProductId == r.ProductId && k.ProductServingUnitId == r.ProductServingUnitId).ToList();
                var servingunit = _unitofwork.ProductServingUnitRepository.GetById(r.ProductServingUnitId);
                servingunit.CostPrice = recipe.Sum(c => c.CostPrice);
                servingunit.CreatedUser = r.CreatedUser;
                servingunit.ModifiedUser = r.ModifiedUser;
                servingunit.ModifiedDate = r.ModifiedDate;
                _unitofwork.ProductServingUnitRepository.Update(servingunit);

                _unitofwork.Save();

            }


            return dbrecipe.Select(p => p.ProductId).Distinct().Count();
        }

        public int ActiveInactiveRecipe(int companyid, int locationid, int productid, decimal recipeqty, int servingunitid, bool activeinactive)
        {
            // 1 for active 0 for deactive

            _unitofwork.CreateTransaction();
            try
            {
                var dbrecipe = _unitofwork.ReceipeRepository.Get(r => r.CompanyID == companyid && r.LocationId == locationid
                                                                            && r.ProductId == productid
                                                                            && r.ProductQty == recipeqty
                                                                            && r.ProductServingUnitId == servingunitid).ToList();
                dbrecipe.ForEach(r => { r.IsActive = activeinactive; });
                _unitofwork.Save();
                _unitofwork.Commit();
                return dbrecipe.Count;
            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return 0;
            }
        }

        public ItemUsageViewModel ItemUsage(ItemUsageViewModel vmitemusage)
        {
            try
            {
                var receipeitems = (
                                    from td in _unitofwork.TransactionDetRepository.GetAsNoTracking(t => t.KitchenCode == vmitemusage.KitchenId &&
                                                                                        t.LocationID == vmitemusage.LocationId
                                                                                        && DbFunctions.TruncateTime(t.RecDate) >= DbFunctions.TruncateTime(vmitemusage.DateFrom)
                                                                                        && DbFunctions.TruncateTime(t.RecDate) <= DbFunctions.TruncateTime(vmitemusage.DateTo)
                                                                                      )
                                    join
                                    r in _unitofwork.ReceipeRepository.GetAsNoTracking() on td.ProductID equals r.ProductId
                                    join
                                    p in _unitofwork.ProductRepository.GetAsNoTracking() on td.ProductID equals p.ProductId
                                    join
                                    ps in _unitofwork.ProductServingUnitRepository.GetAsNoTracking() on new { r.ProductId, r.ProductServingUnitId } equals new { ps.ProductId, ps.ProductServingUnitId }
                                    join
                                    p1 in _unitofwork.ProductRepository.GetAsNoTracking() on r.MaterialId equals p1.ProductId
                                    join
                                    uc in _unitofwork.UnitConversionRepository.GetAsNoTracking() on p1.WeightPerUnit equals uc.UnitConversionId
                                    join
                                     dept in _unitofwork.DepartmentRepository.GetAsNoTracking() on p.DepartmentId equals dept.RstDepartmentID
                                    select new
                                    {
                                        ProductId = p.ProductId,
                                        ProductCode = p.ProductCode,
                                        ProductDesc = p.ProductName,
                                        ProductQty = td.Qty,
                                        ServingUnit = ps.ServingUnit,
                                        MaterialId = r.MaterialId,
                                        MaterialCode = p1.ProductCode,
                                        MaterialName = p1.ProductName,
                                        MaterialUnit = uc.SubUnit,
                                        MaterialQty = r.Quantity,
                                        BillDate = td.RecDate,
                                        tid = td.TransactionDetID,
                                        DepartmentId = dept.RstDepartmentID,
                                    }
                                    ).Distinct().ToList();

                foreach (var i in receipeitems)
                {
                    ItemUsageViewModel.Detail detail = new ItemUsageViewModel.Detail();
                    detail.ProductId = i.ProductId;
                    detail.ProductCode = i.ProductCode;
                    detail.ProductName = i.ProductDesc;
                    detail.ServingUnit = i.ServingUnit;
                    detail.ProductQty = i.ProductQty;
                    detail.ItemId = (int)i.MaterialId;
                    detail.ItemName = i.MaterialName;
                    detail.ItemCode = i.MaterialCode;
                    detail.ItemQty = i.MaterialQty;
                    detail.SubUnit = i.MaterialUnit;
                    detail.DepartmentId = i.DepartmentId;
                    vmitemusage.Details.Add(detail);
                }

                return vmitemusage == null ? null : vmitemusage;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<DataUploadRecipePriceChangeViewModel> DownloadRecipePriceData(int companyid)
        {
            var recipes = (from p in _unitofwork.ProductRepository.GetAsNoTracking(p => p.CompanyID == companyid)
                           join psu in _unitofwork.ProductServingUnitRepository.GetAsNoTracking(psu => psu.CompanyID == companyid) on p.ProductId equals psu.ProductId
                           join l in _unitofwork.LocationRepository.GetAsNoTracking(l => l.CompanyID == companyid) on psu.LocationId equals l.SysLocationID
                           orderby p.ProductCode, psu.ServingUnit
                           select new
                           {
                               psu.ProductId,
                               p.ProductCode,
                               p.ProductName,
                               psu.ServingUnit,
                               psu.CostPrice,
                               psu.SellingPrice,
                               l.LocationCode

                           }).ToList();

            List<DataUploadRecipePriceChangeViewModel> vmrecipelist = new List<DataUploadRecipePriceChangeViewModel>();
            foreach (var r in recipes)
            {
                DataUploadRecipePriceChangeViewModel vmrecipe = new DataUploadRecipePriceChangeViewModel();
                vmrecipe.ProductId = (Int32)r.ProductId;
                vmrecipe.ProductCode = r.ProductCode;
                vmrecipe.ProductName = r.ProductName;
                vmrecipe.ServingUint = r.ServingUnit;
                vmrecipe.CostPrice = r.CostPrice;
                vmrecipe.SellingPrice = r.SellingPrice;
                vmrecipe.LocationCode = r.LocationCode;

                vmrecipelist.Add(vmrecipe);

            }

            return vmrecipelist == null ? null : vmrecipelist;
        }

        public List<DataUploadRecipeViewModel> DownloadRecipeData(int companyid)
        {
            var recipes = (from p in _unitofwork.ProductRepository.GetAsNoTracking(p => p.CompanyID == companyid)
                           join r in _unitofwork.ReceipeRepository.GetAsNoTracking(r => r.CompanyID == companyid) on p.ProductId equals r.ProductId
                           join psu in _unitofwork.ProductServingUnitRepository.GetAsNoTracking(r => r.CompanyID == companyid) on r.ProductServingUnitId equals psu.ProductServingUnitId
                           join l in _unitofwork.LocationRepository.GetAsNoTracking(l => l.CompanyID == companyid) on r.LocationId equals l.SysLocationID
                           orderby p.ProductCode, l.LocationCode
                           select new
                           {
                               r.ProductId,
                               p.ProductCode,
                               p.ProductName,
                               psu.ServingUnit,
                               r.ProductQty,
                               r.MaterialId,
                               r.Quantity,
                               l.LocationCode,
                               psu.SellingPrice

                           }).ToList();

            List<DataUploadRecipeViewModel> vmrecipelist = new List<DataUploadRecipeViewModel>();
            foreach (var r in recipes)
            {
                DataUploadRecipeViewModel vmrecipe = new DataUploadRecipeViewModel();
                vmrecipe.ProductId = (Int32)r.ProductId;
                vmrecipe.ProductCode = r.ProductCode;
                vmrecipe.ProductName = r.ProductName;
                vmrecipe.ServingUint = r.ServingUnit;
                vmrecipe.ProductQuantity = r.ProductQty;
                vmrecipe.SellingPrice = r.SellingPrice;
                vmrecipe.MaterialId = (Int32)r.MaterialId;
                vmrecipe.MaterialQuantity = r.Quantity;
                vmrecipe.LocationCode = r.LocationCode;

                vmrecipelist.Add(vmrecipe);

            }

            vmrecipelist.ForEach(r =>
            {
                var mat = _unitofwork.ProductRepository.GetAsNoTracking(p => p.CompanyID == companyid
                           && p.ProductId == r.MaterialId).FirstOrDefault();
                r.MaterialCode = mat.ProductCode;
                r.SubUnit = _unitofwork.UnitConversionRepository.GetAsNoTracking(c => c.CompanyID == companyid
                            && c.UnitConversionId == mat.WeightPerUnit).FirstOrDefault().SubUnit;

            });
            return vmrecipelist == null ? null : vmrecipelist;
        }
    }
}
