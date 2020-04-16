using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.MainApplication.ReportModel
{
    public class BasicProductReportModel
    {
        public long Id { get; set; }
        public string BpInsurerId { get; set; }
        public string BasicProductId { get; set; }
        public string BasicProductName { get; set; }
        public string BasicProductDesc { get; set; }
        public string CurrencyId { get; set; }
        public string ProductType { get; set; }
        public string FclId { get; set; }
        public string RefundId { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}