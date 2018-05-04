using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoBogus;
using Backend;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Bogus;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using SendGrid.Helpers.Mail;
using Xunit;
using Attachment = Backend.Entities.Attachment;
using DataExtensions = Backend.DataLayer.DataExtensions;
using IdentityUser = Backend.Entities.IdentityUser;

namespace UnitTestProject
{
    public class ServicesFixture
    {
        public ServiceProvider ServiceProvider { get; }
        public IServiceCollection ServiceCollection { get; }
        public T Get<T>() => ServiceProvider.GetService<T>();
        private IDbConnection _dbConnection;
        public IDbConnection DbConnection => _dbConnection ?? (_dbConnection = Get<IDbConnection>());
        public Mock<EmailService> EmailServiceMock => Mock.Get((EmailService) Get<IEmailService>());
        public Mock<EntityService> EntityServiceMock => Mock.Get((EntityService) Get<IEntityService>());

        public ServicesFixture(Action<IServiceCollection> configure = null)
        {
            ServiceCollection = new ServiceCollection();
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Environment", "UnitTest"),
                    new KeyValuePair<string, string>("JWTSettings:SecretKey", "helloWorld"),
                    new KeyValuePair<string, string>("TemplateSettings:NotifyLeaveRequest", "abc"),
                    new KeyValuePair<string, string>("TemplateSettings:RequestLeaveApproval", "123"),
                    new KeyValuePair<string, string>("TemplateSettings:NotifyHrLeaveRequest", "123abc"),
                }
            );
            var startup = new Startup(builder.Build());
            ServiceCollection.AddLogging(loggingBuilder => loggingBuilder.AddConsole().AddDebug());
            startup.ConfigureServices(ServiceCollection);
            ServiceCollection.Replace(ServiceDescriptor.Singleton<IEmailService>(provider =>
            {
                var esm = new Mock<EmailService>(provider.GetService<IOptions<Settings>>(),
                    provider.GetService<IOptions<TemplateSettings>>());
                esm.CallBase = true;
                esm.Setup(email => email.SendEmail(It.IsAny<SendGridMessage>())).Returns(Task.CompletedTask);
                return esm.Object;
            }));
            ServiceCollection.Replace(ServiceDescriptor.Singleton<IEntityService>(provider =>
            {
                var esm = new Mock<EntityService>(provider.GetService<IDbConnection>());
//                esm.CallBase = true;
                return esm.Object;
            }));

            configure?.Invoke(ServiceCollection);
            ServiceProvider = ServiceCollection.BuildServiceProvider();
            startup.ConfigureDatabase(ServiceProvider, new ConsoleLogger("db", (s, level) => true, false));
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
            TryCreateTable<Grade>();
            TryCreateTable<OrgGroup>();
            TryCreateTable<LeaveRequest>();
            TryCreateTable<TrainingRequirement>();
            TryCreateTable<Staff>();
            TryCreateTable<StaffTraining>();
            TryCreateTable<EmergencyContact>();
            TryCreateTable<Evaluation>();
            TryCreateTable<Attachment>();
            TryCreateTable<MissionOrg>();

            var roles = new[] {"admin", "hr", "hradmin"};
            var existingRoles = DbConnection.Roles.Select(role => role.Name).ToArray();
            foreach (var role in roles.Except(existingRoles))
            {
                DbConnection.InsertId(
                    new IdentityRole<int>(role) {NormalizedName = role.ToUpper()});
            }

