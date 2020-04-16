using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Repository.Application.DataModel;
using Web.MainApplication.Service.System;
using Web.MainApplication.Models;
using System.Web.Mvc.Html;

namespace Web.MainApplication.Controllers
{
    public class AspNetUsersController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: AspNetUsers
        public ActionResult Index()
        {
            return View(db.AspNetUsers.ToList());
        }

        // GET: AspNetUsers/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUsers aspNetUsers = db.AspNetUsers.Find(id);
            if (aspNetUsers == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUsers);
        }

        // GET: AspNetUsers/Create
        public ActionResult Create()
        {
            ViewBag.IsActive = WebAppUtility.SelectListIsActive();
            return View();
        }

        // POST: AspNetUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,FullName,Password,Email,LastPasswordChange,ErrorTried,IsLocked,Remark,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] AspNetUsers aspNetUsers)
        {
            if (ModelState.IsValid)
            {
                aspNetUsers.Password = Encriptor.SHA1(aspNetUsers.Password);
                db.AspNetUsers.Add(aspNetUsers);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetUsers.IsActive);

            return View(aspNetUsers);
        }

        // GET: AspNetUsers/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUsers aspNetUsers = db.AspNetUsers.Find(id);
            if (aspNetUsers == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetUsers.IsActive);
            return View(aspNetUsers);
        }

        // POST: AspNetUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Username,FullName,Password,Email,LastPasswordChange,ErrorTried,IsLocked,Remark,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] AspNetUsers aspNetUsers)
        {
            if (ModelState.IsValid)
            {
                aspNetUsers.SetPropertyUpdate();
                db.Entry(aspNetUsers).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetUsers.IsActive);

            return View(aspNetUsers);
        }

        // GET: AspNetUsers/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUsers aspNetUsers = db.AspNetUsers.Find(id);
            if (aspNetUsers == null)
            {
                return HttpNotFound();
            }
            //if (Request.IsAjaxRequest())
            //{
            //    if (WarningMessages().Count == 0)
            //    {
            //        WarningMessagesAdd("Do You Want To Delete This ?");
            //    }               

            //    return View("_Modal", new ModalView()
            //    {
            //        Title = "Delete User",
            //        Body = this.RenderRazorViewToString("Delete", aspNetUsers),
            //        Footer = this.GetHtmlHelper().TextBox("Delete", "Delete", null, new { @class = "btn btn-primary", @type = "submit" }).ToString()
            //    });

            //}

            return View(aspNetUsers);
        }

        // POST: AspNetUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            AspNetUsers aspNetUsers = db.AspNetUsers.Find(id);
            db.AspNetUsers.Remove(aspNetUsers);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
