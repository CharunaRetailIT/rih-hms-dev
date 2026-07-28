using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Logs;
using RIT.HMS.Domain.Loyalty;
using RIT.HMS.Domain.ViewModels;

namespace RIT.HMS.BLL.MasterData
{
  public   class BLL_TourAgentCompany
  {
        private readonly UnitOfWork _unitofwork;
        public BLL_TourAgentCompany()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_TourAgentCompany(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<TourAgentCompany> GetTourAgentCompany(int companyid)
        {
            try
            {
                IEnumerable<TourAgentCompany> tourAgentCompany = _unitofwork.TourAgentCompanyRepository.Get(c => c.IsActive == true && c.CompanyID == companyid).OrderBy(g => g.TourAgentCompanyCode);
                if (tourAgentCompany != null)
                {
                    return tourAgentCompany;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public TourAgentCompany GetTourAgentCompanyByCode(string code, Int32 companyid)
        {
            try
            {
                TourAgentCompany tourAgentCompany = _unitofwork.TourAgentCompanyRepository.Get(g => g.TourAgentCompanyCode == code && g.CompanyID == companyid).FirstOrDefault();
                if (tourAgentCompany != null)
                {
                    return tourAgentCompany;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public TourAgentCompany GetTourAgentCompanyById(long id)
        {
            try
            {
                TourAgentCompany tourAgentCompany = _unitofwork.TourAgentCompanyRepository.Get(g => g.TourAgentCompanyID == id).FirstOrDefault();
                if (tourAgentCompany != null)
                {
                    return tourAgentCompany;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveTourAgentCompany(TourAgentCompany touragtcom)
        {
            try
            {
                _unitofwork.TourAgentCompanyRepository.Insert(touragtcom);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateTourAgentCompany(TourAgentCompany touragtcom)
        {
            try
            {
                _unitofwork.CreateTransaction();
                _unitofwork.TourAgentCompanyRepository.Update(touragtcom);
                int x = _unitofwork.Save();

                _unitofwork.Commit();

                return x;

            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return 0;
            }
        }

    }
}
