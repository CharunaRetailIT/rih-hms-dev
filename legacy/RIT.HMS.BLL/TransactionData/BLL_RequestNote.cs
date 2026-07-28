using RIT.HMS.BLL.MasterData;
using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Transactions;
using RIT.HMS.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.TransactionData
{
    public class BLL_RequestNote
    {
        private readonly UnitOfWork _unitofwork;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_UnitConversion __bllunitconversion;

        public BLL_RequestNote()
        {
            _unitofwork = new UnitOfWork();
            _bllproduct = new BLL_Product();
            __bllunitconversion = new BLL_UnitConversion();
        }
        public BLL_RequestNote(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
            _bllproduct = new BLL_Product(connection);
            __bllunitconversion = new BLL_UnitConversion(connection);
        }
        public List<InterDepartment> GetActiveInterDepartments(Int32  compid)
        {
            return _unitofwork.InterDepartmentRepository.Get(i => i.IsActive == true && i.CompanyID==compid).ToList();
        }

        public List<InterDepartment> GetInterDeptByLocId(long locid, int companyid)
        {
            return _unitofwork.InterDepartmentRepository.Get(i => i.IsActive == true && i.InterDeptLocId == locid && i.CompanyID== companyid).ToList();
        }

        public List<RequestNoteHeader> GetRequestNotesByLocIdDeptId(long locid, long deptid,int companyid)
        {
            return _unitofwork.RequestNoteHeaderRepository.Get(i => i.FromDepartmentId == deptid
                                                                            && i.FromLocationId == locid
                                                                            && i.IsTempRequest == false
                                                                            && i.IsApproved == false
                                                                            && i.DocumentStatus !=5
                                                                            && i.CompanyId==companyid).ToList();
        }

        public List<RequestNoteHeader> GetActiveRequestNotesByLocIdDeptId(long locid, long deptid, int companyid)
        {
            return _unitofwork.RequestNoteHeaderRepository.Get(i => i.ToDepartmentId == deptid
                                                                            && i.ToLocationId == locid
                                                                            && i.IsTempRequest == false
                                                                            && i.IsApproved == false
                                                                            && i.DocumentStatus != 4 && i.DocumentStatus != 7
                                                                            && i.CompanyId == companyid).ToList();
        }

        public List<RequestNoteHeader> GetActiveRequestNotesByLocIdDeptIdDate(long locid, int companyid)
        {
            return _unitofwork.RequestNoteHeaderRepository.Get(i =>   i.FromLocationId == locid
                                                                            && i.IsTempRequest == false
                                                                            && i.IsApproved == false
                                                                            && i.DocumentStatus != 4 && i.DocumentStatus != 7
                                                                            && i.CompanyId == companyid).ToList();
        }

        public RequestNoteHeader GetRequestNoteById(long id)
        {
            RequestNoteHeader requestnoteheader = new RequestNoteHeader();

            requestnoteheader = _unitofwork.RequestNoteHeaderRepository.Get(i => i.RequestnoteHeaderId == id).FirstOrDefault();

            var detail = _unitofwork.RequestNoteDetailRepository.Get(d => d.RequestnoteHeaderId == id)
                .Join(_unitofwork.ProductRepository.Get(p => p.IsActive == true && p.IsDelete == false), 
                    d => d.ProductId,
                    p => p.ProductId,
                    (d, p) => new
                    {
                        RequestNoteDetail = d,
                        ProductName = p.ProductName,
                        ProductCode = p.ProductCode
                    })
                .OrderBy(i => i.RequestNoteDetail.LineNo).ToList();
            detail.ForEach(d => 
            {
                d.RequestNoteDetail.ProductName = _bllproduct.GetProductById(d.RequestNoteDetail.ProductId).ProductName;
            d.RequestNoteDetail.ProductCode = _bllproduct.GetProductById(d.RequestNoteDetail.ProductId).ProductCode;

            });
            requestnoteheader.RequestDetail = detail.Select(d => d.RequestNoteDetail).ToList();

            return requestnoteheader;
        }

        public RequestNoteHeader GetDocumentID(string documentNo, long RequestnoteHeaderId)
        {
            RequestNoteHeader requestnoteheader = new RequestNoteHeader();

            requestnoteheader = _unitofwork.RequestNoteHeaderRepository.Get(i => i.DocumentNo == documentNo  && i.RequestnoteHeaderId == RequestnoteHeaderId).FirstOrDefault();
            return requestnoteheader;
        }

        public RequestNoteHeader GetRequestNoteByDocNummber(string docnum)
        {
            RequestNoteHeader requestnoteheader = new RequestNoteHeader();

            requestnoteheader = _unitofwork.RequestNoteHeaderRepository.Get(i => i.DocumentNo == docnum).FirstOrDefault();

            var detail = _unitofwork.RequestNoteDetailRepository.Get(d => d.RequestnoteHeaderId == requestnoteheader.RequestnoteHeaderId).OrderBy(i => i.LineNo).ToList();
            detail.ForEach(d => { d.ProductName = _bllproduct.GetProductById(d.ProductId).ProductName; });
            requestnoteheader.RequestDetail = detail;

            return requestnoteheader;
        }

        public RequestNoteHeader GetRequestNoteByDocNummberAndPrdId(string docnum,long productid)
        {
            RequestNoteHeader requestnoteheader = new RequestNoteHeader();

            requestnoteheader = _unitofwork.RequestNoteHeaderRepository.Get(i => i.DocumentNo == docnum).FirstOrDefault();

            var detail = _unitofwork.RequestNoteDetailRepository.Get(d => d.RequestnoteHeaderId == requestnoteheader.RequestnoteHeaderId).OrderBy(i => i.LineNo).ToList();
            detail.ForEach(d => { d.ProductName = _bllproduct.GetProductById(d.ProductId).ProductName; });
            requestnoteheader.RequestDetail = detail;

            return requestnoteheader;
        }

        public RequestNoteAccptanceHeader GetBaseDocNo(long Id)
        {
            try
            {
                RequestNoteAccptanceHeader basedocno = _unitofwork.RequestNoteAccptanceHeaderRepository.Get(d => d.RequestNoteAccptanceHeaderId == Id).FirstOrDefault();
                return basedocno == null ? null : basedocno;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<RequestNoteAccptanceHeader> GetAcceptedRequestNotesByLocId(long locid,int companyid)
        {
            return _unitofwork.RequestNoteAccptanceHeaderRepository.Get(i => i.FromLocationId == locid && i.CompanyId== companyid).ToList();
        }
        public List<RequestNoteHeader> GetAllActiveRequestNotes()
        {
            return _unitofwork.RequestNoteHeaderRepository.Get(i => i.DocumentStatus != 2 && i.DocumentStatus != 3).ToList();
        }
        public List<RequestNoteAccptanceHeader> GetAcceptedRequestNotesByLocIdForProduction(long locid,int companyid)
        {
            return _unitofwork.RequestNoteAccptanceHeaderRepository.Get(i => i.FromLocationId == locid && i.CompanyId== companyid).Where(p => p.RequestType == "Production" && p.IsProductionComplete==false).ToList();
        }
        public List<RequestNoteAccptanceHeader> GetAcceptedRequestNotesByLocIdForTOG(long locid,long Tolocationid,int companyid)
        {
            List<RequestNoteAccptanceHeader> x = new List<RequestNoteAccptanceHeader>();
            x = _unitofwork.RequestNoteAccptanceHeaderRepository.Get(i => i.FromLocationId == Tolocationid && i.ToLocationId == locid && i.CompanyId == companyid).Where(p => p.RequestType == "TOG" || p.RequestType == "Request Note based PO").ToList();
            if (x.Count > 0)
                return x;
            else
                return x = new List<RequestNoteAccptanceHeader>();
        }
        public List<RequestNoteAccptanceHeader> GetAcceptedRequestNotesByLocIdForPO(long locid,int companyid)
        {
            return _unitofwork.RequestNoteAccptanceHeaderRepository.Get(i => i.FromLocationId == locid && i.CompanyId == companyid).Where(p=>p.RequestType=="PO").ToList();
        }
        public IEnumerable<RequestNoteHeader> GetTempRequestNotes(int companyid , int LocationID)
        {
            return _unitofwork.RequestNoteHeaderRepository.Get(i => (i.DocumentStatus == 1 || i.DocumentStatus == 2 || i.DocumentStatus==5 || i.DocumentStatus==4) && i.CompanyId== companyid && i.FromLocationId == LocationID).OrderBy(i => i.DocumentNo);
        }
        public List<RequestNoteViewModel> GetReceipesByRequestId(long requestid,int companyid)
        {
            List<RequestNoteViewModel> vvm = new List<RequestNoteViewModel>();

            var reqheader = _unitofwork.RequestNoteHeaderRepository.Get(h => h.RequestnoteHeaderId == requestid).FirstOrDefault();

            var reqdet = new List<RequestNoteDetail>(); ;
            //if (reqheader.IsApproved)
            //{
            //    reqdet = _unitofwork.RequestNoteDetailRepository.Get(r => r.RequestnoteHeaderId == requestid).ToList();
            //}
            //else
            //{
                reqdet = (from req in _unitofwork.RequestNoteDetailRepository.Get(r => r.RequestnoteHeaderId == requestid)
                              join product in _unitofwork.ProductRepository.Get(p => p.IsActive == true && p.IsDelete == false)
                              on req.ProductId equals product.ProductId
                              select req).ToList();
           // }
            

            foreach (var item in reqdet)
            {



                ProductStockMaster productstockmaster = _unitofwork.ProductStockMasterRepository.Get(r => r.ProductId == item.ProductId && r.LocationId == reqheader.FromLocationId).
                                                                 OrderBy(c => c.LocationId).FirstOrDefault();
                if (productstockmaster != null)
                {
                    item.SIH = productstockmaster.Stock;
                }





                var receipes = _unitofwork.ReceipeRepository.Get(r => r.ProductId == item.ProductId
                && r.ProductServingUnitId == item.ServingUnitId && r.CompanyID == companyid).ToList();

                var happyrecipe = receipes.Where(r => r.ProductQty.Equals(item.RequestQty)).ToList();

                if (happyrecipe.Count == 0)
                {
                    happyrecipe = receipes.Where(r => r.ProductQty.Equals(1)).ToList();
                }

                foreach (var receipe in happyrecipe)
                {
                    var vm = new RequestNoteViewModel
                    {
                        ProductId = item.ProductId,
                        ProductName = _bllproduct.GetProductById(item.ProductId).ProductName,
                        ProductQuantity = item.RequestQty,
                        MaterialId = receipe.MaterialId,
                        ProductCode =  _bllproduct.GetProductById(item.ProductId).ProductCode
                    };

                    var wp = _bllproduct.GetProductById(receipe.MaterialId).WeightPerUnit;
                    decimal suval = wp != 0 ? __bllunitconversion.GetConversionById(wp).FirstOrDefault().SubUnitValue : 1;

                    vm.MaterialQuantity = (receipe.Quantity * item.RequestQty) / suval;
                    vm.MaterialUOMId = _bllproduct.GetProductById(receipe.MaterialId).PurchasingUnit;
                    vm.MaterialUOMName = _bllproduct.GetUOMById(_bllproduct.GetProductById(receipe.MaterialId).PurchasingUnit);

                    var dbmat = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == reqheader.ToLocationId
                                                             && p.ProductId == receipe.MaterialId).FirstOrDefault();

                    vm.MaterialCostPrice = (receipe.CostPrice * item.RequestQty);
                    vm.MaterialSellingPrice = dbmat.SellingPrice * item.RequestQty;
                    vm.MaterialName = dbmat.ProductName;
                    vm.Remark = reqheader.Remark;
                    vm.DocumentStatus = reqheader.DocumentStatus;
                    vm.RequestedBy = item.RequestedBy;
                    vm.ServingUnitId = item.ServingUnitId;
                    vm.ServingUnit = item.ServingUnit;
                    vm.RequestType = reqheader.RequestType;
                    vm.SIH = item.SIH;
                    


                        // Check for duplicates before adding
                        if (!vvm.Any(x => x.ProductId == vm.ProductId && x.MaterialId == vm.MaterialId && x.ProductQuantity == vm.ProductQuantity))
                    {
                        vvm.Add(vm);
                    }
                }

                if (receipes.Count == 0)
                {
                    var products = _unitofwork.ProductStockMasterRepository.Get(p => p.ProductId == item.ProductId &&
                                                                            p.LocationId == reqheader.FromLocationId && p.CompanyID == companyid && p.CostPrice > 0).ToList();

                    foreach (var product in products)
                    {
                        var vm = new RequestNoteViewModel
                        {
                            ProductId = item.ProductId,
                            ProductCode = product.ProductCode,
                            ProductName = _bllproduct.GetProductById(item.ProductId).ProductName,
                            ProductQuantity = item.RequestQty,
                            MaterialId = product.ProductId,
                            MaterialQuantity = item.RequestQty,
                            MaterialUOMId = _bllproduct.GetProductById(product.ProductId).PurchasingUnit,
                            MaterialUOMName = _bllproduct.GetUOMById(_bllproduct.GetProductById(product.ProductId).PurchasingUnit),
                            MaterialCostPrice = item.CostPrice,
                            MaterialSellingPrice = item.SellingPrice,
                            MaterialName = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == reqheader.FromLocationId
                                                                     && p.ProductId == product.ProductId && p.CompanyID == companyid).FirstOrDefault().ProductName,
                            Remark = reqheader.Remark,
                            DocumentStatus = reqheader.DocumentStatus,
                            RequestedBy = item.RequestedBy,
                            ServingUnitId = item.ServingUnitId,
                            ServingUnit = item.ServingUnit,
                            RequestType = reqheader.RequestType,
                            DocumentNo = reqheader.DocumentNo,
                            CreateDate = reqheader.DocumentDate.ToString(),
                            CompanyId = reqheader.CompanyId,
                             SIH = item.SIH,
                             CostPrice = item.CostPrice
                        };

                        // Check for duplicates before adding
                        if (!vvm.Any(x => x.ProductId == vm.ProductId && x.MaterialId == vm.MaterialId && x.ProductQuantity == vm.ProductQuantity))
                        {
                            vvm.Add(vm);
                        }
                    }
                }
            }

            return vvm;


        }

        public RequestNoteHeader GetPendingRequest(long RequestnoteHeaderId)
        {
            try
            {
                RequestNoteHeader RNH = _unitofwork.RequestNoteHeaderRepository.Get(e => e.RequestnoteHeaderId == RequestnoteHeaderId).FirstOrDefault();
                if (RNH != null)
                {
                    return RNH;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<RequestNoteViewModel> GetReceipesByPendingRequestId(string DocumentNo, int companyid)
        {
            List<RequestNoteViewModel> vvm = new List<RequestNoteViewModel>();
            var reqheader = _unitofwork.RequestNoteHeaderRepository.Get(h => h.DocumentNo == DocumentNo).FirstOrDefault();
            var reqdet = _unitofwork.RequestNoteDetailRepository.Get(r => r.RequestnoteHeaderId == reqheader.RequestnoteHeaderId).ToList();


            foreach (var item in reqdet)
            {
                var receipes = _unitofwork.ReceipeRepository.Get(r => r.ProductId == item.ProductId
                && r.ProductServingUnitId == item.ServingUnitId && r.CompanyID == companyid
                ).ToList();
                if (receipes.Count > 0)
                {
                    var happyrecipe = receipes.Where(r => r.ProductQty.Equals(item.RequestQty));
                    foreach (var receipe in happyrecipe)
                    {
                        var vm = new RequestNoteViewModel();
                        vm.ProductId = item.ProductId;
                        vm.ProductName = _bllproduct.GetProductById(item.ProductId).ProductName;
                        vm.ProductQuantity = item.RequestQty;
                        vm.MaterialId = receipe.MaterialId;

                        var wp = _bllproduct.GetProductById(receipe.MaterialId).WeightPerUnit;
                        decimal suval = 0;
                        if (wp != 0)
                        {
                            suval = __bllunitconversion.GetConversionById(wp).FirstOrDefault().SubUnitValue;
                        }
                        else
                        {
                            suval = 1;
                        }

                        vm.MaterialQuantity = (receipe.Quantity * item.RequestQty) / suval;
                        vm.MaterialUOMId = _bllproduct.GetProductById(receipe.MaterialId).PurchasingUnit;
                        vm.MaterialUOMName = _bllproduct.GetUOMById(_bllproduct.GetProductById(receipe.MaterialId).PurchasingUnit);
                        var dbmat = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == reqheader.ToLocationId
                                                 && p.ProductId == receipe.MaterialId).FirstOrDefault();

                        // vm.MaterialCostPrice = (dbmat.CostPrice * item.RequestQty);
                        vm.MaterialCostPrice = (receipe.CostPrice * item.RequestQty);
                        vm.MaterialSellingPrice = dbmat.SellingPrice * item.RequestQty;
                        vm.MaterialName = dbmat.ProductName;
                        vm.Remark = reqheader.Remark;
                        vm.DocumentStatus = reqheader.DocumentStatus;
                        vm.RequestedBy = item.RequestedBy;
                        vm.ServingUnitId = item.ServingUnitId;
                        vm.ServingUnit = item.ServingUnit;
                        vm.RequestType = reqheader.RequestType;
                        vvm.Add(vm);
                    }

                    if (happyrecipe.Count() == 0)
                    {
                        var happyrecipe1 = receipes.Where(r => r.ProductQty.Equals(1));
                        foreach (var receipe in happyrecipe1)
                        {
                            var vm = new RequestNoteViewModel();
                            vm.ProductId = item.ProductId;
                            vm.ProductName = _bllproduct.GetProductById(item.ProductId).ProductName;
                            vm.ProductQuantity = item.RequestQty;
                            vm.MaterialId = receipe.MaterialId;

                            var wp = _bllproduct.GetProductById(receipe.MaterialId).WeightPerUnit;
                            decimal suval = 0;
                            if (wp != 0)
                            {
                                suval = __bllunitconversion.GetConversionById(wp).FirstOrDefault().SubUnitValue;
                            }
                            else
                            {
                                suval = 1;
                            }

                            vm.MaterialQuantity = (receipe.Quantity * item.RequestQty) / suval;


                            // vm.MaterialQuantity = receipe.Quantity * item.RequestQty;


                            vm.MaterialUOMId = _bllproduct.GetProductById(receipe.MaterialId).PurchasingUnit;
                            vm.MaterialUOMName = _bllproduct.GetUOMById(_bllproduct.GetProductById(receipe.MaterialId).PurchasingUnit);
                            var dbmat = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == reqheader.ToLocationId
                                                     && p.ProductId == receipe.MaterialId && p.CompanyID == companyid).FirstOrDefault();
                            //vm.MaterialCostPrice = dbmat.CostPrice * item.RequestQty;
                            vm.MaterialCostPrice = (receipe.CostPrice * item.RequestQty);
                            vm.MaterialSellingPrice = dbmat.SellingPrice * item.RequestQty;
                            vm.MaterialName = dbmat.ProductName;
                            vm.Remark = reqheader.Remark;
                            vm.DocumentStatus = reqheader.DocumentStatus;
                            vm.RequestedBy = item.RequestedBy;
                            vm.ServingUnitId = item.ServingUnitId;
                            vm.ServingUnit = item.ServingUnit;
                            vm.RequestType = reqheader.RequestType;
                            vvm.Add(vm);
                        }
                    }
                }
                else
                {
                    var products = _unitofwork.ProductStockMasterRepository.Get(p => p.ProductId == item.ProductId &&
                                                                    p.LocationId == reqheader.ToLocationId && p.CompanyID == companyid && p.CostPrice > 0);

                    foreach (var product in products)
                    {
                        var vm = new RequestNoteViewModel();
                        vm.ProductId = item.ProductId;
                        vm.ProductCode = product.ProductCode;
                        vm.ProductName = _bllproduct.GetProductById(item.ProductId).ProductName;
                        vm.ProductQuantity = item.RequestQty;
                        vm.MaterialId = product.ProductId;
                        vm.MaterialQuantity = item.RequestQty;
                        vm.MaterialUOMId = _bllproduct.GetProductById(product.ProductId).PurchasingUnit;
                        vm.MaterialUOMName = _bllproduct.GetUOMById(_bllproduct.GetProductById(product.ProductId).PurchasingUnit);

                        var dbmat = _unitofwork.ProductStockMasterRepository.Get(p => p.LocationId == reqheader.FromLocationId
                                                 && p.ProductId == product.ProductId && p.CompanyID == companyid).FirstOrDefault();

                        vm.MaterialCostPrice = item.CostPrice;//dbmat.CostPrice;// * item.RequestQty;
                        vm.MaterialSellingPrice = item.SellingPrice;//dbmat.SellingPrice;// * item.RequestQty;
                        vm.MaterialName = dbmat.ProductName;
                        vm.Remark = reqheader.Remark;
                        vm.DocumentStatus = reqheader.DocumentStatus;
                        vm.RequestedBy = item.RequestedBy;
                        vm.ServingUnitId = item.ServingUnitId;
                        vm.ServingUnit = item.ServingUnit;
                        vm.RequestType = reqheader.RequestType;
                        vvm.Add(vm);
                    }
                }
            }

            return vvm;
        }


        public List<RequestNoteHeader> GetRequestPOHO(string documentNo,int CompanyId)
        {
            List<RequestNoteHeader> vvm = new List<RequestNoteHeader>();

            var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                      .Get(h => h.DocumentNo == documentNo.Trim() && h.DocumentStatus == 2)
                      .FirstOrDefault();

            return vvm;
        }
        public  RequestNoteHeader GetRequestPOHeaderIDs( long RequestnoteHeaderId,  string documentNo, int CompanyId)
        {
     
            var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                      .Get(h => h.DocumentNo == documentNo.Trim() 
                          && h.RequestnoteHeaderId == RequestnoteHeaderId
                      && h.DocumentStatus == 2)
                      .FirstOrDefault();

            return reqheaderPO;
        }

        public DataTable GetAllSuppliersByDocumentNo(List<RequestNoteParameter> mylist)
        {
            try
            {
                DataTable dtQueryResult = new DataTable();
                string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();

                using (SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings[cn].ConnectionString))
                {
                    sqlConn.Open();

                    // Base query with a parameter placeholder
                    string query = @"SELECT DISTINCT RND.SupplierID
                             FROM [dbo].[RequestNoteDetails] AS RND
                             INNER JOIN [dbo].[RequestNoteHeaders] AS RNH ON RND.RequestnoteHeaderId = RNH.RequestnoteHeaderId
                             WHERE RNH.IsApproved = 1 AND RND.RequestnoteHeaderId IN ({0})";

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = sqlConn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 0;

                    // Construct parameterized query
                    var idParameterList = new List<string>();
                    var parameters = new List<SqlParameter>();
                    var index = 0;

                    foreach (RequestNoteParameter myobj in mylist)
                    {
                        var paramName = "@idParamRequestnoteHeaderId" + index;
                        idParameterList.Add(paramName);
                        parameters.Add(new SqlParameter(paramName, myobj.RequestNoteHeaderID));
                        index++;
                    }

                    // Use string.Join to include the correct number of parameters
                    var formattedQuery = string.Format(query, string.Join(",", idParameterList));
                    cmd.CommandText = formattedQuery;
                    cmd.Parameters.AddRange(parameters.ToArray());

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtQueryResult);
                }
                return dtQueryResult;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                return null;
            }
        }


        public DataTable GetAllPODataBySupplier(List<RequestNoteParameter> mylist, int supplierID)//pasing list of parameters
        {
            try
            {

                DataTable dtQueryResult = new DataTable();
                List<RequestNoteParameter> ListQueryResult = new List<RequestNoteParameter>();
                string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
                SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings[cn].ConnectionString);
                if (sqlConn.State != ConnectionState.Open)
                {
                    sqlConn.Close();
                    sqlConn.Open();
                }

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = sqlConn;
                //  cmd.CommandText = sqlQuery.ToString();
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                var query = @" Select A.DocumentNo, A.FromLocationID,A.ToLocationID, B.SupplierID,B.ProductID,SUM(B.RequestQty) as OrderQty,sum(B.CostPrice * B.RequestQty)GrossAmount,B.SellingPrice,B.CostPrice,PSM.StockCode,A.RequestnoteHeaderId From RequestNoteHeaders A
                                Inner join RequestNoteDetails B on A.RequestnoteHeaderId = B.RequestnoteHeaderId
                                  inner join Products P on B.ProductId =  P.ProductId 
                                CROSS APPLY
								(

								Select top 1 * from ProductStockMasters PS
								where  B.ProductId = PS.ProductId And A.ToLocationId = PS.LocationId  
								)PSM


                                Where  P.IsActive=1 and P.IsDelete=0  and A.IsApproved = 1 And A.RequestnoteHeaderId IN ({0}) And B.SupplierID =" + supplierID + @"
                                group by A.DocumentNo, A.FromLocationID,A.ToLocationID, B.SupplierID,B.ProductID,B.SellingPrice,B.CostPrice,PSM.StockCode,A.RequestnoteHeaderId";
                var idParameterList = new List<string>(); var index = 0;
                var idParameterList2 = new List<string>();

                foreach (RequestNoteParameter myobj in mylist)
                {
                    var paramName1 = "@idParamReqNotHedID" + index;
                    cmd.Parameters.AddWithValue(paramName1, myobj.RequestNoteHeaderID); idParameterList.Add(paramName1);

                    //var paramName2 = "@idParamLoc" + index;
                    //cmd.Parameters.AddWithValue(paramName2, myobj.locationID); idParameterList2.Add(paramName2);

                    index++;
                }
                cmd.CommandText = String.Format(query, string.Join(",", idParameterList));
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtQueryResult);
                if (sqlConn.State == ConnectionState.Open) sqlConn.Close();
                return dtQueryResult;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<RequestNoteDetail> GetRequestPODt(int fromlocationID,long TolocationID,string documentNo, int CompanyId,int supplierID)
        {
            List<RequestNoteDetail> vvm = new List<RequestNoteDetail>();
            List<RequestNoteDetail> ReqDetList = new List<RequestNoteDetail>();



            var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                      .Get(h => h.DocumentNo == documentNo.Trim() && h.DocumentStatus == 2 && h.IsPoTransfer == false)
                      .FirstOrDefault();


            //var reqdetailPO = _unitofwork.RequestNoteDetailRepository
            //    .Get(d => d.RequestnoteHeaderId == reqheaderPO.RequestnoteHeaderId ).ToList();





            var supplier = _unitofwork.SupplierProductRepository.Get(s => s.SupplierId == supplierID && s.CompanyID == CompanyId).ToList();
 

            foreach (var supp in supplier)
            {


                var reqdetailPO = _unitofwork.RequestNoteDetailRepository
                .Get(d => d.ProductId == supp.ProductId && d.IsPoTransfer == false && d.RequestnoteHeaderId == reqheaderPO.RequestnoteHeaderId).ToList();

                if (reqdetailPO!=null && reqdetailPO.Count>0)
                {
                    
                    foreach (var reqdetails in reqdetailPO)
                    {


                        RequestNoteDetail rnd = new RequestNoteDetail();
                       // var supplier = _unitofwork.SupplierProductRepository.Get(s => s.ProductId == reqdetails.ProductId && s.SupplierId == supplierID).FirstOrDefault();
                        if (supplier!=null)
                        {
                            var StockCode = _unitofwork.ProductStockMasterRepository.Get(psm => psm.ProductId == reqdetails.ProductId).FirstOrDefault();

                            rnd.ProductId = Convert.ToInt32(reqdetails.ProductId);
                            rnd.StockCode = StockCode.StockCode;
                            //vm.CostPrice = reqdetails.CostPrice;
                            //vm.SellingPrice = reqdetails.SellingPrice;
                            rnd.RequestQty = reqdetails.RequestQty;
                            rnd.GrossAmount = reqdetails.CostPrice * reqdetails.RequestQty;
                            rnd.LocationId = fromlocationID;//reqheaderPO.FromLocationId;
                            rnd.SupplierId = supplierID;
                            rnd.ToLocationID = Convert.ToInt32(TolocationID);// reqheaderPO.ToLocationId;
                            rnd.CostPrice = reqdetails.CostPrice;
                            rnd.SellingPrice = reqdetails.SellingPrice;


                            vvm.Add(rnd);
                        }
                    }
                }


            }

            return vvm;

        }

        public List<SupplierProduct>GetSuppliersTable(string documentNo,int CompanyId)
        {
            List<SupplierProduct> SupplierProcuts = new List<SupplierProduct>();

            var reqheaderPO1 = _unitofwork.RequestNoteHeaderRepository
             .Get(h => h.DocumentNo == documentNo.Trim() && h.DocumentStatus == 2 && h.IsPoTransfer == false)
             .FirstOrDefault();

            var reqdetailPO1 = _unitofwork.RequestNoteDetailRepository
                 .Get(d => d.RequestnoteHeaderId == reqheaderPO1.RequestnoteHeaderId)
                 .ToList();
            foreach (var item in reqdetailPO1)
            {
                var supplierID = _unitofwork.SupplierProductRepository
                    .Get(s => s.ProductId == item.ProductId)
                    .FirstOrDefault();

                if (supplierID != null)
                {
                    var vm1 = new SupplierProduct(); // Create a new instance for each iteration
                    vm1.SupplierId = supplierID.SupplierId;
                    SupplierProcuts.Add(vm1);
                }
            }

            return SupplierProcuts;
        }
        public List<SupplierProduct> GetRequestPOSupplier(string documentNo, int CompanyId)
        {
            
            List<Mearge_PO_Header> vvm1 = new List<Mearge_PO_Header>();
            List<SupplierProduct> vvm2 = new List<SupplierProduct>();
        
            var reqheaderPO1 = _unitofwork.RequestNoteHeaderRepository
                  .Get(h => h.DocumentNo == documentNo.Trim() && h.DocumentStatus == 2 && h.IsPoTransfer == false)
                  .ToList();


            foreach (var item in reqheaderPO1)
            {
                var vm1 = new SupplierProduct();

                var reqdetailPO1 = _unitofwork.RequestNoteDetailRepository
                    .Get(d => d.RequestnoteHeaderId == item.RequestnoteHeaderId)
                    .ToList();


                foreach (var item1 in reqdetailPO1)
                {
                    var vm = new Mearge_PO_Header();
                    var supplierID = _unitofwork.SupplierProductRepository
                        .Get(s => s.ProductId == item1.ProductId)
                        .FirstOrDefault();

                    var StockCode = _unitofwork.ProductStockMasterRepository
                        .Get(psm => psm.ProductId == supplierID.ProductId && psm.LocationId == item.ToLocationId)
                        .FirstOrDefault();



                    vm1.SupplierId = supplierID.SupplierId;
                    vm1.StockCode = StockCode.StockCode;
                    vm1.ProductId = Convert.ToInt32(item1.ProductId);
                    vm1.RequestQty = item1.RequestQty;
                    decimal GAmount = item1.CostPrice * item1.RequestQty;
                    vm1.GrossAmount += GAmount;
                    vm1.CostPrice = item1.CostPrice;
                    vm1.SellingPrice = item1.SellingPrice;
                    vvm1.Add(vm);

                    // Create SupplierProduct instance for vvm2
                    vm1.LocationId = item.FromLocationId;
                    vm1.ToLocationID = item.ToLocationId;
                    vm1.ReqDocumentNo = item.DocumentNo;
                    vm1.ReqFromLocation = Convert.ToInt32(item.FromLocationId);
                    vm1.RequestnoteHeaderId = Convert.ToInt32(item.RequestnoteHeaderId);
                }


                // Add vm1 to vvm2 (moved inside the outer loop)
                vvm2.Add(vm1);
            }

            return vvm2;
            //foreach (var reqdetails in reqdetailPO)
            //{
            //    var vm = new SupplierProduct();
            //    var supplierID = _unitofwork.SupplierProductRepository.Get(s => s.ProductId == reqdetails.ProductId).FirstOrDefault();

            //    var StockCode = _unitofwork.ProductStockMasterRepository.Get(psm => psm.ProductId == supplierID.ProductId && psm.LocationId == reqheaderPO.ToLocationId).FirstOrDefault();

            //    vm.ProductId = Convert.ToInt32(reqdetails.ProductId);
            //    vm.StockCode = StockCode.StockCode;
            //    //vm.CostPrice = reqdetails.CostPrice;
            //    //vm.SellingPrice = reqdetails.SellingPrice;
            //    vm.RequestQty = reqdetails.RequestQty;
            //    vm.GrossAmount = reqdetails.CostPrice * reqdetails.RequestQty;
            //    vm.LocationId = reqheaderPO.FromLocationId;
            //    vm.SupplierId = supplierID.SupplierId;
            //    vm.ToLocationID = reqheaderPO.ToLocationId;
            //    vm.CostPrice = reqdetails.CostPrice;
            //    vm.SellingPrice = reqdetails.SellingPrice;
            //    vm.ReqDocumentNo = reqheaderPO.DocumentNo;
            //    vm.ReqFromLocation = Convert.ToInt32(reqheaderPO.FromLocationId);
            //    vm.RequestnoteHeaderId = Convert.ToInt32(reqheaderPO.RequestnoteHeaderId);

            //    vvm.Add(vm);
            //}


        }

        public List<RequestNoteViewModel> GetRequestPO(string documentno,   int companyid)
        {
            List<RequestNoteViewModel> vvm = new List<RequestNoteViewModel>();



            var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                      .Get(h => h.DocumentNo ==documentno.Trim() && h.DocumentStatus == 2)
                      .FirstOrDefault();
  
           
                var reqdetailPO = _unitofwork.RequestNoteDetailRepository
                    .Get(d => d.RequestnoteHeaderId == reqheaderPO.RequestnoteHeaderId).ToList();

            

                foreach(var reqdetails in reqdetailPO)
                {
                    var vm = new RequestNoteViewModel();
                var supplierID = _unitofwork.SupplierProductRepository.Get(s => s.ProductId == reqdetails.ProductId).FirstOrDefault();

                    vm.ProductId = reqdetails.ProductId;
                    vm.CostPrice = reqdetails.CostPrice;
                    vm.SellingPrice = reqdetails.SellingPrice;
                    vm.RequestQty = reqdetails.RequestQty;
                    vm.GrossAmount = reqdetails.CostPrice * reqdetails.RequestQty;
                    vm.LocationId = reqheaderPO.ToLocationId;
                    vm.supplierID = supplierID.SupplierId;

                vvm.Add(vm);
                } 
          


            return vvm;
        }

        public List<RequestNoteViewModel> GetRequestedNoteLocationWise(long fromlocation,DateTime fromdate,DateTime toDate,int companyid)
        {
            List<RequestNoteViewModel> vvm = new List<RequestNoteViewModel>();
            var FromLocations = _unitofwork.LocationRepository.Get(f => f.SysLocationID == fromlocation).FirstOrDefault();
      
            if(FromLocations != null )
            {
                var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                    .Get(h => h.FromLocationId == fromlocation 
                                         && h.DocumentStatus == 2
                                     && h.IsPoTransfer == false
                                     && h.DocumentDate >= fromdate.Date
                                     && h.DocumentDate <= toDate.Date
                                     && h.IsApproved == true).ToList();

                foreach (var reqheader in reqheaderPO)
                {

                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd");
                    var vm = new RequestNoteViewModel();
                    vm.CreateDate = Date;
                    vm.Location = FromLocations.LocationName;
                    vm.DocumentNo = reqheader.DocumentNo;
                    vm.DocumentStatus = reqheader.DocumentStatus;
              
                    vm.AcceptedUser = "Admin";
                    vvm.Add(vm);
                }
            }
            return vvm;
        }

 public List<RequestNoteViewModel> GetLocationWiseRequestPO(long requestid,long requestedToID, DateTime from, DateTime to, int companyid)
{ 
    List<RequestNoteViewModel> vvm = new List<RequestNoteViewModel>();

   
           


            if (requestedToID > 0 && requestid > 0)
                {

                   var  reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                         .Get(h => h.ToLocationId == requestedToID
                                                  && h.FromLocationId == requestid
                                                   && h.DocumentStatus != 1
                                                   && DbFunctions.TruncateTime(h.DocumentDate) >= DbFunctions.TruncateTime(from.Date)
                                                   && DbFunctions.TruncateTime(h.DocumentDate) <= DbFunctions.TruncateTime(to.Date)
                                                                                        )

                                         .OrderByDescending(h => h.DocumentDate)
                                         .ToList();

                    foreach (var reqheader in reqheaderPO)
                    {
                        var vm = new RequestNoteViewModel();
                        var fromlocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.FromLocationId).FirstOrDefault();
                    var Tolocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.ToLocationId).FirstOrDefault();

                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd HH:mm:ss");

                    string ExpectedDeleveryDate = reqheader.ExpectedDeleveryDate.ToString("yyyy-MM-dd");

                    vm.requestnoteheaderid = reqheader.RequestnoteHeaderId;
                        vm.CreateDate = Date;

                    vm.ExpectedDeleveryDate = ExpectedDeleveryDate;
                        vm.Location = fromlocation.LocationName;
                        vm.ToLocation = Tolocation.LocationName;
                        vm.DocumentNo = reqheader.DocumentNo;
                        vm.LocationCode = fromlocation.LocationCode;
                        if (reqheader.DocumentStatus == 2 && reqheader.IsApproved == true)
                        {
                            vm.Approve = "Approved";
                        }
                     
                    else
                        {

                        if (reqheader.DocumentStatus == 4)
                            vm.Approve = "Canceled";
                        else
                            vm.Approve = "Pending";

                      
                        }


                        vvm.Add(vm);
                    }
                }
                else if (requestedToID > 0 )
                {

                    var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                          .Get(h => h.ToLocationId == requestedToID
                                                 && h.DocumentStatus != 1
                                                    && DbFunctions.TruncateTime(h.DocumentDate) >= DbFunctions.TruncateTime(from.Date)
                                                    && DbFunctions.TruncateTime(h.DocumentDate) <= DbFunctions.TruncateTime(to.Date)
                                                                                         )

                                          .OrderByDescending(h => h.DocumentDate)
                                          .ToList();

                    foreach (var reqheader in reqheaderPO)
                    {
                        var vm = new RequestNoteViewModel();
                        var fromlocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.FromLocationId).FirstOrDefault();
                    var Tolocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.ToLocationId).FirstOrDefault();
                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd HH:mm:ss");
                    string ExpectedDeleveryDate = reqheader.ExpectedDeleveryDate.ToString("yyyy-MM-dd");
                    vm.requestnoteheaderid = reqheader.RequestnoteHeaderId;
                        vm.CreateDate = Date;
                    vm.ExpectedDeleveryDate = ExpectedDeleveryDate;

                    vm.Location = fromlocation.LocationName;
                        vm.ToLocation = Tolocation.LocationName;
                        vm.DocumentNo = reqheader.DocumentNo;
                        vm.LocationCode = fromlocation.LocationCode;
                    if (reqheader.DocumentStatus == 2 && reqheader.IsApproved == true)
                    {
                            vm.Approve = "Approved";
                        }
                        else
                        {
                        if (reqheader.DocumentStatus == 4)
                            vm.Approve = "Canceled";
                        else
                            vm.Approve = "Pending";
                    }


                        vvm.Add(vm);
                    }
                }
                else
                if ( requestid > 0)
                {

                    var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                          .Get(h => 
                                                   h.FromLocationId == requestid
                                                 && h.DocumentStatus != 1
                                                    && DbFunctions.TruncateTime(h.DocumentDate) >= DbFunctions.TruncateTime(from.Date)
                                                    && DbFunctions.TruncateTime(h.DocumentDate) <= DbFunctions.TruncateTime(to.Date)
                                                                                         )

                                          .OrderByDescending(h => h.DocumentDate)
                                          .ToList();

                    foreach (var reqheader in reqheaderPO)
                    {
                        var vm = new RequestNoteViewModel();
                        var fromlocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.FromLocationId).FirstOrDefault();
                    var Tolocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.ToLocationId).FirstOrDefault();
                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd HH:mm:ss");
                    string ExpectedDeleveryDate = reqheader.ExpectedDeleveryDate.ToString("yyyy-MM-dd");

                    vm.ExpectedDeleveryDate = ExpectedDeleveryDate;

                    vm.requestnoteheaderid = reqheader.RequestnoteHeaderId;
                        vm.CreateDate = Date;
                        vm.Location = fromlocation.LocationName;
                        vm.ToLocation = Tolocation.LocationName;
                        vm.DocumentNo = reqheader.DocumentNo;
                        vm.LocationCode = fromlocation.LocationCode;
                    if (reqheader.DocumentStatus == 2 && reqheader.IsApproved == true)
                    {
                            vm.Approve = "Approved";
                        }
                        else
                        {
                        if (reqheader.DocumentStatus == 4)
                            vm.Approve = "Canceled";
                        else
                            vm.Approve = "Pending";
                    }


                        vvm.Add(vm);
                    }
                }
                else
                {

                    var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                        .Get(h => h.DocumentStatus != 1 &&
                                                 //&&
                                                  DbFunctions.TruncateTime(h.DocumentDate) >= DbFunctions.TruncateTime(from.Date)
                                                  && DbFunctions.TruncateTime(h.DocumentDate) <= DbFunctions.TruncateTime(to.Date)
                                                                                       )

                                        .OrderByDescending(h => h.DocumentDate)
                                        .ToList();

                    foreach (var reqheader in reqheaderPO)
                    {
                        var vm = new RequestNoteViewModel();
                        var fromlocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.FromLocationId).FirstOrDefault();
                    var Tolocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.ToLocationId).FirstOrDefault();
                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd HH:mm:ss");
                    string ExpectedDeleveryDate = reqheader.ExpectedDeleveryDate.ToString("yyyy-MM-dd");

                    vm.ExpectedDeleveryDate = ExpectedDeleveryDate;
                    vm.requestnoteheaderid = reqheader.RequestnoteHeaderId;
                        vm.CreateDate = Date;
                        vm.Location = fromlocation.LocationName;
                        vm.ToLocation = Tolocation.LocationName;
                        vm.DocumentNo = reqheader.DocumentNo;
                        vm.LocationCode = fromlocation.LocationCode;
                    if (reqheader.DocumentStatus == 2 && reqheader.IsApproved == true)
                    {
                            vm.Approve = "Approved";
                        }
                        else
                        {
                        if (reqheader.DocumentStatus == 4)
                            vm.Approve = "Canceled";
                        else
                            vm.Approve = "Pending";
                    }


                        vvm.Add(vm);
                    }

                }

           // }
    return vvm;
}


        public List<RequestNoteViewModel> GetRequestNoteDetailsView(long requestid, long requestedToID, DateTime from, DateTime to, int companyid)
        {
            List<RequestNoteViewModel> vvm = new List<RequestNoteViewModel>();





            if (requestedToID > 0 && requestid > 0)
            {

                var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                      .Get(h => h.ToLocationId == requestedToID
                                               && h.FromLocationId == requestid
                                             && (h.DocumentStatus == 2 || h.DocumentStatus==1 || h.DocumentStatus ==4)
                                                && DbFunctions.TruncateTime(h.DocumentDate) >= DbFunctions.TruncateTime(from.Date)
                                                && DbFunctions.TruncateTime(h.DocumentDate) <= DbFunctions.TruncateTime(to.Date)
                                                                                     )

                                      .OrderByDescending(h => h.DocumentDate)
                                      .ToList();

                foreach (var reqheader in reqheaderPO)
                {
                    var vm = new RequestNoteViewModel();
                    var fromlocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.FromLocationId).FirstOrDefault();
                    var Tolocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.ToLocationId).FirstOrDefault();

                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd HH:mm:ss");

                    vm.requestnoteheaderid = reqheader.RequestnoteHeaderId;
                    vm.CreateDate = Date;
                    string ExpectedDeleveryDate = reqheader.ExpectedDeleveryDate.ToString("yyyy-MM-dd");

                    vm.ExpectedDeleveryDate = ExpectedDeleveryDate;
                   
                    vm.Location = fromlocation.LocationName;
                    vm.ToLocation = Tolocation.LocationName;
                    vm.DocumentNo = reqheader.DocumentNo;
                    vm.LocationCode = fromlocation.LocationCode;
                    if (reqheader.IsApproved == true)
                    {
                        vm.Approve = "Approved";
                    }
                    else
                    {
                        if(reqheader.DocumentStatus == 4)
                            vm.Approve = "Cancelled";
                        else if (reqheader.IsTempRequest)
                            vm.Approve = "Pending [Tempory Saved]";
                        else
                            vm.Approve = "Pending";
                    }


                    vvm.Add(vm);
                }
            }
            else if (requestedToID > 0)
            {

                var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                      .Get(h => h.ToLocationId == requestedToID
                                              && (h.DocumentStatus == 2 || h.DocumentStatus == 1 || h.DocumentStatus == 4)
                                                && DbFunctions.TruncateTime(h.DocumentDate) >= DbFunctions.TruncateTime(from.Date)
                                                && DbFunctions.TruncateTime(h.DocumentDate) <= DbFunctions.TruncateTime(to.Date)
                                                                                     )

                                      .OrderByDescending(h => h.DocumentDate)
                                      .ToList();

                foreach (var reqheader in reqheaderPO)
                {
                    var vm = new RequestNoteViewModel();
                    var fromlocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.FromLocationId).FirstOrDefault();
                    var Tolocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.ToLocationId).FirstOrDefault();
                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd HH:mm:ss");
                    vm.ExpectedDeleveryDate = reqheader.ExpectedDeleveryDate.ToString("yyyy-MM-dd");
                    vm.requestnoteheaderid = reqheader.RequestnoteHeaderId;
                    vm.CreateDate = Date;
                    vm.Location = fromlocation.LocationName;
                    vm.ToLocation = Tolocation.LocationName;
                    vm.DocumentNo = reqheader.DocumentNo;
                    vm.LocationCode = fromlocation.LocationCode;
                    if (reqheader.IsApproved == true)
                    {
                        vm.Approve = "Approved";
                    }
                    else
                    {
                        if (reqheader.DocumentStatus == 4)
                            vm.Approve = "Cancelled";
                        else if (reqheader.IsTempRequest)
                            vm.Approve = "Pending [Tempory Saved]";
                        else
                            vm.Approve = "Pending";
                    }


                    vvm.Add(vm);
                }
            }
            else
            if (requestid > 0)
            {

                var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                      .Get(h =>
                                               h.FromLocationId == requestid
                                              && (h.DocumentStatus == 2 || h.DocumentStatus == 1 || h.DocumentStatus == 4)
                                                && DbFunctions.TruncateTime(h.DocumentDate) >= DbFunctions.TruncateTime(from.Date)
                                                && DbFunctions.TruncateTime(h.DocumentDate) <= DbFunctions.TruncateTime(to.Date)
                                                                                     )

                                      .OrderByDescending(h => h.DocumentDate)
                                      .ToList();

                foreach (var reqheader in reqheaderPO)
                {
                    var vm = new RequestNoteViewModel();
                    var fromlocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.FromLocationId).FirstOrDefault();
                    var Tolocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.ToLocationId).FirstOrDefault();
                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd HH:mm:ss");
                    vm.ExpectedDeleveryDate = reqheader.ExpectedDeleveryDate.ToString("yyyy-MM-dd");
                    vm.requestnoteheaderid = reqheader.RequestnoteHeaderId;
                    vm.CreateDate = Date;
                    vm.Location = fromlocation.LocationName;
                    vm.ToLocation = Tolocation.LocationName;
                    vm.DocumentNo = reqheader.DocumentNo;
                    vm.LocationCode = fromlocation.LocationCode;
                    if (reqheader.IsApproved == true)
                    {
                        vm.Approve = "Approved";
                    }
                    else
                    {
                        if (reqheader.DocumentStatus == 4)
                            vm.Approve = "Cancelled";
                        else if (reqheader.IsTempRequest)
                            vm.Approve = "Pending [Tempory Saved]";
                        else
                            vm.Approve = "Pending";
                    }


                    vvm.Add(vm);
                }
            }
            else
            {

                var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                    .Get(h =>   (h.DocumentStatus == 2 || h.DocumentStatus == 1 || h.DocumentStatus == 4)
                                              && DbFunctions.TruncateTime(h.DocumentDate) >= DbFunctions.TruncateTime(from.Date)
                                              && DbFunctions.TruncateTime(h.DocumentDate) <= DbFunctions.TruncateTime(to.Date)
                                                                                   )

                                    .OrderByDescending(h => h.DocumentDate)
                                    .ToList();

                foreach (var reqheader in reqheaderPO)
                {
                    var vm = new RequestNoteViewModel();
                    var fromlocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.FromLocationId).FirstOrDefault();
                    var Tolocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.ToLocationId).FirstOrDefault();
                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd HH:mm:ss");

                    vm.ExpectedDeleveryDate = reqheader.ExpectedDeleveryDate.ToString("yyyy-MM-dd");


                    vm.requestnoteheaderid = reqheader.RequestnoteHeaderId;
                    vm.CreateDate = Date;
                    vm.Location = fromlocation.LocationName;
                    vm.ToLocation = Tolocation.LocationName;
                    vm.DocumentNo = reqheader.DocumentNo;
                    vm.LocationCode = fromlocation.LocationCode;
                    if (reqheader.IsApproved == true)
                    {
                        vm.Approve = "Approved";
                    }
                    else
                    {
                        if (reqheader.DocumentStatus == 4)
                            vm.Approve = "Cancelled";
                        else if (reqheader.IsTempRequest)
                            vm.Approve = "Pending [Tempory Saved]";
                        else
                            vm.Approve = "Pending";
                    }


                    vvm.Add(vm);
                }

            }

            // }
            return vvm;
        }



        public List<RequestNoteViewModel> GetApprovedRequestData(long requestid, long requestedToID, DateTime from, DateTime to, int companyid)
        {
            List<RequestNoteViewModel> vvm = new List<RequestNoteViewModel>();





            if (requestedToID > 0 && requestid > 0)
            {

                var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                      .Get(h => h.ToLocationId == requestedToID
                                               && h.FromLocationId == requestid
                                             && h.DocumentStatus == 2    && h.IsApproved==true //&& h.IsPoTransfer ==false
                                                && DbFunctions.TruncateTime(h.DocumentDate) >= DbFunctions.TruncateTime(from.Date)
                                                && DbFunctions.TruncateTime(h.DocumentDate) <= DbFunctions.TruncateTime(to.Date)
                                                                                     )

                                      .OrderByDescending(h => h.DocumentDate)
                                      .ToList();

                foreach (var reqheader in reqheaderPO)
                {
                    var vm = new RequestNoteViewModel();
                    var fromlocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.FromLocationId).FirstOrDefault();
                    var Tolocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.ToLocationId).FirstOrDefault();

                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd HH:mm:ss");
                    string ExpectedDeleveryDate = reqheader.ExpectedDeleveryDate.ToString("yyyy-MM-dd");

                    vm.ExpectedDeleveryDate = ExpectedDeleveryDate;
                    vm.requestnoteheaderid = reqheader.RequestnoteHeaderId;
                    vm.CreateDate = Date;
                    vm.Location = fromlocation.LocationName;
                    vm.ToLocation = Tolocation.LocationName;
                    vm.DocumentNo = reqheader.DocumentNo;
                    vm.LocationCode = fromlocation.LocationCode;
                    if (reqheader.IsPoTransfer == true)
                    {
                        vm.Approve = "PO Generated";
                    }
                    else
                    {
                        vm.Approve = "Pending to Generate PO";
                    }


                    vvm.Add(vm);
                }
            }
            else if (requestedToID > 0)
            {

                var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                      .Get(h => h.ToLocationId == requestedToID
                                              && h.DocumentStatus == 2 && h.IsApproved == true // && h.IsPoTransfer == false
                                                && DbFunctions.TruncateTime(h.DocumentDate) >= DbFunctions.TruncateTime(from.Date)
                                                && DbFunctions.TruncateTime(h.DocumentDate) <= DbFunctions.TruncateTime(to.Date)
                                                                                     )

                                      .OrderByDescending(h => h.DocumentDate)
                                      .ToList();

                foreach (var reqheader in reqheaderPO)
                {
                    var vm = new RequestNoteViewModel();
                    var fromlocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.FromLocationId).FirstOrDefault();
                    var Tolocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.ToLocationId).FirstOrDefault();
                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd HH:mm:ss");


                    string ExpectedDeleveryDate = reqheader.ExpectedDeleveryDate.ToString("yyyy-MM-dd");

                    vm.ExpectedDeleveryDate = ExpectedDeleveryDate;

                    vm.requestnoteheaderid = reqheader.RequestnoteHeaderId;
                    vm.CreateDate = Date;
                    vm.Location = fromlocation.LocationName;
                    vm.ToLocation = Tolocation.LocationName;
                    vm.DocumentNo = reqheader.DocumentNo;
                    vm.LocationCode = fromlocation.LocationCode;
                    if (reqheader.IsPoTransfer == true)
                    {
                        vm.Approve = "PO Generated";
                    }
                    else
                    {
                        vm.Approve = "Pending to Generate PO";
                    }


                    vvm.Add(vm);
                }
            }
            else
            if (requestid > 0)
            {

                var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                      .Get(h =>
                                               h.FromLocationId == requestid
                                              && h.DocumentStatus == 2 && h.IsApproved == true //&& h.IsPoTransfer == false
                                                && DbFunctions.TruncateTime(h.DocumentDate) >= DbFunctions.TruncateTime(from.Date)
                                                && DbFunctions.TruncateTime(h.DocumentDate) <= DbFunctions.TruncateTime(to.Date)
                                                                                     )

                                      .OrderByDescending(h => h.DocumentDate)
                                      .ToList();

                foreach (var reqheader in reqheaderPO)
                {
                    var vm = new RequestNoteViewModel();
                    var fromlocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.FromLocationId).FirstOrDefault();
                    var Tolocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.ToLocationId).FirstOrDefault();
                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd HH:mm:ss");



                    string ExpectedDeleveryDate = reqheader.ExpectedDeleveryDate.ToString("yyyy-MM-dd");

                    vm.ExpectedDeleveryDate = ExpectedDeleveryDate;
                    vm.requestnoteheaderid = reqheader.RequestnoteHeaderId;
                    vm.CreateDate = Date;
                    vm.Location = fromlocation.LocationName;
                    vm.ToLocation = Tolocation.LocationName;
                    vm.DocumentNo = reqheader.DocumentNo;
                    vm.LocationCode = fromlocation.LocationCode;
                    if (reqheader.IsPoTransfer == true)
                    {
                        vm.Approve = "PO Generated";
                    }
                    else
                    {
                        vm.Approve = "Pending to Generate PO";
                    }


                    vvm.Add(vm);
                }
            }
            else
            {

                var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                    .Get(h =>    h.DocumentStatus == 2 && h.IsApproved == true ///&& h.IsPoTransfer == false
                                              && DbFunctions.TruncateTime(h.DocumentDate) >= DbFunctions.TruncateTime(from.Date)
                                              && DbFunctions.TruncateTime(h.DocumentDate) <= DbFunctions.TruncateTime(to.Date)
                                                                                   )

                                    .OrderByDescending(h => h.DocumentDate)
                                    .ToList();

                foreach (var reqheader in reqheaderPO)
                {
                    var vm = new RequestNoteViewModel();
                    var fromlocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.FromLocationId).FirstOrDefault();
                    var Tolocation = _unitofwork.LocationRepository.Get(f => f.SysLocationID == reqheader.ToLocationId).FirstOrDefault();
                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd HH:mm:ss");
                    string ExpectedDeleveryDate = reqheader.ExpectedDeleveryDate.ToString("yyyy-MM-dd");

                    vm.ExpectedDeleveryDate = ExpectedDeleveryDate;



                    vm.requestnoteheaderid = reqheader.RequestnoteHeaderId;
                    vm.CreateDate = Date;
                    vm.Location = fromlocation.LocationName;
                    vm.ToLocation = Tolocation.LocationName;
                    vm.DocumentNo = reqheader.DocumentNo;
                    vm.LocationCode = fromlocation.LocationCode;
                    if (reqheader.IsPoTransfer == true)
                    {
                        vm.Approve = "PO Generated";
                    }
                    else
                    {
                        vm.Approve = "Pending to Generate PO";
                    }


                    vvm.Add(vm);
                }

            }

            // }
            return vvm;
        }





        public List<RequestNoteViewModel> GetRemainingPOs(long requestid, DateTime from, DateTime to, int companyid)
        {
            List<RequestNoteViewModel> vvm = new List<RequestNoteViewModel>();

            var Locations = _unitofwork.LocationRepository.Get(l => l.SysLocationID == requestid).FirstOrDefault();

            if (Locations != null)
            {
                var reqheaderPO = _unitofwork.RequestNoteHeaderRepository
                                      .Get(h => h.ToLocationId == requestid
                                             && h.DocumentStatus == 2
                                             && h.IsPoTransfer == false
                                             && h.DocumentDate >= from.Date
                                             && h.DocumentDate <= to.Date)
                                      .ToList();

                foreach (var reqheader in reqheaderPO)
                {
                    var vm = new RequestNoteViewModel();

                    vm.DocumentNo = reqheader.DocumentNo;
                    vm.LocationCode = Locations.LocationCode;
                    vm.Location = Locations.LocationName;
                    string Date = reqheader.DocumentDate.ToString("yyyy-MM-dd");
                    vm.CreateDate = Date;

                    vm.AcceptedUser = "Admin";
                    vvm.Add(vm);
                }
            }
            return vvm;
        }


        public bool SubmitRequest(RequestNoteHeader header)
        {
            _unitofwork.CreateTransaction();
            {
                try
                {
                    _unitofwork.RequestNoteHeaderRepository.Insert(header);

                    if (_unitofwork.Save() == 1)
                    {
                        foreach (var detail in header.RequestDetail)
                        {
                            var stock = _unitofwork.ProductStockMasterRepository.Get(p => (p.LocationId == header.ToLocationId || p.LocationId == header.FromLocationId)
                                                                                && p.ProductId == detail.ProductId && p.CompanyID==header.CompanyId
                                                                        ).FirstOrDefault();

                            detail.RequestnoteHeaderId = header.RequestnoteHeaderId;
                            
                            detail.AvgCost = stock.AvgCost;
                            detail.LineNo = header.RequestDetail.IndexOf(detail) + 1;
                            detail.UnitOfMeasureId = _bllproduct.GetProductById(detail.ProductId).PurchasingUnit;

                            _unitofwork.RequestNoteDetailRepository.Insert(detail);
                        }

                        _unitofwork.Save();
                        _unitofwork.Commit();
                        return true;
                    }
                    else
                    {
                        _unitofwork.Rollback();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _unitofwork.Rollback();
                    return false;
                }
            }
        }

        public bool AcceptRequest(RequestNoteAccptanceHeader header)
        {
            _unitofwork.CreateTransaction();
            {
                try
                {
                    var dbheader = _unitofwork.RequestNoteHeaderRepository.Get(h => h.DocumentNo == header.DocumentNo && h.RequestnoteHeaderId == header.RequestnoteHeaderId  && h.CompanyId==1).FirstOrDefault();
                    header.ToLocationId = dbheader.ToLocationId;
                    header.ToDepartmentId = dbheader.ToDepartmentId;

                    _unitofwork.RequestNoteAccptanceHeaderRepository.Insert(header);

                    if (_unitofwork.Save() == 1)
                    {
                        
                        foreach (var accdetail in header.AcceptanceDetail)
                        {

                          
                            accdetail.RequestNoteAccptanceHeaderId = header.RequestNoteAccptanceHeaderId;
                            accdetail.LineNo = header.AcceptanceDetail.IndexOf(accdetail) + 1;
                            accdetail.UnitOfMeasureId = _bllproduct.GetProductById(accdetail.ProductId).PurchasingUnit;
                            accdetail.RequestnoteHeaderId = header.RequestnoteHeaderId;
                            _unitofwork.RequestNoteAccptanceDetailRepository.Insert(accdetail);

                            UpdateRequestDetails(accdetail, header);  
                        }
                        UpdateRequestStatus(header);
                        _unitofwork.Save();
                        _unitofwork.Commit();
                        return true;
                    }
                    else
                    {
                        _unitofwork.Rollback();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _unitofwork.Rollback();
                    return false;
                }
            }
        }

        private int UpdateRequestStatus(RequestNoteAccptanceHeader header)
        {
            try
            {
                var dbheader = _unitofwork.RequestNoteHeaderRepository.Get(h => h.DocumentNo == header.DocumentNo && h.RequestnoteHeaderId == header.RequestnoteHeaderId && h.CompanyId==header.CompanyId).FirstOrDefault();
                dbheader.IsApproved = true;
                if (dbheader.DocumentStatus == 1)
                {
                    dbheader.DocumentStatus = 1;
                }
                else if (dbheader.DocumentStatus == 2)
                {
                    dbheader.DocumentStatus = 2;
                }
                else if (dbheader.DocumentStatus == 3)
                {
                    dbheader.DocumentStatus = 3;
                }

                var RequestDetails = _unitofwork.RequestNoteDetailRepository.Get(h => h.RequestnoteHeaderId == header.RequestnoteHeaderId ).ToList();


                dbheader.TotCostPrice = RequestDetails.Sum(X => X.RequestQty * X.CostPrice);
                dbheader.TotSellingPrice = RequestDetails.Sum(X => X.RequestQty * X.SellingPrice);

                _unitofwork.RequestNoteHeaderRepository.Update(dbheader);
                return _unitofwork.Save();
            }
            catch (Exception e)
            {
                return 0;
            }
        }



        private int UpdateRequestDetails(RequestNoteAcceptanceDetail Detail, RequestNoteAccptanceHeader header)
        {
            try
            {
                var RequestDetails = _unitofwork.RequestNoteDetailRepository.Get( h=> h.RequestnoteHeaderId == header.RequestnoteHeaderId && h.ProductId == Detail.ProductId).FirstOrDefault();

                RequestDetails.RequestQty = Detail.IssueQty;
                _unitofwork.RequestNoteDetailRepository.Update(RequestDetails);
               return  _unitofwork.Save();               
             
            }
            catch (Exception ex)
            {
                return 0;
            }
        }






        public int UpdateRequestStatusReject(RequestNoteAccptanceHeader header)
        {
            try
            {
                var dbheader = _unitofwork.RequestNoteHeaderRepository.Get(h => h.DocumentNo == header.DocumentNo && h.CompanyId==header.CompanyId).FirstOrDefault();
                dbheader.DocumentStatus = 4;
                dbheader.IsTempRequest = false;
                _unitofwork.RequestNoteHeaderRepository.Update(dbheader);
                return _unitofwork.Save();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public int UpdateRequestStatusCancel(RequestNoteAccptanceHeader header)
        {
            try
            {
                var dbheader = _unitofwork.RequestNoteHeaderRepository.Get(h => h.DocumentNo == header.DocumentNo && h.CompanyId==header.CompanyId).FirstOrDefault();
                dbheader.DocumentStatus = 5;
                _unitofwork.RequestNoteHeaderRepository.Update(dbheader);
                return _unitofwork.Save();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        private int UpdateRequestStatusReopen(RequestNoteAccptanceHeader header,int ccc)
        {
            try
            {
                var dbheader = _unitofwork.RequestNoteHeaderRepository.Get(h => h.DocumentNo == header.DocumentNo && h.CompanyId==header.CompanyId).FirstOrDefault();
                dbheader.DocumentStatus = 6;
                _unitofwork.RequestNoteHeaderRepository.Update(dbheader);
                return _unitofwork.Save();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public int UpdateRequestStatusForcefullyClose(RequestNoteAccptanceHeader header)
        {
            try
            {
                var dbheader = _unitofwork.RequestNoteHeaderRepository.Get(h => h.DocumentNo == header.DocumentNo && h.CompanyId==header.CompanyId).FirstOrDefault();
                dbheader.DocumentStatus = 7;
                _unitofwork.RequestNoteHeaderRepository.Update(dbheader);
                return _unitofwork.Save();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public long DelateRequestDetailById(long id)
        {
            long res = 0;
            try
            {
                _unitofwork.RequestNoteDetailRepository.DeleteRange(_unitofwork.RequestNoteDetailRepository.Get(x => x.RequestnoteHeaderId == id));
                return res = _unitofwork.Save();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public bool CancelRequest(RequestNoteHeader header)
        {
            _unitofwork.CreateTransaction();
            {
                try
                {
                    
                    var dbheader = _unitofwork.RequestNoteHeaderRepository.Get(h => h.RequestnoteHeaderId == header.RequestnoteHeaderId).FirstOrDefault();

                    dbheader.CanceledDate = DateTime.Now;
                    dbheader.CanceleddBy = header.CanceleddBy;
                    dbheader.DocumentStatus = header.DocumentStatus;
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
        public bool ModifyRequest(RequestNoteHeader header)
        {
            _unitofwork.CreateTransaction();
            {
                try
                {
                    DelateRequestDetailById(header.RequestnoteHeaderId);
                    var dbheader = _unitofwork.RequestNoteHeaderRepository.Get(h => h.RequestnoteHeaderId == header.RequestnoteHeaderId).FirstOrDefault();

                    dbheader.DocumentNo = header.DocumentNo;
                    dbheader.DocumentDate = DateTime.Now;
                    dbheader.IsTempRequest = header.IsTempRequest;
                    dbheader.FromLocationId = header.FromLocationId;
                    dbheader.ToLocationId = header.ToLocationId;
                    dbheader.FromDepartmentId = header.FromDepartmentId;
                    dbheader.ToDepartmentId = header.ToDepartmentId;
                    dbheader.DocumentStatus = header.DocumentStatus;
                    dbheader.TotCostPrice = header.TotCostPrice;
                    dbheader.ExpectedDeleveryDate = header.ExpectedDeleveryDate;
                    //context.RequestNoteAccptanceHeader.Add(header);

                    _unitofwork.Save();

                    foreach (var detail in header.RequestDetail)
                    {
                        var stock = _unitofwork.ProductStockMasterRepository.Get(p => (p.LocationId == header.ToLocationId || p.LocationId == header.FromLocationId)
                                                                            && p.ProductId == detail.ProductId && p.CompanyID==header.CompanyId
                                                                    ).FirstOrDefault();

                        detail.RequestnoteHeaderId = header.RequestnoteHeaderId;
                        detail.AvgCost = stock.AvgCost;
                        detail.LineNo = header.RequestDetail.IndexOf(detail) + 1;
                        detail.UnitOfMeasureId = _bllproduct.GetProductById(detail.ProductId).PurchasingUnit;

                        _unitofwork.RequestNoteDetailRepository.Insert(detail);
                    }

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

        public RequestNoteAccptanceHeader GetAcceptedHeaderById(int id)
        {
            return _unitofwork.RequestNoteAccptanceHeaderRepository.GetById(id);
        }

        public List<RequestNoteAcceptanceDetail> GetAcceptedDetailByHeaderId(int id)
        {
            return _unitofwork.RequestNoteAccptanceDetailRepository.Get(r=>r.RequestNoteAccptanceHeaderId==id).ToList();
        }
    }
}
