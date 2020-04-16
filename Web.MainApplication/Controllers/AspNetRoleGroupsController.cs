using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Repository.Application.DataModel;

namespace Web.MainApplication.Controllers
{
    public class AspNetRoleGroupsController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: AspNetRoleGroups
        public ActionResult Index()
        {
            var aspNetRoleGroup = db.AspNetRoleGroup.Include(a => a.AspNetGroups).Include(a => a.AspNetRoles);
            return View(aspNetRoleGroup.ToList());
        }

        // GET: AspNetRoleGroups/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetRoleGroup aspNetRoleGroup = db.AspNetRoleGroup.Find(id);
            if (aspNetRoleGroup == null)
            {
                return HttpNotFound();
            }
            return View(aspNetRoleGroup);
        }

        // GET: AspNetRoleGroups/Create
        public ActionResult Create()
        {
            ViewBag.GroupName = new SelectList(db.AspNetGroups, "GroupName", "GroupDescription");
            
            List<SelectListItem> sliRoles = new List<SelectListItem>();
            db.AspNetRoles.ToList().ForEach(x =>
            {
                sliRoles.AddItemValText(x.Id.ToString(), x.ParentId != null ? x.AspNetRoles2.Name + "-" + x.Name : x.Name);
            });
            sliRoles.OrderBy(x => x.Text);
            ViewBag.IsActive = WebAppUtility.SelectListIsActive( );
            ViewBag.RolesId = sliRoles.ToSelectList();
            return View();
        }

        // POST: AspNetRoleGroups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RolesId,GroupName,Remark,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] AspNetRoleGroup aspNetRoleGroup)
        {
            if (ModelState.IsValid)
            {
                db.AspNetRoleGroup.Add(aspNetRoleGroup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.GroupName = new SelectList(db.AspNetGroups, "GroupName", "GroupDescription", aspNetRoleGroup.GroupName);
            ViewBag.RolesId = new SelectList(db.AspNetRoles, "Id", "Name", aspNetRoleGroup.RolesId);
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetRoleGroup.IsActive);
            return View(aspNetRoleGroup);
        }

        // GET: AspNetRoleGroups/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetRoleGroup aspNetRoleGroup = db.AspNetRoleGroup.Find(id);
            if (aspNetRoleGroup == null)
            {
                return HttpNotFound();
            }
            ViewBag.GroupName = new SelectList(db.AspNetGroups, "GroupName", "GroupDescription", aspNetRoleGroup.GroupName);
            ViewBag.RolesId = new SelectList(db.AspNetRoles, "Id", "Name", aspNetRoleGroup.RolesId);
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetRoleGroup.IsActive);
            return View(aspNetRoleGroup);
        }

        // POST: AspNetRoleGroups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RolesId,GroupName,Remark,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] AspNetRoleGroup aspNetRoleGroup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetRoleGroup).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.GroupName = new SelectList(db.AspNetGroups, "GroupName", "GroupDescription", aspNetRoleGroup.GroupName);
            ViewBag.RolesId = new SelectList(db.AspNetRoles, "Id", "Name", aspNetRoleGroup.RolesId);
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetRoleGroup.IsActive);
            return View(aspNetRoleGroup);
        }

        // GET: AspNetRoleGroups/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetRoleGroup aspNetRoleGroup = db.AspNetRoleGroup.Find(id);
            if (aspNetRoleGroup == null)
            {
                return HttpNotFound();
            }
            return View(aspNetRoleGroup);
        }

        // POST: AspNetRoleGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetRoleGroup aspNetRoleGroup = db.AspNetRoleGroup.Find(id);
            db.AspNetRoleGroup.Remove(aspNetRoleGroup);
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
