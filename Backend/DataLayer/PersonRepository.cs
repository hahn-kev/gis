using System;
using System.Collections.Generic;
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

        public IQueryable<Person> People => _dbConnection.People
            .OrderBy(person => person.PreferredName)
            .ThenBy(person => person.LastName);

        public IQueryable<LeaveRequest> LeaveRequests => _dbConnection.LeaveRequests;

        public IQueryable<PersonExtended> PeopleExtended => _dbConnection.PeopleExtended
            .OrderBy(person => person.PreferredName)
            .ThenBy(person => person.LastName);
        public IQueryable<PersonWithStaff> PeopleWithStaff => PeopleGeneric<PersonWithStaff>();

        private IQueryable<TPerson> PeopleGeneric<TPerson>() where TPerson : PersonWithStaff, new() =>
            (from person in _dbConnection.PeopleExtended
                from spouse in _dbConnection.People.LeftJoin(person1 => person1.Id == person.SpouseId).DefaultIfEmpty()
                from staff in _dbConnection.Staff.LeftJoin(staff => staff.Id == person.StaffId).DefaultIfEmpty()
                where !person.Deleted
                select new TPerson
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
                }).OrderBy(_ => _.PreferredName).ThenBy(_ => _.LastName);

        private IQueryable<JobWithOrgGroup> JobsWithOrgGroup => from job in _dbConnection.Job
            join orgGroup in _dbConnection.OrgGroups on job.OrgGroupId equals orgGroup.Id
            select new JobWithOrgGroup
            {
                Current = job.Current,
                Id = job.Id,
                GradeId = job.GradeId,
                OrgGroupId = job.OrgGroupId,
                JobDescription = job.JobDescription,
                Title = job.Title,
                Positions = job.Positions,
                Type = job.Type,
                OrgGroup = orgGroup
            };

        public IQueryable<PersonRoleWithJob> PersonRolesWithJob =>
            from personRole in _dbConnection.PersonRoles
            join person in People on personRole.PersonId equals person.Id
            join job in JobsWithOrgGroup on personRole.JobId equals job.Id
            select new PersonRoleWithJob
            {
                Id = personRole.Id,
                JobId = personRole.JobId,
                PersonId = personRole.PersonId,
                Active = personRole.Active,
                StartDate = personRole.StartDate,
                EndDate = personRole.EndDate,
                PreferredName = person.PreferredName,
                LastName = person.LastName,
                Job = job
            };

        public IQueryable<Staff> Staff => _dbConnection.Staff;

        public IQueryable<StaffWithName> StaffWithNames =>
            from staff in _dbConnection.Staff
            from person in _dbConnection.PeopleExtended.InnerJoin(person => person.StaffId == staff.Id)
            where !person.Deleted
            select new StaffWithName
            {
                Id = staff.Id,
                Email = staff.Email,
                OrgGroupId = staff.OrgGroupId,
                MissionOrgId = staff.MissionOrgId,
                PersonId = person.Id,
                PreferredName = person.PreferredName,
                LastName = person.LastName,
                AnnualSalary = staff.AnnualSalary,
                RenwebId = staff.RenwebId,
                MoeLicenseNumber = staff.MoeLicenseNumber,
                ContractIssued = staff.ContractIssued,
                ContractExpireDate = staff.ContractExpireDate,
                InsuranceNumber = staff.InsuranceNumber,
                TeacherLicenseNo = staff.TeacherLicenseNo,
                TeacherLicenseOrg = staff.TeacherLicenseOrg,
                ThaiSsn = staff.ThaiSsn,
                VisaType = staff.VisaType,
                WorkPermitType = staff.WorkPermitType,
                EndorsementAgency = staff.EndorsementAgency,
                Endorsements = staff.Endorsements
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
                ContactPreferedName = person.PreferredName,
                ContactLastName = person.LastName,
                Relationship = emergencyContact.Relationship
            };

        public PersonWithOthers GetById(Guid id)
        {
            var person = PeopleGeneric<PersonWithOthers>().FirstOrDefault(selectedPerson => selectedPerson.Id == id);
            if (person != null)
            {
                person.Roles = GetPersonRolesWithJob(id).ToList();
                person.EmergencyContacts = EmergencyContactsExtended.Where(contact => contact.PersonId == id).ToList();
            }

            return person;
        }

        public IQueryable<PersonRoleWithJob> GetPersonRolesWithJob(Guid personId)
        {
            return PersonRolesWithJob.Where(role => role.PersonId == personId);
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

        public IQueryable<PersonWithStaff> GetHrStaff()
        {
            return GetStaffWithUserRole("HR");
        }

        public IQueryable<PersonWithStaff> GetHrAdminStaff()
        {
            return GetStaffWithUserRole("HRADMIN");
        }

        private IQueryable<PersonWithStaff> GetStaffWithUserRole(string roleName)
        {
            return from person in PeopleWithStaff.Where(staff => staff.StaffId != null)
                from user in _dbConnection.Users.InnerJoin(u => person.Id == u.PersonId)
                from userRole in _dbConnection.UserRoles.InnerJoin(u => u.UserId == user.Id) 
                from role in _dbConnection.Roles.InnerJoin(role => role.Id == userRole.RoleId)
                where role.NormalizedName == roleName.ToUpper()
                select person;
        }
    }
}