using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RIT.HMS.Domain.Common;
using RIT.HMS.Data;
namespace RIT.HMS.BLL.Common
{
    public class BLL_ConnectionManager
    {
        readonly UnitOfWork _unitofwork;
        public BLL_ConnectionManager()
        {
            _unitofwork = new UnitOfWork();
        }

        public BLL_ConnectionManager(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }

        public string GetActualConnectionName(string username)
        {
            return _unitofwork.CompanyUserRepository.Get(c=>c.CompanyUserName==username).FirstOrDefault().CompanyDbName;
        }
    }
}
