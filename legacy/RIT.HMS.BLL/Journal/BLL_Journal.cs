using RIT.HMS.Data;
using RIT.HMS.Domain.ViewModels.Journal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.Journal
{
    public class BLL_Journal
    {
        UnitOfWork _unitofwork;
        public BLL_Journal()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Journal(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }

        public List<JournalViewModel> UploadJournalData(JournalViewModel parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<JournalViewModel>("[dbo].[spImportJournalDetails] @FromDate,@ToDate,@Location,@CompanyID",
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.DateFrom },
                    new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.DateFrom },
                    new SqlParameter("@Location", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(parms.CompanyId) }
                ).ToList();
            return result;
        }

        public List<JournalViewModel> TransferJournalData(JournalViewModel parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<JournalViewModel>("[dbo].[SpTransferToGL] @LocationID,@DateFrom,@DateTo,@CompanyId",
                    new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = parms.DateFrom },
                    new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = parms.DateFrom },
                    new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = Convert.ToInt32(parms.CompanyId) }
                ).ToList();
            return result;
        }

        public List<JournalReport> JournalRecipt(JournalReport jr)
        {

            var dbjournal = (from j in _unitofwork.ImportJournalDetails.GetAsNoTracking(j => jr.LocationCodes.Contains(j.CCODE) &&
                                         DbFunctions.TruncateTime(j.DATE) == DbFunctions.TruncateTime(jr.DATE))
                             join l in _unitofwork.LocationRepository.GetAsNoTracking(l => l.CompanyID == jr.CompanyId) on j.CCODE equals l.LocationCode
                             select new JournalReport
                             {
                                 ACODE=j.ACODE,
                                 DESCRIPTION=j.DESCRIPTION,
                                 DRCR=j.DRCR,
                                 AMOUNT=j.AMOUNT,
                                 LocationName=l.LocationName
                             }
                           ).ToList();
           
            return dbjournal;
        }
    }
}
