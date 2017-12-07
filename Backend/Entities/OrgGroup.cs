using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class OrgGroup : BaseEntity
    {
        public string GroupName { get; set; }
        public string Type { get; set; }
        public Guid? Supervisor { get; set; }
        public Guid? ParentId { get; set; }
        public bool ApproverIsSupervisor { get; set; }
    }

    [Table("OrgGroup", IsColumnAttributeRequired = false)]
    public class OrgGroupWithSupervisor : OrgGroup
    {
        public static OrgGroupWithSupervisor Build(OrgGroupWithSupervisor orgGroup, PersonExtended personExtended)
        {
            if (orgGroup?.Id == Guid.Empty) return null;
            if (orgGroup != null && personExtended?.Id != Guid.Empty)
            {
                orgGroup.SupervisorPerson = personExtended;
            }
            return orgGroup;
        }
        
        [Column(SkipOnInsert = true, SkipOnUpdate = true)]
        public PersonExtended SupervisorPerson { get; set; }
    }
}