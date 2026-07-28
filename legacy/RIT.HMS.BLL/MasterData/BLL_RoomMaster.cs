using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_RoomMaster
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_RoomMaster()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_RoomMaster(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<RstRoomMaster> GetRooms()
        {
            try
            {
                IEnumerable<RstRoomMaster> rstroommaster = _unitofwork.RoomMasterRepository.Get().OrderBy(rm => rm.RoomMasterCode);
                if (rstroommaster != null)
                {
                    return rstroommaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<RstRoomMaster> GetActiveRooms()
        {
            try
            {
                IEnumerable<RstRoomMaster> rstroommaster = _unitofwork.RoomMasterRepository.Get(rm => rm.IsDelete == false && rm.IsActive == true).OrderBy(rm => rm.RoomMasterCode);
                if (rstroommaster != null)
                {
                    return rstroommaster;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public RstRoomMaster GetRoomById(long id)
        {
            try
            {
                RstRoomMaster rstroommaster = _unitofwork.RoomMasterRepository.Get(rm => rm.RstRoomMasterID == id).FirstOrDefault();
                if (rstroommaster != null)
                {
                    return rstroommaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveRoom(RstRoomMaster rm)
        {
            try
            {
                _unitofwork.RoomMasterRepository.Insert(rm);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateRoom(RstRoomMaster rm)
        {
            try
            {
                _unitofwork.RoomMasterRepository.Update(rm);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public RstRoomMaster GetRoomByCode(string code)
        {
            try
            {
                RstRoomMaster room = _unitofwork.RoomMasterRepository.Get(g => g.RoomMasterCode == code).FirstOrDefault();
                if (room != null)
                {
                    return room;
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
