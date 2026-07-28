using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_LocationMapper
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_LocationMapper()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_LocationMapper(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }

        public List<SysLocationMapper> GetActiveAll(Int32 compid)
        {
            try
            {
                List<SysLocationMapper> sm = _unitofwork.LocationMapperRepository.Get(g => g.IsActive == true && g.CompanyID == compid).ToList();
                if (sm != null)
                {
                    return sm;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        public List<SysLocationMapper> GetAllByLocationId(Int32 compid, int locationId)
        {
            var result = new List<SysLocationMapper>();
            try
            {
                result = _unitofwork.LocationMapperRepository.Get(g => g.CompanyID == compid && g.MainLocationId == locationId && g.IsActive == true).ToList();
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }
        public List<SysLocation> MapperdLocationSelect(KitchenAddToLocation kitchenAddToLocation)
        {
            var result = kitchenAddToLocation.KitchenLocationList;
            try
            {
                foreach (var row in kitchenAddToLocation.LocationMapper)
                {
                    var getResult = result.Where(o => o.SysLocationID == row.SubLocationId).FirstOrDefault();
                    getResult.IsSelectLocation = true;
                    getResult.IsSelectLocationIsActive = row.IsActive;
                }

            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public int SaveSubLocation(KitchenAddToLocation kitchenAddToLocation)
        {
            var result = 0;


            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {
                    // _unitofwork.LocationMapperRepository.DeleteRange(kitchenAddToLocation.LocationMapper);
                    List<SysLocationMapper> SysLocationMapperList = new List<SysLocationMapper>();
                    foreach (var r in kitchenAddToLocation.KitchenLocationList)
                    {

                        var isValidateRow = _unitofwork.LocationMapperRepository.Get(g => g.CompanyID == kitchenAddToLocation.GeneralLocation.CompanyID && g.MainLocationId == kitchenAddToLocation.GeneralLocation.SysLocationID && g.SubLocationId == r.SysLocationID).FirstOrDefault();

                        if (isValidateRow == null)
                        {
                            if (r.IsSelectLocation)
                            {
                                var locationMapper = new SysLocationMapper();
                                locationMapper.CompanyID = kitchenAddToLocation.GeneralLocation.CompanyID;
                                locationMapper.GroupOfCompanyID = kitchenAddToLocation.GeneralLocation.GroupOfCompanyID;
                                locationMapper.SubLocationId = r.SysLocationID;
                                locationMapper.MainLocationId = kitchenAddToLocation.GeneralLocation.SysLocationID;
                                locationMapper.CreatedUser = kitchenAddToLocation.CreatedUser;
                                locationMapper.CreatedDate = kitchenAddToLocation.CreatedDate;
                                locationMapper.ModifiedUser = kitchenAddToLocation.ModifiedUser;
                                locationMapper.ModifiedDate = kitchenAddToLocation.ModifiedDate;
                                locationMapper.DataTransfer = 0;
                                locationMapper.IsActive = true;
                                _unitofwork.LocationMapperRepository.Insert(locationMapper);
                                _unitofwork.Save();
                            }
                        }
                        else
                        {
                            isValidateRow.CreatedUser = kitchenAddToLocation.CreatedUser;
                            isValidateRow.CreatedDate = kitchenAddToLocation.CreatedDate;
                            isValidateRow.ModifiedUser = kitchenAddToLocation.ModifiedUser;
                            isValidateRow.ModifiedDate = kitchenAddToLocation.ModifiedDate;

                            if (r.IsSelectLocation)
                            {
                                isValidateRow.DataTransfer = 0;
                                isValidateRow.IsActive = true;
                                _unitofwork.LocationMapperRepository.Update(isValidateRow);
                                _unitofwork.Save();
                            }
                            else
                            {
                                isValidateRow.DataTransfer = 0;
                                isValidateRow.IsActive = false;
                                _unitofwork.LocationMapperRepository.Update(isValidateRow);
                                _unitofwork.Save();
                            }
                        }


                    }

                    transactionScope.Complete();
                    result = 1;


                }
                catch (Exception)
                {
                    transactionScope.Dispose();
                    throw;
                }
            }

            return result;
        }
    }
}
