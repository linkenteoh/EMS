using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using EventManagementSystem.Models;

namespace EventManagementSystem.Controllers
{
    public class UserController : Controller
    {
        DBEntities db = new DBEntities();

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        // GET: User/Register
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

        // POST: User/Register
        [HttpPost]
        public ActionResult Register(User model)
        {
            model.status = 1;
            model.recoveryCode = "ABCDEF";
            model.activationCode = "ABCDEF";
            db.Users.Add(model);
            db.SaveChanges();

            return RedirectToAction("Register");
        }
    }
}