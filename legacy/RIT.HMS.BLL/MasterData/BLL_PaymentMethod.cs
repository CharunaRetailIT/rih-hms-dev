using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_PaymentMethod
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_PaymentMethod()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_PaymentMethod(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<PaymentMethod> GetAllPaymentMethods(Int32 compid)
        {
            try
            {
                IEnumerable<PaymentMethod> paymentmethods = _unitofwork.PaymentMethodRepository.Get(c=>c.CompanyID==compid).OrderBy(c => c.PaymentMethodName);
                return paymentmethods ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public PaymentMethod GetPaymentMethodById(long id)
        {
            try
            {
                PaymentMethod paymentmethod = _unitofwork.PaymentMethodRepository.GetById(id);
                if (paymentmethod != null)
                {
                    return paymentmethod;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdatePaymentMethod(PaymentMethod paymethod)
        {
            try
            {
                _unitofwork.PaymentMethodRepository.Update(paymethod);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int SavePaymentMethod(PaymentMethod paymethod)
        {
            try
            {
                _unitofwork.PaymentMethodRepository.Insert(paymethod);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public PaymentMethod GetPaymentMethodByCode(string code, Int32 compid)
        {
            try
            {
                PaymentMethod paymentmethod = _unitofwork.PaymentMethodRepository.Get(g => g.PaymentMethodCode == code && g.CompanyID==compid).FirstOrDefault();
                if (paymentmethod != null)
                {
                    return paymentmethod;
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
