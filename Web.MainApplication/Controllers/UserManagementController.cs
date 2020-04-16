 using Repository.Application.DataModel;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Web.MainApplication.Controllers
{
    public class UserManagementController : BaseController
    {
        private DBEntities db = new DBEntities();
        // GET: UserManagement
        public ActionResult Index()
        {
            return View();
        }
        //#region User
        //[HttpPost]
        //public ActionResult GetData()
        //{
        //    JsonResult result = new JsonResult();

        //    try
        //    {
        //        // Initialization.
        //        string search = Request.Form.GetValues("search[value]")[0];
        //        string draw = Request.Form.GetValues("draw")[0];
        //        string order = Request.Form.GetValues("order[0][column]")[0];
        //        string orderDir = Request.Form.GetValues("order[0][dir]")[0];
        //        int startRec = Convert.ToInt32(Request.Form.GetValues("start")[0]);
        //        int pageSize = Convert.ToInt32(Request.Form.GetValues("length")[0]);

        //        // Loading.
        //        List<ASPNETUSERS> data = new List<ASPNETUSERS>();

        //        // Total record count.
        //        int totalRecords = 0;

        //        // Verification.
        //        if (!string.IsNullOrEmpty(search) &&
        //            !string.IsNullOrWhiteSpace(search))
        //        {
        //            data = this.unitOfWork.AspNetUserRepository.AspNetUsersDataTableSource(startRec, pageSize, order, orderDir, out totalRecords, search);
        //        }
        //        else
        //        {
        //            data = this.unitOfWork.AspNetUserRepository.AspNetUsersDataTableSource(startRec, pageSize, order, orderDir, out totalRecords);
        //        }
        //        // Sorting.


        //        data.ForEach(x =>
        //        {
        //            x.ASPNETROLESUSERS = null;
        //        });

        //        data = data.Select(x => new ASPNETUSERS
        //        {
        //            FULLNAME = x.FULLNAME,
        //            USERNAME = x.USERNAME,
        //            EMAIL = x.EMAIL,
        //            PHONENUMBER = x.PHONENUMBER,
        //            USERTYPEINTERNALEXTERNAL = x.USERTYPEINTERNALEXTERNAL,
        //            LASTPASSWORDCHANGE = x.LASTPASSWORDCHANGE,
        //            CreateId = x.CreateId,
        //            ModifyId = x.ModifyId,
        //            ModifyDate = x.ModifyDate

        //        }).ToList();

        //        result = this.Json(new { draw = Convert.ToInt32(draw), recordsTotal = totalRecords, recordsFiltered = totalRecords, data = data }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Info
        //        Console.Write(ex);
        //    }

        //    // Return info.
        //    return result;
        //}



        // GET: UserManagement
        public ActionResult ASPNETUSERSIndex()
        {
            return View(db.AspNetUsers.ToList());
        }
        //public ActionResult ASPNETUSERSDetails(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    AspNetUsers aSPNETUSERS = db.AspNetUsers.Find(id);
        //    if (aSPNETUSERS == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    var roles = db.asp.Where(x => x.USERNAMEUSER == aSPNETUSERS.USERNAME).Select(x => new SelectListItem
        //    {
        //        Text = x.ASPNETROLES.ASPNETROLES2 != null ? x.ASPNETROLES.ASPNETROLES2.NAME + " - " + x.ASPNETROLES.NAME : x.ASPNETROLES.NAME

        //    }).OrderBy(x => x.Text).ToList();
        //    ViewBag.Roles = roles;
        //    return View(aSPNETUSERS);
        //}


        public ActionResult ASPNETUSERSCreate()
        {
            return View();
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ASPNETUSERSCreate([Bind(Include = "USERNAME,EMAIL,FULLNAME,PASSWORDHASH,PHONENUMBER,USERTYPEINTERNALEXTERNAL,LASTPASSWORDCHANGE,UPDATEDBYUSER,CREATEDBYUSER,CREATEDDATEUSER,UPDATEDDATEUSER")] AspNetUsers aSPNETUSERS)
        //{
        //    List<string> errorList = new List<string>();
        //    if (db.ASPNETUSERS.Find(aSPNETUSERS.USERNAME) != null)
        //    {
        //        errorList.Add("Username Already Exist.");
        //    }
        //    if (string.IsNullOrWhiteSpace(aSPNETUSERS.USERNAME))
        //    {
        //        errorList.Add("Username Can Not Be Blank.");
        //    }
        //    if (!HtmlValidationHelper.IsEmailFormat(aSPNETUSERS.EMAIL))
        //    {
        //        errorList.Add("Email Format Is Not Valid.");
        //    }
        //    if (string.IsNullOrWhiteSpace(aSPNETUSERS.FULLNAME))
        //    {
        //        errorList.Add("Full Name Can Not Be Blank.");
        //    }
        //    if (ModelState.IsValid && errorList.Count == 0)
        //    {

        //        db.ASPNETUSERS.Add(aSPNETUSERS);
        //        db.SaveChanges();
        //        return RedirectToAction("ASPNETUSERSIndex");
        //    }
        //    ViewBag.ErrorMessage = errorList;
        //    return View(aSPNETUSERS);
        //}


        //public ActionResult ASPNETUSERSEdit(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ASPNETUSERS aSPNETUSERS = db.ASPNETUSERS.Find(id);
        //    if (aSPNETUSERS == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    //ViewBag.SubsidiaryId = new SelectList(this.unitOfWork.CompanyRepository.GetForDropDown(null, q => q.OrderBy(x => x.CompanyName)), "CompanyId", "CompanyName", null, aSPNETUSERS.SubsidiaryId, this.unitOfWork.CompanyRepository.GetForDropDown(null, q => q.OrderBy(x => x.CompanyName)).ToList());
        //    //// ViewBag.USERTYPEINTERNALEXTERNAL = new SelectList(this.unitOfWork.CommonFilesRepository.GetForDropDown(x => x.CommonIdParent == "00000000000000000008", z => z.OrderBy(x => x.CmmKeyValue)), "CmmKeyValue", "CmmKeyValue", aSPNETUSERS.USERTYPEINTERNALEXTERNAL);
        //    //ViewBag.PolicyId = new SelectList(this.unitOfWork.PolicyMasterRepository.GetForDropDown(null, x => x.OrderBy(z => z.PolicyId)), "PolicyId", "PolicyId", aSPNETUSERS.PolicyId);
        //    return View("~/Views/System/UserManagement/ASPNETUSERSEdit.cshtml", aSPNETUSERS);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ASPNETUSERSEdit([Bind(Include = "USERNAME,EMAIL,FULLNAME,PASSWORDHASH,PHONENUMBER,USERTYPEINTERNALEXTERNAL,LASTPASSWORDCHANGE,UPDATEDBYUSER,CREATEDBYUSER,CREATEDDATEUSER,UPDATEDDATEUSER, SubsidiaryId,PolicyId")] ASPNETUSERS aSPNETUSERS)
        //{
        //    if (string.IsNullOrWhiteSpace(aSPNETUSERS.USERNAME))
        //    {
        //        WarningMessagesAdd("Username Can Not Be Blank.");
        //    }
        //    if (!HtmlValidationHelper.IsEmailFormat(aSPNETUSERS.EMAIL))
        //    {
        //        WarningMessagesAdd("Email Format Is Not Valid.");
        //    }
        //    if (string.IsNullOrWhiteSpace(aSPNETUSERS.FULLNAME))
        //    {
        //        WarningMessagesAdd("Full Name Can Not Be Blank.");
        //    }
        //    if (string.IsNullOrWhiteSpace(aSPNETUSERS.PHONENUMBER))
        //    {
        //        WarningMessagesAdd("Phonenumber Can Not Be Blank.");
        //    }
        //    if (string.IsNullOrWhiteSpace(aSPNETUSERS.USERTYPEINTERNALEXTERNAL))
        //    {
        //        WarningMessagesAdd("User Type(Domain User/Non-Domain User) Can Not Be Blank.");
        //    }
        //    if (ModelState.IsValid && MessageToViewError.Messages.Count == 0)
        //    {

        //        this.unitOfWork.AspNetUserRepository.Update(aSPNETUSERS);
        //        this.unitOfWork.Save();
        //        return RedirectToAction("ASPNETUSERSIndex");
        //    }
        //    ViewBag.SubsidiaryId = new SelectList(this.unitOfWork.CompanyRepository.GetForDropDown(null, q => q.OrderBy(x => x.CompanyName)), "CompanyId", "CompanyName");
        //    //    ViewBag.USERTYPEINTERNALEXTERNAL = new SelectList(this.unitOfWork.CommonFilesRepository.GetForDropDown(x => x.CommonIdParent == "00000000000000000008", z => z.OrderBy(x => x.CmmKeyValue)), "CmmKeyValue", "CmmKeyValue", aSPNETUSERS.USERTYPEINTERNALEXTERNAL);
        //    ViewBag.PolicyId = new SelectList(this.unitOfWork.PolicyMasterRepository.GetForDropDown(null, x => x.OrderBy(z => z.PolicyId)), "PolicyId", "PolicyId", aSPNETUSERS.PolicyId);
        //    return View("~/Views/System/UserManagement/ASPNETUSERSEdit.cshtml", aSPNETUSERS);
        //}





        //public ActionResult ASPNETUSERSDelete(string id)
        //{
        //    if (id == null)
        //    {
        //        return new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "UnAuthorized" }));
        //    }
        //    ASPNETUSERS aSPNETUSERS = this.unitOfWork.AspNetUserRepository.GetByID(id);
        //    if (aSPNETUSERS == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    var roles = this.unitOfWork.AspNetRolesUsersRepository.Get(x => x.USERNAMEUSER == aSPNETUSERS.USERNAME).Select(x => new SelectListItem
        //    {
        //        Text = x.ASPNETROLES.ASPNETROLES2 != null ? x.ASPNETROLES.ASPNETROLES2.NAME + " - " + x.ASPNETROLES.NAME : x.ASPNETROLES.NAME

        //    }).OrderBy(x => x.Text).ToList();
        //    ViewBag.Roles = roles;
        //    return View("~/Views/System/UserManagement/ASPNETUSERSDelete.cshtml", aSPNETUSERS);
        //}

        //[HttpPost, ActionName("ASPNETUSERSDelete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult ASPNETUSRSDeletePost(string id)
        //{
        //    ASPNETUSERS aSPNETUSERS = this.unitOfWork.AspNetUserRepository.GetByID(id);
        //    this.unitOfWork.AspNetUserRepository.Delete(aSPNETUSERS);
        //    this.unitOfWork.Save();
        //    return RedirectToAction("ASPNETUSERSIndex");
        //}


        [HttpGet]
        public ActionResult ASPNETUSERSChangePassword()
        {
            AspNetUsers aSPNETUSERS = db.AspNetUsers.Find(User.Identity.Username());
            /*this.unitOfWork.AspNetUserRepository.GetByID(UserBancassurance.UserName);*/
            //User.Identity.Username
            //db.AspNetUsers.Find

            if (aSPNETUSERS == null)
            {
                return HttpNotFound();
            }
            return View(aSPNETUSERS);
            //"~/Views/System/UserManagement/ASPNETUSERSChangePassword.cshtml", 
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ASPNETUSERSChangePassword(string USERNAME)
        {

            string currentPassword = Request.Params["CURRENT_PASSWORD"];
            string newPassword = Request.Params["NEW_PASSWORD"];
            string confirmNewPassword = Request.Params["CONFIRM_NEW_PASSWORD"];
            AspNetUsers aspNetUsers = db.AspNetUsers.Find(USERNAME);
            //samaaa
            //var user1 = db.AspNetUsers.Find(USERNAME);
            //var user2 = db.AspNetUsers.Where(x => x.Username == USERNAME).FirstOrDefault();
            //var user3 = db.Database.SqlQuery<AspNetUsers>("select * from aspnetuser where username='" + USERNAME + "'").FirstOrDefault();
            



            if (string.IsNullOrEmpty(currentPassword))
            {
                WarningMessagesAdd("Fill Current Password");
            }
            if (string.IsNullOrEmpty(newPassword))
            {

                WarningMessagesAdd("Fill New Password ");
            }
            if (string.IsNullOrEmpty(confirmNewPassword))
            {

                WarningMessagesAdd("Fill Confirmed New Password");
            }
            if (newPassword != confirmNewPassword)
            {
                WarningMessagesAdd("New Password and Confirmed Password Is Not Match");
            }

            if (Service.System.Encriptor.SHA1(currentPassword) != aspNetUsers.Password)
            {
                WarningMessagesAdd("Current Password is not Match "); 
            }


            //if (USERNAME != UserBancassurance.UserName || USERNAME == null)
            //{
            //    return new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "UnAuthorized", }));
            //}

            if (WarningMessages().Count > 0 || WarningMessages().Count > 0)
            {
                return View(aspNetUsers);
            }
            aspNetUsers.LastPasswordChange = DateTime.Now;
            aspNetUsers.Password = Service.System.Encriptor.SHA1(newPassword);
            db.Entry(aspNetUsers).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            ViewBag.Success = true;
            SuccessMessagesAdd("Change Password Success");
            return RedirectToAction("Index", "Home");
        }


        //#region ASPNETRroles
        //public ActionResult ASPNETROLESIndex()
        //{
        //    var data = this.unitOfWork.AspNetRolesRepository.Get();


        //    return View("~/Views/System/UserManagement/ASPNETROLESIndex.cshtml", data);
        //}


        //// GET: ASPNETROLES/Details/5
        //public ActionResult ASPNETROLESDetails(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "UnAuthorized" }));

        //    }
        //    ASPNETROLES aSPNETROLES = this.unitOfWork.AspNetRolesRepository.GetByID(id);
        //    if (aSPNETROLES == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View("~/Views/System/UserManagement/ASPNETROLESDetails.cshtml", aSPNETROLES);
        //}

        //// GET: ASPNETROLES/Create
        //public ActionResult ASPNETROLESCreate()
        //{
        //    List<SelectListItem> selectListType = new List<SelectListItem>();
        //    selectListType.Add(new SelectListItem { Text = "", Value = "" });
        //    selectListType.Add(new SelectListItem { Text = "Controller", Value = "controller" });
        //    selectListType.Add(new SelectListItem { Text = "Function", Value = "function" });



        //    ViewBag.TYPE = selectListType;

        //    var listRoles = this.unitOfWork.AspNetRolesRepository.Get(x => x.TYPE.ToLower() == "controller").OrderByDescending(x => x.NAME).ToList();
        //    listRoles.Add(new ASPNETROLES { NAME = "", ID = 0 });
        //    listRoles.Reverse();
        //    List<SelectListItem> selectListRoles = new List<SelectListItem>();

        //    listRoles.ForEach(x =>
        //    {
        //        selectListRoles.Add(new SelectListItem { Text = x.NAME, Value = x.ID == 0 ? "" : x.ID.ToString() });

        //    }
        //        );
        //    ViewBag.PARENTIDORCONTROLLER = new SelectList(selectListRoles, "Value", "Text", null, null, listRoles.Where(x => x.NAME == "").ToList());

        //    return View("~/Views/System/UserManagement/ASPNETROLESCreate.cshtml");
        //}

        //// POST: ASPNETROLES/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ASPNETROLESCreate([Bind(Include = "ID,PARENTIDORCONTROLLER,NAME,TYPE,CREATEDBYUSER,UPDATEDBYUSER,CREATEDDATEUSER,UPDATEDDATEUSER")] ASPNETROLES aSPNETROLES)
        //{
        //    if (string.IsNullOrWhiteSpace(aSPNETROLES.NAME))
        //    {
        //        WarningMessagesAdd("Roles Name Can Not Be Blank.");
        //    }
        //    if (!string.IsNullOrWhiteSpace(aSPNETROLES.PARENTIDORCONTROLLER.ToString()) && aSPNETROLES.TYPE == "controller")
        //    {
        //        WarningMessagesAdd("Roles With Type Controller Can Not Has Parent.");
        //    }
        //    if (string.IsNullOrWhiteSpace(aSPNETROLES.PARENTIDORCONTROLLER.ToString()) && aSPNETROLES.TYPE == "function")
        //    {
        //        WarningMessagesAdd("Roles With Type Function Must Has Parent.");
        //    }
        //    var roles = this.unitOfWork.AspNetRolesRepository.Get(x => x.ASPNETROLES2.ID == aSPNETROLES.PARENTIDORCONTROLLER && x.NAME.ToLower() == aSPNETROLES.NAME.ToLower()).ToList();
        //    if (roles.Count > 0)
        //    {
        //        WarningMessagesAdd("Roles Exist ! (" + roles.FirstOrDefault().ASPNETROLES2.NAME + "/" + roles.FirstOrDefault().NAME + ")");
        //    }
        //    if (ModelState.IsValid && MessageToViewError.Messages.Count == 0)
        //    {

        //        this.unitOfWork.AspNetRolesRepository.Insert(aSPNETROLES);
        //        this.unitOfWork.Save();
        //        return RedirectToAction("ASPNETROLESIndex");
        //    }
        //    List<SelectListItem> selectListType = new List<SelectListItem>();
        //    selectListType.Add(new SelectListItem { Text = "", Value = "" });
        //    selectListType.Add(new SelectListItem { Text = "Controller", Value = "controller" });
        //    selectListType.Add(new SelectListItem { Text = "Function", Value = "function" });



        //    ViewBag.TYPE = selectListType;

        //    var listRoles = this.unitOfWork.AspNetRolesRepository.Get(x => x.TYPE.ToLower() == "controller").OrderByDescending(x => x.NAME).ToList();
        //    listRoles.Add(new ASPNETROLES { NAME = "", ID = 0 });
        //    listRoles.Reverse();
        //    List<SelectListItem> selectListRoles = new List<SelectListItem>();

        //    listRoles.ForEach(x =>
        //    {
        //        selectListRoles.Add(new SelectListItem { Text = x.NAME, Value = x.ID == 0 ? "" : x.ID.ToString() });

        //    }
        //        );
        //    ViewBag.PARENTIDORCONTROLLER = new SelectList(selectListRoles, "Value", "Text", null, null, listRoles.Where(x => x.NAME == "").ToList());
        //    return View("~/Views/System/UserManagement/ASPNETROLESCreate.cshtml", aSPNETROLES);
        //}

        //// GET: ASPNETROLES/Edit/5
        //public ActionResult ASPNETROLESEdit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "UnAuthorized" }));

        //    }
        //    ASPNETROLES aSPNETROLES = this.unitOfWork.AspNetRolesRepository.GetByID(id);
        //    if (aSPNETROLES == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    List<SelectListItem> selectListType = new List<SelectListItem>();
        //    selectListType.Add(new SelectListItem { Text = "", Value = "" });
        //    selectListType.Add(new SelectListItem { Text = "Controller", Value = "controller" });
        //    selectListType.Add(new SelectListItem { Text = "Function", Value = "function" });



        //    ViewBag.TYPE = new SelectList(selectListType, "Value", "Text", aSPNETROLES.TYPE);

        //    var listRoles = this.unitOfWork.AspNetRolesRepository.Get(x => x.TYPE.ToLower() == "controller").OrderByDescending(x => x.NAME).ToList();
        //    listRoles.Add(new ASPNETROLES { NAME = "", ID = 0 });
        //    listRoles.Reverse();
        //    List<SelectListItem> selectListRoles = new List<SelectListItem>();

        //    listRoles.ForEach(x =>
        //    {
        //        selectListRoles.Add(new SelectListItem { Text = x.NAME, Value = x.ID == 0 ? "" : x.ID.ToString() });

        //    }
        //        );
        //    ViewBag.PARENTIDORCONTROLLER = new SelectList(selectListRoles, "Value", "Text", aSPNETROLES.PARENTIDORCONTROLLER);
        //    return View("~/Views/System/UserManagement/ASPNETROLESEdit.cshtml", aSPNETROLES);
        //}

        //// POST: ASPNETROLES/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ASPNETROLESEdit([Bind(Include = "ID,PARENTIDORCONTROLLER,NAME,TYPE,CREATEDBYUSER,UPDATEDBYUSER,CREATEDDATEUSER,UPDATEDDATEUSER")] ASPNETROLES aSPNETROLES)
        //{
        //    List<string> errorList = new List<string>();
        //    if (string.IsNullOrWhiteSpace(aSPNETROLES.NAME))
        //    {
        //        errorList.Add("Roles Name Can Not Be Blank.");
        //    }
        //    if (!string.IsNullOrWhiteSpace(aSPNETROLES.PARENTIDORCONTROLLER.ToString()) && aSPNETROLES.TYPE == "controller")
        //    {
        //        errorList.Add("Roles With Type Controller Can Not Has Parent.");
        //    }
        //    if (string.IsNullOrWhiteSpace(aSPNETROLES.PARENTIDORCONTROLLER.ToString()) && aSPNETROLES.TYPE == "function")
        //    {
        //        errorList.Add("Roles With Type Function Must Has Parent.");
        //    }

        //    if (ModelState.IsValid && errorList.Count == 0)
        //    {

        //        this.unitOfWork.AspNetRolesRepository.Update(aSPNETROLES);
        //        this.unitOfWork.Save();
        //        return RedirectToAction("ASPNETROLESIndex");
        //    }
        //    ViewBag.ErrorMessage = errorList;
        //    List<SelectListItem> selectListType = new List<SelectListItem>();
        //    selectListType.Add(new SelectListItem { Text = "", Value = "" });
        //    selectListType.Add(new SelectListItem { Text = "Controller", Value = "controller" });
        //    selectListType.Add(new SelectListItem { Text = "Function", Value = "function" });



        //    ViewBag.TYPE = selectListType;

        //    var listRoles = this.unitOfWork.AspNetRolesRepository.Get(x => x.TYPE.ToLower() == "controller").OrderByDescending(x => x.NAME).ToList();
        //    listRoles.Add(new ASPNETROLES { NAME = "", ID = 0 });
        //    listRoles.Reverse();
        //    List<SelectListItem> selectListRoles = new List<SelectListItem>();

        //    listRoles.ForEach(x =>
        //    {
        //        selectListRoles.Add(new SelectListItem { Text = x.NAME, Value = x.ID == 0 ? "" : x.ID.ToString() });

        //    }
        //        );
        //    ViewBag.PARENTIDORCONTROLLER = new SelectList(selectListRoles, "Value", "Text", null, null, listRoles.Where(x => x.NAME == "").ToList());
        //    return View("~/Views/System/UserManagement/ASPNETROLESEdit.cshtml", aSPNETROLES);
        //}

        //// GET: ASPNETROLES/Delete/5
        //public ActionResult ASPNETROLESDelete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "UnAuthorized" }));

        //    }
        //    ASPNETROLES aSPNETROLES = this.unitOfWork.AspNetRolesRepository.GetByID(id);
        //    if (aSPNETROLES == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View("~/Views/System/UserManagement/ASPNETROLESDelete.cshtml", aSPNETROLES);
        //}

        //// POST: ASPNETROLES/Delete/5
        //[HttpPost, ActionName("ASPNETROLESDelete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult ASPNETROLESDeleteConfirmed(int id)
        //{
        //    ASPNETROLES aSPNETROLES = this.unitOfWork.AspNetRolesRepository.GetByID(id);
        //    this.unitOfWork.AspNetRolesRepository.Delete(aSPNETROLES);
        //    this.unitOfWork.Save();
        //    return RedirectToAction("ASPNETROLESIndex");
        //}


        //#endregion

        //#region ASPNETROLESUSERS
        //// GET: ASPNETROLESUSERS
        //public ActionResult ASPNETROLESUSERSIndex()
        //{
        //    var aSPNETROLESUSERS = this.unitOfWork.AspNetRolesUsersRepository.Get();
        //    return View("~/Views/System/UserManagement/ASPNETROLESUSERSIndex.cshtml", aSPNETROLESUSERS.ToList());
        //}

        //// GET: ASPNETROLESUSERS/Details/5
        //public ActionResult ASPNETROLESUSERSDetails(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "UnAuthorized" }));
        //    }
        //    ASPNETROLESUSERS aSPNETROLESUSERS = this.unitOfWork.AspNetRolesUsersRepository.GetByID(id);
        //    if (aSPNETROLESUSERS == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View("~/Views/System/UserManagement/ASPNETROLESDetails.cshtml", aSPNETROLESUSERS);
        //}

        //// GET: ASPNETROLESUSERS/Create
        //public ActionResult ASPNETROLESUSERSCreate()
        //{

        //    ViewBag.IDROLE = new SelectList(this.unitOfWork.AspNetRolesRepository.Get().Select(x => new ASPNETROLES
        //    {
        //        NAME = x.ASPNETROLES2 != null ? (x.ASPNETROLES2.NAME + " - " + x.NAME) : "" + x.NAME,
        //        ID = x.ID,
        //        TYPE = x.TYPE

        //    }).OrderBy(x => x.NAME).Select(x => new ASPNETROLES
        //    {
        //        NAME = x.TYPE.ToLower() == "controller" ? x.NAME + " (Controller)" : x.NAME + (" (Function)"),
        //        ID = x.ID,

        //    }), "ID", "NAME");
        //    // SelectListItem
        //    //ViewBag.IDROLE = new SelectList(this.unitOfWork.AspNetRolesRepository.Get(), "ID", "NAME");
        //    var listUsers = this.unitOfWork.AspNetUserRepository.Get().Select(x => new SelectListItem
        //    {
        //        Text = x.USERNAME,
        //        Value = x.USERNAME
        //    });
        //    ViewBag.USERNAMEUSER = new SelectList(listUsers, "Value", "Text");
        //    WarningMessagesAdd("Ketika anda menambahkan role dengan tipe controller, anda termasuk memberi akses semua function yang melekat pada controller tersebut.");
        //    return View("~/Views/System/UserManagement/ASPNETROLESUSERSCreate.cshtml");
        //}

        //// POST: ASPNETROLESUSERS/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ASPNETROLESUSERSCreate([Bind(Include = "ID,USERNAMEUSER,IDROLE")] ASPNETROLESUSERS aSPNETROLESUSERS)
        //{
        //    var existUserRoles = this.unitOfWork.AspNetRolesUsersRepository.Get(x => x.IDROLE == aSPNETROLESUSERS.IDROLE && x.USERNAMEUSER == aSPNETROLESUSERS.USERNAMEUSER).ToList();
        //    if (existUserRoles.Count > 0)
        //    {
        //        WarningMessagesAdd("Role Already Assigned Before.");
        //    }

        //    if (ModelState.IsValid && MessageToViewError.Messages.Count == 0)
        //    {
        //        var roles = this.unitOfWork.AspNetRolesRepository.GetByID(aSPNETROLESUSERS.IDROLE);
        //        if (roles != null && roles.TYPE.ToLower() == "controller")
        //        {
        //            foreach (var item in this.unitOfWork.AspNetRolesRepository.Get(x => x.ASPNETROLES2.ID == roles.ID))
        //            {
        //                this.unitOfWork.AspNetRolesUsersRepository.Insert(new ASPNETROLESUSERS
        //                {
        //                    IDROLE = item.ID,
        //                    USERNAMEUSER = aSPNETROLESUSERS.USERNAMEUSER
        //                });

        //            }
        //            this.unitOfWork.Save();
        //            return RedirectToAction("ASPNETROLESUSERSIndex");
        //        }

        //        this.unitOfWork.AspNetRolesUsersRepository.Insert(aSPNETROLESUSERS);
        //        this.unitOfWork.Save();
        //        return RedirectToAction("ASPNETROLESUSERSIndex");
        //    }
        //    ViewBag.IDROLE = new SelectList(this.unitOfWork.AspNetRolesRepository.Get().Select(x => new ASPNETROLES
        //    {
        //        NAME = x.ASPNETROLES2 != null ? (x.ASPNETROLES2.NAME + " - " + x.NAME) : "" + x.NAME,
        //        ID = x.ID,
        //        TYPE = x.TYPE

        //    }).OrderBy(x => x.NAME).Select(x => new ASPNETROLES
        //    {
        //        NAME = x.TYPE.ToLower() == "controller" ? x.NAME + " (Controller)" : x.NAME + (" (Function)"),
        //        ID = x.ID,

        //    }), "ID", "NAME");
        //    // SelectListItem
        //    //ViewBag.IDROLE = new SelectList(this.unitOfWork.AspNetRolesRepository.Get(), "ID", "NAME");
        //    var listUsers = this.unitOfWork.AspNetUserRepository.Get().Select(x => new SelectListItem
        //    {
        //        Text = x.USERNAME,
        //        Value = x.USERNAME
        //    });
        //    ViewBag.USERNAMEUSER = new SelectList(listUsers, "Value", "Text");
        //    return View("~/Views/System/UserManagement/ASPNETROLESUSERSCreate.cshtml", aSPNETROLESUSERS);
        //}

        //// GET: ASPNETROLESUSERS/Edit/5
        //public ActionResult ASPNETROLESUSERSEdit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "UnAuthorized" }));
        //    }
        //    ASPNETROLESUSERS aSPNETROLESUSERS = this.unitOfWork.AspNetRolesUsersRepository.GetByID(id);
        //    if (aSPNETROLESUSERS == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.IDROLE = new SelectList(this.unitOfWork.AspNetRolesUsersRepository.Get(), "ID", "NAME", aSPNETROLESUSERS.IDROLE);
        //    ViewBag.USERNAMEUSER = new SelectList(this.unitOfWork.AspNetUserRepository.Get(), "USERNAME", "EMAIL", aSPNETROLESUSERS.USERNAMEUSER);
        //    return View(aSPNETROLESUSERS);
        //}

        //// POST: ASPNETROLESUSERS/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ASPNETROLESUSERSEdit([Bind(Include = "ID,USERNAMEUSER,IDROLE")] ASPNETROLESUSERS aSPNETROLESUSERS)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        this.unitOfWork.AspNetRolesUsersRepository.Insert(aSPNETROLESUSERS);
        //        this.unitOfWork.Save();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.IDROLE = new SelectList(this.unitOfWork.AspNetRolesRepository.Get(), "ID", "NAME", aSPNETROLESUSERS.IDROLE);
        //    ViewBag.USERNAMEUSER = new SelectList(this.unitOfWork.AspNetUserRepository.Get(), "USERNAME", "EMAIL", aSPNETROLESUSERS.USERNAMEUSER);
        //    return View(aSPNETROLESUSERS);
        //}

        //// GET: ASPNETROLESUSERS/Delete/5
        //public ActionResult ASPNETROLESUSERSDelete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "UnAuthorized" }));
        //    }
        //    ASPNETROLESUSERS aSPNETROLESUSERS = this.unitOfWork.AspNetRolesUsersRepository.GetByID(id);
        //    if (aSPNETROLESUSERS == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View("~/Views/System/UserManagement/ASPNETROLESUSERSDelete.cshtml", aSPNETROLESUSERS);
        //}

        //// POST: ASPNETROLESUSERS/Delete/5
        //[HttpPost, ActionName("ASPNETROLESUSERSDelete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult ASPNETROLESUSERSDeleteConfirmed(int id)
        //{
        //    ASPNETROLESUSERS aSPNETROLESUSERS = this.unitOfWork.AspNetRolesUsersRepository.GetByID(id);
        //    this.unitOfWork.AspNetRolesUsersRepository.Delete(aSPNETROLESUSERS);
        //    this.unitOfWork.Save();
        //    return RedirectToAction("ASPNETROLESUSERSIndex");
        //}
        //#endregion
    }
}