using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;

[assembly: OwinStartup(typeof(EventManagementSystem.Startup))]

namespace EventManagementSystem
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            var options = new CookieAuthenticationOptions
            {
                AuthenticationType = "AUTH", //Id of auth
                LoginPath = new PathString("/Account/Login"),
                LogoutPath = new PathString("/Account/Logout")
            };
            app.UseCookieAuthentication(options);
        }
    }
}
