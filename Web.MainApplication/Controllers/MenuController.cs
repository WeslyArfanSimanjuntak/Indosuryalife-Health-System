using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Repository.Application.DataModel;

namespace Web.MainApplication.Controllers
{
    public class MenuController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: Menu
        public ActionResult Index()
        {
            var menu = db.Menu.Include(m => m.AspNetRoles1).Include(m => m.Menu2);

            return View(menu.ToList());
        }


        // GET: Menu/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db.Menu.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // GET: Menu/Create
        public void RepairMenu(Menu menu)
        {
            var childMenu = menu.Menu1.ToList();

            if (childMenu != null)
            {
                var totalChild = childMenu.Count;
                int counter = 1;
                foreach (var item in childMenu.OrderBy(x => x.Sequence))
                {

                    item.Sequence = counter;
                    item.SetPropertyUpdate();
                    db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    counter++;
                }


            }

        }
        public ActionResult Create()
        {

            var sliRoles = new List<SelectListItem>();
            sliRoles.AddBlank();
            db.AspNetRoles.ToList().ForEach(x =>
            {
                sliRoles.AddItemValText(x.Id.ToString(), x.AspNetRoles2 != null ? (x.AspNetRoles2.Name + " - " + x.Name) : x.Name);
            });

            var sliMenuParentid = new List<SelectListItem>();
            sliMenuParentid.AddBlank();
            db.Menu.ToList().ForEach(x =>
            {
                sliMenuParentid.AddItemValText(x.MenuId.ToString(), x.Menu2 != null ? x.Menu2.MenuName + " - " + x.MenuName : x.MenuName);
            });

            ViewBag.MenuParentId = sliMenuParentid.OrderBy(x => x.Text).ToList().ToSelectList();
            ViewBag.AspNetRoles = sliRoles.ToSelectList();

            var sequence = new List<SelectListItem>();
            sequence.AddBlank();
            db.Menu.Where(x => x.MenuParentId == null).ToList().ForEach(x =>
            {
                sequence.AddItemValText(x.Sequence.ToString(), x.Sequence.ToString());
            });

            ViewBag.Sequence = sequence.ToSelectList();
            ViewBag.IsActive = WebAppUtility.SelectListIsActive();
            return View();
        }

        // POST: Menu/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MenuId,MenuName,MenuParentId,Sequence,AspNetRoles,MenuIClass,MenuLevel,Remark,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] Menu menu)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menu.MenuParentId != null)
                    {
                        menu.MenuLevel = GetMenuLevel(db.Menu.Find(menu.MenuParentId)) + 1;
                    }
                    if (menu.MenuParentId != null)
                    {
                        menu.ShowAsChild = 1;
                    }

                    menu.SetPropertyCreate();
                    db.Menu.Add(menu);
                    db.SaveChanges();

                    var menuWithBiggerSequence = db.Menu.Where(x =>x.MenuId != menu.MenuId && x.MenuParentId == menu.MenuParentId && x.Sequence >= menu.Sequence).ToList();
                    menuWithBiggerSequence.ForEach(x =>
                    {
                        x.Sequence = x.Sequence + 1;
                        x.SetPropertyUpdate();

                        db.Entry(x).State = System.Data.Entity.EntityState.Modified;
                    });

                    db.SaveChanges();
                    SuccessMessagesAdd(Message.CreateSuccess);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {
                WarningMessagesAdd(Message.CreateFail);
                WarningMessagesAdd(e.MessageToList());


            }

            var sliRoles = new List<SelectListItem>();
            sliRoles.AddBlank();
            db.AspNetRoles.ToList().ForEach(x =>
            {
                sliRoles.AddItemValText(x.Id.ToString(), x.AspNetRoles2 != null ? (x.AspNetRoles2.Name + " - " + x.Name) : x.Name);
            });

            var sliMenuParentid = new List<SelectListItem>();
            sliMenuParentid.AddBlank();
            db.Menu.ToList().ForEach(x =>
            {
                sliMenuParentid.AddItemValText(x.MenuId.ToString(), x.Menu2 != null ? x.Menu2.MenuName + " - " + x.MenuName : x.MenuName);
            });

            ViewBag.MenuParentId = sliMenuParentid.OrderBy(x => x.Text).ToList().ToSelectList(menu.MenuParentId);
            ViewBag.AspNetRoles = sliRoles.ToSelectList(menu.AspNetRoles);

            var sequence = new List<SelectListItem>();
            sequence.AddBlank();
            db.Menu.Where(x => x.MenuParentId == null).ToList().ForEach(x =>
            {
                sequence.AddItemValText(x.Sequence.ToString(), x.Sequence.ToString());
            });

            ViewBag.Sequence = sequence.ToSelectList(menu.Sequence);
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(menu.IsActive);

