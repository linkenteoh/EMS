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