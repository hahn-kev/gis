using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class EmergencyContact : BaseEntity
    {
        public Guid PersonId { get; set; }
        public Guid ContactId { get; set; }
        public int Order { get; set; }
        public string Relationship { get; set; }
    }

    public class EmergencyContactExtended : EmergencyContact
    {
        public string ContactPreferedName { get; set; }
        public string ContactLastName { get; set; }
    }
}