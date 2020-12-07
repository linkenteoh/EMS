﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Security.Claims;
using EventManagementSystem.Models;
using EventManagementSystem.reCAPTCHA;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using PagedList;
using Microsoft.AspNet.Identity;
using System.Data.Entity.Validation;

using QRCoder;
using System.Drawing;
using System.Net.Mail;
using System.Text;

namespace EventManagementSystem.Controllers
{
    public class UserController : Controller
    {
        DBEntities db = new DBEntities();
        PasswordHasher ph = new PasswordHasher();

        private string HashPassword(string password)
        {
            return ph.HashPassword(password);
        }

        // GET: User
        public ActionResult Index()
        {
            return View();
        }
        // GET: User/RegOrganiser
        [Authorize]
        public ActionResult RegOrganiser()
        {
            int id = db.Users.FirstOrDefault(u => u.username == User.Identity.Name).Id;

            return View();
        }

        // POST: User/RegOrganiser
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateGoogleCaptcha]
        [HttpPost]
        public ActionResult RegOrganiser(RegOrgVM model)
        {
            int id = db.Users.FirstOrDefault(u => u.username == User.Identity.Name).Id;

            var Org = db.Organisers.Find(id);

            if (Org != null)
            {
                TempData["Info"] = "You've already registered, please wait for your request to be accepted.";
                return RedirectToAction("RegOrganiser", "User");
            }

            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.username == User.Identity.Name).Id;

                var oragniser = new Organiser
                {
                    Id = user,
                    represent = model.represent,
                    position = model.position,
                    status = null
                };
                
