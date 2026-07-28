using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class RoomTypeService
    {

        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<RstRoomType> GetRoomTypes()
        {
            try
            {
                IEnumerable<RstRoomType> rstroomtype = context.RstRoomType.OrderBy(rt => rt.RoomTypeCode);
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
                IEnumerable<RstRoomType> rstroomtype = context.RstRoomType.Where(rt => rt.IsDelete == false && rt.IsActive == true).OrderBy(rt => rt.RoomTypeCode);
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
                RstRoomType rstroomtype = context.RstRoomType.Where(rt => rt.RstRoomTypeID == id).FirstOrDefault();
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

        public int SaveRoomType(RstRoomType rt)
        {
            try
            {
                context.RstRoomType.Add(rt);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateRoomType(RstRoomType rt)
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

        public RstRoomType GetRoomTypeByCode(string code)
        {
            try
            {
                RstRoomType roomtype = context.RstRoomType.Where(g => g.RoomTypeCode == code).FirstOrDefault();
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


    }
}