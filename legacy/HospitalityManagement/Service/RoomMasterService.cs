using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace HospitalityManagement.Service
{
    public class RoomMasterService
    {

        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<RstRoomMaster> GetRooms()
        {
            try
            {
                IEnumerable<RstRoomMaster> rstroommaster = context.RstRoomMaster.OrderBy(rm => rm.RoomMasterCode);
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
                IEnumerable<RstRoomMaster> rstroommaster = context.RstRoomMaster.Where(rm => rm.IsDelete == false && rm.IsActive == true).OrderBy(rm => rm.RoomMasterCode);
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
                RstRoomMaster rstroommaster = context.RstRoomMaster.Where(rm => rm.RstRoomMasterID == id).FirstOrDefault();
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
                context.RstRoomMaster.Add(rm);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateRoom(RstRoomMaster rm)
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


        public RstRoomMaster GetRoomByCode(string code)
        {
            try
            {
                RstRoomMaster room = context.RstRoomMaster.Where(g => g.RoomMasterCode == code).FirstOrDefault();
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