using Interface.Application;
using PdfSharp.Pdf;
using Repository.Application.DataModel;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Web.MainApplication.Controllers;
using Web.MainApplication.Resources;
using Web.MainApplication.WebUtility;

namespace System.Web.Mvc
{
    public static class WebAppUtility
    {
        public static string RenderRazorViewToString(this Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
        public static void CopyPages(PdfDocument from, PdfDocument to)
        {
            for (int i = 0; i < from.PageCount; i++)
            {
                to.AddPage(from.Pages[i]);
            }
        }
        public static void CopyPropertiesAuditLogTo<T, TU>(T source, TU dest)
        {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    if (p.CanWrite)
                    { // check if the property can be set or no.
                        var originalValue = p.GetValue(dest, null);
                        p.SetValue(dest, sourceProp.GetValue(source, null) != null ? sourceProp.GetValue(source, null) : originalValue, null);
                    }
                }

            }

        }
        public static string LoginUserFullName
        {
            get
            {

                return ((ClaimsIdentity)HttpContext.Current.User.Identity).FindAll(ClaimTypes.GivenName).Select(x => x.Value).FirstOrDefault();
            }
        }

        public static string LoginUserName
        {
            get
            {
                return ((ClaimsIdentity)HttpContext.Current.User.Identity).FindAll(ClaimTypes.Name).Select(x => x.Value).FirstOrDefault();
            }
        }

        public static string Username(this IIdentity identity)
        {

            var claim = ((ClaimsIdentity)identity).FindFirst(ClaimTypes.Name);
            return claim != null ? claim.Value : "";
        }
        public static string FullName(this IIdentity identity)
        {

            var claim = ((ClaimsIdentity)identity).FindFirst(WebClaimIdentity.FullNameType);
            return claim != null ? claim.Value : "";
        }
        public static string MenuString(this IIdentity identity)
        {

            var claim = ((ClaimsIdentity)identity).FindFirst(WebClaimIdentity.MenuString);
            return claim != null ? claim.Value : "";
        }
        public static string Email(this IIdentity identity)
        {

            var claim = ((ClaimsIdentity)identity).FindFirst(ClaimTypes.Email);
            return claim != null ? claim.Value : "";
        }

        public static string ApplicationVersion(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst(WebClaimIdentity.ApplicationVersion);
            return claim != null ? claim.Value : "";
        }
        public static List<string> Roles(this IIdentity identity)
        {

            var claim = ((ClaimsIdentity)identity).FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();
            return claim;
        }


        public static SelectList CreateSelectList(int max, int min = 1, bool giveEmptyList = true, bool orderByAsc = true, string concatText = "")

        {
            List<SelectListItem> returnValue = new List<SelectListItem>();

            if (giveEmptyList)
            {
                returnValue.Add(new SelectListItem
                {
                    Text = "",
                    Value = ""
                });
                for (int counter = min; counter <= max; counter++)
                {
                    returnValue.Add(new SelectListItem
                    {
                        Text = counter.ToString() + concatText,
                        Value = counter.ToString()
                    });
                }
            }
            else
            {
                for (int counter = min; counter <= max; counter++)
                {
                    returnValue.Add(new SelectListItem
                    {
                        Text = counter.ToString() + concatText,
                        Value = counter.ToString()
                    });
                }
            }
            if (orderByAsc == false)
            {
                returnValue.OrderByDescending(x => x.Value);
                return new SelectList(returnValue, "Value", "Text");

            }
            return new SelectList(returnValue, "Value", "Text");
        }
        public static SelectList CreateSelectList(int max, object selectedValue, int min = 1, bool giveEmptyList = true, bool orderByAsc = true, string concatText = "")

