using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.MainApplication.ReportModel
{
    public class PlanReportModel
    {
        public string PolicyId { get; set; }
        public string PlanId { get; set; }
        public string PlanName { get; set; }
        public string PlanDesc { get; set; }
        public DateTime StartDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public short IsActive { get; set; }

    }
}