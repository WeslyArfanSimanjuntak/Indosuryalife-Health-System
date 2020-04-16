using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Repository.Application.DataModel;

namespace Web.MainApplication.Controllers
{
    public class BasicProductLimitController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: BasicProductLimit
        public ActionResult Index()
        {
            var basicProductLimitHdrList = db.BasicProductLimitHdr.ToList();
            
            return View(basicProductLimitHdrList);
        }

        // GET: BasicProductLimit/Details/5
        public ActionResult Details(string id, string bpLimitId)
        {

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

        // GET: BasicProductLimit/Create
        public ActionResult Create()
        {
            ViewBag.BasicProductId = new SelectList(db.BasicProduct, "BasicProductId", "BasicProductName");
            ViewBag.BenefitCode = new SelectList(db.Benefit, "BenefitCode", "BenefitName");
            ViewBag.ListOfBenefit = db.Benefit.Where(x => x.IsActive == 1).ToList();

            return View(new BasicProductLimit());
        }

        // POST: BasicProductLimit/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,BasicProductLimitCode,BasicProductId,BenefitCode,LimitType,LimiNumber,LimitAmount,Pre,Post,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] BasicProductLimit basicProductLimit)
        {
            if (basicProductLimit.BasicProductId == null)
            {
                WarningMessagesAdd("BasicProduct can not be empty");
            }
            if (basicProductLimit.BasicProductLimitCode == null)
            {
                WarningMessagesAdd("LimitId can not be empty");
            }

            if (ModelState.IsValid && WarningMessages().Count == 0)
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {

                    try
                    {
                        var benefit = db.Benefit.ToList();
                        var listOfCheckedBenefit = new List<Benefit>();
                        var listOfBasicProductLimit = new List<BasicProductLimit>();
                        var basicProductLimitHeader = new BasicProductLimitHdr();
                        basicProductLimitHeader.BasicProductId = basicProductLimit.BasicProductId;
                        basicProductLimitHeader.BPLimitId = basicProductLimit.BasicProductLimitCode;
                        basicProductLimitHeader.Description = Request.Params["Benefit.BenefitDesc"];

                        basicProductLimitHeader.SetPropertyCreate();
                        db.BasicProductLimitHdr.Add(basicProductLimitHeader);
                        db.SaveChanges();
                        foreach (var item in benefit)
                        {
                            var value = Request.Params[item.BenefitCode];
                            if (Request.Params[item.BenefitCode] != null && Request.Params[item.BenefitCode] == "true,false")
                            {
                                var pre = 0;
                                var post = 0;
                                var limitNumber = 0;
                                int.TryParse(Request.Params[item.BenefitCode + "_Pre"], out pre);
                                int.TryParse(Request.Params[item.BenefitCode + "_Post"], out post);
                                int.TryParse(Request.Params[item.BenefitCode + "_LimitNumber"], out limitNumber);

                                var newBasicProductLimitCode = new BasicProductLimit()
                                {
                                    BenefitCode = item.BenefitCode,
                                    LimitType = Request[item.BenefitCode + "_LimitType"],
                                    LimitNumber = limitNumber,
                                    LimitAmount = Convert.ToDecimal(Request.Params[item.BenefitCode + "_LimitAmount"], CultureInfo.CurrentCulture),
                                    Pre = pre,
                                    Post = post,
                                    BasicProductId = basicProductLimit.BasicProductId,
                                    BasicProductLimitCode = basicProductLimit.BasicProductLimitCode

                                };
                                newBasicProductLimitCode.SetPropertyCreate();
                                listOfBasicProductLimit.Add(newBasicProductLimitCode);

                            }

                        }

                        basicProductLimit.SetPropertyCreate();
                        db.BasicProductLimit.AddRange(listOfBasicProductLimit);
                        db.SaveChanges();
                        dbTransaction.Commit();
                        SuccessMessagesAdd("Inserting Data Success");
                        return RedirectToAction("Index");

                    }
                    catch (Exception e)
                    {
                        WarningMessagesAdd(e.MessageToList());
                        dbTransaction.Rollback();
                    }
                }
            }
            ViewBag.ListOfBenefit = db.Benefit.Where(x => x.IsActive == 1).ToList();
            ViewBag.BasicProductId = new SelectList(db.BasicProduct, "BasicProductId", "BasicProductName", basicProductLimit.BasicProductId);
            ViewBag.BenefitCode = new SelectList(db.Benefit, "BenefitCode", "BenefitName", basicProductLimit.BenefitCode);

            return View(basicProductLimit);
        }

