//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Repository.Application.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class MemberClientEndorse
    {
        public long MemberClientEndorse1 { get; set; }
        public long MemberId { get; set; }
        public string MemberNumber { get; set; }
        public string ClientId { get; set; }
        public string Type { get; set; }
        public string BranchCode { get; set; }
        public string ContactPerson { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string PrefixClientTitle { get; set; }
        public string EndfixClientTitle { get; set; }
        public Nullable<System.DateTime> BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string IdNumber { get; set; }
        public string MaritalStatus { get; set; }
        public string Sex { get; set; }
        public string Email { get; set; }
        public string EmailAddress1 { get; set; }
        public string EmailAddress2 { get; set; }
        public string Address { get; set; }
        public string MobilePhone1 { get; set; }
        public string MObilePhone2 { get; set; }
        public string MobilePhone3 { get; set; }
        public string ClientRelation { get; set; }
        public string RelateTo { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountCode { get; set; }
        public string BankAccountName { get; set; }
        public Nullable<short> PrintNewCard { get; set; }
        public Nullable<short> LostCard { get; set; }
        public Nullable<short> IsFinancialChange { get; set; }
        public Nullable<short> RecordMode { get; set; }
        public Nullable<short> RecordModeSub { get; set; }
        public Nullable<short> IsPremiumCorrection { get; set; }
        public Nullable<decimal> PrintCardAmount { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<short> IsActive { get; set; }
    
        public virtual Client Client { get; set; }
        public virtual Client Client1 { get; set; }
        public virtual Client Client2 { get; set; }
        public virtual Member_Endorse Member_Endorse { get; set; }
    }
}
