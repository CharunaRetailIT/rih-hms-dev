using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Journal;
using RIT.HMS.Domain.Reports;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.Domain.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;


namespace RIT.HMS.BLL.Reports
{
   public class BLL_Reports
    {
        private readonly UnitOfWork _unitofwork;

        public BLL_Reports()
        {
            _unitofwork = new UnitOfWork();
            
        }
        public BLL_Reports(string connection)
        {
            _unitofwork = new UnitOfWork(connection);

        }
        public IEnumerable<ReportCategory> GetRptCategories()
        {
            try
            {
                IEnumerable<ReportCategory> rptcat = _unitofwork.ReportCategoryRepository.Get().OrderBy(r => r.ReportCategoryCode);
                if (rptcat != null)
                {
                    return rptcat;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        
        public List<TransactionDet> GetRecipts(int companyid)
        {
            try
            {
                List<string> recipt = _unitofwork.TransactionDetRepository.Get().OrderBy(r=>r.Receipt).Select(r => r.Receipt).Distinct().ToList();
               
                List<TransactionDet> recipts = new List<TransactionDet>();
                foreach (var a in recipt )
                {
                    TransactionDet t = new TransactionDet();
                    t.Receipt = a;
                    recipts.Add(t);
                }
                if (recipts != null)
                {
                    return recipts;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<TransactionDet> GetUnitNumbers()
        {
            try
            {
                List<int> unitnos = _unitofwork.TransactionDetRepository.Get().OrderBy(r => r.UnitNo).Select(r => r.UnitNo).Distinct().ToList();

                List<TransactionDet> recipts = new List<TransactionDet>();
                foreach (var a in unitnos)
                {
                    TransactionDet t = new TransactionDet();
                    t.UnitNo = a;
                    recipts.Add(t);
                }
                if (recipts != null)
                {
                    return recipts;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<TransactionDet> GetUnitNumbersByLocId(int locid)
        {
            try
            {
                List<int> unitnos = _unitofwork.TransactionDetRepository.Get(r => r.LocationID == locid).OrderBy(r => r.UnitNo).Select(r => r.UnitNo).Distinct().ToList();

                List<TransactionDet> recipts = new List<TransactionDet>();
                foreach (var a in unitnos)
                {
                    TransactionDet t = new TransactionDet();
                    t.UnitNo = a;
                    recipts.Add(t);
                }
                if (recipts != null)
                {
                    return recipts;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<TransactionDet> GetReciptsByLocId(int locid)
        {
            try
            {
                List<string> recipt = _unitofwork.TransactionDetRepository.Get(r=>r.LocationID==locid).OrderBy(r => r.Receipt).Select(r => r.Receipt).Distinct().ToList();

                List<TransactionDet> recipts = new List<TransactionDet>();
                foreach (var a in recipt)
                {
                    TransactionDet t = new TransactionDet();
                    t.Receipt = a;
                    recipts.Add(t);
                }
                if (recipts != null)
                {
                    return recipts;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public JournalViewModel GetReciptsByLocIdUnitNo(int locid,string unitno,DateTime datefrom,DateTime dateto)
        {
            try
            {
                int unit = Convert.ToInt32(unitno);
                DateTime frmdate = datefrom.Date;
                DateTime todate = dateto.Date;
                JournalViewModel vm = new JournalViewModel();
                List<JournalViewModel.Recipts> recipts = new List<JournalViewModel.Recipts>();
                if (unit != 0)
                {
                    //var tds = _unitofwork.TransactionDetRepository.Get(
                    //r => r.LocationID == locid && r.UnitNo == unit
                    //             && r.BillTypeID != 4 && r.TransStatus == 1 &&  r.RecDate >= frmdate && r.RecDate <= todate &&
                    //            (r.DocumentID == 1 || r.DocumentID == 2 || r.DocumentID == 3 || r.DocumentID == 4 
                    //|| r.DocumentID == 6 || r.DocumentID == 8 || r.DocumentID == 9 || r.DocumentID == 10)
                    //

                  // ).Distinct().ToList();
                    var tds = _unitofwork.TransactionDetRepository.Get(

                        r => r.LocationID == locid && r.UnitNo == unit
                                     && r.BillTypeID != 4 && r.TransStatus == 1 && r.RecDate >= frmdate && r.RecDate <= todate &&
                                    (r.DocumentID == 1 || r.DocumentID == 2 || r.DocumentID == 3 || r.DocumentID == 4
                        || r.DocumentID == 6 || r.DocumentID == 8 || r.DocumentID == 9 || r.DocumentID == 10)

                       
                   ).ToList()
                    .GroupBy(r => new { r.Receipt, r.UnitNo, r.ZNo, Date = r.RecDate.Date, r.LocationID }) // Group by selected columns
                    .Select(g => g.First()) // Take one record per group
                    .ToList();



                    foreach (var r in tds)
                    {
                        JournalViewModel.Recipts recipt = new JournalViewModel.Recipts();
                        //var td = _unitofwork.TransactionDetRepository.Get(t => t.Receipt.Trim() == r.Receipt).FirstOrDefault();
                        // recipt.TransactionDetId = r.TransactionDetID;
                        recipt.TransactionDetId = 0;
                        recipt.ReciptNo = r.Receipt.Trim();
                        recipt.UnitNo = r.UnitNo.ToString();
                        recipt.Zno = r.ZNo;
                        recipt.Date = r.RecDate;
                        recipt.LocationId = r.LocationID;
                        recipts.Add(recipt);

                    }
                }
                else
                {
                   // var tds = _unitofwork.TransactionDetRepository.Get(r => r.LocationID == locid
                   //         && r.BillTypeID != 4 && r.TransStatus == 1 && r.RecDate >= frmdate && r.RecDate <= todate &&
                   //         (r.DocumentID == 1 || r.DocumentID == 2 || r.DocumentID == 3 || r.DocumentID == 4 || r.DocumentID == 6 || r.DocumentID == 8 || r.DocumentID == 9 || r.DocumentID == 10)
                   //).Distinct().ToList();


                                        var tds = _unitofwork.TransactionDetRepository.Get(r =>
                        r.LocationID == locid &&
                        r.BillTypeID != 4 &&
                        r.TransStatus == 1 &&
                        r.RecDate >= frmdate &&
                        r.RecDate <= todate &&
                        (r.DocumentID == 1 || r.DocumentID == 2 || r.DocumentID == 3 ||
                         r.DocumentID == 4 || r.DocumentID == 6 || r.DocumentID == 8 ||
                         r.DocumentID == 9 || r.DocumentID == 10)
                    ).ToList()
                     .GroupBy(r => new { r.Receipt, r.UnitNo, r.ZNo, Date = r.RecDate.Date,r.LocationID }) // Group by selected columns
                     .Select(g => g.First()) // Take one record per group
                     .ToList();


                    foreach (var r in tds)
                    {
                        JournalViewModel.Recipts recipt = new JournalViewModel.Recipts();
                        //var td = _unitofwork.TransactionDetRepository.Get(t => t.Receipt.Trim() == r.Receipt).FirstOrDefault();
                        // recipt.TransactionDetId = r.TransactionDetID;
                        recipt.TransactionDetId = 0;
                        recipt.ReciptNo = r.Receipt.Trim();
                        recipt.UnitNo = r.UnitNo.ToString();
                        recipt.Zno = r.ZNo;
                        recipt.Date = r.RecDate;
                        recipt.LocationId = r.LocationID;

                        recipts.Add(recipt);

                    }
                }
             
                
                vm.InvRecipts = recipts.Distinct().ToList();
                return vm;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public JournalViewModel GetJournal(int id, string reciptNo, string date, string unitNo, string zno, int companyid,int locationID)
        {

            //var tranactiondet = _unitofwork.TransactionDetRepository.GetById(id);

            //var journals = (from td in _unitofwork.TransactionDetRepository.Get(
            //                td=> td.Receipt.Trim() == tranactiondet.Receipt.Trim() &&
            //                td.LocationID == tranactiondet.LocationID &&                           
            //                td.BillTypeID != 4 && td.TransStatus == 1 &&
            //                 (
            //                     td.DocumentID == 1 || td.DocumentID == 2 || td.DocumentID == 3 
            //                     || td.DocumentID == 4 || td.DocumentID == 6 || td.DocumentID == 8 
            //                     || td.DocumentID == 9 || td.DocumentID == 10
            //                 )
            //                )
            //                join l in _unitofwork.LocationRepository.Get(l => l.CompanyID == companyid && l.SysLocationID == tranactiondet.LocationID) on td.LocationID equals l.SysLocationID
            //                join c in _unitofwork.CompanyRepository.Get() on l.CompanyID equals c.SysCompanyID
            //                join cm in _unitofwork.CateringMoodRepository.Get() on td.SaleTypeID equals cm.CateringMoodID
            //                join p in _unitofwork.ProductRepository.Get(p=>p.CompanyID == companyid) on td.ProductID equals p.ProductId
            //                where td.UnitNo == Convert.ToInt32(tranactiondet.UnitNo)


            try
            {

                var targetDate = Convert.ToDateTime(date.ToString().Trim());
                var nextDate = targetDate.AddDays(1);

                var journals = (from td in _unitofwork.TransactionDetRepository.Get(
                                td => td.Receipt.Trim() == reciptNo.ToString().Trim() &&
                                 td.Receipt.Trim() == reciptNo.ToString().Trim() &&
                                 td.RecDate >= targetDate && td.RecDate < nextDate && 
                                 td.UnitNo.ToString().Trim() == unitNo.ToString().Trim() &&
                                 td.ZNo.ToString().Trim() == zno.ToString().Trim() &&


                                //   td => td.zno.Trim() == zno.ToString().Trim() &&
                                //  td.LocationID == tranactiondet.LocationID &&
                                td.BillTypeID != 4 && td.TransStatus == 1 &&
                                 (
                                     td.DocumentID == 1 || td.DocumentID == 2 || td.DocumentID == 3
                                     || td.DocumentID == 4 || td.DocumentID == 6 || td.DocumentID == 8
                                     || td.DocumentID == 9 || td.DocumentID == 10
                                 )
                                 )

                                join l in _unitofwork.LocationRepository.Get(l => l.CompanyID == companyid && l.SysLocationID == locationID) on td.LocationID equals l.SysLocationID
                                join c in _unitofwork.CompanyRepository.Get() on l.CompanyID equals c.SysCompanyID
                                join cm in _unitofwork.CateringMoodRepository.Get() on td.SaleTypeID equals cm.CateringMoodID
                                join p in _unitofwork.ProductRepository.Get(p => p.CompanyID == companyid) on td.ProductID equals p.ProductId
                                where td.UnitNo == Convert.ToInt32(unitNo)

                                //  c.SysCompanyID == companyid &&
                                //  p.CompanyID == companyid &&
                                // cm.CompanyId == companyid &&
                                //  l.CompanyID == companyid &&
                                //  td.Receipt.Trim() == tranactiondet.Receipt.Trim() && 
                                //  td.LocationID == tranactiondet.LocationID && 
                                //  l.SysLocationID == tranactiondet.LocationID && 
                                // td.UnitNo == Convert.ToInt32(tranactiondet.UnitNo) && 
                                //      td.BillTypeID != 4 && td.TransStatus == 1 &&
                                //     (td.DocumentID == 1 || td.DocumentID == 2 || td.DocumentID == 3 || td.DocumentID == 4 || td.DocumentID == 6 || td.DocumentID == 8 || td.DocumentID == 9 || td.DocumentID == 10)
                                select new
                                {
                                    CompanyName = c.CompanyName,
                                    CompanyAddress1 = c.Address1,
                                    CompanyAddress2 = c.Address2,
                                    CompanyAddress3 = c.Address3,
                                    DocumentNo = td.DocumentID,
                                    Time = td.EndTime,
                                    Date = td.RecDate,
                                    VatRegNo = c.TaxRegistrationNo1,
                                    RecNo = td.Receipt,
                                    SalesType = cm.CateringMoodName,
                                    NoOfGuest = td.NoOfCustomers,
                                    Cashier = td.Cashier,
                                    Unitno = td.UnitNo,
                                    //   AdvanceNoteNo=td.ad payment det payment type id=55  ref no column location zno 
                                    // SubTotal=td.su
                                    NetTotal = td.Nett,
                                    //  Cash=td.
                                    //  Balance=td.
                                    // Noofitem=td.no
                                    //  NoofPcs=td.no
                                    ProductId = td.ProductID,
                                    ProductCode = td.ProductCode,
                                    ProductName = p.ProductName,
                                    Qty = td.Qty,
                                    Price = td.Price,
                                    Amount = td.Amount,
                                    Zno = td.ZNo,
                                    IDiscount1 = td.IDiscount1,
                                    SDiscount = td.SDiscount

                                }
                               ).ToList();

                JournalViewModel jornal = new JournalViewModel();
                if (journals.Count != 0)
                {
                    jornal.ReciptNo = journals.First().RecNo;
                    jornal.VatRegNo = journals.First().VatRegNo;
                    jornal.CateringMode = journals.First().SalesType;
                    jornal.NoOfGuests = (int)journals.First().NoOfGuest;
                    jornal.Cashier = journals.First().Cashier;
                    jornal.UnitNo = Convert.ToString(journals.First().Unitno);
                    jornal.Date = journals.First().Date;
                    jornal.Time = journals.First().Time.ToLongTimeString();
                    jornal.Zno = journals.First().Zno;
                    jornal.DocumentId = journals.First().DocumentNo;
                    jornal.CompanyName = journals.First().CompanyName;
                    jornal.CompanyAddress1 = journals.First().CompanyAddress1;
                    jornal.CompanyAddress2 = journals.First().CompanyAddress2;
                    jornal.CompanyAddress3 = journals.First().CompanyAddress3;
                }

                List<JournalViewModel.InvPruduct> productslist = new List<JournalViewModel.InvPruduct>();
                foreach (var j in journals)
                {
                    JournalViewModel.InvPruduct product = new JournalViewModel.InvPruduct();
                    product.ProductId = j.ProductId;
                    product.ProductCode = j.ProductCode;
                    product.ProductName = j.ProductName;
                    product.Qty = j.Qty;
                    product.Price = j.Price;
                    product.Amount = j.Amount;
                    if (j.DocumentNo == 1)
                    {
                        product.SubTotal = (j.Price * j.Qty) - j.IDiscount1;
                    }
                    else if (j.DocumentNo == 2)
                    {
                        product.SubTotal = ((j.Price * j.Qty) - j.IDiscount1) * -1;
                    }
                    product.NetTotal = product.SubTotal - j.SDiscount;
                    productslist.Add(product);
                }
                jornal.InvProducts = productslist;
                jornal.SubTotal = jornal.InvProducts.Sum(s => s.SubTotal);
                jornal.NetTotal = jornal.InvProducts.Sum(s => s.NetTotal);
                int uno = Convert.ToInt32(jornal.UnitNo);
                var paymentdet = _unitofwork.PaymentDetRepository.Get(p => p.Receipt.Trim() == jornal.ReciptNo.Trim() && p.ZNo == jornal.Zno &&
                p.LocationID == locationID && p.UnitNo == uno).FirstOrDefault();
                if (paymentdet != null)
                {
                    if (paymentdet.PayTypeID == 55)
                    {
                        jornal.AdvanceNoteNo = paymentdet.RefNo;
                    }


                    if (paymentdet.PayTypeID != 17)
                    {
                        jornal.Balance = paymentdet.Balance;
                    }
                    else
                    {
                        jornal.Balance = 0;
                    }


                }

                return jornal;
            }catch (Exception ex)
            {

                return null;
            }
           
        }

        public Tuple<JournalViewModel,int,string,bool,decimal> get()
        {
            JournalViewModel jvm = new JournalViewModel();
            var t= Tuple.Create(jvm,1,"",false,Convert.ToDecimal(253.33));
            return t;
        }
        public ReportCategory GetRptCategoryById(long id)
        {
            try
            {
                ReportCategory rptcat = _unitofwork.ReportCategoryRepository.Get(r => r.ReportCategoryId == id).FirstOrDefault();
                if (rptcat != null)
                {
                    return rptcat;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ReportInfo> GetRptInfoIdByRptCatId(long catid)
        {
            try
            {
                IEnumerable<ReportInfo> rptinfo = _unitofwork.ReportInfoRepository.Get(r => r.ReportCategoryId == catid).OrderBy(k => k.OrderId);
                if (rptinfo != null)
                {
                    return rptinfo;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ReportInfo> GetSSRSReports(int companyid)
        {
            try
            {
                IEnumerable<ReportInfo> rptinfo = _unitofwork.ReportInfoRepository.Get(s=>s.CompanyID==companyid).OrderBy(k => k.OrderId);
                if (rptinfo != null)
                {
                    return rptinfo;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ReportInfo> GetURLByReportId(long rptid)
        {
            List<ReportInfo> reportdata = new List<ReportInfo>();

            try
            {
                IEnumerable<ReportInfo> docs = _unitofwork.ReportInfoRepository.Get(e => e.ReportInfoId == rptid)
                                                                                        .OrderBy(k => k.ReportURL);


                if (docs != null)
                {
                    return docs;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<ReportInfo> GetReportURL(long rptcatid, long rptid,int companyid)
        {
            try
            {
                List<ReportInfo> rptinfo = new List<ReportInfo>();

                if (rptcatid != 0 && rptid != 0)
                {
                    rptinfo = _unitofwork.ReportInfoRepository.Get(r => r.ReportInfoId == rptid && r.ReportCategoryId == rptcatid && r.CompanyID==companyid).
                                                              OrderBy(c => c.ReportURL).ToList();
                }


                if (rptinfo != null)
                {
                    return rptinfo;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<SalesRegisterViewModel> GetSalesRegistryReport(string exectype,
         DateTime execfromdate, DateTime exectodate, bool execIsAsAtDate, DateTime execfromtime, DateTime exectotime,
         int execlocf, int execloct, int exedept, int execcat, int execsubcat, int execustomer,int execcompanyid)
        {

            var type = new SqlParameter("@Type", exectype);
            var fromDate = new SqlParameter("@FromDate", execfromdate);
            var toDate = new SqlParameter("@ToDate", exectodate);
            var isatdate = new SqlParameter("@IsAsAtDate", execIsAsAtDate);
            var fromTime = new SqlParameter("@FromTime", execfromtime);
            var toTime = new SqlParameter("@ToTime", exectotime);
            var locationF = new SqlParameter("@LocationF", execlocf);
            var locationT = new SqlParameter("@LocationT", execloct);
            var department = new SqlParameter("@Department", exedept);
            var category = new SqlParameter("@Category", execcat);
            var subCategory = new SqlParameter("@SubCategory", execsubcat);
            var customer = new SqlParameter("@Customer", execustomer);
            var company = new SqlParameter("@CompanyId", execcompanyid);


            // by hasanka

            var param = new SqlParameter[] {
                        new SqlParameter() {
                            ParameterName = "@Type",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = exectype
                        },
                        new SqlParameter() {
                            ParameterName = "@FromDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = execfromdate
                        },
                         new SqlParameter() {
                            ParameterName = "@ToDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = exectodate
                        },

                          new SqlParameter() {
                            ParameterName = "@IsAsAtDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = execIsAsAtDate
                        },

                          new SqlParameter() {
                            ParameterName = "@FromTime",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = execfromtime
                        },
                           new SqlParameter() {
                            ParameterName = "@ToTime",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = exectotime
                        },
                           new SqlParameter() {
                            ParameterName = "@LocationF",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = execlocf
                        },
                           new SqlParameter() {
                            ParameterName = "@LocationT",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = execloct
                        },

                            new SqlParameter() {
                            ParameterName = "@Department",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = exedept
                        },
                            new SqlParameter() {
                            ParameterName = "@Category",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = execcat
                        },
                             new SqlParameter() {
                            ParameterName = "@SubCategory",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = execsubcat
                        },
                              new SqlParameter() {
                            ParameterName = "@Customer",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = execustomer
                        },
                               new SqlParameter() {
                            ParameterName = "@CompanyId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = execcompanyid
                        }
            };
            try
            {
                return _unitofwork.SalesRegisterViewModelRepository.ExecuteSPSalesRegister("[dbo].[sp_rpt_SalesRegistry] @Type, @FromDate, @ToDate, @IsAsAtDate, @FromTime,@ToTime,  @LocationF, @LocationT, @Department,@Category, @SubCategory, @Customer,@CompanyId", type, fromDate, toDate, isatdate, fromTime, toTime, locationF, locationT, department, category, subCategory, customer, company).ToList();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        //public List<AccountDataTransfer.ImportJournalDetailsHMS> GetAuditTrailDataReport(DateTime Todate, DateTime Fromdate)
        //{

        //    //var locationid = new SqlParameter("@LocationId", location);
        //    var reportFromdate = new SqlParameter("@FromDate", Fromdate);
        //    var reportTodate = new SqlParameter("@ToDate", Todate);
        //    var ddd = Fromdate.ToShortDateString();
        //    var ddd2 = Todate.ToShortDateString();
        //    // by hasanka

        //    var param = new SqlParameter[]
        //                {
        //                    new SqlParameter() {
        //                        ParameterName = "@FromDate",
        //                        SqlDbType =  System.Data.SqlDbType.VarChar,
        //                        Direction = System.Data.ParameterDirection.Input,
        //                        Value = ddd
        //                    },
        //                     new SqlParameter() {
        //                        ParameterName = "@ToDate",
        //                        SqlDbType =  System.Data.SqlDbType.VarChar,
        //                        Direction = System.Data.ParameterDirection.Input,
        //                        Value = ddd2
        //                    }
                            
        //                };


        //    return _unitofwork.ImportJurnalDetReportRepository.ExecuteSPImportJurnalDet("[dbo].[spImportJournalDetails]  @FromDate,@ToDate", reportFromdate, reportTodate).ToList();

        //}

        public bool GetAuditTrailDataReport(DateTime Fromdate, DateTime  Todate)
        {

            //var locationid = new SqlParameter("@LocationId", location);
            var reportFromdate = new SqlParameter("@FromDate", Fromdate);
            var reportTodate = new SqlParameter("@ToDate", Todate);
            var ddd = Fromdate.ToShortDateString();
            var ddd2 = Todate.ToShortDateString();
            // by hasanka

            var param = new SqlParameter[]
                        {
                            new SqlParameter() {
                                ParameterName = "@FromDate",
                                SqlDbType =  System.Data.SqlDbType.VarChar,
                                Direction = System.Data.ParameterDirection.Input,
                                Value = ddd
                            },
                             new SqlParameter() {
                                ParameterName = "@ToDate",
                                SqlDbType =  System.Data.SqlDbType.VarChar,
                                Direction = System.Data.ParameterDirection.Input,
                                Value = ddd2
                            }

                        };



            if(_unitofwork.ImportJurnalDetReportRepository.ExecuteSPImportJurnalDet("[dbo].[spImportJournalDetails]  @FromDate,@ToDate", reportFromdate, reportTodate).ToList()!=null)
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        public bool GLTransferingHMStoAccount(DateTime Todate, DateTime Fromdate)
        {

            //var locationid = new SqlParameter("@LocationId", location);
            var reportFromdate = new SqlParameter("@FromDate", Fromdate);
            var reportTodate = new SqlParameter("@ToDate", Todate);
            //var reportLocation = new SqlParameter("@LocationID", 1);
            var reportGroupComp = new SqlParameter("@GroupOfCompany", 1);
            var ddd = Fromdate.ToShortDateString();
            var ddd2 = Todate.ToShortDateString();
            // by hasanka

            var param = new SqlParameter[]
                        {
                            new SqlParameter() {
                                ParameterName = "@FromDate",
                                SqlDbType =  System.Data.SqlDbType.VarChar,
                                Direction = System.Data.ParameterDirection.Input,
                                Value = ddd
                            },
                             new SqlParameter() {
                                ParameterName = "@ToDate",
                                SqlDbType =  System.Data.SqlDbType.VarChar,
                                Direction = System.Data.ParameterDirection.Input,
                                Value = ddd2
                            }

                        };


            if (_unitofwork.ImportJurnalDetReportRepository.ExecuteSPTranferToGL("[dbo].[SpTransferToGL]  @FromDate,@ToDate,@GroupOfCompany", reportFromdate, reportTodate, reportGroupComp).ToList() != null)
            {
                return true;
            }
            else
            {
                return false;
            }


        }
        public List<ImportJournalDetails> GetImportJournalDetails(DateTime datefrom, DateTime dateto)
        {
            try
            {
                List<ImportJournalDetails> ImportJournalDet = new List<ImportJournalDetails>();
                DateTime FromDate = datefrom.Date;
                DateTime ToDate = dateto.Date;

                ImportJournalDet = _unitofwork.ImportJournalDetails.Get(r => r.DATE == FromDate && r.GLPOST==false).
                                         OrderBy(c => c.DOCNO).OrderBy(d => d.DATE).ToList();



                if (ImportJournalDet != null)
                {
                    return ImportJournalDet;
                }

                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<ImportJournalDetails> GetImportJournalDetailReport(DateTime datefrom, DateTime dateto)
        {
            try
            {
                List<ImportJournalDetails> ImportJournalDet= new List<ImportJournalDetails>();
                DateTime FromDate = datefrom.Date;
                DateTime ToDate = dateto.Date;

                ImportJournalDet = _unitofwork.ImportJournalDetails.Get(r => r.DATE == FromDate).
                                         OrderBy(c => c.DOCNO).OrderBy(d => d.DATE).ToList();
               
              

                if (ImportJournalDet != null)
                {
                    return ImportJournalDet;
                }

                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<DailySalesViewMdel.SalesData> GetDailySalesReport(DateTime date, int location)
        {

            var locationid = new SqlParameter("@LocationId", location);
            var reportdate = new SqlParameter("@Date", date);
            var ddd = date.ToShortDateString();
            // by hasanka

            var param = new SqlParameter[]
                        {
                            new SqlParameter() {
                                ParameterName = "@Date",
                                SqlDbType =  System.Data.SqlDbType.VarChar,
                                Direction = System.Data.ParameterDirection.Input,
                                Value = ddd
                            },
                            new SqlParameter() {
                                ParameterName = "@LocationId",
                                SqlDbType =  System.Data.SqlDbType.Int,
                                Direction = System.Data.ParameterDirection.Input,
                                Value = location
                            }
                        };





            // return Context.Database.SqlQuery<DailySalesViewMdel.SalesData>("[dbo].[SP_DailySales] @Date, @LocationId", reportdate, locationid).ToList();

            return _unitofwork.SalesDataReportRepository.ExecuteSPDailySales("[dbo].[SP_DailySales]  @Date, @LocationId", reportdate, locationid).ToList();

        }

        public List<DailySalesViewMdel.SalesData> GetGivenSalesReport(DailySalesViewMdel parms)
        {
            string locations = string.Empty;

            if (parms.Locations != null)
            {
                foreach (var i in parms.Locations)
                {
                    locations += i.ToString() + ",";
                }
            }
            else
            {
                locations = "0";
            }


            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<DailySalesViewMdel.SalesData>("[dbo].[SP_GivenDateSales]  @Date,@DateTo ,@Locations",
                    new SqlParameter("@Date", SqlDbType.DateTime) { Value = parms.Date.ToShortDateString() },
                    new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = parms.DateTo.ToShortDateString() },
                    new SqlParameter("@Locations", SqlDbType.NVarChar) { Value = Convert.ToString(locations) }
                  
                    ).ToList();
            return result;





          //  return _unitofwork.SalesDataReportRepository.ExecuteSPDailySales("[dbo].[SP_GivenDateSales]  @Date,@DateTo ,@Locations", reportdate, locationid).ToList();

        }

        public List<FoodCostingViewModel.FoodCostingDetail> GetFoodCostingReport(DashboardViewModel.DashboardParms parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<FoodCostingViewModel.FoodCostingDetail>("[dbo].[SP_RP_FoodCostEstimate] @FromDate,@ToDate,@CompanyID,@LocationID,@DeptID",
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                    new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = parms.CompanyId },
                    new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                    new SqlParameter("@DeptID", SqlDbType.Int) { Value = Convert.ToInt32(parms.DepartmentId) }
                    ).ToList();
            return result;
        }

        public List<FoodCostingViewModel.FoodCostingDetail> GetConsumptionReport(DashboardViewModel.DashboardParms parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<FoodCostingViewModel.FoodCostingDetail>("[dbo].[SP_RP_MaterialConsumption] @FromDate,@ToDate,@CompanyID,@LocationID,@DeptID",
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.FromDate.ToShortDateString() },
                    new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.ToDate.ToShortDateString() },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = parms.CompanyId },
                    new SqlParameter("@LocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                    new SqlParameter("@DeptID", SqlDbType.Int) { Value = Convert.ToInt32(parms.DepartmentId) }
                    ).ToList();
            return result;
        }
        public List<StockBinCardViewModel.Detail> GetBinCardReport(StockBinCardViewModel parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<StockBinCardViewModel.Detail>("[dbo].[sp_rpt_BinCard] @SelectedLocationID,@FromDate,@ToDate,@ProductID,@DepartmentID",
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = parms.DateFrom.ToShortDateString() },
                    new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = parms.DateTo.ToShortDateString() },
                    new SqlParameter("@SelectedLocationID", SqlDbType.Int) { Value = Convert.ToInt32(parms.LocationId) },
                    new SqlParameter("@DepartmentID", SqlDbType.Int) { Value = Convert.ToInt32(parms.DepartmentId) },
                    new SqlParameter("@ProductID", SqlDbType.Int) { Value = Convert.ToInt32(parms.ProductId) }
                    ).ToList();
            return result;
        }

        public List<StockBinCardViewModel.Detail> GetAsAtStockBalanceReport(StockBinCardViewModel parms)
        {
            var result = _unitofwork.RevenueAndCostRepository.SQLQuery<StockBinCardViewModel.Detail>("[dbo].[SP_AsAtStockBal] @CompanyId, @SelectedLocationID,@ToDate,@ProductID,@DepartmentID,@WithZeroBal",
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = parms.CompanyId },
                    new SqlParameter("@SelectedLocationID", SqlDbType.Int) { Value = parms.LocationId },
                    new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = Convert.ToDateTime(parms.DateTo) },
                    new SqlParameter("@DepartmentID", SqlDbType.VarChar) { Value = Convert.ToString(parms.DepartmentId) },
                    new SqlParameter("@ProductID", SqlDbType.VarChar) { Value = Convert.ToString(parms.ProductId) },
                    new SqlParameter("@WithZeroBal", SqlDbType.Bit) { Value = Convert.ToBoolean(parms.WithZeroBalances) }

                    ).ToList();
            return result;
        }
    }
}