            return View(menu);
        }
        public int GetMenuLevel(Menu menuParent)
        {
            int retval = 1;
            if (menuParent.Menu2 != null)
            {
                retval = retval + GetMenuLevel(menuParent.Menu2);
                return retval;
            }
            return retval;
        }
        // GET: Menu/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db.Menu.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            var sliRoles = new List<SelectListItem>();
            sliRoles.AddBlank();
            db.AspNetRoles.ToList().ForEach(x =>
            {
                sliRoles.AddItemValText(x.Id.ToString(), x.AspNetRoles2 != null ? (x.AspNetRoles2.Name + " - " + x.Name) : x.Name);
            });

            var sliMenuParentid = new List<SelectListItem>();
            sliMenuParentid.AddBlank();
            db.Menu.Where(x => x.MenuId != menu.MenuId).ToList().ForEach(x =>
              {
                  sliMenuParentid.AddItemValText(x.MenuId.ToString(), x.Menu2 != null ? x.Menu2.MenuName + " - " + x.MenuName : x.MenuName);
              });

            ViewBag.MenuParentId = sliMenuParentid.OrderBy(x => x.Text).ToList().ToSelectList(menu.MenuParentId);
            ViewBag.AspNetRoles = sliRoles.ToSelectList(menu.AspNetRoles);
            var sequence = new List<SelectListItem>();
            sequence.AddBlank();
            db.Menu.Where(x => x.MenuParentId == menu.MenuParentId).ToList().ForEach(x =>
            {
                sequence.AddItemValText(x.Sequence.ToString(), x.Sequence.ToString());
            });
            ViewBag.Sequence = sequence.ToSelectList(menu.Sequence);

            ViewBag.IsActive = WebAppUtility.SelectListIsActive(menu.IsActive);
            return View(menu);
        }

        // POST: Menu/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MenuId,MenuName,MenuParentId,Sequence,AspNetRoles,MenuIClass,MenuLevel,Remark,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive,ShowAsChild")] Menu menu)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menu.MenuParentId != null)
                    {
                        menu.MenuLevel = GetMenuLevel(db.Menu.Find(menu.MenuParentId)) + 1;
                    }
                    
                    menu.SetPropertyUpdate();
                    db.Entry(menu).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    var menuWithBiggerSequence = db.Menu.Where(x => x.MenuId != menu.MenuId && x.MenuParentId == menu.MenuParentId && x.Sequence >= menu.Sequence).ToList();
                    menuWithBiggerSequence.ForEach(x =>
                    {
                        x.Sequence = x.Sequence + 1;
                        x.SetPropertyUpdate();

                        db.Entry(x).State = System.Data.Entity.EntityState.Modified;
                    });
                    db.SaveChanges();
                    this.RepairMenu(db.Menu.Find(menu.MenuParentId));
                    SuccessMessagesAdd(Message.UpdateSuccess);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {

                WarningMessagesAdd(Message.UpdateFail);
                WarningMessagesAdd(e.Message);

            }
            var sliRoles = new List<SelectListItem>();
            sliRoles.AddBlank();
            db.AspNetRoles.ToList().ForEach(x =>
            {
                sliRoles.AddItemValText(x.Id.ToString(), x.AspNetRoles2 != null ? (x.AspNetRoles2.Name + " - " + x.Name) : x.Name);
            });

            var sliMenuParentid = new List<SelectListItem>();
            sliMenuParentid.AddBlank();
            db.Menu.Where(x => x.MenuId != menu.MenuId).ToList().ForEach(x =>
            {
                sliMenuParentid.AddItemValText(x.MenuId.ToString(), x.Menu2 != null ? x.Menu2.MenuName + " - " + x.MenuName : x.MenuName);
            });

            ViewBag.MenuParentId = sliMenuParentid.OrderBy(x => x.Text).ToList().ToSelectList(menu.MenuParentId);
            ViewBag.AspNetRoles = sliRoles.ToSelectList(menu.AspNetRoles);
            var sequence = new List<SelectListItem>();
            sequence.AddBlank();

            db.Menu.Where(x => x.MenuParentId == menu.MenuParentId).ToList().ForEach(x =>
            {
                sequence.AddItemValText(x.Sequence.ToString(), x.Sequence.ToString());
            });
            ViewBag.Sequence = sequence.ToSelectList(menu.Sequence);
            ViewBag.IsActive = WebAppUtility.SelectListIsActive(menu.IsActive);

            return View(menu);
        }

        // GET: Menu/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db.Menu.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // POST: Menu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Menu menu = db.Menu.Find(id);

            this.RepairMenu(db.Menu.Find(menu.MenuParentId));
            db.Menu.Remove(menu);
            db.SaveChanges();
            SuccessMessagesAdd(Message.DeleteSuccess);
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
