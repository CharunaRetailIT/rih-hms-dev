using RIT.HMS.Data;
using RIT.HMS.Domain.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.Configurations
{
    public class BLL_Configuration
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Configuration()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Configuration(string actualdb)
        {
            _unitofwork = new UnitOfWork(actualdb);
        }

        public Configuration GetConfiguration(string configkey,int companyid)
        {
            return _unitofwork.ConfigurationRepository.Get(c=>c.ConfigurationKey==configkey && c.CompanyId==companyid).FirstOrDefault();
        }
    }
}
