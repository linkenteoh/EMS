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

        public ActionResult BookEvent()
        {
            return View();
        }

        public ActionResult VenueBooking()
        {
            return View();
        }
    }
}