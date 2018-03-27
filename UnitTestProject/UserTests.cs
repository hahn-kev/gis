using System.Linq;
using Backend.DataLayer;
using Xunit;

namespace UnitTestProject
{
    public class UserTests
    {
        
        private ServicesFixture _servicesFixture;

        public UserTests()
        {
            _servicesFixture = new ServicesFixture();
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
    }
}