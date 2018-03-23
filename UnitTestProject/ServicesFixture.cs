using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoBogus;
using Backend;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Bogus;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Xunit;

namespace UnitTestProject
{
    public class ServicesFixture
    {
        public ServiceProvider ServiceProvider { get; }
        public IServiceCollection ServiceCollection { get; }
        public T Get<T>() => ServiceProvider.GetService<T>();
        private IDbConnection _dbConnection;
        public IDbConnection DbConnection => _dbConnection ?? (_dbConnection = Get<IDbConnection>());

        public ServicesFixture(Action<IServiceCollection> configure = null)
        {
            ServiceCollection = new ServiceCollection();
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.Add(new MemoryConfigurationSource
            {
                InitialData = new[]
                {
                    new KeyValuePair<string, string>("Environment", "UnitTest"),
                    new KeyValuePair<string, string>("JWTSettings:SecretKey", "helloWorld"),
                }
            });
            var startup = new Startup(builder.Build());
            ServiceCollection.AddLogging(loggingBuilder => loggingBuilder.AddConsole().AddDebug());
            startup.ConfigureServices(ServiceCollection);
            ServiceCollection.RemoveAll<IEmailService>().AddSingleton(Mock.Of<IEmailService>());
            ServiceCollection.Replace(ServiceDescriptor.Singleton(Mock.Of<IEntityService>()));
            configure?.Invoke(ServiceCollection);
            ServiceProvider = ServiceCollection.BuildServiceProvider();
            startup.ConfigureDatabase(ServiceProvider);
            DataConnection.DefaultSettings = new MockDbSettings();
            DataConnection.WriteTraceLine = (message, category) => Debug.WriteLine(message, category);
            SetupSchema();
        }


        private void SetupSchema()
        {
            TryCreateTable<IdentityUser>();
            TryCreateTable<LinqToDB.Identity.IdentityUserClaim<int>>();
            TryCreateTable<LinqToDB.Identity.IdentityUserLogin<int>>();
            TryCreateTable<LinqToDB.Identity.IdentityUserToken<int>>();
            TryCreateTable<LinqToDB.Identity.IdentityUserRole<int>>();
            TryCreateTable<LinqToDB.Identity.IdentityRole<int>>();
            TryCreateTable<LinqToDB.Identity.IdentityRoleClaim<int>>();
            TryCreateTable<PersonExtended>();
            TryCreateTable<PersonRole>();
            TryCreateTable<Job>();
            TryCreateTable<OrgGroup>();
            TryCreateTable<LeaveRequest>();
            TryCreateTable<TrainingRequirement>();
            TryCreateTable<Staff>();
            TryCreateTable<StaffTraining>();
            TryCreateTable<EmergencyContact>();
            TryCreateTable<Attachment>();

            var roles = new[] {"admin", "hr"};
            foreach (var role in roles)
            {
                if (!DbConnection.Roles.Any(identityRole => identityRole.Name == role))
                {
                    DbConnection.InsertId(
                        new LinqToDB.Identity.IdentityRole<int>(role) {NormalizedName = role.ToUpper()});
                }
            }
        }

        private void TryCreateTable<T>()
        {
            try
            {
                DbConnection.CreateTable<T>();
            }
            catch (PostgresException e) when (e.SqlState == "42P07") //already exists code I think
            {
            }
        }

        public void SetupPeople()
        {
            var faker = PersonFaker();
            var jacob = faker.Generate();
            jacob.FirstName = "Jacob";
            var bob = faker.Generate();
            bob.FirstName = "Bob";
            var jacobWife = faker.Generate();
            jacobWife.SpouseId = jacob.Id;
            jacob.SpouseId = jacobWife.Id;
            Assert.Empty(_dbConnection.People);
            _dbConnection.Insert(jacob);
            _dbConnection.Insert(jacobWife);
            _dbConnection.Insert(bob);
            _dbConnection.BulkCopy(faker.Generate(5));
            _dbConnection.Insert(jacob.Staff);
            _dbConnection.Insert(bob.Staff);
            var jacobGroup = AutoFaker.Generate<OrgGroup>();
            jacobGroup.Id = jacob.Staff.OrgGroupId ?? Guid.Empty;
            jacobGroup.Supervisor = bob.Id;
            jacobGroup.ApproverIsSupervisor = true;
            _dbConnection.Insert(jacobGroup);
        }

        public Faker<PersonWithStaff> PersonFaker() =>
            new AutoFaker<PersonWithStaff>()
                .RuleFor(p => p.Deleted, false)
                .RuleFor(extended => extended.StaffId, () => Guid.NewGuid())
                .RuleFor(extended => extended.Staff,
                    (f, extended) =>
                    {
                        var staff = AutoFaker.Generate<Staff>();
                        staff.Id = extended.StaffId ?? throw new NullReferenceException("staffId null");
                        return staff;
                    }).RuleSet("notStaff",
                    set =>
                    {
                        set.RuleFor(staff => staff.StaffId, (Guid?) null);
                        set.RuleFor(staff => staff.Staff, (Staff) null);
                    });

