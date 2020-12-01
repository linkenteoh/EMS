using EventManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace EventManagementSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        DBEntities db = new DBEntities();
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;

        }

        /*        protected void Session_Start()
                {
                    if (User.IsInRole("Student") || User.IsInRole("Outsider"))
                    {
                        Session["PhotoURL"] = db.Users.Find(User.Identity.Name).photo;
                    }
                }*/
    }
}
