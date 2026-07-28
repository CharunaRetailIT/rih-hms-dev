using HospitalityManagement.Models;
using HospitalityManagement.Models.Transactions;
using HospitalityManagement.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service.Transactions
{
    public class RequestNoteService
    {
        ApplicationDbContext context = new ApplicationDbContext();
        private readonly ProductService _prdService = new ProductService();
        public List<InterDepartment> GetActiveInterDepartments()
        {
            return context.InterDepartment.Where(i => i.IsActive == true).ToList();
        }

        public List<InterDepartment> GetInterDeptByLocId(long locid)
        {
            return context.InterDepartment.Where(i => i.IsActive == true && i.InterDeptLocId == locid).ToList();
        }

        public List<RequestNoteHeader> GetRequestNotesByLocIdDeptId(long locid, long deptid)
        {
            return context.RequestNoteHeader.Where(i => i.FromDepartmentId == deptid
                                                                            && i.FromLocationId == locid
                                                                            && i.IsTempRequest == false).ToList();
        }

        public RequestNoteHeader GetRequestNoteById(long id)
        {
            RequestNoteHeader requestnoteheader = new RequestNoteHeader();

            requestnoteheader = context.RequestNoteHeader.Where(i => i.RequestnoteHeaderId == id).FirstOrDefault();

            var detail = context.RequestNoteDetail.Where(d => d.RequestnoteHeaderId == id).OrderBy(i => i.LineNo).ToList();
            detail.ForEach(d => { d.ProductName = _prdService.GetProductById(d.ProductId).ProductName; });
            requestnoteheader.RequestDetail = detail;

            return requestnoteheader;
        }


        public List<RequestNoteAccptanceHeader> GetAcceptedRequestNotesByLocId(long locid)
        {
            return context.RequestNoteAccptanceHeader.Where(i => i.ToLocationId == locid).ToList();
        }
        public IEnumerable<RequestNoteHeader> GetTempRequestNotes()
        {
            return context.RequestNoteHeader.Where(i => i.IsTempRequest == true);
        }
        public List<RequestNoteViewModel> GetReceipesByRequestId(long requestid)
        {
            List<RequestNoteViewModel> vvm = new List<RequestNoteViewModel>();
            var reqdet = context.RequestNoteDetail.Where(r => r.RequestnoteHeaderId == requestid).ToList();
            var reqheader = context.RequestNoteHeader.Where(h => h.RequestnoteHeaderId == requestid).FirstOrDefault();

            foreach (var item in reqdet)
            {

                   var receipes = context.Receipe.Where(r => r.ProductId == item.ProductId).ToList();
                if (receipes.Count > 0)
                {
                    foreach (var receipe in receipes)
                    {
                        var vm = new RequestNoteViewModel();
                        vm.ProductId = item.ProductId;
                        vm.ProductName = _prdService.GetProductById(item.ProductId).ProductName;
                        vm.ProductQuantity = item.RequestQty;
                        vm.MaterialId = receipe.MaterialId;
                        vm.MaterialQuantity = receipe.Quantity * item.RequestQty;
                        vm.MaterialUOMName = _prdService.GetUOMById(_prdService.GetProductById(receipe.MaterialId).PurchasingUnit);
                        var dbmat = context.ProductStockMaster.Where(p => p.LocationId == reqheader.ToLocationId
                                                 && p.ProductId == receipe.MaterialId).FirstOrDefault();
                        vm.MaterialCostPrice = dbmat.CostPrice * item.RequestQty;
                        vm.MaterialSellingPrice = dbmat.SellingPrice * item.RequestQty;
                        vm.MaterialName = dbmat.ProductName;

                        vvm.Add(vm);
                    }
                }
                else
                {

                    var products = context.ProductStockMaster.Where(p => p.ProductId == item.ProductId && 
                                                                    p.LocationId == reqheader.ToLocationId);

                    foreach (var product in products)
                    {
                        var vm = new RequestNoteViewModel();
                        vm.ProductId = item.ProductId;
                        vm.ProductName = _prdService.GetProductById(item.ProductId).ProductName;
                        vm.ProductQuantity = item.RequestQty;
                        vm.MaterialId = product.ProductId;
                        vm.MaterialQuantity = item.RequestQty;
                        vm.MaterialUOMName = _prdService.GetUOMById(_prdService.GetProductById(product.ProductId).PurchasingUnit);

                        var dbmat = context.ProductStockMaster.Where(p => p.LocationId == reqheader.FromLocationId
                                                 && p.ProductId == product.ProductId).FirstOrDefault();

                        vm.MaterialCostPrice = dbmat.CostPrice * item.RequestQty;
                        vm.MaterialSellingPrice = dbmat.SellingPrice * item.RequestQty;
                        vm.MaterialName = dbmat.ProductName;

                        vvm.Add(vm);
                    }
                }
            }

            return vvm;
        }


        public bool SubmitRequest(RequestNoteHeader header)
        {

            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {

                    context.RequestNoteHeader.Add(header);

                    if (context.SaveChanges() == 1)
                    {

                        foreach (var detail in header.RequestDetail)
                        {
                            var stock = context.ProductStockMaster.Where(p => p.LocationId == header.ToLocationId
                                                                                && p.ProductId == detail.ProductId
                                                                        ).FirstOrDefault();

                            detail.RequestnoteHeaderId = header.RequestnoteHeaderId;
                            detail.AvgCost = stock.AvgCost;
                            detail.LineNo = header.RequestDetail.IndexOf(detail) + 1;
                            detail.UnitOfMeasureId = _prdService.GetProductById(detail.ProductId).PurchasingUnit;

                            context.RequestNoteDetail.Add(detail);

                        }

                        context.SaveChanges();
                        dbtransaction.Commit();
                        return true;



                    }
                    else
                    {
                        dbtransaction.Rollback();
                        return false;
                    }


                }
                catch (Exception ex)
                {
                    dbtransaction.Rollback();
                    return false;

                }
            }

        }

        public bool AcceptRequest(RequestNoteAccptanceHeader header)
        {

            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {
                    var dbheader = context.RequestNoteHeader.Where(h => h.DocumentNo == header.DocumentNo).FirstOrDefault();
                    header.ToLocationId = dbheader.ToLocationId;
                    header.ToDepartmentId = dbheader.ToDepartmentId;

                    context.RequestNoteAccptanceHeader.Add(header);

                    if (context.SaveChanges() == 1)
                    {

                        foreach (var accdetail in header.AcceptanceDetail)
                        {


                            accdetail.RequestNoteAccptanceHeaderId = header.RequestNoteAccptanceHeaderId;
                            accdetail.LineNo = header.AcceptanceDetail.IndexOf(accdetail) + 1;
                            accdetail.UnitOfMeasureId = _prdService.GetProductById(accdetail.ProductId).PurchasingUnit;
                            context.RequestNoteAcceptanceDetail.Add(accdetail);

                        }

                        context.SaveChanges();
                        dbtransaction.Commit();
                        return true;



                    }
                    else
                    {
                        dbtransaction.Rollback();
                        return false;
                    }


                }
                catch (Exception ex)
                {
                    dbtransaction.Rollback();
                    return false;

                }
            }

        }
        
        public long DelateRequestDetailById(long id)
        {
           
                long res = 0;
                try
                {

                   context.RequestNoteDetail.RemoveRange(context.RequestNoteDetail.Where(x => x.RequestnoteHeaderId == id));
                   return res = context.SaveChanges();


                }
                catch (Exception ex)
                {
                    return 0;
                }
            
        }

        public bool ModifyRequest(RequestNoteHeader header)
        {

            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {
                    DelateRequestDetailById(header.RequestnoteHeaderId);


                    var dbheader = context.RequestNoteHeader.Where(h => h.RequestnoteHeaderId == header.RequestnoteHeaderId).FirstOrDefault();

                    dbheader.DocumentNo = header.DocumentNo;
                    dbheader.DocumentDate = DateTime.Now;
                    dbheader.IsTempRequest = header.IsTempRequest;
                    dbheader.FromLocationId = header.FromLocationId;
                    dbheader.ToLocationId = header.ToLocationId;
                    dbheader.FromDepartmentId = header.FromDepartmentId;
                    dbheader.ToDepartmentId = header.ToDepartmentId;

                    //context.RequestNoteAccptanceHeader.Add(header);

                        context.SaveChanges();
                    

                        foreach (var detail in header.RequestDetail)
                        {
                            var stock = context.ProductStockMaster.Where(p => p.LocationId == header.ToLocationId
                                                                                && p.ProductId == detail.ProductId
                                                                        ).FirstOrDefault();

                            detail.RequestnoteHeaderId = header.RequestnoteHeaderId;
                            detail.AvgCost = stock.AvgCost;
                            detail.LineNo = header.RequestDetail.IndexOf(detail) + 1;
                            detail.UnitOfMeasureId = _prdService.GetProductById(detail.ProductId).PurchasingUnit;

                            context.RequestNoteDetail.Add(detail);

                        }

                        context.SaveChanges();
                        dbtransaction.Commit();
                        return true;


                }
                catch (Exception ex)
                {
                    dbtransaction.Rollback();
                    return false;

                }
            }

        }

    }
}