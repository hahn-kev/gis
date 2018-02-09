using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoBogus;
using Backend;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;
using Xunit;

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
                .SelectMany(type => type.GetProperties()
                    .Where(info => typeof(IQueryable).IsAssignableFrom(info.PropertyType))
                    .Select(info => new object[] {type, info})
                );
        }

        [Theory]
        [MemberData(nameof(GetRepoTypes))]
        public void ShouldEachPropertyResultInAPopulatedObject(Type repoType, PropertyInfo info)
        {
            var repo = _servicesFixture.ServiceProvider.GetService(repoType);
            var list = (IQueryable) info.GetValue(repo);

            var value = list.Cast<object>().FirstOrDefault();
            Assert.NotNull(value);
            Assert.Empty(NotPopulatedValues(value));
        }

        private IEnumerable<(PropertyInfo, object)> NotPopulatedValues(object o)
        {
            return o.GetType().GetProperties().Select(info => (info: info, val: info.GetValue(o)))
                .Where(t =>
                    {
                        if (t.info.PropertyType == typeof(Guid)) return Guid.Empty == (Guid) t.val;
                        return t.val == (t.info.PropertyType.IsPrimitive
                                   ? Activator.CreateInstance(t.info.PropertyType)
                                   : null);
                    }
                );
        }
    }
}