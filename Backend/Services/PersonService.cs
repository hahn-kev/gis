using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;
using Backend.Utils;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Backend.Services
{
    public class PersonService
    {
        private readonly PersonRepository _personRepository;
        private readonly UsersRepository _usersRepository;
        private readonly IEntityService _entityService;
        private readonly JobRepository _jobRepository;
        private readonly LeaveService _leaveService;
        private readonly EvaluationRepository _evaluationRepository;
        private readonly EndorsementService _endorsementService;

        public PersonService(PersonRepository personRepository,
            IEntityService entityService,
            UsersRepository usersRepository,
            JobRepository jobRepository,
            LeaveService leaveService,
            EvaluationRepository evaluationRepository,
            EndorsementService endorsementService)
        {
            _personRepository = personRepository;
            _entityService = entityService;
            _usersRepository = usersRepository;
            _jobRepository = jobRepository;
            _leaveService = leaveService;
            _evaluationRepository = evaluationRepository;
            _endorsementService = endorsementService;
        }

        #region people

        public IList<Person> People() => _personRepository.People.Where(person => !person.Deleted).ToList();

        public IList<Person> SchoolAids() => _personRepository.People.Where(p => p.IsSchoolAid && !p.Deleted).ToList();

        public PersonWithStaff GetStaffById(Guid personId)
        {
            return _personRepository.PeopleWithStaff.FirstOrDefault(person => person.Id == personId);
        }
        
        public PersonWithOthers GetById(Guid id)
        {
            var personWithOthers = _personRepository.GetById(id);
            if (personWithOthers.StaffId.HasValue)
            {
                personWithOthers.Evaluations = _evaluationRepository.EvaluationWithNames
                    .Where(eval => eval.PersonId == id).ToList();
                personWithOthers.LeaveDetails = _leaveService.GetCurrentLeaveDetails(personWithOthers);
                personWithOthers.StaffEndorsements = _endorsementService.ListStaffEndorsements(id);
            }
            return personWithOthers;
        }

        public void Save(PersonWithOthers person)
        {
            if (string.IsNullOrEmpty(person.PreferredName))
            {
                person.PreferredName = person.FirstName;
            }

            if (person.Staff != null)
            {
                _entityService.Save<Staff>(person.Staff);
                person.StaffId = person.Staff.Id;
            }
            else
            {
                if (person.StaffId.HasValue) _personRepository.DeleteStaff(person.StaffId.Value);
                person.StaffId = null;
            }

            if (person.Donor != null)
            {
                _entityService.Save(person.Donor);
                person.DonorId = person.Donor.Id;
            }
            else
            {
                if (person.DonorId.HasValue) _personRepository.DeleteDonor(person.DonorId.Value);
                person.DonorId = null;
            }

            _entityService.Save<PersonExtended>(person);
            if (person.Staff != null)
                MatchStaffWithUser(person.Staff, person.Id);

            if (person.SpouseChanged)
            {
                if (person.SpouseId.HasValue)
                {
                    //find the spouse and set them to be this persons spouse
                    _personRepository.PeopleExtended.Where(extended => extended.Id == person.SpouseId)
                        .Set(extended => extended.SpouseId, person.Id).Update();
                    //note at the moment you could orphan a spouse this way, in the future we could write another update
                    //so to set anyone who has this person as a spouse to null, this would only happen
                    //if you changed someone from the spouse of one person to another though
                }
                else
                {
                    //find the current spouse and make them not spouses anymore
                    _personRepository.PeopleExtended.Where(extended => extended.SpouseId == person.Id)
                        .Set(extended => extended.SpouseId, (Guid?) null).Update();
                }
            }
        }

        public IList<StaffWithName> StaffWithNames => _personRepository.StaffWithNames.ToList();

        public IList<PersonWithStaff> StaffAll =>
            _personRepository.PeopleWithStaff.Where(staff => staff.StaffId != null).ToList();

        public IList<PersonWithStaffSummaries> StaffSummaries =>
            _personRepository.PeopleWithStaffSummaries.Where(staff => staff.StaffId != null).ToList();

        public IList<StaffWithRoles> StaffWithRoles => _personRepository.StaffWithRoles;

        public void MatchStaffWithUser(Staff staff, Guid personId)
        {
            if (string.IsNullOrEmpty(staff.Email)) return;
            var user = _usersRepository.Users.SingleOrDefault(profile =>
                profile.PersonId == null && profile.Email == staff.Email);
            if (user != null)
                _usersRepository.UpdatePersonId(user.Id, personId);
        }

        #endregion

        #region roles

        public void Save(PersonRole role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (role.PersonId == Guid.Empty) throw new NullReferenceException("role person id is null");
            var isNew = role.IsNew();
            _entityService.Save(role);
            if (isNew && role.Active)
            {
                //when a new job is active, take the org group from the job and put it on the staff
                _personRepository.Staff
                    .Where(staff => staff.Id == _personRepository.People
                                        .Where(person => person.Id == role.PersonId && person.StaffId != null)
                                        .Select(person => (Guid) person.StaffId).SingleOrDefault())
                    .Set(staff => staff.OrgGroupId,
                        () => _jobRepository.Job.Where(job => job.Id == role.JobId).Select(job => job.OrgGroupId)
                            .SingleOrDefault()).Update();
            }
        }

        public void DeleteRole(Guid id)
        {
            _entityService.Delete<PersonRole>(id);
        }

        public IList<PersonRoleWithJob> Roles(bool canStartDuringRange, DateTime beginRange, DateTime endRange)
        {
            return _personRepository.PersonRolesWithJob
                .Where(role => (role.StartDate < beginRange || (canStartDuringRange && role.StartDate < endRange)) &&
                               (role.Active || role.EndDate > endRange)).ToList();
        }

        #endregion

        #region emergency contact

        public IList<EmergencyContactExtended> GetEmergencyContacts(Guid personId)
        {
            return _personRepository.EmergencyContactsExtended
                .Where(extended => extended.PersonId == personId)
                .OrderBy(contact => contact.Order)
                .ToList();
        }

        public void Save(EmergencyContact contact)
        {
            _entityService.Save(contact);
        }

        public void DeleteEmergencyContact(Guid id)
        {
            _entityService.Delete<EmergencyContact>(id);
        }

        #endregion

        public void DeletePerson(Guid id)
        {
            _personRepository.People.Where(person => person.Id == id).Set(person => person.Deleted, true).Update();
        }
    }
}