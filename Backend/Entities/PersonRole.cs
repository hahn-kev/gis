using System;

namespace Backend.Entities
{
    public class PersonRole : BaseEntity
    {
        public string Name { get; set; }
        public Guid PersonId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Active { get; set; }
    }
}