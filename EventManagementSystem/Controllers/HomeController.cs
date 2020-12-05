﻿using EventManagementSystem.Models;
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

        public ActionResult Events(int page = 1)
        {
            Func<Event, object> fn = s => s.Id;


            var events = db.Events.Where(e => e.approvalStat == true && e.status == true).OrderBy(fn);
            var model = events.ToPagedList(page, 10);

            return View(model);
        }
    }
}