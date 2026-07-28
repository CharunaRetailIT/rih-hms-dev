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
    public class BLL_GiftVoucherTransfer
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_GiftVoucherTransfer()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_GiftVoucherTransfer(string connection)
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
        public int SaveGiftVoucherTransferDetails(InvGiftVoucherTransferNoteDetail GiftVoucherTransferDetails)
        {
            //  _unitofwork.CreateTransaction();
            try
            {
                //int res1 = _unitofwork.Save();
                _unitofwork.GVTransferDetailsRepository.Insert(GiftVoucherTransferDetails);
                int res1 = _unitofwork.Save();
                // _unitofwork.Commit();

                return res1;
            }
            catch (Exception ex)
            {
                // _unitofwork.Rollback();
                return 0;
            }

        }
        public IEnumerable<InvGiftVoucherTransferNoteHeader> GetGiftvoucherTranfer()
        {
            try
            {
                IEnumerable<InvGiftVoucherTransferNoteHeader> GiftVoucherTransfer = _unitofwork.GVTransferRepository.Get();
                if (GiftVoucherTransfer != null)
                {

                    return GiftVoucherTransfer;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public InvGiftVoucherTransferNoteHeader GetGiftvoucherTranferHeaderID()
        {
            try
            {

                return _unitofwork.GVTransferRepository.Get(g => g.DataTransfer == 0).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public int SaveGiftVoucherTransfer(InvGiftVoucherTransferNoteHeader GiftVoucherTransfer)
        {
            //  _unitofwork.CreateTransaction();
            try
            {
                //int res1 = _unitofwork.Save();
                _unitofwork.GVTransferRepository.Insert(GiftVoucherTransfer);
                int res1 = _unitofwork.Save();
                // _unitofwork.Commit();

                return res1;
            }
            catch (Exception ex)
            {
                // _unitofwork.Rollback();
                return 0;
            }
        }
        public IEnumerable<GiftVoucherGoodReceiveNote> GetOldGRNCode()
        {
            try
            {
                IEnumerable<GiftVoucherGoodReceiveNote> GiftVoucherGRN = _unitofwork.GiftVoucherGoodReceiveNoteRepository.Get().OrderBy(g => g.BookCode);
                if (GiftVoucherGRN != null)
                {
                    return GiftVoucherGRN;
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
