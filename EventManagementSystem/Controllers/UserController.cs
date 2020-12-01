using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Security.Claims;
using EventManagementSystem.Models;
using EventManagementSystem.reCAPTCHA;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using PagedList;


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

        private string SavePhoto(HttpPostedFileBase f)
        {
            string name = Guid.NewGuid().ToString("n") + ".jpg";
            string path = Server.MapPath($"~/Photo/{name}");

            var img = new WebImage(f.InputStream);

            if (img.Width > img.Height)
            {
                int px = (img.Width - img.Height) / 2;
                img.Crop(0, px, 0, px);
            }
            else
            {
                int px = (img.Height - img.Width) / 2;
                img.Crop(px, 0, px, 0);
            }

            img.Resize(201, 201)
               .Crop(1, 1)
               .Save(path, "jpeg");

            return name;
        }

        private void DeletePhoto(string name)
        {
            name = System.IO.Path.GetFileName(name);
            string path = Server.MapPath($"~/Photo/{name}");
            System.IO.File.Delete(path);
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
                contact_no = u.contact_no,
                email = u.email,
                password = u.password,
                PhotoUrl = u.photo
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

                if (model.Photo != null)
                {
                    DeletePhoto(u.photo);
                    u.photo = SavePhoto(model.Photo);
                }

                db.SaveChanges();
                TempData["Info"] = "Profile edited successfully!";
                return RedirectToAction("Index", "Home");
            }
            model.PhotoUrl = u.photo;
            return View(model);
        }

        public ActionResult EventSearchIndex(string name ="", string startDate ="", string endDate="", 
            string startTime = "", string endTime = "", int page = 1)
        {
            //var model = db.Events.ToPagedList(page, 10).AsQueryable();
            var model = db.Events.AsQueryable();
            // Name
            if (!string.IsNullOrEmpty(name))
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

            //StaticPagedList
            //PagedList<Event> paged = new PagedList<Event>();
            //model = db.Events.OrderBy(s => s.Id).Cast<Event>();
            //model = model as PagedList<Event>();
            //if (page < 1)
            //{
            //    return RedirectToAction(null, new { page = 1 });
            //}
            //if (page > model.PageCount)
            //{
            //    return RedirectToAction(null, new { page = model.PageCount });
            //}

            if (Request.IsAjaxRequest())
                return PartialView("_EventResults", model);
            return View(model);
        }

        public ActionResult EventDetail(int id)
        {
            var model = db.Events.Find(id);
          
            if (model == null)
            {
                return RedirectToAction("EventSearchIndex");
            }
            return View(model);
        }

    }
}