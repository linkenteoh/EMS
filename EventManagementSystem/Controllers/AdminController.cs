using EventManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using EventManagementSystem.Models;

namespace EventManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        DBEntities db = new DBEntities();
        // GET: Admin

        //-- helper func
        private string ValidatePhoto(HttpPostedFileBase f)
        {
            var reType = new Regex(@"^image\/(jpeg|png)$", RegexOptions.IgnoreCase); ;
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

        [HttpPost]
        public ActionResult Insert(EventInsertVM model)
        {   
            var id = db.Events.Count() + 1;
            string error = ValidatePhoto(model.Photo);
            if(error != null)
            {
                ModelState.AddModelError("Photo", error);
            }
            if (ModelState.IsValid)
            {
                var e = new Event
                {
                    Id = id,
                    name = model.name,
                    des = model.des,
                    price = model.price,
                    availability = model.availability,
                    participants = model.participants,
                    startDate = model.startDate,
                    endDate = model.endDate,
                    startTime = model.startTime,
                    endTime = model.endTime,
                    duration = "2",
                    organized_by = model.organized_by,
                    approvalStat = false,
                    status = true,
                    venueId = 1,
                    photoURL = SavePhoto(model.Photo)
                }; 
                // TempData["Info"] = "Event record added successfully!";
                try
                {
                    db.Events.Add(e);
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
                return RedirectToAction("Index","Admin");
            }

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