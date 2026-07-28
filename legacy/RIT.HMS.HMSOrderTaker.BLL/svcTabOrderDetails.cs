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
    public class svcTabOrderDetails
    {
        private UnitOfWork<SmartLinkEntities> unitOfWork;
        public svcTabOrderDetails()
        {
            unitOfWork = new UnitOfWork<SmartLinkEntities>();

        }
        public int GetNextRunningItemSeqId(int LocationId, int orderseqid)
        {
            try
            {

                int TempId = 0;
                if ((int)unitOfWork.Tbl_TabOrderDetail.Get(filter: x => x.LocationId == LocationId && x.OrderSeqNumber == orderseqid).ToList().Count == 0)
                {
                    TempId = TempId + 1;
                    return TempId;
                }
                else
                {
                    TempId = (int)unitOfWork.Tbl_TabOrderDetail.Get(filter: x => x.LocationId == LocationId && x.OrderSeqNumber == orderseqid).Max(u => u.ItemSeqId) + 1;
                    return TempId;
                }


            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
        public vmTabOrderDetail SaveTableDetails(vmTabOrderDetail TabOrderDetails)
        {
            try
            {

                STOS_TabOrderDetail objIns = new STOS_TabOrderDetail()
                {

                    LocationId = TabOrderDetails.LocationId,
                    OrderSeqNumber = TabOrderDetails.OrderSeqNumber,
                    ItemSeqId = GetNextRunningItemSeqId(TabOrderDetails.LocationId, TabOrderDetails.OrderSeqNumber),
                    ItemId = TabOrderDetails.ItemId,
                    ItemCode = TabOrderDetails.ItemCode,
                    ItemNameOnBill = TabOrderDetails.ItemNameOnBill,
                    ItemName = TabOrderDetails.ItemName,
                    TableId = TabOrderDetails.TableId,
                    TableCode = TabOrderDetails.TableCode,
                    ItemCostPrice = TabOrderDetails.ItemCostPrice,
                    ItemSellingPrice = TabOrderDetails.ItemSellingPrice,
                    IsItemOnPromotion = TabOrderDetails.IsItemOnPromotion,
                    ItemQty = TabOrderDetails.ItemQty,
                    ItemServingTypeid = TabOrderDetails.ItemServingTypeid,
                    ItemServingTypeName = TabOrderDetails.ItemServingTypeName,
                    IsWithAddOn = TabOrderDetails.IsWithAddOn,
                    OrderedItemRemark = TabOrderDetails.OrderedItemRemark,
                    ItemKOTBOT = TabOrderDetails.ItemKOTBOT,
                    ItemKOTBOTStatus = TabOrderDetails.ItemKOTBOTStatus,
                    ItemKOTBOTStartDateTime = TabOrderDetails.ItemKOTBOTStartDateTime,
                    ItemKOTBOTEndDateTime = TabOrderDetails.ItemKOTBOTEndDateTime,
                    OrderedItemStatus = TabOrderDetails.OrderedItemStatus,
                    CreatedDateTime = DateTime.Now,
                    CreatedBy = TabOrderDetails.CreatedBy,
                    CreatedMachine = TabOrderDetails.CreatedMachine,
                    ModifiedDateTime = TabOrderDetails.ModifiedDateTime,
                    ModifiedBy = TabOrderDetails.ModifiedBy,
                    ModifiedMachine = TabOrderDetails.ModifiedMachine,

                };
                unitOfWork.Tbl_TabOrderDetail.Insert(objIns);
                unitOfWork.Save();

                #region ReturnObj
                vmTabOrderDetail objReturn = new vmTabOrderDetail()
                {
                    LocationId = objIns.LocationId,
                    OrderSeqNumber = objIns.OrderSeqNumber,
                    ItemSeqId = objIns.ItemSeqId,
                    ItemId = objIns.ItemId,
                    ItemCode = objIns.ItemCode,
                    ItemNameOnBill = objIns.ItemNameOnBill,
                    ItemName = objIns.ItemName,
                    TableId = objIns.TableId,
                    TableCode = objIns.TableCode,
                    ItemCostPrice = objIns.ItemCostPrice,
                    ItemSellingPrice = objIns.ItemSellingPrice,
                    IsItemOnPromotion = objIns.IsItemOnPromotion,
                    ItemQty = objIns.ItemQty,
                    ItemServingTypeid = objIns.ItemServingTypeid,
                    ItemServingTypeName = objIns.ItemServingTypeName,
                    IsWithAddOn = objIns.IsWithAddOn,
                    OrderedItemRemark = objIns.OrderedItemRemark,
                    ItemKOTBOT = objIns.ItemKOTBOT,
                    ItemKOTBOTStatus = objIns.ItemKOTBOTStatus,
                    ItemKOTBOTStartDateTime = objIns.ItemKOTBOTStartDateTime,
                    ItemKOTBOTEndDateTime = objIns.ItemKOTBOTEndDateTime,
                    OrderedItemStatus = objIns.OrderedItemStatus,
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
        public List<vmTabOrderDetail> GetActiveOrderItemByLocationOrderSeqId(int locationId, int OrderNo)
        {
            try
            {
                var OrderHead = unitOfWork.Tbl_TabOrderDetail.Get(filter: g => g.LocationId == locationId && g.OrderSeqNumber == OrderNo 
                && g.OrderedItemStatus == (int)Domain.Common.Enums.enumTabOrderDetails.PendingToSterwed
                ).Select(y => new vmTabOrderDetail
                {
                    LocationId = y.LocationId,
                    OrderSeqNumber = y.OrderSeqNumber,
                    ItemSeqId = y.ItemSeqId,
                    ItemId = y.ItemId,
                    ItemCode = y.ItemCode,
                    ItemNameOnBill = y.ItemNameOnBill,
                    ItemName = y.ItemName,
                    TableId = y.TableId,
                    TableCode = y.TableCode,
                    ItemCostPrice = y.ItemCostPrice,
                    ItemSellingPrice = y.ItemSellingPrice,
                    IsItemOnPromotion = y.IsItemOnPromotion,
                    ItemQty = y.ItemQty,
                    ItemServingTypeid = y.ItemServingTypeid,
                    ItemServingTypeName = y.ItemServingTypeName,
                    IsWithAddOn = y.IsWithAddOn,
                    OrderedItemRemark = y.OrderedItemRemark,
                    ItemKOTBOT = y.ItemKOTBOT,
                    ItemKOTBOTStatus = y.ItemKOTBOTStatus,
                    ItemKOTBOTStartDateTime = y.ItemKOTBOTStartDateTime,
                    ItemKOTBOTEndDateTime = y.ItemKOTBOTEndDateTime,
                    OrderedItemStatus = y.OrderedItemStatus,
                    CreatedDateTime = y.CreatedDateTime,
                    CreatedBy = y.CreatedBy,
                    CreatedMachine = y.CreatedMachine,
                    ModifiedDateTime = y.ModifiedDateTime,
                    ModifiedBy = y.ModifiedBy,
                    ModifiedMachine = y.ModifiedMachine,

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

        public bool CheckItemIsExists(int ordersequanceid,int locationid, int itemid)
        {
            return unitOfWork.Tbl_TabOrderDetail.Get(filter: od => od.OrderSeqNumber == ordersequanceid 
                                                                   && od.LocationId == locationid).Any(i=>i.ItemId==itemid);
            
        }

    }
}
