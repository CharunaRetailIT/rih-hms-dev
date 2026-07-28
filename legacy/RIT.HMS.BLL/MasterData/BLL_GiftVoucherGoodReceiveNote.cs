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
    public class BLL_GiftVoucherGoodReceiveNote
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_GiftVoucherGoodReceiveNote()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_GiftVoucherGoodReceiveNote(string connection)
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
        public invGiftVoucherPurchaseHeaders GetPurchaseHeaderID(string DocumentNO)
        {
            try
            {

                return _unitofwork.GVPHRepository.Get(g => g.DocumentNo == DocumentNO).OrderByDescending(s => s.GiftVoucherPurchaseHeaderID).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public IEnumerable<invGiftVoucherPurchaseHeaders> GetPurchaseHeaderIDNew(long DocumentNo)
        {
            try
            {
                IEnumerable<invGiftVoucherPurchaseHeaders> GiftVoucherPO = _unitofwork.GVPHRepository.Get(g => g.InvGiftVoucherPurchaseHeaderID == DocumentNo);
                if (GiftVoucherPO != null)
                {
                    return GiftVoucherPO;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }        
        public IEnumerable<InvGiftVoucherPurchaseDetails> GetPODetails(int POHID)
        {
            try
            {
                IEnumerable<InvGiftVoucherPurchaseDetails> GiftVoucherPOD = _unitofwork.GVPDetailsRepository.Get(g => g.InvGiftVoucherPurchaseHeaderID == POHID);
                if (GiftVoucherPOD != null)
                {
                    return GiftVoucherPOD;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public int SaveGiftVoucherDetails(InvGiftVoucherPurchaseDetails GiftVoucherPDetails)
        {
            //  _unitofwork.CreateTransaction();
            try
            {
                //int res1 = _unitofwork.Save();
                _unitofwork.GVPDetailsRepository.Insert(GiftVoucherPDetails);
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
        public int SaveGiftVoucherGRN(invGiftVoucherPurchaseHeaders GiftVoucherGRN)
        {
            //  _unitofwork.CreateTransaction();
            try
            {
                //int res1 = _unitofwork.Save();
                _unitofwork.GVPHRepository.Insert(GiftVoucherGRN);
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
        public IEnumerable<invGiftVoucherPurchaseHeaders> GetGiftVoucherPH()
        {
            try
            {
                IEnumerable<invGiftVoucherPurchaseHeaders> GiftVoucherPH = _unitofwork.GVPHRepository.Get();
                if (GiftVoucherPH != null)
                {
                    return GiftVoucherPH;
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
