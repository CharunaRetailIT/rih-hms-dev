using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HospitalityManagement.Models;

namespace HospitalityManagement.Service
{
    public class CurrencyHistoryService
    {

       private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public IEnumerable<CurrencyHistory> GetCurrencyHistory()
        {
            try
            {
                IEnumerable<CurrencyHistory> syscCurrencyHistory = _context.CurrencyHistory.OrderBy(c => c.CurrencyId);
                return syscCurrencyHistory ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<CurrencyHistory> GetActiveCurrenyHistory()
        {
            try
            {
                IEnumerable<CurrencyHistory> currencies = _context.CurrencyHistory.OrderBy(ug => ug.CurrencyId);
                return currencies ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<CurrencyHistory> GetCurrencyHistoryByCurrencyId(long id)
        {
            try
            {
               IEnumerable<CurrencyHistory> currencyhistory = _context.CurrencyHistory.Where(ug => ug.CurrencyId == id);
                return currencyhistory;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveCurrencyHistory(CurrencyHistory currencyhistory)
        {
            try
            {
                _context.CurrencyHistory.Add(currencyhistory);
                int res = _context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateCurrencyHistory(CurrencyHistory currencyhistory)
        {
            try
            {

                //  ..context.SysGroupOfCompanys.Add(goc);
                int res = _context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }




    }
}