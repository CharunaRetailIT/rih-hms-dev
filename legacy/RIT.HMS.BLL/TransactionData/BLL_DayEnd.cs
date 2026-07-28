using RIT.HMS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.TransactionData
{
    public class BLL_DayEnd
    {
        private readonly UnitOfWork _unitofwork;

        public BLL_DayEnd()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_DayEnd(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public bool DayEnd(DateTime datefrom, DateTime dateto)
        {


            //var from = new SqlParameter("@DateFrom", datefrom);
            //var to = new SqlParameter("@DateTo", dateto);


            //var k = context.Database.ExecuteSqlCommand(" EXEC [dbo].[DayEnd_DateRange] @DateFrom , @DateTo", from, to);
            //if (k == 0)
            //{
            //    return false;
            //}
            //else
            //{
            //    return true;
            //}

            return false;
            // context.Database.ExecuteSqlCommand("",);
        }
    }
}
