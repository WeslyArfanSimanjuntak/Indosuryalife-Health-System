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
    public class PremiumRateController : BaseController
    {
        private DBEntities db = new DBEntities();
       
        // GET: PremiumRate
        public ActionResult Index()
        {

            var PremiumRate = db.PremiumRate.ToList();
           
            return View(db.PremiumRate.ToList());
        }

        // GET: PremiumRate/Details/5
        public ActionResult Details(long? RateId)
        {
            if (RateId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PremiumRate premiumRate = db.PremiumRate.Find(RateId);
            if (premiumRate == null)
            {
                return HttpNotFound();
            }
            var premiumRateDetails = db.PremiumRateDetails.Where(x => x.RateId == premiumRate.RateId).ToList();
            ViewBag.PremiumRateDetails = premiumRateDetails;
            premiumRate.RateType = db.CommonListValue.Where(x => x.CommonListValue2.Value == "RateType").Where(x => x.Value == premiumRate.RateType).FirstOrDefault() != null ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "RateType").Where(x => x.Value == premiumRate.RateType).FirstOrDefault().Text : premiumRate.RateType;
            premiumRate.PaymentFrequency =db.CommonListValue.Where(x => x.CommonListValue2.Value == "PaymentFrequency").Where(x => x.Value == premiumRate.PaymentFrequency).FirstOrDefault() != null ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "PaymentFrequency").Where(x => x.Value == premiumRate.PaymentFrequency).FirstOrDefault().Text : premiumRate.PaymentFrequency;
            return View(premiumRate);
        }

        // GET: PremiumRate/Create
        public ActionResult Create()
        {
            ViewBag.IsActive = WebAppUtility.SelectListIsActive();
            PremiumRate premiumRate = new PremiumRate();
            premiumRate.ActuaryId = "00001";
            premiumRate.ClassOfRiskId = "01";
            premiumRate.Factor = 1;
          

            var sliPaymentFrequency = new List<SelectListItem>();
            sliPaymentFrequency.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "PaymentFrequency").ToList().ForEach(x =>
            {
                sliPaymentFrequency.AddItemValText(x.Value, x.Text);
            });
            ViewBag.PaymentFrequency = sliPaymentFrequency.ToSelectList();

            var sliRateType = new List<SelectListItem>();
            sliRateType.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "RateType").ToList().ForEach(x =>
            {
                sliRateType.AddItemValText(x.Value, x.Text);
            });
          
            ViewBag.RateType = sliRateType.ToSelectList();
            premiumRate.ActuaryId = "00001";
            premiumRate.ClassOfRiskId = "01";
            premiumRate.Factor = 1;
            premiumRate.InsurerId = "ISL";

            return View(premiumRate);


        }

        // POST: PremiumRate/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,InsurerId,ScheduleId,ActuaryId,RateId,RateType,ClassOfRiskId,PaymentDuration,CoverageDuration,PaymentFrequency,Factor,Name,RecordStatus,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] PremiumRate premiumRate)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (WarningMessages().Count == 0)
                    {
                        premiumRate.IsActive = 1;
                        if (premiumRate.IsActive == 1)
                        {
                            premiumRate.RecordStatus = "Active";
                        }
                        else { premiumRate.RecordStatus = "NonActive"; }


                        premiumRate.SetPropertyCreate();
                        db.PremiumRate.Add(premiumRate);
                        db.SaveChanges();

                        SuccessMessagesAdd("Inserting Data Success");

                        return RedirectToAction("Index");

                    }
                }
                catch (Exception e)
                {
                    WarningMessagesAdd(e.MessageToList());
                }
              
            }
           

            var sliPaymentFrequency = new List<SelectListItem>();
            sliPaymentFrequency.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "PaymentFrequency").ToList().ForEach(x =>
            {
                sliPaymentFrequency.AddItemValText(x.Value, x.Text);
            });
            ViewBag.PaymentFrequency = sliPaymentFrequency.ToSelectList();

            var sliRateType = new List<SelectListItem>();
            sliRateType.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "RateType").ToList().ForEach(x =>
            {
                sliRateType.AddItemValText(x.Value, x.Text);
            });

            ViewBag.RateType = sliRateType.ToSelectList();

            premiumRate.ActuaryId = "00001";
            premiumRate.ClassOfRiskId = "01";
            premiumRate.Factor = 1;
            premiumRate.InsurerId = "ISL";
                       

            


            return View(premiumRate);

        }

        // GET: PremiumRate/Edit/5
        public ActionResult Edit(long ? RateId)
        {
            if (RateId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PremiumRate premiumRate = db.PremiumRate.Find(RateId);
            if (premiumRate == null)
            {
                return HttpNotFound();
            }
           
            

            var sliPaymentFrequency = new List<SelectListItem>();
            sliPaymentFrequency.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "PaymentFrequency").ToList().ForEach(x =>
            {
                sliPaymentFrequency.AddItemValText(x.Value, x.Text);
            });
            ViewBag.PaymentFrequency = sliPaymentFrequency.ToSelectList(premiumRate.PaymentFrequency);

            var sliRateType = new List<SelectListItem>();
            sliRateType.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "RateType").ToList().ForEach(x =>
            {
                sliRateType.AddItemValText(x.Value, x.Text);
            });

            ViewBag.RateType = sliRateType.ToSelectList(premiumRate.RateType);
           

          
           
            return View(premiumRate);
        }

        // POST: PremiumRate/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,InsurerId,ScheduleId,ActuaryId,RateId,RateType,ClassOfRiskId,PaymentDuration,CoverageDuration,PaymentFrequency,Factor,Name,RecordStatus,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] PremiumRate premiumRate)
        {
            if (ModelState.IsValid)
            {
                try {
                    if (WarningMessages().Count == 0)
                    { 
                        premiumRate.SetPropertyUpdate();
                       
                       
                        db.Entry(premiumRate).State = System.Data.Entity.EntityState.Modified;
                        SuccessMessagesAdd("Editing Data Success");
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception e)
                {
                    WarningMessagesAdd(e.MessageToList());
                }
            }
           
            var sliPaymentFrequency = new List<SelectListItem>();
            sliPaymentFrequency.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "PaymentFrequency").ToList().ForEach(x =>
            {
                sliPaymentFrequency.AddItemValText(x.Value, x.Text);
            });
            ViewBag.PaymentFrequency = sliPaymentFrequency.ToSelectList(premiumRate.PaymentFrequency);

            var sliRateType = new List<SelectListItem>();
            sliRateType.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "RateType").ToList().ForEach(x =>
            {
                sliRateType.AddItemValText(x.Value, x.Text);
            });

            ViewBag.RateType = sliRateType.ToSelectList();
           
          
            return View("Edit",premiumRate);

        }

        // GET: PremiumRate/Delete/5
        public ActionResult Delete(long? RateId)
        {
            if (RateId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PremiumRate premiumRate = db.PremiumRate.Find(RateId);
            if (premiumRate == null)
            {
                return HttpNotFound();
            }

            return View(premiumRate);
        }

        // POST: PremiumRate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long? RateId)
        {
            
                PremiumRate premiumRate = db.PremiumRate.Find(RateId);
                db.PremiumRate.Remove(premiumRate);
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
