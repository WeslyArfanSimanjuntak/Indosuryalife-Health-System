using System;
using Interface.Application;

namespace Repository.Application.DataModel
{
    public partial class AuditableObject : IAuditableObject
    {
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public short? IsActive { get; set; }
    }

    public partial class AspNetGroups : IAuditableObject
    {
    }

    public partial class AspNetGroupUser : IAuditableObject
    {
    }

    public partial class AspNetRoleGroup : IAuditableObject
    {
    }

    public partial class AspNetRoles : IAuditableObject
    {
    }
    public partial class AspNetUsers : IAuditableObject
    {
    }
    public partial class Menu : IAuditableObject
    {
    }

    public partial class Client : IAuditableObject
    {
    }


    public partial class CommonListValue : IAuditableObject
    {
    }


    public partial class Member : IAuditableObject
    {
    }
    public partial class BasicProduct : IAuditableObject
    {
    }
    public partial class Policy : IAuditableObject
    {
    }
    public partial class Plan : IAuditableObject
    {
    }
    public partial class PlanDetail : IAuditableObject
    {
    }
    public partial class BasicProductLimit : IAuditableObject
    {
    }
    public partial class Benefit : IAuditableObject
    {
    }
    public partial class PremiumRate : IAuditableObject
    {
    }
    public partial class MemberPlan : IAuditableObject
    {
    }
    public partial class BasicProductLimitHdr : IAuditableObject
    {
    }
    public partial class PCF : IAuditableObject
    {
    }
    public partial class PremiumRateDetails : IAuditableObject
    {
    }
    public partial class PremiumRate : IAuditableObject
    {
    }
    public partial class AspSequential : IAuditableObject
    {
    }
    public partial class Member_Movement : IAuditableObject
    {
    }
    public partial class MemberPlan_H : IAuditableObject
    {
    }
    public partial class Member_Movement_Client : IAuditableObject
    {
    }
    public partial class Endorsement : IAuditableObject
    {
    }
    public partial class MemberPlan_Endorse : IAuditableObject
    {
    }
    public partial class Member_Endorse : IAuditableObject
    {
    }
    public partial class PCF_Endorse : IAuditableObject
    {
    }
    public partial class PlanDetail_Endorse : IAuditableObject
    {
    }
    public partial class Plan_Endorse : IAuditableObject
    {
    }
    public partial class Policy_Endorse : IAuditableObject
    {
    }
    public partial class MemberClientEndorse : IAuditableObject
    {
    }
}