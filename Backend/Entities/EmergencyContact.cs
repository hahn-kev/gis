using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class EmergencyContact : BaseEntity
    {
        public Guid PersonId { get; set; }
        public Guid ContactId { get; set; }
        public int Order { get; set; }
    }

    public class EmergencyContactExtended : EmergencyContact
    {
        [Column(IsColumn = false, SkipOnUpdate = true, SkipOnInsert = true)]
        public string ContactPreferedName { get; set; }
    }
}