using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using CrystalDecisions.CrystalReports.Engine;
using System.Web.Mvc;
using Repository.Application.DataModel;
using System.IO;

namespace Web.MainApplication.Controllers
{
    public class PlanController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: Plan
        public ActionResult Index()
        {
            var plan = db.Plan.Include(p => p.Policy);
            return View(plan.ToList());
        }

        // GET: Plan/Details/5
        public ActionResult Details(string id, string policyId)
        {
            if (id == null || policyId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plan plan = db.Plan.Where(x => x.PlanId == id && x.PolicyId == policyId).FirstOrDefault();
            if (plan == null )
            {
                return HttpNotFound();
            }
            plan.Policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == plan.Policy.PaymentFrequency).FirstOrDefault() != null ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == plan.Policy.PaymentFrequency).FirstOrDefault().Text : plan.Policy.PaymentFrequency;
            return View(plan);
        }

        // GET: Plan/Create
        public ActionResult Create(string policyId)
        {
            var policy = db.Policy.Find(policyId);
            var dropdownBP = new List<SelectListItem>();
            var dropdownBPL = new List<SelectListItem>();

            dropdownBP.AddBlank();
            dropdownBPL.AddBlank();

            db.BasicProduct.ToList().ForEach(x =>
            {
                dropdownBP.AddItemValText(x.BasicProductId.ToString(), x.BasicProductName);
            });

            db.BasicProductLimit.ToList().ForEach(x =>
            {
                dropdownBPL.AddItemValText(x.BasicProductLimitCode, x.BasicProductLimitCode);
            });

            ViewBag.BasicProduct = dropdownBP.ToSelectList();
            ViewBag.BasicProductLimit = dropdownBPL.ToSelectList();
            if (policyId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var plan = new Plan();
            policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == policy.PaymentFrequency).FirstOrDefault() != null
                ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == policy.PaymentFrequency).FirstOrDefault().Text : policy.PaymentFrequency;
            plan.Policy = policy;
            if (policy.StartDate.HasValue)
            {
                plan.StartDate = policy.StartDate;
            }


            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber");
            return View(plan);
        }

        // POST: Plan/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PolicyId,PlanId,PlanName,PlanDesc,StartDate,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] Plan plan)
        {
            if (ModelState.IsValid)
            {

                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var planDetailData = Request.Params["PlanDetailData"];
                        var selectedPlan = planDetailData.Split(',').ToList();
                        var listPlanDetail = new List<PlanDetail>();

                        for (int i = 0; i < selectedPlan.Count; i++)
                        {
                            var limitcode = selectedPlan.ElementAt(i).Split(';').ElementAtOrDefault(1);
                            if (db.BasicProductLimit.Where(z => z.BasicProductLimitCode == limitcode).FirstOrDefault() == null)
                            {
                                WarningMessagesAdd(limitcode + " is not found");
                            }
                            else
                            {
                                var newPlanDetail = new PlanDetail();
                                newPlanDetail.PolicyId = plan.PolicyId;
                                newPlanDetail.BasicProductLimitCode = limitcode;
                                newPlanDetail.BasicProductId = selectedPlan.ElementAt(i).Split(';').ElementAtOrDefault(0);
                                newPlanDetail.PlanId = plan.PlanId;
                                newPlanDetail.SetPropertyCreate();
                                listPlanDetail.Add(newPlanDetail);
                            }

                        }
                        if ((listPlanDetail != null) && (!listPlanDetail.Any()))
                        {
                            WarningMessagesAdd("Plan Detail List can not be null");
                        }
                        if (plan.PlanId == null)
                        {
                            WarningMessagesAdd("Plan ID can not be null");
                        }
                        if (plan.PlanName == null)
                        {
                            WarningMessagesAdd("Plan Name can not be null");
                        }
                        if (plan.PlanName == null)
                        {
                            WarningMessagesAdd("Plan Description can not be null");
                        }
                        if (plan.StartDate == null)
                        {
                            WarningMessagesAdd("Plan Startdate can not be null");
                        }
                        if (WarningMessages().Count == 0)
                        {

                            db.PlanDetail.AddRange(listPlanDetail);
                            plan.SetPropertyCreate();
                            db.Plan.Add(plan);
                            db.SaveChanges();
                            dbTransaction.Commit();
                            SuccessMessagesAdd("Inserting Data Success");
                            return RedirectToAction("Details", "Policy", new { id = plan.PolicyId });
                        }
                    }
                    catch (Exception e)
                    {
                        WarningMessagesAdd(e.MessageToList());
                        dbTransaction.Rollback();
                    }
                }
            }
            var policy = db.Policy.Find(plan.PolicyId);
            var dropdownBP = new List<SelectListItem>();
            var dropdownBPL = new List<SelectListItem>();

            dropdownBP.AddBlank();
            dropdownBPL.AddBlank();

            db.BasicProduct.ToList().ForEach(x =>
            {
                dropdownBP.AddItemValText(x.BasicProductId.ToString(), x.BasicProductName);
            });

            db.BasicProductLimit.ToList().ForEach(x =>
            {
                dropdownBPL.AddItemValText(x.BasicProductLimitCode, x.BasicProductLimitCode);
            });

            ViewBag.BasicProduct = dropdownBP.ToSelectList();
            ViewBag.BasicProductLimit = dropdownBPL.ToSelectList();
            if (policy.PolicyId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == policy.PaymentFrequency).FirstOrDefault() != null
                ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == policy.PaymentFrequency).FirstOrDefault().Text : policy.PaymentFrequency;
            plan.Policy = policy;
            if (policy.StartDate.HasValue)
            {
                plan.StartDate = policy.StartDate;
            }
            return View("Create", plan);
        }

        // GET: Plan/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plan plan = db.Plan.Find(id);
            if (plan == null)
            {
                return HttpNotFound();
            }
            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber", plan.PolicyId);
            return View(plan);
        }

        // POST: Plan/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PolicyId,PlanId,PlanName,PlanDesc,StartDate,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] Plan plan)
        {
            if (ModelState.IsValid)
            {
                plan.SetPropertyUpdate();
                db.Entry(plan).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                SuccessMessagesAdd("Inserting Data Success");
                return RedirectToAction("Index");
            }
            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber", plan.PolicyId);
            return View(plan);
        }
        public ActionResult GenerateReport()
        {
            ReportDocument rd = new ReportDocument();
            var ds = new List<object>();

            db.Plan.ToList().ForEach(x =>
            {
                ds.Add(new
                {
                    PolicyId = x.PolicyId != null ? x.PolicyId : "",
                    PlanId = x.PlanId != null ? x.PlanId : "",
                    StartDate = !x.StartDate.HasValue ? new DateTime(9999, 12, 31) : x.StartDate.Value,
                    PlanName = x.PlanName != null ? x.PlanName : "",
                    PlanDesc = x.PlanDesc != null ? x.PlanDesc : ""
                });

            });
            //data.ToList().ForEach(x => {

            //    object newObject = new object();
            //    newObject = x;
            //    ds.Add(newObject);

            //});
            rd.Load(Path.Combine(Server.MapPath("~/Reports/DetailPlan.rpt")));

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
        // GET: Plan/Delete/5
        public ActionResult Delete(string planId, string policyId)
        {
            if (planId == null || policyId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plan plan = db.Plan.Find(policyId, planId);
            if (plan == null)
            {
                return HttpNotFound();
            }
            if (Request.IsAjaxRequest())
            {
                ViewBag.LayoutIsNull = Convert.ToBoolean(Request.Params["layoutIsNull"]);
                return View("Delete", plan);

            }
            return View(plan);
        }

        // POST: Plan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string planId, string policyId)
        {
            try
            {
                if (planId == null || policyId == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Plan plan = db.Plan.Find(policyId, planId);
                var policy = db.Policy.Find(policyId);
                if (plan == null || policy == null)
                {
                    return HttpNotFound();
                }
                var planDetail = db.PlanDetail.Where(x => x.PlanId == plan.PlanId && x.PolicyId == policyId).ToList();
                var memberPlan = db.Member.Where(x => x.PlanId == plan.PlanId && x.PolicyId == policy.PolicyId);

                
                if (memberPlan.Count() > 0)
                {
                    WarningMessagesAdd("Plan is related to Member");
                }
                if (WarningMessages().Count == 0)
                {
                    using (var dbTransaction = db.Database.BeginTransaction())
                    {
                        try
                        {

                            db.Plan.Remove(plan);

                            var plantDetailToRemove = db.PlanDetail.Where(x => x.PolicyId == policy.PolicyId && x.PlanId == plan.PlanId).ToList();
                            db.PlanDetail.RemoveRange(plantDetailToRemove);
                            db.SaveChanges();
                            dbTransaction.Commit();
                            SuccessMessagesAdd("Deleting is success");
                        }
                        catch (Exception e)
                        {
                            dbTransaction.Rollback();
                            WarningMessagesAdd(e.MessageToList());
                        }
                    }

                }
                
            }
            catch (Exception e)
            {
                WarningMessagesAdd(e.MessageToList());
            }

            return RedirectToAction("Details", "Policy", new { id = policyId, tab = "plan" });
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
