using RIT.HMS.Data;
using RIT.HMS.Domain.Promotions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.Promotions
{
    public class BLL_BundleItemPromotion
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_BundleItemPromotion()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_BundleItemPromotion(string connection)
        {
           // string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _unitofwork = new UnitOfWork(connection);
        }

        public int SubmitPromotion(List<InvBundleItemPrice> promotions)
        {
            _unitofwork.CreateTransaction();
            {
                try
                {

                    int companyid = promotions.FirstOrDefault().CompanyID;
                    int maxgroupid = 0;
                    var a = _unitofwork.InvBundleItemPriceRepository.Get(p=>p.CompanyID == companyid).ToList();
                    if (a.Count != 0)
                    {
                        maxgroupid = _unitofwork.InvBundleItemPriceRepository.Get().Max(g => g.GroupId);
                    }

                    maxgroupid += 1;
                    promotions.ForEach(p=>{ p.GroupId = maxgroupid; });

                    if (promotions.First().SinglePriceForAllItems)
                    {
                        promotions.First().SellingPrice = promotions.First().BundleSellingPrice;
                    }


                   _unitofwork.InvBundleItemPriceRepository.BulkInsert(promotions);
                    var res = _unitofwork.Save();
                    _unitofwork.Commit();
                    return res;
                }
                catch (Exception e)
                {
                    _unitofwork.Rollback();
                    return 0;
                }
            }


        }

        public int SubmitComboPackPromotion(List<InvComboPackBundleItemPrice> promotions)
        {
            _unitofwork.CreateTransaction();
            {
                try
                {

                    int companyid = promotions.FirstOrDefault().CompanyID;
                    int maxgroupid = 0;
                    var a = _unitofwork.InvBundleItemPriceRepositorys.Get(p => p.CompanyID == companyid).ToList();
                    if (a.Count != 0)
                    {
                        maxgroupid = _unitofwork.InvBundleItemPriceRepositorys.Get().Max(g => g.GroupId);
                    }

                    maxgroupid += 1;
                    promotions.ForEach(p => { p.GroupId = maxgroupid; });

                    List<InvComboPackBundleItemPrice> bundlelist = new List<InvComboPackBundleItemPrice>();
                    foreach (var i in promotions)
                    { 
                            i.DiscountValue = i.BundleSellingPrice;
                           
                    }

                    //foreach (var val in promotions)
                    //{
                    //    if (promotions.First().SinglePriceForAllItems)
                    //    {
                    //        if (promotions.First().SellingPrice == promotions.First().SellingPrice)
                    //        {
                    //            promotions.First().SellingPrice = promotions.First().BundleSellingPrice;
                    //        }
                    //    }
                    //}
                    string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
                    BLL_ItemToItemPromotion _itemToItemPromotionService = new BLL_ItemToItemPromotion(cn);

                    var ProID = 0;
                    var res = 0; 
                    foreach (var X in promotions)
                    {

                        var PromotionID = _itemToItemPromotionService.GetComboPackBundleItemById(X.PromotionMasterId);
                        if (PromotionID == null)
                        {
                            ProID = 0;
                        }
                        else
                        {
                            ProID = 1;
                        }
                    }

                    if (ProID == 0)
                    {
                        _unitofwork.InvBundleItemPriceRepositorys.BulkInsert(promotions);
                         res = _unitofwork.Save();
                        _unitofwork.Commit();
                        return res;
                    }
                    else
                    {
                        return res;
                    }
                }
                catch (Exception e)
                {
                    _unitofwork.Rollback();
                    return 0;
                }
            }


        }

        public List<InvBundleItemPrice> GetBundlePromotions(int promotionmasterid,int companyid)
        {
            var items = (
                          from p in _unitofwork.InvBundleItemPriceRepository.GetAsNoTracking(p => p.PromotionMasterId == promotionmasterid && p.CompanyID == companyid)
                          join pm in _unitofwork.PromotionRepository.GetAsNoTracking(pm => pm.CompanyID == companyid && pm.InvPromotionMasterID == promotionmasterid) on p.PromotionMasterId equals pm.InvPromotionMasterID
                          join pp in _unitofwork.ProductRepository.GetAsNoTracking(pp => pp.CompanyID == companyid).Select(
                                                                        pp => new
                                                                        {
                                                                            pp.ProductId,
                                                                            pp.ProductCode,
                                                                            pp.ProductName,
                                                                            pp.PurchasingUnit,
                                                                        }
                                                                        ) on p.ProductId equals pp.ProductId
                          join u in _unitofwork.UnitOfMeasureRepository.GetAsNoTracking(u => u.CompanyID == companyid) on pp.PurchasingUnit equals u.UnitOfMeasureId
                          join s in _unitofwork.ProductServingUnitRepository.GetAsNoTracking(s => s.CompanyID == companyid) on p.ServingUnitId equals s.ProductServingUnitId
                          select new
                          {
                              ProductId = p.ProductId,
                              ProductCode = pp.ProductCode,
                              ProductName = pp.ProductName,
                              Cost = 0,
                              Selling = p.SellingPrice,
                              Qty = p.Quantity,                           
                              UOM = u.UnitOfMeasureName,                            
                              ServingUnit = s.ServingUnit,
                              ServingUnitId = p.ServingUnitId,
                              PromotionMasterId = p.PromotionMasterId,
                              PromotionTypeId = pm.PromotionTypeID,
                              PromotionName = pm.PromotionName,                          
                              GroupId = p.GroupId,
                              InvBundleItemPriceId=p.InvBundleItemPriceId,
                              SinglePriceForAllItems=p.SinglePriceForAllItems,
                              DifferentPricesForItems=p.DifferentPricesForItems,
                              BundleName=p.BundleName,

                          }
                      );

            List<InvBundleItemPrice> bundlelist = new List<InvBundleItemPrice>();
            foreach (var i in items)
            {
                InvBundleItemPrice bundle = new InvBundleItemPrice();
                bundle.InvBundleItemPriceId = i.InvBundleItemPriceId;
                bundle.PromotionMasterId = i.PromotionMasterId;
                bundle.SinglePriceForAllItems = i.SinglePriceForAllItems;
                bundle.DifferentPricesForItems = i.DifferentPricesForItems;
                bundle.ProductId = i.ProductId;
                bundle.ProductCode = i.ProductCode;
                bundle.ProductName = i.ProductName;
                bundle.ServingUnitId = i.ServingUnitId;
                bundle.ServingUnit = i.ServingUnit;
                bundle.Quantity = i.Qty;
                bundle.SellingPrice = i.Selling;
                bundle.GroupId = i.GroupId;
                if (i.BundleName == null)
                {
                    bundle.BundleName = "";
                }
                else
                {
                    bundle.BundleName = i.BundleName;
                }
               
                bundlelist.Add(bundle);

            }
           
            return bundlelist ?? null;
        }


        public int RemoveBundleItemGroup(InvBundleItemPrice bundleitempromotion)
        {
            _unitofwork.CreateTransaction();
            try
            {
                var itemgroup = _unitofwork.InvBundleItemPriceRepository.Get(x => x.GroupId == bundleitempromotion.GroupId
                && x.PromotionMasterId == bundleitempromotion.PromotionMasterId && x.CompanyID == bundleitempromotion.CompanyID 
                && x.ProductId==bundleitempromotion.ProductId);

                _unitofwork.InvBundleItemPriceRepository.DeleteRange(itemgroup);
                _unitofwork.Save();

                _unitofwork.Commit();
                return 1;
            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return 0;
            }

        }

    }
}
