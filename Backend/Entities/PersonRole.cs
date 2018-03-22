using System;
using LinqToDB;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Backend.Entities
{
    public class PersonRole : BaseEntity
    {
        public Guid JobId { get; set; }
        public Guid PersonId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Active { get; set; }

        public TimeSpan LengthOfService()
        {
            return (Active
                       ? DateTime.Now
                       : EndDate ??
                         throw new NullReferenceException(
                             "End date null for inactive role"))
                   - StartDate;
        }
    }

    public class PersonRoleExtended : PersonRole
    {
        public string PreferredName { get; set; }
        public string LastName { get; set; }
    }

    public class PersonRoleWithJob : PersonRoleExtended
    {
        public Job Job { get; set; }
    }
}