using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EventManagementSystem.Models;

namespace EventManagementSystem.Controllers
{
    public class UserController : Controller
    {
        DBEntities db = new DBEntities();
        // GET: User
        public ActionResult Index()
        {
            // Get User Info
            var model = db.Users;
            //ViewBag.Name = "Lee Wen Jun";
            return View(model);
        }

        // Get Events User involved
        public ActionResult EventsJoined()
        {
            return View();
        }

        // Edit Profile 
        public ActionResult Edit()
        {

            return View();
        }

        public ActionResult Login()
        {
            return View();
        }
        
        public ActionResult LogOut()
        {
            return View();
        }



    }
}