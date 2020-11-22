using EventManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        DBEntities db = new DBEntities();
        // GET: Admin
        public ActionResult Index()
        {
            var model = db.Events;
            return View(model);
        }
        public ActionResult Edit(string id)
        {
            // var model = db.Events.Find(id);
            var model = db.Events;
            if(model == null)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }
        /*
        [HttpPost]
        public ActionResult Edit(Event model)
        {
            var id = db.Events.Find(model.Id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }*/
    }

    
}