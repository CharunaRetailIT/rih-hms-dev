using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class VehicleService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<Vehicle> GetVehicles()
        {
            try
            {
                IEnumerable<Vehicle> vehicle = context.Vehicles.OrderBy(v => v.RegistrationNo);
                if (vehicle != null)
                {
                    return vehicle;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Vehicle> GetActiveVehicles()
        {
            try
            {
                IEnumerable<Vehicle> vehicle = context.Vehicles.Where(v => v.IsDelete == false).OrderBy(v => v.RegistrationNo);
                if (vehicle != null)
                {
                    return vehicle;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Vehicle GetVehicleById(long id)
        {
            try
            {
                Vehicle vehicle = context.Vehicles.Where(v => v.VehicleID == id).FirstOrDefault();
                if (vehicle != null)
                {
                    return vehicle;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveVehicle(Vehicle v)
        {
            try
            {
                context.Vehicles.Add(v);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateVehicle(Vehicle v)
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

        public Vehicle GetVehicleByRegistrationNo(string regno)
        {
            try
            {
                Vehicle customer = context.Vehicles.Where(g => g.RegistrationNo == regno).FirstOrDefault();
                if (customer != null)
                {
                    return customer;
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