        {
            List<SelectListItem> returnValue = new List<SelectListItem>();

            if (giveEmptyList)
            {
                returnValue.Add(new SelectListItem
                {
                    Text = "",
                    Value = ""
                });
                for (int counter = min; counter <= max; counter++)
                {
                    returnValue.Add(new SelectListItem
                    {
                        Text = counter.ToString() + concatText,
                        Value = counter.ToString()
                    });
                }
            }
            else
            {
                for (int counter = min; counter <= max; counter++)
                {
                    returnValue.Add(new SelectListItem
                    {
                        Text = counter.ToString() + concatText,
                        Value = counter.ToString()
                    });
                }
            }
            if (orderByAsc == false)
            {
                returnValue.OrderByDescending(x => x.Value);
                return new SelectList(returnValue, "Value", "Text");

            }
            return new SelectList(returnValue, "Value", "Text", selectedValue);
        }
        public static void AddBlank(this List<SelectListItem> x)
        {
            x.Reverse();
            x.Add(new SelectListItem()
            {
                Value = "",
                Text = "---"
            });
            x.Reverse();

        }
        public static void AddBlank(this SelectList x)
        {
            x.Reverse();
            var temp = x;
            var tempToList = temp.ToList();

            tempToList.Add(new SelectListItem()
            {
                Value = "",
                Text = "---"
            });
            tempToList.Reverse();
            x = tempToList.ToSelectList();

        }
        public static SelectList CreateSelectList(this SelectList x, object selectedValue)
        {

            return x.ToList().ToSelectList(selectedValue);

        }
        public static SelectList CreateSelectList(this SelectList x, object selectedValue, IEnumerable disabledValue)
        {
            x.Reverse();
            var temp = x;
            var tempToList = temp.ToList();

            tempToList.Add(new SelectListItem()
            {
                Value = "",
                Text = "---"
            });
            tempToList.Reverse();
            x = tempToList.ToSelectList(selectedValue, disabledValue);
            return tempToList.ToSelectList(selectedValue, disabledValue);

        }
        public static void AddItemValText(this List<SelectListItem> x, string val, string text, bool selected = false, bool disabled = false)
        {
            x.Add(new SelectListItem()
            {
                Value = val,
                Text = text,
                Disabled = disabled,
                Selected = selected
            });
        }
        public static SelectList ToSelectList(this List<SelectListItem> x, bool orderByText = true)
        {
            if (orderByText)
            {
                x = x.OrderBy(z => z.Text).ToList();
            }

            SelectList selectList = new SelectList(x, "Value", "Text");

            return selectList;
        }
        public static SelectList ToSelectList(this List<SelectListItem> x, object selectedValue, bool orderByText = true)
        {
            if (orderByText)
            {
                x = x.OrderBy(z => z.Text).ToList();
            }
            SelectList selectList = new SelectList(x, "Value", "Text", selectedValue);

            return selectList;
        }
        public static SelectList ToSelectList(this List<SelectListItem> x, object selectedValue, IEnumerable disabledValues, bool orderByText = true)
        {
            if (orderByText)
            {
                x = x.OrderBy(z => z.Text).ToList();
            }
            SelectList selectList = new SelectList(x, "Value", "Text", selectedValue, disabledValues);

            return selectList;
        }
        public static SelectList SelectListIsActive(object selectedValue = null)
        {
            List<SelectListItem> sli = new List<SelectListItem>();
            sli.AddBlank();
            sli.AddItemValText("1", "Active");
            sli.AddItemValText("0", "Non-Active");
            if (selectedValue != null)
            {
                return sli.ToSelectList(selectedValue);
            }
            return sli.ToSelectList();
        }
        public static SelectList SelectListOpenOrClose(object selectedValue = null)
        {
            List<SelectListItem> sli = new List<SelectListItem>();
            sli.AddBlank();
            sli.AddItemValText("1", "Open");
            sli.AddItemValText("0", "Close");
            if (selectedValue != null)
            {
                return sli.ToSelectList(selectedValue);
            }
            return sli.ToSelectList();
        }
        public static string RitaseNumberGenerator(double ritaseNumber)
        {
            return "RTS-" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "-" + ritaseNumber.ToString().PadLeft(7, '0');
        }
        public static string TransactionProductAdjustmentNumberGenerator(double counter)
        {
            return "AJT-" + DateTime.Now.ToShortDateString() + "-" + counter.ToString().PadLeft(7, '0');
        }
        public static string TransactionProductConvertionNumberGenerator(double counter)
        {
            return "CVT-" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "-" + counter.ToString().PadLeft(7, '0');
        }
        public static string TransactionProductMixingNumberGenerator(double counter)
        {
            return "MXG-" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "-" + counter.ToString().PadLeft(7, '0');
        }
        public static string TransactionProductNumberGenerator(double transactionIdMax)
        {
            return "TRS-" + DateTime.Now.ToShortDateString() + "-" + transactionIdMax.ToString().PadLeft(7, '0');
        }
        public static string DeliveryRequestNumberGenerator(double drMax)
        {
            return "DLR-" + DateTime.Now.ToShortDateString() + "-" + drMax.ToString().PadLeft(7, '0');
        }
        public static string ContractIdGenerator(double contractNumber)
        {
            return "CTR-" + DateTime.Now.ToShortDateString() + "-" + contractNumber.ToString().PadLeft(7, '0');
        }
        public static string FinanceTransactionNumberGenerator(double counter)
        {
            return "FTR-" + DateTime.Now.ToShortDateString() + "-" + counter.ToString().PadLeft(7, '0');
        }

