using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class Person
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.Empty;
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [Table("Person", IsColumnAttributeRequired = false)]
    public class PersonExtended : Person
    {
        public bool SpeaksEnglish { get; set; }
        public bool IsThai { get; set; }
    }
}