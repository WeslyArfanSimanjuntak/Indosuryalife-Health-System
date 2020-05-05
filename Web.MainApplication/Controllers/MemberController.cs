using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Repository.Application.DataModel;
using EntityState = System.Data.Entity.EntityState;
using System.IO;
using OfficeOpenXml;
using CrystalDecisions.CrystalReports.Engine;
using System.Linq.Dynamic;
using Web.MainApplication.Models.UploadModel;
using Web.MainApplication.ReportModel;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Web.MainApplication.Controllers
{
    public class MemberController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: Member
        public ActionResult Index()
        {
            var member = db.Member.Include(m => m.Client).Include(m => m.Policy);
            return View(member.ToList());
        }

        // GET: Member/Details/5
        public ActionResult Details(long? id)
        {
            //var BasicProductLimitCode = db.PCF.Where(x => x.BasicProductId == bpId && x.MemberId == memberId).ToList();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Member member = db.Member.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            if (Request.IsAjaxRequest())
            {
                ViewBag.LayoutIsNull = true;
                return View("_DetailsMember", member);
            }
            member.Policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault() != null
                ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault().Text : member.Policy.PaymentFrequency;
            ViewBag.AdministrationFee = member.AdministrationFee.ToList();
            return View(member);
        }

        // GET: Member/MemberBenefits/5
        public ActionResult MemberBenefits(long? id, string bpId, string bpCode)
        {
            var selectedBenefit = db.BasicProductLimit.Where(x => x.BasicProductId == bpId && x.BasicProductLimitCode == bpCode).ToList();
            var allActiveBenefit = db.Benefit.Where(x => x.IsActive == 1).ToList();
            var tempBenefitList = new List<Benefit>();

            foreach (var item in allActiveBenefit)
            {
                var newBenefit = new Benefit
                {
                    BenefitCode = item.BenefitCode,
                    BenefitName = item.BenefitName
                };
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


            if (id == null || bpId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Member.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            //ViewData["Benefit"] = benefitData;
            member.Policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault() != null
                ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault().Text : member.Policy.PaymentFrequency;
            return View(member);
        }

        //public IQueryable<BasicProductLimit> FindAllBenefits(string bpId, string bpCode)
        //{
        //    return from n in db.BasicProductLimit
        //           where n.BasicProductId == bpId 
        //           where n.BasicProductLimitCode == bpCode
        //           join c in db.Benefit on n.BenefitCode equals c.BenefitCode
        //           orderby n.id descending
        //           select n;
        //}


        // GET: Member/Create
        public ActionResult Create(string policyId)
        {

            var policy = db.Policy.Find(policyId);
            if (policy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var dropdownPlan = new List<SelectListItem>();
            dropdownPlan.AddBlank();
            db.Plan.Where(x => x.PolicyId == policy.PolicyId).ToList().ForEach(x =>
               {
                   dropdownPlan.AddItemValText(x.PlanId, x.PlanId + " - " + x.PlanName);
               });
            ViewBag.PlanId = dropdownPlan.ToSelectList();

            var dropdownAgent = new List<SelectListItem>();
            dropdownAgent.AddBlank();
            db.Plan.Where(x => x.PolicyId == policy.PolicyId).ToList().ForEach(x =>
            {
                dropdownAgent.AddItemValText(x.Policy.Agent, x.Policy.Agent);
            });
            ViewBag.Agent = dropdownAgent.ToSelectList();

            var member = new Member
            {
                Policy = policy,
                PolicyId = policy.PolicyId
            };

            member.Policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault() != null
                ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault().Text : member.Policy.PaymentFrequency;

            ViewBag.ClientId = new SelectList(db.Client, "ClientId", "Type");
            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber");
            member.MemberNumber = "Auto Generated";
            member.MemberStatus = "Inactive";
            return View(member);


        }

        // POST: Member/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,PolicyId,MemberId,ClientId,CardNumber,PlanId,SequencialNo,StartDate,EndDate,LastEndorseDate,LastClaimDate,ExitDate,TerminateDate,MemberStatus,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] Member member)
        {
            if (ModelState.IsValid)
            {
                var clientToAdd = db.Client.Find(member.ClientId);
                var memberPolicy = db.Policy.Find(member.PolicyId);
                if (clientToAdd != null)
                {
                    if (clientToAdd.Type == "Company")
                    {
                        WarningMessagesAdd("Company can not added as policy member.");

                    }

                }
                if (db.Member.Where(x => x.ClientId == member.ClientId && x.PolicyId != member.PolicyId).FirstOrDefault() != null)
                {
                    WarningMessagesAdd("Member already registered in another policy.");

                }
                if (db.Member.Where(x => x.ClientId == member.ClientId).FirstOrDefault() != null)
                {
                    WarningMessagesAdd("Member already registered.");

                }
                if (member.StartDate == null)
                {
                    WarningMessagesAdd("Member Startdate can not be null");
                }
                if (member.EndDate == null)
                {
                    WarningMessagesAdd("Member Startdate can not be null");
                }
                if (member.StartDate < memberPolicy.StartDate || member.StartDate > memberPolicy.MatureDate)
                {
                    WarningMessagesAdd("Start Date can not be bigger than Policy Mature Date or lower than Policy Start Date.");
                }
                if (member.EndDate > memberPolicy.MatureDate || member.EndDate < memberPolicy.StartDate || member.EndDate < member.StartDate)
                {
                    WarningMessagesAdd("End Date can not be bigger than Policy Mature Date or lower than Policy Start Date or lower than Member Start Date.");
                }

                if (WarningMessages().Count == 0)
                {
                    try
                    {
                        using (var dbTransaction = db.Database.BeginTransaction())
                        {
                            try
                            {

                                //member.EndDate = memberPolicy.MatureDate;
                                var client = db.Client.Find(member.ClientId);
                                //member.CardNumber = "CardNumber_" + member.PolicyId + "_" + client.ClientId;
                                member.MemberStatus = "Inactive";
                                member.SetPropertyCreate();
                                db.Member.Add(member);
                                db.SaveChanges();
                                // set sequencial number
                                if (member.Client.RelateTo != null)
                                {
                                    var allMemberRelateToSameClient = db.Member.Where(x => x.Client.RelateTo == member.Client.RelateTo && x.MemberId != member.MemberId).ToList();
                                    member.SequencialNo = allMemberRelateToSameClient != null && allMemberRelateToSameClient.Count > 0 ? allMemberRelateToSameClient.Max(x => x.SequencialNo) + 1 : 1;
                                }
                                db.Entry(member).State = EntityState.Modified;
                                db.SaveChanges();

                                var planDetails = db.PlanDetail.Where(x => x.PolicyId == memberPolicy.PolicyId && x.PlanId == member.PlanId).ToList();
                                foreach (var item in planDetails)
                                {
                                    var newMemberPlan = new MemberPlan
                                    {
                                        PolicyId = member.PolicyId,
                                        PlanId = member.PlanId,
                                        MemberId = member.MemberId,
                                        BasicProductId = item.BasicProductId,
                                        BasicProductLimitCode = item.BasicProductLimitCode
                                    };
                                    newMemberPlan.SetPropertyCreate();
                                    db.MemberPlan.Add(newMemberPlan);
                                }
                                db.SaveChanges();
                                dbTransaction.Commit();
                                SuccessMessagesAdd("Inserting Data Success");
                                return RedirectToAction("Details", "Policy", new { id = member.PolicyId, tab = "member" });
                            }
                            catch (Exception ex)
                            {
                                dbTransaction.Rollback();
                                WarningMessagesAdd(ex.MessageToList());
                            }

                        }



                    }
                    catch (Exception e)
                    {
                        WarningMessagesAdd(e.MessageToList());
                    }
                }
            }

            var policy = db.Policy.Find(member.PolicyId);
            if (policy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var dropdownPlan = new List<SelectListItem>();
            dropdownPlan.AddBlank();
            db.Plan.Where(x => x.PolicyId == policy.PolicyId).ToList().ForEach(x =>
            {
                dropdownPlan.AddItemValText(x.PlanId, x.PlanId + " - " + x.PlanName);
            });
            member.Policy = policy;
            ViewBag.PlanId = dropdownPlan.ToSelectList(member.PlanId);
            ViewBag.ClientId = new SelectList(db.Client, "ClientId", "Type", member.ClientId);
            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber", member.PolicyId);

            member.Policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault() != null
               ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault().Text : member.Policy.PaymentFrequency;

            member.MemberNumber = "Auto Generated";
            member.MemberStatus = "Inactive";

            return View(member);
        }

        // GET: Member/EditMemberBenefits/5
        public ActionResult EditMemberBenefits(long? id, string bpId, string bpCode)
        {
            var selectedBenefit = db.BasicProductLimit.Where(x => x.BasicProductId == bpId && x.BasicProductLimitCode == bpCode).ToList();
            var allActiveBenefit = db.Benefit.Where(x => x.IsActive == 1).ToList();
            var tempBenefitList = new List<Benefit>();

            foreach (var item in allActiveBenefit)
            {

                var newBenefit = new Benefit
                {
                    BenefitCode = item.BenefitCode,
                    BenefitName = item.BenefitName
                };
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


            if (id == null || bpId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Member.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            //ViewData["Benefit"] = benefitData;
            member.Policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault() != null
                ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault().Text : member.Policy.PaymentFrequency;
            return View(member);
        }

        // GET: Member/Edit/5
        public ActionResult Edit(long? id, string PolicyId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Member.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }

            var policy = db.Policy.Find(member.PolicyId);
            var dropdownPlan = new List<SelectListItem>();
            dropdownPlan.AddBlank();
            db.Plan.Where(x => x.PolicyId == policy.PolicyId).ToList().ForEach(x =>
            {
                dropdownPlan.AddItemValText(x.PlanId, x.PlanId + " - " + x.PlanName);
            });
            member.Policy = policy;
            ViewBag.PlanId = dropdownPlan.ToSelectList(member.PlanId);
            ViewBag.ClientId = new SelectList(db.Client, "ClientId", "Type", member.ClientId);
            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber", member.PolicyId);

            member.Policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault() != null
               ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault().Text : member.Policy.PaymentFrequency;

            return View(member);
        }


        // POST: Member/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,PolicyId,MemberId,ClientId,CardNumber,PlanId,SequencialNo,StartDate,EndDate,LastEndorseDate,LastClaimDate,ExitDate,TerminateDate,MemberStatus,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] Member member)
        {
            if (ModelState.IsValid)
            {
                var clientToAdd = db.Client.Find(member.ClientId);
                var memberPolicy = db.Policy.Find(member.PolicyId);
                if (clientToAdd != null)
                {
                    if (clientToAdd.Type == "Company")
                    {
                        WarningMessagesAdd("Company can not added as policy member.");

                    }

                }

                if (db.Member.Where(x => x.ClientId == member.ClientId && x.PolicyId != memberPolicy.PolicyId).FirstOrDefault() != null)
                {
                    WarningMessagesAdd("Member already registered.");

                }
                if (member.StartDate < memberPolicy.StartDate || member.StartDate > memberPolicy.MatureDate)
                {
                    WarningMessagesAdd("Start Date can not be bigger than Policy Mature Date or lower than Policy Start Date.");
                }
                if (member.EndDate > memberPolicy.MatureDate || member.EndDate < memberPolicy.StartDate || member.EndDate < member.StartDate)
                {
                    WarningMessagesAdd("End Date can not be bigger than Policy Mature Date or lower than Policy Start Date or lower than Member Start Date.");
                }

                if (WarningMessages().Count == 0)
                {
                    try
                    {
                        using (var dbTransaction = db.Database.BeginTransaction())
                        {
                            try
                            {
                                member.MemberStatus = "Inactive";
                                member.SetPropertyUpdate();
                                db.Entry(member).State = EntityState.Modified;
                                db.SaveChanges();
                                var memberPlanToDelete = db.MemberPlan.Where(x => x.PolicyId == member.PolicyId && x.MemberId == member.MemberId);
                                db.MemberPlan.RemoveRange(memberPlanToDelete);
                                db.SaveChanges();

                                var memberBefore = db.Member.Find(member.MemberId);

                                if (memberBefore != null && member.PlanId != memberBefore.PlanId)
                                {
                                    var planDetails = db.PlanDetail.Where(x => x.PolicyId == memberPolicy.PolicyId && x.PlanId == member.PlanId).ToList();
                                    foreach (var item in planDetails)
                                    {
                                        //var memberPlanToDelete = db.MemberPlan.Where(x => x.PolicyId == member.PolicyId && x.MemberId
                                        //== member.MemberId && x.BasicProductId == item.BasicProductId && x.BasicProductLimitCode ==
                                        //item.BasicProductLimitCode).ToList();
                                        //db.MemberPlan.RemoveRange(memberPlanToDelete);

                                        var newMemberPlan = new MemberPlan
                                        {
                                            PolicyId = member.PolicyId,
                                            MemberId = member.MemberId,
                                            PlanId = member.PlanId,
                                            BasicProductId = item.BasicProductId,
                                            BasicProductLimitCode = item.BasicProductLimitCode
                                        };
                                        newMemberPlan.SetPropertyCreate();
                                        db.MemberPlan.Add(newMemberPlan);
                                    }
                                }
                                else
                                {
                                    var planDetails = db.PlanDetail.Where(x => x.PolicyId == memberPolicy.PolicyId && x.PlanId == member.PlanId).ToList();
                                    foreach (var item in planDetails)
                                    {
                                        //var memberPlanToDelete = db.MemberPlan.Where(x => x.PolicyId == member.PolicyId && x.MemberId
                                        //== member.MemberId && x.BasicProductId == item.BasicProductId && x.BasicProductLimitCode ==
                                        //item.BasicProductLimitCode).ToList();
                                        //db.MemberPlan.RemoveRange(memberPlanToDelete);

                                        var newMemberPlan = new MemberPlan
                                        {
                                            PolicyId = member.PolicyId,
                                            MemberId = member.MemberId,
                                            PlanId = member.PlanId,
                                            BasicProductId = item.BasicProductId,
                                            BasicProductLimitCode = item.BasicProductLimitCode
                                        };
                                        newMemberPlan.SetPropertyCreate();
                                        db.MemberPlan.Add(newMemberPlan);
                                    }
                                }
                                db.SaveChanges();
                                dbTransaction.Commit();
                                SuccessMessagesAdd("Editing Data Success");
                                return RedirectToAction("Details", "Policy", new { id = member.PolicyId, tab = "member" });
                            }
                            catch (Exception ex)
                            {
                                dbTransaction.Rollback();
                                WarningMessagesAdd(ex.MessageToList());
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        WarningMessagesAdd(e.MessageToList());
                    }
                }
            }

            var policy = db.Policy.Find(member.PolicyId);
            if (policy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var dropdownPlan = new List<SelectListItem>();
            dropdownPlan.AddBlank();
            db.Plan.Where(x => x.PolicyId == policy.PolicyId).ToList().ForEach(x =>
            {
                dropdownPlan.AddItemValText(x.PlanId, x.PlanId + " - " + x.PlanName);
            });
            member.Policy = policy;
            ViewBag.PlanId = dropdownPlan.ToSelectList(member.PlanId);
            ViewBag.ClientId = new SelectList(db.Client, "ClientId", "Type", member.ClientId);
            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber", member.PolicyId);

            member.Policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault() != null
               ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == member.Policy.PaymentFrequency).FirstOrDefault().Text : member.Policy.PaymentFrequency;

            member.MemberNumber = "Auto Generated";
            member.MemberStatus = "Inactive";
            member.Client = db.Client.Find(member.ClientId);
            return View(member);
        }

        // GET: Member/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Member.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            if (member.MemberStatus != "Inactive")
            {
                InfoMessagesAdd("Member with status \"" + member.MemberStatus + "\" can not be deleted.");
                return RedirectToAction("Details", "Policy", new { id = member.PolicyId, tab = "member" });
            }
            return View(member);
        }

        // POST: Member/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long? id)
        {
            Member member = db.Member.Find(id);
            db.Member.Remove(member);
            db.SaveChanges();
            SuccessMessagesAdd("Deleting Data Success");
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


        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            // 1. Validasi File dan data
            // 2. Simpan ke member dan client
            var policyId = Request.Params["PolicyId"];
            if (policyId == null)
            {
                return RedirectToAction("Index", "Policy");
            }
            var policy = db.Policy.Find(policyId);
            if (policy == null)
            {
                WarningMessagesAdd("Policy is not found");
                return RedirectToAction("Index", "Policy");
            }
            if (Request.Files["file"].ContentLength > 0)
            {
                var excel = new ExcelPackage(Request.Files[0].InputStream);
                var workSheet = excel.Workbook.Worksheets.FirstOrDefault();
                var startLineData = 12;
                var listMemberUpload = new List<UploadMember>();
                var rowCount = startLineData;
                try
                {
                    while (workSheet.Cells[rowCount, 1].Value != null && workSheet.Cells[rowCount, 1].Value.ToString() != string.Empty)
                    {
                        var newUploadMember = new UploadMember
                        {
                            A_No = Convert.ToString(workSheet.Cells[rowCount, 1].Value),
                            B_TypeOfChanges = Convert.ToString(workSheet.Cells[rowCount, 2].Value),
                            C_ParticipantNo = Convert.ToString(workSheet.Cells[rowCount, 3].Value),
                            D_EmployeeNo = Convert.ToString(workSheet.Cells[rowCount, 4].Value),
                            E_Company = Convert.ToString(workSheet.Cells[rowCount, 5].Value),
                            F_ParticipantName = Convert.ToString(workSheet.Cells[rowCount, 6].Value),
                            G_EmployeeName = Convert.ToString(workSheet.Cells[rowCount, 7].Value),
                            H_StatusRelation = Convert.ToString(workSheet.Cells[rowCount, 8].Value),
                            I_SorM = Convert.ToString(workSheet.Cells[rowCount, 9].Value),
                            J_DateOFBirth = Convert.ToString(workSheet.Cells[rowCount, 10].Value),
                            K_Gender = Convert.ToString(workSheet.Cells[rowCount, 11].Value),
                            L_Benefit = Convert.ToString(workSheet.Cells[rowCount, 12].Value),
                            M_EffectiveDate = Convert.ToString(workSheet.Cells[rowCount, 13].Value),
                            N_EndDate = Convert.ToString(workSheet.Cells[rowCount, 14].Value),
                            O_Reason = Convert.ToString(workSheet.Cells[rowCount, 15].Value),
                            P_Contact = Convert.ToString(workSheet.Cells[rowCount, 16].Value),
                            Q_BankAccountName = Convert.ToString(workSheet.Cells[rowCount, 17].Value),
                            R_BankCode = Convert.ToString(workSheet.Cells[rowCount, 18].Value),
                            S_BankAccountNo = Convert.ToString(workSheet.Cells[rowCount, 19].Value)
                        };
                        listMemberUpload.Add(newUploadMember);
                        rowCount++;
                    }

                }
                catch (Exception e)
                {
                    WarningMessagesAdd(e.MessageToList());
                }

                var listClientToInsert = new List<Client>();
                List<(Client client, DateTime startDate, DateTime? endDate, string plan, UploadMember uploadMember)> listClientToInsertWithAnotherProperty = new List<(Client client, DateTime startDate, DateTime? endDate, string plan, UploadMember uploadMember)>();
                var lastClientIdSequence = db.AspSequential.Where(x => x.Name == AspSequentialName.ClientId).FirstOrDefault().LastSequential;
                using (var dbTransaction = db.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        //var allMemberWithEStatusRelationWithEmployeeName = new List<(Client client, string employeeName)>();
                        var allMemberWithEStatusRelation = listMemberUpload.Where(x => x.H_StatusRelation == "E");
                        var allMemberWithNotEStatusRelation = listMemberUpload.Where(x => x.H_StatusRelation != "E");

                        foreach (var item in allMemberWithEStatusRelation)
                        {
                            var birtDate = new DateTime(int.Parse(item.J_DateOFBirth.Substring(0, 4)), int.Parse(item.J_DateOFBirth.Substring(4, 2)), int.Parse(item.J_DateOFBirth.Substring(6, 2)));
                            var startDate = new DateTime(int.Parse(item.M_EffectiveDate.Substring(0, 4)), int.Parse(item.M_EffectiveDate.Substring(4, 2)), int.Parse(item.M_EffectiveDate.Substring(6, 2)));
                            DateTime? endDate = null;
                            if (!string.IsNullOrEmpty(item.N_EndDate))
                            {
                                endDate = new DateTime(int.Parse(item.N_EndDate.Substring(0, 4)), int.Parse(item.N_EndDate.Substring(4, 2)), int.Parse(item.N_EndDate.Substring(6, 2)));
                            }

                            var client = db.Client.Where(x => x.FullName.ToLower() == item.F_ParticipantName.ToLower() && x.BirthDate == birtDate && x.BankAccountNumber == item.S_BankAccountNo && x.BankAccountName == item.Q_BankAccountName).FirstOrDefault();


                            if (client == null)
                            {
                                var newClientToInsert = new Client();
                                newClientToInsert.BankAccountCode = item.R_BankCode;
                                newClientToInsert.BankAccountName = item.Q_BankAccountName;
                                newClientToInsert.BankAccountNumber = item.S_BankAccountNo;
                                newClientToInsert.BirthDate = birtDate;
                                if (item.H_StatusRelation == "S")
                                {
                                    if (item.K_Gender == "F")
                                    {
                                        newClientToInsert.ClientRelation = ClientRelation.Wife;
                                    }
                                    else if (item.K_Gender == "M")
                                    {
                                        newClientToInsert.ClientRelation = ClientRelation.Husband;
                                    }
                                }
                                if (item.H_StatusRelation == "C")
                                {
                                    if (item.K_Gender == "F")
                                    {
                                        newClientToInsert.ClientRelation = ClientRelation.Daughter;
                                    }
                                    else if (item.K_Gender == "M")
                                    {
                                        newClientToInsert.ClientRelation = ClientRelation.Son;
                                    }
                                }
                                newClientToInsert.FullName = item.F_ParticipantName;
                                newClientToInsert.MobilePhone1 = item.P_Contact;
                                newClientToInsert.Sex = item.K_Gender == "M" ? "Male" : item.K_Gender == "F" ? "Female" : null;
                                newClientToInsert.MaritalStatus = item.I_SorM == "M" ? "Married" : item.I_SorM == "S" ? "Single" : null;
                                newClientToInsert.Type = "Personal";
                                newClientToInsert.ClientId = (lastClientIdSequence + 1).ToString().PadLeft(10, '0');
                                newClientToInsert.SetPropertyCreate();
                                db.Client.Add(newClientToInsert);

                                listClientToInsert.Add(newClientToInsert);
                                listClientToInsertWithAnotherProperty.Add((newClientToInsert, startDate, endDate, item.L_Benefit, item));
                                db.SaveChanges();
                                lastClientIdSequence++;
                            }
                            else
                            {
                                var memberIsExist = db.Member.Where(x => x.ClientId == client.ClientId && x.PolicyId != policy.PolicyId && (x.MemberStatus == MemberStatus.Active || x.MemberStatus == MemberStatus.Calculated || x.MemberStatus == MemberStatus.Endorse)).FirstOrDefault();
                                if (memberIsExist != null)
                                {
                                    WarningMessagesAdd(client.FullName + " Already Member Of Policy " + memberIsExist.Policy.PolicyNumber);
                                }
                                client.BankAccountCode = item.R_BankCode;
                                client.BankAccountName = item.Q_BankAccountName;
                                client.BankAccountNumber = item.S_BankAccountNo;
                                client.BirthDate = birtDate;
                                if (item.H_StatusRelation == "S")
                                {
                                    if (item.K_Gender == "F")
                                    {
                                        client.ClientRelation = ClientRelation.Wife;
                                    }
                                    else if (item.K_Gender == "M")
                                    {
                                        client.ClientRelation = ClientRelation.Husband;
                                    }
                                }
                                if (item.H_StatusRelation == "C")
                                {
                                    if (item.K_Gender == "F")
                                    {
                                        client.ClientRelation = ClientRelation.Daughter;
                                    }
                                    else if (item.K_Gender == "M")
                                    {
                                        client.ClientRelation = ClientRelation.Son;
                                    }
                                }
                                client.FullName = item.F_ParticipantName;
                                client.MobilePhone1 = item.P_Contact;
                                client.Sex = item.K_Gender == "M" ? "Male" : item.K_Gender == "F" ? "Female" : null;
                                client.MaritalStatus = item.I_SorM == "M" ? "Married" : item.I_SorM == "S" ? "Single" : null;
                                client.Type = "Personal";
                                client.SetPropertyUpdate();
                                db.SaveChanges();
                                listClientToInsert.Add(client);
                                listClientToInsertWithAnotherProperty.Add((client, startDate, endDate, item.L_Benefit, item));
                            }
                        }
                        foreach (var item in allMemberWithNotEStatusRelation)
                        {
                            var birtDate = new DateTime(int.Parse(item.J_DateOFBirth.Substring(0, 4)), int.Parse(item.J_DateOFBirth.Substring(4, 2)), int.Parse(item.J_DateOFBirth.Substring(6, 2)));
                            var startDate = new DateTime(int.Parse(item.M_EffectiveDate.Substring(0, 4)), int.Parse(item.M_EffectiveDate.Substring(4, 2)), int.Parse(item.M_EffectiveDate.Substring(6, 2)));
                            DateTime? endDate = null;
                            if (!string.IsNullOrEmpty(item.N_EndDate))
                            {
                                endDate = new DateTime(int.Parse(item.N_EndDate.Substring(0, 4)), int.Parse(item.N_EndDate.Substring(4, 2)), int.Parse(item.N_EndDate.Substring(6, 2)));
                            }
                            var client = db.Client.Where(x => x.FullName.ToLower() == item.F_ParticipantName.ToLower() && x.BirthDate == birtDate).FirstOrDefault();
                            if (client == null)
                            {
                                var newClientToInsert = new Client();
                                newClientToInsert.BankAccountCode = item.R_BankCode;
                                newClientToInsert.BankAccountName = item.Q_BankAccountName;
                                newClientToInsert.BankAccountNumber = item.S_BankAccountNo;
                                newClientToInsert.BirthDate = birtDate;
                                if (item.H_StatusRelation == "S")
                                {
                                    if (item.K_Gender == "F")
                                    {
                                        newClientToInsert.ClientRelation = ClientRelation.Wife;
                                    }
                                    else if (item.K_Gender == "M")
                                    {
                                        newClientToInsert.ClientRelation = ClientRelation.Husband;






                                    }
                                }
                                if (item.H_StatusRelation == "C")
                                {
                                    if (item.K_Gender == "F")
                                    {
                                        newClientToInsert.ClientRelation = ClientRelation.Daughter;
                                    }
                                    else if (item.K_Gender == "M")
                                    {
                                        newClientToInsert.ClientRelation = ClientRelation.Son;
                                    }
                                    else
                                    {
                                        WarningMessagesAdd(item.F_ParticipantName + " error, client relation type is not defined.");
                                    }
                                }
                                newClientToInsert.FullName = item.F_ParticipantName;
                                newClientToInsert.MobilePhone1 = item.P_Contact;
                                newClientToInsert.Sex = item.K_Gender == "M" ? "Male" : item.K_Gender == "F" ? "Female" : null;
                                newClientToInsert.MaritalStatus = item.I_SorM == "M" ? "Married" : item.I_SorM == "S" ? "Single" : null;
                                newClientToInsert.Type = "Personal";
                                newClientToInsert.ClientId = (lastClientIdSequence + 1).ToString().PadLeft(10, '0');
                                var clientRelateTo = listClientToInsertWithAnotherProperty.Where(x => x.uploadMember.G_EmployeeName == item.G_EmployeeName && x.uploadMember.H_StatusRelation == "E" && x.client.BankAccountName == item.Q_BankAccountName && x.client.BankAccountNumber == item.S_BankAccountNo).FirstOrDefault();
                                //var clientRelateTo = listClientToInsert.Where(x => x.BankAccountName == item.Q_BankAccountName && x.BankAccountNumber == item.S_BankAccountNo).FirstOrDefault();
                                if (clientRelateTo.client != null)
                                {
                                    newClientToInsert.RelateTo = clientRelateTo.client?.ClientId.PadLeft(10, '0');
                                }
                                else
                                {
                                    WarningMessagesAdd(item.F_ParticipantName + " error, can not find client relation, " + item.G_EmployeeName);
                                }
                                newClientToInsert.SetPropertyCreate();
                                db.Client.Add(newClientToInsert);
                                db.SaveChanges();
                                listClientToInsert.Add(newClientToInsert);
                                listClientToInsertWithAnotherProperty.Add((newClientToInsert, startDate, endDate, item.L_Benefit, item));

                                lastClientIdSequence++;
                            }
                            else
                            {
                                // checking whether already a member of another policy
                                var memberIsExist = db.Member.Where(x => x.ClientId == client.ClientId && x.PolicyId != policy.PolicyId && (x.MemberStatus == MemberStatus.Active || x.MemberStatus == MemberStatus.Calculated || x.MemberStatus == MemberStatus.Endorse)).FirstOrDefault();
                                if (memberIsExist != null)
                                {
                                    WarningMessagesAdd(client.FullName + " Already Member Of Policy " + memberIsExist.Policy.PolicyNumber);
                                }
                                client.BankAccountCode = item.R_BankCode;
                                client.BankAccountName = item.Q_BankAccountName;
                                client.BankAccountNumber = item.S_BankAccountNo;
                                client.BirthDate = birtDate;
                                if (item.H_StatusRelation == "S")
                                {
                                    if (item.K_Gender == "F")
                                    {
                                        client.ClientRelation = ClientRelation.Wife;
                                    }
                                    else if (item.K_Gender == "M")
                                    {
                                        client.ClientRelation = ClientRelation.Husband;
                                    }
                                }
                                if (item.H_StatusRelation == "C")
                                {
                                    if (item.K_Gender == "F")
                                    {
                                        client.ClientRelation = ClientRelation.Daughter;
                                    }
                                    else if (item.K_Gender == "M")
                                    {
                                        client.ClientRelation = ClientRelation.Son;
                                    }
                                }
                                client.FullName = item.F_ParticipantName;
                                client.MobilePhone1 = item.P_Contact;
                                client.Sex = item.K_Gender == "M" ? "Male" : item.K_Gender == "F" ? "Female" : null;
                                client.MaritalStatus = item.I_SorM == "M" ? "Married" : item.I_SorM == "S" ? "Single" : null;
                                client.Type = "Personal";
                                var clientRelateTo = listClientToInsertWithAnotherProperty.Where(x => x.uploadMember.G_EmployeeName == item.G_EmployeeName && x.uploadMember.H_StatusRelation == "E" && x.client.BankAccountName == item.Q_BankAccountName && x.client.BankAccountNumber == item.S_BankAccountNo).FirstOrDefault();
                                //var clientRelateTo = listClientToInsert.Where(x => x.BankAccountName == item.Q_BankAccountName && x.BankAccountNumber == item.S_BankAccountNo).FirstOrDefault();
                                if (clientRelateTo.client != null)
                                {
                                    client.RelateTo = clientRelateTo.client?.ClientId.PadLeft(10, '0');
                                }
                                else
                                {
                                    WarningMessagesAdd(item.F_ParticipantName + " error, can not find client relation, " + item.G_EmployeeName);
                                }
                                client.SetPropertyUpdate();
                                db.SaveChanges();
                                listClientToInsert.Add(client);
                                listClientToInsertWithAnotherProperty.Add((client, startDate, endDate, item.L_Benefit, item));

                            }
                        }




                        if (WarningMessages().Count == 0)
                        {
                            dbTransaction.Commit();

                            using (var dbTransactionInsertingMember = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    //Add to table member
                                    if (listClientToInsertWithAnotherProperty.Count == 0)
                                    {
                                        WarningMessagesAdd("Failed Getting Data From Excel");
                                    }
                                    else
                                    {
                                        foreach (var (client, startDate, endDate, plan, uploadMember) in listClientToInsertWithAnotherProperty)
                                        {

                                            Member memberExist = db.Member.Where(x => x.ClientId == client.ClientId).FirstOrDefault();

                                            if (memberExist == null)
                                            {
                                                var newMember = new Member();
                                                newMember.ClientId = client.ClientId;
                                                newMember.PolicyId = policyId;
                                                newMember.Age = short.Parse(Math.Round(((DateTime.Now - client.BirthDate.Value).TotalDays / 365.25), 0).ToString());
                                                //newMember.CardNumber = "CardNumber_" + policyId + "_" + client.ClientId;
                                                newMember.MemberStatus = "Inactive";
                                                if (startDate < policy.StartDate)
                                                {
                                                    WarningMessagesAdd("Start Date " + client.FullName + " can not lower than Start Date of Policy");
                                                }

                                                newMember.StartDate = startDate;

                                                if (endDate > policy.MatureDate)
                                                {
                                                    WarningMessagesAdd("Start Date " + client.FullName + " can not lower than Start Date of Policy");
                                                }
                                                newMember.EndDate = endDate ?? policy.MatureDate;

                                                newMember.SetPropertyCreate();



                                                db.Member.Add(newMember);
                                                db.SaveChanges();
                                                // set sequencial number


                                                //newMember.Client = db.Client.Find(newMember.ClientId);

                                                if (newMember.Client.RelateTo != null)
                                                {
                                                    var allMemberRelateToSameClient = db.Member.Where(x => x.Client.RelateTo == newMember.Client.RelateTo && x.MemberId != newMember.MemberId).ToList();
                                                    newMember.SequencialNo = allMemberRelateToSameClient != null && allMemberRelateToSameClient.Count > 0 ? allMemberRelateToSameClient.Max(x => x.SequencialNo) + 1 : 1;
                                                }
                                                db.Entry(newMember).State = EntityState.Modified;
                                                db.SaveChanges();

                                                //indentify Plan
                                                var listOfBenefitThisItem = plan.Replace(" ", "").ToLower().Split(',').ToList();

                                                var planOfThisPolicy = db.Plan.Where(x => x.PolicyId == policyId).ToList();
                                                var isHasPlan = false;
                                                int counter = 0;
                                                foreach (var itemInPlanOfThisPolicy in planOfThisPolicy)
                                                {
                                                    var planDetailList = db.PlanDetail.Where(x => x.PlanId == itemInPlanOfThisPolicy.PlanId && x.PolicyId == policyId).ToList();
                                                    var plantDetailListSelect = planDetailList.Select(x => x.BasicProductLimitCode.ToLower()).ToList();



                                                    isHasPlan = listOfBenefitThisItem.All(plantDetailListSelect.Contains) && listOfBenefitThisItem.Count == plantDetailListSelect.Count;


                                                    if (isHasPlan)
                                                    {
                                                        newMember.PlanId = itemInPlanOfThisPolicy.PlanId;

                                                        foreach (var itemInPlanDetails in planDetailList)
                                                        {
                                                            var newMemberPlan = new MemberPlan();
                                                            newMemberPlan.PolicyId = newMember.PolicyId;
                                                            newMemberPlan.MemberId = newMember.MemberId;
                                                            newMemberPlan.PlanId = newMember.PlanId;
                                                            newMemberPlan.BasicProductId = itemInPlanDetails.BasicProductId;
                                                            newMemberPlan.BasicProductLimitCode = itemInPlanDetails.BasicProductLimitCode;
                                                            newMemberPlan.SetPropertyCreate();
                                                            db.MemberPlan.Add(newMemberPlan);
                                                        }
                                                        db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        counter++;
                                                    }


                                                }
                                                if (counter == planOfThisPolicy.Count)
                                                {
                                                    WarningMessagesAdd("Plan [" + plan.Replace(" ", "") + "] for " + client.FullName + " is not found.");
                                                }

                                            }
                                            else
                                            {
                                                WarningMessagesAdd("Member " + client.FullName + ", with Policy Holder " + memberExist.Policy.Client.FullName + " already exist");
                                            }

                                        }

                                    }
                                    if (WarningMessages().Count == 0)
                                    {
                                        db.SaveChanges();
                                        dbTransactionInsertingMember.Commit();
                                    }
                                    else
                                    {
                                        dbTransactionInsertingMember.Rollback();
                                    }
                                }
                                catch (Exception e)
                                {
                                    dbTransactionInsertingMember.Rollback();
                                    WarningMessagesAdd(e.MessageToList());
                                    WarningMessagesDistinct();
                                }
                            }

                        }
                        else
                        {
                            dbTransaction.Rollback();
                        }
                    }
                    catch (Exception e)
                    {
                        WarningMessagesAdd(e.MessageToList());
                        dbTransaction.Rollback();

                    }
                }

            }

            if (WarningMessages().Count == 0)
            {
                SuccessMessagesAdd("Uploading Member Success");
            }
            return RedirectToAction("Details", "Policy", new { id = policyId, tab = "member" });
        }



        public ActionResult PolicyMemberBenefit(string policyId, string memberId, string basicProductId, string basicProductLimitCode)
        {
            var id = basicProductId;
            var bpLimitId = basicProductLimitCode;
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
                    newBenefit.BasicProductLimit = new List<BasicProductLimit>();
                    newBenefit.BasicProductLimit.Add(basicProductLimitNew);
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
            return View(basicProductLimitHdr);

        }
    }
}
