using System;
using Backend.DataLayer;

namespace Backend.Entities
{
    public class PersonRole : BaseEntity
    {
        public Guid JobId { get; set; }
        public Guid PersonId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Active { get; set; }
        public string Notes { get; set; }

        public TimeSpan LengthOfService(int schoolYear)
        {
            return (Active
                       ? schoolYear.EndOfSchoolYear()
                       : EndDate ??
                         throw new NullReferenceException(
                             "End date null for inactive role"))
                   - StartDate;
        }

        public bool ActiveDuringYear(int schoolYear)
        {
            return StartDate.SchoolYear() <= schoolYear &&
                   (Active || (EndDate.HasValue && EndDate.Value.SchoolYear() >= schoolYear));
        }
    }

    public class PersonRoleExtended : PersonRole
    {
        public string PreferredName { get; set; }
        public string LastName { get; set; }
    }

    public class PersonRoleWithJob : PersonRoleExtended
    {
        public JobWithOrgGroup Job { get; set; }
    }
}