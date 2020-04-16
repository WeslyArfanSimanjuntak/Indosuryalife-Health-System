using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Repository.Application.DataModel;

namespace Web.MainApplication.Controllers
{
    public class AspNetGroupUsersController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: AspNetGroupUsers
        public ActionResult Index()
        {
            var aspNetGroupUser = db.AspNetGroupUser.Include(a => a.AspNetGroups).Include(a => a.AspNetUsers);
            return View(aspNetGroupUser.ToList());
        }

        // GET: AspNetGroupUsers/Details/5
        public ActionResult Details(string username, string groupName)
        {
         
            AspNetGroupUser aspNetGroupUser = db.AspNetGroupUser.Find(username,groupName);
            if (aspNetGroupUser == null)
            {
                return HttpNotFound();
            }
            if (username == null || groupName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetGroupUser AspNetGroupUser = db.AspNetGroupUser.Where(x => x.Username == username && x.GroupName == groupName).FirstOrDefault();
            if (AspNetGroupUser == null)
            {
                return HttpNotFound();
            }

            return View(aspNetGroupUser);
        }

        // GET: AspNetGroupUsers/Create
        public ActionResult Create()
        {
            ViewBag.GroupName = new SelectList(db.AspNetGroups, "GroupName", "GroupDescription");
            ViewBag.Username = new SelectList(db.AspNetUsers, "Username", "FullName");
            ViewBag.IsActive = WebAppUtility.SelectListIsActive();

            return View();
        }

        // POST: AspNetGroupUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,GroupName,Remark,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] AspNetGroupUser aspNetGroupUser)
        {
            if (ModelState.IsValid)
            {

                db.AspNetGroupUser.Add(aspNetGroupUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.GroupName = new SelectList(db.AspNetGroups, "GroupName", "GroupDescription", aspNetGroupUser.GroupName);
            ViewBag.Username = new SelectList(db.AspNetUsers, "Username", "FullName", aspNetGroupUser.Username);
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetGroupUser.IsActive);

            return View(aspNetGroupUser);
        }

        // GET: AspNetGroupUsers/Edit/5
        public ActionResult Edit(string username, string groupName)
        {
            if (username == null || groupName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetGroupUser aspNetGroupUser = db.AspNetGroupUser.Where(x => x.Username == username && x.GroupName == groupName).FirstOrDefault();
            if (aspNetGroupUser == null)
            {
                return HttpNotFound();
            }
            ViewBag.GroupName = new SelectList(db.AspNetGroups, "GroupName", "GroupDescription", aspNetGroupUser.GroupName);
            ViewBag.Username = new SelectList(db.AspNetUsers, "Username", "FullName", aspNetGroupUser.Username);
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetGroupUser.IsActive);
            return View(aspNetGroupUser);
        }

        // POST: AspNetGroupUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Username,GroupName,Remark,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] AspNetGroupUser aspNetGroupUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetGroupUser).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.GroupName = new SelectList(db.AspNetGroups, "GroupName", "GroupDescription", aspNetGroupUser.GroupName);
            ViewBag.Username = new SelectList(db.AspNetUsers, "Username", "FullName", aspNetGroupUser.Username);
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetGroupUser.IsActive);
            return View(aspNetGroupUser);
        }

        // GET: AspNetGroupUsers/Delete/5
        public ActionResult Delete(string username, string groupName)
        {
            if (username == null || groupName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetGroupUser aspNetGroupUser = db.AspNetGroupUser.Where(x => x.Username == username && x.GroupName == groupName).FirstOrDefault();
            if (aspNetGroupUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetGroupUser);
        }

        // POST: AspNetGroupUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string username, string groupName)
        {
            AspNetGroupUser aspNetGroupUser = db.AspNetGroupUser.Where(x => x.Username == username && x.GroupName == groupName).FirstOrDefault();

            db.AspNetGroupUser.Remove(aspNetGroupUser);
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
