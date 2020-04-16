using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CrystalDecisions.CrystalReports.Engine;
using Repository.Application.DataModel;

namespace Web.MainApplication.Controllers
{
    public class BenefitController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: Benefit
        public ActionResult Index()
        {
            ViewBag.IsActive = WebAppUtility.SelectListIsActive();
            return View(db.Benefit.ToList());
        }

        // GET: Benefit/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Benefit benefit = db.Benefit.Find(id);
            if (benefit == null)
            {
                return HttpNotFound();
            }
            return View(benefit);
        }

        // GET: Benefit/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Benefit/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,BenefitCode,BenefitName,BenefitDesc,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] Benefit benefit)
        {
            if (ModelState.IsValid)
            {

                benefit.SetPropertyCreate();

                db.Benefit.Add(benefit);
                db.SaveChanges();
                SuccessMessagesAdd("Inserting Data Success");
                return RedirectToAction("Index");
            }

            WarningMessagesAdd(ModelState.ListErrors());
            return View(benefit);
        }

        // GET: Benefit/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Benefit benefit = db.Benefit.Find(id);
            if (benefit == null)
            {
                return HttpNotFound();
            }
            return View(benefit);
        }

        // POST: Benefit/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,BenefitCode,BenefitName,BenefitDesc,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] Benefit benefit)
        {
            if (ModelState.IsValid)
            {
                benefit.SetPropertyUpdate();
                db.Entry(benefit).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                SuccessMessagesAdd("Editing Data Success");
                return RedirectToAction("Index");
            }
            WarningMessagesAdd(ModelState.ListErrors());

            return View(benefit);
        }

        // GET: Benefit/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Benefit benefit = db.Benefit.Find(id);
            if (benefit == null)
            {
                return HttpNotFound();
            }
            return View(benefit);
        }

        // POST: Benefit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Benefit benefit = db.Benefit.Find(id);
            db.Benefit.Remove(benefit);
            db.SaveChanges();
            SuccessMessagesAdd("Delate Success!");
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