        public Faker<JobWithOrgGroup> JobFaker()
        {
            return new AutoFaker<JobWithOrgGroup>()
                .RuleFor(job => job.OrgGroupId, Guid.NewGuid)
                .RuleFor(
                    job => job.OrgGroup, (faker, job) =>
                    {
                        var org = AutoFaker.Generate<OrgGroup>();
                        org.Id = job.OrgGroupId;
                        return org;
                    });
        }

        public Job InsertJob()
        {
            var job = JobFaker().Generate();
            _dbConnection.Insert<Job>(job);
            _dbConnection.Insert(job.OrgGroup);
            return job;
        }

        public PersonWithStaff InsertPerson(bool includeStaff = true)
        {
            var person = PersonFaker().Generate(includeStaff ? "default" : "default,notStaff");
            _dbConnection.Insert(person);
            _dbConnection.Insert(person.Staff);

            return person;
        }

        public PersonRole InsertRole()
        {
            var role = AutoFaker.Generate<PersonRole>();
            _dbConnection.Insert(role);
            return role;
        }

        public void SetupData()
        {
            SetupPeople();
            var personFaker = PersonFaker();
            var leaveRequester = personFaker.Generate();
            _dbConnection.Insert(leaveRequester);
            _dbConnection.Insert(leaveRequester.Staff);
            var leaveApprover = personFaker.Generate();
            _dbConnection.Insert(leaveApprover);
            _dbConnection.Insert(leaveApprover.Staff);
            var leaveRequest = AutoFaker.Generate<LeaveRequest>();
            leaveRequest.PersonId = leaveRequester.Id;
            leaveRequest.ApprovedById = leaveApprover.Id;
            _dbConnection.Insert(leaveRequest);

            var personWithRole = personFaker.Generate();
            _dbConnection.Insert(personWithRole);
            _dbConnection.Insert(personWithRole.Staff);
            var personRoleFaker = new AutoFaker<PersonRole>().RuleFor(role => role.PersonId, personWithRole.Id);
            var personRoles = personRoleFaker.Generate(5);
            _dbConnection.BulkCopy(personRoles);
            var jobs = personRoles.Select(role => JobFaker().RuleFor(job => job.Id, role.JobId).Generate()).ToList();

            _dbConnection.BulkCopy<Job>(jobs);
            _dbConnection.BulkCopy(jobs.Select(job => job.OrgGroup));
            SetupTraining();
            _dbConnection.Insert(new AutoFaker<IdentityUser>().RuleFor(user => user.LockoutEnd, DateTimeOffset.Now)
                .Generate());

            var personWithEmergencyContact = personFaker.Generate("default,notStaff");
            _dbConnection.Insert(personWithEmergencyContact);
            var contactPerson = personFaker.Generate("default,notStaff");

            _dbConnection.Insert(contactPerson);
            var emergencyContact = AutoFaker.Generate<EmergencyContact>();
            emergencyContact.ContactId = contactPerson.Id;
            emergencyContact.PersonId = personWithEmergencyContact.Id;
            _dbConnection.Insert(emergencyContact);

            _dbConnection.Insert(new Attachment()
            {
                AttachedToId = Guid.NewGuid(),
                DownloadUrl = "someurl.com",
                FileType = "picture",
                GoogleId = "someRandomId123",
                Id = Guid.NewGuid(),
                Name = "hello attachments"
            });
        }

        public void SetupTraining()
        {
            var personFaker = PersonFaker();
            var personWithTraining = personFaker.Generate();
            var trainingRequirement = AutoFaker.Generate<TrainingRequirement>();
            trainingRequirement.FirstYear = 2015;
            trainingRequirement.LastYear = 2018;
            trainingRequirement.DepatmentId = personWithTraining.Staff.OrgGroupId;
            trainingRequirement.Scope = TrainingScope.Department;

            var orgGroup = AutoFaker.Generate<OrgGroup>();
            orgGroup.Id = personWithTraining.Staff.OrgGroupId ?? Guid.Empty;
            _dbConnection.Insert(orgGroup);

            _dbConnection.Insert(personWithTraining);
            _dbConnection.Insert(personWithTraining.Staff);
            _dbConnection.Insert(trainingRequirement);

            var staffTraining = AutoFaker.Generate<StaffTraining>();
            staffTraining.StaffId =
                personWithTraining.StaffId ?? throw new NullReferenceException("person staff id is null");
            staffTraining.TrainingRequirementId = trainingRequirement.Id;
            _dbConnection.Insert(staffTraining);
        }
    }

    [CollectionDefinition("ServicesCollection")]
    public class ServicesCollection : ICollectionFixture<ServicesFixture>
    {
    }
}