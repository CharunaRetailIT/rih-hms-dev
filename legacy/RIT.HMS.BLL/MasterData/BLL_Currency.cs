using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_Currency
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Currency()
        {
            _unitofwork = new UnitOfWork();
        }

        public BLL_Currency(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<Currency> GetCurrencies()
        {
            try
            {
                IEnumerable<Currency> syscCurrencies = _unitofwork.CurrencyRepository.Get().OrderBy(c => c.CurrencyCode);
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
                IEnumerable<Currency> currencies = _unitofwork.CurrencyRepository.Get(ug => ug.IsDelete == false).OrderBy(ug => ug.CurrencyCode);
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
                IEnumerable<Currency> currencies = _unitofwork.CurrencyRepository.Get(ug => ug.IsDelete == false && ug.IsActive == true).OrderBy(ug => ug.CurrencyCode);
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
                var currency = _unitofwork.CurrencyRepository.GetById(id);
                return currency;
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
                Currency currency = _unitofwork.CurrencyRepository.Get(g => g.CurrencyCode == code).FirstOrDefault();
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


        public int SaveCurrency(Currency c)
        {
            try
            {
                _unitofwork.CurrencyRepository.Insert(c);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateCurrency(Currency c)
        {
            try
            {
                _unitofwork.CurrencyRepository.Update(c);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        //Added by pavithra 2019-12-06
        public IEnumerable<Currency> GetCurrencyByID(int currencyID)
        {
            try
            {
                IEnumerable<Currency> currencies = _unitofwork.CurrencyRepository.Get(ug => ug.IsDelete == false && ug.IsActive == true && ug.CurrencyId == currencyID);
                return currencies ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }




    }
}
