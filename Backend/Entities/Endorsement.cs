using System;

namespace Backend.Entities
{
    public class Endorsement : BaseEntity
    {
        public string Name { get; set; }
        public string Agency { get; set; }
    }

    public class StaffEndorsement : BaseEntity
    {
        public Guid PersonId { get; set; }
        
        public Guid EndorsementId { get; set; }
        public DateTime? EndorsementDate { get; set; }
    }

    public class StaffEndorsementWithName : StaffEndorsement
    {
        public string EndorsementName { get; set; }
        public string EndorsementAgency { get; set; }
    }

    public class RequiredEndorsement : BaseEntity
    {
        public Guid JobId { get; set; }
        public Guid EndorsementId { get; set; }
    }

    public class RequiredEndorsementWithName : RequiredEndorsement
    {
        public string EndorsementName { get; set; }
        public string EndorsementAgency { get; set; }
    }
}