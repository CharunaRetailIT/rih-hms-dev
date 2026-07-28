using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class UnitOfMeasureService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<UnitOfMeasure> GetUnitOfMeasures()
        {
            try
            {
                IEnumerable<UnitOfMeasure> unitofmeasure = context.UnitOfMeasure.OrderBy(um => um.UnitOfMeasureCode);
                if (unitofmeasure != null)
                {
                    return unitofmeasure;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<UnitOfMeasure> GetActiveUnitOfMeasures()
        {
            try
            {
                IEnumerable<UnitOfMeasure> unitofmeasure = context.UnitOfMeasure.Where(um => um.IsDelete == false).OrderBy(um => um.UnitOfMeasureCode);
                if (unitofmeasure != null)
                {
                    return unitofmeasure;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public UnitOfMeasure GetUnitOfMeasureById(long id)
        {
            try
            {
                UnitOfMeasure unitofmeasure = context.UnitOfMeasure.Where(um => um.UnitOfMeasureId == id).FirstOrDefault();
                if (unitofmeasure != null)
                {
                    return unitofmeasure;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveUnitOfMeasure(UnitOfMeasure um)
        {
            try
            {
                context.UnitOfMeasure.Add(um);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateUnitOfMeasure(UnitOfMeasure um)
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


        public UnitOfMeasure GetUnitOfMeasureByCode(string code)
        {
            try
            {
                UnitOfMeasure unitofmeasure = context.UnitOfMeasure.Where(g => g.UnitOfMeasureCode == code).FirstOrDefault();
                if (unitofmeasure != null)
                {
                    return unitofmeasure;
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