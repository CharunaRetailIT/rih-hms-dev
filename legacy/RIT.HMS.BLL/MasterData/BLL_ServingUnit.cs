using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.Logs;
using RIT.HMS.BLL.Common;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_ServingUnit
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_ServingUnit()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_ServingUnit(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<ServingUnit> GetAllServingUnits(Int32 compid)
        {
            try
            {
                IEnumerable<ServingUnit> servingunit = _unitofwork.ServingUnitRepository.Get(c => c.CompanyID == compid).OrderBy(c => c.ServingUnitName);
                return servingunit ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ServingUnit> GetActiveServingUnits(Int32 compid)
        {
            try
            {
                IEnumerable<ServingUnit> servingunit = _unitofwork.ServingUnitRepository.Get().Where(g => g.IsDelete == false && g.IsActive == true && g.CompanyID == compid).OrderBy(c => c.ServingUnitName);
                return servingunit ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ServingUnit GetServingUnitById(long id)
        {
            try
            {
                ServingUnit servingunit = _unitofwork.ServingUnitRepository.GetById(id);
                if (servingunit != null)
                {
                    return servingunit;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateServingUnit(ServingUnit servingunit)
        {
            try
            {
                _unitofwork.ServingUnitRepository.Update(servingunit);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int SaveServingUnit(ServingUnit servingunit)
        {
            try
            {
                _unitofwork.ServingUnitRepository.Insert(servingunit);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public ServingUnit GetServingUnitByName(string servunitname, Int32 compid)
        {
            try
            {
                ServingUnit servingunit = _unitofwork.ServingUnitRepository.Get(g => g.ServingUnitName == servunitname && g.CompanyID == compid).FirstOrDefault();
                if (servingunit != null)
                {
                    return servingunit;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<ProductServingUnit> GetServingUnitsByPrductId(long productid, int compid)
        {
            
                var servingunit = _unitofwork.ProductServingUnitRepository.Get(p => p.ProductId == productid  && p.CompanyID == compid).ToList();
            var distinctProducts = servingunit
     .Where(p => p.ProductId == productid && p.CompanyID == compid)
     .GroupBy(p => p.ServingUnit)
     .Select(group => group.First())
     .ToList();
            return distinctProducts;
            //return servingunit;
        }
        public List<ProductServingUnit> GetCostSellingPriceByServingUnitsPrductId(long productid, string unit, int compid)
        {

            var servingunit = _unitofwork.ProductServingUnitRepository.Get(p => p.ProductId == productid && p.ServingUnit == unit && p.CompanyID == compid)
                .Select(p => new { p.CostPrice ,p.SellingPrice, p.ProductId, p.CompanyID }).Where(p => p.ProductId == productid && p.CompanyID == compid).OrderBy(g => g.ProductId);
            List<ProductServingUnit> ProductServingUnit = new List<ProductServingUnit>();
            foreach (var p in servingunit)
            {
                ProductServingUnit prdSrvUnit = new ProductServingUnit();
                prdSrvUnit.CostPrice = p.CostPrice;
                prdSrvUnit.SellingPrice = p.SellingPrice;

                ProductServingUnit.Add(prdSrvUnit);
            }

            if (ProductServingUnit != null)
            {
                return ProductServingUnit;
            }
            else
            {
                return null;
            }
        }
        public List<ServingUnitPricesViewModel> GetServingUnitsByPrdId(long productid, string unit, int compid)
        {
            List<ServingUnitPricesViewModel> _servingunitvwmodel = new List<ServingUnitPricesViewModel>();

            var loc = _unitofwork.LocationRepository.Get(l => l.IsActive == true & l.IsDelete == false & l.CompanyID == compid && l.IsShowRoom == true).ToList();
            var su = _unitofwork.ProductServingUnitRepository.Get(s => s.ProductId == productid && s.ServingUnit == unit && s.CompanyID == compid).ToList();
            var recipe = _unitofwork.ReceipeRepository.GetAsNoTracking(r => r.CompanyID == compid && r.ProductId == productid).ToList();

            var res = (
                from l in loc
                join s in su on l.SysLocationID equals s.LocationId
                //  join r in recipe on  new { s.ProductServingUnitId,s.ProductId,s.LocationId } equals  new { r.ProductServingUnitId,r.ProductId,r.LocationId }
                select new
                {
                    LocationId = l.SysLocationID,
                    LocationName = l.LocationName,
                    ServingUnit = s.ServingUnit,
                    CostPrice = s.CostPrice,
                    SellingPrice = s.SellingPrice,
                    ProductId = s.ProductId
                }
                ).ToList();


            //var res = (from l in loc
            //           join s in su on l.SysLocationID equals s.LocationId into empDept                              
            //           from ed in empDept.DefaultIfEmpty()
            //           orderby l.LocationName
            //           select new
            //           {
            //               LocationId = l.SysLocationID,
            //               LocationName = l.LocationName,
            //               ServingUnit = ed == null ? "N/A" : ed.ServingUnit,
            //               CostPrice = ed == null ? 0 : ed.CostPrice,
            //               SellingPrice = ed == null ? 0 : ed.SellingPrice,
            //           }).ToList();
            //.GroupBy(m => m.ProductId).Select(x => x.First()).ToList()
            foreach (var r in res)
            {
                ServingUnitPricesViewModel _suvm = new ServingUnitPricesViewModel();
                _suvm.LocationId = r.LocationId;
                _suvm.Location = r.LocationName;
                _suvm.ServingUnit = r.ServingUnit;
                _suvm.CostPrice = r.CostPrice;
                _suvm.SellingPrice = r.SellingPrice;
                // _suvm.ProductId = (Int32)productid;
                if (recipe.Select(d => d.ProductId).Contains(r.ProductId))
                {
                    _servingunitvwmodel.Add(_suvm);
                }

            }

            return _servingunitvwmodel;

        }

        public bool UpdateProductServingUnits(ServingUnitPricesViewModel servingunits)
        {
            _unitofwork.CreateTransaction();
            try
            {

                foreach (var s in servingunits.ServingUnitsDetail)
                {
                    var exists = _unitofwork.ProductServingUnitRepository.Get(ps => ps.ProductId == servingunits.ProductId
                                                                                && ps.ServingUnit == s.ServingUnit &&
                                                                                ps.LocationId == s.LocationId).FirstOrDefault();

                    if (exists == null)
                    {
                        ProductServingUnit ps = new ProductServingUnit();
                        ps.ServingUnit = s.ServingUnit;
                        ps.LocationId = s.LocationId;
                        ps.CostPrice = s.CostPrice;
                        ps.SellingPrice = s.SellingPrice;
                        ps.ProductId = servingunits.ProductId;
                        ps.DeductStockOnRecipe = true;
                        ps.CreatedDate = servingunits.CreatedDate;
                        ps.CreatedUser = servingunits.CreatedUser;
                        //  _unitofwork.ProductServingUnitRepository.Insert(ps);

                        // LOGProductServingUnit lgprdservingunits = new LOGProductServingUnit();
                        // var mappedprdsunits = HMSExtensions.MatchAndMap(ps, lgprdservingunits);
                        // mappedprdsunits.SourceId = Convert.ToInt32(ps.ProductServingUnitId);
                        // _unitofwork.LOGProductServingUnit.Insert(mappedprdsunits);

                    }
                    else
                    {
                        exists.CostPrice = s.CostPrice;
                        exists.SellingPrice = s.SellingPrice;
                        exists.ModifiedDate = servingunits.ModifiedDate;
                        exists.ModifiedUser = servingunits.ModifiedUser;
                        _unitofwork.ProductServingUnitRepository.Update(exists);

                        //LOGProductServingUnit lgprdservingunits = new LOGProductServingUnit();
                        //var mappedprdsunits = HMSExtensions.MatchAndMap(exists, lgprdservingunits);
                        //mappedprdsunits.SourceId = Convert.ToInt32(exists.ProductServingUnitId);
                        //_unitofwork.LOGProductServingUnit.Insert(mappedprdsunits);
                    }


                }
                var res = _unitofwork.Save();
                if (res != 0)
                {
                    _unitofwork.Commit();
                    return true;

                }
                else
                {
                    _unitofwork.Rollback();
                    return false;
                }

            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return false;

            }


        }
        //By Aruna
        public bool UpdateProductServingUnit(ProductServingUnit servingunits)
        {
            _unitofwork.CreateTransaction();
            try
            {

                if(servingunits.ProductId!=0 && servingunits.CostPrice>0 && servingunits.SellingPrice>0)
                {
                    var exists = _unitofwork.ProductServingUnitRepository.Get(ps => ps.ProductId == servingunits.ProductId
                                                                                      && ps.ServingUnit==servingunits.ServingUnit
                                                                               //&& ps.ProductServingUnitId == servingunits.ProductServingUnitId
                                                                               //&& ps.LocationId == servingunits.LocationId
                                                                               && (servingunits.LocationId == 0 || ps.LocationId == servingunits.LocationId)
                                                                                ).ToList();

                    if (exists == null)
                    {
                        ProductServingUnit ps = new ProductServingUnit();
                        ps.ServingUnit = servingunits.ServingUnit;
                        ps.LocationId = servingunits.LocationId;
                        ps.CostPrice = servingunits.CostPrice;
                        ps.SellingPrice = servingunits.SellingPrice;
                        ps.ProductId = servingunits.ProductId;
                        ps.DeductStockOnRecipe = true;
                        ps.CreatedDate = servingunits.CreatedDate;
                        ps.CreatedUser = servingunits.CreatedUser;

                    }
                    else
                    {
                        foreach (var x in exists)
                        {

                            x.CostPrice = servingunits.CostPrice;
                            x.SellingPrice = servingunits.SellingPrice;
                            x.ModifiedDate = servingunits.ModifiedDate;
                            x.ModifiedUser = servingunits.ModifiedUser;
                            _unitofwork.ProductServingUnitRepository.Update(x);
                        }
                        //_unitofwork.ProductServingUnitRepository.UpdateBySet(servingunits, exists);
                    }


                }
                var res = _unitofwork.Save();
                if (res != 0)
                {
                    _unitofwork.Commit();
                    return true;

                }
                else
                {
                    _unitofwork.Rollback();
                    return false;
                }

            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return false;

            }


        }



        public ProductServingUnit GetProductServingUnitsByPrdIdServingUnit(long prdid, string servinunit, int compid)
        {
            return _unitofwork.ProductServingUnitRepository.Get(p => p.ProductId == prdid && p.ServingUnit == servinunit && p.CompanyID == compid).FirstOrDefault();
        }
        
        

    }
}
