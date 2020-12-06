using System;
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
using EventManagementSystem.reCAPTCHA;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;

namespace EventManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        DBEntities db = new DBEntities();
        PasswordHasher ph = new PasswordHasher();

        private string HashPassword(string password)
        {
            return ph.HashPassword(password);
        }

        private bool VerifyPassword(string hash, string password)
        {
            return ph.VerifyHashedPassword(hash, password) == PasswordVerificationResult.Success;
        }

        // Check if username aldy exists in database
        public JsonResult IsUserNameAvailable(RegisterVM model)
        {
            return Json(!db.Users.Any(u => u.username == model.username), JsonRequestBehavior.AllowGet);
        }



        private void SignIn(string username, string role, bool rememberMe)
        {
            // TODO(1): Identity and claims
            var iden = new ClaimsIdentity("AUTH");
            iden.AddClaim(new Claim(ClaimTypes.Name, username));
            iden.AddClaim(new Claim(ClaimTypes.Role, role));

            // TODO(2): Remember me
            var prop = new AuthenticationProperties
            {
                IsPersistent = rememberMe //Remember
            };

            // TODO(3): Sign in
            Request.GetOwinContext().Authentication.SignIn(prop, iden);

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
            Session["tempName"] = name;
        }

        [HttpPost]
        public string GetTempName()
        {
            return Session["tempName"].ToString(); 
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

        // GET: Account/Register
        public ActionResult Register()
        {

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateGoogleCaptcha]
        public ActionResult Register(RegisterVM model)
        {
            //If entered here captcha = valid


            if (Session["tempName"] != null)
            {
                Session.Clear();
            }

            if(model.webPhoto == null && model.Photo == null)
            {
                ModelState.AddModelError("Photo", "Photo is required");
            }

            if(model.Photo != null)
            {
                string err = ValidatePhoto(model.Photo);
                if (err != null)
                {
                    ModelState.AddModelError("Photo", err);
                }
            }

            if (ModelState.IsValid)
            {
                int count = db.Users.Count() + 1;
                string photoUrl = "";

                if(model.Photo != null)
                {
                    photoUrl = SavePhoto(model.Photo);
                }else if(model.webPhoto != null)
                {
                    photoUrl = model.webPhoto;
                }

                var user = new User
                {
                    Id = count,
                    name = model.name,
                    contact_no = model.contact_no,
                    email = model.email,
                    username = model.username,
                    password = HashPassword(model.password),
                    role = model.role,
                    status = true,
                    activated = false,
                    recoveryCode = null,
                    activationCode = Guid.NewGuid().ToString(),
                    photo = photoUrl
                };

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

                string link = "https://localhost:44302/Account/Activation?activationCode=" + user.activationCode + "&userid=" + user.Id;
                string mail = @"";

                MailMessage m = new MailMessage();
                m.To.Add(model.email);
                m.Subject = "Activate your TARUC EMS account";
                m.Body = link;
                m.IsBodyHtml = true; //Can send HTML FORMATTED Mail
                new SmtpClient().Send(m);

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
            if (model != null)
            {
                if (model.activationCode == activationCode)
                {
                    model.activated = true;
                    db.SaveChanges();
                    return View("Activated");
                }
                return Content("Activation failed");//INCOMPLETE
            }

            return View();
        }
        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }
 
        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateGoogleCaptcha]
        public ActionResult Login(LoginVM model, string returnUrl = "")
        {
            
            var user = db.Users.FirstOrDefault(u => u.username == model.Username);      
            
            HttpCookie blockCookie = new HttpCookie("block");
            HttpCookie getCookie = HttpContext.Request.Cookies.Get("block");
           
            user.lockoutValue = user.lockoutValue +1;
            db.SaveChanges();
            int count = user.lockoutValue;

            if (getCookie == null)
            {
                if (ModelState.IsValid)
                {
                    //Get user record based on username

                    //Check pass
                    if (user != null && VerifyPassword(user.password, model.Password))
                    {
                        if (!user.activated)
                        {
                            return RedirectToAction("Awaitactivation", "Account", new { email = user.email });
                        }

                        SignIn(user.username, user.role, model.RememberMe);
                        Session["PhotoURL"] = user.photo;

                        //Handle return url
                        if (returnUrl == "")
                        {
                            TempData["Info"] = "You have successfully logged in.";
                            if (user.role == "Student")
                                return RedirectToAction("Index", "Home");
                            else if (user.role == "Admin")
                                return RedirectToAction("Index", "Admin");
     
                            user.lockoutValue = 0;
                            db.SaveChanges();
                        }

                    }
                    else if (count > 3)
                    {
                        
                        blockCookie.Value = "IAMDONE";
                        blockCookie.Expires = DateTime.Now.AddMinutes(1);
                        Response.Cookies.Set(blockCookie);
                        ModelState.AddModelError("Password", "Please login after 1 mintue!");
                        user.lockoutValue = 0;
                        db.SaveChanges();

                    }
                    else
                    {
                        ModelState.AddModelError("Password", "Username and password not matched.");

                    }


                }
            }
           
            return View(model);
        }


        // GET: Account/Logout
        public ActionResult Logout()
        {
            // TODO: Sign out user + session
            Session.Remove("PhotoURL");
            Request.GetOwinContext().Authentication.SignOut();
            TempData["Info"] = "You have successfully logged out.";
            return RedirectToAction("Index", "Home");
        }
       


    }
}