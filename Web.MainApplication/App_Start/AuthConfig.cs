using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Configuration;
using Web.MainApplication.Resources;

[assembly: OwinStartup(typeof(Web.MainApplication.App_Start.AuthConfig))]
namespace Web.MainApplication.App_Start
{

    public class AuthConfig
    {

        public void Configuration(IAppBuilder app)
        {
            System.Web.Helpers.AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Email;
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString(SystemResources.LoginPath),
                CookieSecure = CookieSecureOption.SameAsRequest,
                ExpireTimeSpan = TimeSpan.FromSeconds(Convert.ToDouble(WebConfigurationManager.AppSettings["ExpiredCookiesTime"])),
                LogoutPath = new PathString(SystemResources.LogoutPath)
            });
        }
    }
}