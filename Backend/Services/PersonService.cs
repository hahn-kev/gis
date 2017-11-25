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

        public PersonService(PersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public IList<Person> People() => _personRepository.People.ToList();

        public PersonExtended GetById(Guid id) => _personRepository.GetById(id);

        public void Update(PersonExtended person)
        {
            if (person.Id == Guid.Empty)
            {
                person.Id = Guid.NewGuid();
                _personRepository.Insert(person);
            }
            else
            {
                _personRepository.Update(person);
            }
        }
    }
}