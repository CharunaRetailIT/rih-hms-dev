using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_Vehicle
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Vehicle()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Vehicle(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<Vehicle> GetVehicles()
        {
            try
            {
                IEnumerable<Vehicle> vehicle = _unitofwork.VehicleRepository.Get().OrderBy(v => v.RegistrationNo);
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
                IEnumerable<Vehicle> vehicle = _unitofwork.VehicleRepository.Get(v => v.IsDelete == false).OrderBy(v => v.RegistrationNo);
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
                Vehicle vehicle = _unitofwork.VehicleRepository.Get(v => v.VehicleID == id).FirstOrDefault();
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

        public Vehicle GetVehicleByRegistrationNo(string regno)
        {
            try
            {
                Vehicle customer = _unitofwork.VehicleRepository.Get(g => g.RegistrationNo == regno).FirstOrDefault();
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

        public int SaveVehicle(Vehicle v)
        {
            try
            {
                _unitofwork.VehicleRepository.Insert(v);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateVehicle(Vehicle v)
        {
            try
            {
                _unitofwork.VehicleRepository.Update(v);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }






    }
}
