using System;

namespace Backend.Entities
{
    public class OrgGroup : BaseEntity
    {
        public string GroupName { get; set; }
        public string Type { get; set; }
        public Guid? Supervisor { get; set; }
        public Guid? ParentId { get; set; }
    }
}