using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class DeliveryPersonService
    {


        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<DeliveryPerson> GetDeliveryPersons()
        {
            try
            {
                IEnumerable<DeliveryPerson> rstdeliverypersons = context.DeliveryPerson.OrderBy(ug => ug.FullName);
                if (rstdeliverypersons != null)
                {
                    return rstdeliverypersons;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<DeliveryPerson> GetActiveDeliveryPersons()
        {
            try
            {
                IEnumerable<DeliveryPerson> rstdeliverypersons = context.DeliveryPerson.Where(ug => ug.IsDelete == false).OrderBy(ug => ug.FullName);
                if (rstdeliverypersons != null)
                {
                    return rstdeliverypersons;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public DeliveryPerson GetdeliveryPersonByEmpId(string id)
        {
            try
            {
                DeliveryPerson rstdeliverypersons = context.DeliveryPerson.Where(ug => ug.EmployeeId == id).FirstOrDefault();
                if (rstdeliverypersons != null)
                {
                    return rstdeliverypersons;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public DeliveryPerson GetdeliveryPersonById(long id)
        {
            try
            {
                DeliveryPerson rstdeliverypersons = context.DeliveryPerson.Where(ug => ug.DeliveryPersonId == id).FirstOrDefault();
                if (rstdeliverypersons != null)
                {
                    return rstdeliverypersons;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveDeliveryPerson(DeliveryPerson dev)
        {
            try
            {
                context.DeliveryPerson.Add(dev);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdatedeliveryPerson(DeliveryPerson dev)
        {
            try
            {

               
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}