        // GET: BasicProductLimit/Edit/5
        public ActionResult Edit(string id, string bpLimitId)
        {

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

            ViewBag.ListOfBenefit = tempBenefitList.OrderByDescending(x => x.IsActive).ThenBy(x => x.BenefitCode).ToList();
            return View(basicProductLimitHdr);
        }

        // POST: BasicProductLimit/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BPLimitId,BasicProductId,Description,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive")] BasicProductLimitHdr basicProductLimit)
        {
            //WarningMessagesAdd("error");
            if (ModelState.IsValid && WarningMessages().Count == 0)
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {

                        var benefits = db.Benefit.Where(x => x.IsActive == 1).ToList();
                        var listOfCheckedBenefit = new List<Benefit>();
                        var listOfBasicProductLimit = new List<BasicProductLimit>();

                        foreach (var item in benefits)
                        {
                            var value = Request.Params[item.BenefitCode];
                            if (value != null && value.Split(',').Contains("true"))
                            {


                                var newBasicProductLimitCode = new BasicProductLimit();

                                var pre = 0;
                                var post = 0;
                                var limitNumber = 0;
                                int.TryParse(Request.Params[item.BenefitCode + "_Pre"], out pre);
                                int.TryParse(Request.Params[item.BenefitCode + "_Post"], out post);
                                int.TryParse(Request.Params[item.BenefitCode + "_LimitNumber"], out limitNumber);

                                newBasicProductLimitCode.BenefitCode = item.BenefitCode;
                                newBasicProductLimitCode.LimitType = Request[item.BenefitCode + "_LimitType"];
                                
                                newBasicProductLimitCode.LimitNumber = limitNumber;

                                newBasicProductLimitCode.LimitAmount = Convert.ToDecimal(Request.Params[item.BenefitCode + "_LimitAmount"], CultureInfo.CurrentCulture);
                                
                                newBasicProductLimitCode.Pre = pre;
                                
                                newBasicProductLimitCode.Post = post;

                                newBasicProductLimitCode.BasicProductId = basicProductLimit.BasicProductId;
                                newBasicProductLimitCode.BasicProductLimitCode = basicProductLimit.BPLimitId;


                                listOfBasicProductLimit.Add(newBasicProductLimitCode);

                            }
                            else
                            {
                                var existBasicProductLimit = db.BasicProductLimit.Where(x => x.BasicProductLimitCode == basicProductLimit.BPLimitId
                        && x.BasicProductId == basicProductLimit.BasicProductId && x.BenefitCode == item.BenefitCode).FirstOrDefault();
                                if (existBasicProductLimit != null)
                                {
                                    db.BasicProductLimit.Remove(existBasicProductLimit);

                                }
                            }


                        }

                        foreach (var item in listOfBasicProductLimit)
                        {
                            var bplimit = db.BasicProductLimit.Where(x => x.BasicProductLimitCode == basicProductLimit.BPLimitId
                            && x.BasicProductId == basicProductLimit.BasicProductId && x.BenefitCode == item.BenefitCode).FirstOrDefault();

                            if (bplimit != null)
                            {
                                bplimit.LimitAmount = item.LimitAmount;
                                bplimit.LimitNumber = item.LimitNumber;
                                bplimit.LimitType = item.LimitType;
                                bplimit.Post = item.Post;
                                bplimit.Pre = item.Pre;
                                bplimit.SetPropertyUpdate();
                                db.Entry(bplimit).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                            {
                                bplimit = new BasicProductLimit();
                                bplimit.BasicProductLimitCode = basicProductLimit.BPLimitId;
                                bplimit.BasicProductId = basicProductLimit.BasicProductId;
                                bplimit.BenefitCode = item.BenefitCode;
                                bplimit.LimitAmount = item.LimitAmount;
                                bplimit.LimitNumber = item.LimitNumber;
                                bplimit.LimitType = item.LimitType;
                                bplimit.Post = item.Post;
                                bplimit.Pre = item.Pre;
                                bplimit.SetPropertyUpdate();
                                db.BasicProductLimit.Add(bplimit);

                            }
                            db.SaveChanges();

                        }
                        basicProductLimit.SetPropertyUpdate();
                        db.Entry(basicProductLimit).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        dbTransaction.Commit();
                        SuccessMessagesAdd("Editing Data Success");
                        return RedirectToAction("Index");

                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        WarningMessagesAdd(e.MessageToList());
                    }
                }

            }
            var tempBenefitList = new List<Benefit>();

