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
    
    public partial class Client
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Client()
        {
            this.Client1 = new HashSet<Client>();
            this.Client11 = new HashSet<Client>();
            this.Member_Endorse = new HashSet<Member_Endorse>();
            this.Member_Movement_Client = new HashSet<Member_Movement_Client>();
            this.Member = new HashSet<Member>();
            this.MemberClientEndorse = new HashSet<MemberClientEndorse>();
            this.MemberClientEndorse1 = new HashSet<MemberClientEndorse>();
            this.MemberClientEndorse2 = new HashSet<MemberClientEndorse>();
            this.Policy = new HashSet<Policy>();
            this.Policy_Endorse = new HashSet<Policy_Endorse>();
        }
    
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
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<short> IsActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Client> Client1 { get; set; }
        public virtual Client Client2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Client> Client11 { get; set; }
        public virtual Client Client3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Member_Endorse> Member_Endorse { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Member_Movement_Client> Member_Movement_Client { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Member> Member { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MemberClientEndorse> MemberClientEndorse { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MemberClientEndorse> MemberClientEndorse1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MemberClientEndorse> MemberClientEndorse2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Policy> Policy { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Policy_Endorse> Policy_Endorse { get; set; }
    }
}