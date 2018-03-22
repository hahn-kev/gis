using System;

namespace Backend.Entities
{
    public class MissionOrg : BaseEntity
    {
        public string Name { get; set; }
        public Guid RepId { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool OfficeInThailand { get; set; }
    }
}