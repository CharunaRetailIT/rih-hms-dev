using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_RoomType
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_RoomType()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_RoomType(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<RstRoomType> GetRoomTypes()
        {
            try
            {
                IEnumerable<RstRoomType> rstroomtype = _unitofwork.RoomTypeRepository.Get().OrderBy(rt => rt.RoomTypeCode);
                if (rstroomtype != null)
                {
                    return rstroomtype;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<RstRoomType> GetActiveRoomTypes()
        {
            try
            {
                IEnumerable<RstRoomType> rstroomtype = _unitofwork.RoomTypeRepository.Get(rt => rt.IsDelete == false && rt.IsActive == true).OrderBy(rt => rt.RoomTypeCode);
                if (rstroomtype != null)
                {
                    return rstroomtype;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public RstRoomType GetRoomTypeById(long id)
        {
            try
            {
                RstRoomType rstroomtype = _unitofwork.RoomTypeRepository.Get(rt => rt.RstRoomTypeID == id).FirstOrDefault();
                if (rstroomtype != null)
                {
                    return rstroomtype;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public RstRoomType GetRoomTypeByCode(string code)
        {
            try
            {
                RstRoomType roomtype = _unitofwork.RoomTypeRepository.Get(g => g.RoomTypeCode == code).FirstOrDefault();
                if (roomtype != null)
                {
                    return roomtype;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveRoomType(RstRoomType rt)
        {
            try
            {
                _unitofwork.RoomTypeRepository.Insert(rt);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateRoomType(RstRoomType rt)
        {
            try
            {
                _unitofwork.RoomTypeRepository.Update(rt);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }


    }
}
