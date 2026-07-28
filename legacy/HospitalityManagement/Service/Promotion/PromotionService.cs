using HospitalityManagement.Models;
using HospitalityManagement.Models.Promotions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalityManagement.Service.Promotions
{
   public class PromotionService
    {

        ApplicationDbContext context = new ApplicationDbContext();


        public InvPromotionMaster GetpromoByCode(string code)
        {
            try
            {
                InvPromotionMaster promo = context.InvPromotionMaster.Where(g => g.PromotionCode == code).FirstOrDefault();
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



        public int SavePromotion(InvPromotionMaster promo)
        {
            try
            {
                context.InvPromotionMaster.Add(promo);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
    }


        

        //   public int SubmitPromotion(List<ProductStockMasterViewModel> promotions)
        //{
          
        //    using (var dbtransaction = context.Database.BeginTransaction())
        //    {
        //        try
        //        {

        //            foreach (var p in promotions)
        //            {

        //                InvPromoBillValueBasedGetYProduct promotion = new InvPromoBillValueBasedGetYProduct();
        //                promotion.InvPromotionMasterId = p.InvPromotionMasterId;
        //                promotion.ProductId = p.ProductId;
        //                promotion.ProductCode = p.ProductCode;
        //                promotion.ProductName = p.ProductName;
        //                promotion.ServingUnitId = p.ServingUnitId;
        //                promotion.BuyUnitOfMeasureId = p.UOMId;
        //                promotion.Rate = p.SellingPrice;
        //                promotion.Qty = p.Quantity;
        //                promotion.ValueFrom = p.ValueFrom;
        //                promotion.ValueTo = p.ValueTo;
        //                if (p.DiscountType == "Amt")
        //                {
        //                    promotion.DiscountAmount = p.DiscountAmt;
        //                    promotion.DiscountPercentage = 0;

        //                }
        //                else if (p.DiscountType == "Prc")
        //                {
        //                    promotion.DiscountAmount = 0;
        //                    promotion.DiscountPercentage = p.DiscountAmt;

        //                }
        //                else if (p.DiscountType == "N")
        //                {
        //                    promotion.DiscountAmount = 0;
        //                    promotion.DiscountPercentage = 0;
        //                }
        //                promotion.ProductType = p.PromotionItemType;
        //                promotion.GroupOfCompanyID = 1;
        //                promotion.CompanyID = 1;
        //                promotion.LocationId = 1;
        //                promotion.CreatedUser = "";
        //                promotion.CreatedDate = DateTime.Now;
        //                promotion.ModifiedDate = DateTime.Now;
        //                promotion.ModifiedUser = "";
        //                promotion.DataTransfer = 0;
        //                promotion.Points = 0;
                       

        //                context.InvPromoBillValueBasedGetYProduct.Add(promotion);

        //            }

        //            var res = context.SaveChanges();
        //            dbtransaction.Commit();
        //            return res;
        //        }
        //        catch (Exception e)
        //        {
        //            dbtransaction.Rollback();
        //            return 0;
        //        }
        //    }


        //}










        public IEnumerable<InvPromotionType> GetPromoTypes()
        {
            try
            {
                IEnumerable<InvPromotionType> promotype = context.InvPromotionType.Where(g => g.IsDelete == false).OrderBy(g => g.PromotionTypeCode);
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


        public IEnumerable<InvPromotionMaster> GetAllPromos()
        {
            try
            {
                IEnumerable<InvPromotionMaster> promo = context.InvPromotionMaster.Where(r => r.IsDelete == false).OrderBy(g => g.StartDate);
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



        public bool SavePromotionMaster(InvPromotionMaster promo)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {
                   
                    context.InvPromotionMaster.Add(promo);


                    if (context.SaveChanges() == 1)
                    {

                       // int idx = 1;
                        foreach (var detail in promo.VMPromotionSchedular)
                        {
                            

                            if (detail.Day == 1)
                            {
                                detail.FromTime = promo.SundayStartTime;
                                detail.ToTime = promo.SundayEndTime;

                            }
                            if (detail.Day == 2)
                            {
                                detail.FromTime = promo.MondayStartTime;
                                detail.ToTime = promo.MondayEndTime;
                            }
                            if (detail.Day == 3)
                            {
                                detail.FromTime = promo.TuesdayStartTime;
                                detail.ToTime = promo.TuesdayEndTime;
                            }
                            if (detail.Day == 4)
                            {
                                detail.FromTime = promo.WednesdayStartTime;
                                detail.ToTime = promo.WednesdayEndTime;
                            }
                            if (detail.Day == 5)
                            {
                                detail.FromTime = promo.ThuresdayStartTime;
                                detail.ToTime = promo.ThuresdayEndTime;
                            }
                            if (detail.Day == 6)
                            {
                                detail.FromTime = promo.FridayStartTime;
                                detail.ToTime = promo.FridayEndTime;
                            }
                            if (detail.Day == 7)
                            {
                                detail.FromTime = promo.SaturdayStartTime;
                                detail.ToTime = promo.SaturdayEndTime;
                            }

                           // var businesstype = context.InvPromoBusinessType.Where(d => d.InvPromotionMasterID == promo.InvPromotionMasterID).FirstOrDefault();

                            //var customercat = context.InvPromoCustomerCategory.Where(d => d.InvPromotionMasterID == promo.InvPromotionMasterID).FirstOrDefault();


                            //idx += 1;
                           // context.InvPromotionMaster.Add(promo);



                            foreach (var i in promo.BusinessTypeID)
                            {
                                InvPromoBusinessType bt = new InvPromoBusinessType();
                                bt.InvPromotionMasterID = promo.InvPromotionMasterID;
                                bt.CateringMoodID = Convert.ToInt64(i);
                                Int64 cid = Convert.ToInt64(i);
                                bt.CateringMoodName = context.CateringMood.Where(c => c.CateringMoodID == cid).FirstOrDefault().CateringMoodName;
                               // bt.CateringMoodName = "";
                                bt.Remark = "";
                                bt.Status = true;
                                bt.CreatedUser = "";
                                bt.CreatedDate = DateTime.Now;
                                context.InvPromoBusinessType.Add(bt);
                              //  context.SaveChanges();
                            }


                            foreach (var i in promo.CustomerGroupId)
                            {
                                InvPromoCustomerCategory cus = new InvPromoCustomerCategory();
                                cus.InvPromotionMasterID = promo.InvPromotionMasterID;
                                cus.CustomerCategoryID = Convert.ToInt32(i);
                                cus.Remark = "";
                                cus.Status = true;
                                cus.CreatedUser = "";
                                cus.CreatedDate = DateTime.Now;
                                context.InvPromoCustomerCategory.Add(cus);
                              //  context.SaveChanges();
                            }

                            context.SaveChanges();
                        }

                        dbtransaction.Commit();
                    }
                    else
                    {
                        dbtransaction.Rollback();
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    dbtransaction.Rollback();
                    return false;

                }
            }
        }


        public InvPromotionMaster GetPromoById(long id)
        {
            try
            {
                var pm = context.InvPromotionMaster.FirstOrDefault(g => g.InvPromotionMasterID == id);
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
                IEnumerable<InvPromoBusinessType> togdet = context.InvPromoBusinessType.Where(p => p.InvPromoBusinessTypeID == id);
                return togdet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public IEnumerable<InvPromotionMaster> GetActivePromos()
        {
            try
            {
                IEnumerable<InvPromotionMaster> promo = context.InvPromotionMaster.Where(g => g.IsDelete == false ).OrderBy(g => g.PromotionCode);
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

        

        public int SaveLowestPriceWave(InvPromoLowestPriceWaveOff promo)
        {
            try
            {
                context.InvPromoLowestPriceWaveOff.Add(promo);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public InvPromoLowestPriceWaveOff GetLowestPriceWaveOffByCode(string code)
        {
            try
            {
                InvPromoLowestPriceWaveOff promo = context.InvPromoLowestPriceWaveOff.Where(g => g.LowestPriceWaveOffCode == code).FirstOrDefault();
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



        public IEnumerable<CateringMood> GetActiveCateringMoods()
        {
            try
            {
                IEnumerable<CateringMood> cuscat = context.CateringMood.Where(g =>  g.IsActive == true).OrderBy(g => g.CateringMoodID);
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
                IEnumerable<InvPromoCustomerCategory> cusdet = context.InvPromoCustomerCategory.Where(p => p.InvPromotionMasterID == id).OrderBy(g => g.CustomerCategoryID);
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
                IEnumerable<InvPromoBusinessType> businesstypedet = context.InvPromoBusinessType.Where(p => p.InvPromotionMasterID == id).OrderBy(g => g.InvPromoBusinessTypeID);
                return businesstypedet ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }



    }
}
