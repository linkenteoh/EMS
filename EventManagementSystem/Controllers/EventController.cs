using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EventManagementSystem.Models;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using PagedList;
using PagedList.Mvc;

namespace EventManagementSystem.Controllers
{
    public class EventController : Controller
    {
        DBEntities db = new DBEntities();
        // GET: Event
        public ActionResult Index()
        {
            return View();
        }

        // Search Events joined by users
        public ActionResult EventSearchIndex(string name = "", string startDate = "", string endDate = "",
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
            }
            else if (!string.IsNullOrEmpty(startDate))
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

        // Event Details
        public ActionResult EventDetail(int id)
        {
            var model = db.Events.Find(id);

            if (model == null)
            {
                return RedirectToAction("EventSearchIndex");
            }
            return View(model);
        }

        // Register Event
        public ActionResult JoinEvent()
        {
            return View();
        }

        // Register Event
        [HttpPost]
        public ActionResult JoinEvent(Event eve)
        {
            return View();
        }

         public ActionResult EventsProposed(int page = 1)
        {
            var username = User.Identity.Name;
            var u = db.Users.Where(x => x.username == username).FirstOrDefault();
            // GeT organiser data
            var org = db.Organisers.Where(x => x.Id == u.Id).FirstOrDefault();
            // Get events proposed by finding org ID
            var model = db.Events.Where(m => m.OrgId == org.Id).OrderBy(m => m.Id).ToPagedList(page, 10);
            if(model == null)
            {
                TempData["info"] = "Event records not founded";
                return RedirectToAction("ProposeEvent");
            }
            return View(model);
        }

        private string ValidatePhoto(HttpPostedFileBase f)
        {
            var reType = new Regex(@"^image\/(jpeg|png)$", RegexOptions.IgnoreCase);
            var reName = new Regex(@"^.+\.(jpg|jpeg|png)$", RegexOptions.IgnoreCase);

            if (f == null)
            {
                return "No photo.";
            }
            else if (!reType.IsMatch(f.ContentType) || !reName.IsMatch(f.FileName))
            {
                return "Only JPG or PNG photo is allowed.";
            }
            else if (f.ContentLength > 1 * 1024 * 1024)
            {
                return "Photo size cannot more than 1MB.";
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

        public ActionResult ProposeEvent()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ProposeEvent(EventInsertVM model)
        {
            var username = User.Identity.Name;
            var u = db.Users.Where(x => x.username == username).FirstOrDefault();
            // GeT organiser data
            var org = db.Organisers.Where(x => x.Id == u.Id).FirstOrDefault();
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
                    approvalStat = model.approvalStat,
                    status = true,
                    venueId = null,
                    photoURL = SavePhoto(model.Photo),
                    OrgId = org.Id
                };
                db.Events.Add(e);
                db.SaveChanges();
                TempData["info"] = "Event record proposed successfully, Waiting for Approval";
                return RedirectToAction("EventsProposed");

            }
            else
            {
                TempData["Error"] = "Error";
            }
            return View(model);
        }

        // GET venue booking index
        public ActionResult VenueBooking(int Id)
        {
            ViewBag.eventID = Id;
            // Get current event date and time
            var eventCurrent = db.Events.Find(Id);
            ViewBag.eventCurrent = eventCurrent;
            // Validate Availability
            var eventsExisting = db.Events.Where(x => x.Id != Id).AsQueryable();

            // Check if the dates in each event is within range of current Event
            eventsExisting = db.Events.Where(x => x.startDate >= eventCurrent.startDate && x.endDate <= eventCurrent.endDate);

            // Check if  the time in each event is within range of current event
            eventsExisting = db.Events.Where(x => x.startTime >= eventCurrent.startTime && x.endTime <= eventCurrent.endTime);

            // Check if venue exist
            eventsExisting = db.Events.Where(x => x.venueId != null);
            ViewBag.venueOccupied = eventsExisting;

            var model = db.Venues;
            return View(model);
        }

        // Confirm Venue
        [HttpPost]
        public ActionResult VenueBooking(int eventID, int venueID)
        {
            var model = db.Venues.Where(v => v.Id == venueID).FirstOrDefault();
            if (model != null)
            {
                var eve = db.Events.Find(eventID);
                eve.venueId = model.Id;
                db.SaveChanges();
                TempData["info"] = "Venue Booked Successfully";
                return RedirectToAction("EventsProposed");
            }
            else
            {
                TempData["info"] = "Venue Booking Error";
                return View(model);
            }
        }

        //[HttpPost]
        public ActionResult EventSuccess()
        {
            return View();
        }
    }
}