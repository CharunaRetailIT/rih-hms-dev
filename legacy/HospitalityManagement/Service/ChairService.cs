using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class ChairService
    {

        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<ChairMaster> GetChairs()
        {
            try
            {
                IEnumerable<ChairMaster> chairmaster = context.ChairMaster.OrderBy(cm => cm.ChairCode);
                if (chairmaster != null)
                {
                    return chairmaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ChairMaster> GetActiveChairs()
        {
            try
            {
                IEnumerable<ChairMaster> chairmaster = context.ChairMaster.Where(cm => cm.IsDelete == false).OrderBy(cm => cm.ChairCode);
                if (chairmaster != null)
                {
                    return chairmaster;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ChairMaster GetChairById(long id)
        {
            try
            {
                ChairMaster chairmaster = context.ChairMaster.Where(cm => cm.ChairMasterID == id).FirstOrDefault();
                if (chairmaster != null)
                {
                    return chairmaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveChair(ChairMaster cm)
        {
            try
            {
                context.ChairMaster.Add(cm);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateChair(ChairMaster cm)
        {
            try
            {
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public ChairMaster GetChairByCode(string code)
        {
            try
            {
                ChairMaster chair = context.ChairMaster.Where(g => g.ChairCode == code).FirstOrDefault();
                if (chair != null)
                {
                    return chair;
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