            LinqToDB.Linq.Expressions.MapMember<DateTime, int, DateTime>(ProviderName.SQLiteMS,
                (date, months) => date.AddMonths(months),
                (date, months) => DataExtensions.AddMonths(date, months));
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
            var jacobMissionOrg = AutoFaker.Generate<MissionOrg>();
            jacobMissionOrg.Id = jacob.Staff.MissionOrgId ?? Guid.Empty;
            _dbConnection.Insert(jacobMissionOrg);
            var jacobJob = InsertJob();
            InsertRole(role =>
            {
                role.PersonId = jacob.Id;
                role.JobId = jacobJob.Id;
            });
        }

        public Faker<PersonWithStaff> PersonFaker() =>
            new AutoFaker<PersonWithStaff>()
                .RuleFor(p => p.Deleted, false)
                .RuleFor(extended => extended.StaffId, () => Guid.NewGuid())
                .RuleFor(extended => extended.Staff,
                    (f, extended) =>
                    {
                        var staff = AutoFaker.Generate<StaffWithOrgName>();
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

        public JobWithOrgGroup InsertStaffJob(Action<JobWithOrgGroup> action = null)
        {
            return InsertJob(job =>
            {
                job.Status = JobStatus.FullTime;
                job.OrgGroup.Type = GroupType.Department;
                action?.Invoke(job);
            });
        }

        public JobWithOrgGroup InsertJob(Action<JobWithOrgGroup> action = null)
        {
            var job = JobFaker().Generate();
            action?.Invoke(job);
            _dbConnection.Insert<Job>(job);
            _dbConnection.Insert(job.OrgGroup);
            return job;
        }

        public PersonWithStaff InsertPerson(bool includeStaff)
        {
            return InsertPerson(null, includeStaff);
        }

        public PersonWithStaff InsertPerson(Action<PersonWithStaff> action = null, bool includeStaff = true)
        {
            var person = PersonFaker().Generate(includeStaff ? "default" : "default,notStaff");
            action?.Invoke(person);
            _dbConnection.Insert(person);
            if (includeStaff)
                _dbConnection.Insert<Staff>(person.Staff);

            return person;
        }

        public PersonRole InsertRole(Guid jobId, Guid personId = default(Guid), int years = 2, bool active = true)
        {
            return InsertRole(role =>
            {
                role.JobId = jobId;
                role.PersonId = personId;
                role.StartDate = DateTime.Now - TimeSpan.FromDays(years * 366);
                role.Active = active;
            });
        }

        public LeaveRequest InsertLeaveRequest(LeaveType leaveType, Guid personId, int days)
        {
            var leaveRequest = AutoFaker.Generate<LeaveRequest>();
            leaveRequest.Approved = null;
            leaveRequest.Type = leaveType;
            leaveRequest.PersonId = personId;
            leaveRequest.Days = days;
            leaveRequest.OverrideDays = true;
            leaveRequest.StartDate = DateTime.Now;
            leaveRequest.EndDate = DateTime.Now + TimeSpan.FromDays(4);
            _dbConnection.Insert(leaveRequest);
            return leaveRequest;
        }

        public PersonRole InsertRole(Action<PersonRole> action = null)
        {
            var role = AutoFaker.Generate<PersonRole>();
            action?.Invoke(role);
            _dbConnection.Insert(role);
            return role;
        }

        public IdentityUser InsertUser(Action<IdentityUser> modify = null, params string[] roles)
        {
            var user = new AutoFaker<IdentityUser>()
                .RuleFor(identityUser => identityUser.LockoutEnd, DateTimeOffset.Now)
                .RuleFor(identityUser => identityUser.Id, 0)
                .Generate();
            modify?.Invoke(user);
            user.Id = _dbConnection.InsertId(user);
            Assert.True(user.Id > 0, $"{user.Id} > 0");
            if (roles.Any())
            {
                roles = roles.Select(s => s.ToUpper()).ToArray();
                var roleIds = _dbConnection.Roles
                    .Where(role => roles.Contains(role.NormalizedName))
                    .Select(role => role.Id)
                    .ToList();
                foreach (var roleId in roleIds)
                {
                    _dbConnection.InsertId(new IdentityUserRole<int> {RoleId = roleId, UserId = user.Id});
                }
            }

            return user;
        }

        public TrainingRequirement InsertRequirement(int firstYear = 2017, int? lastYear = null, int months = 12)
        {
            var tr = AutoFaker.Generate<TrainingRequirement>();
            tr.FirstYear = firstYear;
            tr.LastYear = lastYear;
            tr.RenewMonthsCount = months;
            _dbConnection.Insert(tr);
            return tr;
        }

        public StaffTraining InsertTraining(Guid trId, DateTime? completedDate = null)
        {
            var tr = AutoFaker.Generate<StaffTraining>();
            tr.TrainingRequirementId = trId;
            tr.CompletedDate = completedDate;
            _dbConnection.Insert(tr);
            return tr;
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
            personRoles[0].Active = true; //always have at least one active
            _dbConnection.BulkCopy(personRoles);
            var grade = AutoFaker.Generate<Grade>();
            _dbConnection.Insert(grade);
            var jobs = personRoles.Select(role => JobFaker()
                .RuleFor(job => job.Id, role.JobId)
                .RuleFor(job => job.GradeId, grade.Id)
                .Generate()).ToList();
            var evaluation = AutoFaker.Generate<Evaluation>();
            evaluation.PersonId = personWithRole.Id;
            evaluation.Evaluator = leaveApprover.Id;
            evaluation.RoleId = personRoles[0].Id;
            _dbConnection.Insert(evaluation);
            
            _dbConnection.BulkCopy<Job>(jobs);
            _dbConnection.BulkCopy(jobs.Select(job => job.OrgGroup));
            SetupTraining();
            InsertUser();

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

            _dbConnection.Insert(AutoFaker.Generate<Grade>());
            _dbConnection.Insert(AutoFaker.Generate<MissionOrg>());
            _dbConnection.Insert(AutoFaker.Generate<IdentityRoleClaim<int>>());
            _dbConnection.Insert(AutoFaker.Generate<IdentityUserClaim<int>>());
            _dbConnection.Insert(AutoFaker.Generate<IdentityUserLogin<int>>());
            _dbConnection.Insert(AutoFaker.Generate<IdentityUserRole<int>>());
            _dbConnection.Insert(AutoFaker.Generate<IdentityUserToken<int>>());
        }

        public void SetupTraining()
        {
            var personFaker = PersonFaker();
            var personWithTraining = personFaker.Generate();
            var trainingRequirement = AutoFaker.Generate<TrainingRequirement>();
            trainingRequirement.FirstYear = 2015;
            trainingRequirement.LastYear = 2018;
            trainingRequirement.DepartmentId = personWithTraining.Staff.OrgGroupId;
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