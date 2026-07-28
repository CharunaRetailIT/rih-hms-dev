using RIT.HMS.Data;
using RIT.HMS.Domain.Common;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_BudgetOutlet
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_BudgetOutlet()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_BudgetOutlet(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public int SaveBudgetOutlet(BudgetOutlet _BudgetOutlet)
        {
            
            try
            {
                _unitofwork.CreateTransaction();
                _unitofwork.BudgetOutletRepository.Insert(_BudgetOutlet);
                int res = _unitofwork.Save();                
                _unitofwork.Commit();
                return res;
            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return 0;
            }
        }
        public IEnumerable<BudgetOutlet> GetBudgetOutletWise()
        {
            try
            {
                IEnumerable<BudgetOutlet> BudgetOutletDetails = _unitofwork.BudgetOutletRepository.Get().OrderBy(g => g.BudgetOutletID);
                if (BudgetOutletDetails != null)
                {
                    return BudgetOutletDetails;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public IEnumerable<BudgetOutlet> GetBudgetOutletWiseID(int BudgetOutletID)
        {
            try
            {
                IEnumerable<BudgetOutlet> BudgetOutletDetails = _unitofwork.BudgetOutletRepository.Get(c => c.BudgetOutletID == BudgetOutletID).OrderBy(g => g.BudgetOutletID);
                if (BudgetOutletDetails != null)
                {
                    return BudgetOutletDetails;
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