        public static string DeliveryOrderNumberGenerator(double counter)
        {
            return "DVO-" + DateTime.Now.ToShortDateString() + "-" + counter.ToString().PadLeft(7, '0');
        }
        public static string DeliveryOrderInvoiceNumberGenerator(double counter)
        {
            return "DVOI-" + DateTime.Now.ToShortDateString() + "-" + counter.ToString().PadLeft(7, '0');
        }
        public static void SetPropertyUpdate(this IAuditableObject entity)
        {
            var type = entity.GetType();
            var name = type.Module.ScopeName == "EntityProxyModule" ? type.BaseType.Name : type.Name;
            using (var db = new DBEntities())
            {
                string sb = "SELECT COLUMN_NAME " +
                            " FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE" +
                            " WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1" +
                            " AND TABLE_NAME = '" + name + "' AND TABLE_SCHEMA = 'dbo'";

                var listPrimaryKey = db.Database.SqlQuery<string>(sb).ToList();
                string filter = "";
                int counter = 0;
                foreach (var item in listPrimaryKey)
                {
                    counter++;
                    var value = type.GetProperty(item).GetValue(entity, null);
                    var dataType = type.GetProperty(item).PropertyType;
                    if (dataType == typeof(DateTime?) || dataType == typeof(DateTime))
                    {
                        var valueDateTime = Convert.ToDateTime(value);
                        var dateTime = "convert(datetime, '" + valueDateTime.Month + "-" + valueDateTime.Day + "-" + valueDateTime.Year + " " + valueDateTime.Hour + ":" + valueDateTime.Minute + ":" + valueDateTime.Second + "." + valueDateTime.Millisecond + "', 121)";
                        filter = filter + item + " = " + dateTime + "" + (counter == listPrimaryKey.Count() ? "" : " and ");
                    }
                    else
                    {
                        filter = filter + item + " = '" + value + "'" + (counter == listPrimaryKey.Count() ? "" : " and ");
                    }
                }
                string sql = "SELECT CreatedBy, CreatedDate from " + name +
                               "  WITH (NOLOCK) where " + filter;

                var auditAble = db.Database.SqlQuery<AuditableObject>(sql).FirstOrDefault();
                if (auditAble != null)
                {
                    entity.CreatedBy = auditAble.CreatedBy;
                    entity.CreatedDate = auditAble.CreatedDate;
                }
                //var cratedDate = premiumRate.GetType().GetProperty("CreatedDate").GetValue(entity, null);
                //var temp = premiumRate as IAuditableObject;
            }

            entity.UpdatedBy = LoginUserName;
            entity.UpdatedDate = DateTime.Now;
        }
        public static void SetPropertyCreate(this IAuditableObject entity, short isActive = 1)
        {
            entity.CreatedBy = LoginUserName;
            entity.CreatedDate = DateTime.Now;
            entity.IsActive = isActive;
        }
        //public static void SetMatureDate (this IAuditableObject entity, DateTime matureDate = new DateTime(2999, 12, 31))
        //{
        //    entity.MatureDate = matureDate;
        //}

