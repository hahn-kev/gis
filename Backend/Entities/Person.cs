using System;
using System.Collections.Generic;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public class Person : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid? StaffId { get; set; }
        public string Email { get; set; }

        public override string ToString()
        {
            return $"{nameof(FirstName)}: {FirstName}, {nameof(LastName)}: {LastName}";
        }
    }

    [Table("Person", IsColumnAttributeRequired = false)]
    public class PersonExtended : Person
    {
        public bool SpeaksEnglish { get; set; }
        public bool IsThai { get; set; }
        public string Country { get; set; }

        private string _preferredName;

        public string PreferredName
        {
            get { return _preferredName ?? FirstName + " " + LastName; }
            set { _preferredName = value; }
        }

        public Staff Staff { get; set; }
    }

    [Table("Person", IsColumnAttributeRequired = false)]
    public class PersonWithOthers : PersonExtended
    {
        public IList<PersonRole> Roles { get; set; }
    }

    [Table("Person", IsColumnAttributeRequired = false)]
    public class PersonWithDaysOfLeave : Person
    {
        public int SickDaysOfLeaveUsed { get; set; }
        public int VacationDaysOfLeaveUsed { get; set; }
    }
}