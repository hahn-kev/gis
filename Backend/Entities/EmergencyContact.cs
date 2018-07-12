using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class EmergencyContact : BaseEntity
    {
        public Guid PersonId { get; set; }
        public Guid? ContactId { get; set; }
        public int Order { get; set; }
        public string Relationship { get; set; }

        //these are only used when contactId is null
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class EmergencyContactExtended : EmergencyContact
    {
        public string ContactPreferredName { get; set; }
        public string ContactLastName { get; set; }
    }
}