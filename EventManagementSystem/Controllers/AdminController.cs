﻿using EventManagementSystem.Models;
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
using ZXing;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Mail;
using QRCoder;
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

            var url = Request.UrlReferrer?.AbsolutePath ?? "/";
           
            return View(url);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult DisplayProposalApporval(int page = 1)
        {
            Func<Event, object> fn = e => e.Id;
            var events = db.Events.OrderBy(fn);
            var model = events.ToPagedList(page, 10);
            return View(model);
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
                    contact_no = model.contact_no.Trim(),
                    email = model.email,
                    username = model.username,
                    password = HashPassword(model.password),
                    role = model.role.ToString(),
                    organizer = model.organizer,
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
                contact_no = u.contact_no.Trim(),
                email = u.email,
                username = u.username,
                password = u.password,
                organizer = u.organizer,
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
                u.contact_no = model.contact_no.Trim();
                u.email = model.email;
                u.organizer = model.organizer;
                if(model.newPassword == null)
                {
                    u.password = u.password;
                }
                else
                {
                    u.password = HashPassword(model.newPassword);
                }
               
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
        [Authorize(Roles = "Admin")]
        public ActionResult DisplayOrganizerApproval(int page = 1)
        {
            Func<Organiser, object> fn = e => e.Id;

            var organisers = db.Organisers.OrderBy(fn);
            var model = organisers.ToPagedList(page, 10);

            return View(model);

        }

        public ActionResult ApproveOrganizer(int id)
        {
            var e = db.Organisers.Find(id);
            if (e != null)
            {
                e.status = true;
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
            if (ModelState.IsValidField("startDate"))
            {
                if (model.startDate > model.endDate)
                {
                    ModelState.AddModelError("startDate", "start date cannot exceed end date!");
                }

            }
            if (ModelState.IsValidField("startTime"))
            {
                if (model.startTime >= model.endTime)
                {
                    ModelState.AddModelError("startTime", "start time cannot exceed or equal end time!");
                }
            }
            string error = ValidatePhoto(model.Photo);
            if (error != null)
            {
                ModelState.AddModelError("Photo", error);
            }
            if (ModelState.IsValid)
            {
                var a = new Advertisement
                {
                    name = model.name,
                    des = model.des,
                    charge = model.charge,
                    startDate = model.startDate,
                    endDate = model.endDate,
                    startTime = model.startTime,
                    endTime = model.endTime,
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
            if (ModelState.IsValidField("startDate"))
            {
                if (model.startDate > model.endDate)
                {
                    ModelState.AddModelError("startDate", "start date cannot exceed end date!");
                }

            }
            if (ModelState.IsValidField("startTime"))
            {
                if (model.startTime >= model.endTime)
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
                a.name = model.name;
                a.des = model.des;
                a.charge = model.charge;               
                a.startDate = model.startDate;
                a.endDate = model.endDate;
                a.startTime = model.startTime;
                a.endTime = model.endTime;
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
            
        public ActionResult Generate()
        {
    /*        string link = "https://localhost:44302/Account/Activation?activationCode=" + user.activationCode + "&userid=" + user.Id;
            string mail = @"";

            MailMessage m = new MailMessage();
            m.To.Add(model.email);
            m.Subject = "Activate your TARUC EMS account";
            m.Body = link;
            m.IsBodyHtml = true; //Can send HTML FORMATTED Mail
            new SmtpClient().Send(m);

            TempData["Info"] = "Registered successfully!";
            return RedirectToAction("Awaitactivation", "Account", new { email = model.email });*/
            return View();
        }
        private string ConvertUrlsToLinks(string msg)
        {
            string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            return r.Replace(msg, "<a href=\"$1\" title=\"Click to open in a new window or tab\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"http://www");
        }
        [HttpPost]
        public ActionResult Generate(QRCodeModel qrcode)
        {
            var q = db.Events.Find(1);


            qrcode.name = q.name;
            qrcode.des = q.des;
            qrcode.price = q.price;
            qrcode.startTime = q.startTime;
            qrcode.endTime = q.endTime;
            qrcode.date = q.date;
            //https://ibb.co/4S7L73k

            try
            {
                qrcode.QRCodeImagePath = GenerateQRCode(
        /*            "Event Name  :" + qrcode.name + '\n' +
                    "Description :" + qrcode.des + '\n' +
                    "Price       :" + qrcode.price + '\n' +
                    "Start Time  :" + qrcode.startTime + '\n' +
                    "End TIme    :" + qrcode.endTime + '\n' +
                    "Date        :" + qrcode.date.ToString("yyyy-MM-dd") +*/
                    ConvertUrlsToLinks("www.localhost:44302/Admin/DisplayEvent")
                    );
                ViewBag.Message = "QR Code Created successfully";
            }
            catch (Exception ex)
            {
                ;//catch exception if there is any
            }
            return View(qrcode);
        }

        private string GenerateQRCode(string qrcodeText)
        {
            string folderPath = "~/Photo";
            string imagePath = "~/Photo/QRCode.png";
            // If the directory doesn't exist then create it.
            if (!Directory.Exists(Server.MapPath(folderPath)))
            {
                Directory.CreateDirectory(Server.MapPath(folderPath));
            }

            var barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            //print details
            var result = barcodeWriter.Write(ConvertUrlsToLinks(qrcodeText));

            string barcodePath = Server.MapPath(imagePath);
            var barcodeBitmap = new Bitmap(result);
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(barcodePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    barcodeBitmap.Save(memory, ImageFormat.Jpeg);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            return imagePath;
        }

        public ActionResult Read()
        {
            return View(ReadQRCode());
        }

        private QRCodeModel ReadQRCode()
        {
            QRCodeModel barcodeModel = new QRCodeModel();
            string barcodeText = "";
            string imagePath = "~/Photo/QRCode.png";
            string barcodePath = Server.MapPath(imagePath);
            var barcodeReader = new BarcodeReader();

            var result = barcodeReader.Decode(new Bitmap(barcodePath));
            if (result != null)
            {
                barcodeText = result.Text;
            }
            return new QRCodeModel() { name = barcodeText, QRCodeImagePath = imagePath };
        }

        public ActionResult DisplayQR()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DisplayQR(string txtQRCode)
        {
            ViewBag.txtQRCode = txtQRCode;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(txtQRCode, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            //System.Web.UI.WebControls.Image imgBarCode = new System.Web.UI.WebControls.Image();
            //imgBarCode.Height = 150;
            //imgBarCode.Width = 150;
            using (Bitmap bitMap = qrCode.GetGraphic(20))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ViewBag.imageBytes = ms.ToArray();
                    //imgBarCode.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(byteImage);
                }
            }
            return View();
        }


        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "hyddhrii%2moi43Hd5%%";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "hyddhrii%2moi43Hd5%%";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }

}