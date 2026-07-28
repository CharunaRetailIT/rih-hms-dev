using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RIT.HMS.Domain.ViewModels.Reports;

namespace RIT.HMS.BLL.TransactionData
{
    public class BLL_MonthEnd
    {
        private readonly UnitOfWork _unitofwork;

        public BLL_MonthEnd()
        {
            _unitofwork = new UnitOfWork();
        }

        public BLL_MonthEnd(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public MonthEnd GetMonthEndID(Int32 _year, Int32 _month, Int32 _locID)
        {
            try
            {
                MonthEnd mnthend = _unitofwork.MonthEndRepository.Get(p => p.LocYear == _year && p.LocMonth == _month && p.LocationId == _locID).FirstOrDefault();
                if (mnthend != null)
                {
                    return mnthend;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public bool IsOpenMonthExist(Int32 _locID)
        {
            try
            {
                MonthEnd mnthend = _unitofwork.MonthEndRepository.Get(p => p.LocationId == _locID && p.LocStatus == true).FirstOrDefault();
                if (mnthend != null)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public string GetMonthDesc(int _monthID)
        {
            string _month = "";

            switch (_monthID)
            {
                case 1:
                    _month = "January";
                    break;
                case 2:
                    _month = "February";
                    break;
                case 3:
                    _month = "March";
                    break;
                case 4:
                    _month = "April";
                    break;
                case 5:
                    _month = "May";
                    break;
                case 6:
                    _month = "June";
                    break;
                case 7:
                    _month = "July";
                    break;
                case 8:
                    _month = "August";
                    break;
                case 9:
                    _month = "September";
                    break;
                case 10:
                    _month = "October";
                    break;
                case 11:
                    _month = "November";
                    break;
                case 12:
                    _month = "December";
                    break;
                default:
                    _month = "";
                    break;
            }
            return _month;
        }

        public IEnumerable<MonthEndViewModel> GetMonthEndDataByYear(int _year,Int32 _compid)
        {

            var products = (from me in _unitofwork.MonthEndRepository.Get()
                            join loc in _unitofwork.LocationRepository.Get() on me.LocationId equals loc.SysLocationID
                            where (me.LocYear == _year && me.CompanyId==_compid)
                            orderby me.LocMonth, me.LocationId
                            select new
                            {
                                me.LocMonthDesc,
                                loc.LocationName,
                                me.LocStatus,
                                me.MonthEndId
                            }
                            ).ToList().Distinct();

            List<MonthEndViewModel> monthends = new List<MonthEndViewModel>();
            foreach (var prd in products)
            {
                MonthEndViewModel monnend = new MonthEndViewModel();
                monnend.LocMonthDesc = prd.LocMonthDesc;
                monnend.LocDesc = prd.LocationName;
                monnend.LocStatusDesc = prd.LocStatus == true ? "OPEN" : "CLOSE";
                //  monnend.MonthEndId = prd.MonthEndId;
                monthends.Add(monnend);
            }
            return monthends.AsEnumerable();
            //   return monthends == null ? null : monthends;
        }

        public IEnumerable<MonthEndViewModel> GetMonthEndDataByYearLoc(int _year, int _loc)
        {

            var products = (from me in _unitofwork.MonthEndRepository.Get()
                            join loc in _unitofwork.LocationRepository.Get() on me.LocationId equals loc.SysLocationID
                            where (me.LocYear == _year && me.LocationId == _loc)
                            orderby me.LocMonth, me.LocationId
                            select new
                            {
                                me.LocMonthDesc,
                                loc.LocationName,
                                me.LocStatus,
                                me.MonthEndId
                            }
                            ).ToList().Distinct();

            List<MonthEndViewModel> monthends = new List<MonthEndViewModel>();
            foreach (var prd in products)
            {
                MonthEndViewModel monnend = new MonthEndViewModel();
                monnend.LocMonthDesc = prd.LocMonthDesc;
                monnend.LocDesc = prd.LocationName;
                monnend.LocStatusDesc = prd.LocStatus == true ? "OPEN" : "CLOSE";
                //  monnend.MonthEndId = prd.MonthEndId;
                monthends.Add(monnend);
            }
            return monthends.AsEnumerable();
            //   return monthends == null ? null : monthends;
        }


        public int UpdateOpenMonth(MonthEnd mnthend)
        {
            try
            {
                var dbmonthend = _unitofwork.MonthEndRepository.GetById(mnthend.MonthEndId);
                dbmonthend.LocStatus = mnthend.LocStatus;
                dbmonthend.ModifiedDate = DateTime.Now;
                dbmonthend.ModifiedUser = mnthend.ModifiedUser;
                dbmonthend.LocIsClose = mnthend.LocIsClose;

                _unitofwork.MonthEndRepository.Update(dbmonthend);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        public int SaveLocationMonths(int _year, int locID, string _loguser)
        {
            try
            {
                int x = 0;

                for (int i = 1; i < 13; i++)
                {

                    MonthEnd mnthend = new MonthEnd();
                    mnthend.LocStatus = false;
                    mnthend.LocIsClose = false;
                    mnthend.ModifiedDate = DateTime.Now;
                    mnthend.ModifiedUser = _loguser;
                    mnthend.CreatedDate = DateTime.Now;
                    mnthend.CreatedUser = _loguser;
                    mnthend.DataTransfer = 0;
                    mnthend.LocYear = _year;
                    mnthend.LocMonth = i;
                    mnthend.LocationId = locID;
                    mnthend.LocMonthDesc = GetMonthDesc(mnthend.LocMonth);

                    _unitofwork.MonthEndRepository.Insert(mnthend);
                    x = _unitofwork.Save();

                }

                return x;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public List<DailySalesViewMdel.ValidMonthEndData> IsValidCloseMonth(int _locID, int _year, int _month)
        {
            try
            {
                var locationid = new SqlParameter("@LocationID", _locID);
                var locationyear = new SqlParameter("@Year", _year);
                var locationmonth = new SqlParameter("@Month", _month);

                var param = new SqlParameter[]
                            {
                                new SqlParameter() {
                                    ParameterName = "@LocationID",
                                    SqlDbType =  System.Data.SqlDbType.Int,
                                    Direction = System.Data.ParameterDirection.Input,
                                    Value = _locID
                                },
                                new SqlParameter() {
                                    ParameterName = "@Year",
                                    SqlDbType =  System.Data.SqlDbType.Int,
                                    Direction = System.Data.ParameterDirection.Input,
                                    Value = _year
                                },
                                new SqlParameter() {
                                    ParameterName = "@Month",
                                    SqlDbType =  System.Data.SqlDbType.Int,
                                    Direction = System.Data.ParameterDirection.Input,
                                    Value = _month
                                }
                            };

                return _unitofwork.ValidMonthEndDataReportRepository.ExecuteSPIsValidMonthEnd("[dbo].[SPIsValidMonthEnd]  @LocationID, @Year,@Month", locationid, locationyear, locationmonth).ToList();

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public int CheckMonthEnd(int _locId, int _year, int _month)
        {

            var monthendstatus = _unitofwork.MonthEndRepository.Get(m => m.LocationId == _locId & m.LocMonth == _month && m.LocYear == _year);
            if (monthendstatus != null && monthendstatus.Count() !=0)
            {
                    if (monthendstatus.FirstOrDefault().LocStatus == false)
                    {
                        return 2; // close
                    }
                        else
                    {
                        return 1; // open

                    }
            }
            else
            {
                    return 3; // not open yet
            }
                       
        }
    }
}