            var selectedBenefit = db.Benefit.Where(x => x.IsActive == 1).ToList();

            foreach (var item in selectedBenefit)
            {
                var newBenefit = new Benefit();
                newBenefit.BenefitCode = item.BenefitCode;
                newBenefit.BenefitDesc = item.BenefitDesc;
                newBenefit.BenefitName = item.BenefitName;
                var paramValue = Request.Params[item.BenefitCode];
                if (paramValue != null && paramValue.Split(',').Contains("true"))
                {
                    try
                    {
                        var newBasicProductLimitCode = new BasicProductLimit();

                        var pre = 0;
                        var post = 0;
                        var limitNumber = 0;
                        int.TryParse(Request.Params[item.BenefitCode + "_Pre"], out pre);
                        int.TryParse(Request.Params[item.BenefitCode + "_Post"], out post);
                        int.TryParse(Request.Params[item.BenefitCode + "_LimitNumber"], out limitNumber);

                        newBasicProductLimitCode.BenefitCode = item.BenefitCode;
                        newBasicProductLimitCode.LimitType = Request[item.BenefitCode + "_LimitType"];
                        
                        newBasicProductLimitCode.LimitNumber = limitNumber;

                        newBasicProductLimitCode.LimitAmount = Convert.ToDecimal(Request.Params[item.BenefitCode + "_LimitAmount"], CultureInfo.CurrentCulture);
                        
                        newBasicProductLimitCode.Pre = pre;
                        newBasicProductLimitCode.Post = post;

                        newBasicProductLimitCode.BasicProductId = basicProductLimit.BasicProductId;
                        newBasicProductLimitCode.BasicProductLimitCode = basicProductLimit.BPLimitId;

                        newBenefit.BasicProductLimit = new List<BasicProductLimit>();
                        newBenefit.BasicProductLimit.Add(newBasicProductLimitCode);
                        newBenefit.IsActive = 1;
                    }
                    catch (Exception e)
                    {

                        WarningMessagesAdd(e.MessageToList());
                    }



                }
                else
                {
                    newBenefit.BasicProductLimit = new List<BasicProductLimit>();
                    newBenefit.IsActive = 0;
                }
                tempBenefitList.Add(newBenefit);
            }



            ViewBag.ListOfBenefit = tempBenefitList.OrderByDescending(x => x.IsActive).ThenBy(x => x.BenefitCode).ToList();
            return View(basicProductLimit);
        }

        // GET: BasicProductLimit/Delete/5
        public ActionResult Delete(string BasicProductId, string BPLimitId)
        {
            if (BasicProductId == null || BPLimitId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var basicProductLimitHeader = db.BasicProductLimitHdr.Where(x => x.BasicProductId == BasicProductId && x.BPLimitId == BPLimitId).FirstOrDefault();
           
            if (basicProductLimitHeader == null)
            {
                return HttpNotFound();
            }
            return View(basicProductLimitHeader);
        }

        // POST: BasicProductLimit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string BasicProductId, string BPLimitId)
        {
            var basicProductLimitHeader = db.BasicProductLimitHdr.Where(x => x.BasicProductId == BasicProductId && x.BPLimitId == BPLimitId).FirstOrDefault();
            db.BasicProductLimitHdr.Remove(basicProductLimitHeader);
            var basicProductLimitList = db.BasicProductLimit.Where(x => x.BasicProductLimitCode == BPLimitId && x.BasicProductId == BasicProductId).ToList();
            db.BasicProductLimit.RemoveRange(basicProductLimitList);
            
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
