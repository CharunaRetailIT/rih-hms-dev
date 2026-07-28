using RIT.HMS.Data;
using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_GiftVoucherGroup
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_GiftVoucherGroup()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_GiftVoucherGroup(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }

        public int SaveGiftVoucherGroup(InvGiftVoucherGroup GiftVoucherGroup)
        {
            //  _unitofwork.CreateTransaction();
            try
            {
                //int res1 = _unitofwork.Save();
                _unitofwork.GiftVoucherGroupRepository.Insert(GiftVoucherGroup);
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

        public int SaveGiftVoucherMaster(InvGiftVoucherMaster GiftVouchermaster)
        {
            //  _unitofwork.CreateTransaction();
            try
            {
                //int res1 = _unitofwork.Save();
                _unitofwork.GiftVoucherMasterRepository.Insert(GiftVouchermaster);
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

        public int GetnewVoucherGroupID1(int cmpID)
        {
            //  _unitofwork.CreateTransaction();
            try
            {
                //int res1 = _unitofwork.Save();
                _unitofwork.GiftVoucherGroupRepository.GetById(cmpID);
                int res1 = 1;
                // _unitofwork.Commit();

                return res1;
            }
            catch (Exception ex)
            {
                // _unitofwork.Rollback();
                return 0;
            }
        }

        public IEnumerable<InvGiftVoucherGroup> GetnewVoucherGroupID(Int32 compid)
        {
            try
            {

                //IEnumerable<InvGiftVoucherGroup> GiftVouchergroupID = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.CompanyID == compid).OrderBy(g => g.InvGiftVoucherGroupID);
                IEnumerable<InvGiftVoucherGroup> GiftVouchergroupID = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.CompanyID == compid).OrderByDescending(g => g.InvGiftVoucherGroupID).Take(1).Select(g => new InvGiftVoucherGroup { GiftVoucherGroupCode = g.GiftVoucherGroupCode }).ToList();
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

        public IEnumerable<InvGiftVoucherGroup> GetGroups(Int32 compid)
        {
            try
            {

                //IEnumerable<InvGiftVoucherGroup> GiftVouchergroupID = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.CompanyID == compid).OrderBy(g => g.InvGiftVoucherGroupID);
                //IEnumerable<InvGiftVoucherGroup> GiftVouchergroupID = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.CompanyID == compid).OrderByDescending(g => g.InvGiftVoucherGroupID).Take(1).Select(g => new InvGiftVoucherGroup { GiftVoucherGroupCode = g.GiftVoucherGroupCode }).ToList();
                IEnumerable<InvGiftVoucherGroup> GiftVouchergroupID = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.CompanyID == compid).OrderByDescending(g => g.InvGiftVoucherGroupID);
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
        public IEnumerable<InvGiftVoucherGroup> GetGroupCodeName(Int32 GroupID)
        {
            try
            {
                IEnumerable<InvGiftVoucherGroup> GiftVouchergroupID = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.InvGiftVoucherGroupID == GroupID).OrderByDescending(g => g.InvGiftVoucherGroupID);
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

        public IEnumerable<InvGiftVoucherBookCode> GetgvBooks(Int32 compid)
        {
            try
            {

                //IEnumerable<InvGiftVoucherGroup> GiftVouchergroupID = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.CompanyID == compid).OrderBy(g => g.InvGiftVoucherGroupID);
                //IEnumerable<InvGiftVoucherGroup> GiftVouchergroupID = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.CompanyID == compid).OrderByDescending(g => g.InvGiftVoucherGroupID).Take(1).Select(g => new InvGiftVoucherGroup { GiftVoucherGroupCode = g.GiftVoucherGroupCode }).ToList();
                IEnumerable<InvGiftVoucherBookCode> InvGiftVoucherBookCodeID = _unitofwork.GiftVoucherbookRepository.Get(g => g.IsDelete == false && g.GroupOfCompanyID == compid);
                if (InvGiftVoucherBookCodeID != null)
                {

                    return InvGiftVoucherBookCodeID;
                }
                else
                    return null;

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
            catch (Exception ex)
            {

                throw;
            }
        }

        //public IEnumerable<InvGiftVoucherGroup> GetInvGiftVoucherGroupByCode(string giftVoucherMasterGroupCode)
        //{
        // // return context.InvGiftVoucherGroups.Where(u => u.GiftVoucherGroupCode == giftVoucherMasterGroupCode && u.IsDelete == false).FirstOrDefault();
        //    IEnumerable<InvGiftVoucherGroup> InvGiftVoucherGroup = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.GiftVoucherGroupCode == giftVoucherMasterGroupCode);
        //    if (InvGiftVoucherGroup != null)
        //    {

        //        return InvGiftVoucherGroup;
        //    }
        //    else
        //        return null;
        //}

        public InvGiftVoucherGroup GetInvGiftVoucherGroupByCode(string giftVoucherMasterGroupCode)
        {
            return _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.GiftVoucherGroupCode == giftVoucherMasterGroupCode).FirstOrDefault();
        }
        public InvGiftVoucherMaster GetInvGiftVoucherMasterByBookID(long bookID)
        {
            return _unitofwork.GiftVoucherMasterRepository.Get(s => s.InvGiftVoucherBookCodeID == bookID && s.IsDelete == false).FirstOrDefault();
        }



        //check the GV book is available by book code
        public IEnumerable<InvGiftVoucherBookCode> GetInvGiftVoucherBookcodeByCode(string giftVoucherbookcode)
        {
            // return context.InvGiftVoucherGroups.Where(u => u.GiftVoucherGroupCode == giftVoucherMasterGroupCode && u.IsDelete == false).FirstOrDefault();
            IEnumerable<InvGiftVoucherBookCode> InvGiftVoucherBookcode = _unitofwork.GiftVoucherbookRepository.Get(g => g.IsDelete == false && g.BookCode == giftVoucherbookcode);
            if (InvGiftVoucherBookcode != null)
            {

                return InvGiftVoucherBookcode;
            }
            else
                return null;
        }

        public IEnumerable<InvGiftVoucherMaster> GetInvGiftVoucherMasterByBookCodeID(long giftVoucherbookcode,int giftvoucherQTY)
        {
            // return context.InvGiftVoucherGroups.Where(u => u.GiftVoucherGroupCode == giftVoucherMasterGroupCode && u.IsDelete == false).FirstOrDefault();
            IEnumerable<InvGiftVoucherMaster> InvGiftVoucherBookcode = _unitofwork.GiftVoucherMasterRepository.Get(g => g.IsDelete == false && g.InvGiftVoucherBookCodeID == giftVoucherbookcode).Take(giftvoucherQTY);
            //.Take(voucherQty).Select(s => s.VoucherNo).ToList();.Take(1).Select(g => new InvGiftVoucherGroup { GiftVoucherGroupCode = g.GiftVoucherGroupCode }).ToList();

            if (InvGiftVoucherBookcode != null)
            {

                return InvGiftVoucherBookcode;
            }
            else
                return null;
        }

        //public InvGiftVoucherBookCode GetInvGiftVoucherBookcodeByCode(string giftVoucherbookcode)
        //{
        //    return _unitofwork.GiftVoucherbookRepository.Get(g => g.IsDelete == false && g.BookCode == giftVoucherbookcode).FirstOrDefault();
        //}

        public IEnumerable<InvGiftVoucherBookCode> GetgvBookDetails(Int32 compid, string GvBookCode)
        {
            try
            {

                //IEnumerable<InvGiftVoucherGroup> GiftVouchergroupID = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.CompanyID == compid).OrderBy(g => g.InvGiftVoucherGroupID);
                //IEnumerable<InvGiftVoucherGroup> GiftVouchergroupID = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.CompanyID == compid).OrderByDescending(g => g.InvGiftVoucherGroupID).Take(1).Select(g => new InvGiftVoucherGroup { GiftVoucherGroupCode = g.GiftVoucherGroupCode }).ToList();
                IEnumerable<InvGiftVoucherBookCode> InvGiftVoucherBookCodeID = _unitofwork.GiftVoucherbookRepository.Get(g => g.IsDelete == false && g.GroupOfCompanyID == compid && g.BookCode == GvBookCode);
                if (InvGiftVoucherBookCodeID != null)
                {

                    return InvGiftVoucherBookCodeID;
                }
                else
                    return null;

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
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<InvGiftVoucherBookCode> GetgvBookDetailsByID(Int32 compid, int GvBookCodeID)
        {
            try
            {
                IEnumerable<InvGiftVoucherBookCode> InvGiftVoucherBookCodeID = _unitofwork.GiftVoucherbookRepository.Get(g => g.IsDelete == false && g.GroupOfCompanyID == compid && g.InvGiftVoucherBookCodeID == GvBookCodeID);
                if (InvGiftVoucherBookCodeID != null)
                {

                    return InvGiftVoucherBookCodeID;
                }
                else
                    return null;

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
            catch (Exception ex)
            {

                throw;
            }
        }
        public InvGiftVoucherMaster CheckBookCode(long id)
        {
            try
            {

                var BookCode = _unitofwork.GiftVoucherMasterRepository.Get(c => c.InvGiftVoucherBookCodeID == id).FirstOrDefault();
                if (BookCode != null)
                {
                    return BookCode;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //public IEnumerable<AutoGenerateInfo> GenerateVoucherGroupCode(Int32 compid)
        //{
        //    try
        //    {

        //        //IEnumerable<InvGiftVoucherGroup> GiftVouchergroupID = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.CompanyID == compid).OrderBy(g => g.InvGiftVoucherGroupID);
        //        IEnumerable<AutoGenerateInfo> GiftVouchergroupCode = _unitofwork.GiftVoucherGroupRepository.Get(g => g.IsDelete == false && g.CompanyID == compid).OrderByDescending(g => g.InvGiftVoucherGroupID).Take(1).Select(g => new AutoGenerateInfo { GiftVoucherGroupCode = g.GiftVoucherGroupCode }).ToList();
        //        if (GiftVouchergroupCode != null)
        //        {

        //            return GiftVouchergroupCode;
        //        }
        //        else
        //            return null;

        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}

    }
}
