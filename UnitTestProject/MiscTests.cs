using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoBogus;
using Backend;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;
using Newtonsoft.Json;
using Shouldly;
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
            _personRepository = _servicesFixture.Get<PersonRepository>();
        }

        [Fact]
        public void JsonDateWithoutKindHasCorrectFormat()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            var utcDate = new DateTime(30000, DateTimeKind.Utc);
            var unspecDate = new DateTime(30000, DateTimeKind.Unspecified);
            Assert.Equal(JsonConvert.SerializeObject(utcDate),
                JsonConvert.SerializeObject(unspecDate));
            Assert.Equal(utcDate, unspecDate);
            Assert.True(utcDate == unspecDate);
            Assert.True(Equals(utcDate, unspecDate));
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
            _servicesFixture.SetupData();
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
            _servicesFixture.SetupData();
            var repo = _servicesFixture.ServiceProvider.GetService(repoType);
            var list = (IQueryable) info.GetValue(repo);

            var value = list.Cast<object>()
                .ToList()
                .Select(o => NotPopulatedValues(o, list.ElementType).ToList())
                .OrderBy(properties => properties.Count).First();
            Assert.Empty(value);
        }

        private IEnumerable<string> NotPopulatedValues(object o, Type listElementType)
        {
            IEnumerable<PropertyInfo> propertyInfos;
            if (o == null) propertyInfos = listElementType.GetProperties();
            else
            {
                propertyInfos = listElementType.GetProperties().Select(info => (info: info, val: info.GetValue(o)))
                    .Where(t =>
                        {
                            if (t.info.PropertyType == typeof(Guid)) return Guid.Empty == (Guid) t.val;
                            return t.val == (t.info.PropertyType.IsPrimitive
                                       ? Activator.CreateInstance(t.info.PropertyType)
                                       : null);
                        }
                    ).Select(tuple => tuple.info);
            }

            return propertyInfos.Select(val => val.DeclaringType.ToString() + "." + val.Name);
        }

        [Fact]
        public void SqlDatesGetExecutedProperly()
        {
            var tr = _servicesFixture.InsertRequirement(months: 12);
            var actualDate = _servicesFixture.DbConnection.TrainingRequirements.Select(requirement => 
            
                Sql.AsSql(new DateTime(2018, 3, 1).AddMonths(requirement.RenewMonthsCount / -2))
            ).First();
            actualDate.ShouldBe(new DateTime(2018, 3, 1).AddMonths(tr.RenewMonthsCount / -2));
        }
    }
}