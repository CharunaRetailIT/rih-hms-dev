using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HospitalityManagement.Models;

namespace HospitalityManagement.Service
{
    public class UnitConversionService
    {
       public readonly ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<UnitConversion> GetUnitConversions()
        {
            try
            {
                IEnumerable<UnitConversion> unitconversions = context.UnitConversion.OrderBy(c => c.UnitConversionId);
                if (unitconversions != null)
                {
                    return unitconversions;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<UnitConversion> GetConversionById(long id)
        {
            try
            {
                IEnumerable<UnitConversion> unitConversions = context.UnitConversion.Where(g =>  g.UnitConversionId == id).
                                                                                                OrderBy(g => g.UnitConversionId);
                return unitConversions ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
       

        public IEnumerable<UnitConversion> GetConversionByMeasurementId(long id)
        {
            try
            {
                IEnumerable<UnitConversion> unitConversions = context.UnitConversion.Where(g => g.UnitOfMeasureId == id);

                return unitConversions ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveUnitConversions(List<UnitConversion> conversions,string loggeduser)
        {
            var res = 0;

            using ( var dbtransaction= context.Database.BeginTransaction())
            {

                DeleteConversionsByUnitOfMeasureId(conversions.FirstOrDefault().UnitOfMeasureId);


                try
                {
               
                    foreach (var unitConversion in conversions)
                    {
                        unitConversion.CreatedUser = loggeduser;
                        unitConversion.CompanyID = 0;
                        unitConversion.GroupOfCompanyID = 0;
                        context.UnitConversion.Add(unitConversion);
                        res = context.SaveChanges();
                        if (res != 0) continue;
                        dbtransaction.Rollback();
                        return res;

                    }
                    dbtransaction.Commit();

                }
                catch (Exception ex)
                {
                    dbtransaction.Rollback();
                    throw;
                }
            }

            return res;
        }
        public int DeleteConversionsByUnitOfMeasureId(long id)
        {
            try
            {


            
                context.UnitConversion.RemoveRange(context.UnitConversion.Where(x => x.UnitOfMeasureId == id));
                int res = context.SaveChanges();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public int UpdateUnitConversion(UnitConversion conversion)
        {
            try
            {

                //  ..context.SysGroupOfCompanys.Add(goc);
                var res = context.SaveChanges();

                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}