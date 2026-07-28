using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_DeliveryPerson
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_DeliveryPerson()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_DeliveryPerson(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<DeliveryPerson> GetDeliveryPersons(Int32 compid)
        {
            try
            {
                //Commented by pavi on 2019-12-01
                //IEnumerable<DeliveryPerson> rstdeliverypersons = _unitofwork.DeliveryPersonRepository.Get().OrderBy(ug => ug.FullName);
                IEnumerable<DeliveryPerson> rstdeliverypersons = _unitofwork.DeliveryPersonRepository.Get(c => c.IsDelete.Equals(false) && c.CompanyID==compid).OrderBy(ug => ug.FullName);
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


        public IEnumerable<DeliveryPerson> GetActiveDeliveryPersons(Int32 compid)
        {
            try
            {
                IEnumerable<DeliveryPerson> rstdeliverypersons = _unitofwork.DeliveryPersonRepository.Get(ug => ug.IsDelete == false && ug.CompanyID==compid).OrderBy(ug => ug.FullName);
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
                DeliveryPerson rstdeliverypersons = _unitofwork.DeliveryPersonRepository.Get(ug => ug.EmployeeId == id).FirstOrDefault();
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
                DeliveryPerson rstdeliverypersons = _unitofwork.DeliveryPersonRepository.Get(ug => ug.DeliveryPersonId == id).FirstOrDefault();
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

        public int SaveDeliveryPerson(DeliveryPerson dp)
        {
            try
            {
                _unitofwork.DeliveryPersonRepository.Insert(dp);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdatedeliveryPerson(DeliveryPerson dp)
        {
            try
            {
                _unitofwork.DeliveryPersonRepository.Update(dp);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }









    }
}
