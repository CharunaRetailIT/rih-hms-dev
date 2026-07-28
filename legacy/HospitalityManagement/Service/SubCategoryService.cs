using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class SubCategoryService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<RstSubCategory> GetSubCategories()
        {
            try
            {
                IEnumerable<RstSubCategory> rstsubcategory = context.RstDepartmentSubCategory.Where(s=>s.IsDelete==false).OrderBy(c => c.RstSubCategoryID);
                if (rstsubcategory != null)
                {
                    return rstsubcategory;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<RstSubCategory> GetByCategoryId(long id)
        {
            try
            {
                IEnumerable<RstSubCategory> rstsubcategory = context.RstDepartmentSubCategory.Where(s=>s.RstCategoryID==id).
                                                                                                OrderBy(g => g.RstSubCategoryCode);
                if (rstsubcategory != null)
                {
                    return rstsubcategory;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<RstSubCategory> GetActiveSubCategories()
        {
            try
            {
                //IEnumerable<RstSubCategory> syssubcategory = context.RstDepartmentSubCategory.Where(g => g.IsDelete == false && g.IsActive == true).
                //                                                                              OrderBy(g => g.RstSubCategoryCode);

               var syssubcategory = context.RstDepartmentSubCategory.Select(c=>new {c.RstSubCategoryID,c.RstSubCategoryName,c.RstSubCategoryCode,c.IsActive,c.IsDelete,c.RstCategoryID }).Where(g => g.IsDelete == false && g.IsActive == true).
                                                                      OrderBy(g => g.RstSubCategoryCode).ToList();

                List<RstSubCategory> subcats = new List<RstSubCategory>();
                foreach (var s in syssubcategory)
                {
                    RstSubCategory subcat = new RstSubCategory();
                    subcat.RstSubCategoryID = s.RstSubCategoryID;
                    subcat.RstCategoryID = s.RstCategoryID;
                    subcat.RstSubCategoryName = s.RstSubCategoryName;
                    subcat.RstSubCategoryCode = s.RstSubCategoryCode;
                    subcat.IsActive = s.IsActive;
                    subcat.IsDelete = s.IsDelete;
                    subcats.Add(subcat);
                }

                if (syssubcategory != null)
                {
                    return subcats;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public RstSubCategory GetSubCategoryById(long id)
        {
            try
            {
                RstSubCategory rstsubcategory = context.RstDepartmentSubCategory.Where(g => g.RstSubCategoryID == id).FirstOrDefault();
                if (rstsubcategory != null)
                {
                    return rstsubcategory;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveSubCategory(RstSubCategory subcat)
        {
            try
            {
                context.RstDepartmentSubCategory.Add(subcat);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateSubCategory(RstSubCategory subcat, RstSubCategory current)
        {
            try
            {
                //context.Entry(subcat).CurrentValues.SetValues(current);

                //  ..context.SysGroupOfCompanys.Add(goc);
                int res = context.SaveChanges();
                
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public RstSubCategory GetSubCatByCode(string code)
        {
            try
            {
                RstSubCategory subcat = context.RstDepartmentSubCategory.Where(g => g.RstSubCategoryCode == code).FirstOrDefault();
                if (subcat != null)
                {
                    return subcat;
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