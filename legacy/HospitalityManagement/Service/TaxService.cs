using HospitalityManagement.Models;
using HospitalityManagement.Models.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class TaxService
    {

        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<Tax> GetTaxes()
        {
            try
            {
                IEnumerable<Tax> tax = context.Taxes.Where(t=> t.IsDelete==false).OrderBy(t => t.TaxCode);
                if (tax != null)
                {
                    return tax;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


       

        public IEnumerable<Tax> GetActiveTaxes()
        {
            try
            {
                IEnumerable<Tax> tax = context.Taxes.Where(t => t.IsDelete == false && t.IsActive == true).OrderBy(t => t.TaxCode);
                if (tax != null)
                {
                    return tax;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Tax GetTaxById(long id)
        {
            try
            {
                Tax tax = context.Taxes.Where(t => t.TaxId == id).FirstOrDefault();
                if (tax != null)
                {
                    return tax;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveTax(Tax rm)
        {
            try
            {
                context.Taxes.Add(rm);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateTax(Tax rm)
        {
            try
            {

                //  ..context.SysGroupOfCompanys.Add(goc);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Tax GetTaxByCode(string code)
        {
            try
            {
                Tax tax = context.Taxes.Where(g => g.TaxCode == code).FirstOrDefault();
                if (tax != null)
                {
                    return tax;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public List<GRNTaxViewModel> GetProductTaxByProductId(long productid)
        {
            try
            {
                
                List<GRNTaxViewModel> tvvm = new List<GRNTaxViewModel>();
                var producttax = (
                           from p in context.ProductTax
                           join t in context.Taxes on p.TaxId equals t.TaxId


                           where p.ProductId == productid && t.IsPurchasingTax == true
                           orderby p.ProductTaxId ascending
                           select new
                           {
                               ProductId = p.ProductId,
                               TaxId = p.TaxId,
                               TaxPrc = t.TaxPercentage,
                           }
                       ).ToList();

                foreach (var p in producttax)
                {
                    GRNTaxViewModel tv = new GRNTaxViewModel();
                    tv.ProductId = p.ProductId;
                    tv.TaxId = p.TaxId;
                    tv.TaxPrc = p.TaxPrc;
                    tvvm.Add(tv);
                }


                if (producttax != null)
                {
                    return tvvm;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}