using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Web.MainApplication.Models
{
    public class ModalView
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Footer { get; set; }
        public ModalForm ModalForm { get; set; }
        public int? WidthPercentage { get; set; }
    }
    public class ModalForm
    {
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public object RouteValues { get; set; }
    }
}