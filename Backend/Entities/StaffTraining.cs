using System;

namespace Backend.Entities
{
    public class StaffTraining : BaseEntity
    {
        public Guid StaffId { get; set; }
        public Guid TrainingRequirementId { get; set; }
        public DateTime? CompletedDate { get; set; }
    }

    public class StaffTrainingWithRequirement : StaffTraining
    {
        public string RequirementName { get; set; }
        public TrainingScope RequirementScope { get; set; }
    }
}