using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.TransactionData
{
    public class BLL_AdvanceNote
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_AdvanceNote()
        {
            _unitofwork = new UnitOfWork();

        }
        public BLL_AdvanceNote(string connection)
        {
            _unitofwork = new UnitOfWork(connection);

        }
        public List<InvAdvanceNoteHed> AdvanceNoteReport(InvAdvanceNoteHed header)
        {

            List<InvAdvanceNoteHed> adheader = new List<InvAdvanceNoteHed>();

            if (header.ProcessLoc != 0 && header.PickupLoc != 0 && header.LocationID !=0)
            {
                adheader = _unitofwork.InvAdvanceNoteHedRepository.Get(h => h.ProcessLoc == header.ProcessLoc &&
                                                                                h.PickupLoc == header.PickupLoc &&
                                                                                h.LocationID == header.LocationID &&
                                                                                DbFunctions.TruncateTime(h.Date) >= header.DateFrom.Date &&
                                                                                DbFunctions.TruncateTime(h.Date) <= header.DateTo.Date &&
                                                                                h.CompanyId== header.CompanyId
                                                                                ).ToList();

            }else if (header.ProcessLoc == 0 && header.PickupLoc == 0 && header.LocationID == 0)
            {
                adheader = _unitofwork.InvAdvanceNoteHedRepository.Get(h => DbFunctions.TruncateTime(h.Date) >= header.DateFrom.Date &&
                                                                            DbFunctions.TruncateTime(h.Date) <= header.DateTo.Date &&
                                                                             h.CompanyId == header.CompanyId
                                                                            ).ToList();

            }else if (header.ProcessLoc != 0 && header.PickupLoc == 0 && header.LocationID == 0)
            {
                adheader = _unitofwork.InvAdvanceNoteHedRepository.Get(h => h.ProcessLoc == header.ProcessLoc &&
                                                                                DbFunctions.TruncateTime(h.Date) >= header.DateFrom.Date &&
                                                                                DbFunctions.TruncateTime(h.Date) <= header.DateTo.Date &&
                                                                                 h.CompanyId == header.CompanyId
                                                                                ).ToList();
            }
            else if (header.ProcessLoc != 0 && header.PickupLoc != 0 && header.LocationID == 0)
            {
                adheader = _unitofwork.InvAdvanceNoteHedRepository.Get(h => h.ProcessLoc == header.ProcessLoc &&
                                                                                h.PickupLoc == header.PickupLoc &&
                                                                                DbFunctions.TruncateTime(h.Date) >= header.DateFrom.Date &&
                                                                                DbFunctions.TruncateTime(h.Date) <= header.DateTo.Date &&
                                                                                 h.CompanyId == header.CompanyId
                                                                                ).ToList();
            }
            else if (header.ProcessLoc == 0 && header.PickupLoc != 0 && header.LocationID != 0)
            {
                adheader = _unitofwork.InvAdvanceNoteHedRepository.Get(h => h.PickupLoc == header.PickupLoc &&
                                                                                h.LocationID == header.LocationID &&
                                                                                DbFunctions.TruncateTime(h.Date) >= header.DateFrom.Date &&
                                                                                DbFunctions.TruncateTime(h.Date) <= header.DateTo.Date &&
                                                                                 h.CompanyId == header.CompanyId
                                                                                ).ToList();
            }
            else if (header.ProcessLoc != 0 && header.PickupLoc == 0 && header.LocationID != 0)
            {
                adheader = _unitofwork.InvAdvanceNoteHedRepository.Get(h => h.ProcessLoc == header.ProcessLoc &&
                                                                                h.LocationID == header.LocationID &&
                                                                                DbFunctions.TruncateTime(h.Date) >= header.DateFrom.Date &&
                                                                                DbFunctions.TruncateTime(h.Date) <= header.DateTo.Date &&
                                                                                 h.CompanyId == header.CompanyId
                                                                                ).ToList();
            }
            else if (header.ProcessLoc == 0 && header.PickupLoc == 0 && header.LocationID != 0)
            {
                adheader = _unitofwork.InvAdvanceNoteHedRepository.Get(h => h.LocationID == header.LocationID &&
                                                                                DbFunctions.TruncateTime(h.Date) >= header.DateFrom.Date &&
                                                                                DbFunctions.TruncateTime(h.Date) <= header.DateTo.Date &&
                                                                                 h.CompanyId == header.CompanyId
                                                                                ).ToList();
            }
            else if (header.ProcessLoc == 0 && header.PickupLoc != 0 && header.LocationID == 0)
            {
                adheader = _unitofwork.InvAdvanceNoteHedRepository.Get(h =>h.PickupLoc == header.PickupLoc &&
                                                                                DbFunctions.TruncateTime(h.Date) >= header.DateFrom.Date &&
                                                                                DbFunctions.TruncateTime(h.Date) <= header.DateTo.Date &&
                                                                                 h.CompanyId == header.CompanyId
                                                                                ).ToList();
            }


            if (adheader != null)
            {
                foreach (var h in adheader)
                {
                    h.InvAdvanceNoteDet = _unitofwork.InvAdvanceNoteDetRepository.Get(d=>d.CreditNoteNo.Remove(d.CreditNoteNo.Length-1,1)==h.AdNoteNo).ToList();

                    foreach (var d in h.InvAdvanceNoteDet)
                    {
                       
                        var prd = _unitofwork.ProductRepository.GetById(d.ProductID);
                        if (prd != null)
                        {
                            d.ProductName = prd.ProductName;
                        }
                        else
                        {
                            d.ProductName = "N/A";
                        }
                    }
                }
            }

            return adheader == null ? null : adheader;
        }


    }
}
