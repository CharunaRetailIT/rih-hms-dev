using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Logs;
using RIT.HMS.Domain.Promotions;
using RIT.HMS.Domain.ViewModels.Promotions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.Promotions
{
    public class BLL_Promotion
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Promotion()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Promotion(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public bool GetpromoByCode(string code,int companyid)
        {
            try
            {
                InvPromotionMaster promo = _unitofwork.PromotionRepository.Get(g => g.PromotionCode == code && g.CompanyID==companyid).FirstOrDefault();
                if (promo != null)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<InvPromotionType> GetPromoTypes(int companyid)
        {
            try
            {
                IEnumerable<InvPromotionType> promotype = _unitofwork.PromotionTypeRepository.Get(g => g.IsDelete == false && g.CompanyID==companyid).OrderBy(g => g.PromotionTypeCode);
                if (promotype != null)
                {
                    return promotype;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public IEnumerable<InvPromotionMaster> GetAllPromos(int companyid)
        {
            try
            {
                IEnumerable<InvPromotionMaster> promo = _unitofwork.PromotionRepository.Get(r => r.IsDelete == false 
                                                                                            && r.CompanyID==companyid).OrderBy(g => g.StartDate);
                if (promo != null)
                {
                    return promo;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public InvPromotionMaster GetPromoById(long id)
        {
            try
            {
                var pm = _unitofwork.PromotionRepository.Get().FirstOrDefault(g => g.InvPromotionMasterID == id);
                return pm ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public IEnumerable<InvPromoBusinessType> GetbusiById(long id)
        {
            try
            {
                IEnumerable<InvPromoBusinessType> busitype = _unitofwork.PromoBusinessTypeRepository.Get(p => p.InvPromoBusinessTypeID == id);
                return busitype ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public InvPromotionMaster GetActivePromosById(int companyid,int promomasterid)
        {
            try
            {
                InvPromotionMaster promo = _unitofwork.PromotionRepository.Get(g => g.IsDelete == false && g.CompanyID == companyid && g.InvPromotionMasterID==promomasterid).FirstOrDefault();
                if (promo != null)
                {
                    return promo;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<InvPromotionMaster> GetActivePromos(int companyid)
        {
            try
            {
                IEnumerable<InvPromotionMaster> promo = _unitofwork.PromotionRepository.Get(g => g.IsDelete == false && g.CompanyID==companyid).OrderBy(g => g.PromotionCode);
                if (promo != null)
                {
                    return promo;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public InvPromoLowestPriceWaveOff GetLowestPriceWaveOffByCode(string code,int companyid)
        {
            try
            {
                InvPromoLowestPriceWaveOff promo = _unitofwork.PromoLowestPriceWaveOffRepository.Get(g => g.LowestPriceWaveOffCode == code && g.CompanyId==companyid).FirstOrDefault();
                if (promo != null)
                {
                    return promo;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }



        public InvPromoLowestPriceWaveOff GetLowestPriceWaveOffById(long id)
        {
            try
            {
                InvPromoLowestPriceWaveOff promo = _unitofwork.PromoLowestPriceWaveOffRepository.Get(cm => cm.InvPromoLowestPriceWaveOffID == id).FirstOrDefault();
                if (promo != null)
                {
                    return promo;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public IEnumerable<CateringMood> GetActiveCateringMoods(int companyid)
        {
            try
            {
                IEnumerable<CateringMood> cuscat = _unitofwork.CateringMoodRepository.Get(g => g.IsActive == true 
                                                                && g.CompanyId==companyid).OrderBy(g => g.CateringMoodID).OrderBy(g => g.OrderSequence);
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


        public IEnumerable<InvPromoCustomerCategory> GetPromoCusById(long id)
        {
            try
            {
                IEnumerable<InvPromoCustomerCategory> cusdet = _unitofwork.PromoCustomerCategoryRepository.Get(p => p.InvPromotionMasterID == id).OrderBy(g => g.CustomerCategoryID);
                return cusdet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<InvPromoBusinessType> GetPromoBusinessTypeById(long id)
        {
            try
            {
                IEnumerable<InvPromoBusinessType> businesstypedet = _unitofwork.PromoBusinessTypeRepository.Get(p => p.InvPromotionMasterID == id).OrderBy(g => g.InvPromoBusinessTypeID);
                return businesstypedet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public bool SavePromotionMaster(List<InvPromotionMaster> promo,int ccc)
        {
           _unitofwork.CreateTransaction();

            try
            {

                _unitofwork.PromotionRepository.BulkInsert(promo);
                if (_unitofwork.Save() > 0)
                {
                    //foreach (var detail in promo.VMPromotionSchedular)
                    //{

                    //foreach (var i in promo.BusinessTypeID)
                    //{
                    //    InvPromoBusinessType bt = new InvPromoBusinessType();
                    //    bt.InvPromotionMasterID = promo.InvPromotionMasterID;
                    //    bt.CateringMoodID = Convert.ToInt64(i);
                    //    Int64 cid = Convert.ToInt64(i);
                    //    bt.CateringMoodName = _unitofwork.CateringMoodRepository.Get(c => c.CateringMoodID == cid).FirstOrDefault().CateringMoodName;
                    //    // bt.CateringMoodName = "";
                    //    bt.Remark = "";
                    //    bt.Status = true;
                    //    bt.CreatedUser = "";
                    //    bt.CreatedDate = DateTime.Now;
                    //    _unitofwork.PromoBusinessTypeRepository.Insert(bt);
                    //    //  context.SaveChanges();
                    //}


                    //foreach (var i in promo.CustomerGroupId)
                    //{
                    //    InvPromoCustomerCategory cus = new InvPromoCustomerCategory();
                    //    cus.InvPromotionMasterID = promo.InvPromotionMasterID;
                    //    cus.CustomerCategoryID = Convert.ToInt32(i);
                    //    cus.Remark = "";
                    //    cus.Status = true;
                    //    cus.CreatedUser = "";
                    //    cus.CreatedDate = DateTime.Now;
                    //    _unitofwork.PromoCustomerCategoryRepository.Insert(cus);
                    //    //  context.SaveChanges();
                    //}

                    //_unitofwork.Save();
                    //}

                    _unitofwork.Commit();
                }
                else
                {
                    _unitofwork.Rollback();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return false;

            }

           // return false;

        }


        public bool UpdatePromoMaster(InvPromotionMaster promo)
        {
            _unitofwork.CreateTransaction();
            {

                try
                {
                    
                    _unitofwork.PromotionRepository.Update(promo);

                    LOGInvPromotionMaster lgpromo = new LOGInvPromotionMaster();
                    var mapped = Common.HMSExtensions.MatchAndMap(promo,lgpromo);
                    mapped.SourceId = Convert.ToInt32(promo.InvPromotionMasterID);
                    _unitofwork.LOGInvPromotionMaster.Insert(mapped);
                    _unitofwork.Save();
                    _unitofwork.Commit();
                    return true;


                }
                catch (Exception ex)
                {

                    _unitofwork.Rollback();
                    return false;
                }
            }
        }

        public int SaveLowestPriceWave(InvPromoLowestPriceWaveOff promo)
        {
            try
            {
                _unitofwork.PromoLowestPriceWaveOffRepository.Insert(promo);
                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateLowestPriceWave(InvPromoLowestPriceWaveOff cm)
        {
            try
            {
                _unitofwork.PromoLowestPriceWaveOffRepository.Update(cm);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public List<VMPromotionItems> GetPromotionItems(int companyid)
        {

            var dbxy = (from p in _unitofwork.PromotionDetailsBuyXProductRepository.Get(p => p.CompanyID == companyid)
                        join prd in _unitofwork.ProductRepository.Get(prd => prd.CompanyID == companyid) on p.ProductId equals prd.ProductId
                        join pm in _unitofwork.PromotionRepository.Get(pm => pm.CompanyID == companyid) on p.InvPromotionMasterId equals pm.InvPromotionMasterID
                        join psu in _unitofwork.ProductServingUnitRepository.Get(psu=>psu.CompanyID==companyid) on p.ServingUnitId equals psu.ProductServingUnitId
                        select new 
                        {
                            PromotionName = pm.PromotionName,
                            PromotionCode = pm.PromotionCode,
                            ProductType = p.ProductType,
                            ProductId=p.ProductId,
                            ProductName = prd.ProductName,
                            ProductCode = prd.ProductCode,
                            ServingUnit = psu.ServingUnit,
                            Quantity = p.Qty,                            
                        }).ToList();

            List<VMPromotionItems> listpromotionitems = new List<VMPromotionItems>();
            foreach (var a in dbxy)
            {
                VMPromotionItems vmpromotionitem = new VMPromotionItems();
                vmpromotionitem.PromotionName = a.PromotionName;
                vmpromotionitem.PromotionCode = a.PromotionCode;

                if (a.ProductType == 1)
                {
                    vmpromotionitem.PromotionItemId =(int)a.ProductId;
                    vmpromotionitem.PromotionItem = a.ProductName;
                    vmpromotionitem.PromotionItemCode = a.ProductCode;
                    vmpromotionitem.PromotionItemServingUnit = a.ServingUnit;
                    vmpromotionitem.PromotionItemQty = a.Quantity;
                } else if (a.ProductType == 2)
                {
                    vmpromotionitem.FreeItemId = (int)a.ProductId;
                    vmpromotionitem.FreeItem = a.ProductName;
                    vmpromotionitem.FreeItemCode = a.ProductCode;
                    vmpromotionitem.FreeItemServingUnit = a.ServingUnit;
                    vmpromotionitem.FreeItemQty = a.Quantity;
                }

                listpromotionitems.Add(vmpromotionitem);
            }

            return listpromotionitems;
        }

        public bool GetpromoByGivenCodes(string code, int companyid)
        {
            try
            {
                InvPromotionMaster promo = _unitofwork.PromotionRepository.Get(g => g.PromotionCode == code && g.CompanyID == companyid).FirstOrDefault();
                if (promo != null)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
