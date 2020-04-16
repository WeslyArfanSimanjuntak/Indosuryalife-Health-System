using Repository.Application.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Web.MainApplication.Models;

namespace Web.MainApplication.Controllers
{
    public class EntityApiController : BaseController
    {
        private DBEntities db = new DBEntities();


        // GET: EntityApi

        public ActionResult GetData()
        {
            string entitas = Request.Params["entitas"];
            string filter = Request.Params["filter"];
            return PopulateData(entitas, filter);

        }
        //public EntityApiController()
        //{
        //    this.db = new DBEntities();
        //    this.db.Configuration.ProxyCreationEnabled = false;
        //}
        //public EntityApiController(DBEntities db)
        //{
        //    this.db = db;
        //    this.db.Configuration.ProxyCreationEnabled = false;
        //}

        [NonAction]
        private JsonResult PopulateData(string entitas, string filter = "true")
        {
            var result = new JsonResult().JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            try
            {
                if (entitas == "AspNetGroups")
                {
                    return this.Json(new
                    {
                        data = db.AspNetGroups.Where(filter).ToList()

                    }, JsonRequestBehavior.AllowGet);

                }
                else if (entitas == "AspNetGroupUser")
                {
                    return this.Json(new
                    {
                        data = db.AspNetGroupUser.Where(filter).ToList()

                    }, JsonRequestBehavior.AllowGet);

                }
                else if (entitas == "AspNetRoleGroup")
                {
                    return this.Json(new
                    {
                        data = db.AspNetRoleGroup.Where(filter).ToList()

                    }, JsonRequestBehavior.AllowGet);

                }
                else if (entitas == "AspNetUsers")
                {
                    return this.Json(new
                    {
                        data = db.AspNetUsers.Where(filter).ToList()

                    }, JsonRequestBehavior.AllowGet);

                }

                else if (entitas == "AspNetRoles")
                {
                    return this.Json(new
                    {
                        data = db.AspNetRoles.Where(filter).ToList()

                    }, JsonRequestBehavior.AllowGet);

                }
                else if (entitas == "Benefit")
                {
                    return this.Json(new
                    {
                        data = db.Benefit.Where(filter).ToList()

                    }, JsonRequestBehavior.AllowGet);

                }
                else if (entitas == "Menu")
                {
                    this.db.Configuration.ProxyCreationEnabled = false;
                    var orderby = Request.Params["orderby"]??"1 descending";

                    return this.Json(new
                    {
                    data = db.Menu.Where(filter).OrderBy(orderby).ToList()

                    }, JsonRequestBehavior.AllowGet);

                }
                else if (entitas == "Client")
                {

                    var retval = db.Client.Where(filter).ToList().Select(x => new Client()
                    {
                        ClientId = x.ClientId,
                        BankAccountCode = x.BankAccountCode,
                        BankAccountName = x.BankAccountName,
                        BankAccountNumber = x.BankAccountNumber,
                        ShortName = x.ShortName,
                        FullName = x.FullName,
                        Email = x.Email,
                        EmailAddress1 = x.EmailAddress1,
                        EmailAddress2 = x.EmailAddress2,
                        MobilePhone1 = x.MobilePhone1,
                        MObilePhone2 = x.MObilePhone2,
                        MobilePhone3 = x.MobilePhone3

                    }).ToList();
                    return this.Json(new
                    {
                        data = retval
                    }, JsonRequestBehavior.AllowGet);

                }
                else if (entitas == "BasicProductLimit")
                {

                    var retval = db.BasicProductLimit.Where(filter).ToList().Select(x => new BasicProductLimit()
                    {
                        BasicProductId = x.BasicProductId,
                        BasicProductLimitCode = x.BasicProductLimitCode

                    }).ToList();
                    return this.Json(new
                    {
                        data = retval
                    }, JsonRequestBehavior.AllowGet);

                }
                else if (entitas == "PlanDetail")
                {

                    var retval = db.PlanDetail.Where(filter).ToList().Select(x => new
                    {
                        BasicProductId = x.BasicProductId,
                        BasicProductName = x.BasicProduct.BasicProductName,
                        BasicProductLimitCode = x.BasicProductLimitCode,
                        PolicyId = x.PolicyId,
                        PlanId = x.PlanId

                    }).ToList();
                    return this.Json(new
                    {
                        data = retval
                    }, JsonRequestBehavior.AllowGet);

                }
                else if (entitas == "BasicProductLimitHdr")
                {

                    var retval = db.BasicProductLimitHdr.Where(filter).ToList().Select(x => new
                    {
                        x.BasicProductId,
                        x.BPLimitId,
                        x.Description

                    }).ToList();
                    return this.Json(new
                    {
                        data = retval
                    }, JsonRequestBehavior.AllowGet);

                }
                else if (entitas == "PremiumRate")
                {

                    var retval = db.PremiumRate.Where(filter).ToList().Select(x => new
                    {
                        x.CoverageDuration,
                        x.PaymentDuration,
                        x.RecordStatus,
                        x.PaymentFrequency,
                        x.RateType,

                    }).ToList();
                    return this.Json(new
                    {
                        data = retval
                    }, JsonRequestBehavior.AllowGet);

                }
                else if (entitas == "Policy")
                {

                    var retval = db.Policy.Where(filter).ToList().Select(x => new
                    {
                        x.PolicyId,
                        x.PolicyNumber,
                        x.Client.FullName,
                        x.StartDate,
                        x.MatureDate,
                        x.LastEndorseDate

                    }).ToList();
                    return this.Json(new
                    {
                        data = retval
                    }, JsonRequestBehavior.AllowGet);

                }
                
                else if (entitas == "PlanDetail_Endorse")
                {

                    var retval = db.PlanDetail_Endorse.Where(filter).ToList().Select(x => new
                    {
                        BasicProductId = x.BasicProductId,
                        BasicProductName = x.BasicProduct.BasicProductName,
                        BasicProductLimitCode = x.BasicProductLimitCode,
                        PolicyId = x.PolicyId,
                        PlanId = x.PlanId

                    }).ToList();
                    return this.Json(new
                    {
                        data = retval
                    }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return this.Json(new
                    {
                        data = "Null",
                        message = "entitas \"" + entitas + "\" not found"

                    }, JsonRequestBehavior.AllowGet);

                }


            }

            catch (Exception ex)
            {
                return this.Json(new
                {
                    data = "Null",
                    message = ex.Message

                }, JsonRequestBehavior.AllowGet);
            }


        }

        [HttpPost]
        public ActionResult Search(string entity, string filter = null, int rowSkip = 0, int rowToTake = 10, string orderByField = "1", string orderByType = "ASC", string draw = "1")
        {

            if (Request.Params["start"] != null)
            {
                rowSkip = Convert.ToInt32(Request.Params["start"]);
            }
            if (Request.Params["length"] != null)
            {
                rowToTake = Convert.ToInt32(Request.Params["length"]);
            }
            if (Request.Params["draw"] != null)
            {
                draw = Request.Params["draw"];
            }
            if (Request.Params["columns[" + Request.Params["order[0][column]"] + "][name]"] != null)
            {
                orderByField = Request.Params["columns[" + Request.Params["order[0][column]"] + "][name]"];
            }
            if (Request.Params["order[0][dir]"] != null)
            {
                orderByType = Request.Params["order[0][dir]"];
            }

            return PopulateDataSearch(entity, filter, rowSkip, rowToTake, orderByField, orderByType, draw);
        }
        [NonAction]
        private JsonResult PopulateDataSearch(string entity, string filter = null, int rowSkip = 0, int rowToTake = 10, string orderByField = null, string orderByType = "ASC", string draw = "1")
        {
            JsonResult result = new JsonResult();

            try
            {
                if (false)
                {

                }
                else
                {
                    return this.Json(new
                    {
                        data = "Null",
                        message = "entitas \"" + entity + "\" not found"

                    }, JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception ex)
            {
                return this.Json(new
                {
                    data = "Null",
                    message = ex.Message

                }, JsonRequestBehavior.AllowGet);
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
    public class EKTPApiController : Controller
    {

        public string CekNIK(string id)
        {


            var cs = new WCFSoapIndosuryaEKTP.EKTPServiceClient();
            var result = cs.StringCekNIK(id);
            cs.Close();
            return result;


        }

    }
}