using RIT.HMS.Data;
using RIT.HMS.Domain.Common;
using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_GiftVoucherBookCode
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_GiftVoucherBookCode()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_GiftVoucherBookCode(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public int SaveGiftVoucherBookGenarator(InvGiftVoucherBookCode giftVoucherBookCode)
        {
            try
            {
                _unitofwork.GiftVoucherbookRepository.Insert(giftVoucherBookCode);
                int res1 = _unitofwork.Save();
                return res1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public IEnumerable<InvGiftVoucherBookCode> GetOldBookCode()
        {
            try
            {
                IEnumerable<InvGiftVoucherBookCode> GiftVoucherBookCode = _unitofwork.GiftVoucherbookRepository.Get().OrderBy(g => g.BookCode);
                if (GiftVoucherBookCode != null)
                {
                    return GiftVoucherBookCode;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        public IEnumerable<InvGiftVoucherBookCode> GetGiftVoucherBookCode()
        {
            try
            {
                IEnumerable<InvGiftVoucherBookCode> GiftVoucherBookCode = _unitofwork.GiftVoucherbookRepository.Get().OrderBy(g => g.BookCode);
                if (GiftVoucherBookCode != null)
                {
                    return GiftVoucherBookCode;
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
