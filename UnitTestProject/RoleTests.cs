using System;
using System.Linq;
using AutoBogus;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Xunit;

namespace UnitTestProject
{
    public class RoleTests:IClassFixture<ServicesFixture>
    {
        private readonly ServicesFixture _servicesFixture;
        private readonly IDbConnection _db;
        private readonly PersonService _personService;

        public RoleTests(ServicesFixture servicesFixture)
        {
            _servicesFixture = servicesFixture;
            _db = _servicesFixture.DbConnection;
            _personService = _servicesFixture.Get<PersonService>();
        }

        [Fact]
        public void StaffOrgGroupIdUpdatedOnNewRoleAdded()
        {
            var job = _servicesFixture.InsertJob();
            Assert.NotEqual(Guid.Empty, job.OrgGroupId);
            var person = _servicesFixture.InsertPerson();
            Assert.NotEqual(job.OrgGroupId, person.Staff.OrgGroupId);
            var role = AutoFaker.Generate<PersonRole>();
            role.Id = Guid.Empty;
            role.Active = true;
            role.JobId = job.Id;
            role.PersonId = person.Id;
            _personService.Save(role);
            Assert.Equal(job.OrgGroupId, _db.Staff.Single(s => s.Id == person.StaffId).OrgGroupId);
        }

        [Fact]
        public void StaffOrgGroupIdNotUpdatedOnExistingRoleUpdated()
        {
            var job = _servicesFixture.InsertJob();
            Assert.NotEqual(Guid.Empty, job.OrgGroupId);
            var person = _servicesFixture.InsertPerson();
            Assert.NotEqual(job.OrgGroupId, person.Staff.OrgGroupId);
            var role = _servicesFixture.InsertRole();
            role.Active = true;
            role.JobId = job.Id;
            role.PersonId = person.Id;
            _personService.Save(role);
            var currentOrgGroupId = _db.Staff.Single(s => s.Id == person.StaffId).OrgGroupId;
            Assert.Equal(person.Staff.OrgGroupId, currentOrgGroupId);
            Assert.NotEqual(job.OrgGroupId, currentOrgGroupId);
        }

        [Fact]
        public void StaffOrgGroupIdNotUpdatedOnInactiveRoleAdded()
        {
            var job = _servicesFixture.InsertJob();
            Assert.NotEqual(Guid.Empty, job.OrgGroupId);
            var person = _servicesFixture.InsertPerson();
            Assert.NotEqual(job.OrgGroupId, person.Staff.OrgGroupId);
            var role = AutoFaker.Generate<PersonRole>();
            role.Id = Guid.Empty;
            role.Active = false;
            role.JobId = job.Id;
            role.PersonId = person.Id;
            _personService.Save(role);
            var currentOrgGroupId = _db.Staff.Single(s => s.Id == person.StaffId).OrgGroupId;
            Assert.Equal(person.Staff.OrgGroupId, currentOrgGroupId);
            Assert.NotEqual(job.OrgGroupId, currentOrgGroupId);
        }
    }
}