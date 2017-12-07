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
        private PersonExtended _supervisorPerson;

        [Column(SkipOnInsert = true, SkipOnUpdate = true)]
        public PersonExtended SupervisorPerson
        {
            get => _supervisorPerson;
            set
            {
                if (value != null && value.Id != Guid.Empty)
                    _supervisorPerson = value;
            }
        }
    }
}