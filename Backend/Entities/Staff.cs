using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class Staff : BaseEntity
    {
        public Guid OrgGroupId { get; set; }
        public int? AnnualSalary { get; set; }
        
    }

    [Table("Staff")]
    public class StaffWithName : Staff
    {
        public string PreferredName { get; set; }
        public Guid PersonId { get; set; }
    }
}