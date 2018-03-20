using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class Staff : BaseEntity
    {
        public Guid? OrgGroupId { get; set; }
        public int? AnnualSalary { get; set; }
        public string RenwebId { get; set; }
        public string MoeLicenseNumber { get; set; }
        public bool ContractIssued { get; set; }
        public DateTime? ContractExpireDate { get; set; }
        public string InsuranceNumber { get; set; }
        public string TeacherLicenseOrg { get; set; }
        public string TeacherLicenseNo { get; set; }
    }

    [Table("Staff")]
    public class StaffWithName : Staff
    {
        public string PreferredName { get; set; }
        public Guid PersonId { get; set; }
    }
}