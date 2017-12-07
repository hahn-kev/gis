using System;
using System.Collections.Generic;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class Person : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid OrgGroupId { get; set; }
    }

    [Table("Person", IsColumnAttributeRequired = false)]
    public class PersonExtended : Person
    {
        public bool SpeaksEnglish { get; set; }
        public bool IsThai { get; set; }
        public string Email { get; set; }
        public string PreferredName { get; set; }
        public IList<PersonRole> Roles { get; set; }
    }
}