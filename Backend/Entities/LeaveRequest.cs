using System;

namespace Backend.Entities
{
    public class LeaveRequest : BaseEntity
    {
        public Guid PersonId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool? Approved { get; set; }
        public Guid ApprovedById { get; set; }
    }
}