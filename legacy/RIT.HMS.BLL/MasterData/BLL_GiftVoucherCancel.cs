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
    public class BLL_GiftVoucherCancel
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_GiftVoucherCancel()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_GiftVoucherCancel(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public int SaveGiftVoucherCancel(InvGiftVoucherCancel giftVoucherCancel)
        {
            
            try
            {
                _unitofwork.CreateTransaction();
                RIT.HMS.Domain.Transactions.InvGiftVoucherMaster obj = new RIT.HMS.Domain.Transactions.InvGiftVoucherMaster();
                obj.VoucherSerial = giftVoucherCancel.VoucherNo;
                obj.ModifiedDate = giftVoucherCancel.ModifiedDate;
                obj.ModifiedUser =giftVoucherCancel.ModifiedUser;                
                obj.IsCancel = true;
                giftVoucherCancel.Remark = "Voucher Cancel";
                _unitofwork.GiftVoucherCancelRepository.Insert(giftVoucherCancel);
                int res = _unitofwork.Save();
                int res1 = 0;
                if (res != 0)
                {
                    var exists = _unitofwork.GiftVoucherMasterRepository.Get(m => m.VoucherSerial == giftVoucherCancel.VoucherNo).SingleOrDefault();
                    if (exists != null)
                    {
                        exists.IsCancel = true;
                        exists.ModifiedDate= giftVoucherCancel.ModifiedDate;
                        exists.ModifiedUser = giftVoucherCancel.ModifiedUser;
                        //Gift Voucher Master Table Update
                        _unitofwork.GiftVoucherMasterRepository.Update(exists);
                        res1 = _unitofwork.Save();
                    }                    
                }
                _unitofwork.Commit();
                return res;
            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return 0;
            }
        }
        public IEnumerable<InvGiftVoucherMaster> GetGiftVoucherDetails(string VoucherNo)
        {
            try
            {
                IEnumerable<InvGiftVoucherMaster> GiftVoucherDetails = _unitofwork.GiftVoucherMasterRepository.Get(c => c.IsCancel == false && c.VoucherNo == VoucherNo).OrderBy(g => g.VoucherNo);
                if (GiftVoucherDetails != null)
                {
                    return GiftVoucherDetails;
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
