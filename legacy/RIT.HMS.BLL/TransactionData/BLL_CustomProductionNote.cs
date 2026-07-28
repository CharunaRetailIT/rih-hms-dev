using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.TransactionData
{
    public class BLL_CustomProductionNote
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_CustomProductionNote()
        {
            _unitofwork = new UnitOfWork();

        }
        public BLL_CustomProductionNote(string connection)
        {
            _unitofwork = new UnitOfWork(connection);

        }
        public List<InvAdvanceNoteHed> GetActiveRecipts(int companyid)
        {
            var recipts = _unitofwork.InvAdvanceNoteHedRepository.Get(i => i.IsProduction == false && i.PickupLoc!=0 && i.CompanyId== companyid).ToList();           
            return recipts == null ? null : recipts;
        }

        public InvAdvanceNoteHed GetReciptById(int reciptid,int companyid)
        {
            var recipts = (from I in _unitofwork.InvAdvanceNoteHedRepository.Get(a => a.IsProduction == false)
                           join L1 in _unitofwork.LocationRepository.Get() on I.PickupLoc equals L1.SysLocationID
                           join L2 in _unitofwork.LocationRepository.Get() on I.ProcessLoc equals L2.SysLocationID
                           where I.InvAdvanceNoteHedID == reciptid && I.CompanyId== companyid && L1.CompanyID== companyid && L2.CompanyID==companyid
                           select new
                           {
                               Receipt = I.Receipt,
                               ProcessLocName = L2.LocationName,
                               PickupLocName = L1.LocationName,
                               ProcessLoc = I.ProcessLoc,
                               PickupLoc=I.PickupLoc,
                               Remark=I.Remark,
                              

                           }
                           ).FirstOrDefault();

            InvAdvanceNoteHed i = new InvAdvanceNoteHed();           
            i.Receipt = recipts.Receipt;
            i.PickupLocName = recipts.PickupLocName;
            i.ProcessLocName = recipts.ProcessLocName;
            i.ProcessLoc = recipts.ProcessLoc;
            i.PickupLoc = recipts.PickupLoc;
            i.Remark = recipts.Remark;
                
            return i == null ? null : i;
        }

        public List<InvAdvanceNoteDet> GetAdvanceNoteDetByRecId(int reciptid,int companyid)
        {
            var det = (from H in _unitofwork.InvAdvanceNoteHedRepository.Get(a => a.IsProduction == false && a.InvAdvanceNoteHedID==reciptid)
                        join D in _unitofwork.InvAdvanceNoteDetRepository.Get() on H.AdNoteNo.Trim() equals D.CreditNoteNo.Remove(D.CreditNoteNo.Length-1,1).Trim()
                        join P in _unitofwork.ProductRepository.Get(p=>p.AutoProduction==false) on D.ProductID equals P.ProductId
                       where H.CompanyId==companyid && P.CompanyID==companyid                
                        select new
                        {
                            Receipt = H.Receipt,
                            ProductID=P.ProductId,
                            ProductCode=P.ProductCode,
                            Descrip=P.ProductName
                               

                        }
                       ).ToList();

            List<InvAdvanceNoteDet> dlist = new List<InvAdvanceNoteDet>();
            foreach (var d in det)
            {
                InvAdvanceNoteDet d1 = new InvAdvanceNoteDet();
                d1.Receipt = d.Receipt;
                d1.ProductID = d.ProductID;
                d1.ProductCode = d.ProductCode;
                d1.Descrip = d.Descrip;
                dlist.Add(d1);
            }
            
            return dlist == null ? null : dlist;
        }

        public ProductStockMaster GetMaterialsByLocId(int locid,int materialid,int companyid)
        {
            //  var mat = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == locid && p.ProductId == materialid).FirstOrDefault();
            try {
                var i = (from pm in _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == locid && p.ProductId == materialid)
                         join p in _unitofwork.ProductRepository.Get() on pm.ProductId equals p.ProductId
                         join uc in _unitofwork.UnitConversionRepository.Get() on p.WeightPerUnit equals uc.UnitConversionId
                         where pm.CompanyID==companyid && p.CompanyID==companyid && uc.CompanyID==companyid
                         select new
                         {
                             ProductId = pm.ProductId,
                             ProductName = p.ProductName,
                             Stock = pm.Stock,
                             AvgCost = pm.AvgCost,
                             SubUnitValue = uc.SubUnitValue
                         }
                       ).FirstOrDefault();

                ProductStockMaster mat = new ProductStockMaster();
                mat.ProductId = i.ProductId;
                mat.ProductName = i.ProductName;
                mat.SubUnitValue = i.SubUnitValue;
                mat.Stock = i.Stock;
                mat.AvgCost = i.AvgCost;

                return mat == null ? null : mat;
            }
            catch (NullReferenceException e)
            {
                ProductStockMaster mat = new ProductStockMaster();
                mat.ProductId = 0;
                mat.ProductName = "";
                mat.SubUnitValue = 0;
                mat.Stock = 0;
                mat.AvgCost = 0;
                return mat == null ? null : mat;
            }
        }

        public InvAdvanceNoteDet GetMaterialsByAdvNote(int productid,string advnote)
        {
            var prd = _unitofwork.InvAdvanceNoteDetRepository.Get(p => p.ProductID == productid && p.CreditNoteNo==advnote).First();
            return prd == null ? null : prd;
        }

        public int Save(CustomProductionNoteHeader c)
        {
            _unitofwork.CreateTransaction();
            try
            {
                _unitofwork.CustomProductionNoteHeaderRepository.Insert(c);
                _unitofwork.Save();

              
                var adno = _unitofwork.InvAdvanceNoteHedRepository.GetById(c.ReciptId).AdNoteNo.Trim()+"A";
                // concat A because everytime InvAdvanceNoteDet CreditNoteNo has A at finally - confirmed by Kapila :D

                foreach (var i in c.CustomProductionNoteDetail.Select(p=> new {p.ProductId}).Distinct())
                {
                    var advdet = _unitofwork.InvAdvanceNoteDetRepository.Get(d => d.CreditNoteNo == adno && 
                                                                                  d.ProductID == i.ProductId).FirstOrDefault();
                    advdet.IsProduction = true;
                    _unitofwork.InvAdvanceNoteDetRepository.Update(advdet);
                    _unitofwork.Save();

                }

                foreach (var i in c.CustomProductionNoteDetail)
                {

                    var mstock = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == c.ProcessLocId && p.ProductId == i.MaterialId && p.CompanyID==c.CompanyID).FirstOrDefault();
                    mstock.Stock -= i.MaterialQty;
                    _unitofwork.ProductStockMasterRepository.Update(mstock);
                    _unitofwork.Save();

                    //var pstock = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == c.PickupLocId && p.ProductId == i.ProductId).FirstOrDefault();
                    //pstock.Stock += i.ProductQty;
                    //_unitofwork.ProductStockMasterRepository.Update(pstock);
                    //_unitofwork.Save();
                }

                foreach (var i in c.CustomProductionNoteDetail.Select(p=> new {p.ProductId,p.ProductQty}).Distinct())
                {

                    //var mstock = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == c.ProcessLocId && p.ProductId == i.MaterialId).FirstOrDefault();
                    //mstock.Stock -= i.MaterialQty;
                    //_unitofwork.ProductStockMasterRepository.Update(mstock);
                    //_unitofwork.Save();

                    var pstock = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == c.PickupLocId && p.ProductId == i.ProductId && p.CompanyID==c.CompanyID).FirstOrDefault();
                    pstock.Stock += i.ProductQty;
                    _unitofwork.ProductStockMasterRepository.Update(pstock);
                    _unitofwork.Save();
                }


                var advancedetcount = _unitofwork.InvAdvanceNoteDetRepository.Get(a => a.CreditNoteNo == adno).ToList().Count();
                var advancedetproduction = _unitofwork.InvAdvanceNoteDetRepository.Get(a => a.CreditNoteNo == adno && a.IsProduction==true).ToList().Count();
                if (advancedetcount == advancedetproduction)
                {
                    var advhed = _unitofwork.InvAdvanceNoteHedRepository.Get(a => a.InvAdvanceNoteHedID == c.ReciptId &&
                                                                                  a.CompanyId==c.CompanyID
                                                                                    //&& 
                                                                                    //a.ProcessLoc == c.ProcessLocId && 
                                                                                    //a.PickupLoc==c.PickupLocId
                                                                                    ).FirstOrDefault();
                    advhed.IsProduction = true;
                    _unitofwork.InvAdvanceNoteHedRepository.Update(advhed);
                    _unitofwork.Save();
                }

                _unitofwork.Commit();
                return 1;
            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return 0;
            }

           
        }

        public List<CustomProductionNoteHeader> CustomProductionReport(CustomProductionNoteHeader header)
        {

            List<CustomProductionNoteHeader> cpheader = new List<CustomProductionNoteHeader>();

            if (header.ProcessLocId != 0 && header.PickupLocId != 0)
            {
                cpheader = _unitofwork.CustomProductionNoteHeaderRepository.Get(h => h.ProcessLocId == header.ProcessLocId &&
                                                                                h.PickupLocId == header.PickupLocId &&
                                                                                DbFunctions.TruncateTime(h.CreatedDate) >= header.DateFrom.Date &&
                                                                                DbFunctions.TruncateTime(h.CreatedDate) <= header.DateTo.Date &&
                                                                                h.CompanyID== header.CompanyID
                                                                                ).ToList();
            } else if (header.ProcessLocId == 0 && header.PickupLocId != 0)
            {
                cpheader = _unitofwork.CustomProductionNoteHeaderRepository.Get(h =>
                                                                                h.PickupLocId == header.PickupLocId &&
                                                                                DbFunctions.TruncateTime(h.CreatedDate) >= header.DateFrom.Date &&
                                                                                DbFunctions.TruncateTime(h.CreatedDate) <= header.DateTo.Date &&
                                                                                h.CompanyID == header.CompanyID
                                                                                ).ToList();
            } else if (header.ProcessLocId != 0 && header.PickupLocId == 0)
            {
                cpheader = _unitofwork.CustomProductionNoteHeaderRepository.Get(h => h.ProcessLocId == header.ProcessLocId &&                                                                            
                                                                                DbFunctions.TruncateTime(h.CreatedDate) >= header.DateFrom.Date &&
                                                                                DbFunctions.TruncateTime(h.CreatedDate) <= header.DateTo.Date &&
                                                                                h.CompanyID == header.CompanyID
                                                                                ).ToList();
            }
            else if (header.ProcessLocId == 0 && header.PickupLocId == 0)
            {
                cpheader = _unitofwork.CustomProductionNoteHeaderRepository.Get(h => 
                                                                                DbFunctions.TruncateTime(h.CreatedDate) >= header.DateFrom.Date &&
                                                                                DbFunctions.TruncateTime(h.CreatedDate) <= header.DateTo.Date &&
                                                                                h.CompanyID == header.CompanyID
                                                                                ).ToList();
            }

            if (cpheader != null)
            {
                foreach (var h in cpheader)
                {
                    foreach (var d in h.CustomProductionNoteDetail)
                    {
                        d.ProductCode = _unitofwork.ProductRepository.GetById(d.ProductId).ProductCode;
                        d.MaterialCode = _unitofwork.ProductRepository.GetById(d.MaterialId).ProductCode;
                    }
                }
            }

            return cpheader == null ? null : cpheader;
        }

    }
}
