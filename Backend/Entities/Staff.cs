using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class Staff : BaseEntity
    {
        public Guid OrgGroupId { get; set; }
    }

    [Table("Staff")]
    public class StaffWithName : Staff
    {
        public static StaffWithName Build(StaffWithName staff, string preferredName, Guid personId)
        {
            staff.PreferredName = preferredName;
            staff.PersonId = personId;
            return staff;
        }
        public string PreferredName { get; set; }
        public Guid PersonId { get; set; }
    }
}