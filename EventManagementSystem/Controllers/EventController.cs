using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EventManagementSystem.Models;

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

        public ActionResult Chat()
        {

            return View();
        }

        public ActionResult EventDetail(int id)
        {
            var model = db.Events.Find(id);
            return View(model);
        }

        public ActionResult EventDetails(Event eve)
        {
            return PartialView("~/Views/Event/_EventDetails.cshtml", eve);
        }

        // Register Event
        public ActionResult RegisterEvent(int eventId, DateTime d)
        {

            var user = db.Users.FirstOrDefault(u => u.username == User.Identity.Name);

            if (db.Registrations.Any(r => r.eventId == eventId && r.userId == user.Id))
            {
                TempData["Info"] = "You've registered this event already!";
                return RedirectToAction("EventDetail", new { id = eventId });
            }

            var e = db.Events.Find(eventId);
            double price = 0;
            double comission = 0;
            double addCharge = 0;
            double temp = 0;

            if(user.role == "Student")
            {
                addCharge = 0;
                price = (double)e.price;
                comission = price * 0.1;
            }else if(user.role == "Outsider")
            {
                addCharge = (double)e.price * 0.1;    //Additional charge
                price = (double)e.price + addCharge; 
                comission = price * 0.1;              //Every transaction 10% are contributed as comission
            }

            int regId = db.Registrations.Count() + 1;
            var register = new Registration
            {
                Id = regId,
                eventId = eventId,
                userId = user.Id,
                status = true,
                date = d
            };


            db.Registrations.Add(register);
            db.SaveChanges();

            var bill = new Payment
            {
                Id = register.Id,
                price = (decimal)price,
                addCharge = (decimal)addCharge,
                commision = (decimal)comission,
                status = false
            };
            db.Payments.Add(bill);
            db.SaveChanges();

            TempData["Info"] = "You've registered successfully, please make your payment before the event date.";
            return RedirectToAction("Billing", "User");     
        }

        // GET venue booking index
        public ActionResult VenueBooking(int Id)
        {
            ViewBag.eventID = Id;
            // Get current event date and time
            var eventCurrent = db.Events.Find(Id);
            ViewBag.eventCurrent = eventCurrent;
            // Validate Availability
            var eventsExisting = db.Events.Where(x => x.Id != Id);

            // Check if the dates in each event is within range of current Event
            eventsExisting = eventsExisting.Where(x => x.date == eventCurrent.date);
            // Check if  the time in each event is within range of current event
            eventsExisting = eventsExisting.Where(x => x.startTime < eventCurrent.endTime);
            eventsExisting = eventsExisting.Where(x => x.endTime > eventCurrent.startTime);

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
                return RedirectToAction("ManageEventProposed", "User", new { id = eventID });
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