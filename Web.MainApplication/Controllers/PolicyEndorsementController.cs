using OfficeOpenXml;
using Repository.Application.DataModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Web.MainApplication.Models;
using Web.MainApplication.Models.UploadModel;
using EntityState = System.Data.Entity.EntityState;

namespace Web.MainApplication.Controllers
{
    public class PolicyEndorsementController : BaseController
    {
        // GET: PolicyEndorsement 
        private DBEntities db = new DBEntities();
        public ActionResult Index()
        {
            var policy = db.Policy.Where(x => x.PolicyNumber != null);
            return View(policy);
        }
        public ActionResult TerminateMember()
        {
            var endorsements = db.Endorsement.Where(x => x.EndorseType == EndorseType.TerminateMember);
            return View(endorsements);
        }
        public ActionResult MemberTerminate(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var memberEndorsement = db.Member_Endorse.Find(id);
            if (memberEndorsement == null)
            {
                return HttpNotFound();
            }

            if (Request.Params["getPlanDetail"] == "true")
            {
                var planId = Request.Params["planId"];
                if (planId == null)
                {
                    return HttpNotFound();
                }
                db.Configuration.ProxyCreationEnabled = false;
                var planDetail = db.PlanDetail_Endorse.Where(x => x.PlanId == planId && x.PolicyId == memberEndorsement.PolicyId && x.EndorseNumber == memberEndorsement.EndorseNumber).ToList();
                foreach (var item in planDetail)
                {
                    var basicProduct = new BasicProduct();
                    basicProduct = db.BasicProduct.Where(x => x.BasicProductId == item.BasicProductId).FirstOrDefault();
                    db.Configuration.ProxyCreationEnabled = false;
                    item.BasicProduct = new BasicProduct();
                    item.BasicProduct.BasicProductName = basicProduct.BasicProductName;
                    item.BasicProduct.BasicProductLimit = basicProduct.BasicProductLimit;
                }
                return this.Json(new
                {
                    data = planDetail
                }, JsonRequestBehavior.AllowGet);
            }

            var sliTerminateType = new List<SelectListItem>();
            sliTerminateType.AddBlank();
            sliTerminateType.AddItemValText(TerminateType.Cancel, TerminateType.Cancel);
            sliTerminateType.AddItemValText(TerminateType.Refund, TerminateType.Refund);
            sliTerminateType.AddItemValText(TerminateType.Death, TerminateType.Death);

            ViewBag.TerminateType = sliTerminateType.ToSelectList(memberEndorsement.MemberStatus.Replace(" Calculated", ""));
            var newModalView = new ModalView()
            {
                ModalForm = new ModalForm { ActionName = "MemberTerminate", ControllerName = "PolicyEndorsement" },
                Title = "Terminate Member",
                Body = this.RenderRazorViewToString("_MemberTerminate", memberEndorsement),
                Footer = this.GetHtmlHelper().TextBox("Submit", "Submit", null, new { @class = "btn btn-primary", @type = "submit" }).ToString()

            };
            return View("_Modal", newModalView);

        }
        [HttpPost]
        [ActionName("MemberTerminate")]
        public ActionResult MemberTerminate(Member_Endorse model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var memberEndorse = db.Member_Endorse.Where(x => x.MemberId == model.MemberId).FirstOrDefault();
            if (memberEndorse == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var terminateType = Request.Params["terminateType"];
            if (terminateType == null)
            {
                WarningMessagesAdd("Terminate Type Is Required");
            }
            if (model.TerminateDate < memberEndorse.LastEndorseDate)
            {
                WarningMessagesAdd("Member Can Not Be Canceled");
            }
            if (model.TerminateDate < memberEndorse.Policy.StartDate)
            {
                WarningMessagesAdd("Terminate Date Invalid");
            }
            if (model.TerminateDate < memberEndorse.StartDate || model.TerminateDate > memberEndorse.EndDate)
            {
                WarningMessagesAdd("Terminate Date Invalid");
            }
            if (terminateType == TerminateType.Cancel && model.TerminateDate != memberEndorse.StartDate)
            {
                WarningMessagesAdd("Terminate Cancel Must Be Same With Start Date");
            }
            if (WarningMessages().Count == 0)
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {

                        memberEndorse.MemberStatus = terminateType;
                        memberEndorse.TerminateDate = model.TerminateDate;
                        memberEndorse.SetPropertyUpdate();
                        db.Entry(memberEndorse).State = EntityState.Modified;
                        db.SaveChanges();
                        dbTransaction.Commit();
                        SuccessMessagesAdd(Message.ProcessSuccess);
                        return RedirectToAction("Details", new { tab = "member", id = memberEndorse.EndorseNumber, endorseType = memberEndorse.Endorsement.EndorseType });
                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        WarningMessagesAdd(e.MessageToList());
                        WarningMessagesDistinct();
                    }
                }

            }
            return RedirectToAction("Details", new { tab = "member", id = memberEndorse.EndorseNumber, endorseType = memberEndorse.Endorsement.EndorseType });

        }
        [HttpGet]
        public ActionResult MemberMutate(long id)
        {
            if (Request.Params["onChangedPolicyTargetIsHit"] == "true")
            {
                var policyTargetId = Request.Params["policyTargetId"];
                if (policyTargetId != null)
                {
                    var policyTargetPlan = db.Plan.Where(x => x.Policy.PolicyNumber == policyTargetId).ToList();
                    var sliPlanIdNew = new List<SelectListItem>();
                    sliPlanIdNew.AddBlank();
                    foreach (var item in policyTargetPlan)
                    {
                        var planDetail = "";
                        var planDetailOfPlan = db.PlanDetail.Where(x => x.PlanId == item.PlanId && x.PolicyId == item.PolicyId);
                        foreach (var itemPlanDetail in planDetailOfPlan)
                        {
                            planDetail = planDetail + " " + itemPlanDetail.BasicProductLimitCode;
                        }
                        sliPlanIdNew.AddItemValText(item.PlanId, item.PlanName + " " + planDetail);
                    }
                    return Json(sliPlanIdNew, JsonRequestBehavior.AllowGet);

                }
                WarningMessagesAdd("PolicyTarget Is Null");

                return Json(new
                {
                    Message = WarningMessages()
                }, JsonRequestBehavior.AllowGet);
            }


            var member = db.Member_Endorse.Where(x => x.MemberId == id).FirstOrDefault();
            if (member == null)
            {
                return HttpNotFound();
            }
            var endorseChildMember = db.Member_Endorse.Where(x => x.Endorsement.Endorsement2.EndorseNumber == member.EndorseNumber && x.Endorsement.EndorseStatus == EndorseStatus.New && x.MemberNumber == member.MemberNumber).FirstOrDefault();
            var sliPolicyList = new List<SelectListItem>();
            sliPolicyList.AddBlank();
            var policyList = db.Policy.Where(x => x.PolicyNumber != null && x.PolicyStatus == PolicyStatus.Active).ToList();
            foreach (var item in policyList)
            {
                sliPolicyList.AddItemValText(item.PolicyNumber, item.PolicyNumber + " " + item.Client.FullName);
            }


            ViewBag.PolicyTarget = sliPolicyList.ToSelectList(endorseChildMember?.Policy.PolicyNumber);

            var sliPlanId = new List<SelectListItem>();
            sliPlanId.AddBlank();
            ViewBag.PlanId = sliPlanId.ToSelectList();

            var newModalView = new ModalView()
            {
                ModalForm = new ModalForm { ActionName = "MemberMutate", ControllerName = "PolicyEndorsement" },
                Title = "Mutate Member",
                Body = this.RenderRazorViewToString("_MemberMutate", member),
                Footer = this.GetHtmlHelper().TextBox("Submit", "Submit", null, new { @class = "btn btn-primary", @type = "submit" }).ToString()

            };
            return View("_Modal", newModalView);
        }
        [HttpPost]
        public ActionResult MemberMutate(Member_Endorse member)
        {
            if (member == null)
            {
                return HttpNotFound();
            }
            member = db.Member_Endorse.Where(x => x.MemberId == member.MemberId).FirstOrDefault();
            if (member == null)
            {
                return HttpNotFound();
            }
            var memberOriginal = db.Member.Where(x => x.MemberNumber == member.MemberNumber).FirstOrDefault();
            if (memberOriginal == null)
            {
                return HttpNotFound();
            }
            var policyNumberTarget = Request.Params["PolicyTarget"];
            if (policyNumberTarget == null)
            {
                return HttpNotFound();
            }
            var policyEndorsementTarget = db.Policy.Where(x => x.PolicyNumber == policyNumberTarget).FirstOrDefault();
            if (policyEndorsementTarget == null)
            {
                return HttpNotFound();
            }
            var endorsement = db.Endorsement.Find(member.EndorseNumber);
            Endorsement memberEndorsementExist = new Endorsement();
            foreach (var item in endorsement.Endorsement1.Where(x => x.EndorseType == EndorseType.Additional && x.EndorseStatus == EndorseStatus.New))
            {
                var endorsementExistForTheCurrentMember = item.Member_Endorse.Where(x => x.MemberNumber == member.MemberNumber).FirstOrDefault();

                if (endorsementExistForTheCurrentMember != null && endorsementExistForTheCurrentMember.Policy.PolicyNumber == policyNumberTarget)
                {
                    memberEndorsementExist = item;
                    WarningMessagesAdd("Endorsement Exist Fot This Member, Endorsement Number : " + item.EndorseNumber);
                }
            }
            //Memeriksa apakah sudah ada endorse untuk  ini
            var currentMemberEndorseExist = db.Member_Endorse.Where(x => x.Endorsement.Endorsement2.EndorseNumber == endorsement.EndorseNumber
            && x.Endorsement.Endorsement2.EndorseStatus == EndorseStatus.New && x.Endorsement.PolicyNumber == policyNumberTarget).FirstOrDefault();
            Plan planNewMember = new Plan();
            var planIdTarget = Request.Params["PlanId"];
            if (planIdTarget == null || planIdTarget == string.Empty)
            {
                // checking apakah ada plan yang sesuai pada polis tujuan.
                var listOfBenefitThisItem = db.PlanDetail.Where(x => x.PlanId == member.PlanId && x.PolicyId == member.PolicyId).ToList().Select(x => x.BasicProductLimitCode.ToLower());
                var plantOfPolicyTarget = db.Plan.Where(x => x.PolicyId == policyEndorsementTarget.PolicyId).ToList();
                var isHasPlan = false;
                int counter = 0;
                foreach (var planOfPolicyTarget in plantOfPolicyTarget)
                {
                    var planDetailList = db.PlanDetail.Where(x => x.PlanId == planOfPolicyTarget.PlanId && x.PolicyId == policyEndorsementTarget.PolicyId).ToList();
                    var plantDetailListSelect = planDetailList.Select(x => x.BasicProductLimitCode.ToLower()).ToList();
                    isHasPlan = listOfBenefitThisItem.All(plantDetailListSelect.Contains) && listOfBenefitThisItem.Count() == plantDetailListSelect.Count;
                    if (!isHasPlan)
                    {
                        counter++;
                    }
                    else
                    {
                        planNewMember = planOfPolicyTarget;
                    }
                }
                var planDetailLimitCode = "";
                foreach (var item in listOfBenefitThisItem)
                {
                    planDetailLimitCode = planDetailLimitCode + " " + item;
                }
                if (counter == plantOfPolicyTarget.Count)
                {
                    WarningMessagesAdd("Plan [" + planDetailLimitCode.ToUpper() + "] for " + member.Client.FullName + " is not found.");
                }
            }
            else
            {
                planNewMember = db.Plan.Where(x => x.PolicyId == policyEndorsementTarget.PolicyId && x.PlanId == planIdTarget).FirstOrDefault();
            }

            if (WarningMessages().Count == 0)
            {
                using (var dbTransaction = db.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        // logika bisnis
                        // ketika suatu member dimutasi, maka akan dibuat endorse baru dengan member dari endorse tersebut adalah member yang 
                        // akan dimutasi.

                        // menambah endorse apabila tidak ada endorse untuk member terkait
                        if (currentMemberEndorseExist == null)
                        {
                            var memberToDelete = db.Member_Endorse.Where(x => x.Endorsement.EndorseParent == member.EndorseNumber && x.MemberNumber == member.MemberNumber).FirstOrDefault();
                            if (memberToDelete != null)
                            {
                                db.PCF_Endorse.RemoveRange(memberToDelete?.PCF_Endorse);
                                db.MemberPlan_Endorse.RemoveRange(memberToDelete?.MemberPlan_Endorse);
                                if (memberToDelete?.Endorsement.Member_Endorse.Count == 1)
                                {
                                    db.Endorsement.Remove(memberToDelete.Endorsement);
                                }
                                db.Member_Endorse.Remove(memberToDelete);

                                db.SaveChanges();
                            }

                            var lastEndorsNumber = db.AspSequential.Where(x => x.Name == AspSequentialName.EndorseNumber).FirstOrDefault();
                            string policyNumber = Request.Params["PolicyTarget"];
                            var policyTarget = db.Policy.Where(x => x.PolicyNumber == policyNumber).FirstOrDefault();
                            var newEndorse = new Endorsement();
                            newEndorse.EndorseNumber = (lastEndorsNumber.LastSequential + 1).ToString().PadLeft(10, '0');
                            newEndorse.PolicyId = policyTarget.PolicyId;
                            newEndorse.PolicyNumber = policyTarget.PolicyNumber;
                            newEndorse.Id = 0;
                            newEndorse.EndorseParent = member.EndorseNumber;
                            newEndorse.EndorseType = EndorseType.Additional;
                            newEndorse.EndorseDate = member.Endorsement.EndorseDate;
                            newEndorse.EndorseStatus = EndorseStatus.New;
                            newEndorse.SetPropertyCreate();
                            db.Endorsement.Add(newEndorse);
                            db.SaveChanges();
                            newEndorse = db.Endorsement.Where(x => x.EndorseNumber == newEndorse.EndorseNumber).FirstOrDefault();
                            newEndorse.EndorseParent = member.EndorseNumber;
                            db.Entry(newEndorse).State = EntityState.Modified;
                            db.SaveChanges();

                            //add policyEndorse data
                            var newPolicyEndorse = new Policy_Endorse();
                            newPolicyEndorse.Agent = policyTarget.Agent;
                            newPolicyEndorse.ClientId = policyTarget.ClientId;
                            newPolicyEndorse.EndorseNumber = newEndorse.EndorseNumber;
                            newPolicyEndorse.EntryDate = policyTarget.EntryDate;
                            newPolicyEndorse.ExitDate = policyTarget.ExitDate;
                            newPolicyEndorse.IsActive = policyTarget.IsActive;
                            newPolicyEndorse.IssueDate = policyTarget.IssueDate;
                            newPolicyEndorse.LastEndorseDate = endorsement.CreatedDate;
                            newPolicyEndorse.MatureDate = policyTarget.MatureDate;
                            newPolicyEndorse.OpenOrClose = policyTarget.OpenOrClose;
                            newPolicyEndorse.PaymentFrequency = policyTarget.PaymentFrequency;
                            newPolicyEndorse.PolicyId = policyTarget.PolicyId;
                            newPolicyEndorse.PolicyNumber = policyTarget.PolicyNumber;
                            newPolicyEndorse.PolicyStatus = PolicyStatus.Endorse;
                            newPolicyEndorse.StartDate = policyTarget.StartDate;
                            newPolicyEndorse.TerminateDate = policyTarget.TerminateDate;
                            newPolicyEndorse.SetPropertyUpdate();

                            db.Policy_Endorse.Add(newPolicyEndorse);
                            db.SaveChanges();


                            // Add PlanEndorse for New Endorse
                            var planOriginal = db.Plan.Where(x => x.PolicyId == policyTarget.PolicyId);
                            foreach (var item in planOriginal)
                            {
                                var newPlanOfPolicyThatEndorse = new Plan_Endorse();
                                newPlanOfPolicyThatEndorse.EndorseNumber = newEndorse.EndorseNumber;
                                newPlanOfPolicyThatEndorse.PolicyId = policyTarget.PolicyId;
                                newPlanOfPolicyThatEndorse.PlanId = item.PlanId;
                                newPlanOfPolicyThatEndorse.PlanName = item.PlanName;
                                newPlanOfPolicyThatEndorse.PlanDesc = item.PlanDesc;
                                newPlanOfPolicyThatEndorse.StartDate = item.StartDate;
                                newPlanOfPolicyThatEndorse.SetPropertyCreate();
                                db.Plan_Endorse.Add(newPlanOfPolicyThatEndorse);
                            }
                            db.SaveChanges();
                            var planDetailOriginal = db.PlanDetail.Where(x => x.PolicyId == policyTarget.PolicyId);

                            // add PlanDetailEndorse
                            foreach (var item in planDetailOriginal)
                            {
                                var newPlanDetailEndorse = new PlanDetail_Endorse();
                                newPlanDetailEndorse.EndorseNumber = newEndorse.EndorseNumber;
                                newPlanDetailEndorse.PolicyId = policyTarget.PolicyId;
                                newPlanDetailEndorse.PlanId = item.PlanId;
                                newPlanDetailEndorse.BasicProductId = item.BasicProductId;
                                newPlanDetailEndorse.BasicProductLimitCode = item.BasicProductLimitCode;
                                newPlanDetailEndorse.SetPropertyCreate();
                                db.PlanDetail_Endorse.Add(newPlanDetailEndorse);
                            }
                            db.SaveChanges();
                            var memberOrigin = db.Member_Endorse.Where(x => x.MemberId == member.MemberId).FirstOrDefault();

                            var memberEndorse = new Member_Endorse();
                            memberEndorse.AdmedikaCode = memberOrigin.AdmedikaCode;
                            memberEndorse.Age = memberOrigin.Age;
                            memberEndorse.MemberNumber = memberOrigin.MemberNumber;
                            memberEndorse.CardNumber = memberOrigin.CardNumber;
                            memberEndorse.ClaimNumber = memberOrigin.ClaimNumber;
                            memberEndorse.ClientId = memberOrigin.ClientId;
                            memberEndorse.EndDate = memberOrigin.EndDate;
                            memberEndorse.EndorseNumber = newEndorse.EndorseNumber;
                            memberEndorse.EntryDate = memberOrigin.EntryDate;
                            memberEndorse.ExitDate = memberOrigin.ExitDate;
                            memberEndorse.IsActive = memberOrigin.IsActive;
                            memberEndorse.LastClaimDate = memberOrigin.LastClaimDate;
                            memberEndorse.LastEndorseDate = memberOrigin.LastEndorseDate; ;
                            memberEndorse.MemberStatus = memberOrigin.MemberStatus;
                            memberEndorse.PlanId = planNewMember.PlanId;
                            memberEndorse.PolicyId = newEndorse.PolicyId;
                            //memberEndorse.ProcessDate = memberOrigin.ProcessDate;
                            memberEndorse.SequencialNo = memberOrigin.SequencialNo;
                            memberEndorse.StartDate = endorsement.EndorseDate;
                            memberEndorse.TerminateDate = memberOriginal.TerminateDate;

                            memberEndorse.SetPropertyCreate();
                            db.Member_Endorse.Add(memberEndorse);
                            db.SaveChanges();

                            // add MemberPlan_Endorse data
                            var listMemberPlansNewMember = db.PlanDetail_Endorse.Where(x => x.EndorseNumber == newEndorse.EndorseNumber && x.PolicyId == policyTarget.PolicyId && x.PlanId == memberEndorse.PlanId);
                            foreach (var item in listMemberPlansNewMember)
                            {
                                var memberPlanEndorse = new MemberPlan_Endorse();
                                memberPlanEndorse.EndorseNumber = newEndorse.EndorseNumber;
                                memberPlanEndorse.PolicyId = policyTarget.PolicyId;
                                memberPlanEndorse.MemberId = memberEndorse.MemberId;
                                memberPlanEndorse.PlanId = memberEndorse.PlanId;
                                memberPlanEndorse.BasicProductId = item.BasicProductId;
                                memberPlanEndorse.StartDate = memberEndorse.StartDate;
                                memberPlanEndorse.BasicProductLimitCode = item.BasicProductLimitCode;
                                memberPlanEndorse.SetPropertyCreate();
                                db.MemberPlan_Endorse.Add(memberPlanEndorse);
                            }
                            db.SaveChanges();


                            if (WarningMessages().Count == 0)
                            {
                                dbTransaction.Commit();
                                SuccessMessagesAdd(Message.ProcessSuccess);
                                return RedirectToAction("Details", new { tab = "member", id = member.EndorseNumber, endorseType = member.Endorsement.EndorseType });
                            }
                            else
                            {
                                dbTransaction.Rollback();
                            }
                        }
                        // melakukan update pada endorse yang sudah ada sebelumnya.
                        else
                        {
                            if (currentMemberEndorseExist.Endorsement.Member_Endorse.Where(x => x.MemberNumber == member.MemberNumber).FirstOrDefault() == null)
                            {
                                var memberToDelete = db.Member_Endorse.Where(x => x.Endorsement.EndorseParent == member.EndorseNumber && x.MemberNumber == member.MemberNumber).FirstOrDefault();
                                if (memberToDelete != null)
                                {
                                    db.PCF_Endorse.RemoveRange(memberToDelete?.PCF_Endorse);
                                    db.MemberPlan_Endorse.RemoveRange(memberToDelete?.MemberPlan_Endorse);
                                    if (memberToDelete?.Endorsement.Member_Endorse.Count == 1)
                                    {
                                        db.Endorsement.Remove(memberToDelete.Endorsement);
                                    }
                                    db.Member_Endorse.Remove(memberToDelete);

                                    db.SaveChanges();
                                }
                                var memberOrigin = db.Member_Endorse.Where(x => x.MemberId == member.MemberId).FirstOrDefault();

                                var memberEndorse = new Member_Endorse();
                                memberEndorse.AdmedikaCode = memberOrigin.AdmedikaCode;
                                memberEndorse.Age = memberOrigin.Age;
                                memberEndorse.MemberNumber = memberOrigin.MemberNumber;
                                memberEndorse.CardNumber = memberOrigin.CardNumber;
                                memberEndorse.ClaimNumber = memberOrigin.ClaimNumber;
                                memberEndorse.ClientId = memberOrigin.ClientId;
                                memberEndorse.EndDate = memberOrigin.EndDate;
                                memberEndorse.EndorseNumber = currentMemberEndorseExist.EndorseNumber;
                                memberEndorse.EntryDate = memberOrigin.EntryDate;
                                memberEndorse.ExitDate = memberOrigin.ExitDate;
                                memberEndorse.IsActive = memberOrigin.IsActive;
                                memberEndorse.LastClaimDate = memberOrigin.LastClaimDate;
                                memberEndorse.LastEndorseDate = memberOrigin.LastEndorseDate;
                                memberEndorse.MemberStatus = memberOrigin.MemberStatus;
                                memberEndorse.PlanId = planNewMember.PlanId;
                                memberEndorse.PolicyId = currentMemberEndorseExist.PolicyId;
                                //memberEndorse.ProcessDate = memberOrigin.ProcessDate;
                                memberEndorse.SequencialNo = memberOrigin.SequencialNo;
                                memberEndorse.StartDate = endorsement.EndorseDate;
                                memberEndorse.TerminateDate = memberOriginal.TerminateDate;

                                memberEndorse.SetPropertyCreate();
                                db.Member_Endorse.Add(memberEndorse);
                                db.SaveChanges();

                                var listMemberPlansNewMember = db.PlanDetail_Endorse.Where(x => x.EndorseNumber == currentMemberEndorseExist.EndorseNumber && x.PolicyId == currentMemberEndorseExist.PolicyId && x.PlanId == memberEndorse.PlanId);
                                foreach (var item in listMemberPlansNewMember)
                                {
                                    var memberPlanEndorse = new MemberPlan_Endorse();
                                    memberPlanEndorse.EndorseNumber = currentMemberEndorseExist.EndorseNumber;
                                    memberPlanEndorse.PolicyId = currentMemberEndorseExist.PolicyId;
                                    memberPlanEndorse.MemberId = memberEndorse.MemberId;
                                    memberPlanEndorse.PlanId = memberEndorse.PlanId;
                                    memberPlanEndorse.BasicProductId = item.BasicProductId;
                                    memberPlanEndorse.StartDate = memberEndorse.StartDate;
                                    memberPlanEndorse.BasicProductLimitCode = item.BasicProductLimitCode;
                                    memberPlanEndorse.SetPropertyCreate();
                                    db.MemberPlan_Endorse.Add(memberPlanEndorse);
                                }
                                db.SaveChanges();

                            }
                            else
                            {
                                member = db.Member_Endorse.Where(x => x.MemberId == member.MemberId).FirstOrDefault();
                                currentMemberEndorseExist.Endorsement.PolicyId = policyEndorsementTarget.PolicyId;
                                currentMemberEndorseExist.Endorsement.PolicyNumber = policyNumberTarget;
                                currentMemberEndorseExist.Endorsement.SetPropertyUpdate();
                                currentMemberEndorseExist.PolicyId = policyEndorsementTarget.PolicyId;
                                currentMemberEndorseExist.SetPropertyUpdate();
                                db.Entry(currentMemberEndorseExist.Endorsement).State = EntityState.Modified;
                                currentMemberEndorseExist.SetPropertyUpdate();
                                db.Entry(currentMemberEndorseExist).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            if (WarningMessages().Count == 0)
                            {
                                dbTransaction.Commit();
                                SuccessMessagesAdd(Message.ProcessSuccess);
                                return RedirectToAction("Details", new { tab = "member", id = member.EndorseNumber, endorseType = member.Endorsement.EndorseType });
                            }
                            else
                            {
                                dbTransaction.Rollback();
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        WarningMessagesAdd(e.MessageToList());
                        WarningMessagesDistinct();
                    }
                }

            }
            return RedirectToAction("Details", new { tab = "member", id = member.EndorseNumber, endorseType = EndorseType.Mutation });

        }
        public ActionResult TerminatePolicy(string policyId)
        {
            if (policyId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var policy = db.Policy.Find(policyId);

            if (policy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(policy);
        }
        [HttpPost]
        [ActionName("TerminatePolicy")]
        public ActionResult TerminatePolicyConfirm(string policyId)
        {
            if (policyId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var policy = db.Policy.Find(policyId);
            if (policy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View();
        }
        [ActionName("Additional")]
        public ActionResult AdditionalMember()
        {
            var endorsements = db.Endorsement.Where(x => x.EndorseType == EndorseType.Additional).OrderByDescending(x => x.EndorseNumber);
            return View(endorsements);
        }
        [HttpGet]
        public ActionResult Create()
        {
            var endorseType = Request.Params["endorseType"];
            if (endorseType == null)
            {
                WarningMessagesAdd("Endorse Type Is Not Defined");
            }
            var model = new Endorsement();
            model.EndorseType = endorseType;
            model.EndorseStatus = "New";
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(Endorsement endorsement)
        {
            var endorsementDB = db.Endorsement.Where(x => x.PolicyNumber == endorsement.PolicyNumber && x.EndorseStatus != EndorseStatus.Done).FirstOrDefault();

            if (endorsementDB != null)
            {
                WarningMessagesAdd("Policy Is Still In Endorse Process");
            }
            var endorseType = Request.Params["endorseType"];
            if (endorseType == null)
            {
                WarningMessagesAdd("Endorse Type Is Not Defined");
            }
            var policy = db.Policy.Where(x => x.PolicyNumber == endorsement.PolicyNumber).FirstOrDefault();
            if (policy == null)
            {
                WarningMessagesAdd("Policy Is Not Found");
            }
            if (policy.StartDate > endorsement.EndorseDate)
            {
                WarningMessagesAdd("Endorse Date Not Allowed, Policy StartDate at " + policy.LastEndorseDate.Value.ToString("dd/MM/yyyy"));
            }
            if (endorseType != EndorseType.TerminateMember)
            {
                if (policy?.LastEndorseDate > endorsement.EndorseDate)
                {
                    WarningMessagesAdd("Endorse Date Not Allowed, Last Policy Endorsed at " + policy.LastEndorseDate.Value.ToString("dd/MM/yyyy"));
                }
            }


            if (ModelState.IsValid && WarningMessages().Count == 0)
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (endorseType == EndorseType.Mutation)
                        {
                            var lastEndorsNumber = db.AspSequential.Where(x => x.Name == AspSequentialName.EndorseNumber).FirstOrDefault();
                            endorsement.EndorseNumber = (lastEndorsNumber.LastSequential + 1).ToString().PadLeft(10, '0');
                            endorsement.Policy = null;
                            endorsement.PolicyId = policy.PolicyId;
                            endorsement.Id = 1000;
                            endorsement.EndorseType = endorseType;
                            endorsement.SetPropertyCreate();
                            db.Endorsement.Add(endorsement);
                            db.SaveChanges();

                            endorsement = db.Endorsement.Where(x => x.EndorseNumber == endorsement.EndorseNumber).FirstOrDefault();

                            if (endorsement == null)
                            {
                                WarningMessagesAdd(Message.CreateFail);

                            }
                            policy.PolicyStatus = "END";

                            policy.SetPropertyUpdate();
                            db.Entry(policy).State = System.Data.Entity.EntityState.Modified;

                            var newPolicyEndorsment = new Policy_Endorse();
                            newPolicyEndorsment.Agent = policy.Agent;
                            newPolicyEndorsment.ClientId = policy.ClientId;
                            newPolicyEndorsment.EndorseNumber = endorsement.EndorseNumber;
                            newPolicyEndorsment.EntryDate = policy.EntryDate;
                            newPolicyEndorsment.ExitDate = policy.ExitDate;
                            newPolicyEndorsment.IsActive = policy.IsActive;
                            newPolicyEndorsment.IssueDate = policy.IssueDate;
                            newPolicyEndorsment.LastEndorseDate = endorsement.CreatedDate;
                            newPolicyEndorsment.MatureDate = policy.MatureDate;
                            newPolicyEndorsment.OpenOrClose = policy.OpenOrClose;
                            newPolicyEndorsment.PaymentFrequency = policy.PaymentFrequency;
                            newPolicyEndorsment.PolicyId = policy.PolicyId;
                            newPolicyEndorsment.PolicyNumber = policy.PolicyNumber;
                            newPolicyEndorsment.PolicyStatus = policy.PolicyStatus;
                            newPolicyEndorsment.StartDate = policy.StartDate;
                            newPolicyEndorsment.TerminateDate = policy.TerminateDate;
                            newPolicyEndorsment.SetPropertyUpdate();

                            db.Policy_Endorse.Add(newPolicyEndorsment);
                            db.SaveChanges();

                            foreach (var plantPolicy in policy.Plan)
                            {
                                var planEndorse = new Plan_Endorse();
                                planEndorse.EndorseNumber = endorsement.EndorseNumber;
                                planEndorse.IsActive = plantPolicy.IsActive;
                                planEndorse.PlanDesc = plantPolicy.PlanDesc;
                                planEndorse.PlanId = plantPolicy.PlanId;
                                planEndorse.IsActive = plantPolicy.IsActive;
                                planEndorse.PlanName = plantPolicy.PlanName;
                                planEndorse.PolicyId = plantPolicy.PolicyId;
                                planEndorse.StartDate = plantPolicy.StartDate;
                                planEndorse.SetPropertyCreate();
                                db.Plan_Endorse.Add(planEndorse);

                                db.SaveChanges();

                                var plantDetailPolicys = db.PlanDetail.Where(x => x.PolicyId == endorsement.PolicyId && x.PlanId == plantPolicy.PlanId);
                                foreach (var plantDetailPolicy in plantDetailPolicys)
                                {
                                    var newPlanDetailEndorse = new PlanDetail_Endorse();
                                    newPlanDetailEndorse.BasicProductId = plantDetailPolicy.BasicProductId;
                                    newPlanDetailEndorse.BasicProductLimitCode = plantDetailPolicy.BasicProductLimitCode;
                                    newPlanDetailEndorse.EndorseNumber = endorsement.EndorseNumber;
                                    newPlanDetailEndorse.IsActive = plantDetailPolicy.IsActive;
                                    newPlanDetailEndorse.PlanId = plantDetailPolicy.PlanId;
                                    newPlanDetailEndorse.PolicyId = plantDetailPolicy.PolicyId;
                                    newPlanDetailEndorse.SetPropertyCreate();
                                    db.PlanDetail_Endorse.Add(newPlanDetailEndorse);

                                }
                                db.SaveChanges();
                            }
                            db.SaveChanges();
                            dbTransaction.Commit();
                            SuccessMessagesAdd(Message.CreateSuccess);
                            return RedirectToAction(EndorseType.Mutation);
                        }
                        else
                        {
                            var lastEndorsNumber = db.AspSequential.Where(x => x.Name == AspSequentialName.EndorseNumber).FirstOrDefault();
                            endorsement.EndorseNumber = (lastEndorsNumber.LastSequential + 1).ToString().PadLeft(10, '0');
                            endorsement.PolicyId = policy.PolicyId;
                            endorsement.Policy = null;
                            endorsement.Id = 1000;
                            endorsement.EndorseType = endorsement.EndorseType;
                            db.Endorsement.Add(endorsement);
                            db.SaveChanges();

                            endorsement = db.Endorsement.Where(x => x.EndorseNumber == endorsement.EndorseNumber).FirstOrDefault();

                            if (endorsement == null)
                            {
                                WarningMessagesAdd(Message.CreateFail);

                            }
                            policy.PolicyStatus = PolicyStatus.Endorse;

                            policy.SetPropertyUpdate();
                            db.Entry(policy).State = System.Data.Entity.EntityState.Modified;

                            var newPolicyEndorsment = new Policy_Endorse();
                            newPolicyEndorsment.Agent = policy.Agent;
                            newPolicyEndorsment.ClientId = policy.ClientId;
                            newPolicyEndorsment.EndorseNumber = endorsement.EndorseNumber;
                            newPolicyEndorsment.EntryDate = policy.EntryDate;
                            newPolicyEndorsment.ExitDate = policy.ExitDate;
                            newPolicyEndorsment.IsActive = policy.IsActive;
                            newPolicyEndorsment.IssueDate = policy.IssueDate;
                            newPolicyEndorsment.LastEndorseDate = endorsement.CreatedDate;
                            newPolicyEndorsment.MatureDate = policy.MatureDate;
                            newPolicyEndorsment.OpenOrClose = policy.OpenOrClose;
                            newPolicyEndorsment.PaymentFrequency = policy.PaymentFrequency;
                            newPolicyEndorsment.PolicyId = policy.PolicyId;
                            newPolicyEndorsment.PolicyNumber = policy.PolicyNumber;
                            newPolicyEndorsment.PolicyStatus = policy.PolicyStatus;
                            newPolicyEndorsment.StartDate = policy.StartDate;
                            newPolicyEndorsment.TerminateDate = policy.TerminateDate;
                            newPolicyEndorsment.SetPropertyUpdate();
                            if (endorsement.EndorseType == EndorseType.Renewal)
                            {
                                newPolicyEndorsment.StartDate = new DateTime((policy.StartDate.Value.Year + 1), 1, 1);
                                newPolicyEndorsment.MatureDate = newPolicyEndorsment.StartDate.Value.AddYears(1).AddDays(-1);


                            }
                            db.Policy_Endorse.Add(newPolicyEndorsment);
                            db.SaveChanges();



                            foreach (var plantPolicy in policy.Plan)
                            {
                                var planEndorse = new Plan_Endorse();
                                planEndorse.EndorseNumber = endorsement.EndorseNumber;
                                planEndorse.IsActive = plantPolicy.IsActive;
                                planEndorse.PlanDesc = plantPolicy.PlanDesc;
                                planEndorse.PlanId = plantPolicy.PlanId;
                                planEndorse.IsActive = plantPolicy.IsActive;
                                planEndorse.PlanName = plantPolicy.PlanName;
                                planEndorse.PolicyId = plantPolicy.PolicyId;
                                planEndorse.StartDate = plantPolicy.StartDate;
                                planEndorse.SetPropertyCreate();
                                db.Plan_Endorse.Add(planEndorse);

                                db.SaveChanges();

                                var plantDetailPolicys = db.PlanDetail.Where(x => x.PolicyId == endorsement.PolicyId && x.PlanId == plantPolicy.PlanId);
                                foreach (var plantDetailPolicy in plantDetailPolicys)
                                {
                                    var newPlanDetailEndorse = new PlanDetail_Endorse();
                                    newPlanDetailEndorse.BasicProductId = plantDetailPolicy.BasicProductId;
                                    newPlanDetailEndorse.BasicProductLimitCode = plantDetailPolicy.BasicProductLimitCode;
                                    newPlanDetailEndorse.EndorseNumber = endorsement.EndorseNumber;
                                    newPlanDetailEndorse.IsActive = plantDetailPolicy.IsActive;
                                    newPlanDetailEndorse.PlanId = plantDetailPolicy.PlanId;
                                    newPlanDetailEndorse.PolicyId = plantDetailPolicy.PolicyId;
                                    newPlanDetailEndorse.SetPropertyCreate();
                                    db.PlanDetail_Endorse.Add(newPlanDetailEndorse);

                                }
                                db.SaveChanges();


                            }


                            db.SaveChanges();
                            dbTransaction.Commit();
                            SuccessMessagesAdd(Message.CreateSuccess);
                            return RedirectToAction(endorsement.EndorseType);
                        }


                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        WarningMessagesAdd(e.MessageToList());
                    }
                }
            }
            return View(endorsement);
        }
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var endorsement = db.Endorsement.Find(id);
            if (endorsement == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.Tab = Request.Params["tab"] ?? "";
            if (endorsement.EndorseStatus == EndorseStatus.Done)
            {
                return View("DetailsStatusDone", endorsement);
            }
            return View(endorsement);
        }
        [ActionName("Details" + EndorseType.Additional)]
        public ActionResult DetailsAdditional(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var endorsement = db.Endorsement.Find(id);
            if (endorsement == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.Tab = Request.Params["tab"] ?? "";
            return View(endorsement);
        }
        [ActionName("Details" + EndorseType.MovePlan)]
        public ActionResult DetailsMovePlan(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var endorsement = db.Endorsement.Find(id);
            if (endorsement == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.Tab = Request.Params["tab"] ?? "";
            return View(endorsement);
        }
        [ActionName("Details" + EndorseType.Mutation)]
        public ActionResult DetailsMutation(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var endorsement = db.Endorsement.Find(id);
            if (endorsement == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.Tab = Request.Params["tab"] ?? "";
            return View(endorsement);
        }
        [ActionName("Details" + EndorseType.TerminateMember)]
        public ActionResult DetailsTerminateMember(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var endorsement = db.Endorsement.Find(id);
            if (endorsement == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.Tab = Request.Params["tab"] ?? "";
            return View("Details", endorsement);
        }
        [HttpGet]
        public ActionResult CreatePlan(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var endorsement = db.Endorsement.Find(id);
            if (endorsement == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (endorsement.Policy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            var policy = endorsement.Policy;
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

            policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == policy.PaymentFrequency).FirstOrDefault() != null
                ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == policy.PaymentFrequency).FirstOrDefault().Text : policy.PaymentFrequency;


            var newPlanEndorsement = new Plan_Endorse();
            newPlanEndorsement.Endorsement = endorsement;
            newPlanEndorsement.EndorseNumber = endorsement.EndorseNumber;
            newPlanEndorsement.Policy = policy;
            newPlanEndorsement.PolicyId = policy.PolicyId;
            if (policy.StartDate.HasValue)
            {
                newPlanEndorsement.StartDate = policy.StartDate;
            }

            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber");
            return View(newPlanEndorsement);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePlan([Bind(Include = "EndorseNumber,PolicyId,PlanId,PlanName,PlanDesc,StartDate,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] Plan_Endorse plan)
        {
            if (ModelState.IsValid)
            {

                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var planDetailData = Request.Params["PlanDetailData"];
                        var selectedPlan = planDetailData.Split(',').ToList();
                        var listPlanDetail = new List<PlanDetail_Endorse>();

                        for (int i = 0; i < selectedPlan.Count; i++)
                        {
                            var limitcode = selectedPlan.ElementAt(i).Split(';').ElementAtOrDefault(1);
                            if (db.BasicProductLimit.Where(z => z.BasicProductLimitCode == limitcode).FirstOrDefault() == null)
                            {
                                WarningMessagesAdd(limitcode + " is not found");
                            }
                            else
                            {
                                var newPlanDetail = new PlanDetail_Endorse();
                                newPlanDetail.EndorseNumber = plan.EndorseNumber;
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
                            db.PlanDetail_Endorse.AddRange(listPlanDetail);
                            plan.SetPropertyCreate();
                            db.Plan_Endorse.Add(plan);
                            db.SaveChanges();
                            dbTransaction.Commit();
                            SuccessMessagesAdd("Inserting Data Success");
                            return RedirectToAction("Details", "policyendorsement", new { id = plan.EndorseNumber, endorseType = Request.Params["endorseType"] });
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

            var endorsement = db.Endorsement.Find(plan.EndorseNumber);
            if (endorsement == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (endorsement.Policy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            plan.Endorsement = endorsement;
            return View(plan);
        }
        [HttpGet]
        public ActionResult CreateMember(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var endorsement = db.Endorsement.Find(id);
            if (endorsement == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var newMemberEndorsement = new Member_Endorse();
            newMemberEndorsement.Endorsement = endorsement;
            newMemberEndorsement.PolicyId = endorsement.PolicyId;
            newMemberEndorsement.Policy = endorsement.Policy;
            newMemberEndorsement.EndorseNumber = endorsement.EndorseNumber;

            var dropdownPlan = new List<SelectListItem>();
            dropdownPlan.AddBlank();
            db.Plan_Endorse.Where(x => x.PolicyId == endorsement.PolicyId && x.EndorseNumber == endorsement.EndorseNumber).ToList().ForEach(x =>
            {
                dropdownPlan.AddItemValText(x.PlanId, x.PlanId + " - " + x.PlanName);
            });
            ViewBag.PlanId = dropdownPlan.ToSelectList();

            var dropdownAgent = new List<SelectListItem>();
            dropdownAgent.AddBlank();
            db.Plan.Where(x => x.PolicyId == endorsement.PolicyId).ToList().ForEach(x =>
            {
                dropdownAgent.AddItemValText(x.Policy.Agent, x.Policy.Agent);
            });
            ViewBag.Agent = dropdownAgent.ToSelectList();



            newMemberEndorsement.Policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == newMemberEndorsement.Policy.PaymentFrequency).FirstOrDefault() != null
                ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == newMemberEndorsement.Policy.PaymentFrequency).FirstOrDefault().Text : newMemberEndorsement.Policy.PaymentFrequency;

            ViewBag.ClientId = new SelectList(db.Client, "ClientId", "Type");
            ViewBag.PolicyId = new SelectList(db.Policy, "PolicyId", "PolicyNumber");
            newMemberEndorsement.MemberNumber = "Auto Generated";
            newMemberEndorsement.MemberStatus = "Inactive";

            return View(newMemberEndorsement);
        }
        [HttpPost]
        public ActionResult CreateMember(Member_Endorse member)
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
                if (db.Member.Where(x => x.ClientId == member.ClientId && x.PolicyId != member.PolicyId).FirstOrDefault() != null || db.Member_Endorse.Where(x => x.ClientId == member.ClientId && x.PolicyId != member.PolicyId).FirstOrDefault() != null)
                {
                    WarningMessagesAdd("Member already registered.");

                }
                if (db.Member.Where(x => x.ClientId == member.ClientId).FirstOrDefault() != null || db.Member_Endorse.Where(x => x.ClientId == member.ClientId).FirstOrDefault() != null)
                {
                    WarningMessagesAdd("Member already registered in another policy.");

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
                                member.Policy = null;
                                db.Member_Endorse.Add(member);
                                db.SaveChanges();
                                // set sequencial number
                                if (member.Client.RelateTo != null)
                                {
                                    var listSeq = new List<int?>();
                                    var allMemberRelateToSameClient = db.Member.Where(x => x.Client.RelateTo == member.Client.RelateTo && x.MemberId != member.MemberId).ToList();
                                    var allMemberRelateToSameClientEndorse = db.Member_Endorse.Where(x => x.Client.RelateTo == member.Client.RelateTo && x.MemberId != member.MemberId).ToList();
                                    listSeq.AddRange(allMemberRelateToSameClient.Select(x => x.SequencialNo));
                                    listSeq.AddRange(allMemberRelateToSameClientEndorse.Select(x => x.SequencialNo));
                                    member.SequencialNo = allMemberRelateToSameClient != null && listSeq.Count > 0 ? listSeq.Max(x => x) + 1 : 1;
                                }
                                db.Entry(member).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                var planDetails = db.PlanDetail_Endorse.Where(x => x.PolicyId == memberPolicy.PolicyId && x.PlanId == member.PlanId).ToList();
                                foreach (var item in planDetails)
                                {
                                    var newMemberPlan = new MemberPlan_Endorse();
                                    newMemberPlan.EndorseNumber = member.EndorseNumber;
                                    newMemberPlan.PolicyId = member.PolicyId;
                                    newMemberPlan.MemberId = member.MemberId;
                                    newMemberPlan.PlanId = member.PlanId;
                                    newMemberPlan.BasicProductId = item.BasicProductId;
                                    newMemberPlan.BasicProductLimitCode = item.BasicProductLimitCode;
                                    newMemberPlan.SetPropertyCreate();
                                    db.MemberPlan_Endorse.Add(newMemberPlan);
                                }
                                db.SaveChanges();
                                dbTransaction.Commit();
                                SuccessMessagesAdd("Inserting Data Success");
                                return RedirectToAction("Details", "PolicyEndorsement", new { id = member.EndorseNumber, tab = "member" });
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
            var endorsement = db.Endorsement.Find(member.EndorseNumber);
            if (policy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (endorsement == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var newMemberEndorsement = new Member_Endorse();
            member.Endorsement = endorsement;
            member.PolicyId = endorsement.PolicyId;
            member.Policy = endorsement.Policy;
            member.EndorseNumber = endorsement.EndorseNumber;
            member.Client = db.Client.Find(member.ClientId) ?? new Client();
            var dropdownPlan = new List<SelectListItem>();
            dropdownPlan.AddBlank();
            db.Plan_Endorse.Where(x => x.PolicyId == endorsement.PolicyId && x.EndorseNumber == endorsement.EndorseNumber).ToList().ForEach(x =>
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
        [HttpGet]
        public ActionResult DeleteAllMember(string endorseNumber)
        {
            if (endorseNumber == null)
            {
                return new HttpNotFoundResult();
            }
            var endorsement = db.Endorsement.Find(endorseNumber);
            if (endorsement == null)
            {
                return new HttpNotFoundResult();
            }

            var allMemberEndorse = db.Member_Endorse.Where(x => x.EndorseNumber == endorseNumber);
            if (allMemberEndorse.Count() == 0)
            {
                WarningMessagesAdd("No Member(s) To Delete");
            }
            return View("_Modal", new ModalView()
            {
                Title = "Delete All Member Policy",
                Body = this.RenderRazorViewToString("_DeleteAllMember", allMemberEndorse),
                Footer = this.GetHtmlHelper().TextBox("Delete All", "Delete All", null, new { @class = "btn btn-primary" + (WarningMessages().Count > 0 ? " disabled" : ""), @type = "submit", @onclick = (WarningMessages().Count > 0 ? " return false;" : "") }).ToString(),
                ModalForm = new ModalForm { ActionName = "DeleteAllMember/" + endorsement.Policy_Endorse.PolicyId, ControllerName = "PolicyEndorsement", RouteValues = new { endorseNumber = endorsement.EndorseNumber, endorseType = Request.Params["endorseType"] } }
            });
        }
        [HttpPost, ActionName("DeleteAllMember")]
        public ActionResult DeleteAllMemberConfirm(string endorseNumber)
        {
            if (endorseNumber == null)
            {
                return new HttpNotFoundResult();
            }
            var endorsement = db.Endorsement.Find(endorseNumber);
            if (endorsement == null)
            {
                return new HttpNotFoundResult();
            }


            if (WarningMessages().Count == 0)
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {


                        foreach (var item in endorsement.Endorsement1.Where(x => x.EndorseStatus == EndorseStatus.New).ToList())
                        {
                            //Delete All Data From Table Relate To Endorse
                            // 1. Delete Policy_Endorse
                            if (item.Policy_Endorse != null)
                            {
                                db.Policy_Endorse.Remove(item.Policy_Endorse);
                            }
                            // 2. Delete PCF_Endorse
                            if (item.PCF_Endorse != null)
                            {
                                db.PCF_Endorse.RemoveRange(item.PCF_Endorse);
                            }
                            // 3. Delete PlanDetail_Endorse
                            if (item.PlanDetail_Endorse != null)
                            {
                                db.PlanDetail_Endorse.RemoveRange(item.PlanDetail_Endorse);
                            }
                            // 4. Delete PlanDetail
                            if (item.Plan_Endorse != null)
                            {
                                db.Plan_Endorse.RemoveRange(item.Plan_Endorse);
                            }
                            // 7. Delete MemberPlan_Endorse
                            if (item.MemberPlan_Endorse != null)
                            {
                                db.MemberPlan_Endorse.RemoveRange(item.MemberPlan_Endorse);
                            }
                            // 8. Delete MemberClientEndorse
                            var allDataMemberEndorse = db.MemberClientEndorse.Where(x => x.Member_Endorse.EndorseNumber == item.EndorseNumber);
                            if (allDataMemberEndorse.Count() > 0)
                            {
                                db.MemberClientEndorse.RemoveRange(allDataMemberEndorse);
                            }

                            // 5. Delete Member_Endorse
                            if (item.Member_Endorse != null)
                            {
                                foreach (var itemMemberEndorse in item.Member_Endorse)
                                {
                                    // updating memberstatus of member master to be active
                                    var member = db.Member.Where(x => x.MemberNumber == itemMemberEndorse.MemberNumber).FirstOrDefault();
                                    if (member != null)
                                    {
                                        member.SetPropertyUpdate();
                                        member.MemberStatus = MemberStatus.Active;
                                        db.Entry(member).State = EntityState.Modified;
                                    }
                                }
                                db.Member_Endorse.RemoveRange(item.Member_Endorse);
                            }
                            // 6. Delete Endorse
                            if (item != null)
                            {
                                db.Endorsement.Remove(item);

                            }
                            db.SaveChanges();
                        }
                        var allMember = db.Member_Endorse.Where(x => x.PolicyId == endorsement.PolicyId && x.EndorseNumber == endorsement.EndorseNumber).ToList();
                        foreach (var item in allMember)
                        {
                            db.PCF_Endorse.RemoveRange(item.PCF_Endorse.ToList());
                            db.MemberPlan_Endorse.RemoveRange(item.MemberPlan_Endorse.ToList());

                            var member = db.Member.Where(x => x.MemberNumber == item.MemberNumber).FirstOrDefault();
                            if (member != null)
                            {
                                member.SetPropertyUpdate();
                                member.MemberStatus = MemberStatus.Active;
                                db.Entry(member).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            // 8. Delete MemberClientEndorse
                            var allDataMemberEndorse = db.MemberClientEndorse.Where(x => x.Member_Endorse.EndorseNumber == item.EndorseNumber);
                            if (allDataMemberEndorse.Count() > 0)
                            {
                                db.MemberClientEndorse.RemoveRange(allDataMemberEndorse);
                            }
                        }

                        db.Member_Endorse.RemoveRange(allMember);
                        db.SaveChanges();
                        dbTransaction.Commit();
                        SuccessMessagesAdd(Message.DeleteSuccess);
                        return RedirectToAction("Details", new { id = endorseNumber, tab = "member", endorseType = Request.Params["endorseType"] });

                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        WarningMessagesAdd(e.MessageToList());
                    }
                }

            }
            if (endorseNumber == null)
            {
                return new HttpNotFoundResult();
            }
            if (endorsement == null)
            {
                return new HttpNotFoundResult();
            }

            var allMemberEndorse = db.Member_Endorse.Where(x => x.EndorseNumber == endorseNumber);
            return RedirectToAction("Details", new { id = endorseNumber, tab = "member" });
        }
        public ActionResult DetailsPlan(string id, string endorseNumber)
        {
            if (id == null || endorseNumber == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plan_Endorse plan = db.Plan_Endorse.Where(x => x.PlanId == id && x.EndorseNumber == endorseNumber).FirstOrDefault();
            if (plan == null)
            {
                return HttpNotFound();
            }
            plan.Policy.PaymentFrequency = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == plan.Policy.PaymentFrequency).FirstOrDefault() != null ? db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").Where(x => x.Value == plan.Policy.PaymentFrequency).FirstOrDefault().Text : plan.Policy.PaymentFrequency;
            return View(plan);
        }
        public ActionResult DetailsMember(long? id, string endorseNumber)
        {
            //var BasicProductLimitCode = db.PCF.Where(x => x.BasicProductId == bpId && x.MemberId == memberId).ToList();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var endorsement = db.Endorsement.Find(endorseNumber);
            if (endorsement == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var member = db.Member_Endorse.Where(x => x.PolicyId == endorsement.PolicyId && x.EndorseNumber == endorseNumber && x.MemberId == id).FirstOrDefault();
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
                var newBenefit = new Benefit();
                newBenefit.BenefitCode = item.BenefitCode;
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
        // GET: Member/EditMemberBenefits/5
        public ActionResult EditMemberBenefits(long? id, string bpId, string bpCode)
        {
            var selectedBenefit = db.BasicProductLimit.Where(x => x.BasicProductId == bpId && x.BasicProductLimitCode == bpCode).ToList();
            var allActiveBenefit = db.Benefit.Where(x => x.IsActive == 1).ToList();
            var tempBenefitList = new List<Benefit>();

            foreach (var item in allActiveBenefit)
            {

                var newBenefit = new Benefit();
                newBenefit.BenefitCode = item.BenefitCode;
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
        public ActionResult Delete(string id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Endorsement endorsement = db.Endorsement.Find(id);
            if (endorsement == null)
            {
                return HttpNotFound();
            }
            var endorseType = Request.Params["endorseType"];
            if (endorseType == null)
            {
                return HttpNotFound();
            }
            return View("_Modal", new ModalView()
            {
                Title = "Endorsement Additional Member Delete",
                Body = this.RenderRazorViewToString("Delete", endorsement),
                Footer = this.GetHtmlHelper().TextBox("Delete", "Delete", null, new { @class = "btn btn-primary", @style = "background-color:#008CBA; color:white;", @type = "submit" }).ToString(),
                ModalForm = new ModalForm()
                {
                    ActionName = "Delete",
                    ControllerName = "PolicyEndorsement",
                    RouteValues = new { id = endorsement.EndorseNumber, endorseType = endorseType }
                }
            });
        }
        // POST: Member/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteEndorsementConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Endorsement endorsement = db.Endorsement.Find(id);
            if (endorsement == null)
            {
                return HttpNotFound();
            }
            var endorseType = Request.Params["endorseType"];

            if (endorseType == null)
            {
                return HttpNotFound();
            }
            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Update Policy To Active
                    var policy = endorsement.Policy;
                    policy.PolicyStatus = PolicyStatus.Active;
                    policy.SetPropertyUpdate();
                    db.Entry(policy).State = EntityState.Modified;

                    //Delete All Data From Table Relate To Endorse
                    // 1. Delete Policy_Endorse
                    if (endorsement.Policy_Endorse != null)
                    {
                        db.Policy_Endorse.Remove(endorsement.Policy_Endorse);
                    }
                    // 2. Delete PCF_Endorse
                    if (endorsement.PCF_Endorse != null)
                    {
                        db.PCF_Endorse.RemoveRange(endorsement.PCF_Endorse);
                    }
                    // 3. Delete PlanDetail_Endorse
                    if (endorsement.PlanDetail_Endorse != null)
                    {
                        db.PlanDetail_Endorse.RemoveRange(endorsement.PlanDetail_Endorse);
                    }
                    // 4. Delete PlanDetail
                    if (endorsement.Plan_Endorse != null)
                    {
                        db.Plan_Endorse.RemoveRange(endorsement.Plan_Endorse);
                    }
                    // 7. Delete MemberPlan_Endorse
                    if (endorsement.MemberPlan_Endorse != null)
                    {
                        db.MemberPlan_Endorse.RemoveRange(endorsement.MemberPlan_Endorse);
                    }
                    // 5. Delete Member_Endorse
                    if (endorsement.Member_Endorse != null)
                    {
                        foreach (var item in endorsement.Member_Endorse)
                        {
                            var member = db.Member.Where(x => x.MemberNumber == item.MemberNumber).FirstOrDefault();
                            var sameMembersInMemberEndorse = db.Member_Endorse.Where(x => x.MemberNumber == item.MemberNumber).Select(x => x.MemberNumber);
                            if (member != null && member.MemberStatus == MemberStatus.Endorse && sameMembersInMemberEndorse.Count() == 1)
                            {
                                member.MemberStatus = MemberStatus.Active;
                                db.Entry(member).State = EntityState.Modified;
                            }
                        }
                        db.Member_Endorse.RemoveRange(endorsement.Member_Endorse);
                    }
                    // 6. Delete Endorse
                    if (endorsement != null)
                    {
                        db.Endorsement.Remove(endorsement);
                    }



                    db.SaveChanges();
                    dbTransaction.Commit();
                    SuccessMessagesAdd(Message.DeleteSuccess);
                    return RedirectToAction(endorseType);
                }
                catch (Exception e)
                {
                    dbTransaction.Rollback();
                    WarningMessagesAdd(Message.DeleteFail);
                    WarningMessagesAdd(e.MessageToList());

                    return RedirectToAction(endorseType);
                }
            }

        }
        // GET: Member/Delete/5
        public ActionResult DeleteMember(long? id)
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
        [HttpPost, ActionName("DeleteMember")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMemberConfirmed(long? id)
        {
            Member member = db.Member.Find(id);
            db.Member.Remove(member);
            db.SaveChanges();
            SuccessMessagesAdd("Deleting Data Success");
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult UploadMember(HttpPostedFileBase file)
        {
            // 1. Validasi File dan data
            // 2. Simpan ke member dan client
            var policyId = Request.Params["PolicyId"];
            var endorsementNumber = Request.Params["EndorseNumber"];
            if (policyId == null || endorsementNumber == null)
            {
                return RedirectToAction("AdditionalMember");
            }
            var policy = db.Policy.Find(policyId);
            if (policy == null)
            {
                WarningMessagesAdd("Policy is not found");
                return RedirectToAction("AdditionalMember");
            }
            var endorsement = db.Endorsement.Find(endorsementNumber);
            if (endorsement == null)
            {
                WarningMessagesAdd("Endorsement is not found");
                return RedirectToAction("AdditionalMember");
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
                        var newUploadMember = new UploadMember();
                        newUploadMember.A_No = Convert.ToString(workSheet.Cells[rowCount, 1].Value);
                        newUploadMember.B_TypeOfChanges = Convert.ToString(workSheet.Cells[rowCount, 2].Value);
                        newUploadMember.C_ParticipantNo = Convert.ToString(workSheet.Cells[rowCount, 3].Value);
                        newUploadMember.D_EmployeeNo = Convert.ToString(workSheet.Cells[rowCount, 4].Value);
                        newUploadMember.E_Company = Convert.ToString(workSheet.Cells[rowCount, 5].Value);
                        newUploadMember.F_ParticipantName = Convert.ToString(workSheet.Cells[rowCount, 6].Value);
                        newUploadMember.G_EmployeeName = Convert.ToString(workSheet.Cells[rowCount, 7].Value);
                        newUploadMember.H_StatusRelation = Convert.ToString(workSheet.Cells[rowCount, 8].Value);
                        newUploadMember.I_SorM = Convert.ToString(workSheet.Cells[rowCount, 9].Value);
                        newUploadMember.J_DateOFBirth = Convert.ToString(workSheet.Cells[rowCount, 10].Value);
                        newUploadMember.K_Gender = Convert.ToString(workSheet.Cells[rowCount, 11].Value);
                        newUploadMember.L_Benefit = Convert.ToString(workSheet.Cells[rowCount, 12].Value);
                        newUploadMember.M_EffectiveDate = Convert.ToString(workSheet.Cells[rowCount, 13].Value);
                        newUploadMember.N_EndDate = Convert.ToString(workSheet.Cells[rowCount, 14].Value);
                        newUploadMember.O_Reason = Convert.ToString(workSheet.Cells[rowCount, 15].Value);
                        newUploadMember.P_Contact = Convert.ToString(workSheet.Cells[rowCount, 16].Value);
                        newUploadMember.Q_BankAccountName = Convert.ToString(workSheet.Cells[rowCount, 17].Value);
                        newUploadMember.R_BankCode = Convert.ToString(workSheet.Cells[rowCount, 18].Value);
                        newUploadMember.S_BankAccountNo = Convert.ToString(workSheet.Cells[rowCount, 19].Value);
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
                using (var dbTransaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                {
                    try
                    {
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
                                        newClientToInsert.ClientRelation = ClientRelation.Daughter;
                                    }
                                }
                                newClientToInsert.FullName = item.F_ParticipantName;
                                newClientToInsert.MobilePhone1 = item.P_Contact;
                                newClientToInsert.Sex = item.K_Gender == "M" ? Sex.Male : item.K_Gender == "F" ? Sex.Female : null;
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
                                }
                                newClientToInsert.FullName = item.F_ParticipantName;
                                newClientToInsert.MobilePhone1 = item.P_Contact;
                                newClientToInsert.Sex = item.K_Gender == "M" ? "Male" : item.K_Gender == "F" ? "Female" : null;
                                newClientToInsert.MaritalStatus = item.I_SorM == "M" ? "Married" : item.I_SorM == "S" ? "Single" : null;
                                newClientToInsert.Type = "Personal";
                                newClientToInsert.ClientId = (lastClientIdSequence + 1).ToString().PadLeft(10, '0');
                                //var clientRelateTo = listClientToInsert.Where(x => x.FullName == item.F_ParticipantName).FirstOrDefault();
                                var clientRelateToMember = db.Client.Where(x => x.FullName == item.G_EmployeeName && x.BankAccountNumber == item.S_BankAccountNo).FirstOrDefault();
                                var clientRelateTo = listClientToInsertWithAnotherProperty.Where(x => x.uploadMember.G_EmployeeName == item.G_EmployeeName && x.uploadMember.H_StatusRelation == "E" && x.client.BankAccountName == item.Q_BankAccountName && x.client.BankAccountNumber == item.S_BankAccountNo).FirstOrDefault();
                                if (clientRelateTo.client != null || clientRelateToMember != null)
                                {
                                    newClientToInsert.RelateTo = clientRelateToMember != null ? clientRelateToMember?.ClientId : clientRelateTo.client?.ClientId.PadLeft(10, '0');
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

                                            var memberExist = db.Member.Where(x => x.ClientId == client.ClientId).FirstOrDefault();
                                            var memberEndorseExist = db.Member_Endorse.Where(x => x.ClientId == client.ClientId).FirstOrDefault();
                                            if (memberExist == null && memberEndorseExist == null)
                                            {
                                                var newMember = new Member_Endorse();
                                                newMember.EndorseNumber = endorsement.EndorseNumber;
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



                                                db.Member_Endorse.Add(newMember);
                                                db.SaveChanges();
                                                // set sequencial number


                                                //newMember.Client = db.Client.Find(newMember.ClientId);

                                                if (newMember.Client.RelateTo != null)
                                                {
                                                    var allMemberRelateToSameClient = db.Member.Where(x => x.Client.RelateTo == newMember.Client.RelateTo && x.MemberId != newMember.MemberId).ToList().Select(x => x.SequencialNo);
                                                    var allMemberEndorseRelateToSameClient = db.Member_Endorse.Where(x => x.Client.RelateTo == newMember.Client.RelateTo && x.MemberId != newMember.MemberId).ToList().Select(x => x.SequencialNo);
                                                    var allMemberRealateTo = new List<int?>();
                                                    allMemberRealateTo.AddRange(allMemberRelateToSameClient);
                                                    allMemberRealateTo.AddRange(allMemberEndorseRelateToSameClient);

                                                    newMember.SequencialNo = allMemberRealateTo != null && allMemberRealateTo.Count > 0 ? allMemberRealateTo.Max(x => x) + 1 : 1;
                                                }
                                                db.Entry(newMember).State = EntityState.Modified;
                                                db.SaveChanges();

                                                //indentify Plan
                                                var listOfBenefitThisItem = plan.Replace(" ", "").ToLower().Split(',').ToList();

                                                var planOfThisPolicy = db.PlanDetail_Endorse.Where(x => x.PolicyId == policyId).ToList();
                                                var isHasPlan = false;
                                                int counter = 0;
                                                var distictPlanId = planOfThisPolicy.Select(x => new { x.PlanId }).Distinct();
                                                foreach (var itemInPlanOfThisPolicy in distictPlanId)
                                                {
                                                    var planDetailList = db.PlanDetail_Endorse.Where(x => x.PlanId == itemInPlanOfThisPolicy.PlanId && x.PolicyId == policyId).ToList();
                                                    var plantDetailListSelect = planDetailList.Select(x => x.BasicProductLimitCode.ToLower()).ToList();



                                                    isHasPlan = listOfBenefitThisItem.All(plantDetailListSelect.Contains) && listOfBenefitThisItem.Count == plantDetailListSelect.Count;


                                                    if (isHasPlan)
                                                    {
                                                        newMember.PlanId = itemInPlanOfThisPolicy.PlanId;

                                                        foreach (var itemInPlanDetails in planDetailList)
                                                        {
                                                            var newMemberPlan = new MemberPlan_Endorse();
                                                            newMemberPlan.EndorseNumber = endorsement.EndorseNumber;
                                                            newMemberPlan.PolicyId = newMember.PolicyId;
                                                            newMemberPlan.MemberId = newMember.MemberId;
                                                            newMemberPlan.PlanId = newMember.PlanId;
                                                            newMemberPlan.BasicProductId = itemInPlanDetails.BasicProductId;
                                                            newMemberPlan.BasicProductLimitCode = itemInPlanDetails.BasicProductLimitCode;
                                                            newMemberPlan.SetPropertyCreate();
                                                            db.MemberPlan_Endorse.Add(newMemberPlan);
                                                        }
                                                        db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        counter++;
                                                    }


                                                }
                                                if (counter == distictPlanId.Count())
                                                {
                                                    WarningMessagesAdd("Plan [" + plan.Replace(" ", "") + "] for " + client.FullName + " is not found.");
                                                }

                                            }
                                            else
                                            {
                                                WarningMessagesAdd("Member " + client.FullName + ", with Policy Holder " + memberExist != null ? memberExist.Policy.Client.FullName : memberEndorseExist.Policy.Client.FullName + " already exist");
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
            return RedirectToAction("Details", "PolicyEndorsement", new { id = endorsement.EndorseNumber, tab = "member" });
        }
        public ActionResult MemberDetails(long? id)
        {
            //var BasicProductLimitCode = db.PCF.Where(x => x.BasicProductId == bpId && x.MemberId == memberId).ToList();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var member = db.Member_Endorse.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            if (Request.IsAjaxRequest())
            {
                ViewBag.LayoutIsNull = true;
                return View("_DetailsMember", member);
            }
            var commonListValue = db.CommonListValue.Where(x => x.CommonListValue2.Value == "Frequency").ToList();
            member.Endorsement.Policy_Endorse.PaymentFrequency = commonListValue.Where(x => x.Value == member.Endorsement.Policy_Endorse.PaymentFrequency).FirstOrDefault()?.Text;
            return View(member);
        }
        public ActionResult Calculate(string id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var endorsement = db.Endorsement.Find(id);
            if (endorsement == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Request.IsAjaxRequest())
            {
                decimal sumOfPremi = 0;
                var allMember = db.Member_Endorse.Where(x => x.PolicyId == endorsement.PolicyId && x.EndorseNumber == endorsement.EndorseNumber && x.StartDate != null & x.EndDate != null).ToList();
                if (endorsement.EndorseType == EndorseType.TransitionData)
                {
                    foreach (var item in allMember)
                    {
                        sumOfPremi = sumOfPremi + this.CalculateMemberPremiMemberTransitionData(item);
                    }
                    //using (var dbTransactionNew = db.Database.BeginTransaction())
                    //{
                    //    try
                    //    {
                    //        var messageSqlParameter = new SqlParameter().SqlParamterForOutputMessage();
                    //        sumOfPremi = db.Database.SqlQuery<decimal>("exec [spCalculateDataTransition] @endorseNumber, @message out", new SqlParameter("endorseNumber", endorsement.EndorseNumber), messageSqlParameter).FirstOrDefault();
                    //        if (messageSqlParameter.MessageToList() == null)
                    //        {
                    //            dbTransactionNew.Commit();
                    //        }
                    //        else
                    //        {
                    //            dbTransactionNew.Rollback();
                    //            WarningMessagesAdd(messageSqlParameter.MessageToList());
                    //        }
                    //        //dbTransactionNew.Rollback();
                    //        //WarningMessagesAdd(messageSqlParameter.MessageToList());
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        dbTransactionNew.Rollback();
                    //        WarningMessagesAdd(e.MessageToList());
                    //    }

                    //}

                    var sumOfPremiPCF = db.PCF_Endorse.Where(x => x.EndorseNumber == endorsement.EndorseNumber && x.TransactionNumber == null).Sum(x => x.Amount);
                    if (sumOfPremiPCF == null)
                    {
                        sumOfPremiPCF = 0;
                    }
                    var sumOfPrintCardFee = db.MemberClientEndorse.Where(x => x.Member_Endorse.EndorseNumber == endorsement.EndorseNumber).Sum(x => x.PrintCardAmount);
                    if (sumOfPrintCardFee == null)
                    {
                        sumOfPrintCardFee = 0;
                    }
                    var sumofDemandedPremi = sumOfPremiPCF + sumOfPrintCardFee;


                    ViewBag.Title = "Details Policy Calculation";
                    ViewBag.SumOfPremi = WarningMessages().Count == 0 ? sumOfPremi : 0;
                    ViewBag.SumOfDemandedPremi = WarningMessages().Count == 0 ? sumofDemandedPremi.Value : 0;
                    ViewBag.WarningMessage = WarningMessages().ToList();
                    return View("_CalculationResult", endorsement);
                }
                else if (endorsement.EndorseType == EndorseType.MovePlan)
                {
                    var memberMasterOfPolicy = endorsement.Policy.Member;

                    foreach (var item in allMember)
                    {
                        if (item.PlanId == memberMasterOfPolicy.Where(x => x.MemberNumber == item.MemberNumber).FirstOrDefault()?.PlanId)
                        {
                            WarningMessagesAdd(item.Client.FullName + "'s" + " Plan Is Not Changed");
                        }
                    }
                    if (WarningMessages().Count == 0)
                    {
                        foreach (var item in allMember)
                        {
                            sumOfPremi = sumOfPremi + this.CalculateMemberPremiMovePlanEndorse(item, 1, 1);

                        }
                    }
                    if (WarningMessages().Count == 0)
                    {
                        SuccessMessagesAdd("Calculation Success");
                    }
                    else
                    {
                        WarningMessages().Insert(0, "Calculation Failed");

                    }
                    ViewBag.Title = "Details Policy Calculation";
                    ViewBag.SumOfPremi = WarningMessages().Count == 0 ? sumOfPremi : 0;
                    ViewBag.SumOfDemandedPremi = WarningMessages().Count == 0 ? sumOfPremi : 0;
                    ViewBag.WarningMessage = WarningMessages().ToList();
                    return View("_CalculationResult", endorsement);
                }
                else if (endorsement.EndorseType == EndorseType.TerminateMember)
                {
                    //1. Apabila status member adalah Canceled, maka generate data PCF sebagai reverse dari PCF yang ada. 
                    //Amount PCF baru adalah hasil kali dari amount PCF lama dikali dengan -1

                    //2. Apabila status member adalah Refund, maka hampir sama prosesnya dengan move plan.
                    // Yang membedakan adalah akan dikalikan lagi dengan faktor presentasi.
                    // Cara mendapatkan faktor presentasi adalah mendapatkan nilai dari Common List Value
                    // Refund Percentage

                    // Di pengembangan kemudian, akan digunakan claim ratio. Apabila member telah pernah melakukan claim, maka otomatis refud diberi nilai 0
                    if (allMember.Where(x => x.TerminateDate == null).FirstOrDefault() != null)
                    {
                        WarningMessagesAdd("Not All Member Have Terminate Date");
                    }

                    if (WarningMessages().Count == 0)
                    {
                        SuccessMessagesAdd("Calculation Success");
                    }
                    else
                    {
                        WarningMessages().Insert(0, "Calculation Failed");

                    }
                    if (WarningMessages().Count == 0)
                    {
                        foreach (var item in allMember)
                        {
                            sumOfPremi = sumOfPremi + this.CalculateMemberPremiTerminateMember(item, 1, 1);
                        }
                    }
                    var pcfTotalAmount2 = db.PCF_Endorse.Where(x => x.EndorseNumber == endorsement.EndorseNumber && x.PolicyId == endorsement.PolicyId && x.TransType == "P" && x.InvoiceDate < DateTime.Now).Select(x => x.Amount.Value).ToList();

                    ViewBag.Title = "Details Policy Calculation";
                    ViewBag.SumOfPremi = WarningMessages().Count == 0 ? sumOfPremi : 0;
                    ViewBag.SumOfDemandedPremi = WarningMessages().Count == 0 ? pcfTotalAmount2.Sum() : 0;
                    ViewBag.WarningMessage = WarningMessages().ToList();
                    return View("_CalculationResult", endorsement);
                }
                else if (endorsement.EndorseType == EndorseType.Mutation)
                {
                    //checking apakah semua member telah mempunyai polis target
                    var eachMemberHasPolicyTarget = false;
                    int totalOfMemberHavePolicy = 0;
                    foreach (var item in endorsement.Endorsement1)
                    {
                        totalOfMemberHavePolicy = totalOfMemberHavePolicy + item.Member_Endorse.Count;
                    }
                    if (totalOfMemberHavePolicy == endorsement.Member_Endorse.Count)
                    {
                        eachMemberHasPolicyTarget = true;
                    }
                    if (!eachMemberHasPolicyTarget)
                    {
                        WarningMessagesAdd("Each Member Has No Policy Mutation Target");
                        WarningMessages().Insert(0, Message.ProcessFail);
                    }


                    if (WarningMessages().Count == 0)
                    {
                        SuccessMessagesAdd(Message.ProcessSuccess);
                        foreach (var item in allMember)
                        {
                            sumOfPremi = sumOfPremi + this.CalculateMemberPremiMutationMember(item, 1, 1);
                        }
                    }
                    var pcfTotalAmount2 = db.PCF_Endorse.Where(x => x.EndorseNumber == endorsement.EndorseNumber && x.PolicyId == endorsement.PolicyId && x.TransType == "P" && x.InvoiceDate < DateTime.Now).Select(x => x.Amount.Value).ToList();

                    ViewBag.Title = "Details Policy Calculation";
                    ViewBag.SumOfPremi = WarningMessages().Count == 0 ? sumOfPremi : 0;
                    ViewBag.SumOfDemandedPremi = WarningMessages().Count == 0 ? pcfTotalAmount2.Sum() : 0;
                    ViewBag.WarningMessage = WarningMessages().ToList();
                    return View("_CalculationResult", endorsement);

                }
                else if (endorsement.EndorseType == EndorseType.Renewal)
                {
                    if (WarningMessages().Count == 0)
                    {
                        foreach (var item in allMember)
                        {
                            sumOfPremi = sumOfPremi + this.CalculateMemberPremiRenewalMember(item);
                        }
                    }
                    var pcfTotalAmount2 = db.PCF_Endorse.Where(x => x.EndorseNumber == endorsement.EndorseNumber && x.PolicyId == endorsement.PolicyId && x.TransType == "P" && x.InvoiceDate < DateTime.Now).Select(x => x.Amount.Value).ToList();

                    ViewBag.Title = "Details Policy Calculation";
                    ViewBag.SumOfPremi = WarningMessages().Count == 0 ? sumOfPremi : 0;
                    ViewBag.SumOfDemandedPremi = WarningMessages().Count == 0 ? pcfTotalAmount2.Sum() : 0;
                    ViewBag.WarningMessage = WarningMessages().ToList();
                    return View("_CalculationResult", endorsement);
                }
                else
                {
                    foreach (var item in allMember)
                    {
                        sumOfPremi = sumOfPremi + this.CalculateMemberPremi(item, 1, 1);
                    }
                }

                var pcfTotalAmount = db.PCF_Endorse.Where(x => x.EndorseNumber == endorsement.EndorseNumber && x.PolicyId == endorsement.PolicyId && x.TransType == "P" && x.InvoiceDate < DateTime.Now).Select(x => x.Amount.Value).ToList();

                var retval = this.Json(new
                {
                    calculateResult = sumOfPremi
                }, JsonRequestBehavior.AllowGet);
                ViewBag.Title = "Details Policy Calculation";
                ViewBag.SumOfPremi = WarningMessages().Count == 0 ? sumOfPremi : 0;
                ViewBag.SumOfDemandedPremi = WarningMessages().Count == 0 ? pcfTotalAmount.Sum() : 0;
                ViewBag.WarningMessage = WarningMessages().ToList();

                if (WarningMessages().Count == 0)
                {
                    SuccessMessagesAdd("Calculation Success");
                }
                else
                {
                    WarningMessages().Insert(0, "Calculation Failed");
                    var pcfToDelete = db.PCF_Endorse.Where(x => x.EndorseNumber == endorsement.EndorseNumber);
                    db.PCF_Endorse.RemoveRange(pcfToDelete);
                    db.SaveChanges();
                }

                return View("_CalculationResult", endorsement);

            }


            return View("_CalculationResult", endorsement);
        }
        private decimal CalculateMemberPremiMutationMember(Member_Endorse member, int coverageDuration, int paymentDuration)
        {
            decimal returnValue = 0;

            //validation
            if (WarningMessages().Count == 0)
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var listOfDecimal = new List<decimal>();

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

                        var iteration = 0;
                        for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                        {
                            iteration++;
                        }
                        var refundPercentageString = db.CommonListValue.Where(x => x.Text == "Refund Percentage").FirstOrDefault()?.Value;
                        var memberEndorsePCF = member.PCF_Endorse.Where(x => x.EndorseNumber == member.EndorseNumber);
                        var listNewEndorsePCF = new List<PCF_Endorse>();
                        var selectedPcf = memberEndorsePCF.Where(x => x.InvoiceDate < member.TerminateDate && x.TransactionNumber != null).OrderByDescending(x => x.InvoiceDate).FirstOrDefault();
                        var listPcfToRefund = new List<PCF_Endorse>();
                        listPcfToRefund.AddRange(memberEndorsePCF.Where(x => x.InvoiceDate == selectedPcf.InvoiceDate && x.TransactionNumber != null));
                        foreach (var item in memberEndorsePCF.Where(x => x.InvoiceDate > selectedPcf.InvoiceDate && x.TransactionNumber != null).OrderBy(x => x.InvoiceDate))
                        {
                            listPcfToRefund.Add(item);
                        }

                        db.PCF_Endorse.RemoveRange(memberEndorsePCF.Where(x => x.TransactionNumber == null));
                        foreach (var item in listPcfToRefund)
                        {
                            if (item.InvoiceDate <= member.TerminateDate)
                            {
                                var newPCFEndorse = new PCF_Endorse();
                                newPCFEndorse.EndorseNumber = item.EndorseNumber;
                                newPCFEndorse.PolicyId = item.PolicyId;
                                newPCFEndorse.MemberId = item.MemberId;
                                newPCFEndorse.BasicProductId = item.BasicProductId;
                                newPCFEndorse.TransType = TransactionType.Refund;
                                newPCFEndorse.InvoiceDate = item.InvoiceDate;
                                newPCFEndorse.DueDate = item.DueDate;

                                var refundPercentage = Convert.ToDecimal(refundPercentageString);
                                var amount = (item.InvoiceDate.AddMonths(frequecyToNumber) - member.TerminateDate).Value.Days * item.Amount * refundPercentage / (item.InvoiceDate.AddMonths(frequecyToNumber) - item.InvoiceDate).Days;
                                newPCFEndorse.Amount = amount * -1;
                                newPCFEndorse.SetPropertyCreate();
                                listNewEndorsePCF.Add(newPCFEndorse);

                            }
                            else
                            {
                                var newPCFEndorse = new PCF_Endorse();
                                newPCFEndorse.EndorseNumber = item.EndorseNumber;
                                newPCFEndorse.PolicyId = item.PolicyId;
                                newPCFEndorse.MemberId = item.MemberId;
                                newPCFEndorse.BasicProductId = item.BasicProductId;
                                newPCFEndorse.TransType = TransactionType.Refund;
                                newPCFEndorse.InvoiceDate = item.InvoiceDate;
                                newPCFEndorse.DueDate = item.DueDate;
                                newPCFEndorse.Amount = item.Amount * -1;
                                newPCFEndorse.SetPropertyCreate();
                                listNewEndorsePCF.Add(newPCFEndorse);
                            }

                        }

                        db.PCF_Endorse.AddRange(listNewEndorsePCF);
                        member.MemberStatus = TerminateType.Refund + " " + MemberStatus.Calculated;
                        db.Entry(member).State = EntityState.Modified;
                        db.SaveChanges();
                        memberEndorsePCF = db.PCF_Endorse.Where(x => x.EndorseNumber == member.EndorseNumber && x.PolicyId == member.PolicyId && x.MemberId == member.MemberId && x.TransactionNumber == null).ToList();
                        foreach (var item in memberEndorsePCF)
                        {
                            listOfDecimal.Add(item.Amount.Value);
                        }

                        dbTransaction.Commit();
                        returnValue = listOfDecimal.Sum(x => x);

                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        WarningMessagesAdd(e.MessageToList());
                    }
                }
            }
            return returnValue;
        }
        private decimal CalculateMemberPremiTerminateMember(Member_Endorse member, int coverageDuration, int paymentDuration)
        {
            //1. Apabila status member adalah Canceled, maka generate data PCF sebagai reverse dari PCF yang ada. 
            //Amount PCF baru adalah hasil kali dari amount PCF lama dikali dengan -1

            //2. Apabila status member adalah Refund, maka hampir sama prosesnya dengan move plan.
            // Yang membedakan adalah akan dikalikan lagi dengan faktor persentasi.
            // Cara mendapatkan faktor persentasi adalah mendapatkan nilai dari Common List Value
            // Refund Percentage

            // Di pengembangan kemudian, akan digunakan claim ratio. Apabila member telah pernah melakukan claim, maka otomatis refud diberi nilai 0

            if (member.MemberStatus == TerminateType.Cancel || member.MemberStatus == TerminateType.Cancel + " " + MemberStatus.Calculated)
            {
                var listOfDecimal = new List<decimal>();
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        //1. Create PCF Reverse
                        var memberEndorsePCF = member.PCF_Endorse.Where(x => x.EndorseNumber == member.EndorseNumber);
                        var listNewEndorsePCF = new List<PCF_Endorse>();
                        db.PCF_Endorse.RemoveRange(memberEndorsePCF.Where(x => x.TransactionNumber == null));
                        db.SaveChanges();
                        foreach (var item in memberEndorsePCF.Where(x => x.TransactionNumber != null))
                        {
                            var newPCFEndorse = new PCF_Endorse();
                            newPCFEndorse.EndorseNumber = item.EndorseNumber;
                            newPCFEndorse.PolicyId = item.PolicyId;
                            newPCFEndorse.MemberId = item.MemberId;
                            newPCFEndorse.BasicProductId = item.BasicProductId;
                            newPCFEndorse.TransType = item.TransType;
                            newPCFEndorse.InvoiceDate = item.Endorsement.EndorseDate.Value;
                            newPCFEndorse.DueDate = item.DueDate;
                            newPCFEndorse.Amount = item.Amount * -1;
                            newPCFEndorse.SetPropertyCreate();
                            listNewEndorsePCF.Add(newPCFEndorse);
                        }
                        db.PCF_Endorse.AddRange(listNewEndorsePCF);
                        member.MemberStatus = TerminateType.Cancel + " " + MemberStatus.Calculated;
                        db.Entry(member).State = EntityState.Modified;
                        db.SaveChanges();
                        dbTransaction.Commit();
                        memberEndorsePCF = db.PCF_Endorse.Where(x => x.EndorseNumber == member.EndorseNumber && x.PolicyId == member.PolicyId && x.MemberId == member.MemberId && x.TransactionNumber == null).ToList();
                        foreach (var item in memberEndorsePCF)
                        {
                            listOfDecimal.Add(item.Amount.Value);
                        }
                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        WarningMessagesAdd(e.MessageToList());
                    }
                    return listOfDecimal.Sum(x => x);
                }
            }
            else if (member.MemberStatus == TerminateType.Refund || member.MemberStatus == TerminateType.Refund + " " + MemberStatus.Calculated)
            {
                var listOfDecimal = new List<decimal>();
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
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

                        var iteration = 0;
                        for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                        {
                            iteration++;
                        }
                        var refundPercentageString = db.CommonListValue.Where(x => x.Text == "Refund Percentage").FirstOrDefault()?.Value;
                        var memberEndorsePCF = member.PCF_Endorse.Where(x => x.EndorseNumber == member.EndorseNumber);
                        var listNewEndorsePCF = new List<PCF_Endorse>();
                        var selectedPcf = memberEndorsePCF.Where(x => x.InvoiceDate < member.TerminateDate && x.TransactionNumber != null).OrderByDescending(x => x.InvoiceDate).FirstOrDefault();
                        var listPcfToRefund = new List<PCF_Endorse>();
                        listPcfToRefund.AddRange(memberEndorsePCF.Where(x => x.InvoiceDate == selectedPcf.InvoiceDate && x.TransactionNumber != null));
                        foreach (var item in memberEndorsePCF.Where(x => x.InvoiceDate > selectedPcf.InvoiceDate && x.TransactionNumber != null).OrderBy(x => x.InvoiceDate))
                        {
                            listPcfToRefund.Add(item);
                        }

                        db.PCF_Endorse.RemoveRange(memberEndorsePCF.Where(x => x.TransactionNumber == null));
                        foreach (var item in listPcfToRefund)
                        {
                            if (item.InvoiceDate <= member.TerminateDate)
                            {
                                var newPCFEndorse = new PCF_Endorse();
                                newPCFEndorse.EndorseNumber = item.EndorseNumber;
                                newPCFEndorse.PolicyId = item.PolicyId;
                                newPCFEndorse.MemberId = item.MemberId;
                                newPCFEndorse.BasicProductId = item.BasicProductId;
                                newPCFEndorse.TransType = TransactionType.Refund;
                                newPCFEndorse.InvoiceDate = item.InvoiceDate;
                                newPCFEndorse.DueDate = item.DueDate;

                                var refundPercentage = Convert.ToDecimal(refundPercentageString);
                                var amount = (item.InvoiceDate.AddMonths(frequecyToNumber) - member.TerminateDate).Value.Days * item.Amount * refundPercentage / (item.InvoiceDate.AddMonths(frequecyToNumber) - item.InvoiceDate).Days;
                                newPCFEndorse.Amount = amount * -1;
                                newPCFEndorse.SetPropertyCreate();
                                listNewEndorsePCF.Add(newPCFEndorse);

                            }
                            else
                            {
                                var newPCFEndorse = new PCF_Endorse();
                                newPCFEndorse.EndorseNumber = item.EndorseNumber;
                                newPCFEndorse.PolicyId = item.PolicyId;
                                newPCFEndorse.MemberId = item.MemberId;
                                newPCFEndorse.BasicProductId = item.BasicProductId;
                                newPCFEndorse.TransType = TransactionType.Refund;
                                newPCFEndorse.InvoiceDate = item.InvoiceDate;
                                newPCFEndorse.DueDate = item.DueDate;
                                newPCFEndorse.Amount = item.Amount * -1;
                                newPCFEndorse.SetPropertyCreate();
                                listNewEndorsePCF.Add(newPCFEndorse);
                            }

                        }

                        db.PCF_Endorse.AddRange(listNewEndorsePCF);
                        member.MemberStatus = TerminateType.Refund + " " + MemberStatus.Calculated;
                        db.Entry(member).State = EntityState.Modified;
                        db.SaveChanges();
                        dbTransaction.Commit();
                        memberEndorsePCF = db.PCF_Endorse.Where(x => x.EndorseNumber == member.EndorseNumber && x.PolicyId == member.PolicyId && x.MemberId == member.MemberId && x.TransactionNumber == null).ToList();
                        foreach (var item in memberEndorsePCF)
                        {
                            listOfDecimal.Add(item.Amount.Value);
                        }
                    }
                    catch (Exception e)
                    {
                        WarningMessagesAdd(e.MessageToList());
                        dbTransaction.Rollback();
                    }

                }
                return listOfDecimal.Sum(x => x);
            }
            else if (member.MemberStatus == TerminateType.Death || member.MemberStatus == TerminateType.Death + " " + MemberStatus.Calculated)
            {
                var listOfDecimal = new List<decimal>();
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
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

                        var iteration = 0;
                        for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                        {
                            iteration++;
                        }
                        var refundPercentageString = 0;
                        var memberEndorsePCF = member.PCF_Endorse.Where(x => x.EndorseNumber == member.EndorseNumber);
                        var listNewEndorsePCF = new List<PCF_Endorse>();
                        var selectedPcf = memberEndorsePCF.Where(x => x.InvoiceDate < member.TerminateDate && x.TransactionNumber != null).OrderByDescending(x => x.InvoiceDate).FirstOrDefault();
                        var listPcfToRefund = new List<PCF_Endorse>();
                        listPcfToRefund.AddRange(memberEndorsePCF.Where(x => x.InvoiceDate == selectedPcf.InvoiceDate && x.TransactionNumber != null));
                        foreach (var item in memberEndorsePCF.Where(x => x.InvoiceDate > selectedPcf.InvoiceDate && x.TransactionNumber != null).OrderBy(x => x.InvoiceDate))
                        {
                            listPcfToRefund.Add(item);
                        }

                        db.PCF_Endorse.RemoveRange(memberEndorsePCF.Where(x => x.TransactionNumber == null));
                        foreach (var item in listPcfToRefund)
                        {
                            if (item.InvoiceDate <= member.TerminateDate)
                            {
                                var newPCFEndorse = new PCF_Endorse();
                                newPCFEndorse.EndorseNumber = item.EndorseNumber;
                                newPCFEndorse.PolicyId = item.PolicyId;
                                newPCFEndorse.MemberId = item.MemberId;
                                newPCFEndorse.BasicProductId = item.BasicProductId;
                                newPCFEndorse.TransType = TransactionType.Refund;
                                newPCFEndorse.InvoiceDate = item.InvoiceDate;
                                newPCFEndorse.DueDate = item.DueDate;

                                var refundPercentage = Convert.ToDecimal(refundPercentageString);
                                var amount = (item.InvoiceDate.AddMonths(frequecyToNumber) - member.TerminateDate).Value.Days * item.Amount * refundPercentage / (item.InvoiceDate.AddMonths(frequecyToNumber) - item.InvoiceDate).Days;
                                newPCFEndorse.Amount = amount * -1;
                                newPCFEndorse.SetPropertyCreate();
                                listNewEndorsePCF.Add(newPCFEndorse);

                            }
                            else
                            {
                                var newPCFEndorse = new PCF_Endorse();
                                newPCFEndorse.EndorseNumber = item.EndorseNumber;
                                newPCFEndorse.PolicyId = item.PolicyId;
                                newPCFEndorse.MemberId = item.MemberId;
                                newPCFEndorse.BasicProductId = item.BasicProductId;
                                newPCFEndorse.TransType = TransactionType.Refund;
                                newPCFEndorse.InvoiceDate = item.InvoiceDate;
                                newPCFEndorse.DueDate = item.DueDate;
                                newPCFEndorse.Amount = item.Amount * -1;
                                newPCFEndorse.SetPropertyCreate();
                                listNewEndorsePCF.Add(newPCFEndorse);
                            }

                        }

                        db.PCF_Endorse.AddRange(listNewEndorsePCF);
                        member.MemberStatus = TerminateType.Refund + " " + MemberStatus.Calculated;
                        db.Entry(member).State = EntityState.Modified;
                        db.SaveChanges();
                        dbTransaction.Commit();
                        memberEndorsePCF = db.PCF_Endorse.Where(x => x.EndorseNumber == member.EndorseNumber && x.PolicyId == member.PolicyId && x.MemberId == member.MemberId && x.TransactionNumber == null).ToList();
                        foreach (var item in memberEndorsePCF)
                        {
                            listOfDecimal.Add(item.Amount.Value);
                        }
                    }
                    catch (Exception e)
                    {
                        WarningMessagesAdd(e.MessageToList());
                        dbTransaction.Rollback();
                    }

                }
                return listOfDecimal.Sum(x => x);
            }
            return 0;

        }
        private decimal CalculateMemberPremiMovePlanEndorse(Member_Endorse member, int coverageDuration, int paymentDuration)
        {
            var listOfDecimal = new List<Decimal>();
            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    //< Menghapus semua PCF Endorse dari member
                    db.PCF_Endorse.RemoveRange(db.PCF_Endorse.Where(x => x.EndorseNumber == member.EndorseNumber && x.MemberId == member.MemberId).ToList());
                    db.SaveChanges();
                    //>

                    //< menyalis dari PCF master ke PCF Endorse
                    var memberPCFMaster = db.PCF.Where(x => x.PolicyId == member.PolicyId && x.Member.MemberNumber == member.MemberNumber).ToList();
                    foreach (var itemMemberPcf in memberPCFMaster)
                    {
                        var pcfEndorse = new PCF_Endorse();
                        pcfEndorse.MemberId = member.MemberId;
                        pcfEndorse.BasicProductId = itemMemberPcf.BasicProductId;
                        pcfEndorse.DueDate = itemMemberPcf.DueDate;
                        pcfEndorse.EndorseNumber = member.EndorseNumber;
                        pcfEndorse.InvoiceDate = itemMemberPcf.InvoiceDate;
                        pcfEndorse.PolicyId = itemMemberPcf.PolicyId;
                        pcfEndorse.TransType = itemMemberPcf.TransType;
                        pcfEndorse.Amount = itemMemberPcf.Amount;
                        pcfEndorse.TransactionNumber = itemMemberPcf.TransactionNumber;
                        pcfEndorse.SetPropertyCreate();
                        db.PCF_Endorse.Add(pcfEndorse);
                    }
                    //>
                    db.SaveChanges();
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
                    var lengthOfBenefitDayPerYear = (member.EndDate - member.Endorsement.EndorseDate).Value.TotalDays / 365.25;
                    var lengthOfBenefitDayPerYearTermLife = (member.EndDate - member.StartDate).Value.TotalDays / 365.25;

                    var memberPlan = db.MemberPlan_Endorse.Where(x => x.MemberId == member.MemberId && x.PolicyId == member.PolicyId && x.EndorseNumber == member.EndorseNumber && x.IsActive == 1).ToList();

                    db.PCF_Endorse.RemoveRange(db.PCF_Endorse.Where(x => x.MemberId == member.MemberId && x.PolicyId == member.PolicyId && x.EndorseNumber == member.EndorseNumber && x.InvoiceDate == null).ToList());
                    var pcfOld = db.PCF_Endorse.Where(x => x.MemberId == member.MemberId && x.PolicyId == member.PolicyId && x.EndorseNumber == member.EndorseNumber && x.InvoiceDate != null).ToList();
                    db.SaveChanges();

                    var frequecyToNumber = 0;
                    decimal multiplierFactorPercentage = new decimal(0);


                    if (member.Policy.PaymentFrequency == PaymentFrequency.Monthly)
                    {
                        frequecyToNumber = 1;
                        multiplierFactorPercentage = decimal.Multiply(decimal.Divide(10, 100), 12);
                    }
                    else if (member.Policy.PaymentFrequency == PaymentFrequency.Quarterly)
                    {
                        frequecyToNumber = 4;
                        multiplierFactorPercentage = decimal.Multiply(decimal.Divide(27, 100), 4);
                    }
                    else if (member.Policy.PaymentFrequency == PaymentFrequency.Semesterly)
                    {
                        frequecyToNumber = 6;
                        multiplierFactorPercentage = decimal.Multiply(decimal.Divide(52, 100), 2);
                    }
                    else if (member.Policy.PaymentFrequency == PaymentFrequency.Yearly)
                    {
                        frequecyToNumber = 12;
                        multiplierFactorPercentage = decimal.Multiply(decimal.Divide(100, 100), 1);
                    }

                    var iteration = 0;
                    for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                    {
                        iteration++;
                    }

                    foreach (var item in pcfOld)
                    {
                        var newPCF = new PCF_Endorse();
                        newPCF.EndorseNumber = member.EndorseNumber;
                        newPCF.PolicyId = item.PolicyId;
                        newPCF.BasicProductId = item.BasicProductId;
                        newPCF.Amount = new Decimal((member.EndDate - member.Endorsement.EndorseDate).Value.TotalDays / (member.EndDate - member.StartDate).Value.TotalDays) * item.Amount * -1;
                        newPCF.MemberId = item.MemberId;
                        newPCF.TransType = "R";
                        newPCF.InvoiceDate = item.Endorsement.EndorseDate.Value;
                        newPCF.SetPropertyCreate();
                        db.PCF_Endorse.Add(newPCF);


                        //var memberMaster = db.Member.Where(x => x.MemberNumber == item.Member_Endorse.MemberNumber).FirstOrDefault();
                        //var memberMasterMemberPlan = memberMaster.MemberPlan.Where(x => x.BasicProductId == item.BasicProductId).FirstOrDefault();
                        //string sql = "select prd.PremiumRate " +
                        //    "from PremiumRate pr " +
                        //    "join PremiumRateDetails prd " +
                        //    "on pr.RateId = prd.RateId " +
                        //    "and pr.CoverageDuration = prd.CoverageDuration " +
                        //    "and pr.PaymentDuration = prd.PaymentDuration " +
                        //    "where pr.ScheduleId ='" + memberMasterMemberPlan.BasicProductLimitCode + "' " +
                        //    (isAdult != "N" ? "and prd.IsAdult = '" + isAdult + "' " : " ") +
                        //    "and prd.Sex = '" + (isAdult != "N" ? sex : "B") + "'";

                        //var premiumRate = db.Database.SqlQuery<decimal?>(sql).FirstOrDefault();

                        //if (premiumRate == null)
                        //{
                        //    WarningMessagesAdd("Premium Rate Is Not Found for " + item.Member_Endorse.Client?.FullName + ", " + item.Member_Endorse.PlanId + ", " + item.BasicProductLimitCode);
                        //}


                        //var totalOfPremium = (new Decimal(lengthOfBenefitDayPerYear) * (premiumRate ?? 0));

                        //int counter = 0;
                        //bool fixedEndDate = false;
                        //for (var date = member.StartDate.Value.AddYears(1); date <= member.EndDate; date = date.AddYears(1))
                        //{
                        //    if (date == member.EndDate)
                        //    {
                        //        fixedEndDate = true;
                        //    }
                        //    counter++;
                        //}
                        //if (fixedEndDate)
                        //{
                        //    totalOfPremium = (Decimal)(counter * (premiumRate ?? 0));
                        //}

                        //    var newPCF = new PCF_Endorse();
                        //    newPCF.EndorseNumber = member.EndorseNumber;
                        //    newPCF.PolicyId = item.PolicyId;
                        //    newPCF.BasicProductId = item.BasicProductId;
                        //    newPCF.Amount = (premiumPerInvoice - premiumTermLife) * multiplierFactorPercentage;
                        //    newPCF.MemberId = item.MemberId;
                        //    newPCF.TransType = "R";
                        //    newPCF.InvoiceDate = item.InvoiceDate;
                        //    newPCF.SetPropertyCreate();
                        //    db.PCF_Endorse.Add(newPCF);

                    }
                    db.SaveChanges();
                    if (memberPlan.Count == 0)
                    {
                        WarningMessagesAdd("Plan Is Not Found");

                    }
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
                            WarningMessagesAdd("Premium Rate Is Not Found for " + item.Member_Endorse.Client?.FullName + ", " + item.Member_Endorse.PlanId + ", " + item.BasicProductLimitCode);
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
                            totalOfPremium = (Decimal)(counter * (totalOfPremium));
                        }


                        listOfDecimal.Add(totalOfPremium);




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
                                for (DateTime? i = member.Endorsement.EndorseDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                                {


                                    db.SaveChanges();
                                    var newPCF = new PCF_Endorse();
                                    newPCF.EndorseNumber = member.EndorseNumber;
                                    newPCF.PolicyId = item.PolicyId;
                                    newPCF.BasicProductId = termLifeBP.BasicProductId;
                                    newPCF.Amount = premiumTermLife * multiplierFactorPercentage;
                                    newPCF.MemberId = item.MemberId;
                                    newPCF.TransType = "P";
                                    newPCF.InvoiceDate = i.Value;
                                    newPCF.SetPropertyCreate();
                                    db.PCF_Endorse.Add(newPCF);
                                }
                            }
                            for (DateTime? i = member.Endorsement.EndorseDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                            {


                                db.SaveChanges();
                                var newPCF = new PCF_Endorse();
                                newPCF.EndorseNumber = member.EndorseNumber;
                                newPCF.PolicyId = item.PolicyId;
                                newPCF.BasicProductId = item.BasicProductId;
                                newPCF.Amount = (premiumPerInvoice - premiumTermLife) * multiplierFactorPercentage;
                                newPCF.MemberId = item.MemberId;
                                newPCF.TransType = "P";
                                newPCF.InvoiceDate = i.Value;
                                newPCF.SetPropertyCreate();
                                db.PCF_Endorse.Add(newPCF);
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            for (DateTime? i = member.Endorsement.EndorseDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                            {


                                db.SaveChanges();
                                var newPCF = new PCF_Endorse();
                                newPCF.EndorseNumber = member.EndorseNumber;
                                newPCF.PolicyId = item.PolicyId;
                                newPCF.BasicProductId = item.BasicProductId;
                                newPCF.Amount = premiumPerInvoice * multiplierFactorPercentage;
                                newPCF.MemberId = item.MemberId;
                                newPCF.TransType = "P";
                                newPCF.InvoiceDate = i.Value;
                                newPCF.SetPropertyCreate();
                                db.PCF_Endorse.Add(newPCF);
                            }
                        }
                        if (WarningMessages().Count == 0)
                        {
                            member.MemberStatus = "Calculated";
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
                    WarningMessagesAdd(e.MessageToList());
                    dbTransaction.Rollback();
                }

            }
            return listOfDecimal.Sum(x => x);
        }
        private decimal CalculateMemberPremi(Member_Endorse member, int coverageDuration, int paymentDuration)
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
                    var memberPlan = db.MemberPlan_Endorse.Where(x => x.MemberId == member.MemberId && x.PolicyId == member.PolicyId && x.EndorseNumber == member.EndorseNumber && x.IsActive == 1).ToList();
                    db.PCF_Endorse.RemoveRange(db.PCF_Endorse.Where(x => x.MemberId == member.MemberId && x.PolicyId == member.PolicyId && x.EndorseNumber == member.EndorseNumber).ToList());
                    db.SaveChanges();


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
                            WarningMessagesAdd("Premium Rate Is Not Found for " + item.Member_Endorse.Client?.FullName + ", " + item.Member_Endorse.PlanId + ", " + item.BasicProductLimitCode);
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
                                for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                                {


                                    db.SaveChanges();
                                    var newPCF = new PCF_Endorse();
                                    newPCF.EndorseNumber = member.EndorseNumber;
                                    newPCF.PolicyId = item.PolicyId;
                                    newPCF.BasicProductId = termLifeBP.BasicProductId;
                                    newPCF.Amount = premiumTermLife * multiplierFactorPercentage;
                                    newPCF.MemberId = item.MemberId;
                                    newPCF.TransType = TransactionType.Premium;
                                    newPCF.InvoiceDate = i.Value;
                                    newPCF.SetPropertyCreate();
                                    db.PCF_Endorse.Add(newPCF);
                                }
                            }
                            for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                            {


                                db.SaveChanges();
                                var newPCF = new PCF_Endorse();
                                newPCF.EndorseNumber = member.EndorseNumber;
                                newPCF.PolicyId = item.PolicyId;
                                newPCF.BasicProductId = item.BasicProductId;
                                newPCF.Amount = (premiumPerInvoice - premiumTermLife) * multiplierFactorPercentage;
                                newPCF.MemberId = item.MemberId;
                                newPCF.TransType = "P";
                                newPCF.InvoiceDate = i.Value;
                                newPCF.SetPropertyCreate();
                                db.PCF_Endorse.Add(newPCF);
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                            {


                                db.SaveChanges();
                                var newPCF = new PCF_Endorse();
                                newPCF.EndorseNumber = member.EndorseNumber;
                                newPCF.PolicyId = item.PolicyId;
                                newPCF.BasicProductId = item.BasicProductId;
                                newPCF.Amount = premiumPerInvoice * multiplierFactorPercentage;
                                newPCF.MemberId = item.MemberId;
                                newPCF.TransType = "P";
                                newPCF.InvoiceDate = i.Value;
                                newPCF.SetPropertyCreate();
                                db.PCF_Endorse.Add(newPCF);
                            }
                        }
                        if (WarningMessages().Count == 0)
                        {
                            member.MemberStatus = "Calculated";
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
            var pcfTotalAmount = db.PCF_Endorse.Where(x => x.PolicyId == member.PolicyId && x.MemberId == member.MemberId && x.EndorseNumber == member.EndorseNumber && x.TransType == "P").Select(x => x.Amount.Value).ToList();
            var retvalDec = listOfDecimal.Sum();
            var sumPCF = pcfTotalAmount.Sum(x => x);
            return sumPCF;
            //return retval;

        }
        private decimal CalculateMemberPremiMemberTransitionData(Member_Endorse member)
        {
            //Nilai yang ditagihkan berasal dari 2 sumber. Yaitu dari premi yang berubah dan dari PrintCardFee.
            //1. Hitung Premi Baru.
            //2. Tambahkan apabila ada Fee untuk print kartu baru.

            var listOfDecimal = new List<decimal>();
            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    var memberClientEndorse = member.MemberClientEndorse.FirstOrDefault();
                    var memberOrigin = db.Member.Where(x => x.MemberNumber == member.MemberNumber).FirstOrDefault();
                    if (memberClientEndorse == null)
                    {
                        WarningMessagesAdd("No Data Replaced For Member " + member.Client.FullName);
                        return 0;
                    }
                    if (memberOrigin == null)
                    {
                        WarningMessagesAdd(Message.ProcessFail);
                        return 0;
                    }
                    if (WarningMessages().Count == 0)
                    {

                        if (memberClientEndorse.IsPremiumCorrection == 1)
                        {
                            var refundFactor = new double();
                            if (member.Client.Sex != memberClientEndorse.Sex)
                            {
                                //Update sex (for adult)
                                if (member.Client.Sex == Sex.Male && memberClientEndorse.Sex == Sex.Female)
                                {
                                    //Find whether the plan for Female is exist

                                    var limitCode = string.Empty;
                                    foreach (var item in memberOrigin.MemberPlan.ToList())
                                    {
                                        if (limitCode != string.Empty && limitCode != item.BasicProductLimitCode.Remove(0, 2))
                                        {
                                            WarningMessagesAdd("Limit Code Is Not Valid");
                                            return 0;
                                        }
                                        limitCode = item.BasicProductLimitCode.Remove(0, 2);
                                    }
                                    var policyPlanDetail = memberOrigin.Policy.PlanDetail.ToList().Where(x => x.BasicProductId.ToLower() == "ma" && x.BasicProductLimitCode.ToLower() == "ma" + limitCode).FirstOrDefault();
                                    if (policyPlanDetail == null)
                                    {
                                        WarningMessagesAdd("Limit Code Is Not Found For Member " + member.Client.FullName);
                                        return 0;
                                    }
                                    //Remove MemberPlanEndorse and Recreate
                                    db.MemberPlan_Endorse.RemoveRange(member.MemberPlan_Endorse);
                                    db.SaveChanges();
                                    foreach (var item in memberOrigin.MemberPlan)
                                    {
                                        var newMemberPlanEndorse = new MemberPlan_Endorse();
                                        newMemberPlanEndorse.EndorseNumber = member.EndorseNumber;
                                        newMemberPlanEndorse.PolicyId = member.PolicyId;
                                        newMemberPlanEndorse.MemberId = member.MemberId;
                                        newMemberPlanEndorse.BasicProductId = item.BasicProductId;
                                        newMemberPlanEndorse.PlanId = item.PlanId;
                                        newMemberPlanEndorse.StartDate = item.StartDate;
                                        newMemberPlanEndorse.BasicProductLimitCode = item.BasicProductLimitCode;
                                        newMemberPlanEndorse.SetPropertyCreate();
                                        db.MemberPlan_Endorse.Add(newMemberPlanEndorse);
                                    }
                                    var newMemberPlanEnd = new MemberPlan_Endorse();
                                    newMemberPlanEnd.EndorseNumber = member.EndorseNumber;
                                    newMemberPlanEnd.PolicyId = member.PolicyId;
                                    newMemberPlanEnd.MemberId = member.MemberId;
                                    newMemberPlanEnd.BasicProductId = policyPlanDetail.BasicProductId;
                                    newMemberPlanEnd.PlanId = policyPlanDetail.PlanId;
                                    newMemberPlanEnd.StartDate = member.Endorsement.EndorseDate;
                                    newMemberPlanEnd.BasicProductLimitCode = policyPlanDetail.BasicProductLimitCode;
                                    newMemberPlanEnd.SetPropertyCreate();
                                    db.MemberPlan_Endorse.Add(newMemberPlanEnd);
                                    db.SaveChanges();

                                }
                                else if ((member.Client.Sex == Sex.Female && memberClientEndorse.Sex == Sex.Male))
                                {
                                    db.MemberPlan_Endorse.RemoveRange(member.MemberPlan_Endorse.Where(x => x.BasicProductLimitCode.ToUpper().Contains("MA")));
                                    db.SaveChanges();
                                }
                                refundFactor = ((memberOrigin.EndDate - member.Endorsement.EndorseDate).Value.TotalDays / (memberOrigin.EndDate - memberOrigin.StartDate).Value.TotalDays);

                                InfoMessagesAdd("Plan For Member " + member.Client.FullName + " Was Changed");
                            }
                            else if (((memberClientEndorse?.ClientRelation == ClientRelation.Husband || memberClientEndorse?.ClientRelation == ClientRelation.Wife) && (member.Client?.ClientRelation == ClientRelation.Son || member.Client?.ClientRelation == ClientRelation.Daughter)))
                            {
                                //Update relationship (from C to W/H, or the opposite)
                                //Update sex (for adult)
                                if (member.Client.Sex == Sex.Male && memberClientEndorse.Sex == Sex.Female)
                                {
                                    //Find whether the plan for Female is exist

                                    var limitCode = string.Empty;
                                    foreach (var item in memberOrigin.MemberPlan.ToList())
                                    {
                                        if (limitCode != string.Empty && limitCode != item.BasicProductLimitCode.Remove(0, 2))
                                        {
                                            WarningMessagesAdd("Limit Code Is Not Valid");
                                            return 0;
                                        }
                                        limitCode = item.BasicProductLimitCode.Remove(0, 2);
                                    }
                                    var policyPlanDetail = memberOrigin.Policy.PlanDetail.ToList().Where(x => x.BasicProductId.ToLower() == "ma" && x.BasicProductLimitCode.ToLower() == "ma" + limitCode).FirstOrDefault();
                                    if (policyPlanDetail == null)
                                    {
                                        WarningMessagesAdd("Limit Code Is Not Found For Member " + member.Client.FullName);
                                        return 0;
                                    }
                                    //Remove MemberPlanEndorse and Recreate
                                    db.MemberPlan_Endorse.RemoveRange(member.MemberPlan_Endorse);
                                    db.SaveChanges();
                                    foreach (var item in memberOrigin.MemberPlan)
                                    {
                                        var newMemberPlanEndorse = new MemberPlan_Endorse();
                                        newMemberPlanEndorse.EndorseNumber = member.EndorseNumber;
                                        newMemberPlanEndorse.PolicyId = member.PolicyId;
                                        newMemberPlanEndorse.MemberId = member.MemberId;
                                        newMemberPlanEndorse.BasicProductId = item.BasicProductId;
                                        newMemberPlanEndorse.PlanId = item.PlanId;
                                        newMemberPlanEndorse.StartDate = item.StartDate;
                                        newMemberPlanEndorse.BasicProductLimitCode = item.BasicProductLimitCode;
                                        newMemberPlanEndorse.SetPropertyCreate();
                                        db.MemberPlan_Endorse.Add(newMemberPlanEndorse);
                                    }
                                    var newMemberPlanEnd = new MemberPlan_Endorse();
                                    newMemberPlanEnd.EndorseNumber = member.EndorseNumber;
                                    newMemberPlanEnd.PolicyId = member.PolicyId;
                                    newMemberPlanEnd.MemberId = member.MemberId;
                                    newMemberPlanEnd.BasicProductId = policyPlanDetail.BasicProductId;
                                    newMemberPlanEnd.PlanId = policyPlanDetail.PlanId;
                                    newMemberPlanEnd.StartDate = member.Endorsement.EndorseDate;
                                    newMemberPlanEnd.BasicProductLimitCode = policyPlanDetail.BasicProductLimitCode;
                                    newMemberPlanEnd.SetPropertyCreate();
                                    db.MemberPlan_Endorse.Add(newMemberPlanEnd);
                                    db.SaveChanges();

                                }
                                else if ((member.Client.Sex == Sex.Female && memberClientEndorse.Sex == Sex.Male))
                                {
                                    db.MemberPlan_Endorse.RemoveRange(member.MemberPlan_Endorse.Where(x => x.BasicProductLimitCode.ToUpper().Contains("MA")));
                                    db.SaveChanges();
                                }
                            }
                            else if (memberClientEndorse.BirthDate != member.Client.BirthDate)
                            {
                                //Update date of birth (from children to adult)

                            }
                            else if (memberOrigin.StartDate != member.StartDate || memberOrigin.EndDate != member.EndDate)
                            {
                                //Update start and end date
                                refundFactor = 1;

                            }
                            //Delete AllPCF From PCFEndorse
                            var allPCFEndorse = member.PCF_Endorse.ToList();
                            db.PCF_Endorse.RemoveRange(allPCFEndorse);
                            db.SaveChanges();
                            //< Menyalin dari PCF master ke PCF Endorse
                            var memberPCFMaster = db.PCF.Where(x => x.PolicyId == member.PolicyId && x.Member.MemberNumber == member.MemberNumber).ToList();
                            foreach (var itemMemberPcf in memberPCFMaster)
                            {
                                var pcfEndorse = new PCF_Endorse();
                                pcfEndorse.MemberId = member.MemberId;
                                pcfEndorse.BasicProductId = itemMemberPcf.BasicProductId;
                                pcfEndorse.DueDate = itemMemberPcf.DueDate;
                                pcfEndorse.EndorseNumber = member.EndorseNumber;
                                pcfEndorse.InvoiceDate = itemMemberPcf.InvoiceDate;
                                pcfEndorse.PolicyId = itemMemberPcf.PolicyId;
                                pcfEndorse.TransType = itemMemberPcf.TransType;
                                pcfEndorse.Amount = itemMemberPcf.Amount;
                                pcfEndorse.TransactionNumber = itemMemberPcf.TransactionNumber;
                                pcfEndorse.SetPropertyCreate();
                                db.PCF_Endorse.Add(pcfEndorse);
                            }
                            db.SaveChanges();
                            //>
                            //Generates Refund Premi
                            if (memberOrigin == null)
                            {
                                WarningMessagesAdd(Message.ProcessFail);
                            }
                            var lastTransaction = memberPCFMaster.OrderByDescending(x => x.TransactionNumber).FirstOrDefault();
                            if (lastTransaction == null)
                            {
                                WarningMessagesAdd(Message.ProcessFail);
                                return 0;
                            }

                            foreach (var itemMemberPcf in memberPCFMaster.Where(x => x.TransactionNumber == lastTransaction.TransactionNumber && x.TransType == TransactionType.Premium))
                            {
                                var pcfEndorse = new PCF_Endorse();
                                pcfEndorse.MemberId = member.MemberId;
                                pcfEndorse.BasicProductId = itemMemberPcf.BasicProductId;
                                pcfEndorse.DueDate = itemMemberPcf.DueDate;
                                pcfEndorse.EndorseNumber = member.EndorseNumber;
                                pcfEndorse.InvoiceDate = itemMemberPcf.InvoiceDate;
                                pcfEndorse.PolicyId = itemMemberPcf.PolicyId;
                                pcfEndorse.TransType = TransactionType.Refund;
                                pcfEndorse.Amount = Convert.ToDecimal(refundFactor) * itemMemberPcf.Amount * -1;
                                pcfEndorse.SetPropertyCreate();
                                db.PCF_Endorse.Add(pcfEndorse);
                            }
                            db.SaveChanges();

                            var memberPlanOfMember = member.MemberPlan_Endorse.ToList();
                            if (memberClientEndorse.Sex == Sex.Male)
                            {
                                memberPlanOfMember = memberPlanOfMember.Where(x => x.PlanId != "MA").ToList();
                            }
                            var age = WebAppUtility.Age(memberClientEndorse.BirthDate);
                            var isAdult = "N";
                            if (age > 22)
                            {
                                isAdult = "Y";
                            }
                            if (memberClientEndorse.MaritalStatus == MaritalStatus.Married)
                            {
                                isAdult = "Y";
                            }

                            var sex = memberClientEndorse.Sex == "Female" ? "F" : "M";
                            var lengthOfBenefitDayPerYear = (member.EndDate - member.StartDate).Value.TotalDays / 365.25;
                            var lengthOfBenefitDayPerYearTermLife = (member.EndDate - member.StartDate).Value.TotalDays / 365;

                            foreach (var item in memberPlanOfMember)
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
                                    WarningMessagesAdd("Premium Rate Is Not Found for " + item.Member_Endorse.Client?.FullName + ", " + item.Member_Endorse.PlanId + ", " + item.BasicProductLimitCode);
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
                                        for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                                        {


                                            db.SaveChanges();
                                            var newPCF = new PCF_Endorse();
                                            newPCF.EndorseNumber = member.EndorseNumber;
                                            newPCF.PolicyId = item.PolicyId;
                                            newPCF.BasicProductId = termLifeBP.BasicProductId;
                                            newPCF.Amount = premiumTermLife * multiplierFactorPercentage;
                                            newPCF.MemberId = item.MemberId;
                                            newPCF.TransType = "P";
                                            newPCF.InvoiceDate = i.Value;
                                            newPCF.SetPropertyCreate();
                                            db.PCF_Endorse.Add(newPCF);
                                        }
                                    }
                                    for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                                    {
                                        db.SaveChanges();
                                        var newPCF = new PCF_Endorse();
                                        newPCF.EndorseNumber = member.EndorseNumber;
                                        newPCF.PolicyId = item.PolicyId;
                                        newPCF.BasicProductId = item.BasicProductId;
                                        newPCF.Amount = (premiumPerInvoice - premiumTermLife) * multiplierFactorPercentage;
                                        newPCF.MemberId = item.MemberId;
                                        newPCF.TransType = "P";
                                        newPCF.InvoiceDate = i.Value;
                                        newPCF.SetPropertyCreate();
                                        db.PCF_Endorse.Add(newPCF);
                                    }
                                    db.SaveChanges();
                                }
                                else
                                {
                                    for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                                    {


                                        db.SaveChanges();
                                        var newPCF = new PCF_Endorse();
                                        newPCF.EndorseNumber = member.EndorseNumber;
                                        newPCF.PolicyId = item.PolicyId;
                                        newPCF.BasicProductId = item.BasicProductId;
                                        newPCF.Amount = premiumPerInvoice * multiplierFactorPercentage;
                                        newPCF.MemberId = item.MemberId;
                                        newPCF.TransType = "P";
                                        newPCF.InvoiceDate = i.Value;
                                        newPCF.SetPropertyCreate();
                                        db.PCF_Endorse.Add(newPCF);
                                    }
                                }
                                if (WarningMessages().Count == 0)
                                {
                                    member.MemberStatus = "Calculated";
                                    db.Entry(member).State = System.Data.Entity.EntityState.Modified;
                                }
                                db.SaveChanges();
                            }

                        }
                        else
                        {
                            //Delete AllPCF From PCFEndorse
                            var allPCFEndorse = member.PCF_Endorse.ToList();
                            db.PCF_Endorse.RemoveRange(allPCFEndorse);
                            db.SaveChanges();
                            //< Menyalin dari PCF master ke PCF Endorse
                            var memberPCFMaster = db.PCF.Where(x => x.PolicyId == member.PolicyId && x.Member.MemberNumber == member.MemberNumber).ToList();
                            foreach (var itemMemberPcf in memberPCFMaster)
                            {
                                var pcfEndorse = new PCF_Endorse();
                                pcfEndorse.MemberId = member.MemberId;
                                pcfEndorse.BasicProductId = itemMemberPcf.BasicProductId;
                                pcfEndorse.DueDate = itemMemberPcf.DueDate;
                                pcfEndorse.EndorseNumber = member.EndorseNumber;
                                pcfEndorse.InvoiceDate = itemMemberPcf.InvoiceDate;
                                pcfEndorse.PolicyId = itemMemberPcf.PolicyId;
                                pcfEndorse.TransType = itemMemberPcf.TransType;
                                pcfEndorse.Amount = itemMemberPcf.Amount;
                                pcfEndorse.TransactionNumber = itemMemberPcf.TransactionNumber;
                                pcfEndorse.SetPropertyCreate();
                                db.PCF_Endorse.Add(pcfEndorse);
                            }
                            //>
                            db.SaveChanges();
                        }
                        if (memberClientEndorse.IsFinancialChange == 1 && memberClientEndorse.PrintNewCard == 1)
                        {
                            var printCardAmountString = db.CommonListValue.Where(x => x.Text == CommonListValueConst.PrintCardFee).FirstOrDefault()?.Value;
                            var printCardAmount = Convert.ToDecimal(printCardAmountString);
                            memberClientEndorse.PrintCardAmount = printCardAmount;
                            if (WarningMessages().Count == 0)
                            {
                                member.MemberStatus = "Calculated";
                                db.Entry(member).State = System.Data.Entity.EntityState.Modified;
                            }
                            db.SaveChanges();
                        }
                        if (WarningMessages().Count == 0)
                        {
                            dbTransaction.Commit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    WarningMessagesAdd(ex.MessageToList());

                }
            }


            var allMemberClientEndorse = member.MemberClientEndorse.ToList();

            var pcfTotalAmount = db.PCF_Endorse.Where(x => x.PolicyId == member.PolicyId && x.MemberId == member.MemberId && x.EndorseNumber == member.EndorseNumber && x.TransType == "P" && x.TransactionNumber == null).Select(x => x.Amount.Value).ToList();
            var retvalDec = listOfDecimal.Sum();
            var sumPCF = pcfTotalAmount.Sum(x => x);
            //penambahan PrincardFee
            var printCardFee = member.MemberClientEndorse.Sum(x => x.PrintCardAmount);
            sumPCF = sumPCF + (printCardFee.HasValue ? printCardFee.Value : 0);
            return sumPCF;
        }
        private decimal CalculateMemberPremiRenewalMember(Member_Endorse member)
        {
            var listOfDecimal = new List<decimal>();
            using (var dbTransaction = db.Database.BeginTransaction())
            {

                try
                {
                    var isAdult = "N";
                    if (WebAppUtility.IsAdult(member.Client.BirthDate))
                    {
                        isAdult = "Y";
                    }
                    if (member.Client.MaritalStatus == MaritalStatus.Married)
                    {
                        isAdult = "Y";
                    }

                    var sex = member.Client.Sex == Sex.Female ? "F" : "M";
                    var lengthOfBenefitDayPerYear = (member.EndDate - member.StartDate).Value.TotalDays / 365.25;
                    var lengthOfBenefitDayPerYearTermLife = (member.EndDate - member.StartDate).Value.TotalDays / 365;
                    var memberPlan = db.MemberPlan_Endorse.Where(x => x.MemberId == member.MemberId && x.PolicyId == member.PolicyId && x.EndorseNumber == member.EndorseNumber && x.IsActive == 1).ToList();
                    db.PCF_Endorse.RemoveRange(db.PCF_Endorse.Where(x => x.MemberId == member.MemberId && x.PolicyId == member.PolicyId && x.EndorseNumber == member.EndorseNumber).ToList());
                    db.SaveChanges();


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
                            WarningMessagesAdd("Premium Rate Is Not Found for " + item.Member_Endorse.Client?.FullName + ", " + item.Member_Endorse.PlanId + ", " + item.BasicProductLimitCode);
                        }
                        var frequecyToNumber = 0;
                        decimal multiplierFactorPercentage = new decimal(0);


                        if (member.Policy.PaymentFrequency == PaymentFrequency.Monthly)
                        {
                            frequecyToNumber = 1;
                            multiplierFactorPercentage = decimal.Multiply(decimal.Divide(10, 100), 12);
                        }
                        else if (member.Policy.PaymentFrequency == PaymentFrequency.Quarterly)
                        {
                            frequecyToNumber = 4;
                            multiplierFactorPercentage = decimal.Multiply(decimal.Divide(27, 100), 4);
                        }
                        else if (member.Policy.PaymentFrequency == PaymentFrequency.Semesterly)
                        {
                            frequecyToNumber = 6;
                            multiplierFactorPercentage = decimal.Multiply(decimal.Divide(52, 100), 2);
                        }
                        else if (member.Policy.PaymentFrequency == PaymentFrequency.Yearly)
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

                        listOfDecimal.Add(totalOfPremium);

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
                                for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                                {
                                    db.SaveChanges();
                                    var newPCF = new PCF_Endorse();
                                    newPCF.EndorseNumber = member.EndorseNumber;
                                    newPCF.PolicyId = item.PolicyId;
                                    newPCF.BasicProductId = termLifeBP.BasicProductId;
                                    newPCF.Amount = premiumTermLife * multiplierFactorPercentage;
                                    newPCF.MemberId = item.MemberId;
                                    newPCF.TransType = TransactionType.Premium;
                                    newPCF.InvoiceDate = i.Value;
                                    newPCF.SetPropertyCreate();
                                    db.PCF_Endorse.Add(newPCF);
                                }
                            }
                            for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                            {
                                db.SaveChanges();
                                var newPCF = new PCF_Endorse();
                                newPCF.EndorseNumber = member.EndorseNumber;
                                newPCF.PolicyId = item.PolicyId;
                                newPCF.BasicProductId = item.BasicProductId;
                                newPCF.Amount = (premiumPerInvoice - premiumTermLife) * multiplierFactorPercentage;
                                newPCF.MemberId = item.MemberId;
                                newPCF.TransType = "P";
                                newPCF.InvoiceDate = i.Value;
                                newPCF.SetPropertyCreate();
                                db.PCF_Endorse.Add(newPCF);
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            for (DateTime? i = member.StartDate; i < member.EndDate; i = i.Value.AddMonths(frequecyToNumber))
                            {
                                db.SaveChanges();
                                var newPCF = new PCF_Endorse();
                                newPCF.EndorseNumber = member.EndorseNumber;
                                newPCF.PolicyId = item.PolicyId;
                                newPCF.BasicProductId = item.BasicProductId;
                                newPCF.Amount = premiumPerInvoice * multiplierFactorPercentage;
                                newPCF.MemberId = item.MemberId;
                                newPCF.TransType = "P";
                                newPCF.InvoiceDate = i.Value;
                                newPCF.SetPropertyCreate();
                                db.PCF_Endorse.Add(newPCF);
                            }
                        }
                        if (WarningMessages().Count == 0)
                        {
                            member.MemberStatus = "Calculated";
                            db.Entry(member).State = System.Data.Entity.EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                    // Calculate PrintCard Fee
                    var memberClientEndorse = member.MemberClientEndorse.ToList();
                    var printCardAmountString = db.CommonListValue.Where(x => x.Text == CommonListValueConst.PrintCardFee).FirstOrDefault()?.Value;
                    var printCardAmount = Convert.ToDecimal(printCardAmountString);
                    if (memberClientEndorse != null)
                    {
                        foreach (var item in memberClientEndorse)
                        {
                            if (item.PrintNewCard == 1)
                            {
                                item.PrintCardAmount = printCardAmount;
                                db.Entry(item).State = EntityState.Modified;
                            }
                        }
                    }
                    db.SaveChanges();
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
            var pcfTotalAmount = db.PCF_Endorse.Where(x => x.PolicyId == member.PolicyId && x.MemberId == member.MemberId && x.EndorseNumber == member.EndorseNumber && x.TransType == "P").Select(x => x.Amount.Value).ToList();
            var retvalDec = listOfDecimal.Sum();
            var sumPCF = pcfTotalAmount.Sum(x => x);
            var printCardFeeAmount = member.MemberClientEndorse.Sum(x => x.PrintCardAmount);
            if (printCardFeeAmount == null)
            {
                printCardFeeAmount = 0;
            }
            return sumPCF + printCardFeeAmount.Value;
        }
        public ActionResult Issue(string endorseId)
        {

            if (endorseId == null)
            {
                return HttpNotFound();

            }
            List<string> issueMessage = new List<string>();
            var endorsement = db.Endorsement.Find(endorseId);
            var policy = endorsement.Policy;
            var policyMember = endorsement.Member_Endorse;
            var policyId = endorsement.PolicyId;
            if (endorsement.Member_Endorse.Count == 0)
            {
                WarningMessagesAdd("No Member(s) To Issue");
            }
            if (policy == null)
            {
                return HttpNotFound();
            }
            if (endorsement == null)
            {
                return HttpNotFound();
            }
            if (endorsement.EndorseType == EndorseType.TerminateMember)
            {
                if (policyMember.Count != policyMember.Where(x => x.MemberStatus == TerminateType.Cancel + " " + MemberStatus.Calculated || x.MemberStatus == TerminateType.Refund + " " + MemberStatus.Calculated).Count() || db.PCF_Endorse.Where(x => x.PolicyId == policyId && x.EndorseNumber == endorseId).GroupBy(x => x.MemberId).Count() != endorsement.Member_Endorse.Count)
                {
                    WarningMessagesAdd("Policy needed to be calculated first");
                }
            }
            else if (endorsement.EndorseType == EndorseType.Mutation)
            {
                if (policyMember.Count != policyMember.Where(x => x.MemberStatus == TerminateType.Cancel + " " + MemberStatus.Calculated || x.MemberStatus == TerminateType.Refund + " " + MemberStatus.Calculated).Count() || db.PCF_Endorse.Where(x => x.PolicyId == policyId && x.EndorseNumber == endorseId).GroupBy(x => x.MemberId).Count() != endorsement.Member_Endorse.Count)
                {
                    WarningMessagesAdd("Policy needed to be calculated first");
                }
                foreach (var item in endorsement.Endorsement1)
                {

                    if (item.Member_Endorse.Count != item.Member_Endorse.Where(x => x.MemberStatus == MemberStatus.Calculated).Count() || db.PCF_Endorse.Where(x => x.PolicyId == policyId && x.EndorseNumber == endorseId).GroupBy(x => x.MemberId).Count() != item.Member_Endorse.Count)
                    {
                        WarningMessagesAdd("Policy needed to be calculated first");
                    }
                }

            }
            else if (endorsement.EndorseType == EndorseType.Renewal)
            {
                if (policyMember.Count != policyMember.Where(x => x.MemberStatus == MemberStatus.Calculated).Count() || db.PCF_Endorse.Where(x => x.PolicyId == policyId && x.EndorseNumber == endorseId).GroupBy(x => x.MemberId).Count() != endorsement.Member_Endorse.Count)
                {
                    WarningMessagesAdd("Policy needed to be calculated first");
                }
                if (endorsement.Policy.Member.Where(x => x.MemberStatus == MemberStatus.Endorse || x.MemberStatus == MemberStatus.Active).Count() != endorsement.Member_Endorse.Count())
                {
                    WarningMessagesAdd("Not All Member Are Processed");
                }
            }
            else
            {
                if (policyMember.Count != policyMember.Where(x => x.MemberStatus == MemberStatus.Calculated).Count() || db.PCF_Endorse.Where(x => x.PolicyId == policyId && x.EndorseNumber == endorseId).GroupBy(x => x.MemberId).Count() != endorsement.Member_Endorse.Count)
                {
                    WarningMessagesAdd("Policy needed to be calculated first");
                }
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
                WarningMessagesDistinct();
                return View("_Modal", new ModalView()
                {
                    Title = "Issue Policy",
                    Body = this.RenderRazorViewToString("Issue", endorsement),
                    Footer = WarningMessages().Count == 0 ? this.GetHtmlHelper().TextBox("Issue", "Issue", null, new { @class = "btn btn-primary" + (Warn() ? " disabled" : ""), @style = "background-color:#008CBA; color:white;", @type = "submit" }).ToString() : null,
                    ModalForm = new ModalForm()
                    {
                        ActionName = "Issue/" + endorsement.EndorseNumber,
                        ControllerName = "PolicyEndorsement",
                        RouteValues = new { endorseId = endorsement.EndorseNumber }
                    }
                });
                //return View("Issue", policy);
            }
            return View(endorsement);
        }
        [HttpPost, ActionName("Issue")]
        public ActionResult IssueConfirm(string endorseId)
        {

            if (endorseId == null)
            {
                return HttpNotFound();

            }

            var endorsement = db.Endorsement.Find(endorseId);
            if (endorsement == null)
            {
                return HttpNotFound();

            }

            var policyId = endorsement.PolicyId;
            if (policyId == null)
            {
                return HttpNotFound();

            }

            var policy = db.Policy.Find(policyId);
            if (policy == null)
            {
                return HttpNotFound();
            }
            var memberPolicy = endorsement.Member_Endorse.ToList();
            if (endorsement.EndorseType == EndorseType.TerminateMember)
            {
                if (endorsement.Member_Endorse.Count != endorsement.Member_Endorse.Where(x => x.MemberStatus == TerminateType.Cancel + " " + MemberStatus.Calculated || x.MemberStatus == TerminateType.Refund + " " + MemberStatus.Calculated).Count() || db.PCF_Endorse.Where(x => x.PolicyId == policyId && x.EndorseNumber == endorseId).GroupBy(x => x.MemberId).Count() != endorsement.Member_Endorse.Count)
                {
                    WarningMessagesAdd("Policy needed to be calculated first");
                }
            }
            else if (endorsement.EndorseType == EndorseType.Mutation)
            {
                if (endorsement.Member_Endorse.Count != endorsement.Member_Endorse.Where(x => x.MemberStatus == TerminateType.Cancel + " " + MemberStatus.Calculated || x.MemberStatus == TerminateType.Refund + " " + MemberStatus.Calculated).Count() || db.PCF_Endorse.Where(x => x.PolicyId == policyId && x.EndorseNumber == endorseId).GroupBy(x => x.MemberId).Count() != endorsement.Member_Endorse.Count)
                {
                    WarningMessagesAdd("Policy needed to be calculated first");
                }
                foreach (var item in endorsement.Endorsement1)
                {

                    if (item.Member_Endorse.Count != item.Member_Endorse.Where(x => x.MemberStatus == MemberStatus.Calculated).Count() || db.PCF_Endorse.Where(x => x.PolicyId == policyId && x.EndorseNumber == endorseId).GroupBy(x => x.MemberId).Count() != item.Member_Endorse.Count)
                    {
                        WarningMessagesAdd("Policy needed to be calculated first");
                    }
                }

            }
            else
            {
                if (endorsement.Member_Endorse.Count != endorsement.Member_Endorse.Where(x => x.MemberStatus == MemberStatus.Calculated).Count() || db.PCF_Endorse.Where(x => x.PolicyId == policyId && x.EndorseNumber == endorseId).GroupBy(x => x.MemberId).Count() != endorsement.Member_Endorse.Count)
                {
                    WarningMessagesAdd("Policy needed to be calculated first");
                }
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
                        policy.SetPropertyUpdate();
                        policy.LastEndorseDate = endorsement.EndorseDate;
                        policy.PolicyStatus = PolicyStatus.Active;
                        db.Entry(policy).State = EntityState.Modified;
                        db.SaveChanges();


                        if (endorsement.EndorseType == EndorseType.MovePlan)
                        {
                            try
                            {
                                //1. Pindahkan MemberPlan ke MemberPlan History
                                //1a. Pindahkan MemberPlanEndorse ke MemberPlan
                                //2. Buat transaction baru
                                //3. Salin PCF_Endorse ke PCF, yang disalin adalah pcf yang tidak mempunyai transaction number
                                //4. Salin PlanEndorse ke Plan
                                //5. Salin PlanDetailEndorse ke Plan Detail
                                //5a. Delete MemberPlanEndorseDetail
                                //5b. Delete MemberPlanEndorse
                                //6. Delete MemberEndorse
                                //7. Update Endorsement status menjadi 
                                //8. Update Member,set Plan to current Plan and set EndorseNumber and LastEndorseDate



                                //2. [Execution]Buat transaction baru
                                var allPolicyPCFEndorse = db.PCF_Endorse.Where(x => x.EndorseNumber == endorsement.EndorseNumber && x.PolicyId == policyId && x.InvoiceDate < DateTime.Now);
                                var financeTransaction = new FinanceTransaction();
                                var transactionNumberSeq = db.AspSequential.Where(x => x.Name == AspSequentialName.TransactionNumber).FirstOrDefault();
                                financeTransaction.RecordMode = 1;
                                financeTransaction.TransactionNumber = "TXTR-" + DateTime.Now.Year + "-" + (transactionNumberSeq.LastSequential + 1).ToString().PadLeft(6, '0');
                                transactionNumberSeq.LastSequential = transactionNumberSeq.LastSequential + 1;
                                transactionNumberSeq.SetPropertyUpdate();
                                db.SaveChanges();
                                db.Entry(transactionNumberSeq).State = EntityState.Modified;
                                financeTransaction.EffectiveDate = DateTime.Today;
                                financeTransaction.TransactionDate = endorsement.EndorseDate;
                                financeTransaction.DueDate = financeTransaction.TransactionDate.Value.AddDays(30);
                                financeTransaction.OutstandingAmount = allPolicyPCFEndorse.Sum(x => x.Amount);
                                financeTransaction.TransCodeId = financeTransaction.OutstandingAmount > 0 ? "Invoice" : "Refund";
                                financeTransaction.PolicyId = endorsement.PolicyId;
                                financeTransaction.PolicyNumber = endorsement.Policy.PolicyNumber;
                                financeTransaction.ReconStatus = "No";
                                financeTransaction.ClientId = endorsement.Policy.ClientId;
                                financeTransaction.ClientTransactionAmount = financeTransaction.OutstandingAmount;
                                financeTransaction.ClosingAgen = endorsement.Policy.Agent;
                                financeTransaction.TransDescription = financeTransaction.TransCodeId + " Policy " + policy.PolicyNumber;
                                financeTransaction.ReferenceNumber = endorsement.EndorseNumber;
                                db.FinanceTransaction.Add(financeTransaction);

                                db.SaveChanges();

                                foreach (var memberEndorseItem in endorsement.Member_Endorse)
                                {
                                    var member = db.Member.Where(x => x.MemberNumber == memberEndorseItem.MemberNumber).FirstOrDefault();
                                    var memberPlanWillMoveToHistory = member.MemberPlan;
                                    //8. [Execution]Update Member,set Plan to current Plan and set EndorseNumber and LastEndorseDate
                                    member.PlanId = memberEndorseItem.PlanId;
                                    member.LastEndorseDate = memberEndorseItem.Endorsement.EndorseDate;
                                    member.EndorseNumber = memberEndorseItem.EndorseNumber;
                                    member.MemberStatus = MemberStatus.Active;
                                    db.Entry(member).State = EntityState.Modified;
                                    db.SaveChanges();
                                    //1.[Execution] Pindahkan MemberPlan ke MemberPlan History
                                    foreach (var item in memberPlanWillMoveToHistory)
                                    {
                                        var endorseNumber = memberEndorseItem.EndorseNumber;
                                        var memberPlanHistory = new MemberPlan_H();
                                        memberPlanHistory.EndorseNumber = endorseNumber;
                                        memberPlanHistory.BasicProductId = item.BasicProductId;
                                        memberPlanHistory.BasicProductLimitCode = item.BasicProductLimitCode;
                                        memberPlanHistory.MemberId = item.MemberId;
                                        memberPlanHistory.PlanId = member.PlanId;
                                        memberPlanHistory.PolicyId = member.PolicyId;
                                        memberPlanHistory.StartDate = item.StartDate;
                                        memberPlanHistory.SetPropertyCreate();
                                        db.MemberPlan_H.Add(memberPlanHistory);

                                        db.SaveChanges();
                                    }
                                    db.MemberPlan.RemoveRange(memberPlanWillMoveToHistory);

                                    //1a.[Execution] Pindahkan MemberPlanEndorse ke MemberPlan
                                    foreach (var item in memberEndorseItem.MemberPlan_Endorse)
                                    {
                                        var memberPlan = new MemberPlan();
                                        memberPlan.EndorseNumber = memberEndorseItem.EndorseNumber;
                                        memberPlan.BasicProductId = item.BasicProductId;
                                        memberPlan.BasicProductLimitCode = item.BasicProductLimitCode;
                                        memberPlan.PlanId = item.PlanId;
                                        memberPlan.MemberId = member.MemberId;
                                        memberPlan.PolicyId = member.PolicyId;
                                        memberPlan.StartDate = item.StartDate;
                                        memberPlan.SetPropertyCreate();
                                        db.MemberPlan.Add(memberPlan);
                                    }



                                    //3. Salin PCF_Endorse ke PCF, yang disalin adalah pcf yang tidak mempunyai transaction number
                                    foreach (var item in memberEndorseItem.PCF_Endorse.Where(x => x.TransactionNumber == null))
                                    {
                                        var newPCF = new PCF();
                                        newPCF.PolicyId = item.PolicyId;
                                        newPCF.MemberId = member.MemberId;
                                        newPCF.BasicProductId = item.BasicProductId;
                                        newPCF.TransType = item.TransType;
                                        newPCF.InvoiceDate = item.InvoiceDate;
                                        newPCF.DueDate = item.DueDate;
                                        newPCF.Amount = item.Amount;
                                        newPCF.TransactionNumber = financeTransaction.TransactionNumber;
                                        newPCF.SetPropertyCreate();
                                        db.PCF.Add(newPCF);
                                    }
                                    db.PCF_Endorse.RemoveRange(memberEndorseItem.PCF_Endorse);
                                }


                                // Generate Member Movement
                                foreach (var item in endorsement.Member_Endorse)
                                {
                                    var itemMember = db.Member.Where(x => x.MemberNumber == item.MemberNumber).FirstOrDefault();

                                    var newMemberMovement = new Member_Movement();
                                    newMemberMovement.AdmedikaCode = itemMember.AdmedikaCode;
                                    newMemberMovement.Age = itemMember.Age;
                                    newMemberMovement.CardNumber = itemMember.CardNumber;
                                    newMemberMovement.ClaimNumber = itemMember.ClaimNumber;
                                    newMemberMovement.ClientId = itemMember.ClientId;
                                    newMemberMovement.EffectiveDate = DateTime.Now.Date;
                                    newMemberMovement.EndDate = itemMember.EndDate;
                                    newMemberMovement.EndorseNumber = item.EndorseNumber;
                                    newMemberMovement.EntryDate = itemMember.EntryDate;
                                    newMemberMovement.ExitDate = itemMember.ExitDate;
                                    newMemberMovement.IssueDate = itemMember.IssueDate;
                                    newMemberMovement.LastClaimDate = itemMember.LastClaimDate;
                                    newMemberMovement.LastEndorseDate = itemMember.LastEndorseDate;
                                    newMemberMovement.MemberId = itemMember.MemberId;
                                    newMemberMovement.MemberNumber = itemMember.MemberNumber;
                                    newMemberMovement.MemberStatus = itemMember.MemberStatus;
                                    newMemberMovement.PlanId = itemMember.PlanId;
                                    newMemberMovement.PolicyId = itemMember.PolicyId;
                                    //newMemberMovement.ProcessDate = itemMember.ProcessDate;

                                    newMemberMovement.RecordMode = RecordModeMemberMovement.MovePlan;

                                    newMemberMovement.SequencialNo = itemMember.SequencialNo;
                                    newMemberMovement.StartDate = itemMember.StartDate;
                                    newMemberMovement.TerminateDate = itemMember.TerminateDate;

                                    newMemberMovement.SetPropertyCreate();
                                    db.Member_Movement.Add(newMemberMovement);
                                    db.SaveChanges();

                                    var newMemberMovementClient = new Member_Movement_Client();
                                    newMemberMovementClient.MovementId = newMemberMovement.Id;
                                    newMemberMovementClient.ClientId = itemMember.Client.ClientId;
                                    newMemberMovementClient.Type = itemMember.Client.Type;
                                    newMemberMovementClient.BranchCode = itemMember.Client.BranchCode;
                                    newMemberMovementClient.ContactPerson = itemMember.Client.ContactPerson;
                                    newMemberMovementClient.ShortName = itemMember.Client.ShortName;
                                    newMemberMovementClient.FullName = itemMember.Client.FullName;
                                    newMemberMovementClient.PrefixClientTitle = itemMember.Client.PrefixClientTitle;
                                    newMemberMovementClient.EndfixClientTitle = itemMember.Client.EndfixClientTitle;
                                    newMemberMovementClient.BirthDate = itemMember.Client.BirthDate;
                                    newMemberMovementClient.BirthPlace = itemMember.Client.BirthPlace;
                                    newMemberMovementClient.IdNumber = itemMember.Client.IdNumber;
                                    newMemberMovementClient.MaritalStatus = itemMember.Client.MaritalStatus;
                                    newMemberMovementClient.Sex = itemMember.Client.Sex;
                                    newMemberMovementClient.Email = itemMember.Client.Email;
                                    newMemberMovementClient.EmailAddress1 = itemMember.Client.EmailAddress1;
                                    newMemberMovementClient.EmailAddress2 = itemMember.Client.EmailAddress2;
                                    newMemberMovementClient.MobilePhone1 = itemMember.Client.MobilePhone1;
                                    newMemberMovementClient.MObilePhone2 = itemMember.Client.MObilePhone2;
                                    newMemberMovementClient.MobilePhone3 = itemMember.Client.MobilePhone3;
                                    newMemberMovementClient.ClientRelation = itemMember.Client.ClientRelation;
                                    newMemberMovementClient.RelateTo = itemMember.Client.RelateTo;
                                    newMemberMovementClient.BankAccountNumber = itemMember.Client.BankAccountNumber;
                                    newMemberMovementClient.BankAccountCode = itemMember.Client.BankAccountCode;
                                    newMemberMovementClient.BankAccountName = itemMember.Client.BankAccountName;
                                    newMemberMovementClient.Status = itemMember.Client.Status;
                                    newMemberMovementClient.Address = itemMember.Client.Address;
                                    newMemberMovementClient.SetPropertyCreate();

                                    db.Member_Movement_Client.Add(newMemberMovementClient);
                                    db.SaveChanges();
                                }



                                //1b.[Execution] Pindahkan MemberPlanDetailEndorse ke MemberPlanDetail
                                //foreach (var item in endorsement.PlanDetail_Endorse)
                                //{
                                //    var planDetail = new PlanDetail();
                                //    planDetail.PolicyId = item.PolicyId;
                                //    planDetail.PlanId = item.PlanId;
                                //    planDetail.BasicProductId = item.BasicProductId;
                                //    planDetail.BasicProductLimitCode = item.BasicProductLimitCode;
                                //    planDetail.SetPropertyCreate();
                                //    db.PlanDetail.Add(planDetail);
                                //}
                                db.SaveChanges();

                                //4.[Execution] Salin PlanEndorse ke Plan
                                foreach (var item in endorsement.Plan_Endorse)
                                {

                                    var newPlanToInsert = endorsement.Policy.Plan.Where(x => x.PlanId == item.PlanId).FirstOrDefault();
                                    if (newPlanToInsert == null)
                                    {
                                        var newPlan = new Plan();
                                        newPlan.PolicyId = item.PolicyId;
                                        newPlan.PlanId = item.PlanId;
                                        newPlan.PlanName = item.PlanName;
                                        newPlan.PlanDesc = item.PlanDesc;
                                        newPlan.StartDate = item.StartDate;
                                        newPlan.SetPropertyCreate();
                                        db.Plan.Add(newPlan);

                                        //5. Salin PlanDetailEndorse ke Plan Detail
                                        foreach (var itemPlanDetail in endorsement.PlanDetail_Endorse.Where(x => x.PlanId == item.PlanId))
                                        {
                                            var newPlanDetail = new PlanDetail();
                                            newPlanDetail.PolicyId = itemPlanDetail.PolicyId;
                                            newPlanDetail.PlanId = item.PlanId;
                                            newPlanDetail.BasicProductId = itemPlanDetail.BasicProductId;
                                            newPlanDetail.BasicProductLimitCode = itemPlanDetail.BasicProductLimitCode;
                                            newPlanDetail.SetPropertyCreate();
                                            db.PlanDetail.Add(newPlanDetail);
                                        }
                                    }

                                }
                                db.Plan_Endorse.RemoveRange(endorsement.Plan_Endorse);
                                db.SaveChanges();

                                // Generate Finance Transaction Detail
                                foreach (var item in allPolicyPCFEndorse.GroupBy(x => x.BasicProductId))
                                {

                                    var newFinanceTransactionDetail = new FinanceTransactionDetail();
                                    newFinanceTransactionDetail.TransactionNumber = financeTransaction.TransactionNumber;
                                    newFinanceTransactionDetail.OutstandingAmount = item.Sum(x => x.Amount);
                                    newFinanceTransactionDetail.BasicProductId = item.FirstOrDefault().BasicProductId;
                                    newFinanceTransactionDetail.TransactionAmount = newFinanceTransactionDetail.OutstandingAmount;
                                    db.FinanceTransactionDetail.Add(newFinanceTransactionDetail);
                                    db.SaveChanges();
                                }


                                //7. [Execution] Update Endorsetype to Done


                                //Delete All Data From Table Relate To Endorse
                                // 1. Delete Policy_Endorse
                                db.Policy_Endorse.Remove(endorsement.Policy_Endorse);
                                // 2. Delete PCF_Endorse
                                db.PCF_Endorse.RemoveRange(endorsement.PCF_Endorse);
                                // 3. Delete PlanDetail_Endorse
                                db.PlanDetail_Endorse.RemoveRange(endorsement.PlanDetail_Endorse);
                                // 7. Delete MemberPlan_Endorse
                                db.MemberPlan_Endorse.RemoveRange(endorsement.MemberPlan_Endorse);
                                // 4. Delete PlanDetail
                                db.Plan_Endorse.RemoveRange(endorsement.Plan_Endorse);
                                // 5. Delete Member_Endorse
                                db.Member_Endorse.RemoveRange(endorsement.Member_Endorse);
                                // 6. Delete Endorse
                                //db.Endorsement.Remove(endorsement);
                                endorsement.EndorseStatus = EndorseStatus.Done;
                                db.Entry(endorsement).State = EntityState.Modified;
                                SuccessMessagesAdd(Message.ProcessSuccess);

                                db.SaveChanges();
                                dbTransaction.Commit();

                                return RedirectToAction(EndorseType.MovePlan);
                            }
                            catch (Exception e)
                            {

                                WarningMessagesAdd(Message.ProcessFail);
                                WarningMessagesAdd(e.MessageToList());
                                WarningMessagesDistinct();
                                return RedirectToAction(EndorseType.MovePlan);
                            }


                        }
                        else if (endorsement.EndorseType == EndorseType.Additional)
                        {
                            var memberMemberEndorsePair = new List<(Member member, Member_Endorse memberEndorse)>();

                            // Memindahkan data dari table Member_Endorse ke Member
                            foreach (var item in memberPolicy)
                            {
                                if (item.Client.RelateTo == null)
                                {
                                    var sequencialMemberNumber = db.AspSequential.Where(x => x.Name == AspSequentialName.MemberNumber).FirstOrDefault();

                                    var newMember = new Member();

                                    newMember.AdmedikaCode = item.AdmedikaCode;
                                    newMember.Age = item.Age;
                                    newMember.CardNumber = item.CardNumber;
                                    newMember.ClaimNumber = item.ClaimNumber;
                                    newMember.ClientId = item.ClientId;
                                    newMember.EndDate = item.EndDate;
                                    newMember.EndorseNumber = item.EndorseNumber;
                                    newMember.EntryDate = item.EntryDate;
                                    newMember.ExitDate = item.ExitDate;
                                    newMember.IsActive = item.IsActive;
                                    newMember.IssueDate = DateTime.Now;
                                    newMember.LastClaimDate = item.LastClaimDate;
                                    newMember.LastEndorseDate = item.LastEndorseDate;
                                    newMember.MemberNumber = (sequencialMemberNumber.LastSequential + 1).ToString().PadLeft(7, '0');
                                    newMember.MemberStatus = item.MemberStatus;
                                    newMember.PlanId = item.PlanId;
                                    newMember.PolicyId = item.PolicyId;
                                    //newMember.ProcessDate = item.ProcessDate;

                                    newMember.SequencialNo = item.SequencialNo;
                                    newMember.StartDate = item.StartDate;
                                    newMember.TerminateDate = item.TerminateDate;

                                    newMember.SetPropertyCreate();

                                    db.Member.Add(newMember);
                                    sequencialMemberNumber.LastSequential = sequencialMemberNumber.LastSequential + 1;
                                    sequencialMemberNumber.SetPropertyUpdate();
                                    db.Entry(sequencialMemberNumber).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    memberMemberEndorsePair.Add((newMember, item));
                                }
                                else
                                {

                                    var newMember = new Member();

                                    newMember.AdmedikaCode = item.AdmedikaCode;
                                    newMember.Age = item.Age;
                                    newMember.CardNumber = item.CardNumber;
                                    newMember.ClaimNumber = item.ClaimNumber;
                                    newMember.ClientId = item.ClientId;
                                    newMember.EndDate = item.EndDate;
                                    newMember.EndorseNumber = item.EndorseNumber;
                                    newMember.EntryDate = item.EntryDate;
                                    newMember.ExitDate = item.ExitDate;
                                    newMember.IsActive = item.IsActive;
                                    newMember.IssueDate = DateTime.Now;
                                    newMember.LastClaimDate = item.LastClaimDate;
                                    newMember.LastEndorseDate = item.LastEndorseDate;

                                    var memberRelateTo = db.Member.Where(x => x.ClientId == item.Client.RelateTo).FirstOrDefault();
                                    var memberEndorseRelateTo = memberPolicy.Where(x => x.ClientId == item.Client.RelateTo).FirstOrDefault();

                                    newMember.MemberNumber = (memberRelateTo != null ? memberRelateTo.MemberNumber : memberEndorseRelateTo?.MemberNumber) + "-" + item.SequencialNo;
                                    newMember.MemberStatus = item.MemberStatus;
                                    newMember.PlanId = item.PlanId;
                                    newMember.PolicyId = item.PolicyId;
                                    //newMember.ProcessDate = item.ProcessDate;

                                    newMember.SequencialNo = item.SequencialNo;
                                    newMember.StartDate = item.StartDate;
                                    newMember.TerminateDate = item.TerminateDate;

                                    newMember.SetPropertyCreate();

                                    db.Member.Add(newMember);

                                    db.SaveChanges();

                                    memberMemberEndorsePair.Add((newMember, item));

                                }




                            }

                            // Generate Finance Transaction
                            var allPolicyPCFEndorse = db.PCF_Endorse.Where(x => x.EndorseNumber == endorsement.EndorseNumber && x.PolicyId == policyId && x.InvoiceDate < DateTime.Now);

                            var financeTransaction = new FinanceTransaction();

                            var transactionNumberSeq = db.AspSequential.Where(x => x.Name == AspSequentialName.TransactionNumber).FirstOrDefault();
                            financeTransaction.RecordMode = 1;
                            financeTransaction.TransactionNumber = "TXTR-" + DateTime.Now.Year + "-" + (transactionNumberSeq.LastSequential + 1).ToString().PadLeft(6, '0');
                            transactionNumberSeq.LastSequential = transactionNumberSeq.LastSequential + 1;
                            transactionNumberSeq.SetPropertyUpdate();
                            db.SaveChanges();
                            db.Entry(transactionNumberSeq).State = System.Data.Entity.EntityState.Modified;
                            financeTransaction.EffectiveDate = policy.IssueDate;
                            financeTransaction.TransactionDate = allPolicyPCFEndorse.Min(x => x.InvoiceDate);
                            financeTransaction.DueDate = financeTransaction.EffectiveDate.Value.AddDays(30);
                            financeTransaction.TransCodeId = "Invoice";
                            financeTransaction.PolicyId = policy.PolicyId;
                            financeTransaction.PolicyNumber = policy.PolicyNumber;
                            financeTransaction.ReconStatus = "No";
                            financeTransaction.OutstandingAmount = allPolicyPCFEndorse.Sum(x => x.Amount);
                            financeTransaction.ClientId = policy.ClientId;
                            financeTransaction.ClientTransactionAmount = financeTransaction.OutstandingAmount;
                            financeTransaction.ClosingAgen = policy.Agent;
                            financeTransaction.TransDescription = "Invoice Policy " + policy.PolicyNumber + " Periode " + financeTransaction.TransactionDate.Value.ToShortDateString();
                            db.FinanceTransaction.Add(financeTransaction);
                            // Generate Finance Transaction Detail
                            foreach (var item in allPolicyPCFEndorse.GroupBy(x => x.BasicProductId))
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
                            // Generate PCF
                            foreach (var item in allPolicyPCFEndorse)
                            {
                                var memberEndorsePairInMember = memberMemberEndorsePair.Where(x => x.memberEndorse.MemberId == item.MemberId).FirstOrDefault();

                                var newPCF = new PCF();
                                newPCF.PolicyId = item.PolicyId;
                                newPCF.MemberId = memberEndorsePairInMember.member.MemberId;
                                newPCF.BasicProductId = item.BasicProductId;
                                newPCF.TransType = item.TransType;
                                newPCF.InvoiceDate = item.InvoiceDate;
                                newPCF.DueDate = item.DueDate;
                                newPCF.Amount = item.Amount;
                                newPCF.TransactionNumber = financeTransaction.TransactionNumber;
                                newPCF.SetPropertyCreate();
                                db.PCF.Add(newPCF);
                                db.SaveChanges();
                            }


                            policy = db.Policy.Find(policyId);
                            foreach (var item in memberMemberEndorsePair)
                            {
                                var member = policy.Member.Where(x => x.MemberId == item.member.MemberId).FirstOrDefault();
                                member.MemberStatus = MemberStatus.Active;
                                db.Entry(member).State = EntityState.Modified;
                            }
                            db.SaveChanges();
                            // Generate Member Movement
                            foreach (var item in memberMemberEndorsePair)
                            {
                                var newMemberMovement = new Member_Movement();
                                newMemberMovement.AdmedikaCode = item.member.AdmedikaCode;
                                newMemberMovement.Age = item.member.Age;
                                newMemberMovement.CardNumber = item.member.CardNumber;
                                newMemberMovement.ClaimNumber = item.member.ClaimNumber;
                                newMemberMovement.ClientId = item.member.ClientId;
                                newMemberMovement.EffectiveDate = DateTime.Now.Date;
                                newMemberMovement.EndDate = item.member.EndDate;
                                newMemberMovement.EndorseNumber = item.member.EndorseNumber;
                                newMemberMovement.EntryDate = item.member.EntryDate;
                                newMemberMovement.ExitDate = item.member.ExitDate;
                                newMemberMovement.IssueDate = item.member.IssueDate;
                                newMemberMovement.LastClaimDate = item.member.LastClaimDate;
                                newMemberMovement.LastEndorseDate = item.member.LastEndorseDate;
                                newMemberMovement.MemberId = item.member.MemberId;
                                newMemberMovement.MemberNumber = item.member.MemberNumber;
                                newMemberMovement.MemberStatus = item.member.MemberStatus;
                                newMemberMovement.PlanId = item.member.PlanId;
                                newMemberMovement.PolicyId = item.member.PolicyId;
                                //newMemberMovement.ProcessDate = item.member.ProcessDate;
                                if (endorsement.EndorseType == EndorseType.Additional)
                                {
                                    newMemberMovement.RecordMode = RecordModeMemberMovement.Additional;
                                }
                                else if (endorsement.EndorseType == EndorseType.MovePlan)
                                {
                                    newMemberMovement.RecordMode = RecordModeMemberMovement.MovePlan;
                                }
                                else if (endorsement.EndorseType == EndorseType.TerminateMember)
                                {
                                    newMemberMovement.RecordMode = RecordModeMemberMovement.TerminateMember;
                                }
                                newMemberMovement.SequencialNo = item.member.SequencialNo;
                                newMemberMovement.StartDate = item.member.StartDate;
                                newMemberMovement.TerminateDate = item.member.TerminateDate;

                                newMemberMovement.SetPropertyCreate();
                                db.Member_Movement.Add(newMemberMovement);
                                db.SaveChanges();

                                var newMemberMovementClient = new Member_Movement_Client();
                                newMemberMovementClient.MovementId = newMemberMovement.Id;
                                newMemberMovementClient.ClientId = item.member.Client.ClientId;
                                newMemberMovementClient.Type = item.member.Client.Type;
                                newMemberMovementClient.BranchCode = item.member.Client.BranchCode;
                                newMemberMovementClient.ContactPerson = item.member.Client.ContactPerson;
                                newMemberMovementClient.ShortName = item.member.Client.ShortName;
                                newMemberMovementClient.FullName = item.member.Client.FullName;
                                newMemberMovementClient.PrefixClientTitle = item.member.Client.PrefixClientTitle;
                                newMemberMovementClient.EndfixClientTitle = item.member.Client.EndfixClientTitle;
                                newMemberMovementClient.BirthDate = item.member.Client.BirthDate;
                                newMemberMovementClient.BirthPlace = item.member.Client.BirthPlace;
                                newMemberMovementClient.IdNumber = item.member.Client.IdNumber;
                                newMemberMovementClient.MaritalStatus = item.member.Client.MaritalStatus;
                                newMemberMovementClient.Sex = item.member.Client.Sex;
                                newMemberMovementClient.Email = item.member.Client.Email;
                                newMemberMovementClient.EmailAddress1 = item.member.Client.EmailAddress1;
                                newMemberMovementClient.EmailAddress2 = item.member.Client.EmailAddress2;
                                newMemberMovementClient.MobilePhone1 = item.member.Client.MobilePhone1;
                                newMemberMovementClient.MObilePhone2 = item.member.Client.MObilePhone2;
                                newMemberMovementClient.MobilePhone3 = item.member.Client.MobilePhone3;
                                newMemberMovementClient.ClientRelation = item.member.Client.ClientRelation;
                                newMemberMovementClient.RelateTo = item.member.Client.RelateTo;
                                newMemberMovementClient.BankAccountNumber = item.member.Client.BankAccountNumber;
                                newMemberMovementClient.BankAccountCode = item.member.Client.BankAccountCode;
                                newMemberMovementClient.BankAccountName = item.member.Client.BankAccountName;
                                newMemberMovementClient.Status = item.member.Client.Status;
                                newMemberMovementClient.Address = item.member.Client.Address;
                                newMemberMovementClient.SetPropertyCreate();

                                db.Member_Movement_Client.Add(newMemberMovementClient);
                                db.SaveChanges();
                            }
                            // Generate MemberPlan
                            foreach (var item in memberMemberEndorsePair)
                            {
                                var memberPlanEndorse = endorsement.MemberPlan_Endorse.Where(x => x.MemberId == item.memberEndorse.MemberId && x.EndorseNumber == endorsement.EndorseNumber).ToList();
                                foreach (var itemMemberPlan in memberPlanEndorse)
                                {
                                    var newMemberPlan = new MemberPlan();
                                    newMemberPlan.BasicProductId = itemMemberPlan.BasicProductId;
                                    newMemberPlan.BasicProductLimitCode = itemMemberPlan.BasicProductLimitCode;
                                    newMemberPlan.MemberId = item.member.MemberId;
                                    newMemberPlan.PlanId = item.member.PlanId;
                                    newMemberPlan.PolicyId = itemMemberPlan.PolicyId;
                                    newMemberPlan.EndorseNumber = itemMemberPlan.EndorseNumber;
                                    newMemberPlan.StartDate = itemMemberPlan.StartDate;
                                    newMemberPlan.SetPropertyCreate(itemMemberPlan.IsActive.Value);
                                    db.MemberPlan.Add(newMemberPlan);
                                }

                            }
                            db.SaveChanges();

                            // Generate Plan
                            var planEndorse = endorsement.Plan_Endorse;
                            foreach (var item in planEndorse)
                            {
                                var plan = db.Plan.Where(x => x.PolicyId == item.PolicyId && x.PlanId == item.PlanId).FirstOrDefault();
                                if (plan == null)
                                {
                                    var newPlan = new Plan();
                                    newPlan.PlanId = item.PlanId;
                                    newPlan.PlanDesc = item.PlanDesc;
                                    newPlan.PolicyId = item.PolicyId;
                                    newPlan.PlanName = item.PlanName;
                                    newPlan.StartDate = endorsement.EndorseDate;
                                    newPlan.SetPropertyCreate();
                                    db.Plan.Add(newPlan);


                                    // Generating PlanDetail if there are Endorse PlanDetails exist. 
                                    var planDetails = endorsement.PlanDetail_Endorse.Where(x => x.PlanId == item.PlanId).ToList();
                                    foreach (var planDetailEndorse in planDetails)
                                    {
                                        var newPlanDetail = new PlanDetail();
                                        newPlanDetail.PolicyId = planDetailEndorse.PolicyId;
                                        newPlanDetail.PlanId = planDetailEndorse.PlanId;
                                        newPlanDetail.BasicProductId = planDetailEndorse.BasicProductId;
                                        newPlanDetail.BasicProductLimitCode = planDetailEndorse.BasicProductLimitCode;
                                        newPlanDetail.SetPropertyCreate();
                                        db.PlanDetail.Add(newPlanDetail);
                                    }

                                }
                            }
                            db.SaveChanges();

                            // Update Policy To Active
                            policy.SetPropertyUpdate();

                            policy.LastEndorseDate = endorsement.EndorseDate;
                            policy.EndorseNumber = endorsement.EndorseNumber;
                            policy.PolicyStatus = PolicyStatus.Active;
                            db.Entry(policy).State = EntityState.Modified;




                            //Delete All Data From Table Relate To Endorse
                            // 1. Delete Policy_Endorse
                            db.Policy_Endorse.Remove(endorsement.Policy_Endorse);
                            // 2. Delete PCF_Endorse
                            db.PCF_Endorse.RemoveRange(endorsement.PCF_Endorse);
                            // 3. Delete PlanDetail_Endorse
                            db.PlanDetail_Endorse.RemoveRange(endorsement.PlanDetail_Endorse);
                            // 7. Delete MemberPlan_Endorse
                            db.MemberPlan_Endorse.RemoveRange(endorsement.MemberPlan_Endorse);
                            // 4. Delete PlanDetail
                            db.Plan_Endorse.RemoveRange(endorsement.Plan_Endorse);
                            // 5. Delete Member_Endorse
                            db.Member_Endorse.RemoveRange(endorsement.Member_Endorse);

                            endorsement.EndorseStatus = EndorseStatus.Done;
                            db.Entry(endorsement).State = EntityState.Modified;
                            db.SaveChanges();
                            dbTransaction.Commit();
                            SuccessMessagesAdd(Message.ProcessSuccess);
                            return RedirectToAction(EndorseType.Additional);

                        }
                        else if (endorsement.EndorseType == EndorseType.TerminateMember)
                        {
                            //Pada saat terminate, setiap member akan diubah statusnya ke terminate.
                            //pcf akan direfund atau cancel, tergantung jenis dari endorse terminatenya.
                            //data member akan masuk ke membermovement
                            //data client juga akan masuk ke client movement
                            // akan ada kalkulasi pcf, ini terkait dengan refund yang diterima member apabila terminate atau cancel
                            // memberstatus dari member akan diset nilainya antra Refund atau Cancel

                            var memberMemberEndorsePair = new List<(Member member, Member_Endorse memberEndorse)>();
                            foreach (var item in endorsement.Member_Endorse)
                            {
                                var member = db.Member.Where(x => x.MemberNumber == item.MemberNumber).FirstOrDefault();
                                memberMemberEndorsePair.Add((member, item));
                            }

                            // Generate FinanceTransaction

                            var financeTransaction = new FinanceTransaction();

                            var transactionNumberSeq = db.AspSequential.Where(x => x.Name == AspSequentialName.TransactionNumber).FirstOrDefault();
                            financeTransaction.RecordMode = 1;
                            financeTransaction.TransactionNumber = "TXTR-" + DateTime.Now.Year + "-" + (transactionNumberSeq.LastSequential + 1).ToString().PadLeft(6, '0');
                            transactionNumberSeq.LastSequential = transactionNumberSeq.LastSequential + 1;
                            transactionNumberSeq.SetPropertyUpdate();
                            db.SaveChanges();
                            db.Entry(transactionNumberSeq).State = EntityState.Modified;
                            financeTransaction.EffectiveDate = policy.IssueDate;
                            financeTransaction.TransactionDate = endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null).Min(x => x.InvoiceDate);
                            financeTransaction.DueDate = financeTransaction.EffectiveDate.Value.AddDays(30);
                            financeTransaction.TransCodeId = "Invoice";
                            financeTransaction.PolicyId = policy.PolicyId;
                            financeTransaction.PolicyNumber = policy.PolicyNumber;
                            financeTransaction.ReconStatus = "No";
                            financeTransaction.OutstandingAmount = endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null).Sum(x => x.Amount);
                            financeTransaction.ClientId = policy.ClientId;
                            financeTransaction.ClientTransactionAmount = financeTransaction.OutstandingAmount;
                            financeTransaction.ClosingAgen = policy.Agent;
                            financeTransaction.TransDescription = "Invoice Policy " + policy.PolicyNumber + " Periode " + financeTransaction.TransactionDate.Value.ToShortDateString();
                            db.FinanceTransaction.Add(financeTransaction);
                            // Generate Finance Transaction Detail
                            foreach (var item in endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null).GroupBy(x => x.BasicProductId))
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
                            // Generate PCF
                            foreach (var item in endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null))
                            {
                                var memberEndorsePairInMember = memberMemberEndorsePair.Where(x => x.memberEndorse.MemberId == item.MemberId).FirstOrDefault();

                                var newPCF = new PCF();
                                newPCF.PolicyId = item.PolicyId;
                                newPCF.MemberId = memberEndorsePairInMember.member.MemberId;
                                newPCF.BasicProductId = item.BasicProductId;
                                newPCF.TransType = item.TransType;
                                newPCF.InvoiceDate = item.InvoiceDate;
                                newPCF.DueDate = item.DueDate;
                                newPCF.Amount = item.Amount;
                                newPCF.TransactionNumber = financeTransaction.TransactionNumber;
                                newPCF.SetPropertyCreate();
                                db.PCF.Add(newPCF);
                                db.SaveChanges();
                            }

                            //Generating data to MemberMovement dan ClientMovement
                            foreach (var pairMemberToMemberEndorse in memberMemberEndorsePair)
                            {
                                var newMemberMovement = new Member_Movement();
                                var newMemberMovementClient = new Member_Movement_Client();

                                var itemMember = pairMemberToMemberEndorse.member;

                                newMemberMovement.AdmedikaCode = itemMember.AdmedikaCode;
                                newMemberMovement.Age = itemMember.Age;
                                newMemberMovement.CardNumber = itemMember.CardNumber;
                                newMemberMovement.ClaimNumber = itemMember.ClaimNumber;
                                newMemberMovement.ClientId = itemMember.ClientId;
                                newMemberMovement.EffectiveDate = DateTime.Now.Date;
                                newMemberMovement.EndDate = itemMember.EndDate;
                                newMemberMovement.EndorseNumber = pairMemberToMemberEndorse.memberEndorse.EndorseNumber;
                                newMemberMovement.EntryDate = itemMember.EntryDate;
                                newMemberMovement.ExitDate = itemMember.ExitDate;
                                newMemberMovement.IssueDate = itemMember.IssueDate;
                                newMemberMovement.LastClaimDate = itemMember.LastClaimDate;
                                newMemberMovement.LastEndorseDate = itemMember.LastEndorseDate;
                                newMemberMovement.MemberId = itemMember.MemberId;
                                newMemberMovement.MemberNumber = itemMember.MemberNumber;
                                newMemberMovement.MemberStatus = itemMember.MemberStatus;
                                newMemberMovement.PlanId = itemMember.PlanId;
                                newMemberMovement.PolicyId = itemMember.PolicyId;
                                //newMemberMovement.ProcessDate = itemMember.ProcessDate;

                                newMemberMovement.RecordMode = RecordModeMemberMovement.TerminateMember;

                                newMemberMovement.SequencialNo = itemMember.SequencialNo;
                                newMemberMovement.StartDate = itemMember.StartDate;
                                newMemberMovement.TerminateDate = endorsement.EndorseDate;

                                newMemberMovement.SetPropertyCreate();
                                db.Member_Movement.Add(newMemberMovement);
                                db.SaveChanges();

                                newMemberMovementClient.MovementId = newMemberMovement.Id;
                                newMemberMovementClient.ClientId = itemMember.Client.ClientId;
                                newMemberMovementClient.Type = itemMember.Client.Type;
                                newMemberMovementClient.BranchCode = itemMember.Client.BranchCode;
                                newMemberMovementClient.ContactPerson = itemMember.Client.ContactPerson;
                                newMemberMovementClient.ShortName = itemMember.Client.ShortName;
                                newMemberMovementClient.FullName = itemMember.Client.FullName;
                                newMemberMovementClient.PrefixClientTitle = itemMember.Client.PrefixClientTitle;
                                newMemberMovementClient.EndfixClientTitle = itemMember.Client.EndfixClientTitle;
                                newMemberMovementClient.BirthDate = itemMember.Client.BirthDate;
                                newMemberMovementClient.BirthPlace = itemMember.Client.BirthPlace;
                                newMemberMovementClient.IdNumber = itemMember.Client.IdNumber;
                                newMemberMovementClient.MaritalStatus = itemMember.Client.MaritalStatus;
                                newMemberMovementClient.Sex = itemMember.Client.Sex;
                                newMemberMovementClient.Email = itemMember.Client.Email;
                                newMemberMovementClient.EmailAddress1 = itemMember.Client.EmailAddress1;
                                newMemberMovementClient.EmailAddress2 = itemMember.Client.EmailAddress2;
                                newMemberMovementClient.MobilePhone1 = itemMember.Client.MobilePhone1;
                                newMemberMovementClient.MObilePhone2 = itemMember.Client.MObilePhone2;
                                newMemberMovementClient.MobilePhone3 = itemMember.Client.MobilePhone3;
                                newMemberMovementClient.ClientRelation = itemMember.Client.ClientRelation;
                                newMemberMovementClient.RelateTo = itemMember.Client.RelateTo;
                                newMemberMovementClient.BankAccountNumber = itemMember.Client.BankAccountNumber;
                                newMemberMovementClient.BankAccountCode = itemMember.Client.BankAccountCode;
                                newMemberMovementClient.BankAccountName = itemMember.Client.BankAccountName;
                                newMemberMovementClient.Status = itemMember.Client.Status;
                                newMemberMovementClient.Address = itemMember.Client.Address;
                                newMemberMovementClient.SetPropertyCreate();

                                db.Member_Movement_Client.Add(newMemberMovementClient);

                            }


                            //Delete All Data From Table Relate To Endorse
                            // 1. Delete Policy_Endorse
                            db.Policy_Endorse.Remove(endorsement.Policy_Endorse);
                            // 2. Delete PCF_Endorse
                            db.PCF_Endorse.RemoveRange(endorsement.PCF_Endorse);
                            // 3. Delete PlanDetail_Endorse
                            db.PlanDetail_Endorse.RemoveRange(endorsement.PlanDetail_Endorse);
                            // 7. Delete MemberPlan_Endorse
                            db.MemberPlan_Endorse.RemoveRange(endorsement.MemberPlan_Endorse);
                            // 4. Delete PlanDetail
                            db.Plan_Endorse.RemoveRange(endorsement.Plan_Endorse);
                            // 5. Delete Member_Endorse
                            db.Member_Endorse.RemoveRange(endorsement.Member_Endorse);


                            // 4 Feb 2020, update member to terminate
                            foreach (var item in memberMemberEndorsePair)
                            {
                                if (item.memberEndorse.MemberStatus == "Death" || item.memberEndorse.MemberStatus == "Death " + MemberStatus.Calculated) {
                                    item.member.MemberStatus = MemberStatus.TerminateDeath;
                                }
                                else
                                {
                                    if (item.memberEndorse.StartDate < item.memberEndorse.TerminateDate && item.memberEndorse.EndDate > item.memberEndorse.TerminateDate)
                                    {
                                        item.member.MemberStatus = MemberStatus.TerminateRefund;
                                        item.member.TerminateDate = item.member.TerminateDate;
                                    }
                                    else if (item.memberEndorse.StartDate == item.memberEndorse.TerminateDate)
                                    {
                                        item.member.MemberStatus = MemberStatus.TerminateCancel;
                                        item.member.TerminateDate = item.member.TerminateDate;
                                    }
                                    else
                                    {
                                        WarningMessagesAdd(Message.ProcessFail);
                                    }
                                }
                                

                                db.Entry(item.member).State = EntityState.Modified;
                            }


                            endorsement.EndorseStatus = EndorseStatus.Done;
                            db.Entry(endorsement).State = EntityState.Modified;

                            db.SaveChanges();
                            if (WarningMessages().Count == 0)
                            {
                                dbTransaction.Commit();
                            }
                            else
                            {
                                SuccessMessagesAdd(Message.ProcessSuccess);
                                dbTransaction.Rollback();
                            }

                            return RedirectToAction(EndorseType.TerminateMember);
                        }
                        else if (endorsement.EndorseType == EndorseType.Mutation)
                        {
                            // PROCESS TERMINATE 

                            //1. Generate Finance Transaction
                            //2. Update PCF transaction number with generated number
                            //3. Generated finance transaction detail
                            //4. updated policy

                            var memberMemberEndorsePairTerminate = new List<(Member member, Member_Endorse memberEndorse)>();
                            foreach (var item in endorsement.Member_Endorse)
                            {
                                var member = db.Member.Where(x => x.MemberNumber == item.MemberNumber).FirstOrDefault();
                                memberMemberEndorsePairTerminate.Add((member, item));
                            }

                            // Generate FinanceTransaction

                            var financeTransactionTerminate = new FinanceTransaction();

                            var transactionNumberSeqTerminate = db.AspSequential.Where(x => x.Name == AspSequentialName.TransactionNumber).FirstOrDefault();
                            financeTransactionTerminate.RecordMode = RecordModeMemberMovement.TerminateMember;
                            financeTransactionTerminate.TransactionNumber = "TXTR-" + DateTime.Now.Year + "-" + (transactionNumberSeqTerminate.LastSequential + 1).ToString().PadLeft(6, '0');
                            transactionNumberSeqTerminate.LastSequential = transactionNumberSeqTerminate.LastSequential + 1;
                            transactionNumberSeqTerminate.SetPropertyUpdate();
                            db.SaveChanges();
                            db.Entry(transactionNumberSeqTerminate).State = EntityState.Modified;
                            financeTransactionTerminate.EffectiveDate = policy.IssueDate;
                            financeTransactionTerminate.TransactionDate = endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null).Min(x => x.InvoiceDate);
                            financeTransactionTerminate.DueDate = financeTransactionTerminate.EffectiveDate.Value.AddDays(30);
                            financeTransactionTerminate.TransCodeId = "Invoice";
                            financeTransactionTerminate.PolicyId = policy.PolicyId;
                            financeTransactionTerminate.PolicyNumber = policy.PolicyNumber;
                            financeTransactionTerminate.ReconStatus = "No";
                            financeTransactionTerminate.OutstandingAmount = endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null).Sum(x => x.Amount);
                            financeTransactionTerminate.ClientId = policy.ClientId;
                            financeTransactionTerminate.ClientTransactionAmount = financeTransactionTerminate.OutstandingAmount;
                            financeTransactionTerminate.ClosingAgen = policy.Agent;
                            financeTransactionTerminate.TransDescription = "Invoice Policy " + policy.PolicyNumber + " Periode " + financeTransactionTerminate.TransactionDate.Value.ToShortDateString();
                            db.FinanceTransaction.Add(financeTransactionTerminate);
                            // Generate Finance Transaction Detail
                            foreach (var item in endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null).GroupBy(x => x.BasicProductId))
                            {

                                var newFinanceTransactionDetail = new FinanceTransactionDetail();
                                newFinanceTransactionDetail.TransactionNumber = financeTransactionTerminate.TransactionNumber;
                                newFinanceTransactionDetail.OutstandingAmount = item.Sum(x => x.Amount);
                                newFinanceTransactionDetail.BasicProductId = item.FirstOrDefault().BasicProductId;
                                newFinanceTransactionDetail.TransactionAmount = newFinanceTransactionDetail.OutstandingAmount;
                                //newFinanceTransactionDetail.BankAmount
                                db.FinanceTransactionDetail.Add(newFinanceTransactionDetail);

                                db.SaveChanges();
                            }
                            // Generate PCF
                            foreach (var item in endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null))
                            {
                                var memberEndorsePairInMember = memberMemberEndorsePairTerminate.Where(x => x.memberEndorse.MemberId == item.MemberId).FirstOrDefault();

                                var newPCF = new PCF();
                                newPCF.PolicyId = item.PolicyId;
                                newPCF.MemberId = memberEndorsePairInMember.member.MemberId;
                                newPCF.BasicProductId = item.BasicProductId;
                                newPCF.TransType = item.TransType;
                                newPCF.InvoiceDate = endorsement.EndorseDate.Value;
                                newPCF.DueDate = item.DueDate;
                                newPCF.Amount = item.Amount;
                                newPCF.TransactionNumber = financeTransactionTerminate.TransactionNumber;
                                newPCF.SetPropertyCreate();
                                db.PCF.Add(newPCF);
                                db.SaveChanges();
                            }

                            //Generating data to MemberMovement dan ClientMovement
                            foreach (var pairMemberToMemberEndorse in memberMemberEndorsePairTerminate)
                            {
                                var newMemberMovement = new Member_Movement();
                                var newMemberMovementClient = new Member_Movement_Client();

                                var itemMember = pairMemberToMemberEndorse.member;

                                newMemberMovement.AdmedikaCode = itemMember.AdmedikaCode;
                                newMemberMovement.Age = itemMember.Age;
                                newMemberMovement.CardNumber = itemMember.CardNumber;
                                newMemberMovement.ClaimNumber = itemMember.ClaimNumber;
                                newMemberMovement.ClientId = itemMember.ClientId;
                                newMemberMovement.EffectiveDate = DateTime.Now.Date;
                                newMemberMovement.EndDate = itemMember.EndDate;
                                newMemberMovement.EndorseNumber = pairMemberToMemberEndorse.memberEndorse.EndorseNumber;
                                newMemberMovement.EntryDate = itemMember.EntryDate;
                                newMemberMovement.ExitDate = itemMember.ExitDate;
                                newMemberMovement.IssueDate = itemMember.IssueDate;
                                newMemberMovement.LastClaimDate = itemMember.LastClaimDate;
                                newMemberMovement.LastEndorseDate = itemMember.LastEndorseDate;
                                newMemberMovement.MemberId = itemMember.MemberId;
                                newMemberMovement.MemberNumber = itemMember.MemberNumber;
                                newMemberMovement.MemberStatus = itemMember.MemberStatus;
                                newMemberMovement.PlanId = itemMember.PlanId;
                                newMemberMovement.PolicyId = itemMember.PolicyId;
                                //newMemberMovement.ProcessDate = itemMember.ProcessDate;

                                newMemberMovement.RecordMode = RecordModeMemberMovement.TerminateMember;

                                newMemberMovement.SequencialNo = itemMember.SequencialNo;
                                newMemberMovement.StartDate = itemMember.StartDate;
                                newMemberMovement.TerminateDate = endorsement.EndorseDate;

                                newMemberMovement.SetPropertyCreate();
                                db.Member_Movement.Add(newMemberMovement);
                                db.SaveChanges();

                                newMemberMovementClient.MovementId = newMemberMovement.Id;
                                newMemberMovementClient.ClientId = itemMember.Client.ClientId;
                                newMemberMovementClient.Type = itemMember.Client.Type;
                                newMemberMovementClient.BranchCode = itemMember.Client.BranchCode;
                                newMemberMovementClient.ContactPerson = itemMember.Client.ContactPerson;
                                newMemberMovementClient.ShortName = itemMember.Client.ShortName;
                                newMemberMovementClient.FullName = itemMember.Client.FullName;
                                newMemberMovementClient.PrefixClientTitle = itemMember.Client.PrefixClientTitle;
                                newMemberMovementClient.EndfixClientTitle = itemMember.Client.EndfixClientTitle;
                                newMemberMovementClient.BirthDate = itemMember.Client.BirthDate;
                                newMemberMovementClient.BirthPlace = itemMember.Client.BirthPlace;
                                newMemberMovementClient.IdNumber = itemMember.Client.IdNumber;
                                newMemberMovementClient.MaritalStatus = itemMember.Client.MaritalStatus;
                                newMemberMovementClient.Sex = itemMember.Client.Sex;
                                newMemberMovementClient.Email = itemMember.Client.Email;
                                newMemberMovementClient.EmailAddress1 = itemMember.Client.EmailAddress1;
                                newMemberMovementClient.EmailAddress2 = itemMember.Client.EmailAddress2;
                                newMemberMovementClient.MobilePhone1 = itemMember.Client.MobilePhone1;
                                newMemberMovementClient.MObilePhone2 = itemMember.Client.MObilePhone2;
                                newMemberMovementClient.MobilePhone3 = itemMember.Client.MobilePhone3;
                                newMemberMovementClient.ClientRelation = itemMember.Client.ClientRelation;
                                newMemberMovementClient.RelateTo = itemMember.Client.RelateTo;
                                newMemberMovementClient.BankAccountNumber = itemMember.Client.BankAccountNumber;
                                newMemberMovementClient.BankAccountCode = itemMember.Client.BankAccountCode;
                                newMemberMovementClient.BankAccountName = itemMember.Client.BankAccountName;
                                newMemberMovementClient.Status = itemMember.Client.Status;
                                newMemberMovementClient.Address = itemMember.Client.Address;
                                newMemberMovementClient.SetPropertyCreate();

                                db.Member_Movement_Client.Add(newMemberMovementClient);

                            }



                            // Handle Each Additional Member





                            foreach (var itemEndorsementChild in endorsement.Endorsement1)
                            {

                                var memberMemberEndorsePairitemEndorsementChild = new List<(Member member, Member_Endorse memberEndorse)>();

                                // Memindahkan data dari table Member_Endorse ke Member

                                foreach (var item in itemEndorsementChild.Member_Endorse.Where(x => x.Client.RelateTo == null))
                                {

                                    var sequencialMemberNumber = db.AspSequential.Where(x => x.Name == AspSequentialName.MemberNumber).FirstOrDefault();

                                    var newMember = new Member();

                                    newMember.AdmedikaCode = item.AdmedikaCode;
                                    newMember.Age = item.Age;
                                    newMember.CardNumber = item.CardNumber;
                                    newMember.ClaimNumber = item.ClaimNumber;
                                    newMember.ClientId = item.ClientId;
                                    newMember.EndDate = item.EndDate;
                                    newMember.EndorseNumber = item.EndorseNumber;
                                    newMember.EntryDate = item.EntryDate;
                                    newMember.ExitDate = item.ExitDate;
                                    newMember.IsActive = item.IsActive;
                                    newMember.IssueDate = DateTime.Now;
                                    newMember.LastClaimDate = item.LastClaimDate;
                                    newMember.LastEndorseDate = item.LastEndorseDate;
                                    newMember.MemberNumber = (sequencialMemberNumber.LastSequential + 1).ToString().PadLeft(7, '0');
                                    newMember.MemberStatus = item.MemberStatus;
                                    newMember.PlanId = item.PlanId;
                                    newMember.PolicyId = item.PolicyId;
                                    //newMember.ProcessDate = item.ProcessDate;

                                    newMember.SequencialNo = item.SequencialNo;
                                    newMember.StartDate = item.StartDate;
                                    newMember.TerminateDate = item.TerminateDate;

                                    newMember.SetPropertyCreate();

                                    db.Member.Add(newMember);
                                    sequencialMemberNumber.LastSequential = sequencialMemberNumber.LastSequential + 1;
                                    sequencialMemberNumber.SetPropertyUpdate();
                                    db.Entry(sequencialMemberNumber).State = EntityState.Modified;
                                    db.SaveChanges();
                                    memberMemberEndorsePairitemEndorsementChild.Add((newMember, item));

                                }

                                foreach (var item in itemEndorsementChild.Member_Endorse.Where(x => x.Client.RelateTo != null))
                                {


                                    var newMember = new Member();

                                    newMember.AdmedikaCode = item.AdmedikaCode;
                                    newMember.Age = item.Age;
                                    newMember.CardNumber = item.CardNumber;
                                    newMember.ClaimNumber = item.ClaimNumber;
                                    newMember.ClientId = item.ClientId;
                                    newMember.EndDate = item.EndDate;
                                    newMember.EndorseNumber = item.EndorseNumber;
                                    newMember.EntryDate = item.EntryDate;
                                    newMember.ExitDate = item.ExitDate;
                                    newMember.IsActive = item.IsActive;
                                    newMember.IssueDate = DateTime.Now;
                                    newMember.LastClaimDate = item.LastClaimDate;
                                    newMember.LastEndorseDate = item.LastEndorseDate;
                                    var memberRelateToInSameEndorse = db.Member.Where(x => x.ClientId == item.Client.RelateTo && x.PolicyId == item.PolicyId).FirstOrDefault();
                                    var memberEndorseRelateTo = memberPolicy.Where(x => x.ClientId == item.Client.RelateTo).FirstOrDefault();

                                    // MemberNumber akan digenerate kembali apabila relateto-nya tidak berada dalam 1 polis.
                                    if (memberRelateToInSameEndorse != null)
                                    {
                                        newMember.MemberNumber = memberRelateToInSameEndorse?.MemberNumber + "-" + item.SequencialNo;
                                    }
                                    else
                                    {
                                        var sequencialMemberNumber = db.AspSequential.Where(x => x.Name == AspSequentialName.MemberNumber).FirstOrDefault();
                                        newMember.MemberNumber = (sequencialMemberNumber.LastSequential + 1).ToString().PadLeft(7, '0');
                                        sequencialMemberNumber.LastSequential = sequencialMemberNumber.LastSequential + 1;
                                        sequencialMemberNumber.SetPropertyUpdate();
                                        db.Entry(sequencialMemberNumber).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    newMember.MemberStatus = item.MemberStatus;
                                    newMember.PlanId = item.PlanId;
                                    newMember.PolicyId = item.PolicyId;
                                    //newMember.ProcessDate = item.ProcessDate;

                                    newMember.SequencialNo = item.SequencialNo;
                                    newMember.StartDate = item.StartDate;
                                    newMember.TerminateDate = item.TerminateDate;

                                    newMember.SetPropertyCreate();

                                    db.Member.Add(newMember);

                                    db.SaveChanges();

                                    memberMemberEndorsePairitemEndorsementChild.Add((newMember, item));

                                }

                                // Generate Finance Transaction
                                var allPolicyPCFEndorseitemEndorsementChild = db.PCF_Endorse.Where(x => x.EndorseNumber == itemEndorsementChild.EndorseNumber && x.PolicyId == itemEndorsementChild.PolicyId && x.InvoiceDate < DateTime.Now).ToList();

                                var financeTransactionitemEndorsementChild = new FinanceTransaction();

                                var transactionNumberSeqitemEndorsementChild = db.AspSequential.Where(x => x.Name == AspSequentialName.TransactionNumber).FirstOrDefault();
                                financeTransactionitemEndorsementChild.RecordMode = 1;
                                financeTransactionitemEndorsementChild.TransactionNumber = "TXTR-" + DateTime.Now.Year + "-" + (transactionNumberSeqitemEndorsementChild.LastSequential + 1).ToString().PadLeft(6, '0');
                                transactionNumberSeqitemEndorsementChild.LastSequential = transactionNumberSeqitemEndorsementChild.LastSequential + 1;
                                transactionNumberSeqitemEndorsementChild.SetPropertyUpdate();
                                db.SaveChanges();
                                db.Entry(transactionNumberSeqitemEndorsementChild).State = System.Data.Entity.EntityState.Modified;
                                financeTransactionitemEndorsementChild.EffectiveDate = policy.IssueDate;
                                financeTransactionitemEndorsementChild.TransactionDate = allPolicyPCFEndorseitemEndorsementChild.Min(x => x.InvoiceDate);
                                financeTransactionitemEndorsementChild.DueDate = financeTransactionitemEndorsementChild.EffectiveDate.Value.AddDays(30);
                                financeTransactionitemEndorsementChild.TransCodeId = "Invoice";
                                financeTransactionitemEndorsementChild.PolicyId = policy.PolicyId;
                                financeTransactionitemEndorsementChild.PolicyNumber = policy.PolicyNumber;
                                financeTransactionitemEndorsementChild.ReconStatus = "No";
                                financeTransactionitemEndorsementChild.OutstandingAmount = allPolicyPCFEndorseitemEndorsementChild.Sum(x => x.Amount);
                                financeTransactionitemEndorsementChild.ClientId = policy.ClientId;
                                financeTransactionitemEndorsementChild.ClientTransactionAmount = financeTransactionitemEndorsementChild.OutstandingAmount;
                                financeTransactionitemEndorsementChild.ClosingAgen = policy.Agent;
                                financeTransactionitemEndorsementChild.TransDescription = "Invoice Policy " + policy.PolicyNumber + " Periode " + financeTransactionitemEndorsementChild.TransactionDate.Value.ToShortDateString();
                                db.FinanceTransaction.Add(financeTransactionitemEndorsementChild);
                                // Generate Finance Transaction Detail
                                foreach (var item in allPolicyPCFEndorseitemEndorsementChild.GroupBy(x => x.BasicProductId))
                                {

                                    var newFinanceTransactionDetail = new FinanceTransactionDetail();
                                    newFinanceTransactionDetail.TransactionNumber = financeTransactionitemEndorsementChild.TransactionNumber;
                                    newFinanceTransactionDetail.OutstandingAmount = item.Sum(x => x.Amount);
                                    newFinanceTransactionDetail.BasicProductId = item.FirstOrDefault().BasicProductId;
                                    newFinanceTransactionDetail.TransactionAmount = newFinanceTransactionDetail.OutstandingAmount;
                                    //newFinanceTransactionDetail.BankAmount
                                    db.FinanceTransactionDetail.Add(newFinanceTransactionDetail);

                                    db.SaveChanges();
                                }
                                // Generate PCF
                                foreach (var item in allPolicyPCFEndorseitemEndorsementChild)
                                {
                                    var memberEndorsePairInMember = memberMemberEndorsePairitemEndorsementChild.Where(x => x.memberEndorse.MemberId == item.MemberId).FirstOrDefault();

                                    var newPCF = new PCF();
                                    newPCF.PolicyId = item.PolicyId;
                                    newPCF.MemberId = memberEndorsePairInMember.member.MemberId;
                                    newPCF.BasicProductId = item.BasicProductId;
                                    newPCF.TransType = item.TransType;
                                    newPCF.InvoiceDate = item.Endorsement.EndorseDate.Value;
                                    newPCF.DueDate = item.DueDate;
                                    newPCF.Amount = item.Amount;
                                    newPCF.TransactionNumber = financeTransactionitemEndorsementChild.TransactionNumber;
                                    newPCF.SetPropertyCreate();
                                    db.PCF.Add(newPCF);
                                    db.SaveChanges();
                                }


                                policy = db.Policy.Find(itemEndorsementChild.PolicyId);
                                foreach (var item in policy.Member)
                                {
                                    item.MemberStatus = "Active";
                                    db.Entry(item).State = EntityState.Modified;
                                }
                                db.SaveChanges();
                                // Generate Member Movement
                                foreach (var item in memberMemberEndorsePairitemEndorsementChild)
                                {
                                    var newMemberMovement = new Member_Movement();
                                    newMemberMovement.AdmedikaCode = item.member.AdmedikaCode;
                                    newMemberMovement.Age = item.member.Age;
                                    newMemberMovement.CardNumber = item.member.CardNumber;
                                    newMemberMovement.ClaimNumber = item.member.ClaimNumber;
                                    newMemberMovement.ClientId = item.member.ClientId;
                                    newMemberMovement.EffectiveDate = DateTime.Now.Date;
                                    newMemberMovement.EndDate = item.member.EndDate;
                                    newMemberMovement.EndorseNumber = item.member.EndorseNumber;
                                    newMemberMovement.EntryDate = item.member.EntryDate;
                                    newMemberMovement.ExitDate = item.member.ExitDate;
                                    newMemberMovement.IssueDate = item.member.IssueDate;
                                    newMemberMovement.LastClaimDate = item.member.LastClaimDate;
                                    newMemberMovement.LastEndorseDate = item.member.LastEndorseDate;
                                    newMemberMovement.MemberId = item.member.MemberId;
                                    newMemberMovement.MemberNumber = item.member.MemberNumber;
                                    newMemberMovement.MemberStatus = item.member.MemberStatus;
                                    newMemberMovement.PlanId = item.member.PlanId;
                                    newMemberMovement.PolicyId = item.member.PolicyId;
                                    //newMemberMovement.ProcessDate = item.member.ProcessDate;
                                    if (endorsement.EndorseType == EndorseType.Additional)
                                    {
                                        newMemberMovement.RecordMode = RecordModeMemberMovement.Additional;
                                    }
                                    else if (endorsement.EndorseType == EndorseType.MovePlan)
                                    {
                                        newMemberMovement.RecordMode = RecordModeMemberMovement.MovePlan;
                                    }
                                    else if (endorsement.EndorseType == EndorseType.TerminateMember)
                                    {
                                        newMemberMovement.RecordMode = RecordModeMemberMovement.TerminateMember;
                                    }
                                    newMemberMovement.SequencialNo = item.member.SequencialNo;
                                    newMemberMovement.StartDate = item.member.StartDate;
                                    newMemberMovement.TerminateDate = item.member.TerminateDate;

                                    newMemberMovement.SetPropertyCreate();
                                    db.Member_Movement.Add(newMemberMovement);
                                    db.SaveChanges();

                                    var newMemberMovementClient = new Member_Movement_Client();
                                    newMemberMovementClient.MovementId = newMemberMovement.Id;
                                    newMemberMovementClient.ClientId = item.member.Client.ClientId;
                                    newMemberMovementClient.Type = item.member.Client.Type;
                                    newMemberMovementClient.BranchCode = item.member.Client.BranchCode;
                                    newMemberMovementClient.ContactPerson = item.member.Client.ContactPerson;
                                    newMemberMovementClient.ShortName = item.member.Client.ShortName;
                                    newMemberMovementClient.FullName = item.member.Client.FullName;
                                    newMemberMovementClient.PrefixClientTitle = item.member.Client.PrefixClientTitle;
                                    newMemberMovementClient.EndfixClientTitle = item.member.Client.EndfixClientTitle;
                                    newMemberMovementClient.BirthDate = item.member.Client.BirthDate;
                                    newMemberMovementClient.BirthPlace = item.member.Client.BirthPlace;
                                    newMemberMovementClient.IdNumber = item.member.Client.IdNumber;
                                    newMemberMovementClient.MaritalStatus = item.member.Client.MaritalStatus;
                                    newMemberMovementClient.Sex = item.member.Client.Sex;
                                    newMemberMovementClient.Email = item.member.Client.Email;
                                    newMemberMovementClient.EmailAddress1 = item.member.Client.EmailAddress1;
                                    newMemberMovementClient.EmailAddress2 = item.member.Client.EmailAddress2;
                                    newMemberMovementClient.MobilePhone1 = item.member.Client.MobilePhone1;
                                    newMemberMovementClient.MObilePhone2 = item.member.Client.MObilePhone2;
                                    newMemberMovementClient.MobilePhone3 = item.member.Client.MobilePhone3;
                                    newMemberMovementClient.ClientRelation = item.member.Client.ClientRelation;
                                    newMemberMovementClient.RelateTo = item.member.Client.RelateTo;
                                    newMemberMovementClient.BankAccountNumber = item.member.Client.BankAccountNumber;
                                    newMemberMovementClient.BankAccountCode = item.member.Client.BankAccountCode;
                                    newMemberMovementClient.BankAccountName = item.member.Client.BankAccountName;
                                    newMemberMovementClient.Status = item.member.Client.Status;
                                    newMemberMovementClient.Address = item.member.Client.Address;
                                    newMemberMovementClient.SetPropertyCreate();

                                    db.Member_Movement_Client.Add(newMemberMovementClient);
                                    db.SaveChanges();
                                }
                                // Generate MemberPlan
                                foreach (var item in memberMemberEndorsePairitemEndorsementChild)
                                {
                                    var memberPlanEndorse = itemEndorsementChild.MemberPlan_Endorse.Where(x => x.MemberId == item.memberEndorse.MemberId).ToList();
                                    foreach (var itemMemberPlan in memberPlanEndorse)
                                    {
                                        var newMemberPlan = new MemberPlan();
                                        newMemberPlan.BasicProductId = itemMemberPlan.BasicProductId;
                                        newMemberPlan.BasicProductLimitCode = itemMemberPlan.BasicProductLimitCode;
                                        newMemberPlan.MemberId = item.member.MemberId;
                                        newMemberPlan.PlanId = item.member.PlanId;
                                        newMemberPlan.PolicyId = itemMemberPlan.PolicyId;
                                        newMemberPlan.EndorseNumber = itemMemberPlan.EndorseNumber;
                                        newMemberPlan.StartDate = itemMemberPlan.StartDate;
                                        newMemberPlan.SetPropertyCreate(itemMemberPlan.IsActive.Value);
                                        db.MemberPlan.Add(newMemberPlan);
                                    }

                                }
                                db.SaveChanges();

                                // Generate Plan
                                var planEndorse = itemEndorsementChild.Plan_Endorse;
                                foreach (var item in planEndorse)
                                {
                                    var plan = db.Plan.Where(x => x.PolicyId == item.PolicyId && x.PlanId == item.PlanId).FirstOrDefault();
                                    if (plan == null)
                                    {
                                        var newPlan = new Plan();
                                        newPlan.PlanId = item.PlanId;
                                        newPlan.PlanDesc = item.PlanDesc;
                                        newPlan.PolicyId = item.PolicyId;
                                        newPlan.PlanName = item.PlanName;
                                        newPlan.StartDate = itemEndorsementChild.EndorseDate;
                                        newPlan.SetPropertyCreate();
                                        db.Plan.Add(newPlan);
                                    }
                                }
                                db.SaveChanges();


                                // Update Policy To Active
                                policy.SetPropertyUpdate();

                                policy.LastEndorseDate = itemEndorsementChild.EndorseDate;
                                policy.EndorseNumber = itemEndorsementChild.EndorseNumber;
                                policy.PolicyStatus = PolicyStatus.Active;
                                db.Entry(policy).State = EntityState.Modified;


                                //Delete All Data From Table Relate To Endorse
                                // 1. Delete Policy_Endorse
                                db.Policy_Endorse.Remove(itemEndorsementChild.Policy_Endorse);
                                // 2. Delete PCF_Endorse
                                db.PCF_Endorse.RemoveRange(itemEndorsementChild.PCF_Endorse);
                                // 3. Delete PlanDetail_Endorse
                                db.PlanDetail_Endorse.RemoveRange(itemEndorsementChild.PlanDetail_Endorse);
                                // 7. Delete MemberPlan_Endorse
                                db.MemberPlan_Endorse.RemoveRange(itemEndorsementChild.MemberPlan_Endorse);
                                // 4. Delete PlanDetail
                                db.Plan_Endorse.RemoveRange(itemEndorsementChild.Plan_Endorse);
                                // 5. Delete Member_Endorse
                                db.Member_Endorse.RemoveRange(itemEndorsementChild.Member_Endorse);

                                itemEndorsementChild.EndorseStatus = EndorseStatus.Done;
                                db.Entry(itemEndorsementChild).State = EntityState.Modified;
                                db.SaveChanges();
                            }



                            var memberMemberEndorsePair = new List<(Member member, Member_Endorse memberEndorse)>();
                            foreach (var item in endorsement.Member_Endorse)
                            {
                                var member = db.Member.Where(x => x.MemberNumber == item.MemberNumber).FirstOrDefault();
                                memberMemberEndorsePair.Add((member, item));
                            }

                            //MutationEndorse, Generate FinanceTransaction

                            var financeTransaction = new FinanceTransaction();

                            var transactionNumberSeq = db.AspSequential.Where(x => x.Name == AspSequentialName.TransactionNumber).FirstOrDefault();
                            financeTransaction.RecordMode = 1;
                            financeTransaction.TransactionNumber = "TXRF-" + DateTime.Now.Year + "-" + (transactionNumberSeq.LastSequential + 1).ToString().PadLeft(6, '0');
                            transactionNumberSeq.LastSequential = transactionNumberSeq.LastSequential + 1;
                            transactionNumberSeq.SetPropertyUpdate();
                            db.SaveChanges();
                            db.Entry(transactionNumberSeq).State = EntityState.Modified;
                            financeTransaction.EffectiveDate = policy.IssueDate;
                            financeTransaction.TransactionDate = endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null).Min(x => x.InvoiceDate);
                            financeTransaction.DueDate = financeTransaction.EffectiveDate.Value.AddDays(30);
                            financeTransaction.TransCodeId = "Invoice";
                            financeTransaction.PolicyId = policy.PolicyId;
                            financeTransaction.PolicyNumber = policy.PolicyNumber;
                            financeTransaction.ReconStatus = "No";
                            financeTransaction.OutstandingAmount = endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null).Sum(x => x.Amount);
                            financeTransaction.ClientId = policy.ClientId;
                            financeTransaction.ClientTransactionAmount = financeTransaction.OutstandingAmount;
                            financeTransaction.ClosingAgen = policy.Agent;
                            financeTransaction.TransDescription = "Invoice Policy " + policy.PolicyNumber + " Periode " + financeTransaction.TransactionDate.Value.ToShortDateString();
                            db.FinanceTransaction.Add(financeTransaction);
                            // Generate Finance Transaction Detail
                            foreach (var item in endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null).GroupBy(x => x.BasicProductId))
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
                            //// Generate PCF
                            //foreach (var item in endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null))
                            //{
                            //    var memberEndorsePairInMember = memberMemberEndorsePair.Where(x => x.memberEndorse.MemberId == item.MemberId).FirstOrDefault();

                            //    var newPCF = new PCF();
                            //    newPCF.PolicyId = item.PolicyId;
                            //    newPCF.MemberId = memberEndorsePairInMember.member.MemberId;
                            //    newPCF.BasicProductId = item.BasicProductId;
                            //    newPCF.TransType = item.TransType;
                            //    newPCF.InvoiceDate = item.InvoiceDate;
                            //    newPCF.DueDate = item.DueDate;
                            //    newPCF.Amount = item.Amount;
                            //    newPCF.TransactionNumber = financeTransaction.TransactionNumber;
                            //    newPCF.SetPropertyCreate();
                            //    db.PCF.Add(newPCF);
                            //    db.SaveChanges();
                            //}

                            // 4 Feb 2020, update member to terminate
                            foreach (var item in memberMemberEndorsePair)
                            {
                                if (item.memberEndorse.StartDate < item.memberEndorse.TerminateDate && item.memberEndorse.EndDate > item.memberEndorse.TerminateDate)
                                {
                                    item.member.MemberStatus = MemberStatus.TerminateRefund;
                                    item.member.LastEndorseDate = endorsement.EndorseDate;
                                    item.member.EndorseNumber = endorsement.EndorseNumber;
                                    item.member.TerminateDate = endorsement.EndorseDate;
                                }
                                else if (item.memberEndorse.StartDate == item.memberEndorse.TerminateDate)
                                {
                                    item.member.MemberStatus = MemberStatus.TerminateCancel;
                                    item.member.LastEndorseDate = endorsement.EndorseDate;
                                    item.member.EndorseNumber = endorsement.EndorseNumber;
                                    item.member.TerminateDate = endorsement.EndorseDate;
                                }
                                else
                                {
                                    WarningMessagesAdd(Message.ProcessFail);
                                }

                                db.Entry(item.member).State = EntityState.Modified;
                            }

                            //update policy
                            endorsement.Policy.SetPropertyUpdate();

                            endorsement.Policy.LastEndorseDate = endorsement.EndorseDate;
                            endorsement.Policy.EndorseNumber = endorsement.EndorseNumber;
                            endorsement.Policy.PolicyStatus = PolicyStatus.Active;
                            db.Entry(endorsement.Policy).State = EntityState.Modified;
                            db.SaveChanges();

                            //Delete All Data From Table Relate To Endorse
                            // 1. Delete Policy_Endorse
                            db.Policy_Endorse.Remove(endorsement.Policy_Endorse);
                            // 2. Delete PCF_Endorse
                            db.PCF_Endorse.RemoveRange(endorsement.PCF_Endorse);
                            // 3. Delete PlanDetail_Endorse
                            db.PlanDetail_Endorse.RemoveRange(endorsement.PlanDetail_Endorse);
                            // 7. Delete MemberPlan_Endorse
                            db.MemberPlan_Endorse.RemoveRange(endorsement.MemberPlan_Endorse);
                            // 4. Delete PlanDetail
                            db.Plan_Endorse.RemoveRange(endorsement.Plan_Endorse);
                            // 5. Delete Member_Endorse
                            db.Member_Endorse.RemoveRange(endorsement.Member_Endorse);

                            endorsement.EndorseStatus = EndorseStatus.Done;
                            db.Entry(endorsement).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        else if (endorsement.EndorseType == EndorseType.TransitionData)
                        {
                            // PROCESS TERMINATE 

                            //1. Generate Finance Transaction
                            //2. Update PCF transaction number with generated number
                            //3. Generated finance transaction detail
                            //4. updated policy
                            var memberMemberEndorsePairTerminate = new List<(Member member, Member_Endorse memberEndorse)>();
                            foreach (var item in endorsement.Member_Endorse)
                            {
                                var member = db.Member.Where(x => x.MemberNumber == item.MemberNumber).FirstOrDefault();
                                memberMemberEndorsePairTerminate.Add((member, item));
                            }

                            // Generate FinanceTransaction

                            var financeTransaction = new FinanceTransaction();
                            var transactionNumberSeq = db.AspSequential.Where(x => x.Name == AspSequentialName.TransactionNumber).FirstOrDefault();
                            financeTransaction.RecordMode = 1;
                            financeTransaction.TransactionNumber = "TXRF-" + DateTime.Now.Year + "-" + (transactionNumberSeq.LastSequential + 1).ToString().PadLeft(6, '0');
                            transactionNumberSeq.LastSequential = transactionNumberSeq.LastSequential + 1;
                            transactionNumberSeq.SetPropertyUpdate();
                            db.SaveChanges();
                            db.Entry(transactionNumberSeq).State = EntityState.Modified;
                            financeTransaction.EffectiveDate = policy.IssueDate;
                            financeTransaction.TransactionDate = endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null).Min(x => x.InvoiceDate);
                            financeTransaction.DueDate = financeTransaction.EffectiveDate.Value.AddDays(30);
                            financeTransaction.TransCodeId = "Invoice";
                            financeTransaction.PolicyId = policy.PolicyId;
                            financeTransaction.PolicyNumber = policy.PolicyNumber;
                            financeTransaction.ReconStatus = "No";
                            financeTransaction.OutstandingAmount = endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null).Sum(x => x.Amount);
                            financeTransaction.ClientId = policy.ClientId;
                            financeTransaction.ClientTransactionAmount = financeTransaction.OutstandingAmount;
                            financeTransaction.ClosingAgen = policy.Agent;
                            financeTransaction.TransDescription = "Invoice Policy " + policy.PolicyNumber + " Periode " + financeTransaction.TransactionDate.Value.ToShortDateString();
                            db.FinanceTransaction.Add(financeTransaction);
                            db.SaveChanges();

                            foreach (var memberMemberEndorsePair in memberMemberEndorsePairTerminate)
                            {
                                foreach (var item in memberMemberEndorsePair.member.MemberPlan)
                                {
                                    var memberPlanHistory = new MemberPlan_H();
                                    memberPlanHistory.EndorseNumber = endorsement.EndorseNumber;
                                    memberPlanHistory.BasicProductId = item.BasicProductId;
                                    memberPlanHistory.BasicProductLimitCode = item.BasicProductLimitCode;
                                    memberPlanHistory.MemberId = item.MemberId;
                                    memberPlanHistory.PlanId = memberMemberEndorsePair.member.PlanId;
                                    memberPlanHistory.PolicyId = endorsement.PolicyId;
                                    memberPlanHistory.StartDate = item.StartDate;
                                    memberPlanHistory.SetPropertyCreate();
                                    db.MemberPlan_H.Add(memberPlanHistory);
                                    db.SaveChanges();
                                }
                                db.MemberPlan.RemoveRange(memberMemberEndorsePair.member.MemberPlan);
                                db.SaveChanges();
                                //1a.[Execution] Pindahkan MemberPlanEndorse ke MemberPlan
                                foreach (var item in memberMemberEndorsePair.memberEndorse.MemberPlan_Endorse)
                                {
                                    var memberPlan = new MemberPlan();
                                    memberPlan.EndorseNumber = endorsement.EndorseNumber;
                                    memberPlan.BasicProductId = item.BasicProductId;
                                    memberPlan.BasicProductLimitCode = item.BasicProductLimitCode;
                                    memberPlan.PlanId = item.PlanId;
                                    memberPlan.MemberId = memberMemberEndorsePair.member.MemberId;
                                    memberPlan.PolicyId = endorsement.PolicyId;
                                    memberPlan.StartDate = item.StartDate;
                                    memberPlan.SetPropertyCreate();
                                    db.MemberPlan.Add(memberPlan);
                                }
                                db.SaveChanges();
                                //3. Salin PCF_Endorse ke PCF, yang disalin adalah pcf yang tidak mempunyai transaction number
                                foreach (var item in memberMemberEndorsePair.memberEndorse.PCF_Endorse.Where(x => x.TransactionNumber == null))
                                {
                                    var newPCF = new PCF();
                                    newPCF.PolicyId = item.PolicyId;
                                    newPCF.MemberId = memberMemberEndorsePair.member.MemberId;
                                    newPCF.BasicProductId = item.BasicProductId;
                                    newPCF.TransType = item.TransType;
                                    newPCF.InvoiceDate = item.InvoiceDate;
                                    newPCF.DueDate = item.DueDate;
                                    newPCF.Amount = item.Amount;
                                    newPCF.TransactionNumber = financeTransaction.TransactionNumber;
                                    newPCF.SetPropertyCreate();
                                    db.PCF.Add(newPCF);
                                }
                                db.PCF_Endorse.RemoveRange(memberMemberEndorsePair.memberEndorse.PCF_Endorse);
                                db.SaveChanges();
                            }

                            // Generate Member Movement
                            foreach (var item in memberMemberEndorsePairTerminate)
                            {
                                var itemMember = item.member;
                                var newMemberMovement = new Member_Movement();
                                newMemberMovement.AdmedikaCode = itemMember.AdmedikaCode;
                                newMemberMovement.Age = itemMember.Age;
                                newMemberMovement.CardNumber = itemMember.CardNumber;
                                newMemberMovement.ClaimNumber = itemMember.ClaimNumber;
                                newMemberMovement.ClientId = itemMember.ClientId;
                                newMemberMovement.EffectiveDate = DateTime.Now.Date;
                                newMemberMovement.EndDate = itemMember.EndDate;
                                newMemberMovement.EndorseNumber = item.memberEndorse.EndorseNumber;
                                newMemberMovement.EntryDate = itemMember.EntryDate;
                                newMemberMovement.ExitDate = itemMember.ExitDate;
                                newMemberMovement.IssueDate = itemMember.IssueDate;
                                newMemberMovement.LastClaimDate = itemMember.LastClaimDate;
                                newMemberMovement.LastEndorseDate = itemMember.LastEndorseDate;
                                newMemberMovement.MemberId = itemMember.MemberId;
                                newMemberMovement.MemberNumber = itemMember.MemberNumber;
                                newMemberMovement.MemberStatus = itemMember.MemberStatus;
                                newMemberMovement.PlanId = itemMember.PlanId;
                                newMemberMovement.PolicyId = itemMember.PolicyId;
                                //newMemberMovement.ProcessDate = itemMember.ProcessDate;

                                newMemberMovement.RecordMode = RecordModeMemberMovement.MovePlan;

                                newMemberMovement.SequencialNo = itemMember.SequencialNo;
                                newMemberMovement.StartDate = itemMember.StartDate;
                                newMemberMovement.TerminateDate = itemMember.TerminateDate;

                                newMemberMovement.SetPropertyCreate();
                                db.Member_Movement.Add(newMemberMovement);
                                db.SaveChanges();

                                var newMemberMovementClient = new Member_Movement_Client();
                                newMemberMovementClient.MovementId = newMemberMovement.Id;
                                newMemberMovementClient.ClientId = itemMember.Client.ClientId;
                                newMemberMovementClient.Type = itemMember.Client.Type;
                                newMemberMovementClient.BranchCode = itemMember.Client.BranchCode;
                                newMemberMovementClient.ContactPerson = itemMember.Client.ContactPerson;
                                newMemberMovementClient.ShortName = itemMember.Client.ShortName;
                                newMemberMovementClient.FullName = itemMember.Client.FullName;
                                newMemberMovementClient.PrefixClientTitle = itemMember.Client.PrefixClientTitle;
                                newMemberMovementClient.EndfixClientTitle = itemMember.Client.EndfixClientTitle;
                                newMemberMovementClient.BirthDate = itemMember.Client.BirthDate;
                                newMemberMovementClient.BirthPlace = itemMember.Client.BirthPlace;
                                newMemberMovementClient.IdNumber = itemMember.Client.IdNumber;
                                newMemberMovementClient.MaritalStatus = itemMember.Client.MaritalStatus;
                                newMemberMovementClient.Sex = itemMember.Client.Sex;
                                newMemberMovementClient.Email = itemMember.Client.Email;
                                newMemberMovementClient.EmailAddress1 = itemMember.Client.EmailAddress1;
                                newMemberMovementClient.EmailAddress2 = itemMember.Client.EmailAddress2;
                                newMemberMovementClient.MobilePhone1 = itemMember.Client.MobilePhone1;
                                newMemberMovementClient.MObilePhone2 = itemMember.Client.MObilePhone2;
                                newMemberMovementClient.MobilePhone3 = itemMember.Client.MobilePhone3;
                                newMemberMovementClient.ClientRelation = itemMember.Client.ClientRelation;
                                newMemberMovementClient.RelateTo = itemMember.Client.RelateTo;
                                newMemberMovementClient.BankAccountNumber = itemMember.Client.BankAccountNumber;
                                newMemberMovementClient.BankAccountCode = itemMember.Client.BankAccountCode;
                                newMemberMovementClient.BankAccountName = itemMember.Client.BankAccountName;
                                newMemberMovementClient.Status = itemMember.Client.Status;
                                newMemberMovementClient.Address = itemMember.Client.Address;
                                newMemberMovementClient.SetPropertyCreate();

                                db.Member_Movement_Client.Add(newMemberMovementClient);
                                db.SaveChanges();
                            }

                            // Set Member Status To Active
                            foreach (var item in memberMemberEndorsePairTerminate)
                            {
                                item.member.MemberStatus = MemberStatus.Active;
                                db.Entry(item.member).State = EntityState.Modified;
                            }
                            db.SaveChanges();
                            //4.[Execution] Salin PlanEndorse ke Plan
                            foreach (var item in endorsement.Plan_Endorse)
                            {
                                var newPlanToInsert = endorsement.Policy.Plan.Where(x => x.PlanId == item.PlanId).FirstOrDefault();
                                if (newPlanToInsert == null)
                                {
                                    var newPlan = new Plan();
                                    newPlan.PolicyId = item.PolicyId;
                                    newPlan.PlanId = item.PlanId;
                                    newPlan.PlanName = item.PlanName;
                                    newPlan.PlanDesc = item.PlanDesc;
                                    newPlan.StartDate = item.StartDate;
                                    newPlan.SetPropertyCreate();
                                    db.Plan.Add(newPlan);

                                    //5. Salin PlanDetailEndorse ke Plan Detail
                                    foreach (var itemPlanDetail in endorsement.PlanDetail_Endorse.Where(x => x.PlanId == item.PlanId))
                                    {
                                        var newPlanDetail = new PlanDetail();
                                        newPlanDetail.PolicyId = itemPlanDetail.PolicyId;
                                        newPlanDetail.PlanId = item.PlanId;
                                        newPlanDetail.BasicProductId = itemPlanDetail.BasicProductId;
                                        newPlanDetail.BasicProductLimitCode = itemPlanDetail.BasicProductLimitCode;
                                        newPlanDetail.SetPropertyCreate();
                                        db.PlanDetail.Add(newPlanDetail);
                                    }
                                }

                            }
                            db.Plan_Endorse.RemoveRange(endorsement.Plan_Endorse);
                            db.SaveChanges();

                            // Generate Finance Transaction Detail
                            foreach (var item in endorsement.PCF_Endorse.Where(x => x.TransactionNumber == null).GroupBy(x => x.BasicProductId))
                            {

                                var newFinanceTransactionDetail = new FinanceTransactionDetail();
                                newFinanceTransactionDetail.TransactionNumber = financeTransaction.TransactionNumber;
                                newFinanceTransactionDetail.OutstandingAmount = item.Sum(x => x.Amount);
                                newFinanceTransactionDetail.BasicProductId = item.FirstOrDefault().BasicProductId;
                                newFinanceTransactionDetail.TransactionAmount = newFinanceTransactionDetail.OutstandingAmount;
                                db.FinanceTransactionDetail.Add(newFinanceTransactionDetail);
                                db.SaveChanges();
                            }

                            // Move Data From MemberClientEndorse To Client
                            foreach (var memberMemberEndorsePair in memberMemberEndorsePairTerminate)
                            {
                                var client = memberMemberEndorsePair.member.Client;
                                var memberClientEndorse = memberMemberEndorsePair.memberEndorse.MemberClientEndorse.ToList().FirstOrDefault();
                                if (memberClientEndorse == null)
                                {
                                    WarningMessagesAdd(Message.ProcessFail);
                                }
                                client.ClientId = memberClientEndorse.ClientId;
                                client.Type = memberClientEndorse.Type;
                                client.BranchCode = memberClientEndorse.BranchCode;
                                client.ContactPerson = memberClientEndorse.ContactPerson;
                                client.ShortName = memberClientEndorse.ShortName;
                                client.FullName = memberClientEndorse.FullName;
                                client.PrefixClientTitle = memberClientEndorse.FullName;
                                client.EndfixClientTitle = memberClientEndorse.EndfixClientTitle;
                                client.BirthDate = memberClientEndorse.BirthDate;
                                client.BirthPlace = memberClientEndorse.BirthPlace;
                                client.IdNumber = memberClientEndorse.IdNumber;
                                client.MaritalStatus = memberClientEndorse.MaritalStatus;
                                client.Sex = memberClientEndorse.Sex;
                                client.Email = memberClientEndorse.Email;
                                client.EmailAddress1 = memberClientEndorse.EmailAddress1;
                                client.EmailAddress2 = memberClientEndorse.EmailAddress2;
                                client.Address = memberClientEndorse.Address;
                                client.MobilePhone1 = memberClientEndorse.MobilePhone1;
                                client.MObilePhone2 = memberClientEndorse.MObilePhone2;
                                client.MobilePhone3 = memberClientEndorse.MobilePhone3;
                                client.ClientRelation = memberClientEndorse.ClientRelation;
                                client.RelateTo = memberClientEndorse.RelateTo;
                                client.BankAccountNumber = memberClientEndorse.BankAccountNumber;
                                client.BankAccountCode = memberClientEndorse.BankAccountCode;
                                client.BankAccountName = memberClientEndorse.BankAccountName;
                                client.Status = memberClientEndorse.Status;

                                db.Entry(client).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            //7. [Execution] Update Endorsetype to Done


                            //Delete All Data From Table Relate To Endorse
                            // 1. Delete Policy_Endorse
                            db.Policy_Endorse.Remove(endorsement.Policy_Endorse);
                            // 2. Delete PCF_Endorse
                            db.PCF_Endorse.RemoveRange(endorsement.PCF_Endorse);
                            // 3. Delete PlanDetail_Endorse
                            db.PlanDetail_Endorse.RemoveRange(endorsement.PlanDetail_Endorse);
                            // 7. Delete MemberPlan_Endorse
                            db.MemberPlan_Endorse.RemoveRange(endorsement.MemberPlan_Endorse);
                            // 4. Delete PlanDetail
                            db.Plan_Endorse.RemoveRange(endorsement.Plan_Endorse);
                            // Delete MemberClientEndorse
                            db.MemberClientEndorse.RemoveRange(db.MemberClientEndorse.Where(x => x.Member_Endorse.EndorseNumber == endorsement.EndorseNumber));
                            // 5. Delete Member_Endorse
                            db.Member_Endorse.RemoveRange(endorsement.Member_Endorse);
                            // 6. Delete Endorse
                            //db.Endorsement.Remove(endorsement);
                            endorsement.EndorseStatus = EndorseStatus.Done;
                            db.Entry(endorsement).State = EntityState.Modified;

                            db.SaveChanges();
                        }
                        else if (endorsement.EndorseType == EndorseType.Renewal)
                        {
                            // Update Polis End Date
                            policy.MatureDate = endorsement.Policy_Endorse.MatureDate;
                            db.Entry(policy).State = EntityState.Modified;
                            db.SaveChanges();

                            var memberMemberEndorsePair = new List<(Member member, Member_Endorse memberEndorse)>();
                            foreach (var item in endorsement.Member_Endorse)
                            {
                                var itemMember = endorsement.Policy.Member.Where(x => x.MemberNumber == item.MemberNumber).FirstOrDefault();
                                memberMemberEndorsePair.Add((itemMember, item));
                            }
                            // Generate MemberMovement and Generates MemberMovementClient
                            foreach (var item in memberMemberEndorsePair)
                            {
                                var newMemberMovement = new Member_Movement();
                                newMemberMovement.AdmedikaCode = item.member.AdmedikaCode;
                                newMemberMovement.Age = item.member.Age;
                                newMemberMovement.CardNumber = item.member.CardNumber;
                                newMemberMovement.ClaimNumber = item.member.ClaimNumber;
                                newMemberMovement.ClientId = item.member.ClientId;
                                newMemberMovement.EffectiveDate = DateTime.Now.Date;
                                newMemberMovement.EndDate = item.member.EndDate;
                                newMemberMovement.EndorseNumber = item.memberEndorse.EndorseNumber;
                                newMemberMovement.EntryDate = item.member.EntryDate;
                                newMemberMovement.ExitDate = item.member.ExitDate;
                                newMemberMovement.IssueDate = item.member.IssueDate;
                                newMemberMovement.LastClaimDate = item.member.LastClaimDate;
                                newMemberMovement.LastEndorseDate = item.member.LastEndorseDate;
                                newMemberMovement.MemberId = item.member.MemberId;
                                newMemberMovement.MemberNumber = item.member.MemberNumber;
                                newMemberMovement.MemberStatus = item.member.MemberStatus;
                                newMemberMovement.PlanId = item.member.PlanId;
                                newMemberMovement.PolicyId = item.member.PolicyId;
                                //newMemberMovement.ProcessDate = item.member.ProcessDate;
                                newMemberMovement.RecordMode = RecordModeMemberMovement.MovePlan;
                                newMemberMovement.SequencialNo = item.member.SequencialNo;
                                newMemberMovement.StartDate = item.member.StartDate;
                                newMemberMovement.TerminateDate = item.member.TerminateDate;
                                newMemberMovement.SetPropertyCreate();
                                db.Member_Movement.Add(newMemberMovement);
                                db.SaveChanges();

                                var newMemberMovementClient = new Member_Movement_Client();
                                newMemberMovementClient.MovementId = newMemberMovement.Id;
                                newMemberMovementClient.ClientId = item.member.Client.ClientId;
                                newMemberMovementClient.Type = item.member.Client.Type;
                                newMemberMovementClient.BranchCode = item.member.Client.BranchCode;
                                newMemberMovementClient.ContactPerson = item.member.Client.ContactPerson;
                                newMemberMovementClient.ShortName = item.member.Client.ShortName;
                                newMemberMovementClient.FullName = item.member.Client.FullName;
                                newMemberMovementClient.PrefixClientTitle = item.member.Client.PrefixClientTitle;
                                newMemberMovementClient.EndfixClientTitle = item.member.Client.EndfixClientTitle;
                                newMemberMovementClient.BirthDate = item.member.Client.BirthDate;
                                newMemberMovementClient.BirthPlace = item.member.Client.BirthPlace;
                                newMemberMovementClient.IdNumber = item.member.Client.IdNumber;
                                newMemberMovementClient.MaritalStatus = item.member.Client.MaritalStatus;
                                newMemberMovementClient.Sex = item.member.Client.Sex;
                                newMemberMovementClient.Email = item.member.Client.Email;
                                newMemberMovementClient.EmailAddress1 = item.member.Client.EmailAddress1;
                                newMemberMovementClient.EmailAddress2 = item.member.Client.EmailAddress2;
                                newMemberMovementClient.MobilePhone1 = item.member.Client.MobilePhone1;
                                newMemberMovementClient.MObilePhone2 = item.member.Client.MObilePhone2;
                                newMemberMovementClient.MobilePhone3 = item.member.Client.MobilePhone3;
                                newMemberMovementClient.ClientRelation = item.member.Client.ClientRelation;
                                newMemberMovementClient.RelateTo = item.member.Client.RelateTo;
                                newMemberMovementClient.BankAccountNumber = item.member.Client.BankAccountNumber;
                                newMemberMovementClient.BankAccountCode = item.member.Client.BankAccountCode;
                                newMemberMovementClient.BankAccountName = item.member.Client.BankAccountName;
                                newMemberMovementClient.Status = item.member.Client.Status;
                                newMemberMovementClient.Address = item.member.Client.Address;
                                newMemberMovementClient.SetPropertyCreate();

                                db.Member_Movement_Client.Add(newMemberMovementClient);
                                db.SaveChanges();
                                var memberMovement = new Member_Movement();
                            }
                            db.SaveChanges();

                            // Update Member StartDate and Member EndDate
                            foreach (var item in memberMemberEndorsePair)
                            {
                                item.member.StartDate = item.memberEndorse.StartDate;
                                item.member.EndDate = item.memberEndorse.EndDate;
                                db.Entry(item.member).State = EntityState.Modified;
                            }
                            db.SaveChanges();

                            // Generate FinanceTransaction
                            var allPolicyPCFEndorse = db.PCF_Endorse.Where(x => x.EndorseNumber == endorsement.EndorseNumber && x.PolicyId == policyId && x.InvoiceDate < DateTime.Now);
                            var financeTransaction = new FinanceTransaction();
                            var transactionNumberSeq = db.AspSequential.Where(x => x.Name == AspSequentialName.TransactionNumber).FirstOrDefault();
                            financeTransaction.RecordMode = RecordMode.RenewalWithoutCard;
                            financeTransaction.TransactionNumber = "TXTR-" + DateTime.Now.Year + "-" + (transactionNumberSeq.LastSequential + 1).ToString().PadLeft(6, '0');
                            transactionNumberSeq.LastSequential = transactionNumberSeq.LastSequential + 1;
                            transactionNumberSeq.SetPropertyUpdate();
                            db.SaveChanges();
                            db.Entry(transactionNumberSeq).State = EntityState.Modified;
                            financeTransaction.EffectiveDate = DateTime.Today;
                            financeTransaction.TransactionDate = endorsement.EndorseDate;
                            financeTransaction.DueDate = financeTransaction.TransactionDate.Value.AddDays(30);
                            financeTransaction.OutstandingAmount = allPolicyPCFEndorse.Sum(x => x.Amount);
                            financeTransaction.TransCodeId = financeTransaction.OutstandingAmount > 0 ? "Invoice" : "Refund";
                            financeTransaction.PolicyId = endorsement.PolicyId;
                            financeTransaction.PolicyNumber = endorsement.Policy.PolicyNumber;
                            financeTransaction.ReconStatus = "No";
                            financeTransaction.ClientId = endorsement.Policy.ClientId;
                            financeTransaction.ClientTransactionAmount = financeTransaction.OutstandingAmount;
                            financeTransaction.ClosingAgen = endorsement.Policy.Agent;
                            financeTransaction.TransDescription = financeTransaction.TransCodeId + " Policy " + policy.PolicyNumber;
                            financeTransaction.ReferenceNumber = endorsement.EndorseNumber;
                            db.FinanceTransaction.Add(financeTransaction);

                            db.SaveChanges();

                            // Generate Finance Transaction Detail
                            foreach (var item in allPolicyPCFEndorse.GroupBy(x => x.BasicProductId))
                            {
                                var newFinanceTransactionDetail = new FinanceTransactionDetail();
                                newFinanceTransactionDetail.TransactionNumber = financeTransaction.TransactionNumber;
                                newFinanceTransactionDetail.OutstandingAmount = item.Sum(x => x.Amount);
                                newFinanceTransactionDetail.BasicProductId = item.FirstOrDefault().BasicProductId;
                                newFinanceTransactionDetail.TransactionAmount = newFinanceTransactionDetail.OutstandingAmount;
                                db.FinanceTransactionDetail.Add(newFinanceTransactionDetail);
                            }
                            db.SaveChanges();
                            // Add PCF from PCFEndorse
                            foreach (var item in memberMemberEndorsePair)
                            {
                                foreach (var pcfEndorse in item.memberEndorse.PCF_Endorse.Where(x => x.TransactionNumber == null))
                                {
                                    var newPCF = new PCF();
                                    newPCF.PolicyId = pcfEndorse.PolicyId;
                                    newPCF.MemberId = item.member.MemberId;
                                    newPCF.BasicProductId = pcfEndorse.BasicProductId;
                                    newPCF.TransType = pcfEndorse.TransType;
                                    newPCF.InvoiceDate = pcfEndorse.InvoiceDate;
                                    newPCF.DueDate = pcfEndorse.DueDate;
                                    newPCF.Amount = pcfEndorse.Amount;
                                    newPCF.TransactionNumber = financeTransaction.TransactionNumber;
                                    newPCF.SetPropertyCreate();
                                    db.PCF.Add(newPCF);
                                }
                            }
                            
                            //Delete All Data From Table Relate To Endorse
                            // 1. Delete Policy_Endorse
                            db.Policy_Endorse.Remove(endorsement.Policy_Endorse);
                            // 2. Delete PCF_Endorse
                            db.PCF_Endorse.RemoveRange(endorsement.PCF_Endorse);
                            // 3. Delete PlanDetail_Endorse
                            db.PlanDetail_Endorse.RemoveRange(endorsement.PlanDetail_Endorse);
                            // 7. Delete MemberPlan_Endorse
                            db.MemberPlan_Endorse.RemoveRange(endorsement.MemberPlan_Endorse);
                            // 4. Delete PlanDetail
                            db.Plan_Endorse.RemoveRange(endorsement.Plan_Endorse);
                            // 5. Delete Member_Endorse
                            db.Member_Endorse.RemoveRange(endorsement.Member_Endorse);
                            // 6. Delete Endorse
                            //db.Endorsement.Remove(endorsement);
                            endorsement.EndorseStatus = EndorseStatus.Done;
                            db.Entry(endorsement).State = EntityState.Modified;
                        }
                        if (!Warn())
                        {
                            dbTransaction.Commit();
                        }
                        else
                        {
                            dbTransaction.Rollback();
                        }

                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        WarningMessagesAdd(e.MessageToList());
                    }
                }
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
                SuccessMessagesAdd("Issued Process Is Success.");
            }
            return RedirectToAction(endorsement.EndorseType);
        }
        [HttpGet]
        public ActionResult DeletePlan(string endorseNumber, string policyId, string planId)
        {
            if (endorseNumber == null || policyId == null || planId == null)
            {
                return HttpNotFound();

            }
            var planEndorse = db.Plan_Endorse.Where(x => x.EndorseNumber == endorseNumber && x.PolicyId == policyId && x.PlanId == planId).FirstOrDefault();
            if (planEndorse == null)
            {
                return HttpNotFound();
            }
            //checking apakah plan yang akan dihapus identik dengan plan yang ada di Master Plan, jika iyah, maka tidak boleh dihapus.
            var plan = db.Plan.Where(x => x.PlanId == planEndorse.PlanId && x.PolicyId == planEndorse.PolicyId).FirstOrDefault();
            if (plan != null)
            {
                WarningMessagesAdd(Message.DeleteIsNotAllowed);
            }
            var isPlanUsedByMember = db.Member_Endorse.Where(x => x.PolicyId == planEndorse.PolicyId && x.EndorseNumber == planEndorse.EndorseNumber && x.PlanId == planEndorse.PlanId).FirstOrDefault() != null;
            if (isPlanUsedByMember)
            {
                WarningMessagesAdd(Message.DeleteIsNotAllowed);
            }
            WarningMessagesDistinct();
            var modalView = new ModalView()
            {
                Title = "Delete Plan",
                Body = this.RenderRazorViewToString("_DeletePlan", planEndorse),
                Footer = this.GetHtmlHelper().TextBox("Delete", "Delete", null, new { @class = "btn btn-primary", @type = "submit" }).ToString(),
                ModalForm = new ModalForm { ActionName = "DeletePlan/" + endorseNumber, ControllerName = "PolicyEndorsement", RouteValues = new { endorseNumber, policyId, planId, endorseType = Request.Params["EndorseType"] } }
            };
            if (WarningMessages().Contains(Message.DeleteIsNotAllowed))
            {
                modalView.Footer = null;
            }
            if (Request.IsAjaxRequest())
            {
                return View("_Modal", modalView);
            }
            return null;

        }
        [HttpPost]
        [ActionName("DeletePlan")]
        public ActionResult DeletePlanConfirm(string endorseNumber, string policyId, string planId)
        {
            if (endorseNumber == null || policyId == null || planId == null)
            {
                return HttpNotFound();

            }
            var planEndorse = db.Plan_Endorse.Where(x => x.EndorseNumber == endorseNumber && x.PolicyId == policyId && x.PlanId == planId).FirstOrDefault();
            if (planEndorse == null)
            {
                return HttpNotFound();
            }
            var plan = db.Plan.Where(x => x.PlanId == planEndorse.PlanId && x.PolicyId == planEndorse.PolicyId).FirstOrDefault();
            if (plan != null)
            {
                WarningMessagesAdd(Message.DeleteIsNotAllowed);
            }
            var isPlanUsedByMember = db.Member_Endorse.Where(x => x.PolicyId == planEndorse.PolicyId && x.EndorseNumber == planEndorse.EndorseNumber && x.PlanId == planEndorse.PlanId).FirstOrDefault() != null;
            if (isPlanUsedByMember)
            {
                WarningMessagesAdd(Message.DeleteIsNotAllowed);
            }
            if (WarningMessages().Count == 0)
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Plan_Endorse.Remove(planEndorse);
                        var planDetailsToDelete = db.PlanDetail_Endorse.Where(x => x.PolicyId == planEndorse.PolicyId && x.EndorseNumber == planEndorse.EndorseNumber && x.PlanId == planEndorse.PlanId);
                        db.PlanDetail_Endorse.RemoveRange(planDetailsToDelete);
                        db.SaveChanges();
                        dbTransaction.Commit();
                        SuccessMessagesAdd(Message.DeleteSuccess);
                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        WarningMessagesAdd(e.MessageToList());
                    }

                }

            }
            return RedirectToAction("Details", new { id = planEndorse.EndorseNumber, endorseType = EndorseType.MovePlan });

        }
        public ActionResult MovePlan()
        {
            var endorsements = db.Endorsement.Where(x => x.EndorseType == EndorseType.MovePlan).OrderByDescending(x => x.EndorseNumber);
            return View(endorsements);
        }
        public ActionResult LoadMember(string endorseNumber)
        {
            if (endorseNumber == null)
            {
                return HttpNotFound();

            }

            var endorsement = db.Endorsement.Find(endorseNumber);
            var policyId = endorsement.PolicyId;
            if (policyId == null)
            {
                return HttpNotFound();

            }

            var policy = db.Policy.Find(policyId);
            if (policy == null)
            {
                return HttpNotFound();
            }
            var model = policy.Member.Where(x => !endorsement.Member_Endorse.ToList().Any(x2 => x.ClientId == x2.ClientId) && x.MemberStatus == MemberStatus.Active);
            if (endorsement.EndorseType == EndorseType.TerminateMember)
            {
                model = model.Where(x => x.EndorseNumber == null || db.Endorsement.Any(x2=>x.EndorseNumber == x.EndorseNumber && x2.EndorseNumber == x.EndorseNumber && x2.EndorseType == EndorseType.Additional));
            }
            if(endorsement.EndorseType == EndorseType.Reactivate)
            {
                model = model.ToList().Where(x => x.MemberStatus.ToString().Contains("Terminate"));
            }
            ViewBag.Endorsement = endorsement;
            if (Request.IsAjaxRequest())
            {
                return View("_Modal", new ModalView()
                {
                    Title = "Load Member Policy",
                    Body = this.RenderRazorViewToString("LoadMember", model.ToList()),
                    Footer = this.GetHtmlHelper().TextBox("Load Selected Member", "Load Selected Member", null, new { @class = "btn btn-primary", @type = "submit" }).ToString(),
                    ModalForm = new ModalForm { ActionName = "LoadMember/" + endorseNumber, ControllerName = "PolicyEndorsement", RouteValues = new { endorseNumber } }
                });

            }
            return null;
        }
        [HttpPost]
        [ActionName("LoadMember")]
        public ActionResult LoadMemberConfirm(string endorseNumber)
        {
            if (endorseNumber == null)
            {
                return HttpNotFound();

            }

            var endorsement = db.Endorsement.Find(endorseNumber);
            var policyId = endorsement.PolicyId;
            if (policyId == null)
            {
                return HttpNotFound();

            }

            var policy = db.Policy.Find(policyId);
            if (policy == null)
            {
                return HttpNotFound();
            }
            var memberToIsLoadedPair = new List<(Member member, bool isLoaded)>();
            foreach (var item in policy.Member)
            {
                var isLoaded = (Request.Params.GetValues(item.MemberId.ToString())?.FirstOrDefault() ?? "false") == "true" ? true : false;
                if (isLoaded)
                {
                    // check whether the member has bigger endorse_date than policy endorse_date specially in MemberTerminate.
                    // member endorse_date is not allowed if it has bigger endorse_date than policy endorse_date.
                    if (endorsement.EndorseType == EndorseType.TerminateMember)
                    {
                        if (endorsement.EndorseDate <= item.LastEndorseDate)
                        {
                            WarningMessagesAdd(item.Client.FullName + " Is Not Loaded, Member LastEndorseDate Is Bigger Than EndorseDate");
                        }
                        else
                        {
                            memberToIsLoadedPair.Add((item, isLoaded));
                        }
                    }
                    else
                    {
                        memberToIsLoadedPair.Add((item, isLoaded));
                    }
                }
            }
            if (memberToIsLoadedPair.Where(x => x.isLoaded == true).Count() == 0)
            {
                WarningMessagesAdd("No Data To Load");
            }
            if (WarningMessages().Count == 0)
            {

                try
                {
                    using (var dbTransaction = db.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {

                        if (endorsement.EndorseType == EndorseType.Mutation)
                        {
                            foreach (var item in memberToIsLoadedPair.Where(x => x.isLoaded == true && x.member.Client.RelateTo == null))
                            {
                                var newMemberEndorse = new Member_Endorse();
                                newMemberEndorse.EndorseNumber = endorsement.EndorseNumber;
                                newMemberEndorse.AdmedikaCode = item.member.AdmedikaCode;
                                newMemberEndorse.Age = item.member.Age;
                                newMemberEndorse.CardNumber = item.member.CardNumber;
                                newMemberEndorse.ClaimNumber = item.member.ClaimNumber;
                                newMemberEndorse.ClientId = item.member.ClientId;
                                newMemberEndorse.EndDate = item.member.EndDate;
                                newMemberEndorse.EntryDate = item.member.EntryDate;
                                newMemberEndorse.ExitDate = item.member.ExitDate;
                                newMemberEndorse.IsActive = item.member.IsActive;
                                newMemberEndorse.IssueDate = DateTime.Now;
                                newMemberEndorse.LastClaimDate = item.member.LastClaimDate;
                                newMemberEndorse.LastEndorseDate = endorsement.EndorseDate;
                                newMemberEndorse.MemberStatus = item.member.MemberStatus;
                                newMemberEndorse.PlanId = item.member.PlanId;
                                newMemberEndorse.PolicyId = item.member.PolicyId;
                                //newMemberEndorse.ProcessDate = item.member.ProcessDate;
                                newMemberEndorse.MemberNumber = item.member.MemberNumber;
                                newMemberEndorse.StartDate = item.member.StartDate;
                                newMemberEndorse.TerminateDate = endorsement.EndorseDate;

                                newMemberEndorse.SetPropertyCreate();

                                item.member.MemberStatus = MemberStatus.Endorse;

                                db.Member_Endorse.Add(newMemberEndorse);
                                db.Entry(item.member).State = EntityState.Modified;
                                db.SaveChanges();
                                var memberPlanList = db.MemberPlan.Where(x => x.PolicyId == item.member.PolicyId && x.MemberId == item.member.MemberId).ToList();
                                foreach (var memberPlan in memberPlanList)
                                {
                                    var newMemberPlanEndorse = new MemberPlan_Endorse();
                                    newMemberPlanEndorse.EndorseNumber = endorsement.EndorseNumber;
                                    newMemberPlanEndorse.PolicyId = memberPlan.PolicyId;
                                    newMemberPlanEndorse.MemberId = newMemberEndorse.MemberId;
                                    newMemberPlanEndorse.PlanId = newMemberEndorse.PlanId;
                                    newMemberPlanEndorse.BasicProductId = memberPlan.BasicProductId;
                                    newMemberPlanEndorse.StartDate = memberPlan.StartDate;
                                    newMemberPlanEndorse.BasicProductLimitCode = memberPlan.BasicProductLimitCode;
                                    newMemberPlanEndorse.SetPropertyCreate();
                                    db.MemberPlan_Endorse.Add(newMemberPlanEndorse);
                                }
                                db.SaveChanges();
                                var memberPcfs = item.member.PCF.Where(x => x.PolicyId == endorsement.PolicyId).ToList();
                                foreach (var itemMemberPcf in memberPcfs)
                                {
                                    var pcfEndorse = new PCF_Endorse();
                                    pcfEndorse.MemberId = newMemberEndorse.MemberId;
                                    pcfEndorse.BasicProductId = itemMemberPcf.BasicProductId;
                                    pcfEndorse.DueDate = itemMemberPcf.DueDate;
                                    pcfEndorse.EndorseNumber = endorseNumber;
                                    pcfEndorse.InvoiceDate = itemMemberPcf.InvoiceDate;
                                    pcfEndorse.PolicyId = itemMemberPcf.PolicyId;
                                    pcfEndorse.TransType = itemMemberPcf.TransType;
                                    pcfEndorse.Amount = itemMemberPcf.Amount;
                                    pcfEndorse.TransactionNumber = itemMemberPcf.TransactionNumber;
                                    pcfEndorse.SetPropertyCreate();
                                    db.PCF_Endorse.Add(pcfEndorse);
                                }
                                db.SaveChanges();
                            }
                            foreach (var item in memberToIsLoadedPair.Where(x => x.isLoaded == true && x.member.Client.RelateTo != null))
                            {
                                var newMemberEndorse = new Member_Endorse();
                                newMemberEndorse.EndorseNumber = endorsement.EndorseNumber;
                                newMemberEndorse.AdmedikaCode = item.member.AdmedikaCode;
                                newMemberEndorse.Age = item.member.Age;
                                newMemberEndorse.CardNumber = item.member.CardNumber;
                                newMemberEndorse.ClaimNumber = item.member.ClaimNumber;
                                newMemberEndorse.ClientId = item.member.ClientId;
                                newMemberEndorse.EndDate = item.member.EndDate;
                                newMemberEndorse.EntryDate = item.member.EntryDate;
                                newMemberEndorse.ExitDate = item.member.ExitDate;
                                newMemberEndorse.IsActive = item.member.IsActive;
                                //newMemberEndorse.IssueDate = DateTime.Now;
                                newMemberEndorse.LastClaimDate = item.member.LastClaimDate;
                                newMemberEndorse.LastEndorseDate = endorsement.EndorseDate;
                                newMemberEndorse.MemberStatus = item.member.MemberStatus;
                                newMemberEndorse.PlanId = item.member.PlanId;
                                newMemberEndorse.PolicyId = item.member.PolicyId;
                                //newMemberEndorse.ProcessDate = item.member.ProcessDate;
                                newMemberEndorse.MemberNumber = item.member.MemberNumber;
                                newMemberEndorse.TerminateDate = endorsement.EndorseDate;

                                var allEndorsementMemberSameRelateTo = db.Member_Endorse.Where(x => x.EndorseNumber == endorsement.EndorseNumber && x.Client.RelateTo == item.member.Client.RelateTo).Select(x => x.SequencialNo).ToList();
                                if (allEndorsementMemberSameRelateTo.Count > 0)
                                {
                                    newMemberEndorse.SequencialNo = allEndorsementMemberSameRelateTo.Max() + 1;
                                }
                                else
                                {
                                    newMemberEndorse.SequencialNo = 1;
                                }

                                newMemberEndorse.StartDate = item.member.StartDate;
                                newMemberEndorse.TerminateDate = endorsement.EndorseDate;

                                newMemberEndorse.SetPropertyCreate();

                                item.member.MemberStatus = MemberStatus.Endorse;

                                db.Member_Endorse.Add(newMemberEndorse);
                                db.Entry(item.member).State = EntityState.Modified;
                                db.SaveChanges();
                                var memberPlanList = db.MemberPlan.Where(x => x.PolicyId == item.member.PolicyId && x.MemberId == item.member.MemberId).ToList();
                                foreach (var memberPlan in memberPlanList)
                                {
                                    var newMemberPlanEndorse = new MemberPlan_Endorse();
                                    newMemberPlanEndorse.EndorseNumber = endorsement.EndorseNumber;
                                    newMemberPlanEndorse.PolicyId = memberPlan.PolicyId;
                                    newMemberPlanEndorse.MemberId = newMemberEndorse.MemberId;
                                    newMemberPlanEndorse.PlanId = newMemberEndorse.PlanId;
                                    newMemberPlanEndorse.BasicProductId = memberPlan.BasicProductId;
                                    newMemberPlanEndorse.StartDate = memberPlan.StartDate;
                                    newMemberPlanEndorse.BasicProductLimitCode = memberPlan.BasicProductLimitCode;
                                    newMemberPlanEndorse.SetPropertyCreate();
                                    db.MemberPlan_Endorse.Add(newMemberPlanEndorse);
                                }
                                db.SaveChanges();
                                var memberPcfs = item.member.PCF.Where(x => x.PolicyId == endorsement.PolicyId).ToList();
                                foreach (var itemMemberPcf in memberPcfs)
                                {
                                    var pcfEndorse = new PCF_Endorse();
                                    pcfEndorse.MemberId = newMemberEndorse.MemberId;
                                    pcfEndorse.BasicProductId = itemMemberPcf.BasicProductId;
                                    pcfEndorse.DueDate = itemMemberPcf.DueDate;
                                    pcfEndorse.EndorseNumber = endorseNumber;
                                    pcfEndorse.InvoiceDate = itemMemberPcf.InvoiceDate;
                                    pcfEndorse.PolicyId = itemMemberPcf.PolicyId;
                                    pcfEndorse.TransType = itemMemberPcf.TransType;
                                    pcfEndorse.Amount = itemMemberPcf.Amount;
                                    pcfEndorse.TransactionNumber = itemMemberPcf.TransactionNumber;
                                    pcfEndorse.SetPropertyCreate();
                                    db.PCF_Endorse.Add(pcfEndorse);
                                }
                                db.SaveChanges();
                            }

                            dbTransaction.Commit();
                            SuccessMessagesAdd(Message.CreateSuccess);
                            return RedirectToAction("Details", new { tab = "member", id = endorsement.EndorseNumber, endorseType = endorsement.EndorseType });

                        }
                        else if (endorsement.EndorseType == EndorseType.Renewal)
                        {
                            var printCardAmountString = db.CommonListValue.Where(x => x.Text == CommonListValueConst.PrintCardFee).FirstOrDefault()?.Value;
                            var printCardAmount = Convert.ToDecimal(printCardAmountString);

                            foreach (var item in memberToIsLoadedPair)
                            {
                                if (item.isLoaded)
                                {
                                    //Update Policy To MovePlan
                                    if (policy.PolicyStatus != PolicyStatus.Endorse)
                                    {
                                        policy.PolicyStatus = PolicyStatus.Endorse;
                                        policy.SetPropertyUpdate();
                                        db.Entry(policy).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }

                                    var newMemberEndorse = new Member_Endorse();
                                    newMemberEndorse.EndorseNumber = endorsement.EndorseNumber;
                                    newMemberEndorse.AdmedikaCode = item.member.AdmedikaCode;
                                    newMemberEndorse.Age = item.member.Age;
                                    newMemberEndorse.CardNumber = item.member.CardNumber;
                                    newMemberEndorse.ClaimNumber = item.member.ClaimNumber;
                                    newMemberEndorse.ClientId = item.member.ClientId;
                                    newMemberEndorse.EndDate = endorsement.Policy_Endorse.MatureDate;
                                    newMemberEndorse.EntryDate = item.member.EntryDate;
                                    newMemberEndorse.ExitDate = item.member.ExitDate;
                                    newMemberEndorse.IsActive = item.member.IsActive;
                                    newMemberEndorse.IssueDate = DateTime.Now;
                                    newMemberEndorse.LastClaimDate = item.member.LastClaimDate;
                                    newMemberEndorse.LastEndorseDate = item.member.LastEndorseDate;
                                    newMemberEndorse.MemberNumber = item.member.MemberNumber;
                                    newMemberEndorse.MemberStatus = item.member.MemberStatus;
                                    newMemberEndorse.PlanId = item.member.PlanId;
                                    newMemberEndorse.PolicyId = item.member.PolicyId;
                                    //newMemberEndorse.ProcessDate = item.member.ProcessDate;

                                    newMemberEndorse.SequencialNo = item.member.SequencialNo;
                                    newMemberEndorse.StartDate = endorsement.Policy_Endorse.StartDate;
                                    newMemberEndorse.TerminateDate = item.member.TerminateDate;

                                    newMemberEndorse.SetPropertyCreate();

                                    item.member.MemberStatus = MemberStatus.Endorse;

                                    db.Member_Endorse.Add(newMemberEndorse);
                                    db.Entry(item.member).State = EntityState.Modified;
                                    db.SaveChanges();
                                    var memberPlanList = db.MemberPlan.Where(x => x.PolicyId == item.member.PolicyId && x.MemberId == item.member.MemberId).ToList();
                                    foreach (var memberPlan in memberPlanList)
                                    {
                                        var newMemberPlanEndorse = new MemberPlan_Endorse();
                                        newMemberPlanEndorse.EndorseNumber = endorsement.EndorseNumber;
                                        newMemberPlanEndorse.PolicyId = memberPlan.PolicyId;
                                        newMemberPlanEndorse.MemberId = newMemberEndorse.MemberId;
                                        newMemberPlanEndorse.PlanId = newMemberEndorse.PlanId;
                                        newMemberPlanEndorse.BasicProductId = memberPlan.BasicProductId;
                                        newMemberPlanEndorse.StartDate = memberPlan.StartDate;
                                        newMemberPlanEndorse.BasicProductLimitCode = memberPlan.BasicProductLimitCode;
                                        newMemberPlanEndorse.SetPropertyCreate();
                                        db.MemberPlan_Endorse.Add(newMemberPlanEndorse);
                                    }
                                    var memberPcfs = item.member.PCF.Where(x => x.PolicyId == endorsement.PolicyId).ToList();
                                    foreach (var itemMemberPcf in memberPcfs)
                                    {
                                        var pcfEndorse = new PCF_Endorse();
                                        pcfEndorse.MemberId = newMemberEndorse.MemberId;
                                        pcfEndorse.BasicProductId = itemMemberPcf.BasicProductId;
                                        pcfEndorse.DueDate = itemMemberPcf.DueDate;
                                        pcfEndorse.EndorseNumber = endorseNumber;
                                        pcfEndorse.InvoiceDate = itemMemberPcf.InvoiceDate;
                                        pcfEndorse.PolicyId = itemMemberPcf.PolicyId;
                                        pcfEndorse.TransType = itemMemberPcf.TransType;
                                        pcfEndorse.Amount = itemMemberPcf.Amount;
                                        pcfEndorse.TransactionNumber = itemMemberPcf.TransactionNumber;
                                        pcfEndorse.SetPropertyCreate();
                                        db.PCF_Endorse.Add(pcfEndorse);
                                    }
                                    var isPrintNewCard = Request.Params["PrintNewCard_" + item.member.MemberId.ToString()].ToLower().Split(',').Where(x => x == "true").ToList().FirstOrDefault() != null;
                                    if (isPrintNewCard)
                                    {
                                        var newMemberClientEndorse = new MemberClientEndorse();
                                        newMemberClientEndorse.MemberId = newMemberEndorse.MemberId;
                                        newMemberClientEndorse.ClientId = item.member.ClientId;
                                        newMemberClientEndorse.MemberNumber = newMemberEndorse.MemberNumber;
                                        newMemberClientEndorse.PrintNewCard = 1;
                                        newMemberClientEndorse.IsFinancialChange = 1;
                                        newMemberClientEndorse.SetPropertyCreate();
                                        db.MemberClientEndorse.Add(newMemberClientEndorse);
                                    }
                                    db.SaveChanges();

                                }
                            }


                            dbTransaction.Commit();
                            SuccessMessagesAdd(Message.CreateSuccess);
                            return RedirectToAction("Details", new { tab = "member", id = endorsement.EndorseNumber, endorseType = endorsement.EndorseType });

                        }

                        foreach (var item in memberToIsLoadedPair)
                        {
                            if (item.isLoaded)
                            {
                                //Update Policy To MovePlan
                                if (policy.PolicyStatus != PolicyStatus.Endorse)
                                {
                                    policy.PolicyStatus = PolicyStatus.Endorse;
                                    policy.SetPropertyUpdate();
                                    db.Entry(policy).State = EntityState.Modified;
                                    db.SaveChanges();
                                }

                                var newMemberEndorse = new Member_Endorse();
                                newMemberEndorse.EndorseNumber = endorsement.EndorseNumber;
                                newMemberEndorse.AdmedikaCode = item.member.AdmedikaCode;
                                newMemberEndorse.Age = item.member.Age;
                                newMemberEndorse.CardNumber = item.member.CardNumber;
                                newMemberEndorse.ClaimNumber = item.member.ClaimNumber;
                                newMemberEndorse.ClientId = item.member.ClientId;
                                newMemberEndorse.EndDate = item.member.EndDate;
                                newMemberEndorse.EntryDate = item.member.EntryDate;
                                newMemberEndorse.ExitDate = item.member.ExitDate;
                                newMemberEndorse.IsActive = item.member.IsActive;
                                newMemberEndorse.IssueDate = DateTime.Now;
                                newMemberEndorse.LastClaimDate = item.member.LastClaimDate;
                                newMemberEndorse.LastEndorseDate = item.member.LastEndorseDate;
                                newMemberEndorse.MemberNumber = item.member.MemberNumber;
                                newMemberEndorse.MemberStatus = item.member.MemberStatus;
                                newMemberEndorse.PlanId = item.member.PlanId;
                                newMemberEndorse.PolicyId = item.member.PolicyId;
                                //newMemberEndorse.ProcessDate = item.member.ProcessDate;

                                newMemberEndorse.SequencialNo = item.member.SequencialNo;
                                newMemberEndorse.StartDate = item.member.StartDate;
                                newMemberEndorse.TerminateDate = item.member.TerminateDate;

                                newMemberEndorse.SetPropertyCreate();

                                item.member.MemberStatus = MemberStatus.Endorse;

                                db.Member_Endorse.Add(newMemberEndorse);
                                db.Entry(item.member).State = EntityState.Modified;
                                db.SaveChanges();
                                var memberPlanList = db.MemberPlan.Where(x => x.PolicyId == item.member.PolicyId && x.MemberId == item.member.MemberId).ToList();
                                foreach (var memberPlan in memberPlanList)
                                {
                                    var newMemberPlanEndorse = new MemberPlan_Endorse();
                                    newMemberPlanEndorse.EndorseNumber = endorsement.EndorseNumber;
                                    newMemberPlanEndorse.PolicyId = memberPlan.PolicyId;
                                    newMemberPlanEndorse.MemberId = newMemberEndorse.MemberId;
                                    newMemberPlanEndorse.PlanId = newMemberEndorse.PlanId;
                                    newMemberPlanEndorse.BasicProductId = memberPlan.BasicProductId;
                                    newMemberPlanEndorse.StartDate = memberPlan.StartDate;
                                    newMemberPlanEndorse.BasicProductLimitCode = memberPlan.BasicProductLimitCode;
                                    newMemberPlanEndorse.SetPropertyCreate();
                                    db.MemberPlan_Endorse.Add(newMemberPlanEndorse);
                                }
                                var memberPcfs = item.member.PCF.Where(x => x.PolicyId == endorsement.PolicyId).ToList();
                                foreach (var itemMemberPcf in memberPcfs)
                                {
                                    var pcfEndorse = new PCF_Endorse();
                                    pcfEndorse.MemberId = newMemberEndorse.MemberId;
                                    pcfEndorse.BasicProductId = itemMemberPcf.BasicProductId;
                                    pcfEndorse.DueDate = itemMemberPcf.DueDate;
                                    pcfEndorse.EndorseNumber = endorseNumber;
                                    pcfEndorse.InvoiceDate = itemMemberPcf.InvoiceDate;
                                    pcfEndorse.PolicyId = itemMemberPcf.PolicyId;
                                    pcfEndorse.TransType = itemMemberPcf.TransType;
                                    pcfEndorse.Amount = itemMemberPcf.Amount;
                                    pcfEndorse.TransactionNumber = itemMemberPcf.TransactionNumber;
                                    pcfEndorse.SetPropertyCreate();
                                    db.PCF_Endorse.Add(pcfEndorse);
                                }



                            }
                        }
                        db.SaveChanges();
                        dbTransaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    WarningMessagesAdd(e.MessageToList());
                    return RedirectToAction("Details", new { tab = "member", id = endorsement.EndorseNumber, endorseType = endorsement.EndorseType });

                }
                SuccessMessagesAdd(Message.CreateSuccess);
                return RedirectToAction("Details", new { tab = "member", id = endorsement.EndorseNumber, endorseType = endorsement.EndorseType });

            }
            else
            {
                return RedirectToAction("Details", new
                {
                    tab = "member",
                    id = endorsement.EndorseNumber,
                    endorseType = endorsement.EndorseType
                });

            }


        }
        public ActionResult MemberChangePlan(long? id)
        {

            if (id == null)
            {
                return HttpNotFound();
            }
            var memberEndorsement = db.Member_Endorse.Find(id);


            if (Request.Params["getPlanDetail"] == "true")
            {
                var planId = Request.Params["planId"];
                if (planId == null)
                {
                    return HttpNotFound();
                }
                db.Configuration.ProxyCreationEnabled = false;
                var planDetail = db.PlanDetail_Endorse.Where(x => x.PlanId == planId && x.PolicyId == memberEndorsement.PolicyId && x.EndorseNumber == memberEndorsement.EndorseNumber).ToList();
                foreach (var item in planDetail)
                {
                    var basicProduct = new BasicProduct();
                    basicProduct = db.BasicProduct.Where(x => x.BasicProductId == item.BasicProductId).FirstOrDefault();
                    db.Configuration.ProxyCreationEnabled = false;
                    item.BasicProduct = new BasicProduct();
                    item.BasicProduct.BasicProductName = basicProduct.BasicProductName;
                    item.BasicProduct.BasicProductLimit = basicProduct.BasicProductLimit;
                }
                return this.Json(new
                {
                    data = planDetail
                }, JsonRequestBehavior.AllowGet);
            }



            var sliPlans = new List<SelectListItem>();
            var member = memberEndorsement.Client.Member.FirstOrDefault();
            var currentMemberPlan = db.Plan.Where(x => x.PlanId == member.PlanId && x.PolicyId == member.PolicyId).FirstOrDefault();
            var currentMemberPlanDetail = db.PlanDetail.Where(x => x.PlanId == member.PlanId && x.PolicyId == member.PolicyId).ToList();

            ViewBag.CurrentMemberPlan = currentMemberPlan;
            ViewBag.CurrentMemberPlanDetail = currentMemberPlanDetail;

            sliPlans.AddBlank();
            var targetPlan = memberEndorsement.Endorsement.Plan_Endorse.ToList().Where(x => x.PlanId != memberEndorsement.PlanId && x.PlanId != member.PlanId);
            foreach (var item in targetPlan)
            {
                sliPlans.AddItemValText(item.PlanId, item.PlanId);
            }
            ViewBag.PlanId = sliPlans.ToSelectList();
            var tuple = Tuple.Create(memberEndorsement, memberEndorsement);
            var newModalView = new ModalView()
            {
                ModalForm = new ModalForm { ActionName = "MemberChangePlan", ControllerName = "PolicyEndorsement" },
                Title = "Change Member Plan",
                Body = this.RenderRazorViewToString("MemberChangePlan", Tuple.Create(memberEndorsement, memberEndorsement)),
                Footer = this.GetHtmlHelper().TextBox("Submit", "Submit", null, new { @class = "btn btn-primary", @type = "submit" }).ToString()

            };
            return View("_Modal", newModalView);

        }
        [HttpPost]
        public ActionResult MemberChangePlan(Member_Endorse memberEndorse)
        {
            if (memberEndorse == null)
            {
                return HttpNotFound();
            }
            var memberEndorseToUpdate = db.Member_Endorse.Find(memberEndorse.MemberId);
            if (memberEndorseToUpdate == null)
            {
                return HttpNotFound();
            }
            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    // alur prosess
                    //1. Update MemberEndorse ke active
                    //2. Hapus MeberPlanEndorse, kemudian create data baru MemberPlanEndorse
                    //3. Hapus PCFEndorse

                    //1. Update MemberEndorse, set PlanId to the new PlanId, save to Database.
                    memberEndorseToUpdate.PlanId = memberEndorse.PlanId;
                    memberEndorseToUpdate.MemberStatus = MemberStatus.Active;
                    db.Entry(memberEndorseToUpdate).State = EntityState.Modified;
                    db.SaveChanges();

                    //2.
                    var memberPlanEndorse = db.MemberPlan_Endorse.Where(x => x.EndorseNumber == memberEndorseToUpdate.EndorseNumber && x.MemberId == memberEndorse.MemberId);
                    db.MemberPlan_Endorse.RemoveRange(memberPlanEndorse);
                    db.SaveChanges();

                    var memberPlanEndorseList = new List<MemberPlan_Endorse>();
                    foreach (var item in db.PlanDetail_Endorse.Where(x => x.EndorseNumber == memberEndorseToUpdate.EndorseNumber && x.PlanId == memberEndorseToUpdate.PlanId))
                    {
                        var memberPlan = new MemberPlan_Endorse();
                        memberPlan.PlanId = item.PlanId;
                        memberPlan.EndorseNumber = item.EndorseNumber;
                        memberPlan.PolicyId = item.PolicyId;
                        memberPlan.MemberId = memberEndorseToUpdate.MemberId;
                        memberPlan.BasicProductId = item.BasicProductId;
                        memberPlan.StartDate = memberEndorseToUpdate.Endorsement.EndorseDate;
                        memberPlan.BasicProductLimitCode = item.BasicProductLimitCode;
                        memberPlan.SetPropertyCreate();
                        memberPlanEndorseList.Add(memberPlan);
                    }
                    db.MemberPlan_Endorse.AddRange(memberPlanEndorseList);
                    db.SaveChanges();

                    var memberPCFEndorse = db.PCF_Endorse.Where(x => x.EndorseNumber == memberEndorseToUpdate.EndorseNumber && x.PolicyId == memberEndorseToUpdate.PolicyId && x.MemberId == memberEndorseToUpdate.MemberId);
                    db.PCF_Endorse.RemoveRange(memberPCFEndorse);
                    db.SaveChanges();

                    ////2. Update MemberPlan of Member.
                    //var memberPlansEndorse = db.MemberPlan_Endorse.Where(x => x.MemberId == memberEndorseToUpdate.MemberId && x.PolicyId == memberEndorseToUpdate.PolicyId && x.EndorseNumber == memberEndorseToUpdate.EndorseNumber).ToList();
                    //var planDetailsEndorse = db.PlanDetail_Endorse.Where(x => x.PolicyId == memberEndorseToUpdate.PolicyId && x.EndorseNumber == memberEndorseToUpdate.EndorseNumber && x.PlanId == memberEndorseToUpdate.PlanId).ToList();

                    //var listMemberPlanToUpdate = new List<MemberPlan_Endorse>();
                    //var listMemberPlanToCreate = new List<MemberPlan_Endorse>();

                    //var distinctPlanDetail = memberPlansEndorse.Select(x => new { x.BasicProductId, x.BasicProductLimitCode }).Intersect(planDetailsEndorse.Select(x => new { x.BasicProductId, x.BasicProductLimitCode }));


                    //foreach (var item in memberPlansEndorse)
                    //{
                    //    var planDetail = planDetailsEndorse.Where(x => x.BasicProductId == item.BasicProductId).FirstOrDefault();
                    //    if (planDetail != null)
                    //    {
                    //        if (item.IsActive == 0)
                    //        {
                    //            item.IsActive = 1;
                    //            item.StartDate = memberEndorseToUpdate.Endorsement.EndorseDate;
                    //        }
                    //        item.BasicProductLimitCode = planDetail.BasicProductLimitCode;
                    //        item.SetPropertyUpdate();
                    //        listMemberPlanToUpdate.Add(item);
                    //    }
                    //    else
                    //    {
                    //        item.IsActive = 0;
                    //        item.SetPropertyUpdate();
                    //        listMemberPlanToUpdate.Add(item);
                    //    }
                    //}
                    //foreach (var item in planDetailsEndorse)
                    //{
                    //    var memberPlan = memberPlansEndorse.Where(x => x.BasicProductId == item.BasicProductId).FirstOrDefault();
                    //    if (memberPlan == null)
                    //    {
                    //        var newMemberPlan = new MemberPlan_Endorse();
                    //        newMemberPlan.EndorseNumber = memberEndorseToUpdate.EndorseNumber;
                    //        newMemberPlan.PolicyId = memberEndorseToUpdate.PolicyId;
                    //        newMemberPlan.MemberId = memberEndorseToUpdate.MemberId;
                    //        newMemberPlan.StartDate = memberEndorseToUpdate.Endorsement.EndorseDate;
                    //        newMemberPlan.BasicProductId = item.BasicProductId;
                    //        newMemberPlan.BasicProductLimitCode = item.BasicProductLimitCode;
                    //        newMemberPlan.SetPropertyCreate();
                    //        listMemberPlanToCreate.Add(newMemberPlan);
                    //    }

                    //}

                    //foreach (var item in listMemberPlanToCreate)
                    //{
                    //    db.MemberPlan_Endorse.Add(item);
                    //}
                    //foreach (var item in listMemberPlanToUpdate)
                    //{
                    //    db.Entry(item).State = EntityState.Modified;
                    //}
                    if (WarningMessages().Count == 0)
                    {
                        db.SaveChanges();
                        dbTransaction.Commit();
                        SuccessMessagesAdd(Message.UpdateSuccess);
                        return RedirectToAction("Details", new { id = memberEndorseToUpdate.EndorseNumber, tab = "member" });
                    }
                    else
                    {
                        dbTransaction.Rollback();
                    }

                }
                catch (Exception e)
                {
                    dbTransaction.Rollback();
                    WarningMessagesAdd(Message.CreateFail);
                    WarningMessagesAdd(e.MessageToList());

                }
            }
            return RedirectToAction("Details", new { id = memberEndorse.EndorseNumber, tab = "member", endorseType = EndorseType.MovePlan });

        }
        [HttpGet]
        public ActionResult Mutation()
        {
            return View(db.Endorsement.Where(x => x.EndorseType == EndorseType.Mutation));
        }
        [HttpGet]
        public ActionResult TransitionData()
        {
            var model = db.Endorsement.Where(x => x.EndorseType == EndorseType.TransitionData);
            return View(model);
        }
        [HttpGet]
        public ActionResult MemberTransitionData(long id)
        {
            if (Request.Params["onChangedPolicyTargetIsHit"] == "true")
            {
                var policyTargetId = Request.Params["policyTargetId"];
                if (policyTargetId != null)
                {
                    var policyTargetPlan = db.Plan.Where(x => x.Policy.PolicyNumber == policyTargetId).ToList();
                    var sliPlanIdNew = new List<SelectListItem>();
                    sliPlanIdNew.AddBlank();
                    foreach (var item in policyTargetPlan)
                    {
                        var planDetail = "";
                        var planDetailOfPlan = db.PlanDetail.Where(x => x.PlanId == item.PlanId && x.PolicyId == item.PolicyId);
                        foreach (var itemPlanDetail in planDetailOfPlan)
                        {
                            planDetail = planDetail + " " + itemPlanDetail.BasicProductLimitCode;
                        }
                        sliPlanIdNew.AddItemValText(item.PlanId, item.PlanName + " " + planDetail);
                    }
                    return Json(sliPlanIdNew, JsonRequestBehavior.AllowGet);

                }
                WarningMessagesAdd("PolicyTarget Is Null");

                return Json(new
                {
                    Message = WarningMessages()
                }, JsonRequestBehavior.AllowGet);
            }


            var member = db.Member_Endorse.Where(x => x.MemberId == id).FirstOrDefault();
            MemberClientEndorse newMemberClientEndorse;
            newMemberClientEndorse = member.MemberClientEndorse.FirstOrDefault();
            if (newMemberClientEndorse == null)
            {
                newMemberClientEndorse = new MemberClientEndorse();
                newMemberClientEndorse.MemberId = member.MemberId;
                newMemberClientEndorse.Member_Endorse = member;

                newMemberClientEndorse.MemberNumber = member.MemberNumber;
                newMemberClientEndorse.ClientId = member.ClientId;
                newMemberClientEndorse.Type = member.Client.Type;
                newMemberClientEndorse.BranchCode = member.Client.BranchCode;
                newMemberClientEndorse.ContactPerson = member.Client.ContactPerson;
                newMemberClientEndorse.ShortName = member.Client.ShortName;
                newMemberClientEndorse.FullName = member.Client.FullName;
                newMemberClientEndorse.PrefixClientTitle = member.Client.FullName;
                newMemberClientEndorse.EndfixClientTitle = member.Client.EndfixClientTitle;
                newMemberClientEndorse.BirthDate = member.Client.BirthDate;
                newMemberClientEndorse.BirthPlace = member.Client.BirthPlace;
                newMemberClientEndorse.IdNumber = member.Client.IdNumber;
                newMemberClientEndorse.MaritalStatus = member.Client.MaritalStatus;
                newMemberClientEndorse.Sex = member.Client.Sex;
                newMemberClientEndorse.Email = member.Client.Email;
                newMemberClientEndorse.EmailAddress1 = member.Client.EmailAddress1;
                newMemberClientEndorse.EmailAddress2 = member.Client.EmailAddress2;
                newMemberClientEndorse.Address = member.Client.Address;
                newMemberClientEndorse.MobilePhone1 = member.Client.MobilePhone1;
                newMemberClientEndorse.MObilePhone2 = member.Client.MObilePhone2;
                newMemberClientEndorse.MobilePhone3 = member.Client.MobilePhone3;
                newMemberClientEndorse.ClientRelation = member.Client.ClientRelation;
                newMemberClientEndorse.RelateTo = member.Client.RelateTo;
                newMemberClientEndorse.BankAccountNumber = member.Client.BankAccountNumber;
                newMemberClientEndorse.BankAccountCode = member.Client.BankAccountCode;
                newMemberClientEndorse.BankAccountName = member.Client.BankAccountName;
                newMemberClientEndorse.Status = member.Client.Status;

                newMemberClientEndorse.SetPropertyCreate();
                db.MemberClientEndorse.Add(newMemberClientEndorse);
                db.SaveChanges();

            }

            if (member == null)
            {
                return HttpNotFound();
            }
            var endorseChildMember = db.Member_Endorse.Where(x => x.Endorsement.Endorsement2.EndorseNumber == member.EndorseNumber && x.Endorsement.EndorseStatus == EndorseStatus.New && x.MemberNumber == member.MemberNumber).FirstOrDefault();
            var sliPolicyList = new List<SelectListItem>();
            sliPolicyList.AddBlank();
            var policyList = db.Policy.Where(x => x.PolicyNumber != null && x.PolicyStatus == PolicyStatus.Active).ToList();
            foreach (var item in policyList)
            {
                sliPolicyList.AddItemValText(item.PolicyNumber, item.PolicyNumber + " " + item.Client.FullName);
            }


            ViewBag.PolicyTarget = sliPolicyList.ToSelectList(endorseChildMember?.Policy.PolicyNumber);

            var sliPlanId = new List<SelectListItem>();
            sliPlanId.AddBlank();
            ViewBag.PlanId = sliPlanId.ToSelectList();

            var sliMemberSex = new List<SelectListItem>();
            sliMemberSex.AddBlank();
            foreach (var item in db.CommonListValue.Where(x => x.CommonListValue2.Text == "Sex").ToList())
            {
                sliMemberSex.AddItemValText(item.Value, item.Text);
            }

            ViewBag.Sex = sliMemberSex.ToSelectList(newMemberClientEndorse.Sex);

            var sliMemberRelationship = new List<SelectListItem>();
            sliMemberRelationship.AddBlank();
            foreach (var item in db.CommonListValue.Where(x => x.CommonListValue2.Text == "Client Relation").ToList())
            {
                sliMemberRelationship.AddItemValText(item.Value, item.Text);
            }
            ViewBag.ClientRelation = sliMemberRelationship.ToSelectList(newMemberClientEndorse.ClientRelation);

            var sliMaritalStatus = new List<SelectListItem>();
            sliMaritalStatus.AddBlank();
            foreach (var item in db.CommonListValue.Where(x => x.CommonListValue2.Text == "Marital Status").ToList())
            {
                sliMaritalStatus.AddItemValText(item.Value, item.Text);
            }
            ViewBag.MaritalStatus = sliMaritalStatus.ToSelectList(newMemberClientEndorse.MaritalStatus);


            var sliClientBankInformation = new List<SelectListItem>();
            sliClientBankInformation.AddBlank();
            foreach (var item in db.CommonListValue.Where(x => x.CommonListValue2.Text == "BankInformation").ToList())
            {
                sliClientBankInformation.AddItemValText(item.Value, item.Text);
            }
            ViewBag.BankAccountCode = sliClientBankInformation.ToSelectList(newMemberClientEndorse.BankAccountCode);

            ViewBag.Member = db.Member.Where(x => x.MemberNumber == member.MemberNumber).FirstOrDefault();
            var newModalView = new ModalView()
            {
                ModalForm = new ModalForm { ActionName = "MemberTransitionData", ControllerName = "PolicyEndorsement" },
                Title = "Transition Data Member",
                Body = this.RenderRazorViewToString("_MemberTransitionData", newMemberClientEndorse),
                Footer = this.GetHtmlHelper().TextBox("Submit", "Submit", null, new { @class = "btn btn-primary", @type = "submit" }).ToString()

            };
            return View("_Modal", newModalView);
        }
        [HttpPost]
        public ActionResult MemberTransitionData(MemberClientEndorse model)
        {

            //using (var dbTransactionNew = db.Database.BeginTransaction())
            //{
            //    var result = db.Database.SqlQuery<decimal?>("exec [spAddAndUpdate] @first, @second, @third", new SqlParameter("first", 1), new SqlParameter("second", 2), new SqlParameter("third", "Wesly Ganteng")).FirstOrDefault();
            //    dbTransactionNew.Rollback();
            //}

            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var memberEndorse = db.Member_Endorse.Where(x => x.MemberId == model.MemberId).FirstOrDefault();
            if (memberEndorse == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Business Validation

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                WarningMessagesAdd("Invalid Name");
            }
            if (model.Member_Endorse.StartDate < memberEndorse.Policy.StartDate || model.Member_Endorse.StartDate > memberEndorse.Policy.MatureDate || model.Member_Endorse.StartDate >= model.Member_Endorse.EndDate ||
                model.Member_Endorse.EndDate > memberEndorse.Policy.MatureDate || model.Member_Endorse.EndDate < memberEndorse.Policy.StartDate
                )
            {
                WarningMessagesAdd("Member StartDate Or EndDate Invalid");
            }
            if (string.IsNullOrWhiteSpace(model.Sex))
            {
                WarningMessagesAdd("Sex Is Required");
            }
            var clientRelation = memberEndorse.Client.ClientRelation;
            if ((string.IsNullOrWhiteSpace(model.ClientRelation) && !string.IsNullOrWhiteSpace(clientRelation)) || (!string.IsNullOrWhiteSpace(model.ClientRelation) && string.IsNullOrWhiteSpace(clientRelation)))
            {
                WarningMessagesAdd("Invalid Client Relation");
            }
            if (model.BirthDate >= DateTime.Now || model.BirthDate == null)
            {
                WarningMessagesAdd("Invalid Date Of Birth");
            }
            if (string.IsNullOrWhiteSpace(model.BankAccountName))
            {
                WarningMessagesAdd("Invalid Bank Account Name");
            }
            if (string.IsNullOrWhiteSpace(model.BankAccountNumber))
            {
                WarningMessagesAdd("Invalid Bank Account Number");
            }
            if (string.IsNullOrWhiteSpace(model.BankAccountCode))
            {
                WarningMessagesAdd("Invalid Bank Name");
            }
            if (string.IsNullOrWhiteSpace(model.MaritalStatus))
            {
                WarningMessagesAdd("Marital Status Is Required");
            }

            model.PrintNewCard = null;
            if (Request.Params["LostCard"].ToString().Split(',').Count() == 2)
            {
                model.PrintNewCard = 1;
                model.LostCard = 1;
            }
            if (model.FullName != memberEndorse.Client.FullName)
            {
                model.PrintNewCard = 1;
            }
            if (model.PrintNewCard == 1 && string.IsNullOrWhiteSpace(memberEndorse.CardNumber))
            {
                WarningMessagesAdd("CardNumber Is Not Exist");
            }
            //Financial Or Non-Financial Validation
            model.IsFinancialChange = 0;
            if (model.FullName.ToLower() != memberEndorse.Client.FullName.ToLower())
            {
                model.IsFinancialChange = 1;
            }
            if (model.Sex != memberEndorse.Client.Sex)
            {
                if (model.MaritalStatus == MaritalStatus.Married || WebAppUtility.IsAdult(model.BirthDate))
                {
                    model.IsFinancialChange = 1;
                }
            }
            if ((model?.ClientRelation == ClientRelation.Husband || model?.ClientRelation == ClientRelation.Wife) && (memberEndorse.Client?.ClientRelation == ClientRelation.Son || memberEndorse.Client?.ClientRelation == ClientRelation.Daughter))
            {
                model.IsFinancialChange = 1;
            }
            if (model?.LostCard == 1 || model?.PrintNewCard == 1)
            {
                model.IsFinancialChange = 1;
            }
            if (WebAppUtility.IsAdult(model.BirthDate) && !WebAppUtility.IsAdult(memberEndorse.Client.BirthDate))
            {
                model.IsFinancialChange = 1;
            }
            if (model.BankAccountName.ToLower() != memberEndorse.Client.BankAccountName.ToLower())
            {
                model.IsFinancialChange = 1;
            }
            if (model.Member_Endorse.StartDate != memberEndorse.StartDate || model.Member_Endorse.EndDate != memberEndorse.EndDate)
            {
                model.IsFinancialChange = 1;
            }
            //validation for recordmode and recordmodesub
            model.IsPremiumCorrection = 0;
            if (model.Sex != memberEndorse.Client.Sex)
            {
                if (model.MaritalStatus == MaritalStatus.Married || WebAppUtility.IsAdult(model.BirthDate))
                {
                    model.RecordMode = 2;
                    model.RecordModeSub = 1;
                    model.IsPremiumCorrection = 1;
                }
                else if (model.MaritalStatus == MaritalStatus.Single && !WebAppUtility.IsAdult(model.BirthDate))
                {
                    model.RecordMode = 2;
                    model.RecordModeSub = 2;
                }
            }
            if ((model?.ClientRelation == ClientRelation.Husband || model?.ClientRelation == ClientRelation.Wife) && (memberEndorse.Client?.ClientRelation == ClientRelation.Son || memberEndorse.Client?.ClientRelation == ClientRelation.Daughter))
            {
                model.RecordMode = 2;
                model.RecordModeSub = 3;
                model.IsPremiumCorrection = 1;
            }
            if ((model?.ClientRelation == ClientRelation.Husband && memberEndorse.Client?.ClientRelation == ClientRelation.Wife) || (model?.ClientRelation == ClientRelation.Wife && memberEndorse.Client?.ClientRelation == ClientRelation.Husband))
            {
                model.RecordMode = 2;
                model.RecordModeSub = 4;
            }
            if (WebAppUtility.IsAdult(model.BirthDate) && !WebAppUtility.IsAdult(memberEndorse.Client.BirthDate))
            {
                model.RecordMode = 2;
                model.RecordModeSub = 5;
            }
            if (model.BirthDate != memberEndorse.Client.BirthDate && ((WebAppUtility.IsAdult(model.BirthDate) && WebAppUtility.IsAdult(memberEndorse.Client.BirthDate)) || (!WebAppUtility.IsAdult(model.BirthDate) && !WebAppUtility.IsAdult(memberEndorse.Client.BirthDate))))
            {
                model.RecordMode = 2;
                model.RecordModeSub = 6;
            }

            if (model.BankAccountName.ToLower() != memberEndorse.Client.BankAccountName.ToLower())
            {
                model.RecordMode = 2;
                model.RecordModeSub = 7;
            }
            if (model.BankAccountNumber.ToLower() != memberEndorse.Client.BankAccountNumber.ToLower())
            {
                model.RecordMode = 2;
                model.RecordModeSub = 8;
            }
            if (model.BankAccountNumber.ToLower() != memberEndorse.Client.BankAccountNumber.ToLower())
            {
                model.RecordMode = 2;
                model.RecordModeSub = 8;
            }
            if (model.BankAccountCode.ToLower() != memberEndorse.Client.BankAccountCode.ToLower())
            {
                model.RecordMode = 2;
                model.RecordModeSub = 9;
            }
            if (model.MaritalStatus.ToLower() != memberEndorse.Client.MaritalStatus.ToLower())
            {
                model.RecordMode = 2;
                model.RecordModeSub = 10;
            }
            if (model.Member_Endorse.StartDate != memberEndorse.StartDate || model.Member_Endorse.EndDate != memberEndorse.EndDate)
            {
                model.RecordMode = 4;
                model.IsPremiumCorrection = 1;
            }
            if (model.FullName.ToLower() != memberEndorse.Client.FullName.ToLower())
            {
                model.RecordMode = 8;
                model.RecordModeSub = 1;
            }
            if (WarningMessages().Count == 0)
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var startDate = model.Member_Endorse.StartDate;
                        var endDate = model.Member_Endorse.EndDate;
                        model.Member_Endorse = null;
                        memberEndorse.MemberStatus = MemberStatus.Active;
                        memberEndorse.StartDate = startDate;
                        memberEndorse.EndDate = endDate;
                        memberEndorse.SetPropertyUpdate();
                        db.Entry(memberEndorse).State = EntityState.Modified;
                        db.Entry(model).State = EntityState.Modified;
                        db.SaveChanges();
                        dbTransaction.Commit();
                        SuccessMessagesAdd(Message.CreateSuccess);
                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        WarningMessagesAdd(Message.CreateFail);
                        WarningMessagesAdd(e.MessageToList());
                    }
                }
            }
            return RedirectToAction("Details", new { id = memberEndorse.EndorseNumber, tab = "member", endorseType = EndorseType.TransitionData });
        }
        [HttpGet]
        public ActionResult Renewal()
        {
            var model = db.Endorsement.Where(x => x.EndorseType == EndorseType.Renewal);
            return View(model);
        }
        [HttpGet]
        public ActionResult Reactivate()
        {
            var model = db.Endorsement.Where(x => x.EndorseType == EndorseType.Reactivate);
            return View(model);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            db.Dispose();
        }
    }
}