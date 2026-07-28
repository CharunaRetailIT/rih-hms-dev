using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class LocationService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<SysLocation> GetLocations()
        {
            try
            {
                IEnumerable<SysLocation> syslocation = context.SysLocations.Where(g=>g.IsDelete==false).OrderBy(g => g.LocationCode);
                if (syslocation != null)
                {
                    return syslocation;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<SysLocation> GetActiveLocations()
        {
            try
            {
                IEnumerable<SysLocation> syslocation = context.SysLocations.Where(g => g.IsDelete == false && g.IsActive==true).OrderBy(g => g.LocationCode);
                if (syslocation != null)
                {
                    return syslocation;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SysLocation GetLocationById(long id)
        {
            try
            {
                SysLocation syslocation = context.SysLocations.Where(g => g.SysLocationID == id).FirstOrDefault();
                if (syslocation != null)
                {
                    return syslocation;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public List<ProductStockMaster> GetStockMasterByLocId(long id)
        {
            try
            {
               List<ProductStockMaster> sm = context.ProductStockMaster.Where(g => g.LocationId == id).ToList();
                if (sm != null)
                {
                    return sm;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

              return   null;
            }
        }

        public int SaveLocation(SysLocation loc)
        {
            try
            {
                
                context.SysLocations.Add(loc);
                int res = context.SaveChanges();
                if (res == 1)
                {
                    if (loc.InheritProducts == true)
                    {
                        InheritProductsFormHeadOffice(loc);
                    }
                }
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int InheritProductsFormHeadOffice(SysLocation loc)
        {
            try
            {
                var headofficeid = context.SysLocations.Where(l=>l.IsHeadOffice).FirstOrDefault().SysLocationID;
                var hoproducts = context.ProductStockMaster.Where(p => p.LocationId == headofficeid).ToList();
                foreach (var prd in hoproducts)
                {
                    var ps = new ProductStockMaster();
                    ps.ProductId = prd.ProductId;
                    ps.LocationId = loc.SysLocationID;
                    ps.CostCentreId = loc.SysLocationID;

                    ps.CostPrice = prd.CostPrice;
                    ps.SellingPrice = prd.SellingPrice;
                    ps.ReOrderLevel = prd.ReOrderLevel;
                    ps.ReOrderQuantity = prd.ReOrderQuantity;
                    ps.ReOrderPeriod = prd.ReOrderPeriod;
                    ps.MaxPrice = prd.MaxPrice;
                    ps.MinimumPrice = prd.MinimumPrice;
                    ps.DiscountPrc = prd.DiscountPrc;
                    ps.ForignCustomerPrice = prd.ForignCustomerPrice;
                    ps.Stock = 0;
                    ps.CostCentreId = loc.SysLocationID;
                    ps.DocumentNo = "";

                    ps.ProductCode = prd.ProductCode;
                    ps.ProductName = prd.ProductName;
                    ps.Barcode = prd.Barcode;
                    ps.StockCode = prd.ProductCode;
                    ps.RefNo1 = prd.RefNo1;
                    ps.RefNo2 = prd.RefNo2;

                    ps.ExtendedId = 0;
                    ps.ExtendedName = "1";
                    ps.PLUCode = "1";
                    ps.WeightPerunit = 1;
                    ps.UomId = 0;
                    ps.Unit = "1";
                    ps.AvgCost = 0;
                    ps.FixedGP = 0;
                    ps.OpenBal = 0;
                    ps.InitSIH = 0;
                    ps.InitCost = 0;
                    ps.AdjQty = 0;
                    ps.AvgCost = 0;
                    ps.IsDamage = false;
                    ps.IsActive = prd.IsActive;
                    ps.IsBundle = false;
                    ps.IsInitialize = false;
                    ps.DataTransfer = 0;
                    ps.Ispacksize = false;
                    ps.Iscommission = false;
                    ps.Isdecimal = false;

                    ps.GroupOfCompanyID = prd.GroupOfCompanyID;
                    ps.LocationId = loc.SysLocationID;
                    ps.CompanyID = prd.CompanyID;
                    ps.CreatedDate = prd.CreatedDate;
                    ps.CreatedUser = prd.CreatedUser;
                    ps.ModifiedDate = prd.ModifiedDate;
                    ps.ModifiedUser = prd.ModifiedUser;

                  
                    context.ProductStockMaster.Add(ps);
                   
                  
                }

                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                return 0;
            }
        }

        public int UpdateLocation(SysLocation loc)
        {
            try
            {

                //  ..context.SysGroupOfCompanys.Add(goc);
                int res = context.SaveChanges();
                if (res == 1)
                {
                    if (loc.InheritProducts == true)
                    {
                        InheritProductsFormHeadOffice(loc);
                    }
                }

                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public SysLocation GetLocByCode(string code)
        {
            try
            {
                SysLocation loc = context.SysLocations.Where(g => g.LocationCode == code).FirstOrDefault();
                if (loc != null)
                {
                    return loc;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int CheckHeadOffice()
        {
            try
            {
                return  context.SysLocations.Where(g => g.IsHeadOffice == true).Count() ;
             
            }
            catch (Exception)
            {

                return 0;
            }
        }

    }
}