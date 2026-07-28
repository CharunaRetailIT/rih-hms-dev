using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.TransactionData
{
    public class BLL_DocStatus
    {

        private readonly UnitOfWork _unitofwork;
      
        public BLL_DocStatus()
        {
            _unitofwork = new UnitOfWork();
          
        }

        public BLL_DocStatus(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);

        }
        public DocStatus GetDocStatusById(int id)
        {
            try
            {
                DocStatus docstatus = _unitofwork.DocStatus.Get(d=>d.DocStatusId==id).FirstOrDefault();               
                return docstatus == null ? null : docstatus;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<DocStatus> GetActiveDocStatuses()
        {
            try
            {
                List<DocStatus> docstatus = _unitofwork.DocStatus.Get().ToList();
                return docstatus == null ? null : docstatus;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
