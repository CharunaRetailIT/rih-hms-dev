using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Promotions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.Promotions
{
    public class BLL_BankBin
    {
        
        private readonly UnitOfWork _unitofwork;
        public BLL_BankBin()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_BankBin(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public List<Bank> GetActiveBanks()
        {
           return _unitofwork.BankRepository.Get(b => b.ISActive == true).ToList() ?? null;
         
        }

        public List<BankBin> GetActiveBankBins()
        {
            var bankbins= _unitofwork.BankBinRepository.Get().ToList() ?? null;
            bankbins.ForEach(b =>
            {
                b.Location = _unitofwork.LocationRepository.GetById(b.LocationId).LocationName;              
            });

            return bankbins;
        }

        public List<CardType> GetActiveCardTypes()
        {
            return _unitofwork.CardTypeRepository.Get(c=>c.IsActive==true).ToList() ?? null;
        }

        public int SubmitCardPromotion(string[] locids, int bankid, int cardid, string cardprefix, string cardname,
                                       decimal discountamt, decimal discountprc, decimal valuefrom, decimal valueto, 
                                       DateTime datefrom, DateTime dateto, TimeSpan starttime, TimeSpan endtime,int companyid)
        {
            try
            {
                int res = 0;

               // List<BankBin> _bankbins = new List<BankBin>();
                foreach (var i in locids)
                {
                   
                    BankBin _bankbin = new BankBin();
                    _bankbin.LocationId = Convert.ToInt32(i);
                    _bankbin.CompanyId = companyid;
                    _bankbin.BankID = bankid;
                    _bankbin.BankName = _unitofwork.BankRepository.GetById(bankid).BankName;
                    _bankbin.CardID = cardid;
                    _bankbin.CardType = _unitofwork.CardTypeRepository.GetById(cardid).CardTypeName;
                    _bankbin.CardPfx = cardprefix;
                    _bankbin.CardName = cardname;
                    _bankbin.DiscountAmount = discountamt;
                    _bankbin.DiscountPercentage = discountprc;
                    _bankbin.ValueFrom = valuefrom;
                    _bankbin.ValueTo = valueto;
                    _bankbin.DateFrom = datefrom;
                    _bankbin.DateTo = dateto;
                    _bankbin.StartTime = starttime;
                    _bankbin.EndTime = endtime;

                   // _bankbins.Add(_bankbin);

                    _unitofwork.BankBinRepository.Insert(_bankbin);
                    _unitofwork.Save();
                    var lastpromotion = _unitofwork.BankBinRepository.GetById(_bankbin.BankBinId);
                    lastpromotion.PromotionID = _bankbin.BankBinId;
                    _unitofwork.BankBinRepository.Update(lastpromotion);
                    res =  _unitofwork.Save();
                }

                if (res > 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception e)
            {
                return 0;
            }
           
        }

        public int RemoveCardPromotions(int rowid)
        {
            _unitofwork.BankBinRepository.Delete(rowid);
            return _unitofwork.Save();
        }


    }
}
