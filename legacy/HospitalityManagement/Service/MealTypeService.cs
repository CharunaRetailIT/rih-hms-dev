using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class MealTypeService
    {

        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<RstMealType> GetMeals()
        {
            try
            {
                IEnumerable<RstMealType> rstmealtype = context.RstMealType.OrderBy(m => m.RstMealTypeCode);
                if (rstmealtype != null)
                {
                    return rstmealtype;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<RstMealType> GetActiveMealss()
        {
            try
            {
                IEnumerable<RstMealType> rstmealtype = context.RstMealType.Where(m => m.IsActive == true).OrderBy(m => m.RstMealTypeCode);
                if (rstmealtype != null)
                {
                    return rstmealtype;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public RstMealType GetMealTypeById(long id)
        {
            try
            {
                RstMealType rstmealtype = context.RstMealType.Where(m => m.RstMealTypeId == id).FirstOrDefault();
                if (rstmealtype != null)
                {
                    return rstmealtype;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveMealType(RstMealType m)
        {
            try
            {
                context.RstMealType.Add(m);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateMealType(RstMealType m)
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

        public RstMealType GetMealTypeByCode(string code)
        {
            try
            {
                RstMealType rstmealtype = context.RstMealType.Where(m => m.RstMealTypeCode == code).FirstOrDefault();
                if (rstmealtype != null)
                {
                    return rstmealtype;
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