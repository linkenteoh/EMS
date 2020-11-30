using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using EventManagementSystem.Models;

namespace EventManagementSystem.Controllers
{
    public class UserController : Controller
    {
        DBEntities db = new DBEntities();

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        // GET: User/Register
        public ActionResult Register()
        {

            return View();
        }

        public void Capture()
        {
            var stream = Request.InputStream;
            string dump;

            using (var reader = new StreamReader(stream))
                dump = reader.ReadToEnd();

            var path = Server.MapPath("~/Captures/test.jpg"); //TODO: Add in random strings to the name
            System.IO.File.WriteAllBytes(path, String_To_Bytes2(dump));
        }

        private byte[] String_To_Bytes2(string strInput)
        {
            int numBytes = (strInput.Length) / 2;
            byte[] bytes = new byte[numBytes];

            for (int x = 0; x < numBytes; ++x)
            {
                bytes[x] = Convert.ToByte(strInput.Substring(x * 2, 2), 16);
            }

            return bytes;
        }

        // POST: User/Register
        [HttpPost]
        public ActionResult Register(User model)
        {
            model.status = 1;
            model.recoveryCode = "ABCDEF";
            model.activationCode = "ABCDEF";
            db.Users.Add(model);
            db.SaveChanges();

            return RedirectToAction("Register");
        }

        // Edit users
        public ActionResult Edit()
        {
            var name = User.Identity.Name;
            var u = db.Users.Where(x => x.username == name).FirstOrDefault();
            if(u == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var Model = new EditProfileVM
            {
                Id = u.Id,
                username = u.username,
                name = u.name,
                status = u.status,
                contact_no = u.contact_no,
                email = u.email,
                organizer = u.organizer,
                role = u.role,
                password = u.password,
                recoveryCode = u.recoveryCode,
                
            };
            return View(Model);
        }

        // POST : Edit User
        [HttpPost]
        public ActionResult Edit(EditProfileVM model)
        {
            var u = db.Users.Find(model.Id);

            if (u == null)
            {
                return RedirectToAction("Edit");
            }

            if (ModelState.IsValid)
            {
                u.name = model.name;
                u.username = model.username;
                u.email = model.email;
                u.contact_no = model.contact_no;
                u.password = model.password;
                
                db.SaveChanges();
                TempData["Info"] = "Profile edited successfully!";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        public ActionResult EventSearchIndex(string name ="", string startDate ="", string endDate="", 
            string startTime = "", string endTime = "")
        {
            ViewBag.StatusList = new SelectList(db.Events, "status");
            var model = db.Events.AsQueryable();
            // Name
            if(!string.IsNullOrEmpty(name))
            {
                model = db.Events.Where(m => m.name.Contains(name));
            }
            // Start Date && End Date
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                var dtFrom = DateTime.Parse(startDate);
                var dtTo = DateTime.Parse(endDate);
                model = db.Events.Where(x => x.startDate >= dtFrom && x.endDate <= dtTo);
            }else if (!string.IsNullOrEmpty(startDate))
            {
                var dtFrom = DateTime.Parse(startDate);
                model = db.Events.Where(x => x.startDate >= dtFrom);
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                var dtTo = DateTime.Parse(endDate);
                model = db.Events.Where(x => x.endDate <= dtTo);
            }
            // Start Time && End Time
            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                var timeFrom = TimeSpan.Parse(startTime);
                var timeTo = TimeSpan.Parse(endTime);
                model = db.Events.Where(x => x.startTime >= timeFrom && x.endTime <= timeTo);
            }
            else if (!string.IsNullOrEmpty(startTime))
            {
                var timeFrom = TimeSpan.Parse(startTime);
                model = db.Events.Where(x => x.startTime >= timeFrom);
            }
            else if (!string.IsNullOrEmpty(endTime))
            {
                var timeTo = TimeSpan.Parse(endTime);
                model = db.Events.Where(x => x.endTime <= timeTo);
            }

            if (Request.IsAjaxRequest())
                return PartialView("_EventResults", model);
            return View(model);
        }

        public ActionResult EventDetail(int id)
        {
            var model = db.Events.Find(id);
          
            if (model == null)
            {
                return RedirectToAction("EventList");
            }
            return View(model);
        }

      
    }
}