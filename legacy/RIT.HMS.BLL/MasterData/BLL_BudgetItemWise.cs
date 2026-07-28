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
    public class BLL_BudgetItemWise
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_BudgetItemWise()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_BudgetItemWise(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public int SaveBudgetItemWise(BudgetItemWise _BudgetItemWise)
        {
            
            try
            {
                _unitofwork.CreateTransaction();
                _unitofwork.BudgetItemWiseRepository.Insert(_BudgetItemWise);
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
        public IEnumerable<BudgetOutlet> GetBudgetItemWise()
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
    }
}
