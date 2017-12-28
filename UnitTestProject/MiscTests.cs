using System;
using System.Linq;
using AutoBogus;
using Backend.DataLayer;
using Backend.Entities;
using Bogus;
using LinqToDB;
using Xunit;
using Person = Backend.Entities.Person;

namespace UnitTestProject
{
    public class MiscTests
    {
        private ServicesFixture _servicesFixture;
        private PersonRepository _personRepository;

        public MiscTests()
        {
            _servicesFixture = new ServicesFixture();
            _servicesFixture.SetupPeople();
            _personRepository = _servicesFixture.Get<PersonRepository>();
        }

        [Fact]
        public void CanMockQuery()
        {
            var person1 = AutoFaker.Generate<PersonExtended>();
            _servicesFixture.DbConnection.Insert(person1);
            _servicesFixture.DbConnection.Insert(AutoFaker.Generate<PersonExtended>());
            var actualPerson = _personRepository.People.Single(person => person.Id == person1.Id);
            Assert.Equal(person1.FirstName, actualPerson.FirstName);
        }
        
        [Fact]
        public void GetStaffDoesntCrash()
        {
            var list = _personRepository.StaffWithNames.Where(name => name.Id != Guid.Empty).ToList();
            Assert.NotNull(list);
            Assert.NotEmpty(list);
        }
    }
}