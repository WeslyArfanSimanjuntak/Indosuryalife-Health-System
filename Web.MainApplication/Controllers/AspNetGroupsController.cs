using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Repository.Application.DataModel;

namespace Web.MainApplication.Controllers
{
    public class AspNetGroupsController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: AspNetGroups
        public ActionResult Index()
        {
            return View(db.AspNetGroups.ToList());
        }

        // GET: AspNetGroups/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetGroups aspNetGroups = db.AspNetGroups.Find(id);
            if (aspNetGroups == null)
            {
                return HttpNotFound();
            }
            return View(aspNetGroups);
        }

        // GET: AspNetGroups/Create
        public ActionResult Create()
        {
            ViewBag.IsActive = WebAppUtility.SelectListIsActive();
            return View();
        }

        // POST: AspNetGroups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "GroupName,GroupDescription,Remark,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] AspNetGroups aspNetGroups)
        {
            if (ModelState.IsValid)
            {
                db.AspNetGroups.Add(aspNetGroups);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetGroups.IsActive);
            return View(aspNetGroups);
        }

        // GET: AspNetGroups/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetGroups aspNetGroups = db.AspNetGroups.Find(id);

            // add index roles for each controller
            var temp = db.AspNetRoles.ToList().Where(x => x.Type.ToLower() == "controller").ToList();
            temp.ForEach(x =>
            {
                if (x.AspNetRoles1.ToList().Where(z => z.Name.ToLower() == "index").Count() == 0)
                {
                    var roles = new AspNetRoles();
                    roles.ParentId = x.Id;
                    roles.Name = "Index";
                    roles.Type = "Function";
                    roles.SetPropertyCreate();

                    db.AspNetRoles.Add(roles);

                }

            });
            db.SaveChanges();


            if (aspNetGroups == null)
            {
                return HttpNotFound();
            }
            ViewBag.Roles = db.AspNetRoles.ToList();
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetGroups.IsActive);

            return View(aspNetGroups);
        }

        // POST: AspNetGroups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "GroupName,GroupDescription,Remark,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] AspNetGroups aspNetGroups)
        {
            var dictionaryGroupRoles = new Dictionary<string, bool>();
            foreach (var item in db.AspNetRoles.ToList())
            {
                dictionaryGroupRoles.Add(item.Id.ToString(), Request.Params.GetValues(item.Id.ToString()).FirstOrDefault() == "true" ? true : false);
            }

            if (ModelState.IsValid)
            {
                var rolesGroup = db.AspNetRoleGroup.Where(x => x.GroupName == aspNetGroups.GroupName).ToList();
                foreach (var item in dictionaryGroupRoles)
                {
                    if (rolesGroup.Where(x => x.RolesId.ToString() == item.Key).FirstOrDefault() == null && item.Value == true)
                    {
                        var newRolesGroup = new AspNetRoleGroup()
                        {
                            GroupName = aspNetGroups.GroupName,
                            RolesId = Convert.ToInt32(item.Key)
                        };
                        newRolesGroup.SetPropertyCreate();
                        db.AspNetRoleGroup.Add(newRolesGroup);
                    }
                    if (rolesGroup.Where(x => x.RolesId.ToString() == item.Key).FirstOrDefault() != null && item.Value == false)
                    {
                        var rolesGroupToBeDropped = db.AspNetRoleGroup.Find(Convert.ToInt32(item.Key), aspNetGroups.GroupName);
                        db.AspNetRoleGroup.Remove(rolesGroupToBeDropped);
                    }
                }


                aspNetGroups.SetPropertyUpdate();
                db.Entry(aspNetGroups).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                SuccessMessagesAdd(Message.UpdateSuccess);
                return RedirectToAction("Index");
            }
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetGroups.IsActive);

            return View(aspNetGroups);
        }

        // GET: AspNetGroups/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetGroups aspNetGroups = db.AspNetGroups.Find(id);
            if (aspNetGroups == null)
            {
                return HttpNotFound();
            }
            return View(aspNetGroups);
        }

        // POST: AspNetGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            AspNetGroups aspNetGroups = db.AspNetGroups.Find(id);
            db.AspNetGroups.Remove(aspNetGroups);
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
