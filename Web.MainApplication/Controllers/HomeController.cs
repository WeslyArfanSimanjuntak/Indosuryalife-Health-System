using Repository.Application.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Web.MainApplication.Models;

namespace Web.MainApplication.Controllers
{
    public class HomeController : BaseController
    {
        private DBEntities db = new DBEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult NavigationBack(string url, bool isButtonClicked = false)
        {
            var listNavigationBack = new Dictionary<int, string>();
            if (isButtonClicked)
            {
                var key = Convert.ToInt32(Request.Params["navigationBackKey"]);

                if (Session["NavigationBack" + Resources.SystemResources.ApplicationName] != null)
                {
                    listNavigationBack = (Dictionary<int, string>)Session["NavigationBack" + Resources.SystemResources.ApplicationName];
                    var listNavigationBackResult = listNavigationBack.Where(x => x.Key < (key)).ToDictionary(x => x.Key, x => x.Value);
                    Session["NavigationBack" + Resources.SystemResources.ApplicationName] = listNavigationBackResult;
                }

                Session["ByNavigationBack"] = true;
                return Redirect(url);
            }
            try
            {
                if (Session["NavigationBack" + Resources.SystemResources.ApplicationName] != null)
                {
                    listNavigationBack = (Dictionary<int, string>)Session["NavigationBack" + Resources.SystemResources.ApplicationName];
                }
                listNavigationBack.Add(listNavigationBack.Count + 1, url);
                Session["NavigationBack" + Resources.SystemResources.ApplicationName] = listNavigationBack;

                return this.Json(new
                {
                    message = "success"

                }, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception e)
            {

                return this.Json(new
                {
                    message = "failed",
                    error = e.Message

                }, JsonRequestBehavior.AllowGet);
            }


        }
        [AllowAnonymous]
        public ActionResult About(string version)
        {
            db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
            var versionInfo = db.CommonListValue.Where(x => x.CommonListValue2.Value == VersionApplication.ApplicationVersionsSequence).ToList().Where(x=>x.Text == version).FirstOrDefault();
            ViewBag.Versions = db.CommonListValue.Where(x=>x.ParentId == versionInfo.ParentId).ToList();
            //ViewBag.Versions = db.CommonListValue.Where(x => x.CommonListValue2.Value == VersionApplication.ApplicationVersionsSequence).FirstOrDefault().CommonListValue1.ToList();
            return View("_Modal", new ModalView()
            {
                Title = "Version Notes",
                Body = this.RenderRazorViewToString("_About", versionInfo)
            });
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult UnAuthorized()
        {

            ViewBag.Message = "you're UnAuthrorized.";

            return View();
        }
    }
}