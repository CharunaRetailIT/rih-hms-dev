using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using RIT.HMS.BLL.MasterData;
using RIT.HMS.Domain;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    [Authorize(Roles = "PrdCreatee")]
    public class ProductInstructionController : Controller
    {
        BLL_ProductInstruction service;
        BLL_Product productservice;

        public ProductInstructionController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            service = new BLL_ProductInstruction(cn);
            productservice = new BLL_Product(cn);
        }
        public ActionResult Create()
        {

            return View();
        }

        public ActionResult Description()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Description(KOTBOTDescription desc)
        {
            desc.ModifiedDate = DateTime.Now;
            //desc.Type = "BOTH";
            desc.CompanyId= Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            bool res = service.SaveDescription(desc);
            if (res)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "2";
            }
            return View();
        }

        [HttpGet]
        public JsonResult GetProductInstructions()
        {

            var descriptions = service.GetDescription(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return Json(JsonConvert.SerializeObject(descriptions, Formatting.None, new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(ProductInstruction ins)
        {
            var temp = ins.Instruction.Aggregate((x, y) => x + "," + y);
            ins.InstructionList = "(" + temp + ")";
            ins.CompanyId= Convert.ToInt32(Session["loggedusercompanyId"].ToString()); 
            bool res = service.Save(ins);

            if (res)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "2";
            }

            return View();
        }

        [HttpGet]

        public JsonResult GetProducts(long deptid, long catid, long subcatid, long pid)
        {
            IEnumerable<Product> products = null;
            if (deptid != 0 && catid == 0 && subcatid == 0 && pid == 0)
            {
                products = productservice.GetMenuByDepartmentId(deptid);
            }
            else if (deptid != 0 && catid != 0 && subcatid == 0 && pid == 0)
            {
                products = productservice.GetMenuByDeptCatId(deptid, catid);
            }
            else if (deptid != 0 && catid != 0 && subcatid != 0 && pid == 0)
            {
                products = productservice.GetMenuByDeptCatSCatId(deptid, catid, subcatid);
            }
            else if (pid != 0)
            {
                products = productservice.GetMenuById(pid);
            }


            return new JsonResult
            {
                Data = products,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }


        public ActionResult DescriptionView()
        {
            //  ProductInstructionService service = new ProductInstructionService();
            var descriptions = service.GetDescription(Convert.ToInt32(Session["loggedusercompanyId"].ToString()));
            return View(descriptions);
        }


        public ActionResult DescriptionEdit(long id)
        {
            //ProductInstructionService service = new ProductInstructionService();
            var exists = service.GetDescriptionsById(id);

            ViewBag.Type = exists.Type;
            return View(exists);
        }

        [HttpPost]
        public ActionResult DescriptionEdit(KOTBOTDescription desc)
        {
            // ProductInstructionService descriptions = new ProductInstructionService();
            var exists = service.GetDescriptionsById(desc.KOTBOTDescriptionId);
            exists.Description = desc.Description;
            exists.Type = desc.Type;
            exists.IsActive = desc.IsActive;
            exists.ModifiedDate = DateTime.Now;

            if (service.UpdateDescription(exists) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "0";
            }

            return View(exists);
        }



        public ActionResult Vieww()
        {
            //ProductInstructionService service = new ProductInstructionService();
            var ins = service.GetInstructions();
            ProductInstruction prdins = new ProductInstruction();
            //    prdins.Idetail = service.GetInstructions().ToList();
            return View("Vieww", ins);
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            var exists = service.GetInstructionsById(id);

            string[] instructions = null;
            if (exists.First().InstructionList.Contains(","))
            {
                instructions = exists.First().InstructionList.Replace('(', ' ').Replace(')', ' ').Trim().Split(',');
            }
            else
            {
                instructions = exists.First().InstructionList.Replace('(', ' ').Replace(')', ' ').Split().Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }
           

            // ProductInstructionService ins = new ProductInstructionService();
            
            List<KOTBOTDescription> pins = new List<KOTBOTDescription>();
            for (int i = 0; i <= instructions.Length - 1; i++)
            {
                KOTBOTDescription pi = new KOTBOTDescription();
                var eins = service.GetInstructionById(Convert.ToInt64(instructions[i].Trim()));
                pi.Description = eins.Description;
                pi.KOTBOTDescriptionId = eins.KOTBOTDescriptionId;
                pi.ProductId = id;
                pi.ModifiedDate = DateTime.Now;
                pins.Add(pi);
            }
            
            return View("Edit", pins);
        }

        [HttpPost]
        public ActionResult Edit(List<KOTBOTDescription> lstpins)
        {
            long InsId = 0;
            try
            {
                // ApplicationDbContext context = new ApplicationDbContext();

                foreach (KOTBOTDescription item in lstpins)
                {

                    KOTBOTDescription existsIns = new KOTBOTDescription();
                    existsIns = service.GetDescriptionsById(item.KOTBOTDescriptionId);
                    if (existsIns != null)
                    {
                        InsId = existsIns.KOTBOTDescriptionId;
                        //context.SaveChanges();
                    }

                }


                //ProductInstructionService ins = new ProductInstructionService();
                var pins = service.GetProductInsByProductId(InsId);

                pins.ToList().ForEach(p =>
                {
                    p.ProductName = productservice.GetProductById(p.ProductId).ProductName;
                    //  p.Instruction = service.GetInstructionById(p.InstructionList).Description;
                });


                return View("Edit", pins.ToList());
            }
            catch (Exception e)
            {

                return View("Edit");
            }

        }

        [HttpGet]
        public ActionResult RemoveIns(int id,int productid)
        {
            var exists = service.GetInstructionsById(productid);
            // string[] ins1 = exists.First().InstructionList.Replace('(', ' ').Replace(')', ' ').Trim().Split(',');
            string[] instructions = null;
            if (exists.First().InstructionList.Contains(","))
            {
                instructions = exists.First().InstructionList.Replace('(', ' ').Replace(')', ' ').Trim().Split(',');
            }
            else
            {
                instructions = exists.First().InstructionList.Replace('(', ' ').Replace(')', ' ').Split().Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }



            string[] ins2 = null;
            var list = new List<string>(instructions);
            list.Remove(id.ToString().Trim());
            ins2 = list.ToArray();
            ProductInstruction ins = new ProductInstruction();
            ins = exists.First();
            if (ins2.Length != 0)
            {
                var temp = ins2.Aggregate((x, y) => x + "," + y);

                ins.InstructionList = "(" + temp + ")";
            }
            else
            {
                ins.InstructionList = "";
            }

            var res = service.UpdateInstruction(ins);
            var exists11 = service.GetInstructionsById(productid);
            List<KOTBOTDescription> pins = new List<KOTBOTDescription>();
            if (exists11.Count() != 0)
            {
                string[] ins11 = exists11.First().InstructionList.Replace('(', ' ').Replace(')', ' ').Trim().Split(',');                      
                for (int i = 0; i <= ins11.Length - 1; i++)
                {
                    KOTBOTDescription pi = new KOTBOTDescription();
                    var eins = service.GetInstructionById(Convert.ToInt64(ins11[i].Trim()));
                    pi.Description = eins.Description;
                    pi.KOTBOTDescriptionId = eins.KOTBOTDescriptionId;
                    pi.ProductId = productid;
                    pi.ModifiedDate = DateTime.Now;
                    pins.Add(pi);
                }

            }

            return View("Edit", pins);
        }
    }
}