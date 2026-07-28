using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class SupplierService
    {

        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<Supplier> GetSuppliers()
        {
            try
            {
                IEnumerable<Supplier> suppliers = context.Supplier.Where(s=>s.IsDelete==false).OrderBy(s => s.SupplierCode);
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

        public IEnumerable<Supplier> GetActiveSuppliers()
        {
            try
            {
                IEnumerable<Supplier> suppliers = context.Supplier.Where(s => s.IsDelete == false).OrderBy(s => s.SupplierCode);
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
                Supplier suppliers = context.Supplier.Where(s => s.SupplierID == id).FirstOrDefault();
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
                context.Supplier.Add(s);
                int res = context.SaveChanges();
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

                    SupplierService supservice = new Service.SupplierService();
                    var exists = supservice.GetSupplierById(sup.SupplierID);

                
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

                    // int res = context.SaveChanges();
                    context.Supplier.Attach(exists);
                    context.Entry(exists).State = EntityState.Modified;
                    int res = context.SaveChanges();          
                    return res;
             }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Supplier GetSupplierByCode(string code)
        {
            try
            {
                Supplier supplier = context.Supplier.Where(g => g.SupplierCode == code).FirstOrDefault();
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


        public IEnumerable<SupplierType> GetSupplierTypes()
        {
            try
            {
                IEnumerable<SupplierType> suppliertypes = context.SupplierType.OrderBy(s => s.SupplierTypeCode);
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


        public IEnumerable<SupplierType> GetActiveSupplierTypes()
        {
            try
            {
                IEnumerable<SupplierType> suppliertypes = context.SupplierType.Where(s => s.IsDelete == false).OrderBy(s => s.SupplierTypeCode);
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




    }
}