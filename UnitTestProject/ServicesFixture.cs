using System;
using System.Diagnostics;
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

        public Faker<PersonExtended> PersonFaker() =>
            new AutoFaker<PersonExtended>()
                .RuleFor(extended => extended.StaffId, f => Guid.NewGuid())
                .RuleFor(extended => extended.Staff,
                    (f, extended) =>
                        new Staff {Id = extended.StaffId ?? throw new NullReferenceException("staffId null")});
    }

    [CollectionDefinition("ServicesCollection")]
    public class ServicesCollection : ICollectionFixture<ServicesFixture>
    {
    }
}