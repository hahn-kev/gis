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


        public IQueryable<PersonWithDaysOfLeave> PeopleWithDaysOfLeave(Guid? limitByPersonId = null) =>
            from person in _dbConnection.GetTable<PersonWithDaysOfLeave>()
            from vacationLeave in LeaveRequestAggrigateByType(LeaveType.Vacation)
                .Having(holder => holder.PersonId == person.Id)
            from sickLeave in LeaveRequestAggrigateByType(LeaveType.Sick)
                .Having(holder => holder.PersonId == person.Id)
            where limitByPersonId == null || person.Id == limitByPersonId
            select new PersonWithDaysOfLeave
            {
                Id = person.Id,
                Email = person.Email,
                FirstName = person.FirstName,
                LastName = person.LastName,
                StaffId = person.StaffId,
                SickDaysOfLeaveUsed = sickLeave.LeaveUsed ?? 0,
                VacationDaysOfLeaveUsed = vacationLeave.LeaveUsed ?? 0
            };

        class AggHolder
        {
            public Guid PersonId { get; set; }
            public int? LeaveUsed { get; set; }
        }

        private IQueryable<AggHolder> LeaveRequestAggrigateByType(LeaveType leaveType)
        {
            return _dbConnection.LeaveRequests.Where(request =>
                    request.StartDate.InSchoolYear(DateTime.Now.SchoolYear()) && request.Type == Sql.ToSql(leaveType))
                .GroupBy(request => request.PersonId,
                    (personId, requests) => new AggHolder
                    {
                        PersonId = personId,
                        LeaveUsed = requests.Sum(request => DataExtensions.DayDiff(request.StartDate, request.EndDate))
                    })
                .DefaultIfEmpty();
        }

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
                StaffId = person.StaffId,
                Country = person.Country
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