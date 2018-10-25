using System.Linq;
using System.Reflection;
using AutoBogus;
using Backend.DataLayer;
using Backend.Entities;
using Shouldly;
using Xunit;

namespace UnitTestProject
{
    public class UserTests:IClassFixture<ServicesFixture>
    {
        private readonly ServicesFixture _servicesFixture;

        public UserTests(ServicesFixture servicesFixture)
        {
            _servicesFixture = servicesFixture;
        }

        [Fact]
        public void GetHrAdminsReturnsUsers()
        {
            var personAdmin = _servicesFixture.InsertPerson();
            var personHrStaff = _servicesFixture.InsertPerson();
            var hrAdmin = _servicesFixture.InsertUser(u => u.PersonId = personAdmin.Id, "hradmin");
            var nonHrAdmin = _servicesFixture.InsertUser(u => u.PersonId = personHrStaff.Id, "hr");
            var actualStaff = _servicesFixture.Get<PersonRepository>().GetHrAdminStaff().Single();
            Assert.Equal(personAdmin.Id, actualStaff.Id);
        }

        [Fact]
        public void CopyFromAllPropertiesUpdated()
        {
            var user = AutoFaker.Generate<UserProfile>();
            var user2 = AutoFaker.Generate<UserProfile>();
            user.CopyFrom(user2);
            var missingProperties = typeof(IUser).GetProperties()
                .Select(info => new {info, v1 = info.GetValue(user), v2 = info.GetValue(user2)})
                .Where(arg => arg.info.Name != "Id" && !Equals(arg.v1, arg.v2))
                .Select(arg => arg.info.Name)
                .ToArray();
            missingProperties.ShouldBeEmpty();
        }
    }
}