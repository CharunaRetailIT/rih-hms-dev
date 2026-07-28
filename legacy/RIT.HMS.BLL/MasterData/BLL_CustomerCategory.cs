using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public  class BLL_CustomerCategory
    {

        private readonly UnitOfWork _unitofwork;
        public BLL_CustomerCategory()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_CustomerCategory(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }

        public IEnumerable<CustomerCategory> GetCustomerCategories(Int32 compayid)
        {
            try
            {
                //Below line commented and added new line by pavi on 2019-12-01
                //IEnumerable<CustomerCategory> customercategory = _unitofwork.CustomerCategoryRepository.Get().OrderBy(c => c.CustomerCategoryCode);
                IEnumerable<CustomerCategory> customercategory = _unitofwork.CustomerCategoryRepository.Get(c => c.IsDelete.Equals(false) && c.CompanyID== compayid).OrderBy(c => c.CustomerCategoryCode);
                if (customercategory != null)
                {
                    return customercategory;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<CustomerCategory> GetActiveCustomerCategories(Int32 compayid)
        {
            try
            {
                IEnumerable<CustomerCategory> customercategory = _unitofwork.CustomerCategoryRepository.Get(c => c.IsDelete == false && c.GroupOfCompanyID == compayid).OrderBy(c => c.CustomerCategoryCode);
                if (customercategory != null)
                {
                    return customercategory;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public CustomerCategory GetCustomercategoryById(long id)
        {
            try
            {
                CustomerCategory customercategory = _unitofwork.CustomerCategoryRepository.Get(c => c.CustomerCategoryID == id).FirstOrDefault();
                if (customercategory != null)
                {
                    return customercategory;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public CustomerCategory GetCustomercategoryByCode(string code,Int32 companyid)
        {
            try
            {
                CustomerCategory customercategory = _unitofwork.CustomerCategoryRepository.Get(c => c.CustomerCategoryCode == code && c.CompanyID== companyid).FirstOrDefault();
                if (customercategory != null)
                {
                    return customercategory;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveCustomerCategory(CustomerCategory c)
        {
            try
            {
                _unitofwork.CustomerCategoryRepository.Insert(c);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateCustomerCategory(CustomerCategory c)
        {
            try
            {
                _unitofwork.CustomerCategoryRepository.Update(c);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

    }
}
