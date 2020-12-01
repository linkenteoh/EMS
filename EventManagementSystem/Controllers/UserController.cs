using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Security.Claims;
using EventManagementSystem.Models;
using EventManagementSystem.reCAPTCHA;

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
        // GET: User/RegOrganiser
        [Authorize]
        public ActionResult RegOrganiser()
        {
            return View();
        }

        // POST: User/RegOrganiser
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateGoogleCaptcha]
        [HttpPost]
        public ActionResult RegOrganiser(RegOrgVM model)
        {

            if (ModelState.IsValid)
            {
                var id = db.Users.First(u => u.username == User.Identity.Name).Id;

                var oragniser = new Organiser
                {
                    Id = id,
                    represent = model.represent,
                    position = model.position,
                    status = false
                };

                db.Organisers.Add(oragniser);
                db.SaveChanges();
                TempData["Info"] = "You've registered successfully. Please wait until it is accepted by an admin.";
            }
            return View(model);
        }

        // Edit users
        public ActionResult Edit(int id = 1)
        {
            var Model = db.Users.Find(id);
            if(Model == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(Model);
        }

        // POST : Edit User
        [HttpPost]
        public ActionResult Edit(User model)
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