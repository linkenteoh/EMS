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
                    status = null
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

        public ActionResult EventSearchIndex(string searchName = "", string name = "", string startDate = "", string endDate = "",
            string startTime = "", string endTime = "", string venue = "", string sort = "", int page = 1)
        {
            ViewBag.VenueList = new SelectList(db.Venues, "name", "name");
            ViewBag.sortList = new SelectList(
                new List<SortItems> { 
                    new SortItems { Id = "Id", name = "Id"}, 
                    new SortItems { Id = "name", name = "Name"}, 
                    new SortItems { Id = "price", name = "Price"}, 
                    new SortItems { Id = "date", name = "Date"}, 
                    new SortItems { Id = "Venue.name", name = "Venue"} 
                },"Id", "name") ;
            var username = User.Identity.Name;
            var u = db.Users.Where(x => x.username == username).FirstOrDefault();
            // Get userID in registration
            var reg = db.Registrations.Where(r => u.Id.Equals(r.userId)).Select(r => r.eventId).ToArray();
            // Get EventID in registration that contains the userID
            var model = db.Events.Where(m => reg.Contains(m.Id));

            //// Name
            if (!string.IsNullOrEmpty(name))
            {
                model = model.Where(m => m.name.Contains(name));
            }
            // Venue
            if (!string.IsNullOrEmpty(venue))
            {
                model = model.Where(x => x.Venue.name.Contains(venue));
            }
            // Start Date && End Date
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                var timeFrom = DateTime.Parse(startDate);
                var timeTo = DateTime.Parse(endDate);
                model = model.Where(x => x.date >= timeFrom && x.date <= timeTo);
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                var timeFrom = DateTime.Parse(startDate);
                model = model.Where(x => x.date >= timeFrom);
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                var timeTo = DateTime.Parse(endDate);
                model = model.Where(x => x.date <= timeTo);
            }
            // Start Time && End Time
            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                var timeFrom = TimeSpan.Parse(startTime);
                var timeTo = TimeSpan.Parse(endTime);
                model = model.Where(x => x.startTime >= timeFrom && x.endTime <= timeTo);
            }
            else if (!string.IsNullOrEmpty(startTime))
            {
                var timeFrom = TimeSpan.Parse(startTime);
                model = model.Where(x => x.startTime >= timeFrom);
            }
            else if (!string.IsNullOrEmpty(endTime))
            {
                var timeTo = TimeSpan.Parse(endTime);
                model = model.Where(x => x.endTime <= timeTo);
            }
            // Search name
            model = model.Where(x => x.name.Contains(searchName) || x.des.Contains(searchName) || x.Venue.name.Contains(searchName));
            // Sort By
            Func<Event, object> fn = s => s.Id;
            switch (sort)
            {
                case "Id": fn = s => s.Id; break;
                case "name": fn = s => s.name; break;
                case "price": fn = s => s.price; break;
                case "date": fn = s => s.date; break;
                case "Venue.name": fn = s => s.venueId; break;
            }
            // PagedList
            var events = model.OrderBy(fn).ToPagedList(page, 10);

            if (Request.IsAjaxRequest())
                return PartialView("_EventResults", events);
            return View(events);
        }

        // GET: User/ProposeEvent
        public ActionResult ProposeEvent()
        {
            ViewBag.OrganizerList = new SelectList(db.Organisers.Where(o => o.status == true), "Id", "represent");
            return View();
        }

        // POST: User/ProposeEvent
        [HttpPost]
        public ActionResult ProposeEvent(EventProposeVM model)
        {     
            string error = ValidatePhoto(model.Photo);
            if (error != null)
            {
                ModelState.AddModelError("Photo", error);
            }
            if (ModelState.IsValid)
            {
                int id = db.Users.FirstOrDefault(u => u.username == User.Identity.Name).Id;

                var e = new Event
                {
                    name = model.name,
                    des = model.des,
                    price = model.price,
                    participants = model.participants,
                    availability = model.participants,
                    date = model.date,
                    startTime = model.startTime,
                    endTime = model.endTime,
                    approvalStat = null,
                    status = true,
                    venueId = null,
                    photoURL = SavePhoto(model.Photo),
                    OrgId = id
                };
                try { 
                    db.Events.Add(e);
                    db.SaveChanges();
                    TempData["info"] = "Event proposed successfully";
                    return RedirectToAction("ManageEventProposed", "User", new { Id=e.Id });
                }
                catch(Exception ex)
                {
                    TempData["Info"] = ex;
                }
               

            }
            else
            {
                TempData["Error"] = "Error";
            }
            ViewBag.OrganizerList = new SelectList(db.Organisers.Where(o => o.status == true), "Id", "represent");
            return View(model);
        }

        public ActionResult EventsProposed(string searchName = "", string name = "", string startDate = "", string endDate = "",
            string startTime = "", string endTime = "", string venue = "", string sort = "", int page = 1)
        {
            ViewBag.VenueList = new SelectList(db.Venues, "name", "name");
            ViewBag.sortList = new SelectList(
                new List<SortItems> {
                    new SortItems { Id = "Id", name = "Id"},
                    new SortItems { Id = "name", name = "Name"},
                    new SortItems { Id = "price", name = "Price"},
                    new SortItems { Id = "date", name = "Date"},
                    new SortItems { Id = "venue", name = "Venue"}
                }, "Id", "name");
            var model = db.Events.Where(u => u.OrgId == db.Users.FirstOrDefault(org => org.username == User.Identity.Name).Id);

            //// Name
            if (!string.IsNullOrEmpty(name))
            {
                model = model.Where(m => m.name.Contains(name));
            }
            // Venue
            if (!string.IsNullOrEmpty(venue))
            {
                model = model.Where(x => x.Venue.name.Contains(venue));
            }
            // Start Date && End Date
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                var timeFrom = DateTime.Parse(startDate);
                var timeTo = DateTime.Parse(endDate);
                model = model.Where(x => x.date >= timeFrom && x.date <= timeTo);
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                var timeFrom = DateTime.Parse(startDate);
                model = model.Where(x => x.date >= timeFrom);
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                var timeTo = DateTime.Parse(endDate);
                model = model.Where(x => x.date <= timeTo);
            }
            // Start Time && End Time
            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                var timeFrom = TimeSpan.Parse(startTime);
                var timeTo = TimeSpan.Parse(endTime);
                model = model.Where(x => x.startTime >= timeFrom && x.endTime <= timeTo);
            }
            else if (!string.IsNullOrEmpty(startTime))
            {
                var timeFrom = TimeSpan.Parse(startTime);
                model = model.Where(x => x.startTime >= timeFrom);
            }
            else if (!string.IsNullOrEmpty(endTime))
            {
                var timeTo = TimeSpan.Parse(endTime);
                model = model.Where(x => x.endTime <= timeTo);
            }
            // Search name
            model = model.Where(x => x.name.Contains(searchName) || x.des.Contains(searchName) || x.Venue.name.Contains(searchName));
            // Sort By
            Func<Event, object> fn = s => s.Id;
            switch (sort)
            {
                case "Id": fn = s => s.Id; break;
                case "name": fn = s => s.name; break;
                case "price": fn = s => s.price; break;
                case "date": fn = s => s.date; break;
                case "venue": fn = s => s.venueId; break;
            }
            // PagedList
            var events = model.OrderBy(fn).ToPagedList(page, 10);

            if (Request.IsAjaxRequest())
                return PartialView("_EventApproveList", events);
            return View(events);
        }

        // GET: Event/ManageEventProposed
        public ActionResult ManageEventProposed(int Id)
        {
            var e = db.Events.Find(Id);
            if (e == null)
            {
                return RedirectToAction("EventsProposed", "User");
            }


            var model = new OrgEditEventVM
            {
                Id = Id,
                name = e.name,
                des = e.des,
                price = e.price,
                participants = e.participants,
                date = e.date,
                startTime = e.startTime,
                endTime = e.endTime,
                availability = e.availability,
                approvalStat = e.approvalStat,
                photoURL = e.photoURL,
                OrgId = e.OrgId,
                venueId = e.venueId,
                Organiser = e.Organiser,
                Venue = e.Venue,
                Registrations = e.Registrations
            };

            ViewBag.Registrations = db.Registrations.Where(r => r.eventId == Id);
            ViewBag.ParticipantsCount = db.Registrations.Where(r => r.eventId == Id && r.Payment.status == true).Count();
            ViewBag.PendingCount = db.Registrations.Where(r => r.eventId == Id && r.Payment.status == false).Count();
            ViewBag.Payments = db.Payments.Where(p => p.Registration.eventId == Id);
            ViewBag.PaymentsCount = db.Payments.Where(p => p.Registration.eventId == Id).Count();
            return View(model);
        }

        // POST: Event/ManageEventProposed
        [HttpPost]
        public ActionResult ManageEventProposed(OrgEditEventVM model)
        {
            var e = db.Events.Find(model.Id);
            if (model == null)
            {
                return RedirectToAction("EventsProposed", "User");
            }
            if (ModelState.IsValid)
            {
                e.name = model.name;
                e.des = model.des;
                e.price = model.price;
                e.availability = e.availability;
                e.participants = model.participants;
                e.date = model.date;
                e.startTime = model.startTime;
                e.endTime = model.endTime;
                if (model.Photo != null)
                {
                    DeletePhoto(e.photoURL);
                    e.photoURL = SavePhoto(model.Photo);
                }
                db.SaveChanges();
                TempData["info"] = "Event record updated successfully";
                return RedirectToAction("ManageEventProposed", "User", new { id = model.Id });
            }
            return View(model);
        }

        public ActionResult Billing()
        {
            int uId = db.Users.FirstOrDefault(u => u.username == User.Identity.Name).Id;
            var bill = db.Payments.ToList().Where(p => p.Registration.userId == uId);
            return View(bill);
        }

        // GET: User/Payment
        public ActionResult Payment(int id)
        {
            var payment = db.Payments.Find(id);

            var model = new PaymentVM
            {
                price = payment.price,
                addCharge = payment.addCharge,
                Registration = payment.Registration
            };

            return View(model);
        }

        // POST: User/Payment
        [HttpPost]
        public ActionResult Payment(PaymentVM model)
        {
            if (ModelState.IsValid)
            {
                var payment = db.Payments.Find(model.Id);
                payment.status = true;
                db.SaveChanges();
            }
            return View();
        }

    }
}