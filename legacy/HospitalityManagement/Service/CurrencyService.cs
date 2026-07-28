using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class CurrencyService
    {


        readonly ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<Currency> GetCurrencies()
        {
            try
            {
                IEnumerable<Currency> syscCurrencies = context.Currency.OrderBy(c =>c.CurrencyCode);
                return syscCurrencies ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Currency> GetActiveCurrencies()
        {
            try
            {
                IEnumerable<Currency> currencies = context.Currency.Where(ug => ug.IsDelete == false).OrderBy(ug => ug.CurrencyCode);
                return currencies ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public IEnumerable<Currency> GetCurrenciesForTransactions()
        {
            try
            {
                IEnumerable<Currency> currencies = context.Currency.Where(ug => ug.IsDelete == false && ug.IsActive==true).OrderBy(ug => ug.CurrencyCode);
                return currencies ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Currency GetCurrencyById(long id)
        {
            try
            {
                var currency = context.Currency.FirstOrDefault(ug => ug.CurrencyId == id);
                return currency;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveCurrency(Currency currency)
        {
            try
            {
                context.Currency.Add(currency);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateCurrency(Currency currency)
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


        public Currency GetCurrencyByCode(string code)
        {
            try
            {
                Currency currency = context.Currency.Where(g => g.CurrencyCode == code).FirstOrDefault();
                if (currency != null)
                {
                    return currency;
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