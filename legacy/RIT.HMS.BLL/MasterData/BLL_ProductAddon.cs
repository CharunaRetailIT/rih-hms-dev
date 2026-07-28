using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
   public class BLL_ProductAddon
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_ProductAddon()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_ProductAddon(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<Addons> GetProductAddons(Int32 compid)
        {
            try
            {
                IEnumerable<Addons> productaddons = _unitofwork.AddonsRepository.Get(g=>g.CompanyID==compid).OrderBy(g => g.AddonsId);
                if (productaddons != null)
                {
                    return productaddons;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Addons> GetProductAddonsByProductId(long id)
        {
            try
            {

                var addons = _unitofwork.AddonsRepository.Get(p => p.ProductId == id).ToList();


                var dbaddons = (from p in _unitofwork.ProductRepository.Get()
                                join a in _unitofwork.AddonsRepository.Get() on p.ProductId equals a.ProductAddonId

                                where p.ProductId == id
                                select new
                                {
                                    p.ProductId,
                                    a.ProductAddonId,
                                    a.AddonQuantity,
                                    a.AddonSellingPrice,
                                    a.AddonsId,
                                    Active = (from pp in _unitofwork.ProductRepository.Get() where pp.ProductId == a.ProductAddonId select new { pp.IsActive })

                                }
                                ).ToList();


                if (addons != null)
                {
                    return addons;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }



        public IEnumerable<Addons> GetAddonsProducts(Int32 compid)
        {
            try
            {
                var productaddons = _unitofwork.AddonsRepository.Get(p=>p.CompanyID==compid).OrderBy(g => g.ProductId).Select(p => new { p.ProductId }).Distinct();
                List<Addons> addons = new List<Addons>();
                foreach (var ad in productaddons)
                {
                    Addons adn = new Addons();
                    adn.ProductId = ad.ProductId;
                    addons.Add(adn);
                }

                if (addons != null)
                {
                    return addons;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Addons> GetByProductId(long id)
        {
            try
            {
                IEnumerable<Addons> addons = _unitofwork.AddonsRepository.Get(g => g.ProductId == id).OrderBy(g => g.AddonsId);
                if (addons != null)
                {
                    return addons;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<Addons> GetActiveAddons(Int32 compid)
        {
            try
            {
                IEnumerable<Addons> addons = _unitofwork.AddonsRepository.Get(g => g.IsActive == true && g.CompanyID==compid).OrderBy(g => g.AddonsId);
                if (addons != null)
                {
                    return addons;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Addons GetAddonsById(long id)
        {
            try
            {
                Addons addons = _unitofwork.AddonsRepository.Get(g => g.AddonsId == id).FirstOrDefault();
                if (addons != null)
                {
                    return addons;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Boolean GetProductAddons(long productid,long productAddonid)
        {
            try
            {
                IEnumerable<Addons> prodaddons = _unitofwork.AddonsRepository.Get(p => p.ProductId == productid && p.ProductAddonId==productAddonid);
                if (prodaddons.Count() !=0)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveProductAddons(List<Addons> addons)
        {
            _unitofwork.CreateTransaction();
                try
                {
                        foreach (var addon in addons)
                        {
                            addon.CreatedDate = DateTime.Now;
                           // addon.CreatedUser = "";
                            addon.IsActive = true;
                            _unitofwork.AddonsRepository.Insert(addon);
                            _unitofwork.Save();
                            LOGAddons logaddons = new LOGAddons();
                            var mapped = Common.HMSExtensions.MatchAndMap(addon, logaddons);
                            mapped.SourceId = addon.AddonsId;
                            mapped.Action = "Added";
                            _unitofwork.LOGAddons.Insert(mapped);

                        }
                    _unitofwork.Save();
                    _unitofwork.Commit();
                    return 1;
                }
                catch (Exception ex)
                {

                _unitofwork.Rollback();
                    return 0;

                }
            
        }

        public int UpdateProductAddons(Addons ad)
        {
            try
            {
                _unitofwork.AddonsRepository.Update(ad);

                LOGAddons logaddons = new LOGAddons();
                var mapped=Common.HMSExtensions.MatchAndMap(ad,logaddons);
                mapped.SourceId = ad.AddonsId;
                mapped.Action = "Updated";
                _unitofwork.LOGAddons.Insert(mapped);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        
        

    }
}
