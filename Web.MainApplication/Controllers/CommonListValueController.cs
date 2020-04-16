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
using EntityState = System.Data.Entity.EntityState;


namespace Web.MainApplication.Controllers
{
    public class CommonListValueController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: CommonListValue
        public ActionResult Index()
        {
            var commonListValue = db.CommonListValue.Include(c => c.CommonListValue2);
            return View(commonListValue.ToList());
        }

        // GET: CommonListValue/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CommonListValue commonListValue = db.CommonListValue.Find(id);
            if (commonListValue == null)
            {
                return HttpNotFound();
            }
            return View(commonListValue);
        }

        // GET: CommonListValue/Create
        public ActionResult Create()
        {
            var sliParentId = new List<SelectListItem>();
            sliParentId.AddBlank();
            db.CommonListValue.ToList().ForEach(x =>
            {
                sliParentId.AddItemValText(x.Id.ToString(), x.ParentId != null ? x.CommonListValue2.Value + " - " + x.Text : x.Text);

            });
            ViewBag.ParentId = sliParentId.ToSelectList();
            ViewBag.IsActive = WebAppUtility.SelectListIsActive();
            return View();
        }

        // POST: CommonListValue/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ParentId,Value,Text,Desc,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] CommonListValue commonListValue)
        {
            if (ModelState.IsValid)
            {
                commonListValue.SetPropertyCreate();
                db.CommonListValue.Add(commonListValue);
                db.SaveChanges();
                SuccessMessagesAdd("Inserting Data Success");
                return RedirectToAction("Index");
            }
            var sliParentId = new List<SelectListItem>();
            sliParentId.AddBlank();
            db.CommonListValue.ToList().ForEach(x =>
            {
                sliParentId.AddItemValText(x.Id.ToString(), x.ParentId != null ? x.CommonListValue2.Value + " - " + x.Text : x.Text);


            });
            ViewBag.ParentId = sliParentId.ToSelectList(commonListValue.ParentId);
            return View(commonListValue);
        }

        // GET: CommonListValue/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CommonListValue commonListValue = db.CommonListValue.Find(id);
            if (commonListValue == null)
            {
                return HttpNotFound();
            }
            var sliParentId = new List<SelectListItem>();
            sliParentId.AddBlank();
            db.CommonListValue.ToList().ForEach(x =>
            {
                sliParentId.AddItemValText(x.Id.ToString(), x.ParentId != null ? x.CommonListValue2.Value + " - " + x.Text : x.Text);


            });
            ViewBag.ParentId = sliParentId.ToSelectList(commonListValue.ParentId);
            return View(commonListValue);
        }

        // POST: CommonListValue/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ParentId,Value,Text,Desc,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] CommonListValue commonListValue)
        {
            if (ModelState.IsValid)

            {
                commonListValue.SetPropertyUpdate();
                db.Entry(commonListValue).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            var sliParentId = new List<SelectListItem>();
            sliParentId.AddBlank();
            db.CommonListValue.ToList().ForEach(x =>
            {
                sliParentId.AddItemValText(x.Id.ToString(), x.ParentId != null ? x.CommonListValue2.Value + " - " + x.Text : x.Text);


            });
            ViewBag.ParentId = sliParentId.ToSelectList(commonListValue.ParentId);
            return View(commonListValue);
        }

        // GET: CommonListValue/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CommonListValue commonListValue = db.CommonListValue.Find(id);
            if (commonListValue == null)
            {
                return HttpNotFound();
            }
            return View(commonListValue);
        }

        // POST: CommonListValue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                CommonListValue commonListValue = db.CommonListValue.Find(id);
                db.CommonListValue.Remove(commonListValue);
                db.SaveChanges();
                SuccessMessagesAdd("Delete Success!");
            }
            catch (Exception e)
            {
                WarningMessagesAdd(e.MessageToList());
            }
            return RedirectToAction("Index");
        }


        public ActionResult GenerateReport()
        {

            ReportDocument rd = new ReportDocument();
            var ds = new List<object>();

            db.CommonListValue.ToList().ForEach(x =>
            {
                ds.Add(new
                {

                    Value = (x.Value != null ? x.Value : ""),
                    Text = (x.Text != null ? x.Text : ""),
                    CreatedBy = (x.CreatedBy != null ? x.Text : ""),

                    //Agent = (x.Agent != null ? x.Agent : ""),
                    //PolicyId = x.PolicyId != null ? x.PolicyId : "",
                    //PolicyNumber = x.PolicyNumber != null ? x.PolicyNumber : "",
                    //StartDate = x.StartDate.Value,
                    //TerminateDate = x.TerminateDate != null ? x.TerminateDate.Value : DateTime.Now,

                });

            });
            //data.ToList().ForEach(x => {

            //    object newObject = new object();
            //    newObject = x;
            //    ds.Add(newObject);

            //});
            rd.Load(Path.Combine(Server.MapPath("~/Reports/DetailCLV.rpt")));

            rd.Database.Tables[0].SetDataSource(ds);
            //rd.Database.Tables[1].SetDataSource(dSTableIlustrasi);
            //rd.SetParameterValue("UsiaCalonTertanggung", Math.Round((DateTime.Now - spajFormList.FirstOrDefault().CPPTanggalLahir.Value).TotalDays / 365.25));
            //rd.SetParameterValue("KodeAgen", User.Identity.Name);
            //rd.SetParameterValue("NamaAgen", User.Identity.FullName());
            //rd.SetParameterValue("IdProdukPar", spajForm.IdProduk);
            //rd.SetParameterValue("TotalPremiPar", totalPremi);
            //rd.SetParameterValue("DateGeneratedIlustrationPar", DateTime.Now.ToString("dd/MM/yyyy"));

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream streamReport = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            streamReport.Seek(0, SeekOrigin.Begin);

            //Response.AddHeader("content-disposition", "inline;filename=" + "Ilustration - " + spajForm.CalonPemegangPolis.NamaLengkap);
            var file = File(streamReport, "application/pdf");
            streamReport.Flush();

            return file;

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


