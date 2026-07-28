using HospitalityManagement.Models;
using HospitalityManagement.Models.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class ProductAddonService
    {

        ApplicationDbContext context = new ApplicationDbContext();
        public IEnumerable<Addons> GetProductAddons()
        {
            try
            {
                IEnumerable<Addons> productaddons = context.Addons.OrderBy(g => g.AddonsId);
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

                var addons = context.Addons.Where(p=>p.ProductId==id).ToList();


                var dbaddons = (from p in context.Product
                                join a in context.Addons on p.ProductId equals a.ProductAddonId
                               
                                where p.ProductId == id
                                select new { p.ProductId, a.ProductAddonId, a.AddonQuantity, a.AddonSellingPrice, a.AddonsId,
                                Active = (from pp in context.Product where pp.ProductId == a.ProductAddonId select new {pp.IsActive })
                                          
                                }
                                ).ToList();
                //List<Addons> addons = new List<Addons>();
                //foreach (var ad in dbaddons)
                //{
                //    Addons add = new Addons();
                //    add.AddonsId = ad.AddonsId;
                //    add.ProductAddonId = ad.ProductAddonId;
                //    add.ProductId = ad.ProductId;
                //    add.AddonQuantity = ad.AddonQuantity;
                //    add.AddonSellingPrice = ad.AddonSellingPrice;
                //    addons.Add(add);
                //}




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

        public IEnumerable<Addons> GetAddonsProducts()
        {
            try
            {
                var productaddons = context.Addons.Select(p=>new {p.ProductId }).OrderBy(g => g.ProductId).Distinct();
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
                IEnumerable<Addons> addons = context.Addons.Where(g => g.ProductId == id).OrderBy(g => g.AddonsId);
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
        public IEnumerable<Addons> GetActiveAddons()
        {
            try
            {
                IEnumerable<Addons> addons = context.Addons.Where(g => g.IsActive == true).OrderBy(g => g.AddonsId);
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
                Addons addons = context.Addons.Where(g => g.AddonsId == id).FirstOrDefault();
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


        public int SaveProductAddons(List<Addons> addons)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {
                try
                {
                    foreach(var addon in addons)
                    {
                        addon.CreatedDate = DateTime.UtcNow;
                        addon.CreatedUser = "";
                        context.Addons.Add(addon);
                    }
                    context.SaveChanges();

                    dbtransaction.Commit();
                    return 1;
                }
                catch (Exception ex)
                {
                    dbtransaction.Rollback();
                    return 0;
                }
            }
        }

        public int UpdateProductAddons(Addons addons)
        {
            try
            {
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


    }
}