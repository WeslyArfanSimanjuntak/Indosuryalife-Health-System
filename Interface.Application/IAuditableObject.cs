using System;

namespace Interface.Application
{
    public interface IAuditableObject
    {
        string CreatedBy { get; set; }
        string UpdatedBy { get; set; }
        Nullable<System.DateTime> CreatedDate { get; set; }
        Nullable<System.DateTime> UpdatedDate { get; set; }
        Nullable<short> IsActive { get; set; }
    }
}
