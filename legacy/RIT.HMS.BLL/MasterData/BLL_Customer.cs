using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Logs;
using RIT.HMS.Domain.Loyalty;
using RIT.HMS.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace RIT.HMS.BLL.MasterData
{
   public class BLL_Customer
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Customer()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Customer(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<Customer> GetCustomers(int companyid)
        {
            try
            {
                IEnumerable<Customer> customer = _unitofwork.CustomerRepository.Get(c => c.IsDelete == false && c.CompanyID== companyid).OrderBy(g => g.CustomerCode);
                if (customer != null)
                {
                    return customer;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IEnumerable<Customer> GetByLocId(long id)
        {
            try
            {
                IEnumerable<Customer> customer = _unitofwork.CustomerRepository.Get(g => g.IsDelete == false && g.LocationId == id).OrderBy(g => g.CustomerCode);
                if (customer != null)
                {
                    return customer;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<Customer> GetActiveCustomers()
        {
            try
            {
                IEnumerable<Customer> customer = _unitofwork.CustomerRepository.Get(g => g.IsDelete == false).OrderBy(g => g.CustomerCode);
                if (customer != null)
                {
                    return customer;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Customer GetCustomerById(long id)
        {
            try
            {
                Customer customer = _unitofwork.CustomerRepository.Get(g => g.CustomerID == id).FirstOrDefault();
                if (customer != null)
                {
                    return customer;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public Customer GetCustomerByCode(string code,Int32 companyid)
        {
            try
            {
                Customer customer = _unitofwork.CustomerRepository.Get(g => g.CustomerCode == code && g.CompanyID== companyid).FirstOrDefault();
                if (customer != null)
                {
                    return customer;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<Customer> GetCustomerByLocId(long locid)
        {
            try
            {
                IEnumerable<Customer> cus = _unitofwork.CustomerRepository.Get(e => e.LocationId == locid)
                                                                                        .OrderBy(k => k.CustomerName);


                if (cus != null)
                {
                    return cus;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<Customer> GetCustomerDetailReport(long locid, long customerid)
        {
            try
            {
                List<Customer> customer = new List<Customer>();

                if (locid != 0 && customerid != 0)
                {
                    customer = _unitofwork.CustomerRepository.Get(r => r.CustomerID == customerid && r.LocationId == locid).
                                         OrderBy(c => c.CustomerName).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid != 0 && customerid == 0)
                {
                    customer = _unitofwork.CustomerRepository.Get(r => r.LocationId == locid).
                                         OrderBy(c => c.CustomerName).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid == 0 && customerid != 0)
                {
                    customer = _unitofwork.CustomerRepository.Get(r => r.CustomerID == customerid).
                                         OrderBy(c => c.CustomerName).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid == 0 && customerid == 0)
                {
                    customer = _unitofwork.CustomerRepository.Get().
                                         OrderBy(c => c.CustomerName).OrderBy(d => d.LocationId).ToList();
                }

                if (customer != null)
                {
                    return customer;
                }

                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Customer FindByCode(string code,Int32 companyid)
        {
            var customer = _unitofwork.CustomerRepository.Get(c => c.CustomerCode == code && c.CompanyID== companyid).FirstOrDefault();
            if (customer != null)
            {
                return customer;
            }
            else
            {
                return null;
            }


        }

        public int SaveCustomer(Customer cus)
        {
            try
            {
                // cus.Race = 1;
                cus.SpouseDateOfBirth = DateTime.Now;
                cus.CustomerSince = DateTime.Now;
                _unitofwork.CustomerRepository.Insert(cus);
              //  return  _unitofwork.Save();
                if (cus.IsActiveForLoyalty)
                {
                    LoyaltyCustomer loyaltycustomer = new LoyaltyCustomer();
                    var cardinfo = _unitofwork.LoyaltyCardIssueDetailReporsitory.Get(c => c.CardNo == cus.CardNumber
                                                                                          ).SingleOrDefault();

                    var cardgendetail = _unitofwork.loyaltyCardGenerationDetailReporsitory.Get(c => c.CardNo == cus.CardNumber).SingleOrDefault();
                    var cardgenheader = _unitofwork.loyaltyCardGenerationHeaderReporsitory.GetById(cardgendetail.LoyaltyCardGenerationHeaderID);

                    loyaltycustomer.CardNo = cus.CardNumber;
                    loyaltycustomer.CustomerId = cus.CustomerID;
                    loyaltycustomer.CardMasterId = cardgenheader.CardMasterId;
                    loyaltycustomer.NameOnCard = cus.NameOnCard;
                    loyaltycustomer.ExpiryDate = cus.ExpiryDate;

                    loyaltycustomer.CardIssued = true;
                    loyaltycustomer.IssuedOn = DateTime.Now;
                    loyaltycustomer.LocationID = cus.LocationId;
                    loyaltycustomer.GroupOfCompanyID = cus.GroupOfCompanyID;
                    loyaltycustomer.CreatedDate = DateTime.Now;
                    loyaltycustomer.CreatedUser = cus.CreatedUser;
                    loyaltycustomer.DataTransfer = cus.DataTransfer;
                    loyaltycustomer.LastUpdatedLocId = cus.LocationId;
                    loyaltycustomer.IsCardIssued = 1;
                    loyaltycustomer.ModifiedDate = DateTime.Now;
                    loyaltycustomer.AcitiveDate = DateTime.Now;
                    loyaltycustomer.ExpiryDate = DateTime.Now;
                    loyaltycustomer.RenewedOn = DateTime.Now;
                    loyaltycustomer.CompanyId = cus.CompanyID;

                    _unitofwork.LoyaltyCustomerReporsitory.Insert(loyaltycustomer);

                    var existscardno = _unitofwork.LoyaltyCardIssueDetailReporsitory.Get(c => c.CardNo == cus.CardNumber).SingleOrDefault();
                    existscardno.IsIssued = true;
                    _unitofwork.LoyaltyCardIssueDetailReporsitory.Update(existscardno);
                   
                }

                return _unitofwork.Save();


            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateCustomer(Customer cus)
        {
            try
            {
                _unitofwork.CreateTransaction();
                _unitofwork.CustomerRepository.Update(cus);
                int x = _unitofwork.Save();
                if (cus.IsActiveForLoyalty)
                {
                    var cardinfo = _unitofwork.LoyaltyCardIssueDetailReporsitory.Get(c => c.CardNo == cus.CardNumber
                                                                                         //  && c.ToLocationID == cus.LocationId
                                                                                         ).SingleOrDefault();

                    var exists = _unitofwork.LoyaltyCustomerReporsitory.Get(l => l.CardNo == cus.CardNumber &&
                                                                                l.CustomerId == cus.CustomerID
                                                                                ).SingleOrDefault();


                    var cardgendetail = _unitofwork.loyaltyCardGenerationDetailReporsitory.Get(c => c.CardNo == cus.CardNumber).SingleOrDefault();
                    var cardgenheader = _unitofwork.loyaltyCardGenerationHeaderReporsitory.GetById(cardgendetail.LoyaltyCardGenerationHeaderID);


                    LoyaltyCustomer loyaltycustomer = new LoyaltyCustomer();
                    loyaltycustomer.CardNo = cus.CardNumber;
                    loyaltycustomer.CustomerId = cus.CustomerID;
                    loyaltycustomer.CardMasterId = cardgenheader.CardMasterId;
                    loyaltycustomer.NameOnCard = cus.NameOnCard;
                    loyaltycustomer.ExpiryDate = cus.ExpiryDate;
                    loyaltycustomer.CardIssued = true;
                    loyaltycustomer.IssuedOn = DateTime.Now;
                    loyaltycustomer.LocationID = cus.LocationId;
                    loyaltycustomer.GroupOfCompanyID = cus.GroupOfCompanyID;
                    loyaltycustomer.CreatedDate = DateTime.Now;
                    loyaltycustomer.CreatedUser = cus.CreatedUser;
                    loyaltycustomer.DataTransfer = cus.DataTransfer;
                    loyaltycustomer.LastUpdatedLocId = cus.LocationId;
                    loyaltycustomer.IsCardIssued = 1;
                    loyaltycustomer.ModifiedDate = DateTime.Now;
                    loyaltycustomer.AcitiveDate = DateTime.Now;
                    loyaltycustomer.ExpiryDate = DateTime.Now;
                    loyaltycustomer.RenewedOn = DateTime.Now;
                    loyaltycustomer.CompanyId = cus.CompanyID;

                    if (exists != null)
                    {

                        exists.CardNo = cus.CardNumber;
                        exists.CustomerId = cus.CustomerID;
                        exists.CardMasterId = cardgenheader.CardMasterId;
                        exists.NameOnCard = cus.NameOnCard;
                        exists.ExpiryDate = cus.ExpiryDate;
                        exists.CardIssued = true;
                        exists.IssuedOn = DateTime.Now;
                        exists.LocationID = cus.LocationId;
                        exists.GroupOfCompanyID = cus.GroupOfCompanyID;
                        exists.CreatedDate = DateTime.Now;
                        exists.CreatedUser = cus.CreatedUser;
                        exists.DataTransfer = cus.DataTransfer;
                        exists.LastUpdatedLocId = cus.LocationId;
                        exists.IsCardIssued = 1;
                        exists.ModifiedDate = DateTime.Now;
                        exists.AcitiveDate = DateTime.Now;
                        exists.ExpiryDate = DateTime.Now;
                        exists.RenewedOn = DateTime.Now;
                        _unitofwork.LoyaltyCustomerReporsitory.Update(exists);

                        cardinfo.IsIssued = true;
                        _unitofwork.LoyaltyCardIssueDetailReporsitory.Update(cardinfo);
                    }
                    else
                        _unitofwork.LoyaltyCustomerReporsitory.Insert(loyaltycustomer);
                        int x1 = _unitofwork.Save();
                }


                LOGCustomer logcustomer = new LOGCustomer();
                var mapped = Common.HMSExtensions.MatchAndMap(cus, logcustomer);
                mapped.SourceId = cus.CustomerID;
                _unitofwork.LOGCustomer.Insert(mapped);

                int x2 = _unitofwork.Save();
                _unitofwork.Commit();

                return x2;

            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return 0;
            }
        }

        public IEnumerable<CustomerCategory> GetActiveCustomerCategories(Int32 compid)
        {
            try
            {
                IEnumerable<CustomerCategory> cuscat = _unitofwork.CustomerCategoryRepository.Get(g => g.IsDelete == false 
                                                        && g.CompanyID==compid).OrderBy(g => g.CustomerCategoryCode);
                if (cuscat != null)
                {
                    return cuscat;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<CustomerCategory> GetByCusCatId(long id)
        {
            try
            {
                IEnumerable<CustomerCategory> catermood = _unitofwork.CustomerCategoryRepository.Get(g => g.IsActive == true).OrderBy(g => g.CustomerCategoryCode);
                if (catermood != null)
                {
                    return catermood;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveCustomerDiscounts(ICollection<CustomerDiscount> customerdiscountlist)
        {
            try
            {
                foreach (var cs in customerdiscountlist)
                {
                    if (_unitofwork.CustomerDiscountRepository.Get(c => c.CustomerId == cs.CustomerId).Any())
                    {
                        _unitofwork.CustomerDiscountRepository.DeleteRange(_unitofwork.CustomerDiscountRepository.Get(c => c.CustomerId == cs.CustomerId));
                    }

                }

                _unitofwork.CustomerDiscountRepository.BulkInsert(customerdiscountlist);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public object GetCustomerPricesByCustomerId(long customerid)
        {
            var customerprices = (from cd in _unitofwork.CustomerDiscountRepository.Get()
                                  join c in _unitofwork.CustomerRepository.Get() on cd.CustomerId equals c.CustomerID
                                  join p in _unitofwork.ProductRepository.Get() on cd.ProductId equals p.ProductId
                                  join ps in _unitofwork.ProductServingUnitRepository.Get() on cd.ServingUnitId equals ps.ProductServingUnitId
                                  where cd.CustomerId == customerid
                                  orderby p.ProductName
                                  select new
                                  {
                                      CustomerDiscountId = cd.CustomerDiscountId,
                                      CustomerId = cd.CustomerId,
                                      CustomerCode = c.CustomerCode,
                                      CustomerName = c.CustomerName,
                                      ProductId = cd.ProductId,
                                      ProductName = p.ProductName,
                                      ProductCode = p.ProductCode,
                                      ServingUnitId = cd.ServingUnitId,
                                      ServingUnit = ps.ServingUnit,
                                      DiscountAmount=cd.DiscountAmount,
                                      DiscountPercentage=cd.CreditDiscountPercentage,
                                      CustomerSellPrice=cd.CustomerSellPrice,
                                      CreditDiscountAmount=cd.CreditDiscountAmount,
                                      CreditDiscountPercentage=cd.CreditDiscountPercentage,
                                      DateFrom=cd.DateFrom,
                                      DateTo=cd.DateTo,
                                      IsActive=cd.IsActive
                                  }
                                 ).ToList();

            //List<CustomerPricesViewModel> customerpriceslist = new List<CustomerPricesViewModel>();
            //if (customerprices != null || customerprices.Count!=0)
            //{
            //    foreach (var cp in customerprices)
            //    {
            //        CustomerPricesViewModel customerprice = new CustomerPricesViewModel();
            //        customerprice.CustomerDiscountId = cp.CustomerDiscountId;
            //        customerprice.CustomerId = cp.CustomerId;
            //        customerprice.CustomerCode = cp.CustomerCode;
            //        customerprice.CustomerName = cp.CustomerName;
            //        customerprice.ProductId = cp.ProductId;
            //        customerprice.ProductName = cp.ProductName;
            //        customerprice.ProductCode = cp.ProductCode;
            //        customerprice.ServingUnitId = cp.ServingUnitId;
            //        customerprice.ServingUnit = cp.ServingUnit;
            //        customerprice.DiscountAmount = cp.DiscountAmount;
            //        customerprice.DiscountPercentage = cp.CreditDiscountPercentage;
            //        customerprice.CustomerSellPrice = cp.CustomerSellPrice;
            //        customerprice.CreditDiscountAmount = cp.CreditDiscountAmount;
            //        customerprice.CreditDiscountPercentage = cp.CreditDiscountPercentage;
            //        customerprice.DateFrom = cp.DateFrom;
            //        customerprice.DateTo = cp.DateTo;
            //        customerprice.IsActive = cp.IsActive;
            //        customerpriceslist.Add(customerprice);
            //    }

            //}

            return customerprices == null ? null : customerprices;

        }

        public List<CustomerPricesViewModel> GetCustomerPrices()
        {
            var customerprices = (from cd in _unitofwork.CustomerDiscountRepository.Get()
                                  join c in _unitofwork.CustomerRepository.Get() on cd.CustomerId equals c.CustomerID
                                  join p in _unitofwork.ProductRepository.Get() on cd.ProductId equals p.ProductId
                                  join ps in _unitofwork.ProductServingUnitRepository.Get() on cd.ServingUnitId equals ps.ProductServingUnitId
                               //   where cd.CustomerId == customerid
                                  orderby p.ProductName
                                  select new
                                  {
                                      CustomerDiscountId = cd.CustomerDiscountId,
                                      CustomerId = cd.CustomerId,
                                      CustomerCode = c.CustomerCode,
                                      CustomerName = c.CustomerName,
                                      ProductId = cd.ProductId,
                                      ProductName = p.ProductName,
                                      ProductCode = p.ProductCode,
                                      ServingUnitId = cd.ServingUnitId,
                                      ServingUnit = ps.ServingUnit,
                                      DiscountAmount = cd.DiscountAmount,
                                      DiscountPercentage = cd.CreditDiscountPercentage,
                                      CustomerSellPrice = cd.CustomerSellPrice,
                                      CreditDiscountAmount = cd.CreditDiscountAmount,
                                      CreditDiscountPercentage = cd.CreditDiscountPercentage,
                                      DateFrom = cd.DateFrom,
                                      DateTo = cd.DateTo,
                                      IsActive = cd.IsActive
                                  }
                                 ).ToList();

            List<CustomerPricesViewModel> customerpriceslist = new List<CustomerPricesViewModel>();
            if (customerprices != null || customerprices.Count != 0)
            {
                foreach (var cp in customerprices)
                {
                    CustomerPricesViewModel customerprice = new CustomerPricesViewModel();
                    customerprice.CustomerDiscountId = cp.CustomerDiscountId;
                    customerprice.CustomerId = cp.CustomerId;
                    customerprice.CustomerCode = cp.CustomerCode;
                    customerprice.CustomerName = cp.CustomerName;
                    customerprice.ProductId = cp.ProductId;
                    customerprice.ProductName = cp.ProductName;
                    customerprice.ProductCode = cp.ProductCode;
                    customerprice.ServingUnitId = cp.ServingUnitId;
                    customerprice.ServingUnit = cp.ServingUnit;
                    customerprice.DiscountAmount = cp.DiscountAmount;
                    customerprice.DiscountPercentage = cp.CreditDiscountPercentage;
                    customerprice.CustomerSellPrice = cp.CustomerSellPrice;
                    customerprice.CreditDiscountAmount = cp.CreditDiscountAmount;
                    customerprice.CreditDiscountPercentage = cp.CreditDiscountPercentage;
                    customerprice.DateFrom = cp.DateFrom;
                    customerprice.DateTo = cp.DateTo;
                    customerprice.IsActive = cp.IsActive;
                    customerpriceslist.Add(customerprice);
                }

            }

            return customerpriceslist == null ? null : customerpriceslist;

        }

        public int RemoveCustomerDiscounts(long customerid, long productid)
        {
            _unitofwork.CustomerDiscountRepository.DeleteRange(_unitofwork.CustomerDiscountRepository.Get(c => c.CustomerId == customerid && c.ProductId == productid));
            var res = _unitofwork.Save();
            return res;
        }

        public List<ReferenceType> ReferanceTypes(string lookuptype)
        {
            var reftypes= _unitofwork.ReferenceTypeRepository.Get(r=>r.LookupType== lookuptype).ToList();
            return reftypes == null ? null : reftypes; 
        }
       
    }
}
