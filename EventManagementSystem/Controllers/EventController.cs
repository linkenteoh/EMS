using System;
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

        // Register Event
        [HttpPost]
        public ActionResult JoinEvent(Event eve)
        {
            return View();
        }

        public ActionResult ProposeEvent()
        {
            ViewBag.step = 1;
            return View();
        }

        [HttpPost]
        public ActionResult ProposeEvent(int step)
        {
            return View();
        }

        public ActionResult VenueBooking(string btn = "")
        {
            
            if (Request.IsAjaxRequest())
                return PartialView("_Venue");
            return View();
        }

        public ActionResult EventSuccess()
        {
            return View();
        }
    }
}