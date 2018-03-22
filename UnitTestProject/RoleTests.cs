using System;
using System.Linq;
using AutoBogus;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace UnitTestProject
{
    public class RoleTests
    {
        private ServicesFixture _servicesFixture;
        private IDbConnection _db;
        private PersonService _personService;

        public RoleTests()
        {
            _servicesFixture = new ServicesFixture(collection =>
                collection.Replace(ServiceDescriptor.Singleton<IEntityService, EntityService>()));
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