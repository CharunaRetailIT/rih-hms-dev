using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HospitalityManagement.Models;
//using System.Activities.Statements;
using HospitalityManagement.Models.ViewModels;

namespace HospitalityManagement.Service
{
    public class AddonCategoryService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<AddonCategoryMaster> GetAddonCategory()
        {
            try
            {
                IEnumerable<AddonCategoryMaster> AddonCategory = context.AddonCategoryMaster.Where(p => p.IsDelete == false);

                if (AddonCategory != null)
                {
                    return AddonCategory;
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