using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using LinqToDB.Linq;
using LinqToDB.Reflection;
using LinqToDB.SqlQuery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using SendGrid.Helpers.Mail;
using Serilog.Extensions.Logging;
using Xunit;
using Attachment = Backend.Entities.Attachment;
using DataExtensions = Backend.DataLayer.DataExtensions;
using IdentityUser = Backend.Entities.IdentityUser;

namespace UnitTestProject
{
    public class ServicesFixture : WebApplicationFactory<TestServerStartup>
    {
        public IWebHost WebHost { get; }
        public IServiceProvider ServiceProvider => WebHost.Services;
        public T Get<T>() => ServiceProvider.GetService<T>();
        public IDbConnection DbConnection { get; }
        public Mock<EmailService> EmailServiceMock => Mock.Get((EmailService) Get<IEmailService>());
        public Mock<EntityService> EntityServiceMock => Mock.Get((EntityService) Get<IEntityService>());

        public ServicesFixture()
        {
            var webHostBuilder = CreateWebHostBuilder();
            //todo config on startup not called, figure out work around
            ConfigureWebHost(webHostBuilder);
            WebHost = webHostBuilder.Build();
            DbConnection = Get<IDbConnection>();
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return new WebHostBuilder().UseStartup<TestServerStartup>().UseEnvironment("UnitTest");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(configBuilder =>
            {
                configBuilder.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("Environment", "UnitTest"),
                        new KeyValuePair<string, string>("JWTSettings:SecretKey",
                            "3C384CBA-393F-4192-A18D-8EF6543E5D01"),
                        new KeyValuePair<string, string>("JWTSettings:Issuer", "dotnet_gis"),
                        new KeyValuePair<string, string>("JWTSettings:Audience", "GisAPI"),
                        new KeyValuePair<string, string>("TemplateSettings:NotifyLeaveRequest", "abc"),
                        new KeyValuePair<string, string>("TemplateSettings:RequestLeaveApproval", "123"),
                        new KeyValuePair<string, string>("TemplateSettings:NotifyHrLeaveRequest", "123abc"),
                        new KeyValuePair<string, string>("web:client_id", "helloClient"),
                        new KeyValuePair<string, string>("web:client_secret", "i'm_A_Secret"),
                    }
                );
            }).ConfigureTestServices(collection => { collection.AddSingleton<IDbConnection, DbConnection>(); });
        }

        protected override void Dispose(bool disposing)
        {
            DbConnection.Dispose();
            base.Dispose(disposing);
        }

        public IdentityUser AuthenticateAs(HttpClient client, string userName)
        {
            var userService = Get<UserService>();
            var identityUser = userService.FindByNameAsync(userName).Result;
            var task = Get<JwtService>().GetJwtSecurityTokenAsString(identityUser);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", task.Result);
            return identityUser;
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

            var jacobDonor = AutoFaker.Generate<Donor>();
            jacob.DonorId = jacobDonor.Id;

            Assert.Empty(DbConnection.People);
            DbConnection.Insert(jacob);
            DbConnection.Insert(jacobWife);
            DbConnection.Insert(bob);
            DbConnection.BulkCopy(faker.Generate(5));
            DbConnection.Insert(jacob.Staff);
            DbConnection.Insert(bob.Staff);
            DbConnection.Insert(jacobDonor);
            InsertUser("jacob", jacob.Id, new[] {"admin"});
            var jacobGroup = AutoFaker.Generate<OrgGroup>();
            jacobGroup.Id = jacob.Staff.OrgGroupId ?? Guid.Empty;
            jacobGroup.Supervisor = bob.Id;
            jacobGroup.ApproverIsSupervisor = true;
            DbConnection.Insert(jacobGroup);
            var jacobMissionOrg = AutoFaker.Generate<MissionOrg>();
            jacobMissionOrg.Id = jacob.Staff.MissionOrgId ?? Guid.Empty;
            DbConnection.Insert(jacobMissionOrg);
            var jacobMissionOrgYear = AutoFaker.Generate<MissionOrgYearSummary>();
            jacobMissionOrgYear.MissionOrgId = jacobMissionOrg.Id;
            DbConnection.Insert(jacobMissionOrgYear);

            var jacobDonation = AutoFaker.Generate<Donation>();
            jacobDonation.PersonId = jacob.Id;
            jacobDonation.MissionOrgId = jacobMissionOrg.Id;
            DbConnection.Insert(jacobDonation);
            var jacobJob = InsertJob();
            InsertRole(role =>
            {
                role.PersonId = jacob.Id;
                role.JobId = jacobJob.Id;
            });

            //endorsments
            var endorsement = AutoFaker.Generate<Endorsement>();
            DbConnection.Insert(endorsement);
            var jacobEndorsement = AutoFaker.Generate<StaffEndorsement>();
            jacobEndorsement.EndorsementId = endorsement.Id;
            jacobEndorsement.PersonId = jacob.Id;
            DbConnection.Insert(jacobEndorsement);
            var requiredEndorsement = AutoFaker.Generate<RequiredEndorsement>();
            requiredEndorsement.EndorsementId = endorsement.Id;
            requiredEndorsement.JobId = jacobJob.Id;
            DbConnection.Insert(requiredEndorsement);

            var education = AutoFaker.Generate<Education>();
            education.PersonId = jacob.Id;
            DbConnection.Insert(education);
        }

        public void InsertUser(string userName,
            Guid? personId = null,
            string[] roles = null,
            bool sendHrLeaveEmails = false)
        {
            var userService = Get<UserService>();
            var identityUser = new IdentityUser
            {
                UserName = userName,
                ResetPassword = true,
                PersonId = personId,
                SendHrLeaveEmails = sendHrLeaveEmails
            };
            userService.CreateAsync(identityUser, "password").Wait();
            if (roles != null)
            {
                Task.WaitAll(roles.Select(role => userService.AddToRoleAsync(identityUser, role)).ToArray());
            }
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
                    job => job.OrgGroup,
                    (faker, job) =>
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
            IDbConnection dbConnection = Get<IDbConnection>();
            dbConnection.Insert<Job>(job);
            dbConnection.Insert(job.OrgGroup);
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
            IDbConnection dbConnection = Get<IDbConnection>();
            dbConnection.Insert(person);
            if (includeStaff)
                dbConnection.Insert<Staff>(person.Staff);

            return person;
        }

        public PersonWithStaff InsertStaff(Guid orgGroupId)
        {
            return InsertPerson(staff => staff.Staff.OrgGroupId = orgGroupId);
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

        public PersonRole InsertRole(Action<PersonRole> action = null)
        {
            var role = AutoFaker.Generate<PersonRole>();
            action?.Invoke(role);
            Get<IDbConnection>().Insert(role);
            return role;
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
            Get<IDbConnection>().Insert(leaveRequest);
            return leaveRequest;
        }

        public IdentityUser InsertUser(Action<IdentityUser> modify = null, params string[] roles)
        {
            var user = new AutoFaker<IdentityUser>()
                .RuleFor(identityUser => identityUser.LockoutEnd, DateTimeOffset.Now)
                .RuleFor(identityUser => identityUser.Id, 0)
                .Generate();
            modify?.Invoke(user);
            IDbConnection dbConnection = Get<IDbConnection>();
            user.Id = dbConnection.InsertId(user);
            Assert.True(user.Id > 0, $"{user.Id} > 0");
            if (roles.Any())
            {
                roles = roles.Select(s => s.ToUpper()).ToArray();
                var roleIds = dbConnection.Roles
                    .Where(role => roles.Contains(role.NormalizedName))
                    .Select(role => role.Id)
                    .ToList();
                foreach (var roleId in roleIds)
                {
                    dbConnection.InsertId(new IdentityUserRole<int> {RoleId = roleId, UserId = user.Id});
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
            Get<IDbConnection>().Insert(tr);
            return tr;
        }

        public StaffTraining InsertTraining(Guid trId, DateTime? completedDate = null)
        {
            var tr = AutoFaker.Generate<StaffTraining>();
            tr.TrainingRequirementId = trId;
            tr.CompletedDate = completedDate;
            Get<IDbConnection>().Insert(tr);
            return tr;
        }

        public OrgGroup InsertOrgGroup(Guid? parentId = null,
            Guid? supervisorId = null,
            Action<OrgGroup> action = null)
        {
            var orgGroup = AutoFaker.Generate<OrgGroup>();
            orgGroup.ParentId = parentId;
            orgGroup.Supervisor = supervisorId;
            action?.Invoke(orgGroup);
            Get<IDbConnection>().Insert(orgGroup);
            return orgGroup;
        }

        public void SetupData()
        {
            SetupPeople();
            var personFaker = PersonFaker();
            var leaveRequester = personFaker.Generate();
            IDbConnection dbConnection = Get<IDbConnection>();
            dbConnection.Insert(leaveRequester);
            var leaveRequesterOrgGroup = AutoFaker.Generate<OrgGroup>();
            leaveRequesterOrgGroup.Id = leaveRequester.Staff.OrgGroupId ??
                                        (Guid) (leaveRequester.Staff.OrgGroupId = Guid.NewGuid());
            dbConnection.Insert(leaveRequesterOrgGroup);
            dbConnection.Insert(leaveRequester.Staff);

            var leaveApprover = personFaker.Generate();
            dbConnection.Insert(leaveApprover);
            dbConnection.Insert(leaveApprover.Staff);
            var leaveRequest = AutoFaker.Generate<LeaveRequest>();
            leaveRequest.PersonId = leaveRequester.Id;
            leaveRequest.ApprovedById = leaveApprover.Id;
            dbConnection.Insert(leaveRequest);

            var personWithRole = personFaker.Generate();
            dbConnection.Insert(personWithRole);
            dbConnection.Insert(personWithRole.Staff);
            var personRoleFaker = new AutoFaker<PersonRole>().RuleFor(role => role.PersonId, personWithRole.Id);
            var personRoles = personRoleFaker.Generate(5);
            personRoles[0].Active = true; //always have at least one active
            dbConnection.BulkCopy(personRoles);
            var grade = AutoFaker.Generate<Grade>();
            dbConnection.Insert(grade);
            var jobs = personRoles.Select(role => JobFaker()
                .RuleFor(job => job.Id, role.JobId)
                .RuleFor(job => job.GradeId, grade.Id)
                .Generate()).ToList();
            var evaluation = AutoFaker.Generate<Evaluation>();
            evaluation.PersonId = personWithRole.Id;
            evaluation.Evaluator = leaveApprover.Id;
            evaluation.RoleId = personRoles[0].Id;
            dbConnection.Insert(evaluation);

            dbConnection.BulkCopy<Job>(jobs);
            dbConnection.BulkCopy(jobs.Select(job => job.OrgGroup));
            SetupTraining();
            InsertUser();

            var personWithEmergencyContact = personFaker.Generate("default,notStaff");
            dbConnection.Insert(personWithEmergencyContact);
            var contactPerson = personFaker.Generate("default,notStaff");

            dbConnection.Insert(contactPerson);
            var emergencyContact = AutoFaker.Generate<EmergencyContact>();
            emergencyContact.ContactId = contactPerson.Id;
            emergencyContact.PersonId = personWithEmergencyContact.Id;
            dbConnection.Insert(emergencyContact);

            dbConnection.Insert(new Attachment
            {
                AttachedToId = Guid.NewGuid(),
                DownloadUrl = "someurl.com",
                FileType = "picture",
                GoogleId = "someRandomId123",
                Id = Guid.NewGuid(),
                Name = "hello attachments"
            });

            dbConnection.Insert(AutoFaker.Generate<Grade>());
            dbConnection.Insert(AutoFaker.Generate<MissionOrg>());
            dbConnection.Insert(AutoFaker.Generate<IdentityRoleClaim<int>>());
            dbConnection.Insert(AutoFaker.Generate<IdentityUserClaim<int>>());
            dbConnection.Insert(AutoFaker.Generate<IdentityUserLogin<int>>());
            dbConnection.Insert(AutoFaker.Generate<IdentityUserRole<int>>());
            dbConnection.Insert(AutoFaker.Generate<IdentityUserToken<int>>());
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
            IDbConnection dbConnection = Get<IDbConnection>();
            dbConnection.Insert(orgGroup);

            dbConnection.Insert(personWithTraining);
            dbConnection.Insert(personWithTraining.Staff);
            dbConnection.Insert(trainingRequirement);

            var staffTraining = AutoFaker.Generate<StaffTraining>();
            staffTraining.StaffId =
                personWithTraining.StaffId ?? throw new NullReferenceException("person staff id is null");
            staffTraining.TrainingRequirementId = trainingRequirement.Id;
            dbConnection.Insert(staffTraining);
        }
    }

    [CollectionDefinition("ServicesCollection")]
    public class ServicesCollection : ICollectionFixture<ServicesFixture>
    {
    }
}