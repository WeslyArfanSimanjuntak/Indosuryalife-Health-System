using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Web.MainApplication.Controllers
{
    public class HomeController : BaseController
    {
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
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
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