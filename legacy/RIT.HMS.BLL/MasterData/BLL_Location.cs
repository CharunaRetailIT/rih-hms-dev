using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Common;
using RIT.HMS.Domain.Loyalty;

namespace RIT.HMS.BLL.MasterData
{
   public class BLL_Location
    {
       private readonly UnitOfWork _unitofwork;
        public  BLL_Location()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Location(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<SysLocation> GetLocations(Int32 compid)
        {
            try
            {
                IEnumerable<SysLocation> syslocation = _unitofwork.LocationRepository.Get(g => g.IsDelete == false && g.CompanyID==compid).OrderBy(g => g.LocationCode);
                if (syslocation != null)
                {
                    return syslocation;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<SysLocation> GetActiveLocations(Int32 compid)
        {
            try
            {
                IEnumerable<SysLocation> syslocation = _unitofwork.LocationRepository.Get(g => g.IsDelete == false && g.IsActive == true && g.CompanyID==compid).OrderBy(g => g.LocationCode);
                if (syslocation != null)
                {
                    return syslocation;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<SysLocation> GetAllActiveLocations()
        {
            try
            {
                IEnumerable<SysLocation> syslocation = _unitofwork.LocationRepository.Get(g => g.IsDelete == false && g.IsActive == true).OrderBy(g => g.LocationCode);
                if (syslocation != null)
                {
                    return syslocation;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<UnitOfMeasure> GetUnitOfMeasures(Int32 compid)
        {
            try
            {
                IEnumerable<UnitOfMeasure> unitofmeasure = _unitofwork.UnitOfMeasureRepository.Get(u => u.CompanyID == compid).OrderBy(um => um.UnitOfMeasureCode);
                if (unitofmeasure != null)
                {
                    return unitofmeasure;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<SysLocation> GetActiveGeneralLocations(Int32 compid)
        {
            try
            {
                IEnumerable<SysLocation> syslocation = _unitofwork.LocationRepository.Get(g => g.IsDelete == false && g.IsActive == true && g.CompanyID == compid && g.LocationTypeId == 1).OrderBy(g => g.LocationCode);
                if (syslocation != null)
                {
                    return syslocation;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<SysLocation> GetActiveKitchenLocations(Int32 compid)
        {
            try
            {
                IEnumerable<SysLocation> syslocation = _unitofwork.LocationRepository.Get(g => g.IsDelete == false && g.IsActive == true && g.CompanyID == compid && g.LocationTypeId == 2).OrderBy(g => g.LocationCode);
                if (syslocation != null)
                {
                    return syslocation;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public IEnumerable<KitchenMaster> GetActiveKitchens(int compid)
        {
            try
            {
                IEnumerable<KitchenMaster> kitchens = _unitofwork.KitchenMasterRepository.Get(g =>g.IsActive == true  && g.CompanyID == compid).OrderBy(g => g.KitchenCode);
                return kitchens ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SysLocation GetLocationById(long id)
        {
            try
            {
                //  SysLocation syslocation = _unitofwork.LocationRepository.Get(g => g.SysLocationID == id).FirstOrDefault();
                // Changed by hasanka 
                SysLocation syslocation = _unitofwork.LocationRepository.GetById(id);
                if (syslocation != null)
                {
                    return syslocation;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SysCompany GetCompanyDetails()
        {
            try
            {
                //  SysLocation syslocation = _unitofwork.LocationRepository.Get(g => g.SysLocationID == id).FirstOrDefault();
                // Changed by hasanka 
                //.Get(g => g.IsHeadOffice==true).FirstOrDefault()
                SysCompany syscompany = _unitofwork.CompanyRepository.Get().FirstOrDefault();
                if (syscompany != null)
                {
                    return syscompany;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveLocation(SysLocation loc)
        {
            try
            {
                _unitofwork.LocationRepository.Insert(loc);
                int res = _unitofwork.Save();
                if (res == 1)
                {
                    if (loc.InheritProducts == true)
                    {
                        InheritProductsFormHeadOffice(loc);
                    }

                    var docs = _unitofwork.DocumentNumberRepository.Get().Select(d => new { d.DocumentId, d.DocumentName, d.DocumentYear, d.PrefixCode }).Distinct();
                    List<DocumentNumber> docnumbers = new List<DocumentNumber>();
                    foreach (var d in docs)
                    {
                        DocumentNumber docnumber = new DocumentNumber();
                        docnumber.DocumentId = d.DocumentId;
                        docnumber.DocumentName = d.DocumentName;
                        docnumber.DocumentYear = d.DocumentYear;
                        docnumber.PrefixCode = d.PrefixCode;
                        docnumber.TempDocumentNo = 0;
                        docnumber.DocumentNo = 0;
                        docnumber.TemplateDocumentNo = "0";
                        docnumber.GroupOfCompanyID = loc.GroupOfCompanyID;
                        docnumber.CompanyID = loc.CompanyID;
                        docnumber.LocationId = loc.SysLocationID;
                        docnumber.CreatedDate = DateTime.Now;
                        docnumber.CreatedUser = loc.CreatedUser;
                        docnumber.ModifiedDate = DateTime.Now;
                        docnumber.DataTransfer = 1;
                        docnumbers.Add(docnumber);
                    }
                    _unitofwork.DocumentNumberRepository.BulkInsert(docnumbers);

                    if (loc.IsHeadOffice)
                    {
                        var exists = _unitofwork.cardGenerationLocationSettingReporsitory.Get().SingleOrDefault();
                        if (exists != null)
                        {
                            _unitofwork.cardGenerationLocationSettingReporsitory.Delete(exists);
                        }

                        CardGenerationLocationSetting cardnogen = new CardGenerationLocationSetting();
                        cardnogen.GroupOfCompanyID = 1;
                        cardnogen.CompanyID = loc.CompanyID;
                        cardnogen.LocationId = loc.SysLocationID;
                        cardnogen.CreatedDate = DateTime.Now;
                        cardnogen.ModifiedDate = DateTime.Now;
                        cardnogen.CreatedUser = loc.CreatedUser;
                        cardnogen.CardNoLength = 7;
                        cardnogen.CardStartingNo = 1;
                        cardnogen.EncodeStartingNo = 1;
                        cardnogen.IsDelete = false;
                        _unitofwork.cardGenerationLocationSettingReporsitory.Insert(cardnogen);
                    }

                    _unitofwork.Save();
                }
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int InheritProductsFormHeadOffice(SysLocation loc)
        {
            try
            {
                var headofficeid = _unitofwork.LocationRepository.Get(l => l.IsHeadOffice && l.CompanyID==loc.CompanyID).FirstOrDefault().SysLocationID;
                var hoproducts = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == headofficeid).ToList();
                foreach (var prd in hoproducts)
                {
                    var ps = new ProductStockMaster();
                    ps.ProductId = prd.ProductId;
                    ps.LocationId = loc.SysLocationID;
                    ps.CostCentreId = loc.SysLocationID;
                    ps.CompanyID = loc.CompanyID;

                    ps.CostPrice = prd.CostPrice;
                    ps.SellingPrice = prd.SellingPrice;
                    ps.ReOrderLevel = prd.ReOrderLevel;
                    ps.ReOrderQuantity = prd.ReOrderQuantity;
                    ps.ReOrderPeriod = prd.ReOrderPeriod;
                    ps.MaxPrice = prd.MaxPrice;
                    ps.MinimumPrice = prd.MinimumPrice;
                    ps.DiscountPrc = prd.DiscountPrc;
                    ps.ForignCustomerPrice = prd.ForignCustomerPrice;
                    ps.Stock = 0;
                    ps.CostCentreId = loc.SysLocationID;
                    ps.DocumentNo = "";

                    ps.ProductCode = prd.ProductCode;
                    ps.ProductName = prd.ProductName;
                    ps.Barcode = prd.Barcode;
                    ps.StockCode = prd.ProductCode;
                    ps.RefNo1 = prd.RefNo1;
                    ps.RefNo2 = prd.RefNo2;

                    ps.ExtendedId = 0;
                    ps.ExtendedName = "1";
                    ps.PLUCode = "1";
                    ps.WeightPerunit = 1;
                    ps.UomId = 0;
                    ps.Unit = "1";
                    ps.AvgCost = 0;
                    ps.FixedGP = 0;
                    ps.OpenBal = 0;
                    ps.InitSIH = 0;
                    ps.InitCost = 0;
                    ps.AdjQty = 0;
                    ps.AvgCost = 0;
                    ps.IsDamage = false;
                    ps.IsActive = prd.IsActive;
                    ps.IsBundle = false;
                    ps.IsInitialize = false;
                    ps.DataTransfer = 0;
                    ps.Ispacksize = false;
                    ps.Iscommission = false;
                    ps.Isdecimal = false;

                    ps.GroupOfCompanyID = prd.GroupOfCompanyID;
                    ps.LocationId = loc.SysLocationID;
                    ps.CompanyID = prd.CompanyID;
                    ps.CreatedDate = prd.CreatedDate;
                    ps.CreatedUser = prd.CreatedUser;
                    ps.ModifiedDate = prd.ModifiedDate;
                    ps.ModifiedUser = prd.ModifiedUser;


                    _unitofwork.ProductStockMasterRepository.Insert(ps);


                }

                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {

                return 0;
            }
        }

        public int UpdateLocation(SysLocation loc)
        {
            try
            {
                _unitofwork.LocationRepository.Update(loc);
                int res = _unitofwork.Save();
                if (res == 1)
                {
                    if (loc.InheritProducts == true)
                    {
                        InheritProductsFormHeadOffice(loc);
                    }

                    if (loc.IsHeadOffice)
                    {
                        var exists = _unitofwork.cardGenerationLocationSettingReporsitory.Get().SingleOrDefault();
                        if (exists != null)
                        {
                            _unitofwork.cardGenerationLocationSettingReporsitory.Delete(exists);
                        }
                        CardGenerationLocationSetting cardnogen = new CardGenerationLocationSetting();
                        cardnogen.GroupOfCompanyID = 1;
                        cardnogen.CompanyID = loc.CompanyID;
                        cardnogen.LocationId = loc.SysLocationID;
                        cardnogen.CreatedDate = DateTime.Now;
                        cardnogen.ModifiedDate = DateTime.Now;
                        cardnogen.CreatedUser = loc.CreatedUser;
                        cardnogen.CardNoLength = 7;
                        cardnogen.CardStartingNo = 1;
                        cardnogen.EncodeStartingNo = 1;
                        cardnogen.IsDelete = false;
                        _unitofwork.cardGenerationLocationSettingReporsitory.Insert(cardnogen);
                    }
                    _unitofwork.Save();
                }

                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public SysLocation GetLocByCode(string code,int companyid)
        {
            try
            {
                SysLocation loc = _unitofwork.LocationRepository.Get(g => g.LocationCode == code && g.CompanyID==companyid).FirstOrDefault();
                if (loc != null)
                {
                    return loc;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int CheckHeadOffice()
        {
            try
            {
                return _unitofwork.LocationRepository.Get(g => g.IsHeadOffice == true).Count();

            }
            catch (Exception)
            {

                return 0;
            }
        }

        public List<ProductStockMaster> GetStockMasterByLocId(long id)
        {
            try
            {
                List<ProductStockMaster> sm = _unitofwork.ProductStockMasterRepository.Get(g => g.LocationId == id).ToList();
                if (sm != null)
                {
                    return sm;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public SysLocation GetHeadOfiice()
        {
            try
            {
                SysLocation ho= _unitofwork.LocationRepository.Get(g => g.IsHeadOffice==true).FirstOrDefault();
                if (ho != null)
                {
                    return ho;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        //Added by pavithra on 2019-11-30
        public SysLocation FindByCode(string code)
        {
            var location = _unitofwork.LocationRepository.Get(c => c.LocationCode == code).FirstOrDefault();
            if (location != null)
            {
                return location;
            }
            else
            {
                return null;
            }

        }

        public KitchenMaster GetKitchenByCode(string code, int companyid)
        {
            try
            {
                KitchenMaster kitch = _unitofwork.KitchenMasterRepository.Get(g => g.KitchenCode == code && g.CompanyID == companyid).FirstOrDefault();
                if (kitch != null)
                {
                    return kitch;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveKitchen(KitchenMaster loc)
        {
            try
            {
                _unitofwork. KitchenMasterRepository.Insert(loc);
                int res = _unitofwork.Save();
                if (res == 1)
                {                 
                  _unitofwork.Save();
                }
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<KitchenMaster> GetKitchens(Int32 compid)
        {
            try
            {
                IEnumerable<KitchenMaster> sysKitchen = _unitofwork.KitchenMasterRepository.Get(g => g.CompanyID == compid).OrderBy(g => g.KitchenCode);
                if (sysKitchen != null)
                {
                    return sysKitchen;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public KitchenMaster GetKitchenById(long id)
        {
            try
            {
                //  SysLocation syslocation = _unitofwork.LocationRepository.Get(g => g.SysLocationID == id).FirstOrDefault();
                // Changed by hasanka 
                KitchenMaster kitchenMaster = _unitofwork.KitchenMasterRepository.GetById(id);
                if (kitchenMaster != null)
                {
                    return kitchenMaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateKitchen(KitchenMaster loc)
        {
            try
            {
                _unitofwork.KitchenMasterRepository.Update(loc);
                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
