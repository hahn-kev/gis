using System;

namespace Backend.Entities
{
    public class Donation : BaseEntity
    {
        public DateTime Date { get; set; }
        public decimal Money { get; set; }
        public Guid PersonId { get; set; }
        public Guid MissionOrgId { get; set; }
    }
}