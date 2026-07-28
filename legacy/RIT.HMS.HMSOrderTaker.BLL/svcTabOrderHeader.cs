using RIT.HMS.HMSOrderTaker.Data;
using RIT.HMS.HMSOrderTaker.Domain;
using RIT.HMS.HMSOrderTaker.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.BLL
{
    public class svcTabOrderHeader
    {
        private UnitOfWork<SmartLinkEntities> unitOfWork;
        public svcTabOrderHeader()
        {
            unitOfWork = new UnitOfWork<SmartLinkEntities>();

        }
        public int GetNextOrderSeqId(int LocationId)
        {
            try
            {
                int TempId = 0;

                if ((int)unitOfWork.Tbl_TabOrderHeader.Get(filter: x => x.LocationId == LocationId).ToList().Count == 0)
                {
                    return TempId = 1;
                }
                else
                {
                    TempId = (int)unitOfWork.Tbl_TabOrderHeader.Get(filter: x => x.LocationId == LocationId).Max(u => u.OrderSeqNumber) + 1;

                    return TempId;
                }


            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
        public string GetNextRunningOrderNumber(int LocationId)
        {
            try
            {
                char pad = '0';
                int TempId = 0;
                string OrderNo;

                if ((int)unitOfWork.Tbl_TabOrderHeader.Get(filter: x => x.LocationId == LocationId).ToList().Count == 0)
                {
                    TempId = TempId + 1;

                    OrderNo = TempId.ToString().PadLeft(4, pad);

                    return OrderNo;
                }
                else
                {
                    TempId = (int)unitOfWork.Tbl_TabOrderHeader.Get(filter: x => x.LocationId == LocationId).Max(u => u.OrderSeqNumber) + 1;

                    OrderNo = TempId.ToString().PadLeft(4, pad);

                    return OrderNo;
                }


            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
        //Before Order items
        public vmTabOrderHeader SaveTableHead(vmTabOrderHeader TabOrderHeade)
        {
            try
            {
                var locationinfo = unitOfWork.Tbl_SysLocation.Get(filter: x => x.SysLocationID == TabOrderHeade.LocationId).FirstOrDefault();
                var tableinfo = unitOfWork.Tbl_TblMasters.Get(filter: x => x.LocationId == TabOrderHeade.LocationId && x.TableMasterID == TabOrderHeade.TableId).FirstOrDefault();

                if (tableinfo == null && locationinfo == null)
                {
                    return null;
                }

                STOS_TabOrderHeader objIns = new STOS_TabOrderHeader()
                {
                    LocationId = locationinfo.SysLocationID,
                    OrderSeqNumber = GetNextOrderSeqId(TabOrderHeade.LocationId),
                    TableId = tableinfo.TableMasterID,
                    TableCode = tableinfo.TableCode,
                    RunningOrderNumber = GetNextRunningOrderNumber(TabOrderHeade.LocationId),
                    OrderName = GetNextRunningOrderNumber(TabOrderHeade.LocationId),
                    LocationCode = locationinfo.LocationCode,
                    LoggedInUserId = TabOrderHeade.LoggedInUserId,
                    LoggedInUseName = TabOrderHeade.LoggedInUseName,
                    CustomeId = TabOrderHeade.CustomeId,
                    CustomeName = TabOrderHeade.CustomeName,
                    isLoyatyCustomer = TabOrderHeade.isLoyatyCustomer,
                    isPromotionItems = TabOrderHeade.isPromotionItems,
                    TabOrderType = TabOrderHeade.TabOrderType,
                    TabOrderStartDateTime = DateTime.Now,
                    //TabOrderCompleteDateTime = TabOrderHeade.TabOrderCompleteDateTime,
                    OrderBillFinalizedPayment = 0,
                    OrderBillDiscount = 0,
                    OrderSubtotal = 0,
                    OrderServiceCharge = 0,
                    OrderItemCount = 0,
                    Remark = TabOrderHeade.Remark,
                    TabOrderStatus = (int)Domain.Common.Enums.enumTabOrderHead.PendingOrder,

                    CreatedDateTime = DateTime.Now,
                    CreatedBy = TabOrderHeade.CreatedBy,
                    CreatedMachine = TabOrderHeade.CreatedMachine,
                    ModifiedDateTime = TabOrderHeade.ModifiedDateTime,
                    ModifiedBy = TabOrderHeade.ModifiedBy,
                    ModifiedMachine = TabOrderHeade.ModifiedMachine,

                };
                unitOfWork.Tbl_TabOrderHeader.Insert(objIns);
                unitOfWork.Save();

                #region ReturnObj
                vmTabOrderHeader objReturn = new vmTabOrderHeader()
                {
                    LocationId = objIns.LocationId,
                    OrderSeqNumber = objIns.OrderSeqNumber,
                    TableId = objIns.TableId,
                    TableCode = objIns.TableCode,
                    RunningOrderNumber = objIns.RunningOrderNumber,
                    OrderName = objIns.OrderName,
                    LocationCode = objIns.LocationCode,
                    LoggedInUserId = objIns.LoggedInUserId,
                    LoggedInUseName = objIns.LoggedInUseName,
                    CustomeId = objIns.CustomeId,
                    CustomeName = objIns.CustomeName,
                    isLoyatyCustomer = objIns.isLoyatyCustomer,
                    isPromotionItems = objIns.isPromotionItems,
                    TabOrderType = objIns.TabOrderType,
                    TabOrderStartDateTime = objIns.TabOrderStartDateTime,
                    //TabOrderCompleteDateTime = TabOrderHeade.TabOrderCompleteDateTime,
                    OrderBillFinalizedPayment = objIns.OrderBillFinalizedPayment,
                    OrderBillDiscount = objIns.OrderBillDiscount,
                    OrderSubtotal = objIns.OrderSubtotal,
                    OrderServiceCharge = objIns.OrderServiceCharge,
                    OrderItemCount = objIns.OrderItemCount,
                    Remark = objIns.Remark,
                    TabOrderStatus = objIns.TabOrderStatus,

                    CreatedDateTime = objIns.CreatedDateTime,
                    CreatedBy = objIns.CreatedBy,
                    CreatedMachine = objIns.CreatedMachine,
                    ModifiedDateTime = objIns.ModifiedDateTime,
                    ModifiedBy = objIns.ModifiedBy,
                    ModifiedMachine = objIns.ModifiedMachine,

                };
                #endregion

                return objReturn;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<vmTabOrderHeader> GetActiveOrderByLocationTableId(int locationId, int TblId)
        {
            try
            {

                List<vmTabOrderHeader> OrderHead = unitOfWork.Tbl_TabOrderHeader.Get(filter: g => g.LocationId == locationId && g.TableId == TblId
                && g.TabOrderStatus != (int)Domain.Common.Enums.enumTabOrderHead.CancelOrder).Select(y => new vmTabOrderHeader
                {
                    LocationId = y.LocationId,
                    OrderSeqNumber = y.OrderSeqNumber,
                    TableId = y.TableId,
                    TableCode = y.TableCode,
                    RunningOrderNumber = y.RunningOrderNumber,
                    OrderName = y.OrderName,
                    LocationCode = y.LocationCode,
                    LoggedInUserId = y.LoggedInUserId,
                    LoggedInUseName = y.LoggedInUseName,
                    CustomeId = y.CustomeId,
                    CustomeName = y.CustomeName,
                    isLoyatyCustomer = y.isLoyatyCustomer,
                    isPromotionItems = y.isPromotionItems,
                    TabOrderType = y.TabOrderType,
                    TabOrderStartDateTime = y.TabOrderStartDateTime,
                    TabOrderCompleteDateTime = y.TabOrderCompleteDateTime,
                    OrderBillFinalizedPayment = y.OrderBillFinalizedPayment,
                    OrderBillDiscount = y.OrderBillDiscount,
                    OrderSubtotal = y.OrderSubtotal,
                    OrderServiceCharge = y.OrderServiceCharge,
                    OrderItemCount = unitOfWork.Tbl_TabOrderDetail.Get(filter: d => d.OrderSeqNumber == y.OrderSeqNumber && d.LocationId == y.LocationId && d.TableId == y.TableId).Count(),
                    Remark = y.Remark,
                    TabOrderStatus = y.TabOrderStatus,
                    CreatedDateTime = y.CreatedDateTime,
                    CreatedBy = y.CreatedBy,
                    CreatedMachine = y.CreatedMachine,
                    ModifiedDateTime = y.ModifiedDateTime,
                    ModifiedBy = y.ModifiedBy,
                    ModifiedMachine = y.ModifiedMachine,
                    RealDateTime = y.CreatedDateTime.ToString(),

                }).ToList();

                if (OrderHead != null)
                {
                    return OrderHead;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public vmTabOrderHeader GetActiveOrderByLocationOrderSeqId(int locationId, int OrderNo)
        {
            try
            {


                vmTabOrderHeader OrderHead = unitOfWork.Tbl_TabOrderHeader.Get(filter: g => g.LocationId == locationId && g.OrderSeqNumber == OrderNo &&
                g.TabOrderStatus != (int)Domain.Common.Enums.enumTabOrderHead.CancelOrder).Select(y => new vmTabOrderHeader
                {
                    LocationId = y.LocationId,
                    OrderSeqNumber = y.OrderSeqNumber,
                    TableId = y.TableId,
                    TableCode = y.TableCode,
                    RunningOrderNumber = y.RunningOrderNumber,
                    OrderName = y.OrderName,
                    LocationCode = y.LocationCode,
                    LoggedInUserId = y.LoggedInUserId,
                    LoggedInUseName = y.LoggedInUseName,
                    CustomeId = y.CustomeId,
                    CustomeName = y.CustomeName,
                    isLoyatyCustomer = y.isLoyatyCustomer,
                    isPromotionItems = y.isPromotionItems,
                    TabOrderType = y.TabOrderType,
                    TabOrderStartDateTime = y.TabOrderStartDateTime,
                    TabOrderCompleteDateTime = y.TabOrderCompleteDateTime,
                    OrderBillFinalizedPayment = y.OrderBillFinalizedPayment,
                    OrderBillDiscount = y.OrderBillDiscount,
                    OrderSubtotal = y.OrderSubtotal,
                    OrderServiceCharge = y.OrderServiceCharge,
                    OrderItemCount = y.OrderItemCount,
                    Remark = y.Remark,
                    TabOrderStatus = y.TabOrderStatus,
                    CreatedDateTime = y.CreatedDateTime,
                    CreatedBy = y.CreatedBy,
                    CreatedMachine = y.CreatedMachine,
                    ModifiedDateTime = y.ModifiedDateTime,
                    ModifiedBy = y.ModifiedBy,
                    ModifiedMachine = y.ModifiedMachine,
                    RealDateTime = y.CreatedDateTime.ToString(),

                }).FirstOrDefault();

                if (OrderHead != null)
                {
                    return OrderHead;
                }
                else
                {
                    return null;
                }


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public vmTabOrderHeader CanselActiveOrderByLocationTableId(int locationId, int orderseqid)
        {
            try
            {
                STOS_TabOrderHeader objUpd = unitOfWork.Tbl_TabOrderHeader.Get(filter: x => x.LocationId == locationId &&
                                                                    x.OrderSeqNumber == orderseqid).FirstOrDefault();

                if (objUpd != null)
                {
                    objUpd.TabOrderStatus = (int)Domain.Common.Enums.enumTabOrderHead.CancelOrder;


                    unitOfWork.Tbl_TabOrderHeader.Update(objUpd);
                    unitOfWork.Save();

                    #region ReturnObj
                    vmTabOrderHeader objReturn = new vmTabOrderHeader()
                    {
                        LocationId = objUpd.LocationId,
                        OrderSeqNumber = objUpd.OrderSeqNumber,
                        TableId = objUpd.TableId,
                        TableCode = objUpd.TableCode,
                        RunningOrderNumber = objUpd.RunningOrderNumber,
                        OrderName = objUpd.OrderName,
                        LocationCode = objUpd.LocationCode,
                        LoggedInUserId = objUpd.LoggedInUserId,
                        LoggedInUseName = objUpd.LoggedInUseName,
                        CustomeId = objUpd.CustomeId,
                        CustomeName = objUpd.CustomeName,
                        isLoyatyCustomer = objUpd.isLoyatyCustomer,
                        isPromotionItems = objUpd.isPromotionItems,
                        TabOrderType = objUpd.TabOrderType,
                        TabOrderStartDateTime = objUpd.TabOrderStartDateTime,
                        //TabOrderCompleteDateTime = TabOrderHeade.TabOrderCompleteDateTime,
                        OrderBillFinalizedPayment = objUpd.OrderBillFinalizedPayment,
                        OrderBillDiscount = objUpd.OrderBillDiscount,
                        OrderSubtotal = objUpd.OrderSubtotal,
                        OrderServiceCharge = objUpd.OrderServiceCharge,
                        OrderItemCount = objUpd.OrderItemCount,
                        Remark = objUpd.Remark,
                        TabOrderStatus = objUpd.TabOrderStatus,

                        CreatedDateTime = objUpd.CreatedDateTime,
                        CreatedBy = objUpd.CreatedBy,
                        CreatedMachine = objUpd.CreatedMachine,
                        ModifiedDateTime = objUpd.ModifiedDateTime,
                        ModifiedBy = objUpd.ModifiedBy,
                        ModifiedMachine = objUpd.ModifiedMachine,

                    };
                    #endregion

                    return objReturn;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public vmTabOrderHeader UpdateTabOrderHeadIteamQty(int locationId, int orderseqid, int itemcount)
        {
            try
            {
                STOS_TabOrderHeader objUpd = unitOfWork.Tbl_TabOrderHeader.Get(filter: x => x.LocationId == locationId &&
                                                                    x.OrderSeqNumber == orderseqid).FirstOrDefault();

                if (objUpd != null)
                {
                    objUpd.OrderItemCount = itemcount;


                    unitOfWork.Tbl_TabOrderHeader.Update(objUpd);
                    unitOfWork.Save();

                    #region ReturnObj
                    vmTabOrderHeader objReturn = new vmTabOrderHeader()
                    {
                        LocationId = objUpd.LocationId,
                        OrderSeqNumber = objUpd.OrderSeqNumber,
                        TableId = objUpd.TableId,
                        TableCode = objUpd.TableCode,
                        RunningOrderNumber = objUpd.RunningOrderNumber,
                        OrderName = objUpd.OrderName,
                        LocationCode = objUpd.LocationCode,
                        LoggedInUserId = objUpd.LoggedInUserId,
                        LoggedInUseName = objUpd.LoggedInUseName,
                        CustomeId = objUpd.CustomeId,
                        CustomeName = objUpd.CustomeName,
                        isLoyatyCustomer = objUpd.isLoyatyCustomer,
                        isPromotionItems = objUpd.isPromotionItems,
                        TabOrderType = objUpd.TabOrderType,
                        TabOrderStartDateTime = objUpd.TabOrderStartDateTime,
                        //TabOrderCompleteDateTime = TabOrderHeade.TabOrderCompleteDateTime,
                        OrderBillFinalizedPayment = objUpd.OrderBillFinalizedPayment,
                        OrderBillDiscount = objUpd.OrderBillDiscount,
                        OrderSubtotal = objUpd.OrderSubtotal,
                        OrderServiceCharge = objUpd.OrderServiceCharge,
                        OrderItemCount = objUpd.OrderItemCount,
                        Remark = objUpd.Remark,
                        TabOrderStatus = objUpd.TabOrderStatus,

                        CreatedDateTime = objUpd.CreatedDateTime,
                        CreatedBy = objUpd.CreatedBy,
                        CreatedMachine = objUpd.CreatedMachine,
                        ModifiedDateTime = objUpd.ModifiedDateTime,
                        ModifiedBy = objUpd.ModifiedBy,
                        ModifiedMachine = objUpd.ModifiedMachine,

                    };
                    #endregion

                    return objReturn;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        // hasanka
        public vmTabOrderHeader GetOrderHeaderBySequanceIdLocationId(int sequanceid, int locationid)
        {
            
            var order = (from h in unitOfWork.Tbl_TabOrderHeader.Get(filter: oh => oh.OrderSeqNumber == sequanceid &&
                                oh.LocationId == locationid) join d in (unitOfWork.Tbl_TabOrderDetail.Get(filter: oh => oh.OrderSeqNumber == sequanceid &&
                                oh.LocationId == locationid)
                                )
                                 on h.OrderSeqNumber equals d.OrderSeqNumber
                                 select new
                                 {
                                     LocationId = h.LocationId,
                                     OrderSeqNumber = h.OrderSeqNumber,
                                     ItemName = d.ItemName,
                                     ItemId = d.ItemId,
                                     ItemSellingPrice = d.ItemSellingPrice,
                                     ItemQty = d.ItemQty,
                                     TableId = h.TableId
                                 }
                        ).ToList();
            List<vmTabOrderDetail> vmorderdetaillist = new List<vmTabOrderDetail>();
            foreach (var ord in order)
            {

                vmTabOrderDetail vmorderedetail = new vmTabOrderDetail();
                vmorderedetail.ItemId = ord.ItemId;
                vmorderedetail.ItemQty = ord.ItemQty;
                vmorderedetail.ItemSellingPrice = ord.ItemSellingPrice;
                vmorderedetail.ItemName = ord.ItemName;
                vmorderedetail.OrderSeqNumber = ord.OrderSeqNumber;
                vmorderedetail.TableId = ord.TableId;
                vmorderdetaillist.Add(vmorderedetail);
            }
            if (order.Count != 0)
            {
                vmTabOrderHeader orderheader = new vmTabOrderHeader();
                orderheader.LocationId = order.FirstOrDefault().LocationId;
                orderheader.OrderSeqNumber = order.FirstOrDefault().OrderSeqNumber;
                orderheader.TabOrderDetailsList = vmorderdetaillist;
            
                return orderheader;
            }
            else
            {
                
                return new vmTabOrderHeader();

            }
        }
    }
}
