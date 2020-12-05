﻿using System;
using System.Collections.Generic;
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
        public ActionResult RegisterEvent(string username, int eventId, DateTime d)
        {
            int id = db.Users.FirstOrDefault(u => u.username == username).Id;
            int regId = db.Registrations.Count() + 1;

            var register = new Registration
            {
                Id = regId,
                eventId = eventId,
                userId = id,
                status = false,
                date = d
            };


            db.Registrations.Add(register);
            db.SaveChanges();

            var bill = new Payment
            {
                Id = register.Id,
                price = 10,
                gst = 10,
                status = false
            };
            db.Payments.Add(bill);
            db.SaveChanges();

            return RedirectToAction("");     
        }

        // GET venue booking index
        public ActionResult VenueBooking(int Id)
        {
            ViewBag.eventID = Id;
            // Get current event date and time
            var eventCurrent = db.Events.Find(Id);
            ViewBag.eventCurrent = eventCurrent;
            // Validate Availability
            // Check if other events'venueID exists 
            var eventsExisting = db.Events.Where(x => x.Id != Id && x.venueId != null);
            //foreach (var e in eventsExisting)
            //{
            eventsExisting = eventsExisting.Where(x => x.startDate >= eventCurrent.startDate && x.endDate <= eventCurrent.endDate);
            //eventsExisting = eventsExisting.Where(x => x.endDate <= eventCurrent.endDate);
            //}
            //eventsExisting = eventsExisting.Where(x => x.startTime < eventCurrent.startTime);
            //eventsExisting = eventsExisting.Where(x => x.startTime >= eventCurrent.startTime);

            // Check if the dates in each event is within range of current Event
            // if exists only proceed to check time within that date
            //eventsExisting = eventsExisting.Where(x => x.startDate >= eventCurrent.startDate && x.endDate <= eventCurrent.endDate && x.startTime >= eventCurrent.startTime && x.endTime <= eventCurrent.endTime);
            //eventsExisting = eventsExisting.Where(x => x.startDate >= eventCurrent.startDate && x.endDate <= eventCurrent.endDate);
            // Check if  the time in each event is within range of current event
            //eventsExisting = eventsExisting.Where(x => x.startTime >= eventCurrent.startTime && x.endTime <= eventCurrent.endTime);

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