using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class Staff : BaseEntity
    {
        public string Email { get; set; }
        public string PhoneExt { get; set; }
        public Guid? OrgGroupId { get; set; }
        public Guid? MissionOrgId { get; set; }
        public int? AnnualSalary { get; set; }
        public string RenwebId { get; set; }
        public string MoeLicenseNumber { get; set; }
        public DateTime? ContractIssued { get; set; }
        public DateTime? ContractExpireDate { get; set; }
        public string InsuranceNumber { get; set; }
        public string TeacherLicenseOrg { get; set; }
        public string TeacherLicenseNo { get; set; }
        public string VisaType { get; set; }
        public string WorkPermitType { get; set; }

        public string Endorsements { get; set; }
        public string EndorsementAgency { get; set; }
    }

    [Table("Staff", IsColumnAttributeRequired = false)]
    public class StaffWithName : Staff
    {
        [Column(SkipOnInsert = true, SkipOnUpdate = true, IsColumn = false)]
        public string PreferredName { get; set; }

        [Column(SkipOnInsert = true, SkipOnUpdate = true, IsColumn = false)]
        public string LastName { get; set; }

        [Column(SkipOnInsert = true, SkipOnUpdate = true, IsColumn = false)]
        public Guid PersonId { get; set; }
    }

    [Table("Staff", IsColumnAttributeRequired = false)]
    public class StaffWithOrgName : Staff
    {
        [Column(SkipOnInsert = true, SkipOnUpdate = true, IsColumn = false)]
        public string MissionOrgName { get; set; }

        [Column(SkipOnInsert = true, SkipOnUpdate = true, IsColumn = false)]
        public string MissionOrgEmail { get; set; }
    }
}