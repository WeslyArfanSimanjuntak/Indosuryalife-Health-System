﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DBEntities : DbContext
    {
        public DBEntities()
            : base("name=DBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AspNetGroups> AspNetGroups { get; set; }
        public virtual DbSet<AspNetGroupUser> AspNetGroupUser { get; set; }
        public virtual DbSet<AspNetRoleGroup> AspNetRoleGroup { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<AspSequential> AspSequential { get; set; }
        public virtual DbSet<BasicProduct> BasicProduct { get; set; }
        public virtual DbSet<BasicProductLimit> BasicProductLimit { get; set; }
        public virtual DbSet<BasicProductLimitHdr> BasicProductLimitHdr { get; set; }
        public virtual DbSet<Benefit> Benefit { get; set; }
        public virtual DbSet<Client> Client { get; set; }
        public virtual DbSet<CommonListValue> CommonListValue { get; set; }
        public virtual DbSet<Endorsement> Endorsement { get; set; }
        public virtual DbSet<FinanceTransaction> FinanceTransaction { get; set; }
        public virtual DbSet<FinanceTransactionDetail> FinanceTransactionDetail { get; set; }
        public virtual DbSet<Member> Member { get; set; }
        public virtual DbSet<Member_Endorse> Member_Endorse { get; set; }
        public virtual DbSet<Member_Movement> Member_Movement { get; set; }
        public virtual DbSet<Member_Movement_Client> Member_Movement_Client { get; set; }
        public virtual DbSet<MemberClientEndorse> MemberClientEndorse { get; set; }
        public virtual DbSet<MemberPlan> MemberPlan { get; set; }
        public virtual DbSet<MemberPlan_Endorse> MemberPlan_Endorse { get; set; }
        public virtual DbSet<MemberPlan_H> MemberPlan_H { get; set; }
        public virtual DbSet<Menu> Menu { get; set; }
        public virtual DbSet<PCF> PCF { get; set; }
        public virtual DbSet<PCF_Endorse> PCF_Endorse { get; set; }
        public virtual DbSet<Plan> Plan { get; set; }
        public virtual DbSet<Plan_Endorse> Plan_Endorse { get; set; }
        public virtual DbSet<PlanDetail> PlanDetail { get; set; }
        public virtual DbSet<PlanDetail_Endorse> PlanDetail_Endorse { get; set; }
        public virtual DbSet<Policy> Policy { get; set; }
        public virtual DbSet<Policy_Endorse> Policy_Endorse { get; set; }
        public virtual DbSet<PremiumRate> PremiumRate { get; set; }
        public virtual DbSet<PremiumRateDetails> PremiumRateDetails { get; set; }
    }
}
