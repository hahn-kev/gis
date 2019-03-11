using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Entities;
using LinqToDB;
using LinqToDB.Tools;

namespace Backend.DataLayer
{
    public class PersonRepository
    {
        private readonly IDbConnection _dbConnection;

        public PersonRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IQueryable<Person> People =>
            _dbConnection.People
                .OrderBy(person => person.PreferredName ?? person.FirstName)
                .ThenBy(person => person.LastName);

        public IQueryable<LeaveRequest> LeaveRequests => _dbConnection.LeaveRequests;

        public IQueryable<LeaveRequest> LeaveRequestsInYear(int year) =>
            _dbConnection.LeaveRequests.Where(request => request.StartDate.InSchoolYear(year));

        public IQueryable<PersonExtended> PeopleExtended =>
            _dbConnection.PeopleExtended
                .OrderBy(person => person.PreferredName ?? person.FirstName)
                .ThenBy(person => person.LastName);

        public IQueryable<PersonWithStaff> PeopleWithStaff => PeopleGeneric<PersonWithStaff>();

        public Dictionary<Guid, PersonWithStaff> FindByIds(IEnumerable<Guid> guids)
        {
            return PeopleWithStaff.Where(p => p.Id.In(guids)).ToDictionary(p => p.Id);
        }

        public IQueryable<PersonWithStaffBasic> PeopleWithStaffBasic
        {
            get
            {
                return from person in _dbConnection.PeopleExtended
                    from staff in Staff
                        .LeftJoin(staff => person.StaffId.HasValue && staff.Id == person.StaffId)
                        .DefaultIfEmpty()
                    where !person.Deleted
                    select new PersonWithStaffBasic
                    {
                        Id = person.Id,
                        Email = person.Email,
                        FirstName = person.FirstName,
                        IsThai = person.IsThai,
                        IsSchoolAid = person.IsSchoolAid,
                        IsAlumni = person.IsAlumni,
                        IsParent = person.IsParent,
                        LastName = person.LastName,
                        ThaiFirstName = person.ThaiFirstName,
                        ThaiLastName = person.ThaiLastName,
                        PreferredName = person.PreferredName,
                        SpeaksEnglish = person.SpeaksEnglish,
                        Staff = staff,
                        StaffId = person.StaffId,
                        DonorId = person.DonorId,
                        PhoneNumber = person.PhoneNumber,
                        SpouseId = person.SpouseId,
                        Birthdate = person.Birthdate,
                        Gender = person.Gender,
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
                        ThaiZip = person.ThaiZip,
                        ProfilePicDriveId = person.ProfilePicDriveId,
                        Deleted = person.Deleted
                    };
            }
        }

        public IQueryable<PersonWithStaffSummaries> PeopleWithStaffSummaries
        {
            get
            {
                return from person in PeopleGeneric<PersonWithStaffSummaries>()
                    from role in _dbConnection.PersonRoles.LeftJoin(role => role.PersonId == person.Id)
                    from job in _dbConnection.Job.LeftJoin(job => job.Id == role.JobId)
                    group new {role, job} by new {person}
                    into g
                    select new PersonWithStaffSummaries
                    {
                        Id = g.Key.person.Id,
                        Email = g.Key.person.Email,
                        FirstName = g.Key.person.FirstName,
                        IsThai = g.Key.person.IsThai,
                        IsSchoolAid = g.Key.person.IsSchoolAid,
                        IsAlumni = g.Key.person.IsAlumni,
                        IsParent = g.Key.person.IsParent,
                        LastName = g.Key.person.LastName,
                        ThaiFirstName = g.Key.person.ThaiFirstName,
                        ThaiLastName = g.Key.person.ThaiLastName,
                        PreferredName = g.Key.person.PreferredName,
                        SpeaksEnglish = g.Key.person.SpeaksEnglish,
                        Staff = g.Key.person.Staff,
                        StaffId = g.Key.person.StaffId,
                        DonorId = g.Key.person.DonorId,
                        PhoneNumber = g.Key.person.PhoneNumber,
                        SpouseId = g.Key.person.SpouseId,
                        Birthdate = g.Key.person.Birthdate,
                        Gender = g.Key.person.Gender,
                        PassportAddress = g.Key.person.PassportAddress,
                        PassportCity = g.Key.person.PassportCity,
                        PassportCountry = g.Key.person.PassportCountry,
                        PassportState = g.Key.person.PassportState,
                        PassportZip = g.Key.person.PassportZip,
                        ThaiAddress = g.Key.person.ThaiAddress,
                        ThaiAmphur = g.Key.person.ThaiAmphur,
                        ThaiMubaan = g.Key.person.ThaiMubaan,
                        ThaiProvince = g.Key.person.ThaiProvince,
                        ThaiSoi = g.Key.person.ThaiSoi,
                        ThaiTambon = g.Key.person.ThaiTambon,
                        ThaiZip = g.Key.person.ThaiZip,
                        ProfilePicDriveId = g.Key.person.ProfilePicDriveId,
                        Deleted = g.Key.person.Deleted,
                        //summary here
                        DaysOfService = g.Sum(r =>
                            r.job.Status == JobStatus.SchoolAid
                                ? 0
                                : r.role.StartDate.DayDiff(r.role.EndDate ?? Sql.CurrentTimestamp)),
                        IsActive = g.Sum(r => r.role.Active ? 1 : 0) > 0,
                        StartDate = g.Min(r => (DateTime?) r.role.StartDate)
                    };
            }
        }

