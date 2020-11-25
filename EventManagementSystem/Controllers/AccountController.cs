﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net.Mail;
using EventManagementSystem.Models;
using System.Data.Entity.Validation;
using System.Text.RegularExpressions;
using System.Web.Helpers;

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

            string name = Guid.NewGuid().ToString("n") + ".jpg";
            string path = Server.MapPath($"~/Photo/{name}");

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

        private string ValidatePhoto(HttpPostedFileBase f)
        {
            var reType = new Regex(@"^image\/(jpeg|png)$", RegexOptions.IgnoreCase);
            var reName = new Regex(@"^.+\.(jpg|jpeg|png)$", RegexOptions.IgnoreCase);

            if (f == null)
            {
                return "No photo.";
            }
            else if (!reType.IsMatch(f.ContentType) || !reName.IsMatch(f.FileName))
            {
                return "Only JPG or PNG photo is allowed.";
            }
            else if (f.ContentLength > 1 * 1024 * 1024)
            {
                return "Photo size cannot more than 1MB.";
            }

            return null;
        }

        private string SavePhoto(HttpPostedFileBase f)
        {
            string name = Guid.NewGuid().ToString("n") + ".jpg";
            string path = Server.MapPath($"~/Photo/{name}");

            var img = new WebImage(f.InputStream);

            if (img.Width > img.Height)
            {
                int px = (img.Width - img.Height) / 2;
                img.Crop(0, px, 0, px);
            }
            else
            {
                int px = (img.Height - img.Width) / 2;
                img.Crop(px, 0, px, 0);
            }

            img.Resize(201, 201)
               .Crop(1, 1)
               .Save(path, "jpeg");

            return name;
        }

        // POST: Account/Register
        [HttpPost]
        public ActionResult Register(RegisterVM model)
        {
            string err = ValidatePhoto(model.Photo);
            if (err != null)
            {
                ModelState.AddModelError("Photo", err);
            }

            if (ModelState.IsValid)
            {
                int userCount = db.Users.Count() + 1;
                var user = new User {
                    Id = userCount,
                    name = model.name,
                    contact_no = model.contact_no,
                    email = model.email,
                    username = model.username,
                    password = model.password,
                    role = model.role,
                    status = 0, //0- Inactive, 1- Active, 2- Deleted
                    recoveryCode = null,
                    activationCode = Guid.NewGuid().ToString(),
                    photo = SavePhoto(model.Photo)
                };

/*                model.Id = userCount;
                model.status = 0; //0- Inactive, 1- Active, 2- Deleted
                model.recoveryCode = null ;
                model.activationCode = Guid.NewGuid().ToString();*/

             
                MailMessage mail = new MailMessage();
                mail.To.Add(model.email);
                mail.From = new MailAddress("imrp1234@gmail.com");
                mail.Subject = "Activate your TARUC Event Management account";
                string link = "https://localhost:44302/Account/Activation?activationCode=" + user.activationCode + "&userid=" + user.Id;
                string Body = link;
                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                string email = "imrp1234@gmail.com";
                smtp.Credentials = new System.Net.NetworkCredential(email, ""); // Enter seders User name and password   
                smtp.EnableSsl = true;
                smtp.Send(mail);

                try
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }


                TempData["Info"] = "Registered successfully!";
                return RedirectToAction("Awaitactivation", "Account", new { email = model.email });
            }

            return View(model);
        }

        public ActionResult Awaitactivation(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        public ActionResult Activation(string activationCode, int userId)
        {
            var model = db.Users.Find(userId);
            if(model != null)
            {
                if(model.activationCode == activationCode)
                {
                    try
                    {
                        model.status = 1;
                        db.SaveChanges();
                        return Content("ACTIVATED");
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                System.Diagnostics.Debug.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                            }
                        }   
                    }

                }
                    return Content(model.activationCode);
            }

            return View();
        }
    }
}