//using HospitalityManagement.Models;
//using HospitalityManagement.Service;
using RIT.HMS.BLL.MasterData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RIT.HMS.Domain;

namespace HospitalityManagement.Controllers
{
    [SessionTimeout]
    public class DeliveryPersonController : Controller
    {
      
        BLL_DeliveryPerson _deliveryPerson;

        public DeliveryPersonController()
        {
            string cn = System.Web.HttpContext.Current.Session["CurrentConnectionName"].ToString();
            _deliveryPerson = new BLL_DeliveryPerson(cn);
        }
        // GET: DeliveryPerson
        [Authorize(Roles = "EmpView")]
        public ActionResult ViewDeliveryPersons()
        {
            int companyid = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            var deliveryperson = _deliveryPerson.GetDeliveryPersons(companyid);

            return View("ViewDeliveryPersons",deliveryperson);
        }

        [Authorize(Roles = "EmpCreatee")]
        public ActionResult Create()
        {
            

            return View();
        }

        [Authorize(Roles = "EmpCreatee")]
        [HttpPost]
        public ActionResult Create(DeliveryPerson devperson)
        {
            devperson.CreatedUser = Session["loggeduser"].ToString();
            devperson.CreatedDate = DateTime.Now;        
            devperson.DataTransfer = 0;
            devperson.LocationId = Convert.ToInt32(Session["loggeduserlocId"].ToString());
            devperson.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());
            //Added by pavi on 2019-12-01
            devperson.IsActive = true;
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (devperson.Photograph != null)
            {
                byte[] photo;
                using (BinaryReader br = new BinaryReader(devperson.Photograph.InputStream))
                {
                    photo = br.ReadBytes(devperson.Photograph.ContentLength);
                    devperson.Picture = photo;
                    devperson.PictureName = devperson.Photograph.FileName;
                    devperson.PictureType = devperson.Photograph.ContentType;
                }
            }
           
            var existsdelpersons = _deliveryPerson.GetdeliveryPersonByEmpId(devperson.EmployeeId);
            if (existsdelpersons != null)
            {
                ViewBag.Message = "3";
                return View();
            }

            if (_deliveryPerson.SaveDeliveryPerson(devperson) == 1)
            {
                ViewBag.Message = "1";
            }
            else
            {
                ViewBag.Message = "2";
            }

            ViewBag.EmpCode = devperson.EmployeeId;
            return View();
        }

        [Authorize(Roles = "EmpEdit")]
        [HttpGet]
        public ActionResult Edit(long id)
        {
           
            var exists = _deliveryPerson.GetdeliveryPersonById(id);
            @ViewBag.FileName = exists.PictureName;

            if (exists.Title == "Mr")
            {
                @ViewBag.sel = "1";
            }
            else if (exists.Title == "Mrs")
            {
                @ViewBag.sel = "2";
            }
            else if (exists.Title == "Ms")
            {
                @ViewBag.sel = "3";
            }
            else if (exists.Title == "Miss")
            {
                @ViewBag.sel = "4";
            }
            else if (exists.Title == "Dr")
            {
                @ViewBag.sel = "5";
            }
            else if (exists.Title == "Rev")
            {
                @ViewBag.sel = "6";
            }

            return View(exists);
        }

        [Authorize(Roles = "EmpEdit")]
        [HttpPost]
        public ActionResult Edit(DeliveryPerson delperson)
        {

            var exists = _deliveryPerson.GetdeliveryPersonById(delperson.DeliveryPersonId);
            exists.EmployeeId = delperson.EmployeeId;
            exists.FullName = delperson.FullName;
            exists.Address = delperson.Address;
            exists.Designation = delperson.Designation;
            exists.DOB = delperson.DOB;
            exists.DrivingLicence = delperson.DrivingLicence;        
            exists.NIC = delperson.NIC;
            exists.Telephone = delperson.Telephone;
            exists.Mobile = delperson.Mobile;
            exists.Email = delperson.Email;        
            exists.IsActive = delperson.IsActive;
            exists.InCaseOfEmergency = delperson.InCaseOfEmergency;
            exists.IsDelete = delperson.IsDelete;
            exists.ModifiedDate = DateTime.Now;
            exists.ModifiedUser = Session["loggeduser"].ToString();
            exists.CompanyID = Convert.ToInt32(Session["loggedusercompanyId"].ToString());

            if (delperson.Photograph != null)
            {
                byte[] newlogo;
                using (BinaryReader br = new BinaryReader(delperson.Photograph.InputStream))
                {
                    newlogo = br.ReadBytes(delperson.Photograph.ContentLength);
                    delperson.Picture = newlogo;
                    delperson.PictureName = delperson.Photograph.FileName;
                    delperson.PictureType = delperson.Photograph.ContentType;
                }


                if (delperson.PictureName != exists.PictureName)
                {
                    byte[] pic;
                    using (BinaryReader br = new BinaryReader(delperson.Photograph.InputStream))
                    {
                        pic = br.ReadBytes(delperson.Photograph.ContentLength);
                        exists.Picture = pic;
                        exists.PictureName = delperson.Photograph.FileName;
                        exists.PictureType = delperson.Photograph.ContentType;
                    }
                }


            }

            if (_deliveryPerson.UpdatedeliveryPerson(exists) == 1)
            {
                @ViewBag.Message = "1";
            }
            else
            {
                @ViewBag.Message = "2";
            }
            @ViewBag.CustCode = delperson.DeliveryPersonId;
            return View(delperson);
        }



    }
}