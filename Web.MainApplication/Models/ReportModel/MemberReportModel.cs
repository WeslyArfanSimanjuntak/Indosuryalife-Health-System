using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.MainApplication.ReportModel
{
    public class MemberReportModel
    {

        public string MemberNumber { get; set; }
        public string ClientId { get; set; }
        public string MemberName { get; set; }
        public string PlanId { get; set; }
        public string MembersExpirityDate { get; set; }
        public string MembersEffectiveDate { get; set; }
        public string AdmedikaCode { get; set; }
        public string MemberId { get; set; }
        public string BankAccountName { get; set; }
        public string PolicyNumber { get; set; }
        public string MaritalStatus { get; set; }
        public string MapingID { get; set; }
        public string RecordMode { get; set; }
        public string RecordType { get; set; }
        public string PayorID { get; set; }
        public string CorporateID { get; set; }
        public string TypeOfWork { get; set; }
        public string Address { get; set; }
        public string Relationship { get; set; }
        public string MobilePhone { get; set; }
        public string NRIC { get; set; }
        public string PassportNo { get; set; }
        public string PassportCountry { get; set; }
        public string DateOfBirth { get; set; }
        public string Sex { get; set; }
        public string Remarks { get; set; }
        public string TanggalProses { get; set; }
        public string TanggalProsesSebelumnya { get; set; }
        public string TanggalProsesSebelumnya2 { get; set; }
        public string PremiIP { get; set; }
        public string PremiOP { get; set; }
        public string PremiDE { get; set; }
        public string PremiMA { get; set; }
        public string PremiTL { get; set; }
        public string KoreksiPremi { get; set; }
        public string TotalTagihanPremi { get; set; }
        public string Keterangan { get; set; }
        public string NoPolisInduk { get; set; }
        public string NamaPemegangPolis { get; set; }
        public string RefundPremi { get; set; }
    }
    public class MemberReportModel2
    {

        public string MemberNumber { get; set; }
        public string ClientId { get; set; }
        public string RefundPremi { get; set; }
        public string KoreksiPremi { get; set; }
        public string TotalTagihanPremi { get; set; }
        public string Keterangan { get; set; }
    }
}