        // Action Link Extention
        public static string AutocompleteJS(this HtmlHelper htmlHelper, string actionName, string controllerName, string entitasName, string htmlControlId)
        {
            //    var user = ((BaseController)htmlHelper.ViewContext.Controller).User;
            //    string currentController = htmlHelper.ViewContext.RouteData.Values["controller"].ToString().ToLower();
            //    if (user.IsInRole(currentController.ToLower() + "/" + actionName.ToLower()))
            //    {
            //        return htmlHelper.ActionLink(linkText, actionName);
            //    }

            string sb = "$(\"#" + htmlControlId + "\").autocomplete({" +
   "            source: function(request, response) {" +
   "            $.ajax({" +
   "                url: '" + controllerName + "\"" + actionName +
   "                dataType: \"json\"," +
   "                data:" +
   "                        {" +
   "                        filter: $(\"#" + htmlControlId + "\").val()," +
   "                    entitas: \"" + entitasName + "\"" +
   "                }," +
   "                success: function(data) {" +
   "                            response($.map(data, function(item) {" +
   "                                return { label: item.Name, value: item.Name };" +
   "                            }));" +
   "                        }," +
   "                error: function(xhr, status, error) {" +
   "                            alert(\"Error\");" +
   "                        }" +
   "                    });" +
   "                }" +
   "            });";


            return sb;
        }
        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName)
        {
            var user = ((BaseController)htmlHelper.ViewContext.Controller).User;
            string currentController = htmlHelper.ViewContext.RouteData.Values["controller"].ToString().ToLower();
            if (user.IsInRole(currentController.ToLower() + "/" + actionName.ToLower()))
            {
                return htmlHelper.ActionLink(linkText, actionName);
            }
            return null;
        }
        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, object routeValues)
        {
            var user = ((BaseController)htmlHelper.ViewContext.Controller).User;
            string currentController = htmlHelper.ViewContext.RouteData.Values["controller"].ToString().ToLower();
            if (user.IsInRole(currentController.ToLower() + "/" + actionName.ToLower()))
            {
                return htmlHelper.ActionLink(linkText, actionName, routeValues);
            }
            return null;
        }
        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, object routeValues, object htmlAttributes)
        {
            var user = ((BaseController)htmlHelper.ViewContext.Controller).User;
            string currentController = htmlHelper.ViewContext.RouteData.Values["controller"].ToString().ToLower();
            if (user.IsInRole(currentController.ToLower() + "/" + actionName.ToLower()))
            {
                return htmlHelper.ActionLink(linkText, actionName, routeValues, htmlAttributes);
            }
            return null;
        }
        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, RouteValueDictionary routeValues)
        {
            var user = ((BaseController)htmlHelper.ViewContext.Controller).User;
            string currentController = htmlHelper.ViewContext.RouteData.Values["controller"].ToString().ToLower();
            if (user.IsInRole(currentController.ToLower() + "/" + actionName.ToLower()))
            {
                return htmlHelper.ActionLink(linkText, linkText, actionName, routeValues);
            }
            return null;
        }
        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            var user = ((BaseController)htmlHelper.ViewContext.Controller).User;
            string currentController = htmlHelper.ViewContext.RouteData.Values["controller"].ToString().ToLower();
            if (user.IsInRole(currentController.ToLower() + "/" + actionName.ToLower()))
            {
                return htmlHelper.ActionLink(linkText, actionName, routeValues, htmlAttributes);
            }
            return null;
        }
        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName)
        {
            var user = ((BaseController)htmlHelper.ViewContext.Controller).User;
            if (user.IsInRole(controllerName.ToLower() + "/" + actionName.ToLower()))
            {
                return htmlHelper.ActionLink(linkText, actionName, controllerName);
            }
            return null;
        }
        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes)
        {
            var user = ((BaseController)htmlHelper.ViewContext.Controller).User;
            if (user.IsInRole(controllerName.ToLower() + "/" + actionName.ToLower()))
            {
                return htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes);
            }
            return null;
        }
        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            var user = ((BaseController)htmlHelper.ViewContext.Controller).User;
            if (user.IsInRole(controllerName.ToLower() + "/" + actionName.ToLower()))
            {
                return htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes);
            }
            return null;
        }
        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes)
        {
            var user = ((BaseController)htmlHelper.ViewContext.Controller).User;
            if (user.IsInRole(controllerName.ToLower() + "/" + actionName.ToLower()))
            {
                return htmlHelper.ActionLink(linkText, actionName, controllerName, protocol, hostName, fragment, routeValues, htmlAttributes);
            }
            return null;
        }
        public static MvcHtmlString ActionLinkAuthorized(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            var user = ((BaseController)htmlHelper.ViewContext.Controller).User;
            if (user.IsInRole(controllerName.ToLower() + "/" + actionName.ToLower()))
            {
                return htmlHelper.ActionLink(linkText, actionName, controllerName, protocol, hostName, fragment, routeValues, htmlAttributes);
            }
            return null;
        }
        public static AjaxOptions AjaxOptionsDefault(this HtmlHelper htmlHelper)
        {
            var retval = new AjaxOptions()
            {
                OnBegin = "ShowSpinner();",
                OnSuccess = "HideSpinner();",
                OnComplete = "$('#modalLg').modal('show');",
                UpdateTargetId = "modalLgParent",
                HttpMethod = "GET",
                InsertionMode = InsertionMode.Replace

            };
            return retval;
        }

        public static List<string> MessageToList(this Exception e)
        {
            var retval = new List<string>();
            if (e.Message != null)
            {
                retval.Add(e.Message);
            }
            if (e.GetType().Name == "DbEntityValidationException")
            {
                var ex = (Data.Entity.Validation.DbEntityValidationException)e;
                ex.EntityValidationErrors.ToList().ForEach(x =>
                {
                    x.ValidationErrors.ToList().ForEach(z =>
                    {
                        retval.Add(z.ErrorMessage);
                    });
                });
            }
            if (e.InnerException != null)
            {
                retval.AddRange(e.InnerException.MessageToList());
            }
            return retval;

        }

        public static List<string> ListErrors(this ModelStateDictionary modelState)
        {
            var errors = modelState.Where(x => x.Value.Errors.Count > 0);
            List<string> retval = new List<string>();
            errors.ToList().ForEach(x =>
            {
                retval.Add(x.Value.Errors.FirstOrDefault().ErrorMessage);

            });
            return retval;
        }

        // dbcontex extention
        //public static long DeliveryOrderSequence(this DBEntities db)
        //{
        //    var sequence = db.TableSequence.Where(x =>x.Id =="DeliveryRequest" && x.Year == DateTime.Now.Year).FirstOrDefault().LastSequenceNumber;
        //    return sequence;

        //}
        //public static float ParamSetupTonToM3(this DBEntities db)
        //{
        //    float retval;
        //    float.TryParse(db.ParameterSetup.Where(x => x.Name == "Convertion(ton to m3)").FirstOrDefault().Value, out retval);
        //    return retval;
        //}
        //public static long DeliveryOrderInvoiceSequence(this DBEntities db)
        //{
        //    var sequence = db.TableSequence.Where(x => x.Id== "DeliveryOrderInvoice" && x.Year == DateTime.Now.Year).FirstOrDefault().LastSequenceNumber;
        //    return sequence;

        //}
        //public static long ProductMixingSequence(this DBEntities db)
        //{
        //    var sequence = db.TableSequence.Where(x => x.Id== "ProductMixing" && x.Year == DateTime.Now.Year).FirstOrDefault().LastSequenceNumber;
        //    return sequence;

        //}
        //public static long RitaseSequence(this DBEntities db)
        //{
        //    var sequence = db.TableSequence.Where(x => x.Id == "Ritase" && x.Year == DateTime.Now.Year).FirstOrDefault().LastSequenceNumber;
        //    return sequence;

        //}
        //public static void RitaseSequencePlusOne(this DBEntities db)
        //{
        //    var sequence = db.TableSequence.Where(x => x.Id == "Ritase" && x.Year == DateTime.Now.Year).FirstOrDefault();
        //    sequence.LastSequenceNumber++;
        //    db.Entry(sequence).State = Data.Entity.EntityState.Modified;
        //}

        public static MvcHtmlString RawActionLink(this AjaxHelper ajaxHelper, string rawHtml, string action, string controller, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes)
        {
            //string anchor = ajaxHelper.ActionLink("##holder##", action, controller, routeValues, ajaxOptions, htmlAttributes).ToString();
            //return MvcHtmlString.Create(anchor.Replace("##holder##", rawHtml));
            /* Updated code to use a generated guid as from the suggestion of Phillip Haydon */
            string holder = Guid.NewGuid().ToString();
            string anchor = ajaxHelper.ActionLink(holder, action, controller, routeValues, ajaxOptions, htmlAttributes).ToString();
            return MvcHtmlString.Create(anchor.Replace(holder, rawHtml));
        }

        public static MvcHtmlString RawActionLink(this HtmlHelper htmlHelper, string rawHtml, string action, string controller, object routeValues, object htmlAttributes)
        {
            //string anchor = htmlHelper.ActionLink("##holder##", action, controller, routeValues, htmlAttributes).ToString();
            //return MvcHtmlString.Create(anchor.Replace("##holder##", rawHtml));
            /* Updated code to use a generated guid as from the suggestion of Phillip Haydon */
            string holder = Guid.NewGuid().ToString();
            string anchor = htmlHelper.ActionLink(holder, action, controller, routeValues, htmlAttributes).ToString();
            return MvcHtmlString.Create(anchor.Replace(holder, rawHtml));
        }
        public static HtmlHelper GetHtmlHelper(this Controller controller)
        {
            var viewContext = new ViewContext(controller.ControllerContext, new FakeView(), controller.ViewData, controller.TempData, TextWriter.Null);
            return new HtmlHelper(viewContext, new ViewPage());
        }

        public static string Terbilang(Decimal d)
        {
            string[] satuan = new string[10] { "nol", "satu", "dua", "tiga", "empat", "lima", "enam", "tujuh", "delapan", "sembilan" };
            string[] belasan = new string[10] { "sepuluh", "sebelas", "dua belas", "tiga belas", "empat belas", "lima belas", "enam belas", "tujuh belas", "delapan belas", "sembilan belas" };
            string[] puluhan = new string[10] { "", "", "dua puluh", "tiga puluh", "empat puluh", "lima puluh", "enam puluh", "tujuh puluh", "delapan puluh", "sembilan puluh" };
            string[] ribuan = new string[5] { "", "ribu", "juta", "milyar", "triliyun" };

            string strHasil = "koma ";
            Decimal frac = d - Decimal.Truncate(d);

            if (Decimal.Compare(frac, 0.0m) != 0)
            {
                string aftercomma = frac.ToString().Replace(',', '.').Split('.').LastOrDefault();

                bool locked = false;
                foreach (var item in aftercomma.Reverse())
                {
                    if (item.ToString() == "0" && !locked)
                    {
                        aftercomma = aftercomma.Remove(aftercomma.Count() - 1);
                    }
                    else
                    {
                        locked = true;
                    }

                }

                int counter = 0;
                foreach (var item in aftercomma)
                {
                    strHasil = strHasil + satuan[Convert.ToInt32(item.ToString())] + " ";
                    counter++;
                }
            }
            else
                strHasil = "";
            int xDigit = 0;
            int xPosisi = 0;

            string strTemp = Decimal.Truncate(d).ToString();
            for (int i = strTemp.Length; i > 0; i--)
            {
                string tmpx = "";
                xDigit = Convert.ToInt32(strTemp.Substring(i - 1, 1));
                xPosisi = (strTemp.Length - i) + 1;
                switch (xPosisi % 3)
                {
                    case 1:
                        bool allNull = false;
                        if (i == 1)
                            tmpx = satuan[xDigit] + " ";
                        else if (strTemp.Substring(i - 2, 1) == "1")
                            tmpx = belasan[xDigit] + " ";
                        else if (xDigit > 0)
                            tmpx = satuan[xDigit] + " ";
                        else
                        {
                            allNull = true;
                            if (i > 1)
                                if (strTemp.Substring(i - 2, 1) != "0")
                                    allNull = false;
                            if (i > 2)
                                if (strTemp.Substring(i - 3, 1) != "0")
                                    allNull = false;
                            tmpx = "";
                        }

                        if ((!allNull) && (xPosisi > 1))
                            if ((strTemp.Length == 4) && (strTemp.Substring(0, 1) == "1"))
                                tmpx = "se" + ribuan[(int)Decimal.Round(xPosisi / 3m)] + " ";
                            else
                                tmpx = tmpx + ribuan[(int)Decimal.Round(xPosisi / 3)] + " ";
                        strHasil = tmpx + strHasil;
                        break;
                    case 2:
                        if (xDigit > 0)
                            strHasil = puluhan[xDigit] + " " + strHasil;
                        break;
                    case 0:
                        if (xDigit > 0)
                            if (xDigit == 1)
                                strHasil = "seratus " + strHasil;
                            else
                                strHasil = satuan[xDigit] + " ratus " + strHasil;
                        break;
                }
            }
            strHasil = strHasil.Trim().ToLower();
            if (strHasil.Length > 0)
            {
                strHasil = strHasil.Substring(0, 1).ToUpper() +
                  strHasil.Substring(1, strHasil.Length - 1);
            }
            return strHasil;
        }

        public static string ToStringIndonesia(this DateTime datetime, string format)
        {
            string separator = "";
            if (format.Contains(" "))
            {
                separator = " ";
            }
            else if (format.Contains("-"))
            {
                separator = "-";
            }
            else if (format.Contains("/"))
            {
                separator = "/";
            }
            var segments = format.Split(Convert.ToChar(separator));

            var nameOfMonths = new string[12] { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "November", "Desember" };
            var selectedMonth = nameOfMonths[datetime.Month - 1];
            string returnValue = "";
            foreach (var item in segments)
            {
                if (item.ToLower().Contains("y"))
                {
                    returnValue = returnValue + datetime.ToString(item) + separator;
                }
                else if (item.ToLower().Contains("m"))
                {
                    if (item.Count() == 4)
                    {
                        returnValue = returnValue + selectedMonth + separator;
                    }
                    else if (item.Count() == 2)
                    {
                        returnValue = returnValue + selectedMonth.Substring(0, 3) + separator;
                    }
                }
                else if (item.ToLower().Contains("d"))
                {
                    returnValue = returnValue + datetime.Day + separator;
                }
            }
            returnValue = returnValue.TrimEnd(Convert.ToChar(separator));
            return returnValue;
        }
        public static double Age(DateTime? birthdate)
        {
            var retval = Math.Round((DateTime.Now - birthdate).Value.TotalDays / 365.25, 0);
            return retval;
        }
        public static bool IsAdult(DateTime? birthdate)
        {
            if (Age(birthdate) > 22)
            {
                return true;
            }
            return false;
        }
    }
    public static class WebViewExtention
    {


        public static List<string> SuccessMessages(this WebViewPage source)
        {
            var allMessage = (List<string>)source.TempData["MessageSuccess1402"];
            if (allMessage == null)
            {
                allMessage = new List<string>();
            }
            return allMessage;
        }

        public static List<string> InfoMessages(this WebViewPage source)
        {
            var allMessage = (List<string>)source.TempData["MessageInfo1402"];
            if (allMessage == null)
            {
                allMessage = new List<string>();
            }

            return allMessage;
        }
        public static List<string> WarningMessages(this WebViewPage source)
        {
            var allMessage = (List<string>)source.TempData["MessageWarning1402"];
            if (allMessage == null)
            {
                allMessage = new List<string>();
            }

            return allMessage;
        }


        public static List<string> ErrorMessages(this WebViewPage source)
        {
            var allMessage = (List<string>)source.TempData["MessageError1402"];
            if (allMessage == null)
            {
                allMessage = new List<string>();
            }
            return allMessage;

        }
        public static SqlParameter SqlParamterForOutputMessage(this SqlParameter sqlParameter)
        {
            sqlParameter.ParameterName = "message";
            sqlParameter.SqlDbType = SqlDbType.VarChar;
            sqlParameter.Size = 512;
            sqlParameter.Direction = ParameterDirection.Output;
            return sqlParameter;
        }
        public static List<string> MessageToList(this SqlParameter sqlParameter)
        {
            var stringMessage = sqlParameter.Value != DBNull.Value ? (string)sqlParameter.Value : null;
            if (stringMessage != null && !string.IsNullOrEmpty(stringMessage))
            {
                return stringMessage.Split(';').ToList();
            }
            return null;
        }


    }
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName);

            ModelState modelState = new ModelState { Value = valueResult };

            object actualValue = null;

            if (valueResult.AttemptedValue != string.Empty)
            {
                try
                {
                    actualValue = Convert.ToDecimal(valueResult.AttemptedValue, CultureInfo.CurrentCulture);
                }
                catch (FormatException e)
                {
                    modelState.Errors.Add(e);
                }
            }

            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);

            return actualValue;
        }
    }
    public class DateTimeModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName);

            ModelState modelState = new ModelState { Value = valueResult };

            object actualValue = null;

            if (valueResult.AttemptedValue != string.Empty)
            {
                var type = valueResult.GetType();
                try
                {
                    var newDate = DateTime.Parse(valueResult.AttemptedValue, new CultureInfo(SystemResources.CultureInfoNameOfTransformator));
                    actualValue = newDate;
                }
                catch (FormatException e)
                {
                    modelState.Errors.Add(e);
                }
            }

            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);

            return actualValue;
        }
    }
    public class EntityEnum
    {

        public const string CONTRACT = "CONTRACT";
        public const string ContractProduct = "ContractProduct";
        public const string DeliveryOrder = "DeliveryOrder";
        public const string DeliveryOrderInvoice = "DeliveryOrderInvoice";
        public const string DeliveryRequest = "DeliveryRequest";

    }
    public class FinanceTransactionRecordMode
    {
        public const int _1 = 1;
        public const int _3 = 3;
        public const int _8 = 8;

    }
    public static class Message
    {
        public const string CreateSuccess = "Data Created Succesfully";
        public const string CreateFail = "Data Failed To Create";

        public const string UpdateSuccess = "Data Updated Succesfully";
        public const string UpdateFail = "Data Failed To Update";

        public const string ProcessSuccess = "Processing Data Succes";
        public const string ProcessFail = "Processing Data Failed";

        public const string DeleteSuccess = "Data Deleted Succesfully";
        public const string DeleteFail = "Data Failed To Delete";


        public const string DeleteIsNotAllowed = "Data Is Not Allowed To Be Deleted";
        public const string UpdateIsNotAllowed = "Data Is Not Allowed To Be Updated";


    }
    public static class EndorseType
    {
        public const string MovePlan = "MovePlan";
        public const string Additional = "Additional";
        public const string TerminateMember = "TerminateMember";
        public const string Mutation = "Mutation";
        public const string TransitionData = "TransitionData";
        public const string Renewal = "Renewal";
        public const string Reactivate = "Reactivate";


    }
    public static class RecordModeMemberMovement
    {
        public const int Additional = 1;
        public const int MovePlan = 12;
        public const int TerminateMember = 3;
    }
    public static class TerminateType
    {
        public const string Cancel = "Cancel";
        public const string Refund = "Refund";
        public const string Death = "Death";
    }
    public static class EndorseStatus
    {
        public const string Done = "Done";
        public const string New = "New";

    }
    public static class TransactionType
    {
        public const string Refund = "R";
        public const string Premium = "P";

    }
    public static class MemberStatus
    {
        public const string Calculated = "Calculated";
        public const string Active = "Active";
        public const string New = "New";
        public const string Endorse = "Endorse";
        public const string Cancel = "Cancel";
        public const string Refund = "Refund";
        public const string TerminateCancel = "Terminate Cancel";
        public const string TerminateRefund = "Terminate Refund";
        public const string TerminateDeath = "Terminate Death";

    }

    public static class AspSequentialName
    {
        public const string TransactionNumber = "TransactionNumber";
        public const string ClientId = "ClientId";
        public const string PolicyId = "PolicyId";
        public const string MemberNumber = "MemberNumber";
        public const string PolicyNumber = "PolicyNumber";
        public const string EndorseNumber = "EndorseNumber";


    }
    public static class ClientRelation
    {
        public const string Wife = "Wife";
        public const string Husband = "Husband";
        public const string Daughter = "Daughter";
        public const string Son = "Son";
    }

    public static class Sex
    {
        public const string Female = "Female";
        public const string Male = "Male";
    }
    public static class PolicyStatus
    {
        public const string Active = "1";
        public const string Endorse = "Endorse";


    }
    public static class MaritalStatus
    {
        public const string Married = "Married";
        public const string Single = "Single";
    }
    public static class PaymentFrequency
    {
        public const string Yearly = "Y";
        public const string Semesterly = "S";
        public const string Quarterly = "Q";
        public const string Monthly = "M";
    }
    public static class CommonListValueConst
    {
        public const string PrintCardFee = "Print Card Fee";
    }
    public static class RecordMode
    {
        public const int RenewalWithoutCard = 5;
        public const int RenewalWithNewCard = 6;
        public const int DeathMember = 3;
        public const int DeleteMember = 3;
        public const int UpdateSex = 2;
    }
    public class VersionApplication
    {
        public const string LastVersionApplication = "LastVersionApplication";
        public const string ApplicationVersionsSequence = "Application Versions Sequence";

    }
    public class FakeView : IView
    {
        public void Render(ViewContext viewContext, TextWriter writer)
        {
            throw new InvalidOperationException();
        }
    }

}
