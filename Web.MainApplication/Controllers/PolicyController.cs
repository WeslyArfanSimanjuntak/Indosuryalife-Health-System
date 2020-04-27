using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using CrystalDecisions.CrystalReports.Engine;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Repository.Application.DataModel;
using Web.MainApplication.Models;
using Web.MainApplication.ReportModel;
using Web.MainApplication.Reports;

namespace Web.MainApplication.Controllers
{
    public class PolicyController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: Policy
        public ActionResult Index()
        {
            var policy = db.Policy.Include(p => p.Client);
            return View(policy.ToList());
        }

        // GET: Policy/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }



            Policy policy = db.Policy.Find(id);


            if (policy == null)
            {
                return HttpNotFound();
            }
            ViewBag.Tab = Request.Params["tab"] ?? "";
            if (Request.IsAjaxRequest())
            {
                if (Request.Params["DeleteAllMember"] == "true")
                {
                    var viewResult = new ViewResult();
                    viewResult.TempData = View().TempData;
                    return viewResult;
                }
                decimal sumOfPremi = 0;
                var allMember = db.Member.Where(x => x.PolicyId == id && x.StartDate != null & x.EndDate != null).ToList();
                foreach (var item in allMember)
                {
                    if ((item.EndDate.Value.Year - item.StartDate.Value.Year) > 500)
                    {
                        WarningMessagesAdd(item.Client.FullName + ", Calculation For More Than 500 Years Is Not Allowed");
                    }
                    else
                    {
                        sumOfPremi = sumOfPremi + this.CalculateMemberPremi(item, 1, 1);
                    }
                }
                var pcfTotalAmount = db.PCF.Where(x => x.PolicyId == policy.PolicyId && x.TransType == "P" && x.InvoiceDate < DateTime.Now).Select(x => x.Amount.Value).ToList();

                var retval = this.Json(new
                {
                    calculateResult = sumOfPremi
                }, JsonRequestBehavior.AllowGet);
                ViewBag.Title = "Details Policy Calculation";
                ViewBag.SumOfPremi = WarningMessages().Count == 0 ? sumOfPremi : 0;
                ViewBag.SumOfDemandedPremi = WarningMessages().Count == 0 ? pcfTotalAmount.Sum() : 0;
                ViewBag.WarningMessage = WarningMessages().ToList();
                //policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == policy.PaymentFrequency).FirstOrDefault() != null ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == policy.PaymentFrequency).FirstOrDefault().Text : policy.PaymentFrequency;
                if (WarningMessages().Count == 0)
                {
                    SuccessMessagesAdd("Calculation Success");
                    //ViewBag.SuccessMessages = SuccessMessages().ToList();
                    //SuccessMessages().Clear();
                }
                else
                {
                    WarningMessages().Insert(0, "Calculation Failed");
                    var pcfToDelete = db.PCF.Where(x => x.PolicyId == policy.PolicyId);
                    db.PCF.RemoveRange(pcfToDelete);
                    db.SaveChanges();
                }
                //WarningMessages().Clear();

                return View("_CalculationResult", policy);

                //return retval;
            }
            policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == policy.PaymentFrequency).FirstOrDefault() != null ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == policy.PaymentFrequency).FirstOrDefault().Text : policy.PaymentFrequency;

            return View(policy);


        }
        // GET: Policy/Create
        public ActionResult Create()
        {
            var sliFrequency = new List<SelectListItem>();
            sliFrequency.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").ToList().ForEach(x =>
            {
                sliFrequency.AddItemValText(x.Value, x.Text);
            });
            ViewBag.PaymentFrequency = sliFrequency.ToSelectList();

            var sliAgent = new List<SelectListItem>();
            sliAgent.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Agent").ToList().ForEach(x =>
            {
                sliAgent.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Agent = sliAgent.ToSelectList();

            var sliOpenOrClose = new List<SelectListItem>();
            sliOpenOrClose.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "OpenOrClose").ToList().ForEach(x =>
            {
                sliOpenOrClose.AddItemValText(x.Value, x.Text);
            });
            ViewBag.OpenOrClose = sliOpenOrClose.ToSelectList();

            ViewBag.IsActive = WebAppUtility.SelectListIsActive();
            ViewBag.ClientId = new SelectList(db.Client, "ClientId", "Type");
            return View();
        }


        // POST: Policy/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PolicyId,PolicyNumber,ClientId,StartDate,MatureDate,LastEndorseDate,ExitDate,TerminateDate,PaymentFrequency,OpenOrClose,Agent,PolicyStatus,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] Policy policy)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    policy.PolicyId = "";
                    policy.SetPropertyCreate();
                    if (policy.OpenOrClose == "1")
                    {
                        policy.MatureDate = new DateTime(2999, 12, 31);
                    }
                    policy.PolicyStatus = "0";
                    if (policy.ClientId == null)
                    {
                        WarningMessagesAdd("Client ID can not be NULL");
                    }
                    if (policy.StartDate > policy.MatureDate)
                    {
                        WarningMessagesAdd("Start Date can not be latter than Mature Date");
                    }
                    if (policy.PaymentFrequency == null)
                    {
                        WarningMessagesAdd("Payment Frequency can not be NULL");
                    }
                    if (WarningMessages().Count == 0)
                    {
                        policy.SetPropertyCreate();
                        db.Policy.Add(policy);
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
            var sliFrequency = new List<SelectListItem>();
            sliFrequency.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").ToList().ForEach(x =>
            {
                sliFrequency.AddItemValText(x.Value, x.Text);
            });
            ViewBag.PaymentFrequency = sliFrequency.ToSelectList();

            var sliAgent = new List<SelectListItem>();
            sliAgent.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Agent").ToList().ForEach(x =>
            {
                sliAgent.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Agent = sliAgent.ToSelectList();

            var sliOpenOrClose = new List<SelectListItem>();
            sliOpenOrClose.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "OpenOrClose").ToList().ForEach(x =>
            {
                sliOpenOrClose.AddItemValText(x.Value, x.Text);
            });
            ViewBag.OpenOrClose = sliOpenOrClose.ToSelectList();

            ViewBag.IsActive = WebAppUtility.SelectListIsActive();


            ViewBag.ClientId = new SelectList(db.Client, "ClientId", "Type", policy.ClientId);

            return View("Create", policy);
        }

        // GET: Policy/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Policy policy = db.Policy.Find(id);
            if (policy == null)
            {
                return HttpNotFound();
            }
            var sliFrequency = new List<SelectListItem>();
            sliFrequency.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").ToList().ForEach(x =>
            {
                sliFrequency.AddItemValText(x.Value, x.Text);
            });
            ViewBag.PaymentFrequency = sliFrequency.ToSelectList(policy.PaymentFrequency);

            var sliStatus = new List<SelectListItem>();
            sliStatus.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Status").ToList().ForEach(x =>
            {
                sliStatus.AddItemValText(x.Value, x.Text);
            });
            ViewBag.PolicyStatus = sliStatus.ToSelectList(policy.PolicyStatus);

            var sliAgent = new List<SelectListItem>();
            sliAgent.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Agent").ToList().ForEach(x =>
            {
                sliAgent.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Agent = sliAgent.ToSelectList(policy.Agent);

            ViewBag.IsActive = WebAppUtility.SelectListIsActive(policy.IsActive);

            ViewBag.OpenOrClose = WebAppUtility.SelectListOpenOrClose(policy.OpenOrClose);

            return View(policy);
        }

        // POST: Policy/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PolicyId,PolicyNumber,ClientId,StartDate,MatureDate,LastEndorseDate,ExitDate,TerminateDate,PaymentFrequency,OpenOrClose,Agent,PolicyStatus,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] Policy policy)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //if (somerrror)
                    //{
                    //    WarningMessagesAdd("Set Warning Message here");
                    //}
                    if (WarningMessages().Count == 0)
                    {
                        policy.SetPropertyUpdate();
                        db.Entry(policy).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }

                }

                catch (Exception e)
                {
                    WarningMessagesAdd(e.MessageToList());
                }
            }
            var sliFrequency = new List<SelectListItem>();
            sliFrequency.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").ToList().ForEach(x =>
            {
                sliFrequency.AddItemValText(x.Value, x.Text);
            });
            ViewBag.PaymentFrequency = sliFrequency.ToSelectList(policy.PaymentFrequency);

            var sliAgent = new List<SelectListItem>();
            sliAgent.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Agent").ToList().ForEach(x =>
            {
                sliAgent.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Agent = sliAgent.ToSelectList(policy.Agent);

            ViewBag.IsActive = WebAppUtility.SelectListIsActive(policy.IsActive);

            ViewBag.OpenOrClose = WebAppUtility.SelectListOpenOrClose(policy.OpenOrClose);
            return View("Edit", policy);
        }

        // GET: Policy/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Policy policy = db.Policy.Find(id);
            //Member member = db.Member.Find(id);
            if (policy == null)
            {
                return HttpNotFound();
            }

            //if (member.MemberStatus != "Inactive")
            //{
            //    WarningMessagesAdd("you can not delete this member");
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            return View(policy);
        }

        // POST: Policy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {

                    Policy policy = db.Policy.Find(id);
                    if (policy.PolicyStatus == "0")
                    {
                        db.Plan.RemoveRange(policy.Plan);
                        db.PCF.RemoveRange(policy.PCF);
                        db.Member.RemoveRange(policy.Member);
                        db.MemberPlan.RemoveRange(policy.MemberPlan);
                        db.PlanDetail.RemoveRange(policy.PlanDetail);
                        db.Policy.Remove(policy);
                        db.SaveChanges();
                        dbTransaction.Commit();
                        SuccessMessagesAdd(Message.DeleteSuccess);
                        return RedirectToAction("Index");

                    }
                    else
                    {
                        WarningMessagesAdd("Policy Can Not Be Deleted");
                    }
                    return RedirectToAction("Index");

                }
                catch (Exception e)
                {

                    WarningMessagesAdd(e.MessageToList());
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult PolicyMemberBenefit(string policyId, string memberId, string id, string bpLimitId)
        {
            var temp = Request;
            if (id == null || bpLimitId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BasicProductLimitHdr basicProductLimitHdr = db.BasicProductLimitHdr.Where(x => x.BasicProductId == id && x.BPLimitId == bpLimitId).FirstOrDefault();
            if (basicProductLimitHdr == null)
            {
                return HttpNotFound();
            }
            var selectedBenefit = db.BasicProductLimit.Where(x => x.BasicProductId == basicProductLimitHdr.BasicProductId && x.BasicProductLimitCode == basicProductLimitHdr.BPLimitId).ToList();
            var allActiveBenefit = db.Benefit.Where(x => x.IsActive == 1).ToList();
            var tempBenefitList = new List<Benefit>();


            foreach (var item in allActiveBenefit)
            {
                var newBenefit = new Benefit();
                newBenefit.BenefitCode = item.BenefitCode;
                newBenefit.BenefitDesc = item.BenefitDesc;
                newBenefit.BenefitName = item.BenefitName;
                if (selectedBenefit.Where(x => x.BenefitCode == item.BenefitCode).FirstOrDefault() != null)
                {
                    var basicProductLimitNew = selectedBenefit.Where(y => y.BenefitCode == item.BenefitCode
                    ).FirstOrDefault();
                    newBenefit.BasicProductLimit = new List<BasicProductLimit>
                    {
                        basicProductLimitNew
                    };
                    newBenefit.IsActive = 1;
                }
                else
                {
                    newBenefit.BasicProductLimit = new List<BasicProductLimit>();
                    newBenefit.IsActive = 0;
                }

                tempBenefitList.Add(newBenefit);
            }

            ViewBag.ListOfBenefit = tempBenefitList.Where(x => x.IsActive == 1).ToList();
            if (Request.Params["layout"] == "null")
            {
                return View("_Modal", new ModalView()
                {
                    Body = this.RenderRazorViewToString("PolicyMemberBenefit", basicProductLimitHdr),
                    WidthPercentage = 90,
                    ModalForm = new ModalForm()
                });
            }
            return View(basicProductLimitHdr);

        }

        private decimal CalculateMemberPremi(Member member, int coverageDuration, int paymentDuration)
        {
            var listOfDecimal = new List<decimal>();
            using (var dbTransaction = db.Database.BeginTransaction())
            {

                try
                {
                    var age = Math.Round((DateTime.Now - member.Client.BirthDate).Value.TotalDays / 365.25, 0);
                    var isAdult = "N";
                    if (age > 22)
                    {
                        isAdult = "Y";
                    }
                    if (member.Client.MaritalStatus == "Married")
                    {
                        isAdult = "Y";
                    }

                    var sex = member.Client.Sex == "Female" ? "F" : "M";
                    var lengthOfBenefitDayPerYear = (member.EndDate - member.StartDate).Value.TotalDays / 365.25;
                    var lengthOfBenefitDayPerYearTermLife = (member.EndDate - member.StartDate).Value.TotalDays / 365;
                    var memberPlan = db.MemberPlan.Where(x => x.MemberId == member.MemberId && x.PolicyId == member.PolicyId).ToList();
                    db.PCF.RemoveRange(db.PCF.Where(x => x.MemberId == member.MemberId && x.PolicyId == member.PolicyId).ToList());
                    db.SaveChanges();
                    //foreach (var item in memberPlan)
                    //{
                    //    var termLifeBenefit = db.BasicProductLimit.Where(x => x.BasicProductId == item.BasicProductId && x.BasicProductLimitCode == item.BasicProductLimitCode && x.BenefitCode == "IP016").FirstOrDefault();
                    //    //var termLifeBenefit = db.BasicProductLimit.Where(x => x.BasicProductId == item.BasicProductId && x.BasicProductLimitCode == item.BasicProductLimitCode && x.BenefitCode == "IP016");
                    //    if (termLifeBenefit != null)
                    //    {
                    //        var pcfToDeleteTL = db.PCF.Where(x => x.PolicyId == member.PolicyId && x.MemberId == member.MemberId && x.TransType == "P" && x.BasicProductId == termLifeBenefit.BasicProductId);
                    //        db.PCF.RemoveRange(pcfToDeleteTL);
                    //        db.SaveChanges();
                    //    }
                    //    var pcfToDelete = db.PCF.Where(x => x.PolicyId == member.PolicyId && x.MemberId == member.MemberId && x.TransType == "P" && x.BasicProductId == item.BasicProductId);
                    //    db.PCF.RemoveRange(pcfToDelete);
                    //    db.SaveChanges();

                    //}


                    foreach (var item in memberPlan)
                    {
                        string sql = "select prd.PremiumRate " +
                            "from PremiumRate pr " +
                            "join PremiumRateDetails prd " +
                            "on pr.RateId = prd.RateId " +
                            "and pr.CoverageDuration = prd.CoverageDuration " +
                            "and pr.PaymentDuration = prd.PaymentDuration " +
                            "where pr.ScheduleId ='" + item.BasicProductLimitCode + "' " +
                            (isAdult != "N" ? "and prd.IsAdult = '" + isAdult + "' " : " ") +
                            "and prd.Sex = '" + (isAdult != "N" ? sex : "B") + "'";

                        var premiumRate = db.Database.SqlQuery<decimal?>(sql).FirstOrDefault();

                        if (premiumRate == null)
                        {
                            WarningMessagesAdd("Premium Rate Is Not Found for " + item.Member.Client?.FullName + ", " + item.Member.PlanId + ", " + item.BasicProductLimitCode);
                        }
                        var frequecyToNumber = 0;
                        decimal multiplierFactorPercentage = new decimal(0);


                        if (member.Policy.PaymentFrequency == "M")
                        {
                            frequecyToNumber = 1;
                            multiplierFactorPercentage = decimal.Multiply(decimal.Divide(10, 100), 12);
                        }
                        else if (member.Policy.PaymentFrequency == "Q")
                        {
                            frequecyToNumber = 4;
                            multiplierFactorPercentage = decimal.Multiply(decimal.Divide(27, 100), 4);
                        }
                        else if (member.Policy.PaymentFrequency == "S")
                        {
                            frequecyToNumber = 6;
                            multiplierFactorPercentage = decimal.Multiply(decimal.Divide(52, 100), 2);
                        }
                        else if (member.Policy.PaymentFrequency == "Y")
                        {
                            frequecyToNumber = 12;
                            multiplierFactorPercentage = decimal.Multiply(decimal.Divide(100, 100), 1);
                        }
                        //var totalOfPremium = (new Decimal(lengthOfBenefitDayPerYear) * (premiumRate ?? 0) + ((new Decimal(lengthOfBenefitDayPerYear) * (premiumRate ?? 0)) * 10 / 100));
                        var totalOfPremium = (new Decimal(lengthOfBenefitDayPerYear) * (premiumRate ?? 0));

                        int counter = 0;
                        bool fixedEndDate = false;
                        for (var date = member.StartDate.Value.AddYears(1); date <= member.EndDate; date = date.AddYears(1))
                        {
                            if (date == member.EndDate)
                            {
                                fixedEndDate = true;
                            }
                            counter++;
                        }
                        if (fixedEndDate)
                        {
                            totalOfPremium = (Decimal)(counter * (premiumRate ?? 0));
                        }
                        var datetimeNow = DateTime.Now;
                        var yearslater = datetimeNow.AddYears(3);
                        var tempp = yearslater - datetimeNow;

                        listOfDecimal.Add(totalOfPremium);
                        //retval = retval + (new Decimal(lengthOfBenefitPerYear) * (premiumRate ?? 0) + ((new Decimal(lengthOfBenefitPerYear) * (premiumRate ?? 0)) * 10 / 100));

                        //add data to PCF Table


                        var iteration = 0;
                        for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                        {
                            iteration++;
                        }
                        var premiumPerInvoice = totalOfPremium / iteration;

                        //Pengecekan apakah ada benefit santunan kematian benefit dari user, apabila ada, maka akan dimasukkan pada table pcf
                        var termLifeBenefit = db.BasicProductLimit.Where(x => x.BasicProductId == item.BasicProductId && x.BasicProductLimitCode == item.BasicProductLimitCode && x.BenefitCode == "IP016").FirstOrDefault();
                        //var termLifeBenefit = db.BasicProductLimit.Where(x => x.BasicProductId == item.BasicProductId && x.BasicProductLimitCode == item.BasicProductLimitCode && x.BenefitCode == "IP016");
                        if (termLifeBenefit != null)
                        {
                            var termLifeBP = db.BasicProduct.Where(x => x.BasicProductId == "TL").FirstOrDefault();
                            var percentagePremiFromInpatien = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Term Life Premium").FirstOrDefault();
                            var premiumTermLife = new decimal(lengthOfBenefitDayPerYearTermLife) * termLifeBenefit.LimitAmount * Convert.ToDecimal(percentagePremiFromInpatien.Value);

                            if (percentagePremiFromInpatien != null)
                            {
                                if (termLifeBP == null)
                                {
                                    WarningMessagesAdd("TermiLife Is Not Found In BasicProduct");
                                }
                                for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                                {
                                    db.SaveChanges();
                                    var newPCF = new PCF();
                                    newPCF.PolicyId = item.PolicyId;
                                    newPCF.BasicProductId = termLifeBP.BasicProductId;
                                    newPCF.Amount = premiumTermLife * multiplierFactorPercentage;
                                    newPCF.MemberId = item.MemberId;
                                    newPCF.TransType = TransactionType.Premium;
                                    newPCF.InvoiceDate = i.Value;
                                    newPCF.SetPropertyCreate();
                                    db.PCF.Add(newPCF);
                                }
                            }
                            for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                            {


                                db.SaveChanges();
                                var newPCF = new PCF();
                                newPCF.PolicyId = item.PolicyId;
                                newPCF.BasicProductId = item.BasicProductId;
                                newPCF.Amount = (premiumPerInvoice - premiumTermLife) * multiplierFactorPercentage;
                                newPCF.MemberId = item.MemberId;
                                newPCF.TransType = TransactionType.Premium;
                                newPCF.InvoiceDate = i.Value;
                                newPCF.SetPropertyCreate();
                                db.PCF.Add(newPCF);
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                            {


                                db.SaveChanges();
                                var newPCF = new PCF();
                                newPCF.PolicyId = item.PolicyId;
                                newPCF.BasicProductId = item.BasicProductId;
                                newPCF.Amount = premiumPerInvoice * multiplierFactorPercentage;
                                newPCF.MemberId = item.MemberId;
                                newPCF.TransType = TransactionType.Premium;
                                newPCF.InvoiceDate = i.Value;
                                newPCF.SetPropertyCreate();
                                db.PCF.Add(newPCF);
                            }
                        }
                        if (WarningMessages().Count == 0)
                        {
                            member.MemberStatus = MemberStatus.Calculated;
                            db.Entry(member).State = System.Data.Entity.EntityState.Modified;
                        }
                        db.SaveChanges();


                    }
                    if (WarningMessages().Count != 0)
                    {
                        dbTransaction.Rollback();
                        var error = WarningMessages().ToList();
                    }
                    else
                    {
                        dbTransaction.Commit();

                    }
                }
                catch (Exception e)
                {
                    dbTransaction.Rollback();
                    WarningMessagesAdd(e.MessageToList());

                }
            }
            //decimal retval = 0;


            //var memberPlan = db.MemberPlan.Where(x=>x.MemberId==member.MemberId && x.PolicyId == member.PolicyId)
            //var premi1Year = db.PremiumRate.Where(x => x.CoverageDuration == coverageDuration && x.PaymentDuration == paymentDuration)
            //    .Join(db.PremiumRateDetails, pr => pr.RateId , prd => prd.RateId, (pr, prd) => new { pr, prd });
            var pcfTotalAmount = db.PCF.Where(x => x.PolicyId == member.PolicyId && x.MemberId == member.MemberId && x.TransType == "P").Select(x => x.Amount.Value).ToList();
            var retvalDec = listOfDecimal.Sum();
            var sumPCF = pcfTotalAmount.Sum(x => x);
            return sumPCF;
            //return retval;

        }

        public ActionResult GenerateReport()
        {
            string policyId = Request.Params["policyId"];
            var policy = db.Policy.Find(policyId);
            if (policy == null)
            {
                return HttpNotFound();
            }
            if (Request.IsAjaxRequest())
            {

                if (policy == null)
                {
                    return HttpNotFound();
                }
                return View("_Modal", new ModalView()
                {
                    Title = "Generate Report",
                    Body = this.RenderRazorViewToString("GenerateReport", policy),
                    Footer = this.GetHtmlHelper().ActionLink("Download", "GenerateReport", new { policyId = policy.PolicyId }, new { @href = "#", @class = "btn btn-primary", @style = "background-color:#008CBA; color:white;", @onclick = "BeforePopItUp('" + Url.Action("GenerateReport", new { policyId = policy.PolicyId }) + "')" }).ToString(),
                    ModalForm = new ModalForm()
                    {
                        ActionName = "GenerateReport",
                        ControllerName = "Member",
                        RouteValues = new { policyId = policy.PolicyId }
                    }
                });

            }

            var startDate = Convert.ToDateTime(Request.Params["startDate"]);
            var endDate = Convert.ToDateTime(Request.Params["endDate"]);
            var financeTransactions = db.FinanceTransaction.Where(x => x.EffectiveDate >= startDate && x.EffectiveDate <= endDate);
            var allDSMemberMovement = db.Member_Movement.Where(x => x.EffectiveDate >= startDate && x.EffectiveDate <= endDate && x.PolicyId == policyId).ToList();

            decimal? sumPremiIP = 0;
            decimal? sumPremiOP = 0;
            decimal? sumPremiDE = 0;
            decimal? sumPremiMA = 0;
            decimal? sumPremiTL = 0;
            decimal? sumPremiRefund = 0;
            decimal? sumPremiCorrection = 0;
            var listOfReportModel = new List<MemberReportModel>();
            foreach (var item in allDSMemberMovement.OrderBy(x => x.EffectiveDate).GroupBy(x => x.MemberId))
            {
                var memberMovement = allDSMemberMovement.Where(x => x.MemberId == item.Key).FirstOrDefault();
                try
                {
                    //var reportModelExist = listOfReportModel.Where(x => x.MemberId == item.MemberId.ToString()).FirstOrDefault();
                    //if (reportModelExist == null)
                    //{
                    var memberMovementClient = db.Member_Movement_Client.Find(item.Key);
                    var planId = "";
                    var planSequence = db.CommonListValue.Where(x => x.Text == AplicationConfig.PlanBasicProductSequence).FirstOrDefault();
                    var memberPlans = memberMovement.Member.MemberPlan.ToList();

                    foreach (var bpItem in planSequence.Value.Split(';'))
                    {
                        var memberPlan = memberPlans.Where(x => x.BasicProductId == bpItem).FirstOrDefault();
                        if (memberPlan != null)
                        {
                            planId = planId + memberPlan.BasicProductLimitCode + " ";
                        }
                    }

                    foreach (var memberPlan in memberMovement.Member.MemberPlan.OrderBy(x => x.BasicProductLimitCode))
                    {
                        planId = planId + memberPlan.BasicProductLimitCode + " ";
                    }
                    //                 var query = database.Posts    // your starting point - table in the "from" statement
                    //.Join(database.Post_Metas, // the source table of the inner join
                    //   post => post.ID,        // Select the primary key (the first part of the "on" clause in an sql "join" statement)
                    //   meta => meta.Post_ID,   // Select the foreign key (the second part of the "on" clause)
                    //   (post, meta) => new { Post = post, Meta = meta }) // selection
                    //.Where(postAndMeta => postAndMeta.Post.ID == id);
                    var itemMemberPCF = memberMovement.Member.PCF.Join(financeTransactions, post => post.TransactionNumber, meta => meta.TransactionNumber, (post, meta) => new { PCF = post }).Select(x => x.PCF).ToList();
                    var recordMode = memberMovement.Member.Member_Movement.OrderByDescending(x => x.EffectiveDate).FirstOrDefault().RecordMode;
                    var commonListValue = db.CommonListValue.Where(x => x.CommonListValue2.Value == "RecordModeParent").ToList();

                    var keterangan = commonListValue.Where(x => x.Value == recordMode.ToString()).FirstOrDefault().Desc;

                    var premiIPPlusRefundIP = itemMemberPCF.Where(x => x.BasicProductId == "IP" && x.TransType == "P").Sum(x => x.Amount);
                    var PremiIP = premiIPPlusRefundIP == 0 ? "-" : string.Format("{0:N}", premiIPPlusRefundIP);

                    var premiOPPlusRefundOP = itemMemberPCF.Where(x => x.BasicProductId == "OP" && x.TransType == "P").Sum(x => x.Amount);
                    var PremiOP = premiOPPlusRefundOP == 0 ? "-" : string.Format("{0:N}", premiOPPlusRefundOP);


                    var premiDEPlusRefundDE = itemMemberPCF.Where(x => x.BasicProductId == "DE" && x.TransType == "P").Sum(x => x.Amount);
                    var PremiDE = premiDEPlusRefundDE == 0 ? "-" : string.Format("{0:N}", premiDEPlusRefundDE);

                    var premiMAPlusRefundMA = itemMemberPCF.Where(x => x.BasicProductId == "MA" && x.TransType == "P").Sum(x => x.Amount);
                    var PremiMA = premiMAPlusRefundMA == 0 ? "-" : string.Format("{0:N}", premiMAPlusRefundMA);

                    var premiTLPlusRefundTL = itemMemberPCF.Where(x => x.BasicProductId == "TL" && x.TransType == "P").Sum(x => x.Amount);
                    var PremiTL = premiTLPlusRefundTL == 0 ? "-" : string.Format("{0:N}", premiTLPlusRefundTL);
                    var koreksiPremiDec = itemMemberPCF.Where(x => x.TransType == "R").Sum(x => x.Amount);
                    var koreksiPremi = koreksiPremiDec == 0 ? "-" : string.Format("{0:N}", itemMemberPCF.Where(x => x.TransType == "R").Sum(x => x.Amount)); ;

                    var RefundPremi = "-";

                    if (recordMode == RecordModeMemberMovement.TerminateMember)
                    {
                        RefundPremi = itemMemberPCF.Where(x => x.TransType == "R").Sum(x => x.Amount) == 0 ? "-" : string.Format("{0:N}", itemMemberPCF.Where(x => x.TransType == "R").Sum(x => x.Amount)); ;
                        koreksiPremi = "-";
                    }
                    var listParams = new List<SqlParameter>();
                    listParams.Add(new SqlParameter("memberId", memberMovement.MemberId));
                    listParams.Add(new SqlParameter("startDate", startDate));
                    listParams.Add(new SqlParameter("endDate", endDate));

                    //var koreksiPremiMember = db.Database.SqlQuery<decimal?>("exec [ProcedureGetMemberCorrectionPremi] @memberId, @startDate, @endDate", new SqlParameter("memberId", memberMovement.MemberId), new SqlParameter("startDate", startDate), new SqlParameter("endDate", endDate)).FirstOrDefault();
                    var totalTagihanPremi = premiIPPlusRefundIP + premiOPPlusRefundOP + premiDEPlusRefundDE + premiMAPlusRefundMA + premiTLPlusRefundTL + koreksiPremiDec;
                    var TotalTagihan = totalTagihanPremi == 0 ? "-" : string.Format("{0:N}", itemMemberPCF.Sum(x => x.Amount));


                    sumPremiIP = sumPremiIP + premiIPPlusRefundIP;
                    sumPremiOP = sumPremiOP + premiOPPlusRefundOP;
                    sumPremiDE = sumPremiDE + premiDEPlusRefundDE;
                    sumPremiMA = sumPremiMA + premiMAPlusRefundMA;
                    sumPremiTL = sumPremiTL + premiTLPlusRefundTL;
                    sumPremiCorrection = sumPremiCorrection + koreksiPremiDec;
                    sumPremiRefund = sumPremiRefund + 0;


                    var newMRM = new MemberReportModel()
                    {

                        Address = memberMovement.Member.Client.Client3?.FullName ?? memberMovement.Member.Client.FullName,
                        BankAccountName = memberMovement.Member.Client.BankAccountName,
                        MemberNumber = memberMovement.Member.MemberNumber,
                        ClientId = memberMovement.ClientId,
                        MemberName = memberMovement.Member.Client.FullName,
                        PlanId = planId,
                        MembersExpirityDate = memberMovement.Member.EndDate?.ToString("dd/MM/yyyy"),
                        MembersEffectiveDate = memberMovement.EffectiveDate.ToString("dd/MM/yyyy"),
                        AdmedikaCode = memberMovement.AdmedikaCode.ToString(),
                        MemberId = memberMovement.MemberId.ToString(),
                        PolicyNumber = memberMovement.Member.Policy.PolicyNumber,

                        MaritalStatus = memberMovement.Member.Client.MaritalStatus != null ? (memberMovement.Member.Client.MaritalStatus == "Single" ? "S" : memberMovement.Member.Client.MaritalStatus == "Married" ? "M" : "") : "",
                        MapingID = memberMovement.Member.Policy.Member.Where(x => x.ClientId == memberMovement.Member.Client.RelateTo).FirstOrDefault()?.MemberNumber,
                        RecordMode = memberMovement.RecordMode.ToString(),
                        RecordType = memberMovement.Member.Client.ClientRelation != null ? "D" : "P",
                        PayorID = memberMovement.Member.Policy.Client.FullName,
                        CorporateID = memberMovement.Member.Policy.Client.IdNumber,
                        // untuk sementara dikasih nilai 0
                        TypeOfWork = 0.ToString(),

                        Relationship = memberMovement.Member.Client.ClientRelation != null ? (memberMovement.Member.Client.ClientRelation == "Son" ? "S" : memberMovement.Member.Client.ClientRelation == "Daughter" ? "D" : memberMovement.Member.Client.ClientRelation == "Wife" ? "W" : "") : "",
                        MobilePhone = memberMovement.Member.Client.MobilePhone1,
                        NRIC = memberMovement.Member.Client.BankAccountCode,
                        PassportNo = memberMovement.Member.Client.BankAccountNumber.ToString(),
                        PassportCountry = memberMovement.Member.Policy.Client.FullName,
                        DateOfBirth = memberMovement.Member.Client.BirthDate.Value.ToString("dd/MM/yyyy"),
                        Sex = memberMovement.Member.Client.Sex != null ? (memberMovement.Member.Client.Sex == "Female" ? "F" : memberMovement.Member.Client.Sex == "Male" ? "M" : "") : "",
                        Remarks = memberMovement.Remarks,
                        TanggalProses = memberMovement.ProcessDate.ToString(),
                        TanggalProsesSebelumnya = memberMovement.ProcessDate.ToString(),
                        TanggalProsesSebelumnya2 = memberMovement.ProcessDate.ToString(),
                        PremiIP = PremiIP,
                        PremiOP = PremiOP,
                        PremiDE = PremiDE,
                        PremiMA = PremiMA,
                        PremiTL = PremiTL,
                        NoPolisInduk = memberMovement.Member.Policy.PolicyNumber,
                        NamaPemegangPolis = memberMovement.Member.Policy.Client.FullName,
                        KoreksiPremi = koreksiPremi,
                        RefundPremi = RefundPremi,
                        TotalTagihanPremi = TotalTagihan,
                        Keterangan = keterangan

                    };

                    listOfReportModel.Add(newMRM);
                    //}
                    //else
                    //{

                    //}

                }
                catch (Exception e)
                {
                    WarningMessagesAdd(e.MessageToList());
                    WarningMessagesAdd(memberMovement.Member.MemberId.ToString());
                }

            }



            ReportDocument rd = new ReportDocument();
            ReportDocument rd2 = new ReportDocument();
            ReportDocument rd3 = new ReportDocument();
            var ds = new List<object>();
            var ds2 = new List<object>();

            foreach (var item in listOfReportModel.OrderBy(x => x.MemberNumber))
            {
                ds.Add(new
                {
                    item.Address,
                    item.BankAccountName,
                    item.MemberNumber,
                    item.ClientId,
                    item.MemberName,
                    item.PlanId,
                    item.MembersExpirityDate,
                    item.MembersEffectiveDate,
                    item.AdmedikaCode,
                    item.MemberId,
                    item.PolicyNumber,
                    item.MaritalStatus,
                    item.MapingID,
                    item.RecordMode,
                    item.RecordType,
                    item.PayorID,
                    item.CorporateID,
                    // untuk sementara dikasih nilai 
                    item.TypeOfWork,
                    item.Relationship,
                    item.MobilePhone,
                    item.NRIC,
                    item.PassportNo,
                    item.PassportCountry,
                    item.DateOfBirth,
                    item.Sex,
                    item.Remarks,
                    item.TanggalProses,
                    item.TanggalProsesSebelumnya,
                    item.TanggalProsesSebelumnya2,
                    item.PremiIP,
                    item.PremiOP,
                    item.PremiDE,
                    item.PremiTL,
                    item.PremiMA,
                    item.TotalTagihanPremi,
                    item.KoreksiPremi,
                    item.RefundPremi,
                    item.NoPolisInduk,
                    item.NamaPemegangPolis,
                    item.Keterangan
                });
            }


            try
            {

                rd.Load(Path.Combine(Server.MapPath("~/Reports/PolicyMemberDetailsBenefitPart1.rpt")));
                rd2.Load(Path.Combine(Server.MapPath("~/Reports/PolicyMemberDetailsBenefitPart2.rpt")));
                rd3.Load(Path.Combine(Server.MapPath("~/Reports/PolicyMemberDetailsBenefitSummary.rpt")));

                rd.Database.Tables[0].SetDataSource(ds);
                rd2.Database.Tables[0].SetDataSource(ds);
                //rd2.Database.Tables[1].SetDataSource(ds2);
                rd3.Database.Tables[0].SetDataSource(ds);

                //var transactionHeader = db.FinanceTransaction.Where(z => z.PolicyId == policyId && z.EffectiveDate >= startDate && z.EffectiveDate <= endDate).ToList();

                //var transactionDetail = from financeTransaction in transactionHeader
                //                        join financeTransactionDetail in db.FinanceTransactionDetail
                //                        on financeTransaction.TransactionNumber equals financeTransactionDetail.TransactionNumber
                //                        where financeTransaction.EffectiveDate >= startDate && financeTransaction.EffectiveDate <= endDate &&
                //                        financeTransaction.RecordMode == FinanceTransactionRecordMode._1
                //                        select financeTransactionDetail;

                //var transactionDetailRefundPremi = from financeTransaction in transactionHeader
                //                                   join financeTransactionDetail in db.FinanceTransactionDetail
                //                                   on financeTransaction.TransactionNumber equals financeTransactionDetail.TransactionNumber
                //                                   where financeTransaction.EffectiveDate >= startDate && financeTransaction.EffectiveDate <= endDate &&
                //                                   financeTransaction.RecordMode == FinanceTransactionRecordMode._3
                //                                   select financeTransactionDetail;

                //var transactionDetailpremiKoreksiPremi = from financeTransaction in transactionHeader
                //                                         join financeTransactionDetail in db.FinanceTransactionDetail
                //                                         on financeTransaction.TransactionNumber equals financeTransactionDetail.TransactionNumber
                //                                         where financeTransaction.EffectiveDate >= startDate && financeTransaction.EffectiveDate <= endDate &&
                //                                         financeTransaction.RecordMode == FinanceTransactionRecordMode._8
                //                                         select financeTransactionDetail;



                //var premiRawatInap = transactionDetail.Where(x => x.BasicProductId == "IP").Select(x => x.TransactionAmount).Sum() ?? 0;
                //var premiRawatJalan = transactionDetail.Where(x => x.BasicProductId == "OP").Select(x => x.TransactionAmount).Sum() ?? 0;
                //var premiRawatGigi = transactionDetail.Where(x => x.BasicProductId == "DE").Select(x => x.TransactionAmount).Sum() ?? 0;
                //var premiMaternity = transactionDetail.Where(x => x.BasicProductId == "MA").Select(x => x.TransactionAmount).Sum() ?? 0;
                //var premiTermLife = transactionDetail.Where(x => x.BasicProductId == "TL").Select(x => x.TransactionAmount).Sum() ?? 0;
                //var premiKoreksiPremi = transactionDetailpremiKoreksiPremi.Select(x => x.TransactionAmount).Sum() ?? 0;
                //var premiRefundPremi = transactionDetailRefundPremi.Select(x => x.TransactionAmount).Sum() ?? 0;
                var totalPremi = sumPremiIP + sumPremiOP + sumPremiDE + sumPremiMA + sumPremiTL + sumPremiRefund + sumPremiCorrection;






                rd3.SetParameterValue("PremiRawatInap", string.Format("{0:N}", sumPremiIP));
                rd3.SetParameterValue("PremiRawatJalan", string.Format("{0:N}", sumPremiOP));
                rd3.SetParameterValue("PremiRawatGigi", string.Format("{0:N}", sumPremiDE));
                rd3.SetParameterValue("PremiMaternity", string.Format("{0:N}", sumPremiMA));
                rd3.SetParameterValue("PremiTermLife", string.Format("{0:N}", sumPremiTL));
                rd3.SetParameterValue("KoreksiPremi", string.Format("{0:N}", sumPremiCorrection));
                rd3.SetParameterValue("RefundPremi", string.Format("{0:N}", sumPremiRefund));
                rd3.SetParameterValue("TotalPremi", string.Format("{0:N}", totalPremi));
                rd3.SetParameterValue("Terbilang", (totalPremi.Value < 0 ? "Minus" + WebAppUtility.Terbilang(totalPremi.Value * -1) : WebAppUtility.Terbilang(totalPremi.Value)) + " rupiah");
                var disetujuiOleh = db.CommonListValue.Where(x => x.Text == "Disetujui Oleh").FirstOrDefault();
                rd3.SetParameterValue("DisetujuiOleh", disetujuiOleh?.Value ?? "");
                var disiapkanOleh = db.CommonListValue.Where(x => x.Text == "Disiapkan Oleh").FirstOrDefault();

                rd3.SetParameterValue("DisiapkanOleh", disiapkanOleh?.Value ?? "");

                rd.SetParameterValue("POLIS No", policy.PolicyNumber);
                rd.SetParameterValue("NAMA PEMEGANG POLIS", policy.Client.FullName);
                rd.SetParameterValue("TANGGAL_PROSES", startDate.ToStringIndonesia("dd MMMM yyyy") + " - " + endDate.ToStringIndonesia("dd MMMM yyyy"));
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                //Stream streamReport = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                //streamReport.Seek(0, SeekOrigin.Begin);

                //var file = File(streamReport, "application/pdf");
                //streamReport.Flush();
                //return file;
                using (Stream streamReport = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat))
                using (Stream streamReport2 = rd2.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat))
                using (Stream streamReport3 = rd3.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat))
                using (PdfDocument one = PdfReader.Open(streamReport, PdfDocumentOpenMode.Import))
                using (PdfDocument two = PdfReader.Open(streamReport2, PdfDocumentOpenMode.Import))
                using (PdfDocument three = PdfReader.Open(streamReport3, PdfDocumentOpenMode.Import))
                using (PdfDocument outPdf = new PdfDocument())
                {
                    //streamReport.Seek(0, SeekOrigin.Begin);
                    //three.Pages[0].Rotate = 270;
                    WebAppUtility.CopyPages(three, outPdf);
                    WebAppUtility.CopyPages(one, outPdf);
                    WebAppUtility.CopyPages(two, outPdf);
                    //outPdf.Save("file1and2.pdf");

                    MemoryStream ms = new MemoryStream();
                    var fileName = "Invoice_" + policy.PolicyNumber + "_" + policy.Client.FullName + "_" + startDate.ToString("dd/MM/yyyy") + "_" + endDate.ToString("dd/MM/yyyy");

                    outPdf.Save(ms, false);
                    byte[] buffer = new byte[ms.Length];
                    ms.Seek(0, SeekOrigin.Begin);
                    var retval = File(ms, "application/pdf");

                    ms.Flush();
                    return retval;
                }
            }
            catch (Exception e)
            {
                WarningMessagesAdd(e.MessageToList());
                return RedirectToAction("Index");
            }


        }

        public ActionResult Issue(string policyId)
        {

            if (policyId == null)
            {
                return HttpNotFound();

            }
            List<string> issueMessage = new List<string>();
            var policy = db.Policy.Find(policyId);
            var policyMember = policy.Member;

            if (policy == null)
            {
                return HttpNotFound();
            }
            if (policyMember.Count != policyMember.Where(x => x.MemberStatus == "Calculated").Count() || db.PCF.Where(x => x.PolicyId == policyId).GroupBy(x => x.MemberId).Count() != policy.Member.Count)
            {
                WarningMessagesAdd("Policy needed to be calculated first");
            }
            if (policyMember.Count == 0)
            {
                WarningMessagesAdd("No member found in the policy");
            }
            var paymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "PaymentFrequency").ToList();
            var openOrClose = db.CommonListValue.Where(x => x.CommonListValue2.Value == "OpenOrClose").ToList();

            policy.PaymentFrequency = paymentFrequency.Where(x => x.Value == policy.PaymentFrequency).FirstOrDefault()?.Text;
            policy.OpenOrClose = openOrClose.Where(x => x.Value == policy.OpenOrClose).FirstOrDefault()?.Text;
            if (Request.IsAjaxRequest())
            {
                //ViewBag.IssueMessage = issueMessage;
                if (WarningMessages().Count == 0)
                {
                    InfoMessagesAdd("Policy is ready to issue");
                }
                var modalView = new ModalView()
                {
                    Title = "Issue Policy",
                    Body = this.RenderRazorViewToString("Issue", policy),
                    ModalForm = new ModalForm()
                    {
                        ActionName = "Issue/" + policy.PolicyId,
                        ControllerName = "Policy",
                        RouteValues = new { policyId = policy.PolicyId }
                    }
                };
                if (WarningMessages().Count == 0)
                {
                    modalView.Footer = this.GetHtmlHelper().TextBox("Issue", "Issue", null, new { @class = "btn btn-primary", @style = "background-color:#008CBA; color:white;", @type = "submit" }).ToString();
                }
                return View("_Modal", modalView);
                //return View("Issue", policy);
            }
            return View(policy);
        }

        [HttpPost, ActionName("Issue")]
        public ActionResult IssueConfirm(string policyId)
        {

            if (policyId == null)
            {
                return HttpNotFound();

            }

            var policy = db.Policy.Find(policyId);
            if (policy == null)
            {
                return HttpNotFound();
            }
            var memberPolicy = policy.Member.ToList();
            if (memberPolicy.Count != memberPolicy.Where(x => x.MemberStatus == "Calculated").Count())
            {
                WarningMessagesAdd("Not All Member have been calculated");
            }
            if (memberPolicy.Count == 0)
            {
                WarningMessagesAdd("No member found in the policy");
            }
            if (WarningMessages().Count == 0)
            {
                using (var dbTransaction = db.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
                {

                    try
                    {
                        var lastPolicyNumberSequential = db.AspSequential.Where(x => x.Name == AspSequentialName.PolicyNumber).FirstOrDefault();
                        var currentDateYear = DateTime.Now.Year.ToString();
                        var currentDateMonth = DateTime.Now.Month.ToString().PadLeft(2, '0');
                        var lastPolicyNumberPaddedLeft = (lastPolicyNumberSequential?.LastSequential + 1).Value.ToString().PadLeft(6, '0');
                        policy.PolicyNumber = currentDateYear + currentDateMonth + lastPolicyNumberPaddedLeft;
                        policy.IssueDate = DateTime.Now;
                        db.Entry(policy).State = System.Data.Entity.EntityState.Modified;
                        lastPolicyNumberSequential.LastSequential = lastPolicyNumberSequential.LastSequential + 1;
                        db.Entry(lastPolicyNumberSequential).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        foreach (var item in memberPolicy.Where(x => x.Client.RelateTo == null))
                        {
                            var sequencialMemberNumber = db.AspSequential.Where(x => x.Name == AspSequentialName.MemberNumber).FirstOrDefault();
                            item.MemberNumber = (sequencialMemberNumber.LastSequential + 1).ToString().PadLeft(7, '0');
                            item.SetPropertyUpdate();
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            sequencialMemberNumber.LastSequential = sequencialMemberNumber.LastSequential + 1;
                            sequencialMemberNumber.SetPropertyUpdate();
                            db.Entry(sequencialMemberNumber).State = System.Data.Entity.EntityState.Modified;

                            db.SaveChanges();

                        }
                        foreach (var item in memberPolicy.Where(x => x.Client.RelateTo != null))
                        {
                            //var sequencialMemberNumber = db.AspSequential.Where(x => x.Name == "MemberNumber").FirstOrDefault();
                            var memberRelateTo = memberPolicy.Where(x => x.ClientId == item.Client.RelateTo).FirstOrDefault();
                            if (memberRelateTo == null)
                            {
                                WarningMessagesAdd("Member Related With " + item.Client.FullName + " Is Not Found");
                            }
                            item.MemberNumber = memberRelateTo.MemberNumber + "-" + item.SequencialNo;
                            item.SetPropertyUpdate();
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            //sequencialMemberNumber.LastSequential = sequencialMemberNumber.LastSequential + 1;
                            //db.Entry(sequencialMemberNumber).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        // Update 17 April 2020
                        //var allPolicyPCF = db.PCF.Where(x => x.PolicyId == policyId && x.InvoiceDate < DateTime.Now);
                        var allPolicyPCF = policy.PCF.Where(x => x.TransactionNumber == null).ToList();
                        var financeTransaction = new FinanceTransaction();
                        var transactionNumberSeq = db.AspSequential.Where(x => x.Name == AspSequentialName.TransactionNumber).FirstOrDefault();
                        financeTransaction.RecordMode = 1;
                        financeTransaction.TransactionNumber = "TXTR-" + DateTime.Now.Year + "-" + (transactionNumberSeq.LastSequential + 1).ToString().PadLeft(6, '0');
                        transactionNumberSeq.LastSequential = transactionNumberSeq.LastSequential + 1;
                        transactionNumberSeq.SetPropertyUpdate();
                        db.SaveChanges();
                        db.Entry(transactionNumberSeq).State = System.Data.Entity.EntityState.Modified;
                        //financeTransaction.TransactionBranch
                        financeTransaction.EffectiveDate = policy.IssueDate;
                        financeTransaction.TransactionDate = allPolicyPCF.Min(x => x.InvoiceDate);
                        financeTransaction.DueDate = financeTransaction.EffectiveDate.Value.AddDays(30);
                        financeTransaction.TransCodeId = "Invoice";
                        //financeTransaction.DCN_Number
                        //financeTransaction.ProcessDate
                        //financeTransaction.ReferenceNumber
                        financeTransaction.PolicyId = policy.PolicyId;
                        financeTransaction.PolicyNumber = policy.PolicyNumber;
                        //financeTransaction.ReconDate
                        //financeTransaction.ReconNumber
                        financeTransaction.ReconStatus = "No";
                        financeTransaction.OutstandingAmount = allPolicyPCF.Sum(x => x.Amount);
                        financeTransaction.ClientId = policy.ClientId;
                        //financeTransaction.BankId
                        //financeTransaction.ClientAccountNumber
                        //financeTransaction.ClientAccountName
                        //financeTransaction.ClientCurrency
                        //financeTransaction.ClientTransExchangeRate
                        financeTransaction.ClientTransactionAmount = financeTransaction.OutstandingAmount;
                        financeTransaction.ClosingAgen = policy.Agent;
                        //financeTransaction.DebitCOA
                        //financeTransaction.ReconCOA 
                        financeTransaction.TransDescription = "Invoice Policy " + policy.PolicyNumber + " Periode " + financeTransaction.TransactionDate.Value.ToShortDateString();
                        //financeTransaction
                        //financeTransaction
                        //financeTransaction
                        //financeTransaction
                        //financeTransaction
                        //financeTransaction
                        db.FinanceTransaction.Add(financeTransaction);

                        db.SaveChanges();

                        foreach (var item in allPolicyPCF.GroupBy(x => x.BasicProductId))
                        {

                            var newFinanceTransactionDetail = new FinanceTransactionDetail();
                            newFinanceTransactionDetail.TransactionNumber = financeTransaction.TransactionNumber;
                            newFinanceTransactionDetail.OutstandingAmount = item.Sum(x => x.Amount);
                            newFinanceTransactionDetail.BasicProductId = item.FirstOrDefault().BasicProductId;
                            newFinanceTransactionDetail.TransactionAmount = newFinanceTransactionDetail.OutstandingAmount;
                            //newFinanceTransactionDetail.BankAmount
                            db.FinanceTransactionDetail.Add(newFinanceTransactionDetail);

                            db.SaveChanges();
                        }
                        foreach (var item in allPolicyPCF)
                        {

                            item.TransactionNumber = financeTransaction.TransactionNumber;
                            item.SetPropertyUpdate();
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }


                        policy = db.Policy.Find(policyId);
                        foreach (var item in policy.Member)
                        {
                            item.MemberStatus = MemberStatus.Active;
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        }
                        db.SaveChanges();

                        foreach (var item in policy.Member)
                        {
                            var newMemberMovement = new Member_Movement();
                            newMemberMovement.AdmedikaCode = item.AdmedikaCode;
                            newMemberMovement.Age = item.Age;
                            newMemberMovement.CardNumber = item.CardNumber;
                            newMemberMovement.ClaimNumber = item.ClaimNumber;
                            newMemberMovement.ClientId = item.ClientId;
                            newMemberMovement.EffectiveDate = DateTime.Now.Date;
                            newMemberMovement.EndDate = item.EndDate;
                            newMemberMovement.EndorseNumber = item.EndorseNumber;
                            newMemberMovement.EntryDate = item.EntryDate;
                            newMemberMovement.ExitDate = item.ExitDate;
                            newMemberMovement.IssueDate = item.IssueDate;
                            newMemberMovement.LastClaimDate = item.LastClaimDate;
                            newMemberMovement.LastEndorseDate = item.LastEndorseDate;
                            newMemberMovement.MemberId = item.MemberId;

                            newMemberMovement.MemberNumber = item.MemberNumber;
                            newMemberMovement.MemberStatus = item.MemberStatus;
                            newMemberMovement.PlanId = item.PlanId;
                            newMemberMovement.PolicyId = item.PolicyId;
                            newMemberMovement.ProcessDate = item.ProcessDate;
                            newMemberMovement.RecordMode = 1;
                            //newMemberMovement.Remarks
                            newMemberMovement.SequencialNo = item.SequencialNo;
                            newMemberMovement.StartDate = item.StartDate;
                            newMemberMovement.TerminateDate = item.TerminateDate;
                            newMemberMovement.LimitCode = item.MemberPlan.Where(x => x.PlanId == item.PlanId)?.FirstOrDefault().BasicProductLimitCode;
                            newMemberMovement.SetPropertyCreate();
                            db.Member_Movement.Add(newMemberMovement);
                            db.SaveChanges();

                            var newMemberMovementClient = new Member_Movement_Client();
                            newMemberMovementClient.MovementId = newMemberMovement.Id;
                            newMemberMovementClient.ClientId = item.Client.ClientId;
                            newMemberMovementClient.Type = item.Client.Type;
                            newMemberMovementClient.BranchCode = item.Client.BranchCode;
                            newMemberMovementClient.ContactPerson = item.Client.ContactPerson;
                            newMemberMovementClient.ShortName = item.Client.ShortName;
                            newMemberMovementClient.FullName = item.Client.FullName;
                            newMemberMovementClient.PrefixClientTitle = item.Client.PrefixClientTitle;
                            newMemberMovementClient.EndfixClientTitle = item.Client.EndfixClientTitle;
                            newMemberMovementClient.BirthDate = item.Client.BirthDate;
                            newMemberMovementClient.BirthPlace = item.Client.BirthPlace;
                            newMemberMovementClient.IdNumber = item.Client.IdNumber;
                            newMemberMovementClient.MaritalStatus = item.Client.MaritalStatus;
                            newMemberMovementClient.Sex = item.Client.Sex;
                            newMemberMovementClient.Email = item.Client.Email;
                            newMemberMovementClient.EmailAddress1 = item.Client.EmailAddress1;
                            newMemberMovementClient.EmailAddress2 = item.Client.EmailAddress2;
                            newMemberMovementClient.MobilePhone1 = item.Client.MobilePhone1;
                            newMemberMovementClient.MObilePhone2 = item.Client.MObilePhone2;
                            newMemberMovementClient.MobilePhone3 = item.Client.MobilePhone3;
                            newMemberMovementClient.ClientRelation = item.Client.ClientRelation;
                            newMemberMovementClient.RelateTo = item.Client.RelateTo;
                            newMemberMovementClient.BankAccountNumber = item.Client.BankAccountNumber;
                            newMemberMovementClient.BankAccountCode = item.Client.BankAccountCode;
                            newMemberMovementClient.BankAccountName = item.Client.BankAccountName;
                            newMemberMovementClient.Status = item.Client.Status;
                            newMemberMovementClient.Address = item.Client.Address;
                            newMemberMovementClient.SetPropertyCreate();

                            db.Member_Movement_Client.Add(newMemberMovementClient);
                            db.SaveChanges();
                        }

                        policy.PolicyStatus = PolicyStatus.Active;
                        db.Entry(policy).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        dbTransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        WarningMessagesAdd(e.MessageToList());
                    }
                }

            }
            if (policyId == null)
            {
                return HttpNotFound();

            }
            List<string> issueMessage = new List<string>();
            var policyMember = policy.Member;

            if (policy == null)
            {
                return HttpNotFound();
            }

            if (memberPolicy.Count == 0)
            {
                WarningMessagesAdd("No member found in the policy");
            }

            if (WarningMessages().Count == 0)
            {
                SuccessMessagesAdd("Issue Process Is Success.");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DeleteAllMember(string policyId)
        {
            if (policyId == null)
            {
                return new HttpNotFoundResult();
            }
            var policy = db.Policy.Find(policyId);
            if (policy == null)
            {
                return new HttpNotFoundResult();
            }
            if (policy.PolicyStatus == "Issued" || policy.IssueDate != null)
            {
                WarningMessagesAdd("Policy Have Been Issued");
            }
            return View("_Modal", new ModalView()
            {
                Title = "Delete All Member Policy",
                Body = this.RenderRazorViewToString("_DeleteAllMember", policy),
                Footer = this.GetHtmlHelper().TextBox("Delete All", "Delete All", null, new { @class = "btn btn-primary", @type = "submit" }).ToString(),
                ModalForm = new ModalForm { ActionName = "DeleteAllMember/" + policyId, ControllerName = "Policy", RouteValues = new { policyId } }
            });
        }

        [HttpPost, ActionName("DeleteAllMember")]
        public ActionResult DeleteAllMemberConfirm(string policyId)
        {
            if (policyId == null)
            {
                return new HttpNotFoundResult();
            }
            var policy = db.Policy.Find(policyId);
            if (policy == null)
            {
                return new HttpNotFoundResult();
            }
            if (policy.PolicyStatus == "Issued" || policy.IssueDate != null)
            {
                WarningMessagesAdd("Policy Have Been Issued");
            }

            if (WarningMessages().Count == 0)
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {

                        var allMember = db.Member.Where(x => x.PolicyId == policyId).ToList();
                        foreach (var item in allMember)
                        {
                            db.PCF.RemoveRange(item.PCF.ToList());
                            db.MemberPlan.RemoveRange(item.MemberPlan.ToList());
                        }
                        db.Member.RemoveRange(allMember);
                        db.SaveChanges();
                        dbTransaction.Commit();
                        SuccessMessagesAdd(Message.DeleteSuccess);
                        return RedirectToAction("Details", new { id = policyId, tab = "member" });

                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        WarningMessagesAdd(e.MessageToList());
                    }
                }

            }
            return View(policy);
        }
        public ActionResult PrintPolicy(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var policy = db.Policy.Find(id);
            if (policy == null)
            {
                return HttpNotFound();
            }

            try
            {
                ReportDocument printPolicy = new ReportDocument();
                printPolicy.Load(Path.Combine(Server.MapPath("~/Reports/PrintPolicy.rpt")));

                //rd.Database.Tables[0].SetDataSource(ds);
                printPolicy.SetParameterValue("PolicyHolderName", policy.Client.FullName);
                //rd.SetParameterValue("PolicyHolderName", policy.Client.FullName  +" "+ policy.Client.FullName);
                printPolicy.SetParameterValue("PolicyNumber", policy.PolicyNumber);
                printPolicy.SetParameterValue("PolicyHolderAddress", policy.Client.Address);
                printPolicy.SetParameterValue("PlaceAndDateCOO", "Jakarta, " + DateTime.Now.ToString("d MMM yyyy"));
                printPolicy.SetParameterValue("InsuranceCompanyName", "PT Asuransi Jiwa Indosurya Sukses");
                var disetujuiOleh = db.CommonListValue.Where(x => x.Text == "Disetujui Oleh").FirstOrDefault();
                printPolicy.SetParameterValue("COOName", disetujuiOleh?.Value ?? "");

                ReportDocument printPolicySummary = new ReportDocument();
                printPolicySummary.Load(Path.Combine(Server.MapPath("~/Reports/PrintPolicySummary.rpt")));

                string paymentFrequencyWord = "";
                if (policy.PaymentFrequency == "M")
                {
                    paymentFrequencyWord = "Bulanan";
                }
                else if (policy.PaymentFrequency == "Q")
                {
                    paymentFrequencyWord = "Triwulan";
                }
                else if (policy.PaymentFrequency == "S")
                {
                    paymentFrequencyWord = "Semesteran";
                }
                else if (policy.PaymentFrequency == "Y")
                {
                    paymentFrequencyWord = "Tahunan";
                }


                printPolicySummary.SetParameterValue("PolicyHolderName", policy.Client.FullName);
                printPolicySummary.SetParameterValue("PolicyNumber", policy.PolicyNumber);
                printPolicySummary.SetParameterValue("CaraBayar", paymentFrequencyWord);

                var benefit = policy.MemberPlan.Select(x => x.BasicProduct.BasicProductName).Distinct();
                var benefitString = "";
                foreach (var item in benefit)
                {
                    benefitString = benefitString + item + "\r\n";
                }
                printPolicySummary.SetParameterValue("ListBenefit", benefitString);
                printPolicySummary.SetParameterValue("PolicyEfectiveDate", policy.StartDate.Value.ToString("d MMM yyyy") + " - " + policy.MatureDate.Value.ToString("d MMM yyyy"));


                using (Stream streamReportPrintPolicy = printPolicy.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat))
                using (Stream streamReportPrintPolicySummary = printPolicySummary.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat))
                using (PdfDocument one = PdfReader.Open(streamReportPrintPolicy, PdfDocumentOpenMode.Import))
                using (PdfDocument two = PdfReader.Open(streamReportPrintPolicySummary, PdfDocumentOpenMode.Import))
                using (PdfDocument outPdf = new PdfDocument())
                {
                    WebAppUtility.CopyPages(one, outPdf);
                    WebAppUtility.CopyPages(two, outPdf);

                    MemoryStream ms = new MemoryStream();

                    outPdf.Save(ms, false);
                    byte[] buffer = new byte[ms.Length];
                    ms.Seek(0, SeekOrigin.Begin);
                    var retval = File(ms, "application/pdf");

                    ms.Flush();
                    return retval;
                }
            }
            catch (Exception e)
            {
                WarningMessagesAdd(e.MessageToList());
                return RedirectToAction("Index");
            }

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
