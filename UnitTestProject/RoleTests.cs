using System;
using System.Linq;
using AutoBogus;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using LinqToDB.Data;
using Shouldly;
using Xunit;

namespace UnitTestProject
{
    public class RoleTests:IClassFixture<ServicesFixture>, IDisposable
    {
        private readonly ServicesFixture _servicesFixture;
        private readonly IDbConnection _db;
        private readonly PersonService _personService;
        private readonly PersonRepository _personRepository;
        private DataConnectionTransaction _transaction;

        public RoleTests(ServicesFixture servicesFixture)
        {
            _servicesFixture = servicesFixture;
            _db = _servicesFixture.DbConnection;
            _transaction = _db.BeginTransaction();
            _personService = _servicesFixture.Get<PersonService>();
            _personRepository = _servicesFixture.Get<PersonRepository>();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
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


        [Fact]
        public void StaffSummariesIncludeStaffWithoutRole()
        {
            var pWithRole = _servicesFixture.InsertPerson();
            var job = _servicesFixture.InsertStaffJob();
            _servicesFixture.InsertRole(job.Id, pWithRole.Id);
            var pWithoutRole = _servicesFixture.InsertPerson();
            _personRepository.Staff.Count().ShouldBe(2);
            _personRepository.PeopleWithStaffSummaries.Count().ShouldBe(2);
        }
    }
}