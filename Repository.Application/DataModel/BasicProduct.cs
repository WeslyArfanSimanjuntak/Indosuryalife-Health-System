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
    
    public partial class BasicProduct
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BasicProduct()
        {
            this.BasicProductLimitHdr = new HashSet<BasicProductLimitHdr>();
            this.BasicProductLimit = new HashSet<BasicProductLimit>();
            this.FinanceTransactionDetail = new HashSet<FinanceTransactionDetail>();
            this.MemberPlan_Endorse = new HashSet<MemberPlan_Endorse>();
            this.MemberPlan = new HashSet<MemberPlan>();
            this.PCF = new HashSet<PCF>();
            this.PCF_Endorse = new HashSet<PCF_Endorse>();
            this.PlanDetail_Endorse = new HashSet<PlanDetail_Endorse>();
            this.PlanDetail = new HashSet<PlanDetail>();
        }
    
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
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<short> IsActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BasicProductLimitHdr> BasicProductLimitHdr { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BasicProductLimit> BasicProductLimit { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FinanceTransactionDetail> FinanceTransactionDetail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MemberPlan_Endorse> MemberPlan_Endorse { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MemberPlan> MemberPlan { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PCF> PCF { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PCF_Endorse> PCF_Endorse { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PlanDetail_Endorse> PlanDetail_Endorse { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PlanDetail> PlanDetail { get; set; }
    }
}
