//using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
//using HospitalityManagement.Models.ViewModels;
using System.Drawing.Imaging;
using RIT.HMS.BLL.TransactionData;
using RIT.HMS.Domain.ViewModels;
using RIT.HMS.BLL.MasterData;

namespace HospitalityManagement.Service
{
    public class DashBoardService
    {

        //    ApplicationDbContext context = new ApplicationDbContext();
        BLL_PurchaseOrder _bllorder = new BLL_PurchaseOrder();
        BLL_ProductionNote _bllproductionnote = new BLL_ProductionNote();
        BLL_TransferNote _blltransfernote = new BLL_TransferNote();
        BLL_Product _bllproduct = new BLL_Product();
       
        public int GetPOCountToday(int loggeduserid)
        {
            try
            {              
                   DateTime date = DateTime.Today.Date;
                   var pos = _bllorder.GetAllPos().Where(p => DbFunctions.TruncateTime(p.CreatedDate) == date.Date).ToList();
             
                if (pos == null)
                {
                    return 0;
                }
                else
                {
                    return pos.Count() ;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int GetPOCountThisWeek(int loggeduserid)
        {
            try
            {

                DateTime fromdate = DateTime.Today.Date.AddDays(-7);
                DateTime todate = DateTime.Today.Date;
                var pos = _bllorder.GetAllPos().Where(p => p.IsTempPO == false
                                                              &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) >= fromdate.Date
                                                             &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) <= todate.Date
                                                             ).ToList();

                if (pos == null)
                {
                    return 0;
                }
                else
                {
                    return pos.Count();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public int GetProductionCountToday(int loggeduserid)
        {
            try
            {
                DateTime date = DateTime.Today.Date;
                var production = _bllproductionnote.GetActiveProductions().Where(p =>p.IsTempPN == false
                                                             &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) == date.Date
                                                            ).ToList();
                if (production == null)
                {
                    return 0;
                }
                else
                {
                    return production.Count();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int GetProductionCountThisWeek(int loggeduserid)
        {
            try
            {
                DateTime fromdate = DateTime.Today.Date.AddDays(-7);
                DateTime todate = DateTime.Today.Date;
                var production = _bllproductionnote.GetActiveProductions().Where(p => p.IsTempPN == false
                                                             &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) >= fromdate.Date
                                                             &&
                                                              DbFunctions.TruncateTime(p.CreatedDate) <= todate.Date
                                                            ).ToList();
                if (production == null)
                {
                    return 0;
                }
                else
                {
                    return production.Count();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public int GetTOGCountThisWeek(int loggeduserid)
        {
            try
            {
                DateTime fromdate = DateTime.Today.Date.AddDays(-7);
                DateTime todate = DateTime.Today.Date;
                var togs = _blltransfernote.GetAllTOGs().Where(p => p.IsTempTOG == false
                                                             &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) >=fromdate.Date
                                                             &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) <= todate.Date
                                                            ).ToList();
                if (togs == null)
                {
                    return 0;
                }
                else
                {
                    return togs.Count();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public int GetTOGCountToday(int loggeduserid)
        {
            try
            {
                DateTime date = DateTime.Today.Date;
                var togs = _blltransfernote.GetAllTOGs().Where(p => p.IsTempTOG == false
                                                             &&
                                                             DbFunctions.TruncateTime(p.CreatedDate) == date.Date
                                                            ).ToList();
                if (togs == null)
                {
                    return 0;
                }
                else
                {
                    return togs.Count();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public List<DashboardViewModel.Top10Productions> GetTop10Productions(long locationid)
        {
            try
            {

              
                var productions = (from p in _bllproductionnote.GetProductionNoteDetail()
                                   where p.ProductId != 0
                                  group p by p.ProductId into groupedtable
                                 
                                  select new {
                                      ProductId = groupedtable.Key,
                                      Qty= groupedtable.Sum(s=>s.ProductQty)

                                  }).ToList();
                var orderedlist = productions.OrderByDescending(p => p.Qty).ToList() ;
                var tolist = orderedlist.Take(10);

                //.OrderByDescending(k=>k.ProductQty).ToList();

                List < DashboardViewModel.Top10Productions > top10list = new List<DashboardViewModel.Top10Productions>();
              //  ProductService _porductService = new ProductService();
               

                foreach (var prd in tolist)
                {
                   
                    DashboardViewModel.Top10Productions newprd = new DashboardViewModel.Top10Productions();

                   var ext = _bllproduct.GetProductById(prd.ProductId);
                    if (ext != null)
                    {
                        newprd.ProductName = ext.ProductName;
                        newprd.ProductCount = prd.Qty;
                        top10list.Add(newprd);
                    }
                }

                return top10list;



            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}