        private IQueryable<TPerson> PeopleGeneric<TPerson>() where TPerson : PersonWithStaff, new() =>
            from person in _dbConnection.PeopleExtended
            from staff in StaffWithOrgNames.LeftJoin(staff => person.StaffId.HasValue && staff.Id == person.StaffId)
                .DefaultIfEmpty()
            where !person.Deleted
            select new TPerson
            {
                Id = person.Id,
                Email = person.Email,
                FirstName = person.FirstName,
                IsThai = person.IsThai,
                IsSchoolAid = person.IsSchoolAid,
                IsAlumni = person.IsAlumni,
                IsParent = person.IsParent,
                LastName = person.LastName,
                ThaiFirstName = person.ThaiFirstName,
                ThaiLastName = person.ThaiLastName,
                PreferredName = person.PreferredName,
                SpeaksEnglish = person.SpeaksEnglish,
                Staff = staff,
                StaffId = person.StaffId,
                DonorId = person.DonorId,
                PhoneNumber = person.PhoneNumber,
                SpouseId = person.SpouseId,
                Birthdate = person.Birthdate,
                Gender = person.Gender,
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
                ThaiZip = person.ThaiZip,
                ProfilePicDriveId = person.ProfilePicDriveId,
                Deleted = person.Deleted,
            };

        private IQueryable<JobWithOrgGroup> JobsWithOrgGroup =>
            from job in _dbConnection.Job
            from orgGroup in _dbConnection.OrgGroups.LeftJoin(g => g.Id == job.OrgGroupId).DefaultIfEmpty()
            from grade in _dbConnection.JobGrades.LeftJoin(grade => grade.Id == job.GradeId).DefaultIfEmpty()
            select new JobWithOrgGroup
            {
                Current = job.Current,
                Id = job.Id,
                GradeId = job.GradeId,
                OrgGroupId = job.OrgGroupId,
                JobDescription = job.JobDescription,
                Title = job.Title,
                Positions = job.Positions,
                Status = job.Status,
                Type = job.Type,
                OrgGroup = orgGroup,
                GradeNo = (int?) grade.GradeNo
            };

        public IQueryable<PersonRoleWithJob> PersonRolesWithJob =>
            (from personRole in _dbConnection.PersonRoles
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
                    Notes = personRole.Notes,
                    PreferredName = person.PreferredName,
                    LastName = person.LastName,
                    Job = job
                }).OrderByDescending(job => job.StartDate);

        public IList<StaffWithRoles> StaffWithRoles
        {
            get
            {
                var lookup = PersonRolesWithJob.ToLookup(job => job.PersonId);
                return StaffWithNames.Select(staff => new StaffWithRoles
                    {
                        StaffWithName = staff,
                        PersonRolesWithJob = lookup[staff.PersonId].ToList()
                    }
                ).ToList();
            }
        }

        public IQueryable<Staff> Staff => _dbConnection.Staff;

        public IQueryable<StaffWithName> StaffWithNames =>
            from staff in _dbConnection.Staff
            from person in _dbConnection.PeopleExtended.InnerJoin(person => person.StaffId == staff.Id)
            from missionOrg in _dbConnection.MissionOrgs.LeftJoin(org => staff.MissionOrgId == org.Id).DefaultIfEmpty()
            where !person.Deleted
            orderby person.PreferredName ?? person.FirstName, person.LastName
            select new StaffWithName
            {
                Id = staff.Id,
                Email = staff.Email,
                OrgGroupId = staff.OrgGroupId,
                MissionOrgId = staff.MissionOrgId,
                PersonId = person.Id,
                PreferredName = person.PreferredName ?? person.FirstName,
                LastName = person.LastName,
                AnnualSalary = staff.AnnualSalary,
                RenwebId = staff.RenwebId,
                MoeLicenseNumber = staff.MoeLicenseNumber,
                ContractIssued = staff.ContractIssued,
                ContractExpireDate = staff.ContractExpireDate,
                InsuranceNumber = staff.InsuranceNumber,
                Insurer = staff.Insurer,
                TeacherLicenseNo = staff.TeacherLicenseNo,
                TeacherLicenseOrg = staff.TeacherLicenseOrg,
                VisaType = staff.VisaType,
                WorkPermitType = staff.WorkPermitType,
                EndorsementAgency = staff.EndorsementAgency,
                Endorsements = staff.Endorsements,
                YearsOfServiceAdjustment = staff.YearsOfServiceAdjustment,
                PhoneExt = staff.PhoneExt,
                LeaveDelegateGroupId = staff.LeaveDelegateGroupId
            };

