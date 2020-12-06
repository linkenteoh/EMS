using EventManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace EventManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        DBEntities db = new DBEntities();

        public ActionResult Index()
        {
            var model = db.Advertisements;
            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Events(string searchName = "", string name = "", string startDate = "", string endDate = "",
            string startTime = "", string endTime = "", string venue = "", string sort = "", int page = 1)
        {
            ViewBag.VenueList = new SelectList(db.Venues, "name", "name");
            ViewBag.sortList = new SelectList(
                new List<SortItems> {
                    new SortItems { Id = "name", name = "Name"},
                    new SortItems { Id = "price", name = "Price"},
                    new SortItems { Id = "date", name = "Date"},
                    new SortItems { Id = "venue", name = "Venue"}
                }, "Id", "name");
            var model = db.Events.Where(e => e.approvalStat == true && e.status == true);

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
                case "name": fn = s => s.name; break;
                case "price": fn = s => s.price; break;
                case "date": fn = s => s.date; break;
                case "venue": fn = s => s.venueId; break;
            }
            // PagedList
            var events = model.OrderBy(fn).ToPagedList(page, 10);

            if (Request.IsAjaxRequest())
                return PartialView("_BrowseEvent", events);
            return View(events);
        }
    }
}