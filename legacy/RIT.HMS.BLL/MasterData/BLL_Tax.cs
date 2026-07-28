using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
   public class BLL_Tax
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Tax()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Tax(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<Tax> GetTaxes(Int32 compid)
        {
            try
            {
                IEnumerable<Tax> tax = _unitofwork.TaxRepository.Get(t => t.IsDelete == false && t.CompanyID==compid).OrderBy(t => t.TaxCode);
                if (tax != null)
                {
                    return tax;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Tax> GetActiveTaxes(Int32 compid)
        {
            try
            {
                IEnumerable<Tax> tax = _unitofwork.TaxRepository.Get(t => t.IsActive == true && t.IsDelete==false && t.CompanyID==compid).OrderBy(t => t.TaxName);
                if (tax != null)
                {
                    return tax;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public int SaveTaxModes(List<TaxModesViewModel> taxmodes)
        {
            List<LocationTax> locationtaxes = new List<LocationTax>();
            List<PayTypeTax> paytypetaxes = new List<PayTypeTax>();
            List<CateringModeTax> cateringmodetaxes = new List<CateringModeTax>();

            foreach (var t in taxmodes)
            {
                if (t.TaxLocationId != 0)
                {
                    LocationTax locationtax = new LocationTax();
                    locationtax.TaxId = t.TaxId;
                    locationtax.TaxLocationId = t.TaxLocationId;
                    locationtax.TaxSequence = taxmodes.IndexOf(t) + 1;
                    locationtax.TaxPracentage = 100;
                    locationtax.LocationId = t.LocationId;
                    locationtax.CompanyID = t.CompanyId;
                    locationtax.CreatedDate = DateTime.Now;
                    locationtax.CreatedUser = t.CreateUser;
                    locationtax.ModifiedDate = DateTime.Now;
                    locationtax.ModifiedUser = t.CreateUser;
                    locationtax.GroupOfCompanyID = 1;
                    locationtax.DataTransfer = 0;

                    locationtaxes.Add(locationtax);
                }
               // else 
                if (t.PayModeId != 0)
                {
                    PayTypeTax paytypetax = new PayTypeTax();
                    paytypetax.TaxId = t.TaxId;
                    paytypetax.PayTypeId = t.PayModeId;
                    paytypetax.TaxSequence = taxmodes.IndexOf(t) + 1;
                    paytypetax.TaxPracentage = 100;
                    paytypetax.LocationId = t.LocationId;
                    paytypetax.CompanyID = t.CompanyId;
                    paytypetax.CreatedDate = DateTime.Now;
                    paytypetax.CreatedUser = t.CreateUser;
                    paytypetax.ModifiedDate = DateTime.Now;
                    paytypetax.ModifiedUser = t.CreateUser;
                    paytypetax.GroupOfCompanyID = 1;
                    paytypetax.DataTransfer = 0;

                    paytypetaxes.Add(paytypetax);
                }
               // else
                if (t.CateringModeId != 0)
                {
                    CateringModeTax cateringmodetax = new CateringModeTax();
                    cateringmodetax.TaxId = t.TaxId;
                    cateringmodetax.CateringModeId = t.CateringModeId;
                    cateringmodetax.TaxSequence = taxmodes.IndexOf(t) + 1;
                    cateringmodetax.TaxPracentage = 100;
                    cateringmodetax.LocationId = t.LocationId;
                    cateringmodetax.CompanyID = t.CompanyId;
                    cateringmodetax.CreatedDate = DateTime.Now;
                    cateringmodetax.CreatedUser = t.CreateUser;
                    cateringmodetax.ModifiedDate = DateTime.Now;
                    cateringmodetax.ModifiedUser = t.CreateUser;
                    cateringmodetax.GroupOfCompanyID = 1;
                    cateringmodetax.DataTransfer = 0;

                    cateringmodetaxes.Add(cateringmodetax);
                }

            }

            if (locationtaxes.Count != 0)
            {
                _unitofwork.LocationTaxRepository.BulkInsert(locationtaxes);
                
            }
            if (paytypetaxes.Count != 0)
            {
                _unitofwork.PayTypeTaxRepository.BulkInsert(paytypetaxes);
              
            }
            if (cateringmodetaxes.Count != 0)
            {
                _unitofwork.CateringModeTaxRepository.BulkInsert(cateringmodetaxes);
               
            }

            return _unitofwork.Save();
        }


        public List<TaxModesViewModel> GettaxModes(int companyid,int locationid,int taxid)
        {
            var loc = _unitofwork.LocationTaxRepository.Get(l=>l.CompanyID==companyid && l.LocationId==locationid && l.TaxId==taxid).OrderBy(l=>l.LocationTaxId).ToList();
            var pay= _unitofwork.PayTypeTaxRepository.Get(l => l.CompanyID == companyid && l.LocationId == locationid && l.TaxId == taxid).OrderBy(l=>l.PayTypeTaxId).ToList();
            var cat = _unitofwork.CateringModeTaxRepository.Get(l => l.CompanyID == companyid && l.LocationId == locationid && l.TaxId == taxid).OrderBy(l => l.CateringModeTaxId).ToList();

            foreach (var l in loc)
            {

            }


            return new List<TaxModesViewModel>();
        }

        public IEnumerable<PayType> GetActivePayModes(Int32 compid)
        {
            try
            {
                IEnumerable<PayType> tax = _unitofwork.PayTypeRepository.Get(t => t.IsActive == true).OrderBy(t => t.Descrip);
                if (tax != null)
                {
                    return tax;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Tax GetTaxById(long id)
        {
            try
            {
                Tax tax = _unitofwork.TaxRepository.GetById(id);
                if (tax != null)
                {
                    return tax;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveTax(Tax rm)
        {
            try
            {
                _unitofwork.TaxRepository.Insert(rm);
                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateTax(Tax rm)
        {
            try
            {

                _unitofwork.TaxRepository.Update(rm);
                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Tax GetTaxByCode(string code, Int32 compid)
        {
            try
            {
                Tax tax = _unitofwork.TaxRepository.Get(g => g.TaxCode == code && g.CompanyID == compid).FirstOrDefault();
                if (tax != null)
                {
                    return tax;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<GRNTaxViewModel> GetProductTaxByProductId(long productid)
        {
            try
            {

                List<GRNTaxViewModel> tvvm = new List<GRNTaxViewModel>();
                var producttax = (
                           from p in _unitofwork.ProductTaxRepository.Get(filter:p=> p.ProductId == productid)
                           join t in _unitofwork.TaxRepository.Get(filter:t=>t.IsPurchasingTax == true) on p.TaxId equals t.TaxId
                          // where p.ProductId == productid && t.IsPurchasingTax == true
                           orderby p.ProductTaxId ascending
                           select new
                           {
                               ProductId = p.ProductId,
                               TaxId = p.TaxId,
                               TaxPrc = t.TaxPercentage,
                           }
                       ).ToList();

                foreach (var p in producttax)
                {
                    GRNTaxViewModel tv = new GRNTaxViewModel();
                    tv.ProductId = p.ProductId;
                    tv.TaxId = p.TaxId;
                    tv.TaxPrc = p.TaxPrc;
                    tvvm.Add(tv);
                }


                if (producttax != null)
                {
                    return tvvm;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
