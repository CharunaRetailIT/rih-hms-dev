using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_GiftVoucherPO
    {
        private readonly UnitOfWork _unitofwork;
        ApplicationDbContext dbcontext = new ApplicationDbContext();
        public BLL_GiftVoucherPO()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_GiftVoucherPO(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }

        public IEnumerable<Supplier> GetSuppliers(Int32 compid)
        {
            try
            {
                IEnumerable<Supplier> GiftVoucherSupplier = _unitofwork.SuplierRepository.Get(g => g.IsDelete == false);
                if (GiftVoucherSupplier != null)
                {

                    return GiftVoucherSupplier;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        //GetPOHeads
        public IEnumerable<InvGiftVoucherPurchaseOrderHeader> GetPOHeads(Int32 compid)
        {
            try
            {
                IEnumerable<InvGiftVoucherPurchaseOrderHeader> GiftVoucherPOr = _unitofwork.GVPOHRepository.Get();
                if (GiftVoucherPOr != null)
                {
                    return GiftVoucherPOr;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<InvGiftVoucherPurchaseOrderDetail> GetPODetails(int POHID)
        {
            try
            {
                IEnumerable<InvGiftVoucherPurchaseOrderDetail> GiftVoucherPOD = _unitofwork.GVPODetailsRepository.Get(g => g.InvGiftVoucherPurchaseOrderHeaderID == POHID);
                if (GiftVoucherPOD != null)
                {
                    return GiftVoucherPOD;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<InvGiftVoucherMaster> GetGVMasterDetails(int GVMasterID)
        {
            try
            {
                IEnumerable<InvGiftVoucherMaster> GiftVoucherMaster = _unitofwork.GiftVoucherMasterRepository.Get(g => g.InvGiftVoucherMasterID == GVMasterID);
                if (GiftVoucherMaster != null)
                {
                    return GiftVoucherMaster;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        
        public IEnumerable<InvGiftVoucherPurchaseOrderHeader> GetPOHeadsDetails(long DocumentNo)
        {
            try
            {
                IEnumerable<InvGiftVoucherPurchaseOrderHeader> GiftVoucherPO = _unitofwork.GVPOHRepository.Get(g => g.InvGiftVoucherPurchaseOrderHeaderID == DocumentNo);
                if (GiftVoucherPO != null)
                {
                    return GiftVoucherPO;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public IEnumerable<InvGiftVoucherPurchaseOrderHeader> GetPOHeadsDetailsbyHeaderID(long DocumentNo)
        {
            try
            {
                IEnumerable<InvGiftVoucherPurchaseOrderHeader> GiftVoucherPO = _unitofwork.GVPOHRepository.Get(g => g.InvGiftVoucherPurchaseOrderHeaderID == DocumentNo);
                if (GiftVoucherPO != null)
                {
                    return GiftVoucherPO;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }



        public IEnumerable<PaymentTerm> GetPaymentTerm(Int32 compid)
        {
            try
            {
                IEnumerable<PaymentTerm> GiftVoucherTerm = _unitofwork.PaymentTermRepository.Get(g => g.IsDelete == false);
                if (GiftVoucherTerm != null)
                {

                    return GiftVoucherTerm;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<SysLocation> GetAllLocation()
        {
            try
            {
                IEnumerable<SysLocation> LocationA = _unitofwork.LocationRepository.Get(g => g.IsDelete == false);
                if (LocationA != null)
                {

                    return LocationA;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public InvGiftVoucherPurchaseOrderHeader CheckPOHeader(int docId)
        {
            try
            {

                var POHeader = _unitofwork.GVPOHRepository.Get(c => c.DocumentID == docId).FirstOrDefault();
                if (POHeader != null)
                {
                    return POHeader;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public InvGiftVoucherPurchaseOrderHeader GetPurchaseOrderHeaderID()
        {
            try
            {

               // var POHeader = _unitofwork.GVPOHRepository.Get(c => c.DocumentID == docId).FirstOrDefault();

                return _unitofwork.GVPOHRepository.Get(g => g.DataTransfer==0).OrderByDescending(s => s.InvGiftVoucherPurchaseOrderHeaderID).FirstOrDefault();
                
                ////if (POHeader != null)
                ////{
                //return POHeader;
                ////}
                //else
                //    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public int SaveGiftVoucherPOH(InvGiftVoucherPurchaseOrderHeader GiftVoucherPO)
        {
            //  _unitofwork.CreateTransaction();
            try
            {
                //int res1 = _unitofwork.Save();
                _unitofwork.GVPOHRepository.Insert(GiftVoucherPO);
                int res1 = _unitofwork.Save();
                // _unitofwork.Commit();

                return res1;
            }
            catch (Exception ex)
            {
                // _unitofwork.Rollback();
                return 0;
            }

        }

        public int SaveGiftVoucherDetails(InvGiftVoucherPurchaseOrderDetail GiftVoucherPODetails)
        {
            //  _unitofwork.CreateTransaction();
            try
            {
                //int res1 = _unitofwork.Save();
                _unitofwork.GVPODetailsRepository.Insert(GiftVoucherPODetails);
                int res1 = _unitofwork.Save();
                // _unitofwork.Commit();

                return res1;
            }
            catch (Exception ex)
            {
                // _unitofwork.Rollback();
                return 0;
            }

        }
        public IEnumerable<InvGiftVoucherDocumentNumber> GetnewVoucherDocNo(string DocumentName)
        {
            try
            {
                IEnumerable<InvGiftVoucherDocumentNumber> GiftVouchergroupID = _unitofwork.GVPODocNoRepository.Get(g => g.DocumentName == DocumentName).OrderByDescending(g => g.DocumentNumberId).Take(1).Select(g => new InvGiftVoucherDocumentNumber { DocumentNo = g.DocumentNo }).ToList();
                if (GiftVouchergroupID != null)
                {

                    return GiftVouchergroupID;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<InvGiftVoucherDocumentNumber> GetnewVoucherDocNo(Int32 compid)
        {
            try
            {
                IEnumerable<InvGiftVoucherDocumentNumber> GiftVouchergroupID = _unitofwork.GVPODocNoRepository.Get(g => g.CompanyID == compid).OrderByDescending(g => g.DocumentNumberId).Take(1).Select(g => new InvGiftVoucherDocumentNumber { DocumentNo = g.DocumentNo }).ToList();
                if (GiftVouchergroupID != null)
                {

                    return GiftVouchergroupID;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public int SaveGiftVoucherDocNo(InvGiftVoucherDocumentNumber GiftVoucherDocNo)
        {
            //  _unitofwork.CreateTransaction();
            try
            {
                //int res1 = _unitofwork.Save();
                _unitofwork.GVPODocNoRepository.Insert(GiftVoucherDocNo);
                int res1 = _unitofwork.Save();
                // _unitofwork.Commit();

                return res1;
            }
            catch (Exception ex)
            {
                // _unitofwork.Rollback();
                return 0;
            }

        }

        public int UpdateGVMaster(InvGiftVoucherMaster GVMaster)
        {
            try
            {
                _unitofwork.GiftVoucherMasterRepository.Update(GVMaster);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        //public List<InvGiftVoucherBookCode> SpgetBookcode()
        //{
        //    try
        //    {
        //        return dbcontext.Database.SqlQuery<InvGiftVoucherBookCode>("[dbo].[SpGetInvGiftVoucherBookCodes]").ToList();

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return dbcontext.Database.SqlQuery<InvGiftVoucherBookCode>("[dbo].[SpGetInvGiftVoucherBookCodes]").ToList();
        //}
    }
}
