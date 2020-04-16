using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.MainApplication.ReportModel
{
    public class ClientReportModel
    {
        public string ClientId { get; set; }
        public string Type { get; set; }
        public string BranchCode { get; set; }
        public string ContactPerson { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string PrefixClientTitle { get; set; }
        public string EndfixClientTitle { get; set; }
        public DateTime BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string IdNumber { get; set; }
        public string MaritalStatus { get; set; }
        public string Sex { get; set; }
        public string Email { get; set; }
        public string EmailAddress1 { get; set; }
        public string EmailAddress2 { get; set; }
        public string MobilePhone1 { get; set; }
        public string MObilePhone2 { get; set; }
        public string MobilePhone3 { get; set; }
        public string ClientRelation { get; set; }
        public string RelateTo { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountCode { get; set; }
        public string BankAccountName { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime IsActive { get; set; }
    }
}