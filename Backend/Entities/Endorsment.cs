using System;

namespace Backend.Entities
{
    public class Endorsment : BaseEntity
    {
        public string Name { get; set; }
        public string Agency { get; set; }
    }

    public class StaffEndorsment : BaseEntity
    {
        public Guid PersonId { get; set; }
        public Guid EndorsmentId { get; set; }
        public DateTime? EndorsmentDate { get; set; }
    }

    public class StaffEndorsmentWithName : StaffEndorsment
    {
        public string EndorsmentName { get; set; }
        public string EndorsmentAgency { get; set; }
    }

    public class RequiredEndorsment : BaseEntity
    {
        public Guid JobId { get; set; }
        public Guid EndorsmentId { get; set; }
    }

    public class RequiredEndorsmentWithName : RequiredEndorsment
    {
        public string EndorsmentName { get; set; }
        public string EndorsmentAgency { get; set; }
    }
}