        public IQueryable<StaffWithOrgName> StaffWithOrgNames =>
            from staff in _dbConnection.Staff
            from missionOrg in _dbConnection.MissionOrgs.LeftJoin(org => staff.MissionOrgId == org.Id).DefaultIfEmpty()
            from orgGroup in _dbConnection.OrgGroups.LeftJoin(org => org.Id == staff.OrgGroupId).DefaultIfEmpty()
            select new StaffWithOrgName
            {
                Id = staff.Id,
                Email = staff.Email,
                OrgGroupId = staff.OrgGroupId,
                MissionOrgId = staff.MissionOrgId,
                MissionOrgName = missionOrg.Name,
                MissionOrgEmail = missionOrg.Email,
                AnnualSalary = staff.AnnualSalary,
                RenwebId = staff.RenwebId,
                MoeLicenseNumber = staff.MoeLicenseNumber,
                ContractIssued = staff.ContractIssued,
                ContractExpireDate = staff.ContractExpireDate,
                InsuranceNumber = staff.InsuranceNumber,
                Insurer = staff.Insurer,
                TeacherLicenseNo = staff.TeacherLicenseNo,
                TeacherLicenseOrg = staff.TeacherLicenseOrg,
                VisaType = staff.VisaType,
                WorkPermitType = staff.WorkPermitType,
                EndorsementAgency = staff.EndorsementAgency,
                Endorsements = staff.Endorsements,
                YearsOfServiceAdjustment = staff.YearsOfServiceAdjustment,
                PhoneExt = staff.PhoneExt,
                LeaveDelegateGroupId = staff.LeaveDelegateGroupId,
                OrgGroupName = orgGroup.GroupName,
                OrgGroupSupervisor = orgGroup.Supervisor
            };

        public IQueryable<EmergencyContactExtended> EmergencyContactsExtended =>
            from emergencyContact in _dbConnection.EmergencyContacts
            from person in PeopleExtended.LeftJoin(person => emergencyContact.ContactId == person.Id)
            where person == null || !person.Deleted
            orderby emergencyContact.Order
            select new EmergencyContactExtended
            {
                Id = emergencyContact.Id,
                ContactId = emergencyContact.ContactId,
                Order = emergencyContact.Order,
                PersonId = emergencyContact.PersonId,
                ContactPreferredName = person.PreferredName ?? person.FirstName,
                ContactLastName = person.LastName,
                ContactEmail = person.Email,
                ContactPhone = person.PhoneNumber,
                Relationship = emergencyContact.Relationship,
                Name = emergencyContact.Name,
                Email = emergencyContact.Email,
                Phone = emergencyContact.Phone
            };

        public PersonWithOthers GetById(Guid id)
        {
            var result = (from p in PeopleGeneric<PersonWithOthers>()
                from donor in _dbConnection.Donors.LeftJoin(donor => p.DonorId.HasValue && donor.Id == p.DonorId)
                    .DefaultIfEmpty()
                select new {person = p, donor}).FirstOrDefault(arg => arg.person.Id == id);
            if (result == null) throw new NullReferenceException($"Unable to find person with ID {id}");
            var person = result.person;
            person.Donor = result.donor;
            if (person.DonorId.HasValue)
            {
                person.Donations = _dbConnection.Donations
                    .Where(donation => donation.PersonId == person.Id)
                    .OrderBy(d => d.Date)
                    .ToList();
            }

            if (person.StaffId.HasValue)
            {
                person.Education = _dbConnection.Education
                    .Where(education => education.PersonId == person.Id)
                    .OrderBy(education => education.CompletedDate)
                    .ToList();
            }

            person.Roles = GetPersonRolesWithJob(id).ToList();
            person.EmergencyContacts = EmergencyContactsExtended.Where(contact => contact.PersonId == id).ToList();

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

        public IQueryable<PersonWithStaff> GetStaffNotifyHr()
        {
            return from person in PeopleWithStaff.Where(staff => staff.StaffId != null)
                from user in _dbConnection.Users.InnerJoin(u => person.Id == u.PersonId)
                where user.SendHrLeaveEmails
                select person;
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

        public void DeleteDonor(Guid donorId)
        {
            _dbConnection.Donors.Delete(donor => donor.Id == donorId);
        }
    }
}