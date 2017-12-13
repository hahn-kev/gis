using System;

namespace Backend.Entities
{
    public class StaffTraining: BaseEntity
    {
        public Guid StaffId { get; set; }
        public Guid TrainingRequirementId { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}