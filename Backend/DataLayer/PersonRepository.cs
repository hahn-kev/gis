using System;
using System.Linq;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class PersonRepository
    {
        private readonly DbConnection _dbConnection;

        public PersonRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IQueryable<Person> People => _dbConnection.People;

        public IQueryable<PersonWithDaysOfLeave> PeopleWithDaysOfLeave =>
            from person in _dbConnection.GetTable<PersonWithDaysOfLeave>()
            from vacationLeave in _dbConnection.LeaveRequests.LeftJoin(request =>
                request.PersonId == person.Id && request.StartDate.InSchoolYear(Sql.CurrentTimestamp.SchoolYear()) &&
                request.Type == LeaveType.Vacation)
            from sickLeave in _dbConnection.LeaveRequests.LeftJoin(request =>
                request.PersonId == person.Id && request.StartDate.InSchoolYear(Sql.CurrentTimestamp.SchoolYear()) &&
                request.Type == LeaveType.Sick)
            group new
            {
                vacation = DataExtensions.DayDiff(vacationLeave.StartDate, vacationLeave.EndDate),
                sick = DataExtensions.DayDiff(sickLeave.StartDate, sickLeave.EndDate)
            } by person
            into leaveGroup
            select new PersonWithDaysOfLeave
            {
                Id = leaveGroup.Key.Id,
                Email = leaveGroup.Key.Email,
                FirstName = leaveGroup.Key.FirstName,
                LastName = leaveGroup.Key.LastName,
                StaffId = leaveGroup.Key.StaffId,
                SickDaysOfLeaveUsed = leaveGroup.Sum(arg => arg.sick) ?? 0,
                VacationDaysOfLeaveUsed = leaveGroup.Sum(arg => arg.vacation) ?? 0
            };

        public IQueryable<PersonExtended> PeopleExtended => PeopleGeneric<PersonExtended>();

        private IQueryable<T_Person> PeopleGeneric<T_Person>() where T_Person : PersonExtended, new() =>
            from person in _dbConnection.GetTable<T_Person>()
            from staff in _dbConnection.Staff.LeftJoin(staff => staff.Id == person.StaffId).DefaultIfEmpty()
            select new T_Person
            {
                Id = person.Id,
                Email = person.Email,
                FirstName = person.FirstName,
                IsThai = person.IsThai,
                LastName = person.LastName,
                PreferredName = person.PreferredName,
                SpeaksEnglish = person.SpeaksEnglish,
                Staff = staff,
                StaffId = person.StaffId
            };

        public IQueryable<PersonRoleExtended> PersonRolesExtended =>
            from personRole in _dbConnection.PersonRoles
            join person in People on personRole.PersonId equals person.Id
            select new PersonRoleExtended
            {
                Id = personRole.Id,
                PersonId = personRole.PersonId,
                Active = personRole.Active,
                Name = personRole.Name,
                StartDate = personRole.StartDate,
                EndDate = personRole.EndDate,
                FirstName = person.FirstName,
                LastName = person.LastName
            };

        public IQueryable<StaffWithName> StaffWithNames =>
            from staff in _dbConnection.GetTable<Staff>()
            from person in _dbConnection.PeopleExtended.InnerJoin(person => person.StaffId == staff.Id)
            select new StaffWithName
            {
                Id = staff.Id,
                OrgGroupId = staff.OrgGroupId,
                PersonId = person.Id,
                PreferredName = person.PreferredName
            };

        public PersonWithOthers GetById(Guid id)
        {
            var person = PeopleGeneric<PersonWithOthers>().FirstOrDefault(selectedPerson => selectedPerson.Id == id);
            if (person != null)
            {
                person.Roles = _dbConnection.PersonRoles.Where(role => role.PersonId == id).ToList();
            }

            return person;
        }
    }
}