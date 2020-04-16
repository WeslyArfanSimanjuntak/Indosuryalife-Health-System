using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Repository.Application.DataModel;

namespace Web.MainApplication.Controllers
{
    public class PlanDetailController : Controller
    {
        private DBEntities db = new DBEntities();

        // GET: PlanDetail
        public ActionResult Index()
        {
            var planDetail = db.PlanDetail.Include(p => p.BasicProduct).Include(p => p.Policy);
            return View(planDetail.ToList());
        }

        // GET: PlanDetail/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanDetail planDetail = db.PlanDetail.Find(id);
            if (planDetail == null)
            {
                return HttpNotFound();
            }
            return View(planDetail);
        }

        // GET: PlanDetail/Create
        public ActionResult Create()
        {
            ViewBag.BasicProductId = new SelectList(db.BasicProduct, "BasicProductId", "BpInsurerId");
            ViewBag.PlanId = new SelectList(db.Plan, "PlanId", "PolicyId");
            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber");
            return View();
        }

        // POST: PlanDetail/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PolicyId,PlanId,BasicProductId,BasicProductLimitCode,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] PlanDetail planDetail)
        {
            if (ModelState.IsValid)
            {
                db.PlanDetail.Add(planDetail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BasicProductId = new SelectList(db.BasicProduct, "BasicProductId", "BpInsurerId", planDetail.BasicProductId);
            ViewBag.PlanId = new SelectList(db.Plan, "PlanId", "PolicyId", planDetail.PlanId);
            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber", planDetail.PolicyId);
            return View(planDetail);
        }

        // GET: PlanDetail/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanDetail planDetail = db.PlanDetail.Find(id);
            if (planDetail == null)
            {
                return HttpNotFound();
            }
            ViewBag.BasicProductId = new SelectList(db.BasicProduct, "BasicProductId", "BpInsurerId", planDetail.BasicProductId);
            ViewBag.PlanId = new SelectList(db.Plan, "PlanId", "PolicyId", planDetail.PlanId);
            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber", planDetail.PolicyId);
            return View(planDetail);
        }

        // POST: PlanDetail/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PolicyId,PlanId,BasicProductId,BasicProductLimitCode,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] PlanDetail planDetail)
        {
            if (ModelState.IsValid)
            {
                db.Entry(planDetail).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BasicProductId = new SelectList(db.BasicProduct, "BasicProductId", "BpInsurerId", planDetail.BasicProductId);
            ViewBag.PlanId = new SelectList(db.Plan, "PlanId", "PolicyId", planDetail.PlanId);
            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber", planDetail.PolicyId);
            return View(planDetail);
        }

        // GET: PlanDetail/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlanDetail planDetail = db.PlanDetail.Find(id);
            if (planDetail == null)
            {
                return HttpNotFound();
            }
            return View(planDetail);
        }

        // POST: PlanDetail/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            PlanDetail planDetail = db.PlanDetail.Find(id);
            db.PlanDetail.Remove(planDetail);
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
