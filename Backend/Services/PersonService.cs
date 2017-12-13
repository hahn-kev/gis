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
        private readonly IEntityService _entityService;

        public PersonService(PersonRepository personRepository, IEntityService entityService)
        {
            _personRepository = personRepository;
            _entityService = entityService;
        }

        public IList<Person> People() => _personRepository.People.OrderBy(person => person.FirstName)
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

        public void Save(PersonExtended person)
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
                if (person.StaffId.HasValue) _entityService.Delete<Staff>(person.StaffId.Value);
                person.StaffId = null;
            }
            _entityService.Save(person);
        }
    }
}