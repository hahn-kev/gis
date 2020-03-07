using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using IdentityUser = Backend.Entities.IdentityUser;

namespace UnitTestProject
{
    public class ServicesFixture :
        WebApplicationFactory<TestServerStartup>,
        IAsyncLifetime
    {
        public IWebHost WebHost { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }
        public T Get<T>() => ServiceProvider.GetService<T>();
        private readonly Lazy<IDbConnection> _lazyConnection;
        public IDbConnection DbConnection => _lazyConnection.Value;
        public Mock<EmailService> EmailServiceMock => Mock.Get((EmailService) Get<IEmailService>());
        public Mock<EntityService> EntityServiceMock => Mock.Get((EntityService) Get<IEntityService>());

        public ServicesFixture()
        {
            _lazyConnection = new Lazy<IDbConnection>(Get<IDbConnection>);
        }

        public void CreateWebHost(Action<IWebHostBuilder> action = null)
        {
            var webHostBuilder = CreateWebHostBuilder();
//            todo config on startup not called, figure out work around
            ConfigureWebHost(webHostBuilder);
            action?.Invoke(webHostBuilder);
            WebHost = webHostBuilder.Build();
            ServiceProvider = WebHost.Services;
        }

        public Mock<EmailService> GetScopedMockEmailService(IServiceScope scope)
        {
            return Mock.Get((EmailService) scope.ServiceProvider.GetService<IEmailService>());
        }

        public Mock<EntityService> GetScopedMockEntityService(IServiceScope scope)
        {
            return Mock.Get((EntityService) scope.ServiceProvider.GetService<IEntityService>());
        }

        private bool hasDoneBefore;

        public void DoOnce(Action<ServicesFixture> action)
        {
            if (!hasDoneBefore)
            {
                hasDoneBefore = true;
                action(this);
            }
        }

        public Task InitializeAsync()
        {
            return Setup();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        private async Task Setup()
        {
            CreateWebHost();

            TryCreateTable<IdentityUser>(DbConnection);
            TryCreateTable<IdentityUserClaim<int>>(DbConnection);
            TryCreateTable<IdentityUserLogin<int>>(DbConnection);
            TryCreateTable<IdentityUserToken<int>>(DbConnection);
            TryCreateTable<IdentityUserRole<int>>(DbConnection);
            TryCreateTable<IdentityRole<int>>(DbConnection);
            TryCreateTable<IdentityRoleClaim<int>>(DbConnection);
            TryCreateTable<PersonExtended>(DbConnection);
            TryCreateTable<PersonRole>(DbConnection);
            TryCreateTable<Job>(DbConnection);
            TryCreateTable<Grade>(DbConnection);
            TryCreateTable<Endorsement>(DbConnection);
            TryCreateTable<StaffEndorsement>(DbConnection);
            TryCreateTable<RequiredEndorsement>(DbConnection);
            TryCreateTable<Education>(DbConnection);
            TryCreateTable<OrgGroup>(DbConnection);
            TryCreateTable<LeaveRequest>(DbConnection);
            TryCreateTable<TrainingRequirement>(DbConnection);
            TryCreateTable<Staff>(DbConnection);
            TryCreateTable<StaffTraining>(DbConnection);
            TryCreateTable<EmergencyContact>(DbConnection);
            TryCreateTable<Donor>(DbConnection);
            TryCreateTable<Donation>(DbConnection);
            TryCreateTable<Evaluation>(DbConnection);
            TryCreateTable<Attachment>(DbConnection);
            TryCreateTable<MissionOrg>(DbConnection);
            TryCreateTable<MissionOrgYearSummary>(DbConnection);
            TryCreateTable<Holiday>(DbConnection);

            DbConnection.MappingSchema.SetConvertExpression<string, string[]>(
                s => s.Split(',', StringSplitOptions.RemoveEmptyEntries),
                true);
            DbConnection.MappingSchema.SetConvertExpression<string[], string>(s => string.Join(',', s));
            await Startup.SetupDatabase(Get<ILoggerFactory>(), ServiceProvider);
#if !DEBUG
            await Startup.SetupDevDatabase(ServiceProvider);
#endif
        }

        private void TryCreateTable<T>(IDbConnection dbConnection)
        {
            try
            {
                dbConnection.CreateTable<T>();
            }
            catch (SqliteException e) when (e.SqliteErrorCode == 1) //already exists code I think
            {
            }
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return new WebHostBuilder().UseStartup<TestServerStartup>().UseEnvironment("UnitTest");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
#if Debug
            builder.ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Trace).AddDebug().AddConsole();
            });
#else
            builder.ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Error).AddDebug().AddConsole();
            });
