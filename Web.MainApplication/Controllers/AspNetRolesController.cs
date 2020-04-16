using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System;
using System.Net;
using System.Web.Mvc;
using Repository.Application.DataModel;

namespace Web.MainApplication.Controllers
{
    public class AspNetRolesController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: AspNetRoles
        public ActionResult Index()
        {
            var aspNetRoles = db.AspNetRoles.Include(a => a.AspNetRoles2);
            return View(aspNetRoles.ToList());
        }

        // GET: AspNetRoles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetRoles aspNetRoles = db.AspNetRoles.Find(id);
            if (aspNetRoles == null)
            {
                return HttpNotFound();
            }
            return View(aspNetRoles);
        }

        // GET: AspNetRoles/Create
        public ActionResult Create()
        {
            var selectList = new List<SelectListItem>();
            selectList.AddBlank();
            db.AspNetRoles.Where(x => x.Type == "Controller").OrderBy(x => x.Name).ToList().ForEach(x =>
              {
                  selectList.AddItemValText(x.Id.ToString(), x.Name);
              });
            var selectListTypeController = new List<SelectListItem>();
            selectListTypeController.AddBlank();
            selectListTypeController.AddItemValText("Controller", "Controller");
            selectListTypeController.AddItemValText("Function", "Function");
            ViewBag.ParentId = selectList.ToSelectList();
            ViewBag.Type = selectListTypeController.ToSelectList();
            ViewBag.IsActive = WebAppUtility.SelectListIsActive();

            return View();
        }

        // POST: AspNetRoles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ParentId,Name,Type,Remark,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] AspNetRoles aspNetRoles)
        {

            if (ModelState.IsValid && WarningMessages().Count == 0)
            {
                try
                {
                    var checkAllRoles = db.AspNetRoles.Where(x => x.ParentId == aspNetRoles.ParentId && x.Name.ToLower() == aspNetRoles.Name.ToLower()).FirstOrDefault();
                    var funcData = Request.Params["AddFunction"];
                    var selectedFunc = funcData.Split(',').ToList();
                    var listOfCheckedFunction = new List<AspNetRoles>();
                    if (checkAllRoles != null)
                    {
                        WarningMessagesAdd("Roles with parent \"" + checkAllRoles.AspNetRoles2.Name + "\" and roles name \"" + aspNetRoles.Name + "\" is already exist.");
                    }
                    if (aspNetRoles.Name == null)
                    {
                        WarningMessagesAdd("Name or Type can not be blank");
                    }

                    if (WarningMessages().Count == 0)
                    {
                        if (aspNetRoles.ParentId != null)
                        {
                            aspNetRoles.Type = "Function";
                        }
                        else
                        {
                            aspNetRoles.Type = "Controller";
                        }
                        aspNetRoles.SetPropertyCreate();
                        db.AspNetRoles.Add(aspNetRoles);
                        db.SaveChanges();
                    }

                    if (selectedFunc.Count > 0 && selectedFunc.FirstOrDefault() != "")
                    {
                        for (int i = 0; i < selectedFunc.Count; i++)
                        {
                            var newAspNetRoles = new AspNetRoles();
                            newAspNetRoles.ParentId = aspNetRoles.Id;
                            newAspNetRoles.Name = selectedFunc.ElementAt(i).ToString();
                            newAspNetRoles.Type = "Function";
                            newAspNetRoles.IsActive = 1;
                            newAspNetRoles.SetPropertyCreate();
                            listOfCheckedFunction.Add(newAspNetRoles);
                        }
                        if (WarningMessages().Count == 0)
                        {

                            db.AspNetRoles.AddRange(listOfCheckedFunction);
                            db.SaveChanges();
                        }

                    }

                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    WarningMessagesAdd(e.MessageToList());
                }

            }

            var selectList = new List<SelectListItem>();
            selectList.AddBlank();
            db.AspNetRoles.Where(x => x.Type == "Controller").OrderBy(x => x.Name).ToList().ForEach(x =>
            {
                selectList.AddItemValText(x.Id.ToString(), x.Name);
            });
            var selectListTypeController = new List<SelectListItem>();
            selectListTypeController.AddBlank();
            selectListTypeController.AddItemValText("Controller", "Controller");
            selectListTypeController.AddItemValText("Function", "Function");
            ViewBag.ParentId = selectList.ToSelectList();
            ViewBag.Type = selectListTypeController.ToSelectList();
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetRoles.IsActive);

            return View(aspNetRoles);
        }

        // GET: AspNetRoles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetRoles aspNetRoles = db.AspNetRoles.Find(id);
            if (aspNetRoles == null)
            {
                return HttpNotFound();
            }

            var selectList = new List<SelectListItem>();
            selectList.AddBlank();
            db.AspNetRoles.Where(x => x.Type == "Controller" && x.Id != aspNetRoles.Id).OrderBy(x => x.Name).ToList().ForEach(x =>
            {
                selectList.AddItemValText(x.Id.ToString(), x.Name);
            });
            var selectListTypeController = new List<SelectListItem>();
            selectListTypeController.AddBlank();
            selectListTypeController.AddItemValText("Controller", "Controller");
            selectListTypeController.AddItemValText("Function", "Function");
            ViewBag.ParentId = selectList.ToSelectList(aspNetRoles.ParentId);
            ViewBag.Type = selectListTypeController.ToSelectList(aspNetRoles.Type);
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetRoles.IsActive);
            return View(aspNetRoles);
        }

        // POST: AspNetRoles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ParentId,Name,Type,Remark,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] AspNetRoles aspNetRoles)
        {
            if (ModelState.IsValid)
            {
                aspNetRoles.SetPropertyUpdate();
                db.Entry(aspNetRoles).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var selectList = new List<SelectListItem>();
            selectList.AddBlank();
            db.AspNetRoles.Where(x => x.Type == "Controller" && x.Id != aspNetRoles.Id).OrderBy(x => x.Name).ToList().ForEach(x =>
            {
                selectList.AddItemValText(x.Id.ToString(), x.Name);
            });
            var selectListTypeController = new List<SelectListItem>();
            selectListTypeController.AddBlank();
            selectListTypeController.AddItemValText("Controller", "Controller");
            selectListTypeController.AddItemValText("Function", "Function");
            ViewBag.ParentId = selectList.ToSelectList();
            ViewBag.Type = selectListTypeController.ToSelectList();
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(aspNetRoles.IsActive);
            if (aspNetRoles.ParentId != null)
            {
                aspNetRoles.Type = "Controller";
            }
            else
            {
                aspNetRoles.Type = "Function";
            }
            return View(aspNetRoles);
        }

        // GET: AspNetRoles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetRoles aspNetRoles = db.AspNetRoles.Find(id);
            if (aspNetRoles == null)
            {
                return HttpNotFound();
            }
            return View(aspNetRoles);
        }

        // POST: AspNetRoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                AspNetRoles aspNetRoles = db.AspNetRoles.Find(id);
                db.AspNetRoles.Remove(aspNetRoles);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                WarningMessagesAdd(e.MessageToList());
            }
            
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
