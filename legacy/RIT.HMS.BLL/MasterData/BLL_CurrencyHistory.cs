using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_CurrencyHistory
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_CurrencyHistory()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_CurrencyHistory(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<CurrencyHistory> GetCurrencyHistory()
        {
            try
            {
                IEnumerable<CurrencyHistory> syscCurrencyHistory = _unitofwork.CurrencyHistoryRepository.Get().OrderBy(c => c.CurrencyId);
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
                IEnumerable<CurrencyHistory> currencies = _unitofwork.CurrencyHistoryRepository.Get().OrderBy(ug => ug.CurrencyId);
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
                IEnumerable<CurrencyHistory> currencyhistory = _unitofwork.CurrencyHistoryRepository.Get(ch=>ch.CurrencyId==id);
                return currencyhistory;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public int SaveCurrencyHistory(CurrencyHistory ch)
        {
            try
            {
                _unitofwork.CurrencyHistoryRepository.Insert(ch);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateCurrencyHistory(CurrencyHistory ch)
        {
            try
            {
                _unitofwork.CurrencyHistoryRepository.Update(ch);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }







    }
}
