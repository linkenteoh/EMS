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
                // TODO: TempData
                // TempData["Info"] = "Student record edited successfully!";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        public ActionResult EventSearch(EventSearchModel searchModel)
        {
            var results = new EventSearchCriteria();
            var model = results.GetEvents(searchModel);
            return View(model);
        }

        public ActionResult EventList(int id = 1)
        {
            // Get userID in registration
            var reg = db.Registrations.Where(r => id.Equals(r.userId));

            // Get EventID in registration that contains the userID


            // LIst event based on EventID

            var model = db.Events;
            //var eventList = db.Events.Where(e => reg.Contains(e.Id);
            // reg.userId = 
            return View(model);
        }

        public ActionResult EventDetail(int id)
        {
            var model = db.Events.Find(id);
          
            if (model == null)
            {
                return RedirectToAction("EventList");
            }
            //if (Request.IsAjaxRequest())
            //    return PartialView("_Detail", model);

            return View(model);
        }

      
    }
}