using RIT.HMS.Data;
using RIT.HMS.Domain.MasterEnums;
using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.TransactionData
{
    public class BLL_StockCount
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_StockCount()
        {
            _unitofwork = new UnitOfWork();

        }
        public BLL_StockCount(string connection)
        {
            _unitofwork = new UnitOfWork(connection);

        }

        public JobHeader CurrentJob(int companyid)
        {
            int ongoing = (Int32)enumJobStatus.Ongoing;
            var job = _unitofwork.JobHeaderRepository.Get(j=> j.CompanyID==companyid && j.Status== ongoing).FirstOrDefault();
            return job ?? null;
        }

        public JobHeader CurrentStock(JobHeader jobheader)
        {
            _unitofwork.CreateTransaction();
            try
            {
                
                var stock = (from p in _unitofwork.ProductRepository.Get(p=>p.CompanyID== jobheader.CompanyID && p.IsActive == true && p.IsDelete == false && p.IsRowMaterial == true)
                             join d in _unitofwork.DepartmentRepository.Get(d => d.CompanyID == jobheader.CompanyID) on p.DepartmentId equals d.RstDepartmentID
                             join ps in _unitofwork.ProductStockMasterRepository.Get(ps=>ps.CompanyID== jobheader.CompanyID && ps.LocationId== jobheader.LocationId) on p.ProductId equals ps.ProductId                            
                             select new
                            {
                                ProductId = p.ProductId,
                                ProductName = p.ProductName,
                                ProductCode = p.ProductCode,
                                Stock=ps.Stock,
                                DepartmentId = p.DepartmentId,
                                DepartmentCode=d.DepartmentCode,
                                DeprtmentName=d.DepartmentName
                            }).ToList();
                if (jobheader.DepartmentId != 0)
                {
                    stock = stock.Where(b => b.DepartmentId== jobheader.DepartmentId).ToList();
                }
              
                foreach (var s in stock)
                {
                    JobItem i = new JobItem();
                    i.ProductId = s.ProductId;
                    i.ProductCode = s.ProductCode;
                    i.ProductName = s.ProductName;
                    i.SystemQty = s.Stock;
                    i.DepartmentId = s.DepartmentId;
                    i.LocationId = jobheader.LocationId;
                    i.DepartmentCode = s.DepartmentCode;
                    i.DepartmentName = s.DeprtmentName;
                    jobheader.JobItem.Add(i);

                }

                var existsjob = _unitofwork.JobHeaderRepository.Get(j=>j.JobNumber==jobheader.JobNumber).FirstOrDefault();
                if (existsjob == null)
                {
                    _unitofwork.JobHeaderRepository.Insert(jobheader);
                }
                else
                {
                    jobheader.JobHeaderId = existsjob.JobHeaderId;
                    jobheader.JobDate = existsjob.JobDate;
                    jobheader.StartTime = existsjob.StartTime;
                    jobheader.JobItem.ForEach(i => { i.JobHeaderId = jobheader.JobHeaderId; });
                    _unitofwork.JobHeaderRepository.UpdateBySet(existsjob,jobheader);
                    
                    _unitofwork.JobItemRepository.BulkInsert(jobheader.JobItem);
                }
                _unitofwork.Save();
                _unitofwork.Commit();
                return jobheader ?? null;

            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return null;
            }

           
        }

        public JobHeader UploadFile(JobHeader jobheader)
        {
            _unitofwork.CreateTransaction();
            try
            {

                var existsheader = _unitofwork.JobHeaderRepository.Get(h => h.JobNumber == jobheader.JobNumber).FirstOrDefault();
                jobheader.JobItem.ForEach(ji=>{ ji.JobHeaderId = existsheader.JobHeaderId; });

                List<JobItem> itemlist = new List<JobItem>();
             
                foreach (var i in jobheader.JobItem)
                {
                    var systemi = _unitofwork.JobItemRepository.Get(j => j.JobHeaderId == i.JobHeaderId
                                                                      && j.ProductId == i.ProductId).FirstOrDefault();
                    var product = _unitofwork.ProductRepository.GetAsNoTracking(p=>p.ProductId==i.ProductId 
                                                                        && p.CompanyID==existsheader.CompanyID).FirstOrDefault();
                    var department = _unitofwork.DepartmentRepository.GetAsNoTracking(d=>d.RstDepartmentID== systemi.DepartmentId).FirstOrDefault();
                    systemi.ProductCode = product.ProductCode;
                    systemi.ProductName = product.ProductName;
                    systemi.DepartmentCode = department.DepartmentCode;
                    systemi.DepartmentName = department.DepartmentName;
                    systemi.JobHeaderId = existsheader.JobHeaderId;
                    systemi.PhysicalQty = i.PhysicalQty;
                        
                    itemlist.Add(systemi);
                }
                jobheader.JobItem = itemlist;              
                jobheader = existsheader;
                jobheader.JobItem = itemlist;

               _unitofwork.JobHeaderRepository.UpdateBySet(existsheader,jobheader);              
                foreach (var i in itemlist)
                {
                    var item = _unitofwork.JobItemRepository.GetById(i.JobItemId);
                    
                    item.PhysicalQty = i.PhysicalQty;
                    item.Status = 1;
                    _unitofwork.JobItemRepository.Update(item);
                   
                }

                int saved =  _unitofwork.Save();
                if (saved == itemlist.Count())
                {
                    _unitofwork.Commit();
                    jobheader.Messanger = 4;
                    return jobheader ?? null;
                }
                else
                {
                    _unitofwork.Rollback();
                    JobHeader emptyjobheader = new JobHeader();
                    emptyjobheader.Messanger = 5;
                    jobheader = emptyjobheader;
                    return jobheader;
                }
                     
            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return null;
            }
        }



        public JobHeader ConfirmJob(JobHeader jobheader)
        {
            _unitofwork.CreateTransaction();
            try
            {
                var header = _unitofwork.JobHeaderRepository.Get(h => h.JobNumber == jobheader.JobNumber && h.CompanyID == jobheader.CompanyID).FirstOrDefault();
                var detail = _unitofwork.JobItemRepository.Get(d => d.JobHeaderId == header.JobHeaderId && d.Status==1).ToList();

                StockAdjustmentHeader adjheader = new StockAdjustmentHeader();
                adjheader.DocumentNo = header.JobNumber;
                adjheader.StockLocationId = header.LocationId;
                adjheader.Remark = "Sys Stock Count";
                adjheader.GroupOfCompanyID = header.GroupOfCompanyID;
                adjheader.CompanyID = header.CompanyID;
                adjheader.LocationId = header.LocationId;
                adjheader.CreatedUser = header.CreatedUser;
                adjheader.CreatedDate = header.JobDate;
                adjheader.ModifiedDate = header.ModifiedDate;
                adjheader.DataTransfer = 0;
                adjheader.DocumentId = 0;
                adjheader.NewDocNumber = "";
                adjheader.DocumentStatus = 0;
                _unitofwork.StockAdjustmentHeaderRepository.Insert(adjheader);
                _unitofwork.Save();

                foreach (var d in detail)
                {
                    var stockmaster = _unitofwork.ProductStockMasterRepository.Get(s=>s.CompanyID==jobheader.CompanyID 
                                                                                        && s.LocationId==d.LocationId 
                                                                                        && s.ProductId==d.ProductId
                                                                                        ).FirstOrDefault();

                    decimal actualqty = 0;
                    actualqty = (d.PhysicalQty - d.SystemQty);

                    StockAdjustmentDetail adjdetail = new StockAdjustmentDetail();

                    adjdetail.StockAdjustmentHeaderId = adjheader.StockAdjustmentHeaderId;
                    adjdetail.ProductId = d.ProductId;
                    adjdetail.CurrentStock = stockmaster.Stock;
                    adjdetail.AdjustStock = actualqty;
                    adjdetail.NewStock = stockmaster.Stock + actualqty;
                    if (actualqty < 0)
                    {
                        adjdetail.BaseType = "Reduce";

                    }
                    else
                    {
                        adjdetail.BaseType = "Add";
                    }
                    adjdetail.Reason = "Stock Count";
                    _unitofwork.StockAdjustmentDetailRepository.Insert(adjdetail);


                    stockmaster.Stock = (stockmaster.Stock + actualqty);
                    stockmaster.DocumentNo = jobheader.JobNumber;

                }
                header.Status = 1;
                _unitofwork.Save();
                _unitofwork.Commit();
                jobheader.Messanger = 6;
                return jobheader;
            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                jobheader.Messanger = 7;
                return jobheader;
            }
        }





        public JobHeader ViewJob(JobHeader jobheader)
        {
   
            try
            {

                var stock = (from p in _unitofwork.ProductRepository.Get(p => p.CompanyID == jobheader.CompanyID && p.IsActive == true && p.IsDelete == false && p.IsRowMaterial == true)
                             join d in _unitofwork.DepartmentRepository.Get(d => d.CompanyID == jobheader.CompanyID) on p.DepartmentId equals d.RstDepartmentID
                             join jd in _unitofwork.JobItemRepository.Get() on p.ProductId equals jd.ProductId 
                             join jh in _unitofwork.JobHeaderRepository.Get(j=>j.CompanyID== jobheader.CompanyID && j.JobNumber==jobheader.JobNumber) on jd.JobHeaderId equals jh.JobHeaderId
                             select new
                             {
                                 ProductId = p.ProductId,
                                 ProductName = p.ProductName,
                                 ProductCode = p.ProductCode,
                                 Stock = jd.SystemQty,
                                 PhysicalQty=jd.PhysicalQty,
                                 DepartmentId = p.DepartmentId,
                                 DepartmentCode = d.DepartmentCode,
                                 DeprtmentName = d.DepartmentName,
                                 JobDate=jh.JobDate,
                                 JobHeaderId=jh.JobHeaderId,
                                
                             }).ToList();

                if (stock.Count() != 0 || stock != null)
                {
                    jobheader.JobDate = stock.FirstOrDefault().JobDate;
                    jobheader.JobHeaderId = stock.FirstOrDefault().JobHeaderId;
                    jobheader.ExistsJobId = jobheader.JobHeaderId;
                }

                foreach (var s in stock)
                {
                    JobItem i = new JobItem();
                    i.ProductId = s.ProductId;
                    i.ProductCode = s.ProductCode;
                    i.ProductName = s.ProductName;
                    i.SystemQty = s.Stock;
                    i.PhysicalQty = s.PhysicalQty;
                    i.DepartmentId = s.DepartmentId;
                    i.DepartmentCode = s.DepartmentCode;
                    i.DepartmentName = s.DeprtmentName;
                    jobheader.JobItem.Add(i);

                }

                
                return jobheader ?? null;

            }
            catch (Exception e)
            {

                return jobheader;
            }


        }


    }
}
