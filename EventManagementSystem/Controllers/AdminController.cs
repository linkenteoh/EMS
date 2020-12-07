using EventManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;
using System.Net;
using System.IO;
using System.Configuration;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.AspNet.Identity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Mail;
using System.Security.Cryptography;

namespace EventManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        DBEntities db = new DBEntities();
        // GET: Admin
        //update
        //-- helper func

        PasswordHasher ph = new PasswordHasher();

        private string HashPassword(string password)
        {
            return ph.HashPassword(password);
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
        private void DeletePhoto(string name)
        {
            name = System.IO.Path.GetFileName(name);
            string path = Server.MapPath($"~/Photo/{name}");

            System.IO.File.Delete(path);
        }
        private string SavePhoto(HttpPostedFileBase f)
        {
            //generate unique id
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

            img.Resize(201, 201).Crop(1, 1).Save(path, "jpeg");
            return name;
        }
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult OrganizerData()
        {

            var dt = db.Registrations.Where(a => a.userId == a.User.Id).GroupBy(o => o.Event.Organiser.represent).ToList().Select(g => new object[] {

             g.Key,
                g.Count(s => s.User.memberRole == "Student"),
                g.Count(s => s.User.memberRole == "Outsider")

            });

            return Json(dt, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Dashboard2()
        {
            /*           Func<Event, object> fn = s => s.Id;*/

            var model = db.Events.Where(ev => ev.OrgId == ev.Organiser.User.Id);
            /*            var e = model.OrderBy(fn).ToPagedList(page, 10);*/
            return View(model);
        }

        public ActionResult OrganizerData2()
        {
            var dt = db.Users.Where(u => u.Id == u.Organiser.Id).GroupBy(u => u.memberRole).ToList().Select(g => new object[] {
                g.Key,
                g.Count(s => s.Organiser.Events.Any())

            });


            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RoleCount()
        {
            return View();
        }

        public ActionResult RoleCountData()
        {
            var dt = db.Users
                .GroupBy(e => e.role)
                .ToList()
                .Select(g => new object[] {
                    g.Key,
                    g.Count()
                });
            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RevenueGainInEvent()
        {

            return View();
        }

        public ActionResult RevenueGainInEventData()
        {

            var dt = db.Events
                .GroupBy(e => e.name)
                .ToList()
                .Select(g => new object[] {
                    g.Key.ToString(),
                    g.Sum(e => e.price * e.availability)

                });
            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        // Wen Jun Report
        public ActionResult DataSet1(int year = 0)
        {
            var dt = db.Events
              .Where(e => e.venueId != null && e.date.Year == year)
              .GroupBy(e => e.Venue.name)
              .OrderByDescending(e => e.Count())
              .ToList()
              .Select(g => new object[]
              {
                g.Key.ToString(),
                g.Count(),
              });
            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MostBookedVenueReport()
        {
            // Get Year
            int min = DateTime.Today.Year;
            int max = DateTime.Today.Year;

            if (db.Events.Count() > 0)
            {
                min = db.Events.Min(e => e.date).Year;
                max = db.Events.Max(e => e.date).Year;
            }
            ViewBag.YearList = GetYearList(min, max);
            return View();
        }

        private SelectList GetMonthList()
        {
            var items = new List<object>();
            for (int n = 1; n <= 12; n++)
            {
                items.Add(new { Id = n, Name = new DateTime(1, n, 1).ToString("MMMM") });
            }
            return new SelectList(items, "Id", "Name");
        }

        // Return select list for years
        private SelectList GetYearList(int min, int max, bool reverse = false)
        {
            var items = new List<int>();
            for (int n = min; n <= max; n++)
            {
                items.Add(n);
            }
            if (reverse) items.Reverse();
            return new SelectList(items);
        }
        private string GetMonthAbbr(int n)
        {
            return new DateTime(1, n, 1).ToString("MMM");
        }

        public ActionResult DataSet2(int year = 0, int month = 0)
        {
            var dt = db.Events
                .Where(e => e.venueId != null && e.date.Year == year &&
                e.date.Month == month)
                .GroupBy(e => e.date)
                .ToList()
                .Select(g => new object[] {
                    g.Key.ToString("yyyy-MM-dd"),
                    g.Count()
                });

            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VBMonthYearReport()
        {
            // Return available years
            int min = DateTime.Today.Year;
            int max = DateTime.Today.Year;
            if (db.Events.Count() > 0)
            {
                min = db.Events.Min(e => e.date).Year;
                max = db.Events.Max(e => e.date).Year;
                // retrieve largest month and year from DB
                ViewBag.Year = db.Events.Max(e => e.date).Year;
                ViewBag.Month = db.Events.Max(e => e.date).Month;
            }
            ViewBag.YearList = GetYearList(min, max);
            // Return months
            ViewBag.MonthList = GetMonthList();
            // TODO: Default selections (largest year and month)

            return View();
        }
        public ActionResult RegistrationsReport()
        {
            // Return available years
            int min = DateTime.Today.Year;
            int max = DateTime.Today.Year;
            if (db.Registrations.Count() > 0)
            {
                min = db.Registrations.Min(o => o.date).Year;
                max = db.Registrations.Max(o => o.date).Year;

                ViewBag.Year = db.Registrations.Max(o => o.date).Year;
                ViewBag.Month = db.Registrations.Max(o => o.date).Month;
            }
            ViewBag.YearList = GetYearList(min, max);

            // Return months
            ViewBag.MonthList = GetMonthList();

            return View();
        }

        public ActionResult CommissionsReport()
        {
            // Return available years
            int min = DateTime.Today.Year;
            int max = DateTime.Today.Year;
            if (db.Registrations.Count() > 0)
            {
                min = db.Payments.Min(o => o.paymentdate).Value.Year;
                max = db.Payments.Max(o => o.paymentdate).Value.Year;

                ViewBag.Year = db.Registrations.Max(o => o.date).Year;
                ViewBag.Month = db.Registrations.Max(o => o.date).Month;
            }
            ViewBag.YearList = GetYearList(min, max);

            // Return months
            ViewBag.MonthList = GetMonthList();

            return View();
        }

        public ActionResult CommissionData(int year = 0, int month = 0)
        {
            var dict = new Dictionary<string, decimal>
            {
                ["Jan"] = 0m,
                ["Feb"] = 0m,
                ["Mar"] = 0m,
                ["Apr"] = 0m,
                ["May"] = 0m,
                ["Jun"] = 0m,
                ["Jul"] = 0m,
                ["Aug"] = 0m,
                ["Sep"] = 0m,
                ["Oct"] = 0m,
                ["Nov"] = 0m,
                ["Dec"] = 0m,
            };

            db.Payments
                .Where(p => p.paymentdate.Value.Year == year)
                .GroupBy(p => p.paymentdate.Value.Month)
                .ToList()
                .ForEach(p => dict[GetMonthAbbr(p.Key)] = p.Sum(pay => pay.commision));


            var dt = dict.Select(i => new object[] {
                            i.Key,
                            i.Value
                        });
            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RegistrationData(int year = 0, int month = 0)
        {
            var dt = db.Registrations
                    .Where(r => r.date.Year == year && r.date.Month == month)
                    .GroupBy(r => r.date)
                    .ToList()
                    .Select(g => new object[] {
                        g.Key.ToString("dd-MM-yyyy"),
                        g.Count()
                    });

            return Json(dt, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult DisplayProposalApporval(string searchName = "", string name = "", string startDate = "", string endDate = "",
            string startTime = "", string endTime = "", string venue = "", string sort = "", int page = 1)
        {
            ViewBag.VenueList = new SelectList(db.Venues, "name", "name");
            ViewBag.sortList = new SelectList(
                new List<SortItems> {
                    new SortItems { Id = "Id", name = "Id"},
                    new SortItems { Id = "name", name = "Name"},
                    new SortItems { Id = "price", name = "Price"},
                    new SortItems { Id = "date", name = "Date"},
                    new SortItems { Id = "venue", name = "Venue"}
                }, "Id", "name");
            var model = db.Events.Where(u => u.approvalStat == null);

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
                case "Id": fn = s => s.Id; break;
                case "name": fn = s => s.name; break;
                case "price": fn = s => s.price; break;
                case "date": fn = s => s.date; break;
                case "venue": fn = s => s.venueId; break;
            }
            // PagedList
            var events = model.OrderBy(fn).ToPagedList(page, 10);

            if (Request.IsAjaxRequest())
                return PartialView("_EventApproveList", events);
            return View(events);

        }
        [Authorize(Roles = "Admin")]
        public ActionResult ApproveProposal(int id)
        {
            var e = db.Events.Find(id);
            if (e != null)
            {
                e.status = true;
                e.approvalStat = true;
                db.SaveChanges();
                TempData["info"] = "Proposal approved ";
            }

            var url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult DeclineProposal(int id)
        {
            var e = db.Events.Find(id);
            if (e != null)
            {
                e.status = false;
                e.approvalStat = false;
                db.SaveChanges();
                TempData["info"] = "Proposal denied";
            }

            var url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult DisplayEvent(string searchName = "", string name = "", string startDate = "", string endDate = "",
            string startTime = "", string endTime = "", string venue = "", string sort = "", int page = 1)
        {
            ViewBag.VenueList = new SelectList(db.Venues, "name", "name");
            ViewBag.sortList = new SelectList(
                new List<SortItems> {
                    new SortItems { Id = "Id", name = "Id"},
                    new SortItems { Id = "name", name = "Name"},
                    new SortItems { Id = "price", name = "Price"},
                    new SortItems { Id = "date", name = "Date"},
                    new SortItems { Id = "Venue.name", name = "Venue"}
                }, "Id", "name");

            var model = db.Events.Where(e => e.status == true && e.venueId != null);

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
                case "Id": fn = s => s.Id; break;
                case "name": fn = s => s.name; break;
                case "price": fn = s => s.price; break;
                case "date": fn = s => s.date; break;
                case "Venue.name": fn = s => s.venueId; break;
            }
            // PagedList
            var events = model.OrderBy(fn).ToPagedList(page, 10);

            if (Request.IsAjaxRequest())
                return PartialView("_EventListAdmin", events);
            return View(events);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult InsertEvent()
        {
            ViewBag.OrganizerList = new SelectList(db.Organisers.Where(o => o.status == true), "Id", "represent");
            ViewBag.VenueList = new SelectList(db.Venues, "Id", "name");

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult InsertEvent(EventInsertVM model)
        {

            string error = ValidatePhoto(model.Photo);
            if (error != null)
            {
                ModelState.AddModelError("Photo", error);
            }
            if (ModelState.IsValidField("startTime"))
            {
                if (model.startTime > model.endTime)
                {
                    ModelState.AddModelError("startTime", "start time cannot exceed or equal end time!");
                }
            }
            if (ModelState.IsValid)
            {
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
                    approvalStat = true,
                    status = true,
                    venueId = model.venueId,
                    photoURL = SavePhoto(model.Photo),
                    OrgId = model.OrgId
                };
                try
                {
                    db.Events.Add(e);
                    db.SaveChanges();
                    TempData["info"] = "Event added successfully";
                    return RedirectToAction("DisplayEvent", "Admin");
                }
                catch (Exception ex)
                {
                    TempData["Info"] = ex;
                }

            }
            else
            {
                TempData["Error"] = "Error";
            }
            ViewBag.OrganizerList = new SelectList(db.Organisers.Where(o => o.status == true), "Id", "represent");
            ViewBag.VenueList = new SelectList(db.Venues, "Id", "name");
            return View(model);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult EditEvent(int id)
        {
            ViewBag.OrganizerList = new SelectList(db.Organisers.Where(o => o.status == true), "Id", "represent");
            ViewBag.VenueList = new SelectList(db.Venues, "Id", "name");

            var e = db.Events.Find(id);
            if (e == null)
            {
                return RedirectToAction("Index", "Admin");
            }

            var model = new EventEditVM
            {
                Id = id,
                name = e.name,
                des = e.des,
                participants = e.participants,
                price = e.price,
                date = e.date,
                startTime = e.startTime,
                endTime = e.endTime,
                photoURL = e.photoURL,
                OrgId = e.OrgId,
                venueId = e.venueId
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult EditEvent(EventEditVM model)
        {
            var e = db.Events.Find(model.Id);
            ViewBag.OrganizerList = new SelectList(db.Organisers.Where(o => o.status == true), "Id", "represent");
            ViewBag.VenueList = new SelectList(db.Venues, "Id", "name");
            if (ModelState.IsValidField("startTime"))
            {
                if (model.startTime > model.endTime)
                {
                    ModelState.AddModelError("startTime", "start time cannot exceed or equal end time!");
                }
            }
            if (model == null)
            {
                return RedirectToAction("Index", "Admin");
            }
            if (ModelState.IsValid)
            {
                e.name = model.name;
                e.des = model.des;
                e.price = model.price;
                e.participants = e.participants;
                e.availability = e.availability;
                e.date = model.date;
                e.startTime = model.startTime;
                e.endTime = model.endTime;
                e.approvalStat = true;
                if (model.Photo != null)
                {
                    DeletePhoto(e.photoURL);
                    e.photoURL = SavePhoto(model.Photo);
                }
                e.OrgId = model.OrgId;
                e.venueId = model.venueId;
                db.SaveChanges();
                TempData["info"] = "Event record updated successfully";
                return RedirectToAction("DisplayEvent", "Admin");
            }
            return View(model);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult DeleteEvent(int id)
        {
            var e = db.Events.Find(id);
            if (e != null)
            {
                e.status = false;
                db.SaveChanges();
                TempData["info"] = "Event record deleleted successfully";
            }

            var url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DisplayUser(string searchName = "", string name = "", string username = "", string email = "",
            string role = "", string sort = "", int page = 1)
        {
            var users = db.Users.Where(m => m.name.Contains(name));

            // Username
            if (!string.IsNullOrEmpty(username))
            {
                users = users.Where(m => m.username.Contains(username));
            }
            // Email
            if (!string.IsNullOrEmpty(email))
            {
                users = users.Where(m => m.email.Contains(email));
            }
            // Role
            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(m => m.role.Contains(role));
            }

            ViewBag.RoleList = new SelectList(
                new List<SortItems> {
                    new SortItems { Id = "Student", name = "Student"},
                    new SortItems { Id = "Admin", name = "Admin"},
                    new SortItems { Id = "Staff", name = "Staff"},
                }, "Id", "name");

            ViewBag.sortList = new SelectList(
                new List<SortItems> {
                    new SortItems { Id = "Id", name = "Id"},
                    new SortItems { Id = "name", name = "Name"},
                    new SortItems { Id = "email", name = "Email"},
                    new SortItems { Id = "username", name = "Username"},
                    new SortItems { Id = "role", name = "Role"},
                }, "Id", "name");

            // Search name
            users = users.Where(x => x.name.Contains(searchName) || x.username.Contains(searchName) || x.email.Contains(searchName));

            Func<User, object> fn = e => e.Id;

            switch (sort)
            {
                case "Id": fn = s => s.Id; break;
                case "name": fn = s => s.name; break;
                case "email": fn = s => s.email; break;
                case "username": fn = s => s.username; break;
                case "role": fn = s => s.role; break;
            }
            // PagedList
            var model = users.OrderBy(fn).ToPagedList(page, 10);

            if (Request.IsAjaxRequest())
                return PartialView("_UserList", model);
            return View(model);

        }
        [Authorize(Roles = "Admin")]
        public ActionResult InsertUser()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult InsertUser(UserInsertVM model)
        {
            int id = db.Users.Count() + 1;

            string error = ValidatePhoto(model.Photo);
            if (error != null)
            {
                ModelState.AddModelError("Photo", error);
            }
            if (ModelState.IsValid)
            {
                var u = new User
                {
                    Id = id,
                    name = model.name,
                    contact_no = model.contact_no.Trim(),
                    email = model.email,
                    username = model.username,
                    password = HashPassword(model.password),
                    role = model.role.ToString(),
                    memberRole = model.memberRole.ToString(),
                    status = true,
                    recoveryCode = null,
                    activationCode = null,
                    photo = SavePhoto(model.Photo),
                    activated = true,
                    lockoutValue = 0
                };

                db.Users.Add(u);
                db.SaveChanges();
                TempData["info"] = "User record inserted successfully";
                return RedirectToAction("DisplayUser", "Admin");
            }
            else
            {
                TempData["Error"] = "Error";
            }
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult EditUser(int id)
        {
            var u = db.Users.Find(id);

            if (u == null)
            {
                return RedirectToAction("Index", "Admin");
            }

            var model = new UserEditVM
            {
                name = u.name,
                contact_no = u.contact_no.Trim(),
                email = u.email,
                username = u.username,
                password = u.password,
                role = (Role)Enum.Parse(typeof(Role), u.role),
                memberRole = (MemberRole)Enum.Parse(typeof(MemberRole), u.memberRole),
                photoURL = u.photo,
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult EditUser(UserEditVM model)
        {
            var u = db.Users.Find(model.Id);

            if (model == null)
            {
                return RedirectToAction("Index", "Admin");
            }
            if (ModelState.IsValid)
            {

                u.name = model.name;
                u.contact_no = model.contact_no.Trim();
                u.email = model.email;
                if (model.newPassword == null)
                {
                    u.password = u.password;
                }
                else
                {
                    u.password = HashPassword(model.newPassword);
                }

                u.role = model.role.ToString();
                u.memberRole = model.memberRole.ToString();
                if (model.Photo != null)
                {
                    DeletePhoto(u.photo);
                    u.photo = SavePhoto(model.Photo);

                }
                db.SaveChanges();

                TempData["info"] = "User record updated successfully";
                return RedirectToAction("DisplayUser", "Admin");
            }
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteUser(int id)
        {

            var u = db.Users.Find(id);
            if (u != null)
            {
                u.status = false;
                db.SaveChanges();
                TempData["info"] = "User record deleleted successfully";
            }

            var url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult DisplayOrganizerApproval(string searchName = "", string rep = "", string position = "", string sort = "", int page = 1)
        {
            //ViewBag.positionList = new SelectList(
            //  new List<SortItems> {
            //        new SortItems { Id = "President", name = "Id"},
            //        new SortItems { Id = "Secretary", name = "Representative"},
            //        new SortItems { Id = "Treasurer", name = "Position"},
            //  }, "Id", "name");
            ViewBag.sortList = new SelectList(
              new List<SortItems> {
                    new SortItems { Id = "Id", name = "Id"},
                    new SortItems { Id = "represent", name = "Representative"},
                    new SortItems { Id = "position", name = "Position"},
              }, "Id", "name");

            var organisers = db.Organisers.Where(x => x.represent.Contains(searchName) || x.position.Contains(searchName));

            Func<Organiser, object> fn = e => e.Id;
            switch (sort)
            {
                case "Id": fn = s => s.Id; break;
                case "represent": fn = s => s.represent; break;
                case "position": fn = s => s.position; break;
            }
            var model = organisers.OrderBy(fn).ToPagedList(page, 10);

            if (Request.IsAjaxRequest())
                return PartialView("_OrganiserApproveList", model);
            return View(model);

        }

        public ActionResult ApproveOrganizer(int id)
        {
            var e = db.Organisers.Find(id);
            var user = db.Users.Find(id);
            if (e != null)
            {
                e.status = true;
                user.role = "Organizer";
                db.SaveChanges();
                TempData["info"] = "Request Approved!";
            }

            var url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult DeclineOrganizer(int id)
        {
            var e = db.Organisers.Find(id);
            if (e != null)
            {
                e.status = false;
                db.SaveChanges();
                TempData["info"] = "Request Declined!";
            }

            var url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }
        /* public ActionResult DeleteAdvert()
         { 
             db.Advertisements.RemoveRange(db.Advertisements);
             db.SaveChanges();
             db.Database.ExecuteSqlCommand(@"DBCC CHECKIDENT([Advertisement],RESEED,0);");

             return View();
         }*/

        [Authorize(Roles = "Admin")]
        public ActionResult DisplayAdvert(string searchName = "", string name = "", string desc = "",
            string startDate = "", string endDate = "", string sort = "", int page = 1)
        {
            ViewBag.sortList = new SelectList(
               new List<SortItems> {
                    new SortItems { Id = "Id", name = "Id"},
                    new SortItems { Id = "name", name = "Name"},
                    new SortItems { Id = "startDate", name = "Start Date"},
                    new SortItems { Id = "endDate", name = "End Date"}
               }, "Id", "name");

            var advertisement = db.Advertisements.Where(a => a.status == true);

            // Name
            if (!string.IsNullOrEmpty(name))
            {
                advertisement = advertisement.Where(m => m.Event.name.Contains(name));
            }
            // Start Date && End Date
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                var timeFrom = DateTime.Parse(startDate);
                var timeTo = DateTime.Parse(endDate);
                advertisement = advertisement.Where(x => x.startDate >= timeFrom && x.endDate <= timeTo);
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                var timeFrom = DateTime.Parse(startDate);
                advertisement = advertisement.Where(x => x.startDate >= timeFrom);
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                var timeTo = DateTime.Parse(endDate);
                advertisement = advertisement.Where(x => x.endDate <= timeTo);
            }
            // Search name
            advertisement = advertisement.Where(x => x.Event.name.Contains(searchName));
            // Sort By
            Func<Advertisement, object> fn = s => s.Id;
            switch (sort)
            {
                case "Id": fn = s => s.Id; break;
                case "Event Name": fn = s => s.Event.name; break;
                case "startDate": fn = s => s.startDate; break;
                case "endDate": fn = s => s.endDate; break;
            }

            var model = advertisement.OrderBy(fn).ToPagedList(page, 10);

            if (Request.IsAjaxRequest())
                return PartialView("_AdsList", model);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult InsertAdvert()
        {
            var id = db.Events.FirstOrDefault().Id;

            ViewBag.EventList = new SelectList(db.Events, "Id", "name");
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult InsertAdvert(AdvertManageVM model)
        {

            if (ModelState.IsValidField("startDate"))
            {
                if (model.startDate > model.endDate)
                {
                    ModelState.AddModelError("startDate", "start date cannot exceed end date!");
                }

            }
            var adv = db.Advertisements.Find(model.Id);
            if (adv != null)
            {
                ModelState.AddModelError("Id", "This event had been promoted! Please choose other options!");
            }
            if (ModelState.IsValid)
            {
                var a = new Advertisement
                {
                    Id = model.Id,
                    startDate = model.startDate,
                    endDate = model.endDate,
                    status = true,
                };
                db.Advertisements.Add(a);
                db.SaveChanges();
                TempData["info"] = "Advertisement record inserted successfully";
                return RedirectToAction("DisplayAdvert", "Admin");

            }
            else
            {
                TempData["Error"] = "Error";
            }
            ViewBag.EventList = new SelectList(db.Events, "Id", "name");
            return View(model);
        }

        public ActionResult EditAdvert(int id)
        {
            ViewBag.EventList = new SelectList(db.Events, "Id", "name");
            var a = db.Advertisements.Find(id);
            if (a == null)
            {
                return RedirectToAction("Index", "Admin");
            }

            var model = new AdvertManageVM
            {
                Id = id,
                startDate = a.startDate,
                endDate = a.endDate,
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditAdvert(AdvertManageVM model)
        {
            var eventID = db.Events.FirstOrDefault(e => e.Id == model.Id).Id;
            var a = db.Advertisements.FirstOrDefault(o => o.Id == eventID);
            if (ModelState.IsValidField("startDate"))
            {
                if (model.startDate > model.endDate)
                {
                    ModelState.AddModelError("startDate", "start date cannot exceed end date!");
                }

            }
            if (model == null)
            {
                return RedirectToAction("Index", "Admin");
            }
            if (ModelState.IsValid)
            {
                a.Id = model.Id;
                a.startDate = model.startDate;
                a.endDate = model.endDate;
                db.SaveChanges();
                TempData["info"] = "Advertisement record updated successfully";
                return RedirectToAction("DisplayAdvert", "Admin");
            }
            ViewBag.EventList = new SelectList(db.Events, "Id", "name");
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteAdvert(int id)
        {
            var a = db.Advertisements.Find(id);
            if (a != null)
            {
                a.status = false;
                db.SaveChanges();
                TempData["info"] = "Advertisement record deleleted successfully";
            }

            var url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }
    }
}