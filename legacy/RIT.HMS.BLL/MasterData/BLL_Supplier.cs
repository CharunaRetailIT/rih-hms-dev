using RIT.HMS.BLL.Common;
using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_Supplier
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Supplier()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Supplier(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<Supplier> GetSuppliers(Int32 compid)
        {
            try
            {
                IEnumerable<Supplier> suppliers = _unitofwork.SuplierRepository.Get(s => s.IsDelete == false && s.CompanyID==compid).OrderBy(s => s.SupplierCode);
                if (suppliers != null)
                {
                    return suppliers;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IEnumerable<Supplier> GetActiveSuppliers(Int32 compid)
        {
            try
            {
                IEnumerable<Supplier> suppliers = _unitofwork.SuplierRepository.Get(s => s.IsDelete == false && s.IsBlocked == false && s.CompanyID==compid).OrderBy(s => s.SupplierName);
                if (suppliers != null)
                {
                    return suppliers;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Supplier GetSupplierById(long id)
        {
            try
            {
                Supplier suppliers = _unitofwork.SuplierRepository.GetById(id);
                if (suppliers != null)
                {
                    return suppliers;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

 

        public int SaveSupplier(Supplier s)
        {
            try
            {
                _unitofwork.SuplierRepository.Insert(s);
                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public int UpdateSupplier(Supplier sup)
        {
            try
            {
                // SupplierService supservice = new Service.SupplierService();
                var exists = GetSupplierById(sup.SupplierID);
                
                exists.SupplierName = sup.SupplierName;
                exists.Gender = sup.Gender;
                exists.SupplierTypeID = sup.SupplierTypeID;
                exists.ContactPersonName = sup.ContactPersonName;
                exists.BillingAddress1 = sup.BillingAddress1;
                exists.BillingAddress2 = sup.BillingAddress2;
                exists.BillingAddress3 = sup.BillingAddress3;
                exists.BillingTelephone = sup.BillingTelephone;
                exists.BillingMobile = sup.BillingMobile;
                exists.BillingFax = sup.BillingFax;
                exists.Email = sup.Email;
                exists.RepresentativeName = sup.RepresentativeName;
                exists.RepresentativeNICNo = sup.RepresentativeNICNo;
                exists.PayeeName = sup.PayeeName;
                exists.DeliveryAddress1 = sup.DeliveryAddress1;
                exists.DeliveryAddress2 = sup.DeliveryAddress2;
                exists.DeliveryAddress3 = sup.DeliveryAddress3;
                exists.DeliveryTelephone = sup.DeliveryTelephone;
                exists.DeliveryMobile = sup.DeliveryMobile;
                exists.DeliveryFax = sup.DeliveryFax;
                exists.ReferenceNo = sup.ReferenceNo;
                exists.ReferenceSerial = sup.ReferenceSerial;
                exists.PostalCode = sup.PostalCode;
                exists.TaxID1 = sup.TaxID1;
                exists.TaxNo1 = sup.TaxNo1;
                exists.TaxID2 = sup.TaxID2;
                exists.TaxNo2 = sup.TaxNo2;
                exists.TaxID3 = sup.TaxID3;
                exists.TaxNo3 = sup.TaxNo3;
                exists.TaxID4 = sup.TaxID4;
                exists.TaxNo4 = sup.TaxNo4;
                exists.TaxID5 = sup.TaxID5;
                exists.TaxNo5 = sup.TaxNo5;
                exists.TaxRegistrationNo = sup.TaxRegistrationNo;
                exists.TaxRegistrationName = sup.TaxRegistrationName;
                exists.PaymentMethod = sup.PaymentMethod;
                exists.CreditLimit = sup.CreditLimit;
                exists.ChequeLimit = sup.ChequeLimit;
                exists.ChequePeriod = sup.ChequePeriod;
                exists.PaymentTermID = sup.PaymentTermID;
                exists.CreditPeriod = sup.CreditPeriod;
                exists.ProductBusinessType = sup.ProductBusinessType;
                exists.SuppliedProducts = sup.SuppliedProducts;
                exists.OrderCircle = sup.OrderCircle;
                exists.SupplierGroupID = sup.SupplierGroupID;
                exists.LedgerID = sup.LedgerID;
                exists.OtherLedgerID = sup.OtherLedgerID;
                exists.TaxIdNo = sup.TaxIdNo;
                exists.DepositeAmount = sup.DepositeAmount;
                exists.EmailBoday = sup.EmailBoday;
                exists.EmailSubject = sup.EmailSubject;
                exists.Remark = sup.Remark;
                exists.IsUpload = sup.IsUpload;
                exists.IsSuspended = sup.IsSuspended;
                exists.IsPOMail = sup.IsPOMail;
                exists.IsBlocked = sup.IsBlocked;
                exists.IsDelete = sup.IsDelete;
                exists.SupplierTitle = sup.SupplierTitle;
                //exists.CreatedDate = sup.CreatedDate;
                //exists.CreatedUser = sup.CreatedUser;
                exists.ModifiedDate = DateTime.Now;
                exists.ModifiedUser = sup.ModifiedUser;
                if (sup.Photograph != null)
                {
                    byte[] newlogo;
                    using (BinaryReader br = new BinaryReader(sup.Photograph.InputStream))
                    {
                        newlogo = br.ReadBytes(sup.Photograph.ContentLength);
                        sup.SupplierPicture = newlogo;
                        sup.SupplierPictureName = sup.Photograph.FileName;
                        sup.SupplierPictureType = sup.Photograph.ContentType;
                    }

                    if (sup.SupplierPictureName != exists.SupplierPictureName)
                    {
                        byte[] pic;
                        using (BinaryReader br = new BinaryReader(sup.Photograph.InputStream))
                        {
                            pic = br.ReadBytes(sup.Photograph.ContentLength);
                            exists.SupplierPicture = pic;
                            exists.SupplierPictureName = sup.Photograph.FileName;
                            exists.SupplierPictureType = sup.Photograph.ContentType;
                        }
                    }
                }

                _unitofwork.SuplierRepository.Update(exists);

                LOGSupplier lgsupplier = new LOGSupplier();
                var mappedsupplier =  HMSExtensions.MatchAndMap(exists,lgsupplier);
                mappedsupplier.SourceId =Convert.ToInt32(exists.SupplierID);
                _unitofwork.LOGSupplier.Insert(mappedsupplier);
                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Supplier GetSupplierByCode(string code, Int32 compid)
        {
            try
            {
                Supplier supplier = _unitofwork.SuplierRepository.Get(g => g.SupplierCode == code && g.CompanyID==compid).FirstOrDefault();
                if (supplier != null)
                {
                    return supplier;
                }
                else
                    return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<SupplierType> GetSupplierTypes(Int32 compid)
        {
            try
            {
                IEnumerable<SupplierType> suppliertypes = _unitofwork.SuplierTypeRepository.Get(s=>s.CompanyID== compid).OrderBy(s => s.SupplierTypeCode);
                if (suppliertypes != null)
                {
                    return suppliertypes;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SupplierType GetSupplierTypesById(int id)
        {
            try
            {
                SupplierType suppliertypes = _unitofwork.SuplierTypeRepository.Get(g => g.SupplierTypeID == id).FirstOrDefault();
                if (suppliertypes != null)
                {
                    return suppliertypes;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IEnumerable<SupplierType> GetActiveSupplierTypes(Int32 compid)
        {
            try
            {
                IEnumerable<SupplierType> suppliertypes = _unitofwork.SuplierTypeRepository.Get(s => s.IsDelete == false && s.CompanyID==compid).OrderBy(s => s.SupplierTypeCode);
                if (suppliertypes != null)
                {
                    return suppliertypes;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        //Added by pavithra on 2019-11-30
        public Supplier FindByCode(string code, Int32 compid)
        {
            var supplier = _unitofwork.SuplierRepository.Get(c => c.SupplierCode == code && c.CompanyID==compid).FirstOrDefault();
            if (supplier != null)
            {
                return supplier;
            }
            else
            {
                return null;
            }
        }
    }
}
