using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;

namespace Backend.Services
{
    public class PersonService
    {
        private readonly PersonRepository _personRepository;
        private readonly UsersRepository _usersRepository;
        private readonly IEntityService _entityService;

        public PersonService(PersonRepository personRepository,
            IEntityService entityService,
            UsersRepository usersRepository)
        {
            _personRepository = personRepository;
            _entityService = entityService;
            _usersRepository = usersRepository;
        }

        public IList<Person> People() =>
            _personRepository.People.OrderBy(person => person.FirstName)
                .ThenBy(person => person.LastName).ToList();

        public PersonWithOthers GetById(Guid id) => _personRepository.GetById(id);

        public void Save(PersonRole role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (role.PersonId == Guid.Empty) throw new NullReferenceException("role person id is null");
            _entityService.Save(role);
        }

        public IList<PersonRoleExtended> Roles(bool canStartDuringRange, DateTime beginRange, DateTime endRange)
        {
            return _personRepository.PersonRolesExtended
                .Where(role => (role.StartDate < beginRange || (canStartDuringRange && role.StartDate < endRange)) &&
                               (role.Active || role.EndDate > endRange)).ToList();
        }

        public void Save(PersonWithStaff person)
        {
            if (string.IsNullOrEmpty(person.PreferredName))
            {
                person.PreferredName = $"{person.FirstName} {person.LastName}";
            }

            if (person.Staff != null)
            {
                _entityService.Save(person.Staff);
                person.StaffId = person.Staff.Id;
            }
            else
            {
                if (person.StaffId.HasValue) _personRepository.DeleteStaff(person.StaffId.Value);
                person.StaffId = null;
            }

            _entityService.Save(person);
            MatchPersonWithUser(person);

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

        public void MatchPersonWithUser(Person person)
        {
            if (string.IsNullOrEmpty(person.Email)) return;
            var user = _usersRepository.Users.SingleOrDefault(profile =>
                profile.PersonId == null && profile.Email == person.Email);
            if (user != null)
                _usersRepository.UpdatePersonId(user.Id, person.Id);
        }

        public IList<PersonWithDaysOfLeave> PeopleWithDaysOfLeave(Guid? limitByPersonId = null)
        {
            return _personRepository.PeopleWithDaysOfLeave(limitByPersonId).ToList();
        }
    }
}