using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using EventManagementSystem.Models;

namespace EventManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        DBEntities db = new DBEntities();

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        // GET: Account/Register
        public ActionResult Register()
        {

            return View();
        }

        public void Capture()
        {
            var stream = Request.InputStream;
            string dump;

            using (var reader = new StreamReader(stream))
                dump = reader.ReadToEnd();

            var path = Server.MapPath("~/Captures/test.jpg"); //TODO: Add in random strings to the name
            System.IO.File.WriteAllBytes(path, String_To_Bytes2(dump));
        }

        private byte[] String_To_Bytes2(string strInput)
        {
            int numBytes = (strInput.Length) / 2;
            byte[] bytes = new byte[numBytes];

            for (int x = 0; x < numBytes; ++x)
            {
                bytes[x] = Convert.ToByte(strInput.Substring(x * 2, 2), 16);
            }

            return bytes;
        }

        // POST: Account/Register
        [HttpPost]
        public ActionResult Register(User model)
        {
            if (ModelState.IsValid)
            {
                int userCount = db.Users.Count() + 1;
                model.Id = userCount;
                model.status = true;
                model.recoveryCode = Guid.NewGuid().ToString();
                model.activationCode = "ABCDEF";
                db.Users.Add(model);
                db.SaveChanges();

                TempData["Info"] = "New user created";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}