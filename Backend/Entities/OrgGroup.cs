using System;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Backend.Entities
{
    public class OrgGroup : BaseEntity
    {
        public string GroupName { get; set; }
        public GroupType Type { get; set; }
        public Guid? Supervisor { get; set; }
        public Guid? ParentId { get; set; }
        public bool ApproverIsSupervisor { get; set; }
    }

    [Table("OrgGroup", IsColumnAttributeRequired = false)]
    public class OrgGroupWithSupervisor : OrgGroup
    {
        [Column(SkipOnInsert = true, SkipOnUpdate = true)]
        public PersonWithStaff SupervisorPerson { get; set; }
    }
    
    

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GroupType
    {
        [MapValue(nameof(Division))]
        Division,
        [MapValue(nameof(Department))]
        Department,
        [MapValue(nameof(Supervisor))]
        Supervisor
    }
}