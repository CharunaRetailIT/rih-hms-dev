using Newtonsoft.Json;
using RIT.HMS.HMSOrderTaker.BLL;
using RIT.HMS.HMSOrderTaker.BLL.Masters;
using RIT.HMS.HMSOrderTaker.Domain;
using RIT.HMS.HMSOrderTaker.Domain.DTOs;
using RIT.HMS.HMSOrderTaker.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HMSOrderTaker.Controllers.STOS
{
    public class STOSOrderController : Controller
    {
        BLL_Department _department = null;
        BLL_Categories _category = null;
        BLL_Product _product = null;
        BLL_Locations _blllocations = null;
        BLL_Tables _blltables = null;
        svcTabOrderHeader _taborderheader = null;
        svcTabOrderDetails _taborderdetails = null;
        public STOSOrderController()
        {
            _department = new BLL_Department();
            _category = new BLL_Categories();
            _product = new BLL_Product();
            _blllocations = new BLL_Locations();
            _blltables = new BLL_Tables();
            _taborderheader = new svcTabOrderHeader();
            _taborderdetails = new svcTabOrderDetails();

        }
        public ActionResult Index()
        {
            return View();
        }

        #region BookTblPage
        [AllowAnonymous]
        //call from stos login
        public ActionResult BookTable()
        {
            vmTabOrderHeader taborderheader = new vmTabOrderHeader();
            taborderheader.Tables = _blltables.GetActiveTablesByCompanyIdAndLocationId(                                                   
                                                        Convert.ToInt32(Session["LocationId"])
                                                        ).ToList();
            // return View();
            return PartialView("~/Views/STOSOrder/TabView/BookTable.cshtml",taborderheader);
        }
        [AllowAnonymous]
        [HttpPost]
        //call from booktabl select box change 
        public JsonResult GetTblOrder(int tblid)
        {
            try
            {
                var orderhead = _taborderheader.GetActiveOrderByLocationTableId(Convert.ToInt32(Session["LocationId"]), tblid);
                return Json(new { result = orderhead, isRedirect = false });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        //call from booktabl cansel btn
        public JsonResult CanselTblOrder(int locationid, int orderseqid)
        {
            try
            {
                //var ListCountByAction = objexamine.prescriptions.Where(act => act.ActionId == action.ActionId).ToList();
                var orderhead = _taborderheader.CanselActiveOrderByLocationTableId(locationid, orderseqid);

                if (orderhead != null)
                {
                    //if (Session["TabOrderHead"] != null)
                    //{
                        //List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)orderhead;
                        //li.RemoveAll(x => x.LocationId == locationid && x.OrderSeqNumber == orderseqid);
                        //Session["TabOrderHead"] = li;

                        //#region ItemCount
                        //if (Session["count"] != null)
                        //{
                        //    Session["count"] = Convert.ToInt32(Session["count"]) - 1;
                        //}
                        //else
                        //{
                        //    Session["count"] = 1;
                        //}
                        //#endregion

                        return Json(new { result = orderhead, isRedirect = false });
                   // }
                    //else
                    //{
                    //    return Json(new
                    //    {
                    //        redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                    //        isRedirect = true
                    //    });
                    //}

                }
                else
                {
                    return Json(new
                    {
                        redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                        isRedirect = true
                    });
                }


            }
            catch (Exception ex)
            {
                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }
        }
        #endregion

        [AllowAnonymous]
        //Before Order items
        //Call from book tbl book my tbl btn 

       // public ActionResult SaveTabOrderHead(int locationid, int tblid)
        public ActionResult SaveTabOrderHead(int tblid)
        {
            try
            {
                vmTabOrderHeader orderheader = new vmTabOrderHeader()
                {
                    // LocationId = locationid,
                    LocationId = Convert.ToInt32(Session["LocationId"]),
                    TableId = tblid,

                };

                var result = _taborderheader.SaveTableHead(orderheader);
                if (result != null)
                {
                    Session["CurrentOrderLoctionId"] = result.LocationId;
                    Session["CurrentOrderOrderSeqId"] = result.OrderSeqNumber;

                    if (Session["TabOrderHead"] == null)
                    {
                        List<vmTabOrderHeader> li = new List<vmTabOrderHeader>();

                        li.Add(result);
                        Session["TabOrderHead"] = null;

                        Session["TabOrderHead"] = li;
                        ViewBag.cart = li.Count();
                        Session["count"] = 1;


                    }
                    else
                    {
                        List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];
                        li.Add(result);
                        Session["TabOrderHead"] = li;
                        ViewBag.cart = li.Count();

                        #region ItemCount

                        if (Session["count"] != null)
                        {
                            Session["count"] = Convert.ToInt32(Session["count"]) - 1;
                        }
                        else
                        {
                            Session["count"] = 1;
                        }

                        #endregion Item Count 

                    }
                    return Json(new { result = result, isRedirect = false });
                }
                else
                {

                    return Json(new
                    {
                        redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                        isRedirect = true
                    });

                }

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }
        }

        [AllowAnonymous]
        //Call from booktable book my tbl btn 
        public ActionResult ProductDepartment()
        {
            List<DTO_Department> vmdepartmentlist = new List<DTO_Department>();
            var Department = _department.GetActiveDepartmentsByLocationId(Convert.ToInt32(Session["LocationId"]));
            var result = Department.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 2).Select(x => x.Select(v => v.Value).ToList()).ToList();
            if (result != null)
            {
                foreach (var item in result)
                {
                    DTO_Department vmdepartment = new DTO_Department();
                    vmdepartment.DepartmentList = item;
                    vmdepartmentlist.Add(vmdepartment);
                }

            }
            //  return View("~/Views/STOSOrder/ProductDepartment.cshtml", vmdepartmentlist

             return PartialView("~/Views/STOSOrder/ProductDepartment.cshtml", vmdepartmentlist);
            //return PartialView("~/Views/STOSOrder/TabView/ProductDepartment.cshtml", vmdepartmentlist);
           

        }

        [AllowAnonymous]
        //Call from productdeprtment go btn 
        public ActionResult ProductCategory(string deptid)
        {
            
            int depid = 0;

            if (deptid != null && deptid != "")
            {
                depid = Convert.ToInt32(deptid);
            }

            List<DTO_Category> vmcategorylist = new List<DTO_Category>();
            var category = _category.GetActiveCategoriesByDepartmentIdAndLocationId(depid, Convert.ToInt32(Session["LocationId"]));
            var result = category.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 2).Select(x => x.Select(v => v.Value).ToList()).ToList();
            if (result != null)
            {
                foreach (var item in result)
                {
                    DTO_Category vmcategory = new DTO_Category();
                    vmcategory.CategoryList = item;
                    vmcategorylist.Add(vmcategory);
                }

            }
            return View("~/Views/STOSOrder/ProductCategory.cshtml",vmcategorylist);

        }

        [AllowAnonymous]
        //Call from MenuList grid btn 
        public ActionResult MenuGrid(string categoryid, string departmentid)
        {
            int deptid = 0;
            int catid = 0;

            if (categoryid != null && departmentid != null)
            {
                deptid = Convert.ToInt32(departmentid);
                catid = Convert.ToInt32(categoryid);

                ViewBag.Deptid = deptid;
                ViewBag.Catid = catid;
            }


            List<DTO_Product> vmproductlist = new List<DTO_Product>();

            var ProductList = _product.GetProductsByDeptCatId(deptid, catid);


            var result = ProductList.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 6).Select(x => x.Select(v => v.Value).ToList()).ToList();
            if (result != null)
            {
                foreach (var item in result)
                {
                    DTO_Product vmproduct = new DTO_Product();
                    vmproduct.ProductList = item;
                    vmproductlist.Add(vmproduct);
                }
            }
            return View("~/Views/STOSOrder/MenuGrid.cshtml",vmproductlist);
            //return View("~/Views/STOS/MenuItem/MenuGrid.cshtml", vmproductlist);
        }


        [AllowAnonymous]
        //Call from productcategory go btn 
        public ActionResult MenuList(string categoryid, string departmentid)
        {
            int deptid = 0;
            int catid = 0;
            if (categoryid != null && departmentid != null)
            {
                deptid = Convert.ToInt32(departmentid);
                catid = Convert.ToInt32(categoryid);

                ViewBag.Deptid = deptid;
                ViewBag.Catid = catid;
            }


            List<DTO_Product> vmproductlist = new List<DTO_Product>();
            var ProductList = _product.GetProductsByDeptCatId(deptid, catid);
            var result = ProductList.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 6).Select(x => x.Select(v => v.Value).ToList()).ToList();
            if (result != null)
            {
                foreach (var item in result)
                {
                    DTO_Product vmproduct = new DTO_Product();
                    vmproduct.ProductList = item;
                    vmproductlist.Add(vmproduct);
                }
            }
            return View("~/Views/STOSOrder/MenuList.cshtml",vmproductlist);
            //return View("~/Views/STOS/MenuItem/Menu.cshtml", vmproductlist);
        }


        public ActionResult ServingUnits(int productid,int categoryid, int departmentid)
        {
           
            DTO_Product vmproductlist = new DTO_Product();
            var ProductList = _product.GetProductServingUnitsByProductIdLocationID(productid, Convert.ToInt32(Session["LocationId"]));            
            ViewBag.Deptid = departmentid;
            ViewBag.Catid = categoryid;
           
            vmproductlist.ProductList = ProductList;
            return View("~/Views/STOSOrder/ServingUnits.cshtml", vmproductlist);
           
        }


        [AllowAnonymous]
        //Call from booktable data grid add btn 
        public ActionResult AddItem(string LocationId, string OrderSeqId)
        {
            Session["CurrentOrderLoctionId"] = LocationId;
            Session["CurrentOrderOrderSeqId"] = OrderSeqId;

            List<DTO_Department> vmdepartmentlist = new List<DTO_Department>();
            var Department = _department.GetActiveDepartmentsByLocationId(Convert.ToInt32(Session["LocationId"]));
            var result = Department.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 2).Select(x => x.Select(v => v.Value).ToList()).ToList();
            if (result != null)
            {
                foreach (var item in result)
                {
                    DTO_Department vmdepartment = new DTO_Department();
                    vmdepartment.DepartmentList = item;
                    vmdepartmentlist.Add(vmdepartment);
                }

            }
            return View("~/Views/STOSOrder/ProductDepartment.cshtml", vmdepartmentlist);

        }
        [AllowAnonymous]
        //Call from layout cart btn 
        public ActionResult Cart()
        {
            try
            {

                if (Session["CurrentOrderLoctionId"] != null && Session["CurrentOrderOrderSeqId"] != null)
                {
                    int currentorderlocation = Convert.ToInt32(Session["CurrentOrderLoctionId"]);
                    int currentorderseqid = Convert.ToInt32(Session["CurrentOrderOrderSeqId"]);


                    if (Session["TabOrderHead"] != null)
                    {
                        List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];

                        var ListCountByAction = li.Where(act => act.LocationId == currentorderlocation && act.OrderSeqNumber == currentorderseqid).FirstOrDefault();
                        ListCountByAction = _taborderheader.GetOrderHeaderBySequanceIdLocationId(currentorderseqid, currentorderlocation);
                        return View(ListCountByAction);
                    }
                    else
                    {
                        return View("~/Views/STOSOrder/Cart.cshtml");
                    }

                }
                else
                {
                    return View("~/Views/STOSOrder/Cart.cshtml");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AllowAnonymous]
        //Call from booktable view 
        public ActionResult AddToCart(int locationid, int orderno)
        {
            try
            {

                int currentorderlocation = locationid;
                int currentorderseqid = orderno;
                if (Session["TabOrderHead"] != null)
                {
                    List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];
                    var ListCountByAction = li.Where(act => act.LocationId == currentorderlocation &&
                                                                                act.OrderSeqNumber == currentorderseqid
                                                                                ).FirstOrDefault();

                    return View("~/Views/STOSOrder/Cart.cshtml", ListCountByAction);
                }
                else
                {
                    return View("~/Views/STOSOrder/Cart.cshtml", _taborderheader.GetOrderHeaderBySequanceIdLocationId(orderno, locationid));
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AllowAnonymous]
        [HttpPost]
        //Call from MenuList view 
        public JsonResult AddItemToCart(int productid, int qty,int servingunitid)
        {
            try
            {

                // var product = _product.GetProductsByProductId(productid);
                var product = _product.GetProductsByProductIdServingUnitId(productid, servingunitid, Convert.ToInt32(Session["CompanyId"]), Convert.ToInt32(Session["LocationId"]));

                List<vmTabOrderDetail> orderdetailslist = new List<vmTabOrderDetail>();

                if (Session["CurrentOrderLoctionId"] != null && Session["CurrentOrderOrderSeqId"] != null)
                {
                    int currentorderlocation = Convert.ToInt32(Session["CurrentOrderLoctionId"]);
                    int currentorderseqid = Convert.ToInt32(Session["CurrentOrderOrderSeqId"]);
                    //int orderdetailscount = 0;
                    if (Session["TabOrderHead"] != null)
                    {
                        List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];

                        var taborderheader = _taborderheader.GetActiveOrderByLocationOrderSeqId(currentorderlocation, 
                                                                                                currentorderseqid);

                        if (taborderheader != null)
                        {
                            vmTabOrderDetail orderdetails = new vmTabOrderDetail()
                            {
                                LocationId = currentorderlocation,
                                OrderSeqNumber = currentorderseqid,
                                //ItemSeqId = orderdetailscount,
                                ItemId = product.ProductId,
                                ItemCode = product.ProductCode,
                                ItemNameOnBill = product.ProductName,
                                ItemName = product.ProductName,
                                TableId = taborderheader.TableId,
                                TableCode = taborderheader.TableCode,
                                ItemCostPrice = product.CostPrice,
                                ItemSellingPrice = product.SellingPrice,
                                ItemQty = qty,
                                OrderedItemStatus = (int)RIT.HMS.HMSOrderTaker.Domain.Common.Enums.enumTabOrderDetails.ActiveSession,
                            };

                            foreach (var item in li)
                            {
                                if (item.LocationId == currentorderlocation && item.OrderSeqNumber == currentorderseqid)
                                {
                                    if (item.TabOrderDetailsList == null)
                                    {
                                        orderdetailslist.Add(orderdetails);
                                        item.TabOrderDetailsList = orderdetailslist;
                                    }
                                    else
                                    {
                                        var Itemdetails = item.TabOrderDetailsList.Where(act => act.LocationId == currentorderlocation && act.OrderSeqNumber == currentorderseqid && act.ItemId == productid).FirstOrDefault();
                                        if (Itemdetails != null)
                                        {
                                            Itemdetails.ItemQty = Itemdetails.ItemQty + qty;
                                        }
                                        else
                                        {
                                            //ord 01
                                            item.TabOrderDetailsList.Add(orderdetails);
                                        }
                                        //foreach (var itemlist in item.TabOrderDetailsList)
                                        //{
                                        //    if (itemlist.ItemId == productid)
                                        //    {
                                        //        itemlist.ItemQty = itemlist.ItemQty + qty;
                                        //    }
                                        //}

                                    }
                                }

                                
                            }

                            //var orderheader = li.Where(y => y.LocationId == currentorderlocation && y.OrderSeqNumber == currentorderseqid).FirstOrDefault();
                            //orderheader.TabOrderDetailsList.Add(orderdetails);
                            //li.RemoveAll(x => x.LocationId == currentorderlocation && x.OrderSeqNumber == currentorderseqid);
                            //li.Add(orderheader);

                            Session["TabOrderHead"] = li;
                            var vli = li;
                            var sa = Session["TabOrderHead"];
                            var updateresut = _taborderheader.UpdateTabOrderHeadIteamQty(currentorderlocation,currentorderseqid, qty);

                            foreach (var l in li)
                            {
                                foreach (var d in l.TabOrderDetailsList)
                                {
                                    if (!_taborderdetails.CheckItemIsExists(currentorderseqid, currentorderlocation,d.ItemId))
                                    {
                                        d.ItemSeqId = l.OrderSeqNumber;
                                        var detail = _taborderdetails.SaveTableDetails(d);
                                    }
                                    
                                }
                            }

                            #region ItemCount
                            if (Session["count"] != null)
                            {
                                Session["count"] = Convert.ToInt32(Session["count"]) - 1;
                            }
                            else
                            {
                                Session["count"] = 1;
                            }
                            #endregion
                            return Json(new { result = true, isRedirect = false });
                        }
                        else
                        {
                            return Json(new { result = false, isRedirect = false });
                        }
                    }
                    else
                    {
                        List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];
                        var tbsession = Session["TabOrderHead"];

                        var taborderheader = _taborderheader.GetActiveOrderByLocationOrderSeqId(currentorderlocation, currentorderseqid);
                        // taborderheader.TabOrderDetailsList = _taborderdetails.GetActiveOrderItemByLocationOrderSeqId(currentorderlocation,currentorderseqid);
                        taborderheader.TabOrderDetailsList = new List<vmTabOrderDetail>();
                        if (taborderheader != null)
                        {
                            vmTabOrderDetail orderdetails = new vmTabOrderDetail()
                            {
                                LocationId = currentorderlocation,
                                OrderSeqNumber = currentorderseqid,
                                //ItemSeqId = orderdetailscount,
                                ItemId = product.ProductId,
                                ItemCode = product.ProductCode,
                                ItemNameOnBill = product.ProductName,
                                ItemName = product.ProductName,
                                TableId = taborderheader.TableId,
                                TableCode = taborderheader.TableCode,
                                ItemCostPrice = product.CostPrice,
                                ItemSellingPrice = product.SellingPrice,
                                ItemQty = qty,
                                OrderedItemStatus = (int)RIT.HMS.HMSOrderTaker.Domain.Common.Enums.enumTabOrderDetails.ActiveSession
                            };

                            if (taborderheader.TabOrderDetailsList != null)
                            {
                                orderdetailslist.Add(orderdetails);
                                taborderheader.TabOrderDetailsList = orderdetailslist;
                            }
                            else
                            {
                                //ord02
                                taborderheader.TabOrderDetailsList.Add(orderdetails);
                            }
                            Session["TabOrderHead"] = li;
                            var updateresut = _taborderheader.UpdateTabOrderHeadIteamQty(currentorderlocation, 
                                                                                         currentorderseqid, qty);
                           // foreach (var l in li)
                           // {
                                foreach (var d in taborderheader.TabOrderDetailsList)
                                {
                                    if (!_taborderdetails.CheckItemIsExists(currentorderseqid, currentorderlocation, d.ItemId))
                                    {
                                        d.ItemSeqId = taborderheader.OrderSeqNumber;
                                        var detail = _taborderdetails.SaveTableDetails(d);
                                    }
                                }
                           // }
                            #region ItemCount
                            if (Session["count"] != null)
                            {
                                Session["count"] = Convert.ToInt32(Session["count"]) - 1;
                            }
                            else
                            {
                                Session["count"] = 1;
                            }
                            #endregion
                            return Json(new { result = true, isRedirect = false });
                        }
                        else
                        {
                            return Json(new { result = false, isRedirect = false });
                        }
                    }
                }
                else
                {
                    return Json(new { result = false, isRedirect = false });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        //Call from Cart  Plus Min Btn 
        public JsonResult UpdateSession(int locationid, int ordernumber, int itemnumber, bool addormin)
        {
            try
            {
                if (addormin == true)
                {
                    if (Session["TabOrderHead"] != null)
                    {
                        List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];

                        foreach (var item in li)
                        {
                            if (item.LocationId == locationid && item.OrderSeqNumber == ordernumber)
                            {
                                foreach (var itemdetails in item.TabOrderDetailsList)
                                {
                                    if (itemdetails.ItemId == itemnumber)
                                    {
                                        itemdetails.ItemQty = itemdetails.ItemQty + 1;
                                    }
                                }
                            }
                        }
                        Session["TabOrderHead"] = li;
                        var updateresut = _taborderheader.UpdateTabOrderHeadIteamQty(locationid, ordernumber, 1);

                        return Json(new { result = li, isRedirect = false });
                    }
                    else
                    {
                        return Json(new
                        {
                            redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                            isRedirect = true
                        });
                    }
                }
                else
                {
                    if (Session["TabOrderHead"] != null)
                    {
                        List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];

                        foreach (var item in li)
                        {
                            if (item.LocationId == locationid && item.OrderSeqNumber == ordernumber)
                            {
                                foreach (var itemdetails in item.TabOrderDetailsList)
                                {
                                    if (itemdetails.ItemId == itemnumber)
                                    {
                                        itemdetails.ItemQty = itemdetails.ItemQty - 1;
                                    }
                                }
                            }
                        }
                        Session["TabOrderHead"] = li;
                        var updateresut = _taborderheader.UpdateTabOrderHeadIteamQty(locationid, ordernumber, -1);
                        return Json(new { result = li, isRedirect = false });
                    }
                    else
                    {
                        return Json(new
                        {
                            redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                            isRedirect = true
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }
        }
        [AllowAnonymous]
        [HttpPost]
        //Call from MenuList  Plus Min Btn - test
        public JsonResult UpdateItemQty(int itemid, bool addormin)
        {
            try
            {
                var product = _product.GetProductsByProductId(itemid);
                List<vmTabOrderDetail> orderdetailslist = new List<vmTabOrderDetail>();
                if (Session["CurrentOrderLoctionId"] != null && Session["CurrentOrderOrderSeqId"] != null)
                {
                    int count = 0;
                    int orderdetailscount = 0;
                    int currentorderlocation = Convert.ToInt32(Session["CurrentOrderLoctionId"]);
                    int currentorderseqid = Convert.ToInt32(Session["CurrentOrderOrderSeqId"]);
                    if (addormin == true)
                    {
                        if (Session["TabOrderHead"] != null)
                        {
                            List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];

                            foreach (var item in li)
                            {
                                if (item.LocationId == currentorderlocation && item.OrderSeqNumber == currentorderseqid)
                                {
                                    if (item.TabOrderDetailsList != null)
                                    {
                                        foreach (var itemdetails in item.TabOrderDetailsList)
                                        {
                                            if (itemdetails.ItemSeqId == itemid)
                                            {
                                                itemdetails.ItemQty = itemdetails.ItemQty + 1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        count = count + 1;
                                        #region addItemtocart
                                        List<vmTabOrderHeader> lisession = (List<vmTabOrderHeader>)Session["TabOrderHead"];
                                        var orderheader = lisession.Where(y => y.LocationId == currentorderlocation && y.OrderSeqNumber == currentorderseqid).FirstOrDefault();
                                        if (orderheader != null)
                                        {
                                            if (orderheader.TabOrderDetailsList == null)
                                            {
                                                orderdetailscount = 1;
                                            }
                                            else
                                            {
                                                orderdetailscount = orderheader.TabOrderDetailsList.Count + 1;
                                            }
                                        }

                                        var taborderheader = _taborderheader.GetActiveOrderByLocationOrderSeqId(currentorderlocation, currentorderseqid);
                                        if (taborderheader != null)
                                        {
                                            vmTabOrderDetail orderdetails = new vmTabOrderDetail()
                                            {
                                                LocationId = currentorderlocation,
                                                OrderSeqNumber = currentorderseqid,
                                                ItemSeqId = orderdetailscount,
                                                ItemId = product.ProductId,
                                                ItemCode = product.ProductCode,
                                                ItemNameOnBill = product.ProductName,
                                                ItemName = product.ProductName,
                                                TableId = taborderheader.TableId,
                                                TableCode = taborderheader.TableCode,
                                                ItemCostPrice = product.CostPrice,
                                                ItemSellingPrice = product.SellingPrice,
                                                ItemQty = 1,

                                            };

                                            foreach (var itemlist in li)
                                            {
                                                if (itemlist.LocationId == currentorderlocation && itemlist.OrderSeqNumber == currentorderseqid)
                                                {
                                                    if (itemlist.TabOrderDetailsList == null)
                                                    {
                                                        orderdetailslist.Add(orderdetails);
                                                        itemlist.TabOrderDetailsList = orderdetailslist;
                                                    }
                                                    else
                                                    {

                                                        itemlist.TabOrderDetailsList.Add(orderdetails);
                                                    }
                                                }
                                            }

                                            //var orderheader = li.Where(y => y.LocationId == currentorderlocation && y.OrderSeqNumber == currentorderseqid).FirstOrDefault();
                                            //orderheader.TabOrderDetailsList.Add(orderdetails);
                                            //li.RemoveAll(x => x.LocationId == currentorderlocation && x.OrderSeqNumber == currentorderseqid);
                                            //li.Add(orderheader);

                                            Session["TabOrderHead"] = li;

                                            #region ItemCount
                                            if (Session["count"] != null)
                                            {
                                                Session["count"] = Convert.ToInt32(Session["count"]) - 1;
                                            }
                                            else
                                            {
                                                Session["count"] = 1;
                                            }
                                            #endregion
                                        }
                                        else
                                        {

                                        }
                                        #endregion
                                    }
                                }
                            }

                            if (count == 0)
                            {
                                #region addItemtocart
                                List<vmTabOrderHeader> lisession = (List<vmTabOrderHeader>)Session["TabOrderHead"];
                                var orderheader = lisession.Where(y => y.LocationId == currentorderlocation && y.OrderSeqNumber == currentorderseqid).FirstOrDefault();
                                if (orderheader != null)
                                {
                                    if (orderheader.TabOrderDetailsList == null)
                                    {
                                        orderdetailscount = 1;
                                    }
                                    else
                                    {
                                        orderdetailscount = orderheader.TabOrderDetailsList.Count + 1;
                                    }
                                }
                                var taborderheader = _taborderheader.GetActiveOrderByLocationOrderSeqId(currentorderlocation, currentorderseqid);
                                if (taborderheader != null)
                                {
                                    vmTabOrderDetail orderdetails = new vmTabOrderDetail()
                                    {
                                        LocationId = currentorderlocation,
                                        OrderSeqNumber = currentorderseqid,
                                        ItemSeqId = orderdetailscount,
                                        ItemId = product.ProductId,
                                        ItemCode = product.ProductCode,
                                        ItemNameOnBill = product.ProductName,
                                        ItemName = product.ProductName,
                                        TableId = taborderheader.TableId,
                                        TableCode = taborderheader.TableCode,
                                        ItemCostPrice = product.CostPrice,
                                        ItemSellingPrice = product.SellingPrice,
                                        ItemQty = 1,

                                    };

                                    foreach (var item in li)
                                    {
                                        if (item.LocationId == currentorderlocation && item.OrderSeqNumber == currentorderseqid)
                                        {
                                            if (item.TabOrderDetailsList == null)
                                            {
                                                orderdetailslist.Add(orderdetails);
                                                item.TabOrderDetailsList = orderdetailslist;
                                            }
                                            else
                                            {

                                                item.TabOrderDetailsList.Add(orderdetails);
                                            }
                                        }
                                    }

                                    //var orderheader = li.Where(y => y.LocationId == currentorderlocation && y.OrderSeqNumber == currentorderseqid).FirstOrDefault();
                                    //orderheader.TabOrderDetailsList.Add(orderdetails);
                                    //li.RemoveAll(x => x.LocationId == currentorderlocation && x.OrderSeqNumber == currentorderseqid);
                                    //li.Add(orderheader);

                                    Session["TabOrderHead"] = li;

                                    #region ItemCount
                                    if (Session["count"] != null)
                                    {
                                        Session["count"] = Convert.ToInt32(Session["count"]) - 1;
                                    }
                                    else
                                    {
                                        Session["count"] = 1;
                                    }
                                    #endregion



                                }
                                else
                                {

                                }
                                #endregion
                            }

                            Session["TabOrderHead"] = li;
                            return Json(new { result = li, isRedirect = false });
                        }
                        else
                        {
                            return Json(new
                            {
                                redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                                isRedirect = true
                            });
                        }
                    }
                    else
                    {
                        if (Session["TabOrderHead"] != null)
                        {
                            List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];

                            foreach (var item in li)
                            {
                                if (item.LocationId == currentorderlocation && item.OrderSeqNumber == currentorderseqid)
                                {
                                    if (item.TabOrderDetailsList != null)
                                    {
                                        foreach (var itemdetails in item.TabOrderDetailsList)
                                        {
                                            if (itemdetails.ItemSeqId == itemid)
                                            {
                                                itemdetails.ItemQty = itemdetails.ItemQty - 1;
                                            }
                                        }
                                    }

                                }
                            }
                            Session["TabOrderHead"] = li;
                            return Json(new { result = li, isRedirect = false });
                        }
                        else
                        {
                            return Json(new
                            {
                                redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                                isRedirect = true
                            });
                        }
                    }

                }
                else
                {
                    return Json(new
                    {
                        redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                        isRedirect = true
                    });
                }

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        //Call from Cart  remove Btn 
        public JsonResult RemoveCartItem(int locationid, int ordernumber, int itemid)
        {
            try
            {

                if (Session["TabOrderHead"] != null)
                {
                    List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];

                    foreach (var item in li)
                    {
                        if (item.LocationId == locationid && item.OrderSeqNumber == ordernumber)
                        {
                            item.TabOrderDetailsList.RemoveAll(x => x.ItemId == itemid);

                        }
                    }


                    Session["TabOrderHead"] = li;

                    #region ItemCount
                    if (Session["count"] != null)
                    {
                        Session["count"] = Convert.ToInt32(Session["count"]) - 1;
                    }
                    else
                    {
                        Session["count"] = 1;
                    }
                    #endregion

                    return Json(new { result = li, isRedirect = false });
                }
                else
                {
                    return Json(new
                    {
                        redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                        isRedirect = true
                    });
                }

            }
            catch (Exception ex)
            {

                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        //Call from menu list  remove Btn 
        public JsonResult RemoveSessionItem(int itemid)
        {
            try
            {
                if (Session["CurrentOrderLoctionId"] != null && Session["CurrentOrderOrderSeqId"] != null)
                {

                    int currentorderlocation = Convert.ToInt32(Session["CurrentOrderLoctionId"]);
                    int currentorderseqid = Convert.ToInt32(Session["CurrentOrderOrderSeqId"]);

                    if (Session["TabOrderHead"] != null)
                    {
                        List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];

                        foreach (var item in li)
                        {
                            if (item.LocationId == currentorderlocation && item.OrderSeqNumber == currentorderseqid)
                            {
                                item.TabOrderDetailsList.RemoveAll(x => x.ItemId == itemid);

                            }
                        }


                        Session["TabOrderHead"] = li;

                        #region ItemCount
                        if (Session["count"] != null)
                        {
                            Session["count"] = Convert.ToInt32(Session["count"]) - 1;
                        }
                        else
                        {
                            Session["count"] = 1;
                        }
                        #endregion

                        return Json(new { result = li, isRedirect = false });
                    }
                    else
                    {
                        return Json(new
                        {
                            redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                            isRedirect = true
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                        isRedirect = true
                    });
                }

            }
            catch (Exception ex)
            {

                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        //Call from layout cart btn 
        public JsonResult LordCart()
        {
            try
            {

                if (Session["CurrentOrderLoctionId"] != null && Session["CurrentOrderOrderSeqId"] != null)
                {
                    int currentorderlocation = Convert.ToInt32(Session["CurrentOrderLoctionId"]);
                    int currentorderseqid = Convert.ToInt32(Session["CurrentOrderOrderSeqId"]);


                    if (Session["TabOrderHead"] != null)
                    {
                        List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];

                        var result = li.Where(act => act.LocationId == currentorderlocation && act.OrderSeqNumber == currentorderseqid).FirstOrDefault();
                        if (result != null)
                        {

                            return Json(new { result = result, isRedirect = false, itemstatus = (int)RIT.HMS.HMSOrderTaker.Domain.Common.Enums.enumTabOrderDetails.ActiveSession });

                        }
                        else
                        {
                            var orderhead = _taborderheader.GetActiveOrderByLocationOrderSeqId(currentorderlocation, currentorderseqid);

                            if (orderhead != null)
                            {
                                var orderdetails = _taborderdetails.GetActiveOrderItemByLocationOrderSeqId(currentorderlocation, currentorderseqid);
                                if (orderdetails != null)
                                {
                                    orderhead.TabOrderDetailsList = orderdetails;
                                    return Json(new { result = orderhead, isRedirect = false });
                                }
                                else
                                {
                                    return Json(new
                                    {
                                        redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                                        isRedirect = true
                                    });
                                }

                            }
                            else
                            {
                                return Json(new
                                {
                                    redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                                    isRedirect = true
                                });
                            }
                        }

                    }
                    else
                    {
                        var orderhead = _taborderheader.GetActiveOrderByLocationOrderSeqId(currentorderlocation, currentorderseqid);
                        if (orderhead != null)
                        {
                            var orderdetails = _taborderdetails.GetActiveOrderItemByLocationOrderSeqId(currentorderlocation, currentorderseqid);
                            if (orderdetails != null)
                            {
                                orderhead.TabOrderDetailsList = orderdetails;
                                return Json(new { result = orderhead, isRedirect = false });
                            }
                            else
                            {
                                return Json(new
                                {
                                    redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                                    isRedirect = true
                                });
                            }

                        }
                        else
                        {
                            return Json(new
                            {
                                redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                                isRedirect = true
                            });
                        }

                    }

                }
                else
                {
                    return Json(new
                    {
                        redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                        isRedirect = true
                    });
                }

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }


        }


        [AllowAnonymous]
        [HttpPost]
        //Call from layout cart btn 
        public JsonResult LordStewardCart()
        {
            try
            {

                if (Session["CurrentOrderLoctionId"] != null && Session["CurrentOrderOrderSeqId"] != null)
                {
                    int currentorderlocation = Convert.ToInt32(Session["CurrentOrderLoctionId"]);
                    int currentorderseqid = Convert.ToInt32(Session["CurrentOrderOrderSeqId"]);


                    if (Session["TabOrderHead"] != null)
                    {
                        List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];

                        var result = li.Where(act => act.LocationId == currentorderlocation && act.OrderSeqNumber == currentorderseqid).FirstOrDefault();
                        if (result == null)
                        {
                            var orderhead = _taborderheader.GetActiveOrderByLocationOrderSeqId(currentorderlocation, currentorderseqid);

                            if (orderhead != null)
                            {
                                var orderdetails = _taborderdetails.GetActiveOrderItemByLocationOrderSeqId(currentorderlocation, currentorderseqid);
                                if (orderdetails != null)
                                {
                                    orderhead.TabOrderDetailsList = orderdetails;
                                    return Json(new { result = orderhead, isRedirect = false, itemstatus = (int)RIT.HMS.HMSOrderTaker.Domain.Common.Enums.enumTabOrderDetails.PendingToSterwed });
                                }
                                else
                                {
                                    return Json(new
                                    {
                                        redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                                        isRedirect = true
                                    });
                                }

                            }
                            else
                            {
                                return Json(new
                                {
                                    redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                                    isRedirect = true
                                });
                            }
                        }
                        else
                        {
                            return Json(new { result = result, isRedirect = false, itemstatus = (int)RIT.HMS.HMSOrderTaker.Domain.Common.Enums.enumTabOrderDetails.PendingToSterwed });
                        }


                    }
                    else
                    {
                        var orderhead = _taborderheader.GetActiveOrderByLocationOrderSeqId(currentorderlocation, currentorderseqid);

                        if (orderhead != null)
                        {
                            var orderdetails = _taborderdetails.GetActiveOrderItemByLocationOrderSeqId(currentorderlocation, currentorderseqid);
                            if (orderdetails != null)
                            {
                                orderhead.TabOrderDetailsList = orderdetails;
                                return Json(new { result = orderhead, isRedirect = false });
                            }
                            else
                            {
                                return Json(new
                                {
                                    redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                                    isRedirect = true
                                });
                            }

                        }
                        else
                        {
                            return Json(new
                            {
                                redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                                isRedirect = true
                            });
                        }


                    }

                }
                else
                {
                    return Json(new
                    {
                        redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                        isRedirect = true
                    });
                }

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }


        }
        [AllowAnonymous]
        [HttpPost]
        //Call from layout cart btn 
        public JsonResult CheckOutItemByCustomer_Del()
        {
            try
            {
                if (Session["CurrentOrderLoctionId"] != null && Session["CurrentOrderOrderSeqId"] != null)
                {
                    if (Session["TabOrderHead"] != null)
                    {
                        int currentorderlocation = Convert.ToInt32(Session["CurrentOrderLoctionId"]);
                        int currentorderseqid = Convert.ToInt32(Session["CurrentOrderOrderSeqId"]);

                        List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];
                        var result = li.Where(act => act.LocationId == currentorderlocation && act.OrderSeqNumber == currentorderseqid).FirstOrDefault();

                        if (result != null)
                        {
                            if (result.TabOrderDetailsList != null)
                            {
                                foreach (var item in result.TabOrderDetailsList)
                                {
                                    if (item.OrderedItemStatus == (int)RIT.HMS.HMSOrderTaker.Domain.Common.Enums.enumTabOrderDetails.ActiveSession)
                                    {
                                        item.OrderedItemStatus = (int)RIT.HMS.HMSOrderTaker.Domain.Common.Enums.enumTabOrderDetails.PendingToSterwed;
                                    }
                                }
                                Session["TabOrderHead"] = li;
                                return Json(new { result = true, isRedirect = false });
                            }
                            else
                            {
                                return Json(new { result = false, isRedirect = false });
                            }
                        }
                        else
                        {
                            return Json(new { result = false, isRedirect = false });
                        }
                    }
                    else
                    {
                        return Json(new { result = false, isRedirect = false });
                    }
                }
                else
                {
                    return Json(new { result = false, isRedirect = false });
                }
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }

        }
        [AllowAnonymous]
        [HttpPost]
        //Call from layout cart btn 
        public JsonResult CheckOutItemByCustomer()
        {
            try
            {
                if (Session["CurrentOrderLoctionId"] != null && Session["CurrentOrderOrderSeqId"] != null)
                {
                    if (Session["TabOrderHead"] != null)
                    {
                        int currentorderlocation = Convert.ToInt32(Session["CurrentOrderLoctionId"]);
                        int currentorderseqid = Convert.ToInt32(Session["CurrentOrderOrderSeqId"]);
                        vmTabOrderHeader DisplayItemHead = new vmTabOrderHeader();
                        List<vmTabOrderDetail> DisplayItemDetails = new List<vmTabOrderDetail>();
                        List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];
                        var result = li.Where(act => act.LocationId == currentorderlocation && act.OrderSeqNumber == currentorderseqid).FirstOrDefault();

                        if (result != null)
                        {
                            if (result.TabOrderDetailsList != null)
                            {
                                foreach (var item in result.TabOrderDetailsList)
                                {
                                    if (item.OrderedItemStatus == (int)RIT.HMS.HMSOrderTaker.Domain.Common.Enums.enumTabOrderDetails.ActiveSession)
                                    {
                                        item.OrderedItemStatus = (int)RIT.HMS.HMSOrderTaker.Domain.Common.Enums.enumTabOrderDetails.PendingToSterwed;
                                        DisplayItemDetails.Add(item);
                                    }
                                }
                                DisplayItemHead.TabOrderDetailsList = DisplayItemDetails;
                                Session["TabOrderHead"] = li;
                                return Json(new { result = DisplayItemHead, isRedirect = false });
                            }
                            else
                            {
                                return Json(new { result = DisplayItemHead, isRedirect = false });
                            }
                        }
                        else
                        {
                            return Json(new { result = DisplayItemHead, isRedirect = false });
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            redirectUrl = Url.Action("Index", "Error", new { message = "Error" }),
                            isRedirect = true
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        redirectUrl = Url.Action("Index", "Error", new { message = "Error" }),
                        isRedirect = true
                    });
                }
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }

        }

        [AllowAnonymous]
        [HttpPost]
        //Call from cart continue btn 
        public JsonResult SaveTabOrderDetails()
        {
            try
            {
                if (Session["TabOrderHead"] != null)
                {
                    if (Session["CurrentOrderLoctionId"] != null && Session["CurrentOrderOrderSeqId"] != null)
                    {

                        int currentorderlocation = Convert.ToInt32(Session["CurrentOrderLoctionId"]);
                        int currentorderseqid = Convert.ToInt32(Session["CurrentOrderOrderSeqId"]);

                        List<vmTabOrderHeader> li = (List<vmTabOrderHeader>)Session["TabOrderHead"];
                        var orderdetails = li.Where(act => act.LocationId == currentorderlocation && act.OrderSeqNumber == currentorderseqid).FirstOrDefault();


                        if (orderdetails != null)
                        {
                            if (orderdetails.TabOrderDetailsList != null)
                            {
                                foreach (var item in orderdetails.TabOrderDetailsList)
                                {

                                  //  if (item.OrderedItemStatus == (int)RIT.HMS.HMSOrderTaker.Domain.Common.Enums.enumTabOrderDetails.PendingToSterwed)
                                   // {
                                        item.OrderedItemStatus = (int)RIT.HMS.HMSOrderTaker.Domain.Common.Enums.enumTabOrderDetails.KOTBOT;
                                        var result = _taborderdetails.SaveTableDetails(item);
                                    //}

                                }

                                Session["TabOrderHead"] = li;
                                return Json(new { result = orderdetails, isRedirect = false });
                            }
                            else
                            {
                                //CHECK AGAIN
                                return Json(new { result = orderdetails, isRedirect = false });
                            }

                        }
                        else
                        {
                            //CHECK AGAIN
                            return Json(new { result = orderdetails, isRedirect = false });
                        }


                    }
                    else
                    {
                        return Json(new
                        {
                            redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                            isRedirect = true
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        redirectUrl = Url.Action("Index", "Error", new { message = "error" }),
                        isRedirect = true
                    });
                }

            }
            catch (Exception ex)
            {

                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }
        }


        #region DropDown

        [AllowAnonymous]
        //Call from BookTbl location selectbox 
        public JsonResult GetActiveLocation()
        {
            try
            {
                 var Location = _blllocations.GetActiveLocationsByCompanyId(1);
                 return Json(JsonConvert.SerializeObject(Location, 
                 Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);            
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }
        }
        [AllowAnonymous]
        //Call from BookTbl table selectbox 
        public JsonResult GetActiveTbl(int locationid)
        {
            try
            {

                var Tbl = _blltables.GetActiveTablesByCompanyIdAndLocationId(Convert.ToInt32(Session["LocationId"]));
                return Json(JsonConvert.SerializeObject(Tbl, Formatting.None, new 
                    JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                    JsonRequestBehavior.AllowGet);
               
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    redirectUrl = Url.Action("Index", "Error", new { message = ex.Message }),
                    isRedirect = true
                });
            }
        }

        #endregion

    }
}