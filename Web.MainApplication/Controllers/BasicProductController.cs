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
    public class BasicProductController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: BasicProduct
        public ActionResult Index()
        {
            return View(db.BasicProduct.ToList());
        }

        // GET: BasicProduct/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BasicProduct basicProduct = db.BasicProduct.Find(id);
            if (basicProduct == null)
            {
                return HttpNotFound();
            }
            return View(basicProduct);
        }

        // GET: BasicProduct/Create
        public ActionResult Create()
        {
            ViewBag.IsActive = WebAppUtility.SelectListIsActive();
            return View();
        }

        // POST: BasicProduct/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,BpInsurerId,BasicProductId,BasicProductName,BasicProductDesc,CurrencyId,ProductType,RefundId,FclId,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] BasicProduct basicProduct)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    basicProduct.SetPropertyCreate();
                    db.BasicProduct.Add(basicProduct);
                    SuccessMessagesAdd("Inserting Data Success");
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    WarningMessagesAdd(e.MessageToList());
                }

            }

            ViewBag.IsActive = WebAppUtility.SelectListIsActive();
            return View(basicProduct);
        }

        // GET: BasicProduct/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BasicProduct basicProduct = db.BasicProduct.Find(id);
            if (basicProduct == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsActive = WebAppUtility.SelectListIsActive();
            return View(basicProduct);
        }

        // POST: BasicProduct/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BpInsurerId,BasicProductId,BasicProductName,BasicProductDesc,CurrencyId,ProductType,InvestmentRelated,FclId,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] BasicProduct basicProduct)
        {
            try
            {
                var basicProductOrigin = db.BasicProduct.Find(basicProduct.BasicProductId);
                basicProductOrigin.BasicProductName = basicProduct.BasicProductName;
                basicProductOrigin.BasicProductDesc = basicProduct.BasicProductDesc;
                basicProductOrigin.SetPropertyUpdate();
                db.Entry(basicProductOrigin).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                SuccessMessagesAdd("Editing Data Success");
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                WarningMessagesAdd(e.MessageToList());
            }

            WarningMessagesAdd(ModelState.ListErrors());
            return View(basicProduct);
        }

        // GET: BasicProduct/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BasicProduct basicProduct = db.BasicProduct.Find(id);
            if (basicProduct == null)
            {
                return HttpNotFound();
            }
            return View(basicProduct);
        }

        // POST: BasicProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {

            try
            {
                BasicProduct basicProduct = db.BasicProduct.Find(id);
                db.BasicProduct.Remove(basicProduct);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                WarningMessagesAdd(e.MessageToList());
                return RedirectToAction("Index");
            }

        }

        public ActionResult GenerateReport()
        {



            ReportDocument rd = new ReportDocument();
            var ds = new List<object>();

            db.BasicProduct.ToList().ForEach(x =>
            {
                ds.Add(new
                {
                    BasicProductName = x.BasicProductName != null ? x.BasicProductName : "",
                    BasicProductDesc = x.BasicProductDesc != null ? x.BasicProductDesc : "",
                    CurrencyId = x.CurrencyId != null ? x.CurrencyId : "",
                    x.Id,
                    x.BpInsurerId,
                    x.BasicProductId

                });

            });
            //data.ToList().ForEach(x => {

            //    object newObject = new object();
            //    newObject = x;
            //    ds.Add(newObject);

            //});
            rd.Load(Path.Combine(Server.MapPath("~/Reports/DetailBasicProduct.rpt")));

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
