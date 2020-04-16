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
    
    public partial class PCF_Endorse
    {
        public long Id { get; set; }
        public string EndorseNumber { get; set; }
        public string PolicyId { get; set; }
        public long MemberId { get; set; }
        public string BasicProductId { get; set; }
        public string TransType { get; set; }
        public System.DateTime InvoiceDate { get; set; }
        public Nullable<System.DateTime> DueDate { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public string TransactionNumber { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<short> IsActive { get; set; }
    
        public virtual BasicProduct BasicProduct { get; set; }
        public virtual Endorsement Endorsement { get; set; }
        public virtual Member_Endorse Member_Endorse { get; set; }
        public virtual Policy Policy { get; set; }
    }
}