#endif
            builder.ConfigureAppConfiguration(SetupConfig);
        }

        private void SetupConfig(IConfigurationBuilder configBuilder)
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
                new KeyValuePair<string, string>("web:client_secret", "i'm_A_Secret")
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (_lazyConnection.IsValueCreated) DbConnection.Dispose();
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

        public PersonWithStaff Jacob { get; private set; }
        public PersonWithStaff JacobSupervisor { get; private set; }

        public void SetupPeople()
        {
            if (Jacob != null) return;
            var faker = PersonFaker();
            Jacob = faker.Generate();
            Jacob.FirstName = "Jacob";
            JacobSupervisor = faker.Generate();
            JacobSupervisor.FirstName = "Bob";
            var jacobWife = faker.Generate();
            jacobWife.SpouseId = Jacob.Id;
            Jacob.SpouseId = jacobWife.Id;

            var jacobDonor = AutoFaker.Generate<Donor>();
            Jacob.DonorId = jacobDonor.Id;

            Assert.Empty(DbConnection.People);
            DbConnection.Insert(Jacob);
            DbConnection.Insert(jacobWife);
            DbConnection.Insert(JacobSupervisor);
            DbConnection.BulkCopy(faker.Generate(5));
            DbConnection.Insert(Jacob.Staff);
            DbConnection.Insert(JacobSupervisor.Staff);
            DbConnection.Insert(jacobDonor);
            InsertUser("jacob", Jacob.Id, new[] {"admin"});

            var jacobParentGroup = AutoFaker.Generate<OrgGroup>();
            jacobParentGroup.Id = Guid.NewGuid();
            jacobParentGroup.Supervisor = InsertPerson().Id;
            jacobParentGroup.ApproverIsSupervisor = true;
            DbConnection.Insert(jacobParentGroup);

            var jacobGroup = AutoFaker.Generate<OrgGroup>();
            jacobGroup.Id = Jacob.Staff.OrgGroupId ?? Guid.Empty;
            jacobGroup.ParentId = jacobParentGroup.Id;
            jacobGroup.Supervisor = JacobSupervisor.Id;
            jacobGroup.ApproverIsSupervisor = true;
            DbConnection.Insert(jacobGroup);


            var jacobMissionOrg = AutoFaker.Generate<MissionOrg>();
            jacobMissionOrg.Id = Jacob.Staff.MissionOrgId ?? Guid.Empty;
            DbConnection.Insert(jacobMissionOrg);
            var jacobMissionOrgYear = AutoFaker.Generate<MissionOrgYearSummary>();
            jacobMissionOrgYear.MissionOrgId = jacobMissionOrg.Id;
            DbConnection.Insert(jacobMissionOrgYear);

            var jacobDonation = AutoFaker.Generate<Donation>();
            jacobDonation.PersonId = Jacob.Id;
            jacobDonation.MissionOrgId = jacobMissionOrg.Id;
            DbConnection.Insert(jacobDonation);
            var jacobJob = InsertJob(job =>
            {
                job.OrgGroup = null;
                job.OrgGroupId = jacobGroup.Id;
            });
            InsertRole(role =>
            {
                role.PersonId = Jacob.Id;
                role.JobId = jacobJob.Id;
            });

            //endorsments
            var endorsement = AutoFaker.Generate<Endorsement>();
            DbConnection.Insert(endorsement);
            var jacobEndorsement = AutoFaker.Generate<StaffEndorsement>();
            jacobEndorsement.EndorsementId = endorsement.Id;
            jacobEndorsement.PersonId = Jacob.Id;
            DbConnection.Insert(jacobEndorsement);
            var requiredEndorsement = AutoFaker.Generate<RequiredEndorsement>();
            requiredEndorsement.EndorsementId = endorsement.Id;
            requiredEndorsement.JobId = jacobJob.Id;
            DbConnection.Insert(requiredEndorsement);

            var education = AutoFaker.Generate<Education>();
            education.PersonId = Jacob.Id;
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

        public static Faker<PersonWithStaff> PersonFaker() =>
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
            DbConnection.Insert<Job>(job);
            if (job.OrgGroup != null)
                DbConnection.Insert(job.OrgGroup);
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
            DbConnection.Insert(person);
            if (includeStaff)
                DbConnection.Insert<Staff>(person.Staff);

            return person;
        }

        public PersonWithStaff InsertStaff(Guid orgGroupId)
        {
            return InsertPerson(staff => staff.Staff.OrgGroupId = orgGroupId);
        }

        public PersonRole InsertRole(Guid jobId, Guid personId = default, int years = 2, bool active = true)
        {
            return InsertRole(role =>
            {
                role.JobId = jobId;
                role.PersonId = personId;
                role.StartDate = DateTime.Now - TimeSpan.FromDays(years * 366);
                role.Active = active;
            });
        }

        public (PersonRole role, JobWithOrgGroup job) InsertRoleAndJob(Guid personId,
            int years = 2,
            bool active = true,
            Guid jobOrgGroupId = default)
        {
            var job = InsertJob(_ =>
            {
                if (jobOrgGroupId != default)
                {
                    _.OrgGroup = null;
                    _.OrgGroupId = jobOrgGroupId;
                }
            });
            return (InsertRole(job.Id, personId, years, active), job);
        }

        public PersonRole InsertRole(Action<PersonRole> action = null)
        {
            var role = AutoFaker.Generate<PersonRole>();
            role.Active = true;
            action?.Invoke(role);
            if (role.StartDate == default)
            {
                role.StartDate = DateTime.Now.AddYears(-2);
            }

            if (!role.Active && !role.EndDate.HasValue)
            {
                role.EndDate = role.StartDate.AddYears(1);
            }

            DbConnection.Insert(role);
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
            DbConnection.Insert(leaveRequest);
            return leaveRequest;
        }

        public IdentityUser InsertUser(Action<IdentityUser> modify = null, params string[] roles)
        {
            var user = new AutoFaker<IdentityUser>()
                .RuleFor(identityUser => identityUser.LockoutEnd, DateTimeOffset.Now)
                .RuleFor(identityUser => identityUser.Id, 0)
                .Generate();
            modify?.Invoke(user);
            user.Id = DbConnection.InsertId(user);
            Assert.True(user.Id > 0, $"{user.Id} > 0");
            if (roles.Any())
            {
                roles = roles.Select(s => s.ToUpper()).ToArray();
                var roleIds = DbConnection.Roles
                    .Where(role => roles.Contains(role.NormalizedName))
                    .Select(role => role.Id)
                    .ToList();
                foreach (var roleId in roleIds)
                {
                    DbConnection.InsertId(new IdentityUserRole<int> {RoleId = roleId, UserId = user.Id});
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
            DbConnection.Insert(tr);
            return tr;
        }

        public StaffTraining InsertTraining(Guid trId, DateTime? completedDate = null)
        {
            var tr = AutoFaker.Generate<StaffTraining>();
            tr.TrainingRequirementId = trId;
            tr.CompletedDate = completedDate;
            DbConnection.Insert(tr);
            return tr;
        }

        public OrgGroup InsertOrgGroup(Guid? parentId = null,
            Guid? supervisorId = null,
            Action<OrgGroup> action = null,
            bool approvesLeave = false,
            string name = null)
        {
            var orgGroup = AutoFaker.Generate<OrgGroup>();
            orgGroup.ParentId = parentId;
            orgGroup.Supervisor = supervisorId;
            orgGroup.ApproverIsSupervisor = approvesLeave;
            if (!string.IsNullOrEmpty(name)) orgGroup.GroupName = name;
            action?.Invoke(orgGroup);
            DbConnection.Insert(orgGroup);
            return orgGroup;
        }

        public static (OrgGroupWithSupervisor root, OrgGroupWithSupervisor a, OrgGroupWithSupervisor aa,
            OrgGroupWithSupervisor ab, OrgGroupWithSupervisor aaa) MakeGroupTree()
        {
            OrgGroupWithSupervisor MakeGroup(string name, Guid? parent, out Guid groupId)
            {
                groupId = Guid.NewGuid();
                var staffId = Guid.NewGuid();
                Guid supervisor = Guid.NewGuid();
                return new OrgGroupWithSupervisor
                {
                    GroupName = name, Id = groupId, ParentId = parent, Supervisor = supervisor,
                    SupervisorPerson = new PersonWithStaff
                    {
                        FirstName = name + " group super",
                        Id = supervisor,
                        StaffId = staffId,
                        Staff = new StaffWithOrgName
                        {
                            Id = staffId,
                            OrgGroupId = parent,
                            Email = name + " staff email"
                        }
                    }
                };
            }

            return (MakeGroup("root", null, out var rootId),
                MakeGroup("a", rootId, out var aId),
                MakeGroup("aa", aId, out var aaId), MakeGroup("ab", aId, out _),
                MakeGroup("aaa", aaId, out _));
        }

        public void SetupData()
        {
            SetupPeople();
            var personFaker = PersonFaker();
            var leaveRequester = personFaker.Generate();
            DbConnection.Insert(leaveRequester);
            var leaveRequesterOrgGroup = AutoFaker.Generate<OrgGroup>();
            leaveRequesterOrgGroup.Id = leaveRequester.Staff.OrgGroupId ??
                                        (Guid) (leaveRequester.Staff.OrgGroupId = Guid.NewGuid());
            DbConnection.Insert(leaveRequesterOrgGroup);
            DbConnection.Insert(leaveRequester.Staff);

            var leaveApprover = personFaker.Generate();
            DbConnection.Insert(leaveApprover);
            DbConnection.Insert(leaveApprover.Staff);
            var leaveRequest = AutoFaker.Generate<LeaveRequest>();
            leaveRequest.PersonId = leaveRequester.Id;
            leaveRequest.ApprovedById = leaveApprover.Id;
            DbConnection.Insert(leaveRequest);

            var personWithRole = personFaker.Generate();
            DbConnection.Insert(personWithRole);
            DbConnection.Insert(personWithRole.Staff);
            var personRoleFaker = new AutoFaker<PersonRole>().RuleFor(role => role.PersonId, personWithRole.Id);
            var personRoles = personRoleFaker.Generate(5);
            personRoles[0].Active = true; //always have at least one active
            DbConnection.BulkCopy(personRoles);
            var grade = AutoFaker.Generate<Grade>();
            DbConnection.Insert(grade);
            var jobs = personRoles.Select(role => JobFaker()
                .RuleFor(job => job.Id, role.JobId)
                .RuleFor(job => job.GradeId, grade.Id)
                .Generate()).ToList();
            var evaluation = AutoFaker.Generate<Evaluation>();
            evaluation.PersonId = personWithRole.Id;
            evaluation.Evaluator = leaveApprover.Id;
            evaluation.RoleId = personRoles[0].Id;
            DbConnection.Insert(evaluation);

            DbConnection.BulkCopy<Job>(jobs);
            DbConnection.BulkCopy(jobs.Select(job => job.OrgGroup));
            SetupTraining();
            InsertUser();

            var personWithEmergencyContact = personFaker.Generate("default,notStaff");
            DbConnection.Insert(personWithEmergencyContact);
            var contactPerson = personFaker.Generate("default,notStaff");

            DbConnection.Insert(contactPerson);
            var emergencyContact = AutoFaker.Generate<EmergencyContact>();
            emergencyContact.ContactId = contactPerson.Id;
            emergencyContact.PersonId = personWithEmergencyContact.Id;
            DbConnection.Insert(emergencyContact);

            DbConnection.Insert(new Attachment
            {
                AttachedToId = Guid.NewGuid(),
                DownloadUrl = "someurl.com",
                FileType = "picture",
                GoogleId = "someRandomId123",
                Id = Guid.NewGuid(),
                Name = "hello attachments"
            });

            DbConnection.Insert(AutoFaker.Generate<Grade>());
            DbConnection.Insert(AutoFaker.Generate<MissionOrg>());
            DbConnection.Insert(AutoFaker.Generate<Holiday>());
            DbConnection.Insert(AutoFaker.Generate<IdentityRoleClaim<int>>());
            DbConnection.Insert(AutoFaker.Generate<IdentityUserClaim<int>>());
            DbConnection.Insert(AutoFaker.Generate<IdentityUserLogin<int>>());
            DbConnection.Insert(AutoFaker.Generate<IdentityUserRole<int>>());
            DbConnection.Insert(AutoFaker.Generate<IdentityUserToken<int>>());
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
            DbConnection.Insert(orgGroup);

            DbConnection.Insert(personWithTraining);
            DbConnection.Insert(personWithTraining.Staff);
            DbConnection.Insert(trainingRequirement);

            var staffTraining = AutoFaker.Generate<StaffTraining>();
            staffTraining.StaffId =
                personWithTraining.StaffId ?? throw new NullReferenceException("person staff id is null");
            staffTraining.TrainingRequirementId = trainingRequirement.Id;
            DbConnection.Insert(staffTraining);
        }

        public (Staff staff, PersonExtended person) InsertStaff(Guid? personId = null, Guid? orgGroupId = null)
        {
            var person = PersonFaker().Generate();
            var staff = person.Staff;
            staff.OrgGroupId = orgGroupId;
            DbConnection.Insert(staff);
            person.StaffId = staff.Id;
            person.Id = personId ?? Guid.NewGuid();
            DbConnection.Insert(person);
            return (staff, person);
        }

        public (LeaveRequest request, PersonWithStaff requester, PersonWithStaff supervisor) SetupLeaveRequest(
            Action<PersonWithStaff> modifyRequester)
        {
            var supervisor = InsertPerson(true);
            var orgGroup = InsertOrgGroup(supervisorId: supervisor.Id);
            var requester = InsertPerson(p =>
            {
                modifyRequester(p);
                p.Staff.OrgGroupId = orgGroup.Id;
            });
            var request = InsertLeaveRequest(LeaveType.Sick, requester.Id, 5);
            return (request, requester, supervisor);
        }
    }
}