                db.Organisers.Add(oragniser);
                db.SaveChanges();
                TempData["Info"] = "You've registered successfully. Please wait until it is accepted by an admin.";
            }
            return View(model);
        }

        private string ValidatePhoto(HttpPostedFileBase f)
        {
            var reType = new Regex(@"^image\/(jpeg|png)$", RegexOptions.IgnoreCase);
            var reName = new Regex(@"^.+\.(jpg|jpeg|png)$", RegexOptions.IgnoreCase);

            if (f == null)
            {
                return "No photo selected.";
            }
            else if (!reType.IsMatch(f.ContentType) ||
               !reName.IsMatch(f.FileName))
            {
                return "Only JPG or PNG photo is allowed.";
            }
            else if (f.ContentLength > 1 * 1024 * 1024)
            {
                return "Photo size cannot more than 1 MB";
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

        private void DeletePhoto(string name)
        {
            name = System.IO.Path.GetFileName(name);
            string path = Server.MapPath($"~/Photo/{name}");
            System.IO.File.Delete(path);
        }


        // Edit users
        public ActionResult Edit()
        {
            var u = db.Users.FirstOrDefault(x => x.username == User.Identity.Name);

            var model = new EditProfileVM
            {
                Id = u.Id,
                username = u.username,
                name = u.name,
                contact_no = u.contact_no.Trim(),
                email = u.email,
                PhotoUrl = u.photo
            };
            return View(model);
        }

        // POST : Edit User
        [HttpPost]
        public ActionResult Edit(EditProfileVM model)
        {
            var u = db.Users.Find(model.Id);
            if (u == null)
            {
                TempData["info"] = model.Id;
                return RedirectToAction("Edit", "User");
            }

            if (ModelState.IsValid)
            {
                u.name = model.name;
                u.username = model.username;
                u.email = model.email;
                u.contact_no = model.contact_no;
                if(model.newPassword != null)
                {
                    u.password = HashPassword(model.newPassword);
                }

                if (model.Photo != null)
                {
                    DeletePhoto(u.photo);
                    u.photo = SavePhoto(model.Photo);
                }

                if (model.webPhoto != null)
                {
                    u.photo = model.webPhoto;
                }


                try
                {
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
                TempData["Info"] = "Profile edited successfully!";
                return RedirectToAction("Edit", "User");
            }
            return View(model);
        }

        public ActionResult EventSearchIndex(string searchName = "", string name = "", string startDate = "", string endDate = "",
            int priceFrom = 0, int priceTo = 0, string startTime = "", string endTime = "", string venue = "", string sort = "", int page = 1)
        {
            ViewBag.VenueList = new SelectList(db.Venues, "name", "name");
            ViewBag.sortList = new SelectList(
                new List<SortItems> { 
                    new SortItems { Id = "name", name = "Name"}, 
                    new SortItems { Id = "price", name = "Price"}, 
                    new SortItems { Id = "date", name = "Date"},
                    new SortItems { Id = "startTime", name = "Start Time"},
                    new SortItems { Id = "endTime", name = "End Time"},
                    new SortItems { Id = "venue", name = "Venue"} 
                },"Id", "name") ;
            var username = User.Identity.Name;
            var u = db.Users.Where(x => x.username == username).FirstOrDefault();
            // Get userID in registration
            var reg = db.Registrations.Where(r => u.Id.Equals(r.userId)).Select(r => r.eventId).ToArray();
            // Get EventID in registration that contains the userID
            var model = db.Events.Where(m => reg.Contains(m.Id));

            // Name
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
            // Price Range
            if (priceFrom != 0 && priceTo != 0)
            {
                model = model.Where(x => x.price >= priceFrom && x.price <= priceTo);
            }
            else if (priceFrom != 0)
            {
                model = model.Where(x => x.price >= priceFrom);
            }
            else if (priceTo != 0)
            {
                model = model.Where(x => x.price <= priceTo);
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
                case "startTime": fn = s => s.startTime; break;
                case "endTime": fn = s => s.endTime; break;
                case "venue": fn = s => s.venueId; break;
            }
            // PagedList
            var events = model.OrderBy(fn).ToPagedList(page, 10);

            if (Request.IsAjaxRequest())
                return PartialView("_EventResults", events);
            return View(events);
        }
        [Authorize(Roles = "Organizer")]
        // GET: User/ProposeEvent
        public ActionResult ProposeEvent()
        {
            ViewBag.OrganizerList = new SelectList(db.Organisers.Where(o => o.status == true), "Id", "represent");
            return View();
        }
        [Authorize(Roles = "Organizer")]
        // POST: User/ProposeEvent
        [HttpPost]
        public ActionResult ProposeEvent(EventProposeVM model)
        {     
            string error = ValidatePhoto(model.Photo);
            if (error != null)
            {
                ModelState.AddModelError("Photo", error);
            }
            if (ModelState.IsValid)
            {
                int id = db.Users.FirstOrDefault(u => u.username == User.Identity.Name).Id;

                var e = new Event
                {
                    name = model.name,
                    des = model.des,
                    price = model.price,
                    participants = model.participants,
                    availability = model.participants,
                    date = model.date,
                    startTime = model.startTime,
                    endTime = model.endTime,
                    approvalStat = null,
                    status = true,
                    venueId = null,
                    photoURL = SavePhoto(model.Photo),
                    OrgId = id
                };
                try { 
                    db.Events.Add(e);
                    db.SaveChanges();
                    TempData["info"] = "Event proposed successfully";
                    return RedirectToAction("ManageEventProposed", "User", new { Id=e.Id });
                }
                catch(Exception ex)
                {
                    TempData["Info"] = ex;
                }
               

            }
            else
            {
                TempData["Error"] = "Error";
            }
            ViewBag.OrganizerList = new SelectList(db.Organisers.Where(o => o.status == true), "Id", "represent");
            return View(model);
        }

        public ActionResult EventsProposed(string searchName = "", string name = "", string startDate = "", string endDate = "",
            string startTime = "", string endTime = "", string status = "", string sort = "", int page = 1)
        {
            ViewBag.statusList = new SelectList(
                new List<SortItems> {
                    new SortItems { Id = "approved", name = "Approved"},
                    new SortItems { Id = "pending", name = "Pending"},
                    new SortItems { Id = "rejected", name = "Rejected"},
                }, "Id", "name");
            ViewBag.sortList = new SelectList(
                new List<SortItems> {
                    new SortItems { Id = "name", name = "Name"},
                    new SortItems { Id = "date", name = "Date"},
                    new SortItems { Id = "startTime", name = "Start Time"},
                    new SortItems { Id = "endTime", name = "End Time"},
                    new SortItems { Id = "status", name = "Status"}
                }, "Id", "name");
            var model = db.Events.Where(u => u.OrgId == db.Users.FirstOrDefault(org => org.username == User.Identity.Name).Id);

            // Name
            if (!string.IsNullOrEmpty(name))
            {
                model = model.Where(m => m.name.Contains(name));
            }
            // Status
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "approved")
                {
                    model = model.Where(x => x.approvalStat == true);
                }
                else if (status == "rejected")
                {
                    model = model.Where(x => x.approvalStat == false);
                }
                else if (status == "pending")
                {
                    model = model.Where(x => x.approvalStat == null);
                }
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
                case "date": fn = s => s.date; break;
                case "startTime": fn = s => s.startTime; break;
                case "endTime": fn = s => s.endTime; break;
                case "status": fn = s => s.approvalStat; break;
            }
            // PagedList
            var events = model.OrderBy(fn).ToPagedList(page, 10);

            if (Request.IsAjaxRequest())
                return PartialView("_EventApproveList", events);
            return View(events);
        }
        [Authorize(Roles = "Organizer")]
        // GET: Event/ManageEventProposed
        public ActionResult ManageEventProposed(int Id)
        {
            var e = db.Events.Find(Id);
            if (e == null)
            {
                return RedirectToAction("EventsProposed", "User");
            }

            var model = new OrgEditEventVM
            {
                Id = Id,
                name = e.name,
                des = e.des,
                price = e.price,
                participants = e.participants,
                date = e.date,
                startTime = e.startTime,
                endTime = e.endTime,
                availability = e.availability,
                approvalStat = e.approvalStat,
                photoURL = e.photoURL,
                OrgId = e.OrgId,
                venueId = e.venueId,
                Organiser = e.Organiser,
                Venue = e.Venue,
                Registrations = e.Registrations
            };

            ViewBag.Registrations = db.Registrations.Where(r => r.eventId == Id);
            ViewBag.ParticipantsCount = db.Registrations.Where(r => r.eventId == Id && r.Payment.status == true).Count();
            ViewBag.PendingCount = db.Registrations.Where(r => r.eventId == Id && r.Payment.status == false).Count();
            ViewBag.Payments = db.Payments.Where(p => p.Registration.eventId == Id);
            ViewBag.PaymentsCount = db.Payments.Where(p => p.Registration.eventId == Id).Count();
            return View(model);
        }
        [Authorize(Roles = "Organizer")]
        // POST: Event/ManageEventProposed
        [HttpPost]
        public ActionResult ManageEventProposed(OrgEditEventVM model)
        {
            var e = db.Events.Find(model.Id);
            if (model == null)
            {
                return RedirectToAction("EventsProposed", "User");
            }
            if (ModelState.IsValid)
            {
                e.name = model.name;
                e.des = model.des;
                e.price = model.price;
                e.availability = model.participants;
                e.participants = model.participants;
                e.date = model.date;
                e.startTime = model.startTime;
                e.endTime = model.endTime;
                if (model.Photo != null)
                {
                    DeletePhoto(e.photoURL);
                    e.photoURL = SavePhoto(model.Photo);
                }
                db.SaveChanges();
                TempData["info"] = "Event record updated successfully";
                return RedirectToAction("ManageEventProposed", "User", new { id = model.Id });
            }
            return View(model);
        }

        public ActionResult AdvertisementSelect(int Id)
        {
            return View();
        }

        public ActionResult Billing(string searchName = "", string name = "", string startDate = "", string  endDate ="",
            int priceFrom = 0, int priceTo = 0, string status = "", string sort = "", int page = 1)
        {
            ViewBag.statusList = new SelectList(
                new List<SortItems> {
                    new SortItems { Id = "paid", name = "Paid"},
                    new SortItems { Id = "unpaid", name = "Unpaid"},
                }, "Id", "name");
            ViewBag.sortList = new SelectList(
                new List<SortItems> {
                    new SortItems { Id = "name", name = "Name"},
                    new SortItems { Id = "price", name = "Price"},
                    new SortItems { Id = "date", name = "Date"},
                    new SortItems { Id = "status", name = "Status"}
                }, "Id", "name");
            int uId = db.Users.FirstOrDefault(u => u.username == User.Identity.Name).Id;
            var model = db.Payments.Where(p => p.Registration.userId == uId);

            // Event Name
            if (!string.IsNullOrEmpty(name))
            {
                model = model.Where(x => x.Registration.Event.name.Contains(name));
            }
            // Date Range
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                var timeFrom = DateTime.Parse(startDate);
                var timeTo = DateTime.Parse(endDate);
                model = model.Where(x => x.Registration.Event.date >= timeFrom && x.Registration.Event.date <= timeTo);
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                var timeFrom = DateTime.Parse(startDate);
                model = model.Where(x => x.Registration.Event.date >= timeFrom);
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                var timeTo = DateTime.Parse(endDate);
                model = model.Where(x => x.Registration.Event.date <= timeTo);
            }
            // Price Range
            if (priceFrom != 0 && priceTo != 0)
            {
                model = model.Where(x => x.Registration.Event.price >= priceFrom && x.Registration.Event.price <= priceTo);
            }
            else if (priceFrom != 0)
            {
                model = model.Where(x => x.Registration.Event.price >= priceFrom);
            }
            else if (priceTo != 0)
            {
                model = model.Where(x => x.Registration.Event.price <= priceTo);
            }
            // Status
            if (!string.IsNullOrEmpty(status))
            {
                if(status == "paid")
                {
                    model = model.Where(x => x.status == true);
                }
                else if(status == "unpaid")
                {
                    model = model.Where(x => x.status == false);
                }
            }

            // Search name
            model = model.Where(x => x.Registration.Event.name.Contains(searchName));
            // Sort By
            Func<Payment, object> fn = s => s.Id;
            switch (sort)
            {
                case "name": fn = s => s.Registration.Event.name; break;
                case "price": fn = s => s.price; break;
                case "date": fn = s => s.Registration.Event.date; break;
                case "status": fn = s => s.status; break;
            }

            // PagedList
            var bill = model.OrderBy(fn).ToPagedList(page, 10);

            if (Request.IsAjaxRequest())
                return PartialView("_BillingList", bill);
            return View(bill);
        }

        // GET: User/Payment
        public ActionResult Payment(int id)
        {
            var payment = db.Payments.Find(id);

            var model = new PaymentVM
            {
                price = payment.price,
                addCharge = payment.addCharge,
                Registration = payment.Registration
            };

            return View(model);
        }

        // POST: User/Payment
        [HttpPost]
        public ActionResult Payment(PaymentVM model)
        {
            if (ModelState.IsValid)
            {
                var payment = db.Payments.Find(model.Id);
                payment.status = true;
                payment.paymentdate = DateTime.Now;
                db.SaveChanges();

                int eveId = payment.Registration.eventId;
                var eve = db.Events.Find(eveId);
                eve.availability = eve.availability - 1;
                db.SaveChanges();
                
                var user = db.Users.FirstOrDefault(u => u.username == User.Identity.Name);
                int eventId = db.Registrations.FirstOrDefault(r => r.Id == model.Id).eventId;
                string link = "https://localhost:44302/Event/EventDetail?id=" + eventId;
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);

                string base64String = null;
                using (Bitmap bitMap = qrCode.GetGraphic(20))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        byte[] imageByte = ms.ToArray();
                        base64String = Convert.ToBase64String(imageByte);
                        SaveImage(base64String, "QR");
                        
                    }
                }    

                string path = Server.MapPath("~/Photo/QR.jpg");
                var att = new Attachment(path);
                MailMessage m = new MailMessage();
                m.Attachments.Add(att);
                string mail = @"";
               
                m.To.Add(user.email);
                m.Subject = "Event Details";
                m.Body = link;
                m.IsBodyHtml = true; //Can send HTML FORMATTED Mail
                new SmtpClient().Send(m);
                TempData["Info"] = "Payment successful! You will receive an email! ";
                return RedirectToAction("Billing", "User");
            }
            return View();
        }

        public bool SaveImage(string base64String, string ImgName)
        {
            String path = Server.MapPath("~/Photo/"); //Path

            //Check if directory exist
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }

            string imageName = ImgName + ".jpg";

            //set the image path
            string imgPath = Path.Combine(path, imageName);

            byte[] imageBytes = Convert.FromBase64String(base64String);

            System.IO.File.WriteAllBytes(imgPath, imageBytes);

            return true;
        }

    }
}