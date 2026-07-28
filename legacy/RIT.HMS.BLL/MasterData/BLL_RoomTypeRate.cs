using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
   public class BLL_RoomTypeRate
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_RoomTypeRate()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_RoomTypeRate(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<RstRoomTypeRate> GetRoomTypeRates()
        {
            try
            {
                IEnumerable<RstRoomTypeRate> rstroomtyperate = _unitofwork.RoomTypeRateRepository.Get().OrderBy(rtr => rtr.RoomTypeRateCode);
                if (rstroomtyperate != null)
                {
                    return rstroomtyperate;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<RstRoomTypeRate> GetActiveRoomTypeRates()
        {
            try
            {
                IEnumerable<RstRoomTypeRate> rstroomtyperate = _unitofwork.RoomTypeRateRepository.Get(rtr => rtr.IsDelete == false && rtr.IsActive == true).OrderBy(rtr => rtr.RoomTypeRateCode);
                if (rstroomtyperate != null)
                {
                    return rstroomtyperate;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public RstRoomTypeRate GetRoomTypeRateById(long id)
        {
            try
            {
                RstRoomTypeRate rstroomtyperate = _unitofwork.RoomTypeRateRepository.Get(rtr => rtr.RstRoomTypeRateID == id).FirstOrDefault();
                if (rstroomtyperate != null)
                {
                    return rstroomtyperate;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public RstRoomTypeRate GetRoomTypeRateByCode(string code)
        {
            try
            {
                RstRoomTypeRate roomtyperate = _unitofwork.RoomTypeRateRepository.Get(g => g.RoomTypeRateCode == code).FirstOrDefault();
                if (roomtyperate != null)
                {
                    return roomtyperate;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveRoomTypeRate(RstRoomTypeRate rtr)
        {
            try
            {
                _unitofwork.RoomTypeRateRepository.Insert(rtr);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateRoomTypeRate(RstRoomTypeRate rtr)
        {
            try
            {
                _unitofwork.RoomTypeRateRepository.Update(rtr);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        
    }
}
