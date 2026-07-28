using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_Category
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Category()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Category(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<RstCategory> GetCategories(Int32 compid)
        {
            try
            {
                IEnumerable<RstCategory> rstcategory = _unitofwork.CategoryRepository.Get(c => c.IsDelete == false && c.CompanyID==compid).OrderBy(c => c.RstCategoryCode);
                if (rstcategory != null)
                {
                    return rstcategory;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<RstCategory> GetByDepartmentId(long id)
        {
            try
            {
                IEnumerable<RstCategory> syscategory = _unitofwork.CategoryRepository.Get(g => g.IsDelete == false && g.CompanyID == id).OrderBy(g => g.RstCategoryCode);
                if (syscategory != null)
                {
                    return syscategory;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<RstCategory> GetActiveCategory(Int32 compid)
        {
            try
            {

                //IEnumerable<RstCategory> syscategory = context.RstDepartmentCategory.Where(g => g.IsDelete == false && g.IsActive == true).
                //                                                                           OrderBy(g => g.RstCategoryCode);

                var syscat = _unitofwork.CategoryRepository.Get(g => g.IsDelete == false && g.IsActive == true && g.CompanyID==compid).
                                                                                           OrderBy(g => g.RstCategoryCode).Select(c => new { c.RstCategoryID, c.RstCategoryName, c.RstCategoryCode, c.IsActive, c.IsDelete });
                List<RstCategory> cats = new List<RstCategory>();
                foreach (var c in syscat)
                {
                    RstCategory cat = new RstCategory();
                    cat.RstCategoryID = c.RstCategoryID;
                    cat.RstCategoryName = c.RstCategoryName;
                    cat.RstCategoryCode = c.RstCategoryCode;
                    cat.IsActive = c.IsActive;
                    cat.IsDelete = c.IsDelete;
                    cats.Add(cat);
                }



                if (cats != null)
                {
                    return cats;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<RstCategory> GetCategoryByDepartmentId(long id)
        {
            try
            {
                IEnumerable<RstCategory> syscategory = _unitofwork.CategoryRepository.Get(g => g.IsDelete == false && g.IsActive == true && g.RstDepartmentID == id).OrderBy(g => g.RstCategoryCode);
                if (syscategory != null)
                {
                    return syscategory;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public RstCategory GetCategoryById(long id)
        {
            try
            {
                RstCategory rstcategory = _unitofwork.CategoryRepository.Get(g => g.RstCategoryID == id).FirstOrDefault();
                if (rstcategory != null)
                {
                    return rstcategory;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public RstCategory GetCatByCode(string code,Int32 compid)
        {
            try
            {
                RstCategory cat = _unitofwork.CategoryRepository.Get(g => g.RstCategoryCode == code && g.CompanyID==compid).FirstOrDefault();
                if (cat != null)
                {
                    return cat;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public int SaveCategory(RstCategory cat)
        {
            try
            {
                _unitofwork.CategoryRepository.Insert(cat);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateCategory(RstCategory cat)
        {
            try
            {
                _unitofwork.CategoryRepository.Update(cat);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        //Added by pavithra on 2019-11-30
        public RstCategory FindByCode(string code,Int32 compid)
        {
            var category = _unitofwork.CategoryRepository.Get(c => c.RstCategoryCode == code && c.CompanyID==compid).FirstOrDefault();
            if (category != null)
            {
                return category;
            }
            else
            {
                return null;
            }

        }

    }
}
