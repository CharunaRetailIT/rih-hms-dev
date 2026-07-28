using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_PaymentTerm
    {
        private readonly UnitOfWork _unitofwork;

        public BLL_PaymentTerm()
        {
            _unitofwork = new UnitOfWork();
        }

        public BLL_PaymentTerm(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<PaymentTerm> GetAllPaymentTerms(Int32 compid)
        {
            try
            {
                IEnumerable<PaymentTerm> paymentterms = _unitofwork.PaymentTermRepository.Get(c=>c.CompanyID==compid).OrderBy(c => c.PaymentTermName);
                return paymentterms ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public PaymentTerm GetPaymentTermById(long id)
        {
            try
            {
                PaymentTerm paymentterm = _unitofwork.PaymentTermRepository.GetById(id);
                if (paymentterm != null)
                {
                    return paymentterm;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdatePaymentMethod(PaymentTerm payterm)
        {
            try
            {
                _unitofwork.PaymentTermRepository.Update(payterm);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int SavePaymentMethod(PaymentTerm payterm)
        {
            try
            {
                _unitofwork.PaymentTermRepository.Insert(payterm);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public PaymentTerm GetPaymentMethodByCode(string code, Int32 compid)
        {
            try
            {
                PaymentTerm paymentterm = _unitofwork.PaymentTermRepository.Get(g => g.PaymentTermCode == code && g.CompanyID==compid).FirstOrDefault();
                if (paymentterm != null)
                {
                    return paymentterm;
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
