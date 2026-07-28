using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
   public class BLL_SubCategory
    {

        private readonly UnitOfWork _unitofwork;
        public BLL_SubCategory()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_SubCategory(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }

        public IEnumerable<RstSubCategory> GetSubCategories(Int32 compid)
        {
            try
            {
                IEnumerable<RstSubCategory> rstsubcategory = _unitofwork.SubCategoryRepository.Get(s => s.IsDelete == false && s.CompanyID==compid).OrderBy(c => c.RstSubCategoryID);
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
                IEnumerable<RstSubCategory> rstsubcategory = _unitofwork.SubCategoryRepository.Get(s => s.RstCategoryID == id).
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


        public IEnumerable<RstSubCategory> GetActiveSubCategories(Int32 compid)
        {
            try
            {
                //IEnumerable<RstSubCategory> syssubcategory = context.RstDepartmentSubCategory.Where(g => g.IsDelete == false && g.IsActive == true).
                //                                                                              OrderBy(g => g.RstSubCategoryCode);

                //    var syssubcategory = _unitofwork.SubCategoryRepository.Select(c => new { c.RstSubCategoryID, c.RstSubCategoryName, c.RstSubCategoryCode, c.IsActive, c.IsDelete, c.RstCategoryID }).Where(g => g.IsDelete == false && g.IsActive == true).
                //                                                         OrderBy(g => g.RstSubCategoryCode).ToList();

                var syssubcategory = _unitofwork.SubCategoryRepository.Get(g => g.IsDelete == false && g.IsActive == true && g.CompanyID==compid).
                                                                      OrderBy(g => g.RstSubCategoryCode).Select(c => new { c.RstSubCategoryID, c.RstSubCategoryName, c.RstSubCategoryCode, c.IsActive, c.IsDelete, c.RstCategoryID }).ToList();

               List < RstSubCategory> subcats = new List<RstSubCategory>();
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
                RstSubCategory rstsubcategory = _unitofwork.SubCategoryRepository.Get(g => g.RstSubCategoryID == id).FirstOrDefault();
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


        public RstSubCategory GetSubCatByCode(string code, Int32 compid)
        {
            try
            {
                RstSubCategory subcat = _unitofwork.SubCategoryRepository.Get(g => g.RstSubCategoryCode == code && g.CompanyID==compid).FirstOrDefault();
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

        public int SaveSubCategory(RstSubCategory scat)
        {
            try
            {
                _unitofwork.SubCategoryRepository.Insert(scat);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateSubCategory(RstSubCategory scat, RstSubCategory current)
        {
            try
            {
                _unitofwork.SubCategoryRepository.Update(scat);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }





    }
}
