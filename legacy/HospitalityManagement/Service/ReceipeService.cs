using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class ReceipeService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public int SaveReceipe(Receipe receipe)
        {
            try
            {
                //if (GetItems(receipe.ProductId, receipe.ProductServingUnitId).Count() != 0)
                //{

                //}

                var existsreceipe = context.Receipe.Where(r => r.ProductId == receipe.ProductId &&
                                                        r.ProductServingUnitId == receipe.ProductServingUnitId &&
                                                        r.ProductQty == receipe.ProductQty &&
                                                        r.LocationId == receipe.LocationId &&
                                                        r.CompanyID == receipe.CompanyID &&
                                                        r.GroupOfCompanyID == receipe.GroupOfCompanyID).ToList();


                if (existsreceipe.Count != 0)
                {
                    context.Receipe.RemoveRange(context.Receipe.Where(x => x.ProductId == receipe.ProductId &&
                                                            x.ProductServingUnitId == receipe.ProductServingUnitId &&
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

                    context.Receipe.Add(newres);

                }

                var su = context.ProductServingUnit.Find(receipe.ProductServingUnitId);
                su.SellingPrice = receipe.TotSellingPrice;
                su.CostPrice = receipe.TotCostPrice;
                su.ModifiedDate = DateTime.Now;

                su.LocationId = receipe.LocationId;
                su.CreatedDate = DateTime.Now;
                su.DataTransfer = 0;
                su.ModifiedDate = DateTime.Now;
                su.CompanyID = receipe.CompanyID;
                su.GroupOfCompanyID = receipe.GroupOfCompanyID;
                su.CreatedUser = receipe.CreatedUser;
                su.ModifiedUser = receipe.CreatedUser;

                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int RemoveReceipe(Receipe receipe)
        {
            context.Receipe.RemoveRange(context.Receipe.Where(x => x.ProductId == receipe.ProductId &&
                                                              x.ProductServingUnitId == receipe.ProductServingUnitId && x.ProductQty==receipe.ProductQty));
            var res = context.SaveChanges();
            return res;
        }
        public long ServingUnitId(long prdid, string serv)
        {
            return context.ProductServingUnit.Where(p => p.ServingUnit == serv && p.ProductId == prdid).First().ProductServingUnitId;
        }

        public IEnumerable<Receipe> GetReceipes()
        {
            try
            {

                var receipeitems = (
                          from p in context.Receipe
                          join ps in context.ProductServingUnit on p.ProductId equals ps.ProductId
                          join pm in context.Product on p.ProductId equals pm.ProductId
                          where ps.ProductServingUnitId==p.ProductServingUnitId
                          select new
                          {

                              ProductId = p.ProductId,
                              ProductDesc = pm.ProductName,
                              ServingUnit = ps.ServingUnit,
                              ProductQty=p.ProductQty,
                              Cost = ps.CostPrice,
                              Selling = ps.SellingPrice,
                              Code = pm.ProductCode,
                              ServingUnitId=ps.ProductServingUnitId

                          }
                      ).Distinct().ToList();

                List<Receipe> receipes = new List<Receipe>();
                foreach (var r in receipeitems)
                {
                    Receipe res = new Receipe();

                    res.ProductId = r.ProductId;
                    res.ProductName = r.ProductDesc+'('+r.Code+')';
                    res.ServingUnitName = r.ServingUnit;
                    res.ProductQty = r.ProductQty;
                    res.TotCostPrice = r.Cost;
                    res.TotSellingPrice = r.Selling;
                    res.ProductCode = r.Code;
                    res.ProductServingUnitId = r.ServingUnitId;
                    receipes.Add(res);

                }

                return receipes;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<Models.ViewModels.ReceipeViewModel> GetItems(long productid, long unitid, long locid, long compid, long gcompid,decimal proqty)
        {
            try
            {


                List<Models.ViewModels.ReceipeViewModel> receipes = new List<Models.ViewModels.ReceipeViewModel>();

                foreach (var r in context.Receipe.Where(p => p.ProductId == productid &&
                                                        p.ProductServingUnitId == unitid &&
                                                        p.ProductQty == proqty &&
                                                        p.LocationId == locid && p.CompanyID == compid && p.GroupOfCompanyID == gcompid

                                                        )
                                                        )


                {
                    Models.ViewModels.ReceipeViewModel res = new Models.ViewModels.ReceipeViewModel();

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

        public List<Models.ViewModels.ReceipeViewModel> GetReceipeByProductId(long productid)
        {
            try
            {


                List<Models.ViewModels.ReceipeViewModel> receipes = new List<Models.ViewModels.ReceipeViewModel>();
                foreach (var r in context.Receipe.Where(p => p.ProductId == productid))
                {
                    Models.ViewModels.ReceipeViewModel res = new Models.ViewModels.ReceipeViewModel();

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
            return context.ProductServingUnit.Where(p => p.ProductServingUnitId == suid).FirstOrDefault();
        }

        public List<ProductServingUnit> GetServingUnitByPrductId(long prdid)
        {
            return context.ProductServingUnit.Where(p => p.ProductId == prdid).ToList();
        }
        public List<Receipe> GetReceipeReport(long locid, long productid)
        {
            try
            {
                List<Receipe> receipe = new List<Receipe>();

                if (locid != 0 && productid != 0)
                {
                    receipe = context.Receipe.Where(r => r.ProductId == productid && r.LocationId == locid).
                                         OrderBy(c => c.MaterialId).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid != 0 && productid == 0)
                {
                    receipe = context.Receipe.Where(r => r.LocationId == locid).
                                         OrderBy(c => c.MaterialId).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid == 0 && productid != 0)
                {
                    receipe = context.Receipe.Where(r => r.ProductId == productid).
                                         OrderBy(c => c.MaterialId).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid == 0 && productid == 0)
                {
                    receipe = context.Receipe.OrderBy(c => c.MaterialId).OrderBy(d => d.LocationId).ToList();
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



    }
}