using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class CategoryService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<RstCategory> GetCategories()
        {
            try
            {
                IEnumerable<RstCategory> rstcategory = context.RstDepartmentCategory.Where(c=>c.IsDelete==false).OrderBy(c => c.RstCategoryCode);
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
                IEnumerable<RstCategory> syscategory = context.RstDepartmentCategory.Where(g => g.IsDelete == false && g.CompanyID == id).OrderBy(g => g.RstCategoryCode);
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
        public IEnumerable<RstCategory> GetActiveCategory()
        {
            try
            {

                //IEnumerable<RstCategory> syscategory = context.RstDepartmentCategory.Where(g => g.IsDelete == false && g.IsActive == true).
                //                                                                           OrderBy(g => g.RstCategoryCode);

                var syscat = context.RstDepartmentCategory.Select(c=>new {c.RstCategoryID,c.RstCategoryName,c.RstCategoryCode,c.IsActive,c.IsDelete })
                                                                         .Where(g => g.IsDelete == false && g.IsActive == true).
                                                                                           OrderBy(g => g.RstCategoryCode);
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
                IEnumerable<RstCategory> syscategory = context.RstDepartmentCategory.Where(g => g.IsDelete == false && g.IsActive == true && g.RstDepartmentID==id).OrderBy(g => g.RstCategoryCode);
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
                RstCategory rstcategory = context.RstDepartmentCategory.Where(g => g.RstCategoryID == id).FirstOrDefault();
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

        public int SaveCategory(RstCategory cat)
        {
            try
            {
                context.RstDepartmentCategory.Add(cat);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateCategory(RstCategory cat)
        {
            try
            {

                //  ..context.SysGroupOfCompanys.Add(goc);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public RstCategory GetCatByCode(string code)
        {
            try
            {
                RstCategory cat = context.RstDepartmentCategory.Where(g => g.RstCategoryCode == code).FirstOrDefault();
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




    }
}