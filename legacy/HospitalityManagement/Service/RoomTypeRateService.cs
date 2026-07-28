using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class RoomTypeRateService
    {

        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<RstRoomTypeRate> GetRoomTypeRates()
        {
            try
            {
                IEnumerable<RstRoomTypeRate> rstroomtyperate = context.RstRoomTypeRate.OrderBy(rtr => rtr.RoomTypeRateCode);
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
                IEnumerable<RstRoomTypeRate> rstroomtyperate = context.RstRoomTypeRate.Where(rtr => rtr.IsDelete == false && rtr.IsActive == true).OrderBy(rtr => rtr.RoomTypeRateCode);
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
                RstRoomTypeRate rstroomtyperate = context.RstRoomTypeRate.Where(rtr => rtr.RstRoomTypeRateID == id).FirstOrDefault();
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

        public int SaveRoomTypeRate(RstRoomTypeRate rtr)
        {
            try
            {
                context.RstRoomTypeRate.Add(rtr);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateRoomTypeRate(RstRoomTypeRate rtr)
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


        public RstRoomTypeRate GetRoomTypeRateByCode(string code)
        {
            try
            {
                RstRoomTypeRate roomtyperate = context.RstRoomTypeRate.Where(g => g.RoomTypeRateCode == code).FirstOrDefault();
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


    }
}