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
    public class PremiumRateDetailController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: PremiumRateDetail
        public ActionResult Index()
        {
            var premiumRateDetails = db.PremiumRateDetails.Include(p => p.PremiumRate1);


            return View(premiumRateDetails.ToList());
        }

        // GET: PremiumRateDetail/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PremiumRateDetails premiumRateDetails = db.PremiumRateDetails.Find(id);
            if (premiumRateDetails == null)
            {
                return HttpNotFound();
            }
            premiumRateDetails.IsAdult = db.CommonListValue.Where(x => x.CommonListValue2.Value == "IsAdult").Where(x => x.Value == premiumRateDetails.IsAdult).FirstOrDefault() != null ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "IsAdult").Where(x => x.Value == premiumRateDetails.IsAdult).FirstOrDefault().Text : premiumRateDetails.IsAdult;
            premiumRateDetails.IsSmoking = db.CommonListValue.Where(x => x.CommonListValue2.Value == "IsSmoking").Where(x => x.Value == premiumRateDetails.IsSmoking).FirstOrDefault() != null ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "IsSmoking").Where(x => x.Value == premiumRateDetails.IsSmoking).FirstOrDefault().Text : premiumRateDetails.IsSmoking;
            premiumRateDetails.Sex = db.CommonListValue.Where(x => x.CommonListValue2.Value == "SexPremium").Where(x => x.Value == premiumRateDetails.Sex).FirstOrDefault() != null ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "SexPremium").Where(x => x.Value == premiumRateDetails.Sex).FirstOrDefault().Text : premiumRateDetails.Sex;
            return View(premiumRateDetails);
        }

        // GET: PremiumRateDetail/Create
        public ActionResult Create()
        {
            var rateId = Request.Params["rateId"];

            if (rateId == null)
            {
                WarningMessagesAdd("RateId Can Not Be Null");
            }
            var longRateId = 0L;
            Int64.TryParse(rateId, out longRateId);
            var premiumRate = db.PremiumRate.Where(x => x.RateId == longRateId).FirstOrDefault();
            if (premiumRate == null)
            {
                WarningMessagesAdd("Premium Rate Is Not Found");
            }
            if (WarningMessages().Count == 0)
            {

                ViewBag.IsActive = WebAppUtility.SelectListIsActive();
                var sliIsAdult = new List<SelectListItem>();
                sliIsAdult.AddBlank();
                db.CommonListValue.Where(x => x.CommonListValue2.Value == "IsAdult").ToList().ForEach(x =>
                {
                    sliIsAdult.AddItemValText(x.Value, x.Text);
                });
                ViewBag.IsAdult = sliIsAdult.ToSelectList();

                var sliSex = new List<SelectListItem>();
                sliSex.AddBlank();
                db.CommonListValue.Where(x => x.CommonListValue2.Value == "SexPremium").ToList().ForEach(x =>
                {
                    sliSex.AddItemValText(x.Value, x.Text);
                });
                ViewBag.Sex = sliSex.ToSelectList();

                var sliIsSmoking = new List<SelectListItem>();
                sliIsSmoking.AddBlank();
                db.CommonListValue.Where(x => x.CommonListValue2.Value == "IsSmoking ").ToList().ForEach(x =>
                {
                    sliIsSmoking.AddItemValText(x.Value, x.Text);
                });
                ViewBag.IsSmoking = sliIsSmoking.ToSelectList();
                var sliRateId = new List<SelectListItem>();
                foreach (var item in db.PremiumRate)
                {
                    sliRateId.AddItemValText(item.RateId.ToString(), item.RateId.ToString() + " - " + item.Name);
                }
                ViewBag.RateId = sliRateId.ToSelectList(rateId, sliRateId.Where(x => x.Value != rateId).Select(x => x.Value));
                var model = new PremiumRateDetails();
                model.RateId = longRateId;
                return View(model);
            }
            return RedirectToAction("Details", "PremiumRate", new { rateId = premiumRate.RateId });
            
        }

        // POST: PremiumRateDetail/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,RateId,PaymentDuration,CoverageDuration,IsAdult,Sex,IsSmoking,RangeUpMin,RangeUpMax,RangeAgeMin,RangeAgeMax,PremiumRate,RecordStatus,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] PremiumRateDetails premiumRateDetails)
        {

            if (ModelState.IsValid)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        if (premiumRateDetails.RangeUpMin > premiumRateDetails.RangeUpMax)
                        {
                            WarningMessagesAdd("RangeUp Min can not be bigger than RangeUp Max");
                        }

                        if (premiumRateDetails.RangeAgeMin > premiumRateDetails.RangeAgeMax)
                        {
                            WarningMessagesAdd("RangeAge Min can not be bigger than RangeUp Max");
                        }
                        if (WarningMessages().Count == 0)
                        {
                            premiumRateDetails.SetPropertyCreate();
                            db.PremiumRateDetails.Add(premiumRateDetails);
                            db.SaveChanges();
                            SuccessMessagesAdd("Inserting Data Success");

                            return RedirectToAction("Details", "PremiumRate", new { rateId = premiumRateDetails.RateId });
                        }

                    }
                    catch (Exception e)
                    {
                        WarningMessagesAdd(e.MessageToList());
                    }
                }
            }

            ViewBag.RateId = new SelectList(db.PremiumRate, "RateId", "ScheduleId", premiumRateDetails.RateId);

            var sliIsAdult = new List<SelectListItem>();
            sliIsAdult.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "IsAdult").ToList().ForEach(x =>
            {
                sliIsAdult.AddItemValText(x.Value, x.Text);
            });
            ViewBag.IsAdult = sliIsAdult.ToSelectList();

            var sliSex = new List<SelectListItem>();
            sliSex.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "SexPremium").ToList().ForEach(x =>
            {
                sliSex.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Sex = sliSex.ToSelectList();

            var sliIsSmoking = new List<SelectListItem>();
            sliIsSmoking.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "IsSmoking ").ToList().ForEach(x =>
            {
                sliIsSmoking.AddItemValText(x.Value, x.Text);
            });
            ViewBag.IsSmoking = sliIsSmoking.ToSelectList();

            return RedirectToAction("Details", "PremiumRate", new { rateId = premiumRateDetails.RateId });
            
        }

        // GET: PremiumRateDetail/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PremiumRateDetails premiumRateDetails = db.PremiumRateDetails.Find(id);
            if (premiumRateDetails == null)
            {
                return HttpNotFound();
            }
            ViewBag.RateId = new SelectList(db.PremiumRate, "RateId", "RateId", premiumRateDetails.RateId);

            var sliIsAdult = new List<SelectListItem>();
            sliIsAdult.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "IsAdult").ToList().ForEach(x =>
            {
                sliIsAdult.AddItemValText(x.Value, x.Text);
            });
            ViewBag.IsAdult = sliIsAdult.ToSelectList(premiumRateDetails.IsAdult);

            var sliSex = new List<SelectListItem>();
            sliSex.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "SexPremium").ToList().ForEach(x =>
            {
                sliSex.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Sex = sliSex.ToSelectList(premiumRateDetails.Sex);


            var sliIsSmoking = new List<SelectListItem>();
            sliIsSmoking.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "IsSmoking ").ToList().ForEach(x =>
            {
                sliIsSmoking.AddItemValText(x.Value, x.Text);
            });
            ViewBag.IsSmoking = sliIsSmoking.ToSelectList(premiumRateDetails.IsSmoking);



            return View(premiumRateDetails);
        }

        // POST: PremiumRateDetail/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,RateId,PaymentDuration,CoverageDuration,IsAdult,Sex,IsSmoking,RangeUpMin,RangeUpMax,RangeAgeMin,RangeAgeMax,PremiumRate,RecordStatus,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] PremiumRateDetails premiumRateDetails)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    premiumRateDetails.SetPropertyUpdate();
                    db.Entry(premiumRateDetails).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    SuccessMessagesAdd("Editing Data Success");

                    return RedirectToAction("Details", "PremiumRate", new { rateId = premiumRateDetails.RateId });
                }
                catch (Exception e)
                {
                    WarningMessagesAdd(e.MessageToList());
                }

            }
            ViewBag.RateId = new SelectList(db.PremiumRate, "RateId", "RateId", premiumRateDetails.RateId);

            var sliIsAdult = new List<SelectListItem>();
            sliIsAdult.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "IsAdult").ToList().ForEach(x =>
            {
                sliIsAdult.AddItemValText(x.Value, x.Text);
            });
            ViewBag.IsAdult = sliIsAdult.ToSelectList(premiumRateDetails.IsAdult);

            var sliSex = new List<SelectListItem>();
            sliSex.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "SexPremium").ToList().ForEach(x =>
            {
                sliSex.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Sex = sliSex.ToSelectList(premiumRateDetails.Sex);


            var sliIsSmoking = new List<SelectListItem>();
            sliIsSmoking.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "IsSmoking ").ToList().ForEach(x =>
            {
                sliIsSmoking.AddItemValText(x.Value, x.Text);
            });
            ViewBag.IsSmoking = sliIsSmoking.ToSelectList(premiumRateDetails.IsSmoking);



            return RedirectToAction("Details", "PremiumRate", new { rateId = premiumRateDetails.RateId });
            //return View(premiumRateDetails);
        }

        // GET: PremiumRateDetail/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PremiumRateDetails premiumRateDetails = db.PremiumRateDetails.Find(id);
            if (premiumRateDetails == null)
            {
                return HttpNotFound();
            }
            return View(premiumRateDetails);
        }

        // POST: PremiumRateDetail/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            PremiumRateDetails premiumRateDetails = db.PremiumRateDetails.Find(id);
            var rateId = premiumRateDetails.RateId;
            db.PremiumRateDetails.Remove(premiumRateDetails);
            db.SaveChanges();
            SuccessMessagesAdd("Delate Success!");
            return RedirectToAction("Details", "PremiumRate", new { rateId });
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
