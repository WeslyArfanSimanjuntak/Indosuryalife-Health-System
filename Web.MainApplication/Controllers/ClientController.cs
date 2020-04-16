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
    public class ClientController : BaseController
    {
        private DBEntities db = new DBEntities();

        // GET: Client
        public ActionResult Index()
        {
            var client = db.Client;

            return View(client.ToList());
        }

        // GET: Client/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Client.Find(id);

            if (client == null)
            {
                return HttpNotFound();
            }
            //var type = Request.Params["type"];
            if (client.Type == "Personal")
            {
                return View(client);
            }
            return View("DetailsCompany", client);
            //return View(client);
        }

        // GET: Client/Create
        public ActionResult Create()
        {
            ViewBag.ContactPerson = new SelectList(db.Client, "ClientId", "Type");//client2
            ViewBag.RelateTo = new SelectList(db.Client, "ClientId", "Type");//client3

            var sliType = new List<SelectListItem>();
            sliType.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Type").ToList().ForEach(x =>
            {
                sliType.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Type = sliType.ToSelectList();

            var sliSex = new List<SelectListItem>();
            sliSex.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Sex").ToList().ForEach(x =>
            {
                sliSex.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Sex = sliSex.ToSelectList();

            var sliMaritalStatus = new List<SelectListItem>();
            sliMaritalStatus.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Marital Status").ToList().ForEach(x =>
            {
                sliMaritalStatus.AddItemValText(x.Value, x.Text);
            });
            ViewBag.MaritalStatus = sliMaritalStatus.ToSelectList();

            var sliClientRelation = new List<SelectListItem>();
            sliClientRelation.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Client Relation ").ToList().ForEach(x =>
            {
                sliClientRelation.AddItemValText(x.Value, x.Text);
            });
            ViewBag.ClientRelation = sliClientRelation.ToSelectList();
            var sliBankAccountCode = new List<SelectListItem>();
            sliBankAccountCode.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "BankInformation").OrderBy(x => x.Value).ToList().ForEach(x =>
              {
                  sliBankAccountCode.AddItemValText(x.Value, x.Value + " - " + x.Text);
              });
            ViewBag.BankAccountCode = sliBankAccountCode.ToSelectList();
            var type = Request.Params["type"];
            var model = new Client();
            if (type == "Personal")
            {
                model.Type = "Personal";
                return View(model);
            }

            model.Type = "Company";
            return View("CreateCompany", model);
        }

        // POST: Client/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ClientId,Type,ContactPerson,ShortName,BankAccountName,FullName,PrefixClientTitle,EndfixClientTitle,BirthDate,BirthPlace,MaritalStatus,Sex,Email,EmailAddress1,EmailAddress2,MobilePhone1,MObilePhone2,MobilePhone3,ClientRelation,RelateTo,BankAccountNumber,BankAccountCode,Status,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive,IdNumber,BranchCode,Address")] Client client)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (client.Type == "Company")
                    {
                        client.RelateTo = null;
                    }

                    //validation 
                    if (client.RelateTo != null)
                    {
                        var clientRelateTo = db.Client.Where(x => x.ClientId == client.RelateTo).FirstOrDefault();
                        if (clientRelateTo != null)
                        {
                            if (clientRelateTo.RelateTo != null)
                            {
                                WarningMessagesAdd("Client can not relate to " + client.FullName);
                            }
                        }

                    }

                    if (client.BirthDate > DateTime.Now)
                    {
                        WarningMessagesAdd("Client Birth Date can not be bigger than current Date");
                    }

                    if (db.Client.Where(x => x.IdNumber == client.IdNumber).FirstOrDefault() != null)
                    {
                        WarningMessagesAdd("Client already exist in database");
                    }

                    //client.ClientName = client.PrefixClientTitle +" "+ client.FullName +" "+ client.EndfixClientTitle;
                    if (WarningMessages().Count == 0)
                    {
                        client.ClientId = "";
                        client.SetPropertyCreate();
                        db.Client.Add(client);
                        db.SaveChanges();
                        SuccessMessagesAdd("Inserting Client Success");

                    }
                    return RedirectToAction("Index");

                }
                catch (Exception e)
                {
                    WarningMessagesAdd(e.MessageToList());
                    var errors = ModelState.Where(x => x.Value.Errors.Count > 0);
                    errors.ToList().ForEach(x =>
                    {
                        WarningMessagesAdd(x.Value.Errors.FirstOrDefault().ErrorMessage);

                    });
                }

            }
            var errorsState = ModelState.Where(x => x.Value.Errors.Count > 0);
            errorsState.ToList().ForEach(x =>
            {
                WarningMessagesAdd(x.Value.Errors.FirstOrDefault().ErrorMessage);

            });
            ViewBag.ContactPerson = new SelectList(db.Client, "ClientId", "Type", client.ContactPerson);
            ViewBag.RelateTo = new SelectList(db.Client, "ClientId", "Type", client.RelateTo);

            var sliType = new List<SelectListItem>();
            sliType.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Type").ToList().ForEach(x =>
            {
                sliType.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Type = sliType.ToSelectList(client.Type);

            var sliSex = new List<SelectListItem>();
            sliSex.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Sex").ToList().ForEach(x =>
            {
                sliSex.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Sex = sliSex.ToSelectList(client.Sex);

            var sliMaritalStatus = new List<SelectListItem>();
            sliMaritalStatus.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Marital Status").ToList().ForEach(x =>
            {
                sliMaritalStatus.AddItemValText(x.Value, x.Text);
            });
            ViewBag.MaritalStatus = sliMaritalStatus.ToSelectList(client.MaritalStatus);

            var sliClientRelation = new List<SelectListItem>();
            sliClientRelation.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Client Relation ").ToList().ForEach(x =>
            {
                sliClientRelation.AddItemValText(x.Value, x.Text);
            });
            ViewBag.ClientRelation = sliClientRelation.ToSelectList();
            var sliBankAccountCode = new List<SelectListItem>();
            sliBankAccountCode.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "BankInformation").OrderBy(x => x.Value).ToList().ForEach(x =>
              {
                  sliBankAccountCode.AddItemValText(x.Value, x.Value + " - " + x.Text);
              });
            ViewBag.BankAccountCode = sliBankAccountCode.ToSelectList(client.BankAccountCode);

            if (client.Type == "Personal")
            {
                return View(client);
            }
            return View("CreateCompany", client);
        }

        // GET: Client/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Client.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            ViewBag.ContactPerson = new SelectList(db.Client, "ClientId", "Type", client.ContactPerson);
            ViewBag.RelateTo = new SelectList(db.Client, "ClientId", "Type", client.RelateTo);

            var sliType = new List<SelectListItem>();
            sliType.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Type").ToList().ForEach(x =>
            {
                sliType.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Type = sliType.ToSelectList();

            var sliSex = new List<SelectListItem>();
            sliSex.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Sex").ToList().ForEach(x =>
            {
                sliSex.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Sex = sliSex.ToSelectList(client.Sex);

            var sliMaritalStatus = new List<SelectListItem>();
            sliMaritalStatus.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Marital Status").OrderBy(x => x.Value).ToList().ForEach(x =>
            {
                sliMaritalStatus.AddItemValText(x.Value, x.Text);
            });
            ViewBag.MaritalStatus = sliMaritalStatus.ToSelectList(client.MaritalStatus);

            var sliClientRelation = new List<SelectListItem>();
            sliClientRelation.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Client Relation ").ToList().ForEach(x =>
            {
                sliClientRelation.AddItemValText(x.Value, x.Text);
            });
            ViewBag.ClientRelation = sliClientRelation.ToSelectList(client.ClientRelation);

            var sliBankAccountCode = new List<SelectListItem>();
            sliBankAccountCode.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "BankInformation").OrderBy(x => x.Value).ToList().ForEach(x =>
            {
                sliBankAccountCode.AddItemValText(x.Value, x.Value + " - " + x.Text);
            });
            ViewBag.BankAccountCode = sliBankAccountCode.ToSelectList(client.BankAccountCode);

            var type = Request.Params["type"];
            if (client.Type == "Personal")
            {
                return View(client);
            }
            return View("EditCompany", client);
            //    return View(client);
        }

        // POST: Client/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ClientId,Type,ContactPerson,ShortName,FullName,PrefixClientTitle,EndfixClientTitle,BirthDate,BirthPlace,MaritalStatus,Sex,Email,EmailAddress1,EmailAddress2,MobilePhone1,MObilePhone2,MobilePhone3,ClientRelation,RelateTo,BankAccountName,BankAccountNumber,BankAccountCode,Status,CreatedBy,UpdatedBy,CreatedDate,UpdatedDate,IsActive,IdNumber,BranchCode,Address")] Client client)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var clientOrigin = db.Client.Find(client.ClientId);
                    db.Entry(clientOrigin).State = EntityState.Detached;
                    if (clientOrigin.Member.Count() > 0 || clientOrigin.Client1.Count() > 0 || clientOrigin.Client11.Count > 0)
                    {
                        WarningMessagesAdd("Editing Not Allowed");
                    }
                    if (WarningMessages().Count == 0)
                    {
                        client.SetPropertyUpdate();
                        db.Entry(client).State = EntityState.Modified;
                        db.SaveChanges();
                        SuccessMessagesAdd("Editing Data Success");
                        return RedirectToAction("Index");
                    }

                }
                catch (Exception e)
                {
                    WarningMessagesAdd(e.MessageToList());
                    var errors = ModelState.Where(x => x.Value.Errors.Count > 0);
                    errors.ToList().ForEach(x =>
                    {
                        WarningMessagesAdd(x.Value.Errors.FirstOrDefault().ErrorMessage);

                    });
                }

            }
            ViewBag.ContactPerson = new SelectList(db.Client, "ClientId", "Type", client.ContactPerson);
            ViewBag.RelateTo = new SelectList(db.Client, "ClientId", "Type", client.RelateTo);
            var sliType = new List<SelectListItem>();
            sliType.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Type").ToList().ForEach(x =>
            {
                sliType.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Type = sliType.ToSelectList();

            var sliSex = new List<SelectListItem>();
            sliSex.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Sex").OrderBy(x => x.Value).ToList().ForEach(x =>
            {
                sliSex.AddItemValText(x.Value, x.Text);
            });
            ViewBag.Sex = sliSex.ToSelectList(client.Sex);

            var sliMaritalStatus = new List<SelectListItem>();
            sliMaritalStatus.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Marital Status").OrderBy(x => x.Value).ToList().ForEach(x =>
            {
                sliMaritalStatus.AddItemValText(x.Value, x.Text);
            });
            ViewBag.MaritalStatus = sliMaritalStatus.ToSelectList(client.MaritalStatus);

            var sliClientRelation = new List<SelectListItem>();
            sliClientRelation.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "Client Relation ").ToList().ForEach(x =>
            {
                sliClientRelation.AddItemValText(x.Value, x.Text);
            });
            ViewBag.ClientRelation = sliClientRelation.ToSelectList();

            var sliBankAccountCode = new List<SelectListItem>();
            sliBankAccountCode.AddBlank();
            db.CommonListValue.Where(x => x.CommonListValue2.Value == "BankInformation").OrderBy(x => x.Value).ToList().ForEach(x =>
            {
                sliBankAccountCode.AddItemValText(x.Value, x.Value + " - " + x.Text);
            });
            ViewBag.BankAccountCode = sliBankAccountCode.ToSelectList(client.BankAccountCode);
            if (client.Type == "Personal")
            {

                return View("Edit", client);
            }
            else
            {

                return View("EditCompany", client);
            }

        }

        // GET: Client/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Client.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            if (client.Type == "Personal")
            {
                return View(client);
            }
            return View("DeleteCompany", client);
        }

        // POST: Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                Client client = db.Client.Find(id);
                db.Client.Remove(client);
                db.SaveChanges();
                SuccessMessagesAdd("Delate Success!");
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

            db.Client.ToList().ForEach(x =>
            {
                ds.Add(new
                {
                    //Agent = (x.Agent != null ? x.Agent : ""),
                    //PolicyId = x.PolicyId != null ? x.PolicyId : "",
                    //PolicyNumber = x.PolicyNumber != null ? x.PolicyNumber : "",
                    //StartDate = x.StartDate.Value,
                    //TerminateDate = x.TerminateDate != null ? x.TerminateDate.Value : DateTime.Now,
                    x.ClientId,
                    FullName = (x.FullName != null ? x.FullName : ""),
                    BirthDate = x.BirthDate.Value,
                    IdNumber = (x.IdNumber != null ? x.IdNumber : ""),
                    Email = (x.Email != null ? x.Email : "")

                });

            });
            //data.ToList().ForEach(x => {

            //    object newObject = new object();
            //    newObject = x;
            //    ds.Add(newObject);

            //});
            rd.Load(Path.Combine(Server.MapPath("~/Reports/DetailsClient.rpt")));

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
