using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;

namespace Backend.Services
{
    public class PersonService
    {
        private readonly PersonRepository _personRepository;
        private readonly EntityService _entityService;

        public PersonService(PersonRepository personRepository, EntityService entityService)
        {
            _personRepository = personRepository;
            _entityService = entityService;
        }

        public IList<Person> People() => _personRepository.People.ToList();

        public PersonExtended GetById(Guid id) => _personRepository.GetById(id);

        public void Save(PersonRole role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (role.PersonId == Guid.Empty) throw new NullReferenceException("role person id is null");
            _entityService.Save(role);
        } 
    }
}