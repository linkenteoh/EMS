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

namespace EventManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        DBEntities db = new DBEntities();
        // GET: Admin
        //update
        //-- helper func
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
            string path = Server.MapPath($"~/Images/{name}");

            System.IO.File.Delete(path);
        }
        private string SavePhoto(HttpPostedFileBase f)
        {
            //generate unique id
            string name = Guid.NewGuid().ToString("n") + ".jpg";
            string path = Server.MapPath($"~/Images/{name}");

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
        //--------------------------------------------------
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DisplayEvent(int page = 1)
        {
            Func<Event, object> fn = e => e.Id;


            var events = db.Events.OrderBy(fn);
            var model = events.ToPagedList(page, 10);

            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult InsertEvent()
        {
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
            if (ModelState.IsValid)
            {
                int duration = ((int)model.endTime.TotalMinutes - (int)model.startTime.TotalMinutes) / 60;
                var e = new Event
                {
                    name = model.name,
                    des = model.des,
                    price = model.price,
                    availability = model.availability,
                    participants = model.participants,
                    startDate = model.startDate,
                    endDate = model.endDate,
                    startTime = model.startTime,
                    endTime = model.endTime,
                    duration = duration.ToString(),
                    organized_by = model.organized_by,
                    approvalStat = model.approvalStat,
                    status = true,
                    venueId = null,
                    photoURL = SavePhoto(model.Photo)
                };
                // TempData["Info"] = "Event record added successfully!";
                db.Events.Add(e);
                db.SaveChanges();
                TempData["info"] = "Event record inserted successfully";
                return RedirectToAction("DisplayEvent", "Admin");

            }
            else
            {
                TempData["Error"] = "Error";
            }
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult EditEvent(int id)
        {
            var e = db.Events.Find(id);
            if (e == null)
            {
                return RedirectToAction("Index", "Admin");
            }
            /*            int duration = ((int)e.endTime.TotalMinutes - (int)e.startTime.TotalMinutes) / 60;
            */
            var model = new EventEditVM
            {
                Id = id,
                name = e.name,
                des = e.des,
                price = e.price,
                availability = e.availability,
                participants = e.participants,
                startDate = e.startDate,
                endDate = e.endDate,
                startTime = e.startTime,
                endTime = e.endTime,
                duration = e.duration,
                organized_by = e.organized_by,
                approvalStat = e.approvalStat,
                photoURL = e.photoURL,
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult EditEvent(EventEditVM model)
        {
            var e = db.Events.Find(model.Id);
            if (model == null)
            {
                return RedirectToAction("Index", "Admin");
            }
            int duration = ((int)model.endTime.TotalMinutes - (int)model.startTime.TotalMinutes) / 60;
            if (ModelState.IsValid)
            {
                e.name = model.name;
                e.des = model.des;
                e.price = model.price;
                e.availability = model.availability;
                e.participants = model.participants;
                e.startDate = model.startDate;
                e.endDate = model.endDate;
                e.startTime = model.startTime;
                e.endTime = model.endTime;
                e.duration = duration.ToString();
                e.organized_by = model.organized_by;
                e.approvalStat = model.approvalStat;
                if (model.Photo != null)
                {
                    DeletePhoto(e.photoURL);
                    e.photoURL = SavePhoto(model.Photo);
                }
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
        public ActionResult DisplayUser(int page = 1)
        {
            Func<User, object> fn = e => e.Id;


            var users = db.Users.OrderBy(fn);
            var model = users.ToPagedList(page, 10);

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
            int id = db.Users.Count() +1;

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
                    contact_no = model.contact_no,
                    email = model.email,
                    username = model.username,
                    password = model.password,
                    role = model.role.ToString(),
                    organizer = null,
                    status = true,
                    recoveryCode = "ABCDEF",
                    activationCode = "ABCDEF",
                    photo = SavePhoto(model.Photo),
                    activated = true
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
                Id = id,
                name = u.name,
                contact_no = u.contact_no,
                email = u.email,
                username = u.username,
                password = u.password,
                confirmPassword = u.password,
                role = (Role)Enum.Parse(typeof(Role), u.role),
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
                u.contact_no = model.contact_no;
                u.email = model.email;
                u.password = model.password;
                u.role = model.role.ToString();
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

         /* public ActionResult DeleteAdvert()
          { 
              db.Advertisements.RemoveRange(db.Advertisements);
              db.SaveChanges();
              db.Database.ExecuteSqlCommand(@"DBCC CHECKIDENT([Advertisement],RESEED,0);");

              return View();
          }*/
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
        public ActionResult DisplayAdvert(int page = 1)
        {
            Func<Advertisement, object> fn = a => a.Id;


            var advertisement = db.Advertisements.OrderBy(fn);
            var model = advertisement.ToPagedList(page, 10);

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult InsertAdvert()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult InsertAdvert(AdvertManageVM model)
        {
            var id = db.Users.FirstOrDefault(u => u.username == User.Identity.Name).Id;
            string error = ValidatePhoto(model.Photo);
            if (error != null)
            {
                ModelState.AddModelError("Photo", error);
            }
            if (ModelState.IsValid)
            {
                int duration = ((int)model.endTime.TotalMinutes - (int)model.startTime.TotalMinutes) / 60;
                var a = new Advertisement
                {
                    name = model.name,
                    des = model.des,
                    charge = model.charge,
                    startDate = model.startDate,
                    endDate = model.endDate,
                    startTime = model.startTime,
                    endTime = model.endTime,
                    duration = duration.ToString(),
                    status = true,
                    userId = id,    
                    photoURL = SavePhoto(model.Photo)
                };
                // TempData["Info"] = "Event record added successfully!";
                db.Advertisements.Add(a);
                db.SaveChanges();
                TempData["info"] = "Advertisement record inserted successfully";
                return RedirectToAction("DisplayAdvert", "Admin");

            }
            else
            {
                TempData["Error"] = "Error";
            }
            return View(model);
        }

        public ActionResult EditAdvert(int id)
        {
            var a = db.Advertisements.Find(id);
            if (a == null)
            {
                return RedirectToAction("Index", "Admin");
            }

            var model = new AdvertManageVM
            {
                Id = id,
                name = a.name,
                des = a.des,
                charge = a.charge,
                startDate = a.startDate,
                endDate = a.endDate,
                startTime = a.startTime,
                endTime = a.endTime,
                duration = a.duration,
                userId = a.userId,
                photoURL = a.photoURL,
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditAdvert(AdvertManageVM model)
        {
            var a = db.Advertisements.Find(model.Id);
            if (model == null)
            {
                return RedirectToAction("Index", "Admin");
            }
            int duration = ((int)model.endTime.TotalMinutes - (int)model.startTime.TotalMinutes) / 60;
            if (ModelState.IsValid)
            {
                a.name = model.name;
                a.des = model.des;
                a.charge = model.charge;               
                a.startDate = model.startDate;
                a.endDate = model.endDate;
                a.startTime = model.startTime;
                a.endTime = model.endTime;
                a.duration = duration.ToString();
                if (model.Photo != null)
                {
                    DeletePhoto(a.photoURL);
                    a.photoURL = SavePhoto(model.Photo);
                }
                db.SaveChanges();
                TempData["info"] = "Advertisement record updated successfully";
                return RedirectToAction("DisplayAdvert", "Admin");
            }
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