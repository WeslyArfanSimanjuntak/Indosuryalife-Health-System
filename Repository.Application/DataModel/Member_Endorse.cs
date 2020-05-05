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
    
    public partial class Member_Endorse
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Member_Endorse()
        {
            this.AdministrationFeeEndorsement = new HashSet<AdministrationFeeEndorsement>();
            this.MemberClientEndorse = new HashSet<MemberClientEndorse>();
            this.MemberPlan_Endorse = new HashSet<MemberPlan_Endorse>();
            this.PCF_Endorse = new HashSet<PCF_Endorse>();
        }
    
        public string EndorseNumber { get; set; }
        public string PolicyId { get; set; }
        public long MemberId { get; set; }
        public long MemberOriginId { get; set; }
        public string MemberNumber { get; set; }
        public string ClientId { get; set; }
        public string CardNumber { get; set; }
        public string PlanId { get; set; }
        public Nullable<int> SequencialNo { get; set; }
        public Nullable<short> Age { get; set; }
        public Nullable<System.DateTime> EntryDate { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<System.DateTime> LastEndorseDate { get; set; }
        public Nullable<System.DateTime> LastClaimDate { get; set; }
        public string ClaimNumber { get; set; }
        public Nullable<System.DateTime> ExitDate { get; set; }
        public Nullable<System.DateTime> TerminateDate { get; set; }
        public Nullable<System.DateTime> IssueDate { get; set; }
        public Nullable<System.DateTime> ProcessDate { get; set; }
        public string MemberStatus { get; set; }
        public Nullable<short> AdmedikaCode { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<short> IsActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AdministrationFeeEndorsement> AdministrationFeeEndorsement { get; set; }
        public virtual Client Client { get; set; }
        public virtual Endorsement Endorsement { get; set; }
        public virtual Member Member { get; set; }
        public virtual Policy Policy { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MemberClientEndorse> MemberClientEndorse { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MemberPlan_Endorse> MemberPlan_Endorse { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PCF_Endorse> PCF_Endorse { get; set; }
    }
}
