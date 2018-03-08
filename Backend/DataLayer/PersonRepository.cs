using System;
using System.Linq;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class PersonRepository
    {
        private readonly IDbConnection _dbConnection;

        public PersonRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IQueryable<Person> People => _dbConnection.People;

        public IQueryable<LeaveRequest> LeaveRequests => _dbConnection.LeaveRequests;

        public IQueryable<PersonExtended> PeopleExtended => _dbConnection.PeopleExtended;
        public IQueryable<PersonWithStaff> PeopleWithStaff => PeopleGeneric<PersonWithStaff>();

        private IQueryable<T_Person> PeopleGeneric<T_Person>() where T_Person : PersonWithStaff, new() =>
            from person in _dbConnection.PeopleExtended
            from spouse in _dbConnection.People.LeftJoin(person1 => person1.Id == person.SpouseId).DefaultIfEmpty()
            from staff in _dbConnection.Staff.LeftJoin(staff => staff.Id == person.StaffId).DefaultIfEmpty()
            where !person.Deleted
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
                PhoneNumber = person.PhoneNumber,
                SpouseId = person.SpouseId,
                SpousePreferedName = spouse.PreferredName,
                Birthdate = person.Birthdate,
                Gender = person.Gender,
                Nationality = person.Nationality,
                PassportAddress = person.PassportAddress,
                PassportCity = person.PassportCity,
                PassportCountry = person.PassportCountry,
                PassportState = person.PassportState,
                PassportZip = person.PassportZip,
                ThaiAddress = person.ThaiAddress,
                ThaiAmphur = person.ThaiAmphur,
                ThaiMubaan = person.ThaiMubaan,
                ThaiProvince = person.ThaiProvince,
                ThaiSoi = person.ThaiSoi,
                ThaiTambon = person.ThaiTambon,
                ThaiZip = person.ThaiZip
            };

        public IQueryable<PersonRole> PersonRoles => _dbConnection.PersonRoles;

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
                PreferredName = person.PreferredName,
                IsDirectorPosition = personRole.IsDirectorPosition,
                IsStaffPosition = personRole.IsStaffPosition,
                FullHalfTime = personRole.FullHalfTime
            };

        public IQueryable<StaffWithName> StaffWithNames =>
            from staff in _dbConnection.Staff
            from person in _dbConnection.PeopleExtended.InnerJoin(person => person.StaffId == staff.Id)
            where !person.Deleted
            select new StaffWithName
            {
                Id = staff.Id,
                OrgGroupId = staff.OrgGroupId,
                PersonId = person.Id,
                PreferredName = person.PreferredName,
                AnnualSalary = staff.AnnualSalary
            };

        public IQueryable<EmergencyContactExtended> EmergencyContactsExtended =>
            from emergencyContact in _dbConnection.EmergencyContacts
            join person in PeopleExtended on emergencyContact.ContactId equals person.Id
            where !person.Deleted
            select new EmergencyContactExtended
            {
                Id = emergencyContact.Id,
                ContactId = emergencyContact.ContactId,
                Order = emergencyContact.Order,
                PersonId = emergencyContact.PersonId,
                ContactPreferedName = person.PreferredName
            };

        public PersonWithOthers GetById(Guid id)
        {
            var person = PeopleGeneric<PersonWithOthers>().FirstOrDefault(selectedPerson => selectedPerson.Id == id);
            if (person != null)
            {
                person.Roles = GetPersonRoles(id).ToList();
                person.EmergencyContacts = EmergencyContactsExtended.Where(contact => contact.PersonId == id).ToList();
            }

            return person;
        }

        public IQueryable<PersonRole> GetPersonRoles(Guid personId)
        {
            return _dbConnection.PersonRoles.Where(role => role.PersonId == personId);
        }

        public void DeleteStaff(Guid staffId)
        {
            using (var transaction = _dbConnection.BeginTransaction())
            {
                _dbConnection.StaffTraining.Where(training => training.StaffId == staffId).Delete();
                _dbConnection.Staff.Where(staff => staff.Id == staffId).Delete();
                transaction.Commit();
            }
        }
    }
}