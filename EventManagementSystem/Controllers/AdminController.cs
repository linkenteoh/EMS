using EventManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

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
            string path = Server.MapPath($"~/Photo/{name}");
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
        //--------------------------------------------------
        public ActionResult Index()
        {
            var model = db.Events;
            return View(model);
        }
        public ActionResult InsertEvent()
        {
            return View();
        }
        [HttpPost]
        public ActionResult InsertEvent(EventInsertVM model)
        {   

          string error = ValidatePhoto(model.Photo);
            if(error != null)
            {
                ModelState.AddModelError("Photo", error);
            }
            if (ModelState.IsValid)
            {
                int duration =  ((int)model.endTime.TotalMinutes - (int)model.startTime.TotalMinutes) /60;
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
                return RedirectToAction("Index", "Admin");
           
            }
            else
            {
                TempData["Error"] = "Error";
            }
            return View(model);
        }
        
        public ActionResult EditEvent(int id)
        {
             var e = db.Events.Find(id);
            if(e == null)
            {
                return RedirectToAction("Index","Admin");
            }
/*            int duration = ((int)e.endTime.TotalMinutes - (int)e.startTime.TotalMinutes) / 60;
*/            var model = new EventEditVM
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
        
        
        [HttpPost]
        public ActionResult EditEvent(EventEditVM model)
        {
            var e = db.Events.Find(model.Id);  
            if (model == null)
            {
                return RedirectToAction("Index","Admin");
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
                return RedirectToAction("Index","Admin");
            }
            return View(model);
        }

        public ActionResult DeleteEvent(int id)
        {
            var e = db.Events.Find(id);
            if(e != null)
            {
                db.Events.Remove(e);
                db.SaveChanges();
            }

            var url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }



    }

    
}