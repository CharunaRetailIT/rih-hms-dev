using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class CheckDependencyService
    {
        ApplicationDbContext context = new ApplicationDbContext();
        public bool CheckDependency(string formname)
        {
            try
            {
                bool  IsDepend = context.AutoGenerateInfo.Where(g => g.FormName == formname).FirstOrDefault().IsDepend;
                if (IsDepend != null)
                {
                    return IsDepend;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}