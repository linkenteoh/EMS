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
                var id = db.Users.FirstOrDefault(u => u.username == User.Identity.Name).Id;

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

        private string ValidatePhoto(HttpPostedFileBase f)
        {
            var reType = new Regex(@"^image\/(jpeg|png)$", RegexOptions.IgnoreCase);
            var reName = new Regex(@"^.+\.(jpg|jpeg|png)$", RegexOptions.IgnoreCase);

            if (f == null)
            {
                return "No photo selected.";
            }
            else if (!reType.IsMatch(f.ContentType) ||
               !reName.IsMatch(f.FileName))
            {
                return "Only JPG or PNG photo is allowed.";
            }
            else if (f.ContentLength > 1 * 1024 * 1024)
            {
                return "Photo size cannot more than 1 MB";
            }
            return null;
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
                contact_no = u.contact_no.Trim(),
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
            var username = User.Identity.Name;
            var u = db.Users.Where(x => x.username == username).FirstOrDefault();
            // Get userID in registration
            var reg = db.Registrations.Where(r => u.Id.Equals(r.userId)).Select(r => r.eventId).ToArray();
            // Get EventID in registration that contains the userID
            var model = db.Events.Where(m => reg.Contains(m.Id)).AsQueryable();

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

            if (Request.IsAjaxRequest())
                return PartialView("_EventResults", model);
            return View(model);
        }

        // GET: User/ProposeEvent
        public ActionResult ProposeEvent()
        {
            return View();
        }

        // POST: User/ProposeEvent
        [HttpPost]
        public ActionResult ProposeEvent(EventInsertVM model)
        {
            string error = ValidatePhoto(model.Photo);
            if (error != null)
            {
                ModelState.AddModelError("Photo", error);
            }
            if (ModelState.IsValid)
            {
                int duration = ((int)model.endTime.TotalMinutes - (int)model.startTime.TotalMinutes) / 60;
                var e = new Event
                {
                    name = model.name,
                    des = model.des,
                    price = model.price,
                    availability = model.availability,
                    participants = model.participants,
                    startDate = model.startDate,
                    endDate = model.endDate,
                    startTime = model.startTime,
                    endTime = model.endTime,
                    duration = duration.ToString(),
                    organized_by = model.organized_by,
                    approvalStat = null,
                    status = false,
                    venueId = null,
                    photoURL = SavePhoto(model.Photo),
                    OrgId = db.Users.FirstOrDefault(u => u.username == User.Identity.Name).Id
                };
                try { 
                db.Events.Add(e);
                db.SaveChanges();
                }catch(Exception ex)
                {
                    TempData["Info"] = ex;
                }
                TempData["info"] = "Event record inserted successfully";

            }
            else
            {
                TempData["Error"] = "Error";
            }
            return View(model);
        }

        public ActionResult EventsProposed(int page = 1)
        {
            Func<Event, object> fn = s => s.Id;


            var events = db.Events.OrderBy(fn).Where(u => u.OrgId == db.Users.FirstOrDefault(org => org.username == User.Identity.Name).Id);
            var model = events.ToPagedList(page, 5);

            return View(model);
        }

        public ActionResult Billing()
        {
            int uId = db.Users.FirstOrDefault(u => u.username == User.Identity.Name).Id;
            var bill = db.Payments.ToList().Where(p => p.Registration.userId == uId);
            return View(bill);
        }

    }
}