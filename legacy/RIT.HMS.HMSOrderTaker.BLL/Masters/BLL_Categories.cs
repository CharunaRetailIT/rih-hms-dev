using RIT.HMS.HMSOrderTaker.Data;
using RIT.HMS.HMSOrderTaker.Domain;
using RIT.HMS.HMSOrderTaker.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.BLL.Masters
{
    public class BLL_Categories
    {
        private UnitOfWork<SmartLinkEntities> unitOfWork;
        public BLL_Categories()
        {
            unitOfWork = new UnitOfWork<SmartLinkEntities>();

        }

        public IEnumerable<DTO_Category> GetActiveCategoriesByDepartmentIdAndLocationId(int deptid,int locationid)
        {
            var categories = unitOfWork.Tbl_RstCategory.Get(filter: l => l.LocationId == locationid && l.RstDepartmentID==deptid
                               && l.IsActive == true && l.IsDelete == false
                                ).OrderBy(l => l.CatImageName);

            List<DTO_Category> objcategories = new List<DTO_Category>();
            foreach (var cat in categories)
            {
                DTO_Category objcat = new DTO_Category()
                {
                    RstCategoryID = cat.RstCategoryID,
                    RstCategoryCode = cat.RstCategoryCode,
                    RstCategoryName = cat.RstCategoryName,
                    CatImageName = cat.CatImageName,
                    CatImage = cat.CatImage,
                    CatImageType = cat.CatImageType,
                    RstDepartmentID = cat.RstDepartmentID,
                    
                };
                objcategories.Add(objcat);

            }

            return objcategories;

        }
    }
}
