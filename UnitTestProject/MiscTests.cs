using System;
using System.Linq;
using Backend.DataLayer;
using Xunit;

namespace UnitTestProject
{
    public class MiscTests
    {
        private ServicesFixture _servicesFixture;
        private DbConnection _dbConnection;
        private PersonRepository _personRepository;

        public MiscTests()
        {
            _servicesFixture = new ServicesFixture();
            _dbConnection = _servicesFixture.Get<DbConnection>();
            _personRepository = _servicesFixture.Get<PersonRepository>();
        }

        [Fact]
        public void GetStaffDoesntCrash()
        {
            var list = _personRepository.StaffWithNames.Where(name => name.Id == Guid.Empty).ToList();
            Assert.NotNull(list);
        }
    }
}