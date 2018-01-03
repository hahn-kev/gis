using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoBogus;
using Backend;
using Backend.DataLayer;
using Backend.Entities;
using Bogus;
using LinqToDB;
using LinqToDB.Extensions;
using Newtonsoft.Json;
using Xunit;
using Xunit.Sdk;
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
            _servicesFixture.SetupData();
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

        public static IEnumerable<object[]> GetRepoTypes()
        {
            return typeof(Startup).Assembly.GetTypes()
                .Where(type => type.Name.Contains("Repository") && !type.IsInterface && type != typeof(ImageRepository))
                .Select(type => new object[] {type});
        }

        [Theory]
        [MemberData(nameof(GetRepoTypes))]
        public void ShouldEachPropertyResultInAPopulatedObject(Type repoType)
        {
            var repo = _servicesFixture.ServiceProvider.GetService(repoType);
            var valueTuples = repoType.GetProperties()
                .Where(info => typeof(IQueryable).IsAssignableFrom(info.PropertyType))
                .Select(info => (propertyName: info.Name, list: (IQueryable) info.GetValue(repo)));
            foreach (var tuple in valueTuples)
            {
                var value = tuple.list.Cast<object>().FirstOrDefault();
                if (value == null) throw new XunitException($"property:[{tuple.propertyName}] didn't list any values");
                if (!IsPopulated(value))
                    throw new XunitException(
                        $"property:[{tuple.propertyName}] didn't populate all values {JsonConvert.SerializeObject(value)}");
            }
        }

        private bool IsPopulated(object o)
        {
            return o.GetType().GetProperties()
                .All(info =>
                {
                    var val =
                    info.GetValue(o);
                    if (info.PropertyType == typeof(Guid)) return Guid.Empty != (Guid) val;
                    return val != (info.PropertyType.IsPrimitive
                        ? Activator.CreateInstance(info.PropertyType)
                        : null);
                }
                    );
        }
    }
}