using RIT.HMS.Domain.Common;
using RIT.HMS.Domain.Transactions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalityManagement.Controllers
{
    public class StructureChangesController : Controller
    {
        string connectionString = "";
        public StructureChangesController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
             connectionString = ConfigurationManager.ConnectionStrings[cn].ConnectionString;
        }
        // GET: StructureChanges
        public ActionResult Index()
        {
            return View();
        }

        // GET: StructureChanges/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: StructureChanges/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StructureChanges/Create
        [HttpPost]
        public ActionResult Create(InvStructureChanges invStructureChanges)
        {
            try
            {
                // TODO: Add insert logic here
                string Datetimenow = DateTime.Now.ToString("dd/MM/yyyy");
                string Day = DateTime.Now.ToString("dd");
                string Month = DateTime.Now.ToString("MM");
                string Year = DateTime.Now.ToString("yyyy");
                string Mint = DateTime.Now.ToString("mm");
                string Hour = DateTime.Now.ToString("HH");

                string TempPassword = Day + Month + Year + Hour + Mint;

                if(TempPassword== invStructureChanges.Password)
                {
                    StructureChanges stru = new StructureChanges();
                    stru.mainQueries(connectionString);
                    ViewBag.Message = "3";
                }
                else
                {
                    ViewBag.Message = "0";
                }
                //return RedirectToAction("Index");
               
                return View();
            }
            catch
            {
                return View();
            }
        }

        // GET: StructureChanges/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: StructureChanges/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: StructureChanges/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: StructureChanges/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
