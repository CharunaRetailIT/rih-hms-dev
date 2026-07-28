using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service.Transactions
{
    public class DayEndService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public bool DayEnd(DateTime datefrom, DateTime dateto)
        {


            var from = new SqlParameter("@DateFrom", datefrom);
            var to = new SqlParameter("@DateTo", dateto);
         

            var k =    context.Database.ExecuteSqlCommand(" EXEC [dbo].[DayEnd_DateRange] @DateFrom , @DateTo", from, to);
            if (k == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

            // context.Database.ExecuteSqlCommand("",);
        }

    }
}