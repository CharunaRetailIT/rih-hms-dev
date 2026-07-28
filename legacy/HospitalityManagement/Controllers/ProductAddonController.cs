using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace HospitalityManagement.Controllers
{

    [SessionTimeout]
    [Authorize(Roles = "PrdCreatee")]
    public class ProductAddonController : Controller
    {
        private readonly  BLL_ProductAddon _bllProdutAddons;
        private readonly BLL_Product _bllproduct;
        private readonly BLL_Department _blldepartment;

        public ProductAddonController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _bllProdutAddons = new BLL_ProductAddon(cn);
            _bllproduct = new BLL_Product(cn);
            _blldepartment = new BLL_Department(cn);
        }
     
        public ActionResult Create()     
        {
            return View();
        }

      //  [HttpPost]
        public ActionResult Edit(long id)
        {
          

            var exists = _bllProdutAddons.GetAddonsById(id);
            exists.ModifiedDate = DateTime.Now;
            exists.ModifiedUser = Session["loggeduser"].ToString();
            exists.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var res = _bllproduct.RemoveAddonsbyId(exists);
         
            var addons = _bllProdutAddons.GetProductAddonsByProductId(exists.ProductId);
            
            addons.ToList().ForEach(a =>
            {
                
                a.ProductDesc = _bllproduct.GetProductById(a.ProductId).ProductName;
                a.AddonDesc = _bllproduct.GetProductById(a.ProductAddonId).ProductName;
            });

            return View("ViewProductAddons", addons.ToList());
  
        }

        public ActionResult ApplyChanges(long id, decimal AddonSellingPrice, decimal AddonQuantity)
        {


            var exists = _bllProdutAddons.GetAddonsById(id);
            //exists.DepartmentDesc = _departmentService.GetDepartmentById(exists.DepartmentId).DepartmentName;


            var res = _bllproduct.RemoveAddonsbyId(exists);

            //   ProductAddonService addon = new ProductAddonService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var addons = _bllProdutAddons.GetProductAddons(companyid);

            addons.ToList().ForEach(a =>
            {
                //a.DepartmentDesc = _departmentService.GetDepartmentById(a.DepartmentId).DepartmentName;
                a.ProductDesc = _bllproduct.GetProductById(a.ProductId).ProductName;
                a.AddonDesc = _bllproduct.GetProductById(a.ProductAddonId).ProductName;
            });

            

            return View("ViewProductAddons", addons);


            //exists.ProductDesc = _productService.GetProductById(exists.ProductId).ProductName;
            //exists.AddonDesc = _productService.GetProductById(exists.ProductAddonId).ProductName;
            //return View(exists);
        }
        [HttpPost]  // By Anura
        public ActionResult ApplyChanges(List<Addons> lstAddons)
        {
            long ProductId=0;
            try
            {
               // ApplicationDbContext context = new ApplicationDbContext();
                
                foreach (Addons item in lstAddons)
                {
                    
                    Addons existsAddon = new Addons();
                    existsAddon = _bllProdutAddons.GetAddonsById(item.AddonsId);
                    if (existsAddon!=null)
                    {
                        ProductId = existsAddon.ProductId;
                        existsAddon.AddonSellingPrice = item.AddonSellingPrice;
                        existsAddon.AddonQuantity = item.AddonQuantity;
                        existsAddon.ModifiedDate = DateTime.Now;
                        existsAddon.ModifiedUser = Session["loggeduser"].ToString();
                        existsAddon.CompanyID=  Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                        // context.SaveChanges();
                        _bllProdutAddons.UpdateProductAddons(existsAddon);
                        
                    }

                }
            }
            catch (Exception e)
            {
                
                 
            }
          //  ProductAddonService addon = new ProductAddonService();
            var addons = _bllProdutAddons.GetProductAddonsByProductId(ProductId);

            addons.ToList().ForEach(a =>
            {
                //a.DepartmentDesc = _departmentService.GetDepartmentById(a.DepartmentId).DepartmentName;
                a.ProductDesc = _bllproduct.GetProductById(a.ProductId).ProductName;
                a.AddonDesc = _bllproduct.GetProductById(a.ProductAddonId).ProductName;
                a.ProductAddonCode = _bllproduct.GetAddonsDescById(a.ProductAddonId).ProductCode;
            });

            ViewBag.Message = "1";

            return View("ViewProductAddons", addons.ToList());


            //exists.ProductDesc = _productService.GetProductById(exists.ProductId).ProductName;
            //exists.AddonDesc = _productService.GetProductById(exists.ProductAddonId).ProductName;
            //return View(exists);
        }
        [HttpPost]
        public ActionResult Edit(Addons addons)
        {


           // ProductAddonService repository = new ProductAddonService();
            var exists = _bllProdutAddons.GetAddonsById(addons.AddonsId);
            exists.ProductId = addons.ProductId;
            exists.ProductAddonId = addons.ProductAddonId;
            exists.DepartmentId = addons.DepartmentId;
            exists.IsActive = addons.IsActive;
            exists.ModifiedDate = DateTime.Now;
            exists.ModifiedUser = Session["loggeduser"].ToString();
            exists.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            exists.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (_bllProdutAddons.UpdateProductAddons(exists) >0 )
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }
           
            return View(exists);
        }

        [HttpPost]
        public ActionResult Create(List<Addons> addons)
        {

            if (addons==null)
            {
                @ViewBag.Message = "4";
                return View();
            }

            addons.ForEach(
                u => { u.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
                    u.CreatedUser = Session["loggeduser"].ToString();
                    u.CompanyID= Convert.ToInt32(Session["loggedusercompanyId"].ToString());
                }
                
                );
           
            var sss = _bllProdutAddons.SaveProductAddons(addons);
            if (sss == 0)
            {
                @ViewBag.Message = "2";
            }
            else
            {
                @ViewBag.Message = "1";
            }        
            return View();
        }


        public ActionResult ViewProductAddons(long id)
        {
          //  ProductAddonService addon = new ProductAddonService();
            //   var addons = addon.GetProductAddons();
            var addons = _bllProdutAddons.GetProductAddonsByProductId(id);
            addons.ToList().ForEach(a=>
            {
                
                a.ProductDesc = _bllproduct.GetProductDescById(a.ProductId).ProductName;
                a.ProductCode = _bllproduct.GetProductDescById(a.ProductId).ProductCode;
                a.AddonDesc = _bllproduct.GetAddonsDescById(a.ProductAddonId).ProductName;
                a.ProductAddonCode= _bllproduct.GetAddonsDescById(a.ProductAddonId).ProductCode;
            });


            return View(addons.ToList());
        }

        public ActionResult GetProductAddons(long productid ,long productAddonId)
        {
            var productaddons = _bllProdutAddons.GetProductAddons(productid, productAddonId);

            return new JsonResult
            {
                Data = productaddons,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult ViewAddonBasedProducts()
        {
            // ProductAddonService addon = new ProductAddonService();
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var mainproducts = _bllProdutAddons.GetAddonsProducts(companyid);
            mainproducts.ToList().ForEach(a =>
            {
                //a.DepartmentDesc = _departmentService.GetDepartmentById(a.DepartmentId).DepartmentName;
                a.ProductCode = _bllproduct.GetProductDescById(a.ProductId).ProductCode;
                a.ProductDesc = _bllproduct.GetProductDescById(a.ProductId).ProductName;
                //a.AddonDesc = _productService.GetProductDescById(a.ProductAddonId).ProductName;
            });


            return View("AddonbasedProducts",mainproducts);
        }



        public JsonResult RemoveAddon(long id, long addonid)
        {
            var res = _bllproduct.RemoveAddons(id,addonid);
         
            return new JsonResult
            {
                Data = res,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

    }
}