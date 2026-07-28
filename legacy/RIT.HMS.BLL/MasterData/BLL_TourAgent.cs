using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Logs;
using RIT.HMS.Domain.Loyalty;
using RIT.HMS.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_TourAgent
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_TourAgent()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_TourAgent(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<TourAgent> GetTourAgents(int companyid)
        {
            try
            {
                IEnumerable<TourAgent> tourAgent = _unitofwork.TourAgentRepository.Get(c => c.IsActive == true && c.CompanyID == companyid).OrderBy(c => c.AgentCode);
                if (tourAgent != null)
                {
                    return tourAgent;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public TourAgent GetTourAgentByCode(string code, Int32 companyid)
        {
            try
            {
                TourAgent touragent = _unitofwork.TourAgentRepository.Get(g => g.AgentCode == code && g.CompanyID == companyid).FirstOrDefault();
                if (touragent != null)
                {
                    return touragent;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public TourAgent GetTourAgentById(long id)
        {
            try
            {
                TourAgent touragent = _unitofwork.TourAgentRepository.Get(g => g.TourAgentID == id).FirstOrDefault();
                if (touragent != null)
                {
                    return touragent;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveTourAgent(TourAgent touragt)
        {
            try
            {
                _unitofwork.TourAgentRepository.Insert(touragt);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateTourAgent(TourAgent touragent)
        {
            try
            {
                _unitofwork.CreateTransaction();
                _unitofwork.TourAgentRepository.Update(touragent);
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
        public IEnumerable<TourAgentCompany> GetActiveTourAgentCompany(Int32 compid)
        {
            try
            {
                IEnumerable<TourAgentCompany> tourAgCom = _unitofwork.TourAgentCompanyRepository.Get(g => g.IsDelete == false
                                                        && g.CompanyID == compid).OrderBy(g => g.TourAgentCompanyCode);
                if (tourAgCom != null)
                {
                    return tourAgCom;
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
