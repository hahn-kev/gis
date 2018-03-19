using System;
using LinqToDB;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Backend.Entities
{
    public class PersonRole : BaseEntity
    {
        public string Name { get; set; }
        public Guid PersonId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Active { get; set; }
        public bool IsDirectorPosition { get; set; }
        public bool IsStaffPosition { get; set; }
        
        [Column(DataType = DataType.VarChar)]
        public RoleType? FullHalfTime { get; set; }

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
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RoleType
    {
        FullTime,
        HalfTime, 
        Contractor,
        DailyWorker,
        SchoolAid
    }
}