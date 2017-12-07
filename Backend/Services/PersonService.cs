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

        public IList<Person> People() => _personRepository.People.OrderBy(person => person.FirstName).ThenBy(person => person.LastName).ToList();

        public PersonExtended GetById(Guid id) => _personRepository.GetById(id);

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
    }
}