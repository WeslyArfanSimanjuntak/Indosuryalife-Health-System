using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.MainApplication.ReportModel
{
    public class CLVReportModel
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
        public string Desc { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime IsActive { get; set; }
    }
}