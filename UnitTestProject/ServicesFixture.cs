using System;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
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
            DbConnection.Setup();
        }

        public void SetupPeople()
        {
            var faker = PersonFaker();
            var jacob = faker.Generate();
            jacob.FirstName = "Jacob";
            var bob = faker.Generate();
            bob.FirstName = "Bob";
            Assert.Empty(_dbConnection.People);
            _dbConnection.Insert(jacob);
            _dbConnection.Insert(bob);
            _dbConnection.BulkCopy(faker.Generate(5));
            _dbConnection.Insert(jacob.Staff);
            _dbConnection.Insert(bob.Staff);
            var jacobGroup = AutoFaker.Generate<OrgGroup>();
            jacobGroup.Id = jacob.Staff.OrgGroupId;
            jacobGroup.Supervisor = bob.Id;
            jacobGroup.ApproverIsSupervisor = true;
            _dbConnection.Insert(jacobGroup);
        }

        public Faker<PersonWithStaff> PersonFaker() =>
            new AutoFaker<PersonWithStaff>()
                .RuleFor(extended => extended.StaffId, f => Guid.NewGuid())
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
            _dbConnection.BulkCopy(personRoleFaker.Generate(5));
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
            orgGroup.Id = personWithTraining.Staff.OrgGroupId;
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