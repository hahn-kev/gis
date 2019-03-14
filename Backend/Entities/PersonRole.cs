using System;
using System.Linq.Expressions;
using Backend.DataLayer;
using LinqToDB;

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

        [ExpressionMethod(nameof(ActiveNowImp))]
        public bool ActiveNow()
        {
            return Active || EndDate > DateTime.Now;
        }

        private static Expression<Func<PersonRole, bool>> ActiveNowImp()
        {
            return role => role.Active || role.EndDate > DateTime.Now;
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