using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoBogus;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Backend.Utils;
using Bogus.DataSets;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using SendGrid.Helpers.Mail;
using Shouldly;
using Xunit;

namespace UnitTestProject
{
    public class LeaveRequestTests : IClassFixture<ServicesFixture>, IDisposable
    {
        private LeaveService _leaveService;
        private OrgGroupRepository _orgGroupRepository;
        private ServicesFixture _sf;
        private DataConnectionTransaction _transaction;

        public LeaveRequestTests(ServicesFixture sf)
        {
            _sf = sf;
            _leaveService = _sf.Get<LeaveService>();
            _orgGroupRepository = _sf.Get<OrgGroupRepository>();
            _sf.DoOnce(fixture => fixture.SetupPeople());
            _transaction = _sf.DbConnection.BeginTransaction();
        }

        [Fact]
        public async Task FindsSupervisor()
        {
            var jacob = _sf.DbConnection.People.FirstOrDefault(person => person.FirstName == "Jacob");
            Assert.NotNull(jacob);
            var expectedSupervisor = _sf.DbConnection.People.FirstOrDefault(person => person.FirstName == "Bob");
            Assert.NotNull(expectedSupervisor);
            var actualSupervisor = await _leaveService.RequestLeave(new LeaveRequest
                {PersonId = jacob.Id, StartDate = DateTime.Now});
            Assert.NotNull(actualSupervisor);
            Assert.Equal(expectedSupervisor.FirstName, actualSupervisor.FirstName);
        }

        [Fact]
        public static async Task SendsEmail()
        {
            var sf = new ServicesFixture();
            await sf.InitializeAsync();
            sf.SetupPeople();
            var leaveService = sf.Get<LeaveService>();

            var emailMock = sf.EmailServiceMock;
            emailMock.Setup(service => service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                It.IsAny<string>(),
                It.IsAny<EmailTemplate>(),
                It.IsAny<PersonWithStaff>(),
                It.IsAny<PersonWithStaff>())).Returns(Task.CompletedTask);
            var jacob = sf.DbConnection.People.FirstOrDefault(person => person.FirstName == "Jacob");
            Assert.NotNull(jacob);

            await leaveService.RequestLeave(new LeaveRequest {PersonId = jacob.Id, StartDate = DateTime.Now});

            emailMock.Verify(service =>
                    service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                        It.IsAny<string>(),
                        It.Is<EmailTemplate>(template =>
                            template == EmailTemplate.RequestLeaveApproval),
                        It.Is<PersonWithStaff>(extended => extended.FirstName == "Jacob"),
                        It.Is<PersonWithStaff>(extended => extended.FirstName == "Bob")),
                Times.Once);

            emailMock.Verify(service =>
                    service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                        It.IsAny<string>(),
                        It.Is<EmailTemplate>(template => template == EmailTemplate.NotifyLeaveRequest),
                        It.IsAny<PersonWithStaff>(),
                        It.IsAny<PersonWithStaff>()),
                Times.Never);
        }

        [Fact]
        public static async Task SavesLeaveRequest()
        {
            var sf = new ServicesFixture();
            await sf.InitializeAsync();
            sf.SetupPeople();
            var leaveService = sf.Get<LeaveService>();

            var jacob = sf.DbConnection.People.FirstOrDefault(person => person.FirstName == "Jacob");
            Assert.NotNull(jacob);
            var expectedLeaveRequest = new LeaveRequest {PersonId = jacob.Id, StartDate = DateTime.Now};
            await leaveService.RequestLeave(expectedLeaveRequest);
            sf.EntityServiceMock.Verify(service => service.Save(It.IsAny<LeaveRequest>()), Times.Once);
        }

        [Fact]
        public static async Task EmailThrowingAnErrorWillCauseALeaveRequestToBeDeleted()
        {
            var sf = new ServicesFixture();
            await sf.InitializeAsync();
            sf.SetupPeople();

            var leaveService = sf.Get<LeaveService>();

            var expectedException = new Exception("email test exception");
            sf.EmailServiceMock
                .Setup(email => email.SendEmail(It.IsAny<SendGridMessage>()))
                .Throws(expectedException);
            var jacob = sf.DbConnection.People.FirstOrDefault(person => person.FirstName == "Jacob");
            Assert.NotNull(jacob);
            var expectedLeaveRequest = new LeaveRequest {PersonId = jacob.Id, StartDate = DateTime.Now};
            var actualException =
                await Assert.ThrowsAsync<Exception>(() => leaveService.RequestLeave(expectedLeaveRequest));
            Assert.Same(expectedException, actualException);
            sf.EntityServiceMock.Verify(service => service.Save(expectedLeaveRequest), Times.Once);
            sf.EntityServiceMock.Verify(service => service.Delete(expectedLeaveRequest), Times.Once);
        }

        [Fact]
        public void OrgGroupChainResolvesAsExpected()
        {
            var personFaker = ServicesFixture.PersonFaker();
            var expectedPersonOnLeave = personFaker.Generate();
            var expectedDepartment = AutoFaker.Generate<OrgGroup>();
            var expectedDevision = AutoFaker.Generate<OrgGroup>();
            var expectedDevisionSupervisor = personFaker.Generate();

            expectedPersonOnLeave.Staff.OrgGroupId = expectedDepartment.Id;

            expectedDepartment.Supervisor = null;
            expectedDepartment.ParentId = expectedDevision.Id;

            expectedDevision.Supervisor = expectedDevisionSupervisor.Id;
            expectedDevision.ParentId = null;

            _sf.DbConnection.Insert(expectedPersonOnLeave);
            _sf.DbConnection.Insert(expectedPersonOnLeave.Staff);
            _sf.DbConnection.Insert(expectedDepartment);
            _sf.DbConnection.Insert(expectedDevision);
            _sf.DbConnection.Insert(expectedDevisionSupervisor);
            _sf.DbConnection.Insert(expectedDevisionSupervisor.Staff);

            //test method
            var orgGroupWithSupervisors = _orgGroupRepository.StaffParentOrgGroups(expectedPersonOnLeave.Staff)
                .Take(3)
                .AsEnumerable()
                .Concat(new OrgGroupWithSupervisor[3]).ToList();

            OrgGroupWithSupervisor actualDepartment = orgGroupWithSupervisors[0];
            OrgGroupWithSupervisor actualDevision = orgGroupWithSupervisors[1];
            OrgGroupWithSupervisor actualSupervisorGroup = orgGroupWithSupervisors[2];

            Assert.NotNull(actualDepartment);
            Assert.Equal(expectedDepartment.Id, actualDepartment.Id);
            Assert.Null(actualDepartment.SupervisorPerson);
            Assert.NotNull(actualDevision);
            Assert.Equal(expectedDevision.Id, actualDevision.Id);
            Assert.Equal(expectedDevisionSupervisor.Id, actualDevision.SupervisorPerson.Id);
            Assert.Null(actualSupervisorGroup);
        }

        public static IEnumerable<object[]> GetExpectedNotifyHrValues()
        {
            Guid personId = Guid.NewGuid();

            PersonWithStaff P()
            {
                Guid staffId = Guid.NewGuid();
                return new PersonWithStaff
                {
                    IsThai = true,
                    Id = personId,
                    StaffId = staffId,
                    Staff = new StaffWithOrgName {Id = staffId}
                };
            }

            LeaveRequest LR(LeaveType type, decimal days = 1)
            {
                personId = Guid.NewGuid();
                return new LeaveRequest
                {
                    Type = type,
                    PersonId = personId,
                    Days = days,
                    OverrideDays = true,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now + TimeSpan.FromDays((double) days)
                };
            }

            IEnumerable<(LeaveRequest, PersonWithStaff, int usedLeave, int totalYears, bool sendEmail)> GetValues()
            {
                yield return (LR(LeaveType.Sick), P(), 10, 2, false);
                yield return (LR(LeaveType.Emergency, 5), P(), 1, 2, true);
                yield return (LR(LeaveType.Other), P(), 0, 2, true);
                yield return (LR(LeaveType.SchoolRelated), P(), 0, 2, true);
                yield return (LR(LeaveType.Vacation, 5), P(), 10, 9, true);
                yield return (LR(LeaveType.Vacation, 5), P(), 10, 14, false);
                yield return (LR(LeaveType.Vacation, 5), P(), 1, 10, false);
            }

            return GetValues().Select(tuple => tuple.ToArray());
        }

        [Theory]
        [MemberData(nameof(GetExpectedNotifyHrValues))]
        public void NotifiesHrWhenAppropriate(LeaveRequest request,
            PersonWithStaff personWithStaff,
            int usedLeave,
            int totalYears,
            bool expectedEmailed)
        {
            var job = _sf.InsertJob(j =>
            {
                //force job to provide time off
                j.Status = JobStatus.FullTime;
                j.OrgGroup.Type = GroupType.Department;
            });
            _sf.DbConnection.Insert(personWithStaff);
            _sf.DbConnection.Insert(personWithStaff.Staff);
            //create roles for # of years
            _sf.InsertRole(job.Id, personWithStaff.Id, totalYears);
            //insert used leave
            _sf.InsertLeaveRequest(request.Type, personWithStaff.Id, usedLeave);
            var actualEmailed = _leaveService.ShouldNotifyHr(request,
                _leaveService.GetCurrentLeaveUseage(request.Type, personWithStaff.Id));
            Assert.Equal(expectedEmailed, actualEmailed);
        }

        public static IEnumerable<object[]> GetExpectedEmailValues()
        {
            var person1Id = Guid.NewGuid();
            var person2Id = Guid.NewGuid();
            var person3Id = Guid.NewGuid();
            var person4Id = Guid.NewGuid();
            var departmentId = Guid.NewGuid();
            var devisionId = Guid.NewGuid();
            var supervisorGroupId = Guid.NewGuid();

            PersonWithStaff Person(Guid id)
            {
                return new PersonWithStaff
                {
                    Id = id,
                    Staff = new StaffWithOrgName()
                };
            }

            IEnumerable<(string reason, LeaveRequest request,
                PersonWithStaff requestedBy,
                OrgGroupWithSupervisor department,
                OrgGroupWithSupervisor devision,
                OrgGroupWithSupervisor supervisorGroup,
                Guid expectedApproverId,
                bool expectApprovalEmailSent,
                int expectedNotifyEmailCount)> MakeValues()
            {
                yield return ("Person with a single supervisor", new LeaveRequest {PersonId = person1Id},
                    Person(person1Id),
                    new OrgGroupWithSupervisor {Id = departmentId, ParentId = devisionId},
                    new OrgGroupWithSupervisor {Id = devisionId, ParentId = supervisorGroupId},
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = Person(person2Id)
                    },
                    person2Id,
                    true,
                    0);

                yield return ("Person with a supervisor to notify", new LeaveRequest {PersonId = person1Id},
                    Person(person1Id),
                    new OrgGroupWithSupervisor {Id = departmentId, ParentId = devisionId},
                    new OrgGroupWithSupervisor
                    {
                        Id = devisionId,
                        ParentId = supervisorGroupId,
                        ApproverIsSupervisor = false,
                        Supervisor = person3Id,
                        SupervisorPerson = Person(person3Id)
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = Person(person2Id)
                    },
                    person2Id,
                    true,
                    1);

                yield return ("Person with 2 supervisors to notify", new LeaveRequest {PersonId = person1Id},
                    Person(person1Id),
                    new OrgGroupWithSupervisor
                    {
                        Id = departmentId,
                        ParentId = devisionId,
                        ApproverIsSupervisor = false,
                        Supervisor = person4Id,
                        SupervisorPerson = Person(person4Id)
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = devisionId,
                        ParentId = supervisorGroupId,
                        ApproverIsSupervisor = false,
                        Supervisor = person3Id,
                        SupervisorPerson = Person(person3Id)
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = Person(person2Id)
                    },
                    person2Id,
                    true,
                    2);
                
                yield return ("Person with 1 supervisor to notify, don't notify supervisor above approver", new LeaveRequest {PersonId = person1Id},
                    Person(person1Id),
                    new OrgGroupWithSupervisor
                    {
                        Id = departmentId,
                        ParentId = devisionId,
                        ApproverIsSupervisor = false,
                        Supervisor = person4Id,
                        SupervisorPerson = Person(person4Id)
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = devisionId,
                        ParentId = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person3Id,
                        SupervisorPerson = Person(person3Id)
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = false,
                        Supervisor = person2Id,
                        SupervisorPerson = Person(person2Id)
                    },
                    person3Id,
                    true,
                    1);

                yield return ("person with a group inbetween to not notify", new LeaveRequest {PersonId = person1Id},
                    Person(person1Id),
                    new OrgGroupWithSupervisor
                    {
                        Id = departmentId,
                        ParentId = devisionId,
                        ApproverIsSupervisor = false,
                        Supervisor = person4Id,
                        SupervisorPerson = Person(person4Id)
                    },
                    new OrgGroupWithSupervisor {Id = devisionId, ParentId = supervisorGroupId},
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = Person(person2Id)
                    },
                    person2Id,
                    true,
                    1);

                yield return ("person with 2 groups who could approve, stops at department", new LeaveRequest
                    {
                        PersonId = person1Id
                    },
                    Person(person1Id),
                    new OrgGroupWithSupervisor
                    {
                        Id = departmentId,
                        ParentId = devisionId,
                        ApproverIsSupervisor = true,
                        Supervisor = person4Id,
                        SupervisorPerson = Person(person4Id)
                    },
                    new OrgGroupWithSupervisor {Id = devisionId, ParentId = supervisorGroupId},
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = Person(person2Id)
                    },
                    person4Id,
                    true,
                    0);

                yield return ("supervisor requesting leave, no one to notify", new LeaveRequest {PersonId = person1Id},
                    Person(person1Id),
                    new OrgGroupWithSupervisor
                    {
                        Id = departmentId,
                        ParentId = devisionId,
                        ApproverIsSupervisor = true,
                        Supervisor = person1Id,
                        SupervisorPerson = Person(person1Id)
                    },
                    new OrgGroupWithSupervisor {Id = devisionId, ParentId = supervisorGroupId},
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = Person(person2Id)
                    },
                    person2Id,
                    true,
                    0);

                yield return ("supervisor requesting leave, 1 person to notify",
                    new LeaveRequest {PersonId = person1Id},
                    Person(person1Id),
                    new OrgGroupWithSupervisor
                    {
                        Id = departmentId,
                        ParentId = devisionId,
                        ApproverIsSupervisor = true,
                        Supervisor = person1Id,
                        SupervisorPerson = Person(person1Id)
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = devisionId,
                        ParentId = supervisorGroupId,
                        ApproverIsSupervisor = false,
                        Supervisor = person3Id,
                        SupervisorPerson = Person(person3Id)
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = Person(person2Id)
                    },
                    person2Id,
                    true,
                    1);

                yield return ("supervisor is in list twice",
                        new LeaveRequest {PersonId = person1Id},
                        Person(person1Id),
                        new OrgGroupWithSupervisor
                        {
                            Id = departmentId,
                            ParentId = devisionId,
                            ApproverIsSupervisor = false,
                            Supervisor = person2Id,
                            SupervisorPerson = Person(person2Id)
                        },
                        new OrgGroupWithSupervisor
                        {
                            Id = devisionId,
                            ParentId = supervisorGroupId,
                            ApproverIsSupervisor = true,
                            Supervisor = person2Id,
                            SupervisorPerson = Person(person2Id)
                        },
                        new OrgGroupWithSupervisor {Id = supervisorGroupId},
                        person2Id,
                        true,
                        0
                    );
            }

            return MakeValues().Select(tuple => tuple.ToArray());
        }


        [Theory]
        [MemberData(nameof(GetExpectedEmailValues))]
        public static async Task SendsExpectedEmails(string reason,
            LeaveRequest request,
            PersonWithStaff requestedBy,
            OrgGroupWithSupervisor department,
            OrgGroupWithSupervisor devision,
            OrgGroupWithSupervisor supervisorGroup,
            Guid expectedApproverId,
            bool expectApprovalEmailSent,
            int expectedNotifyEmailCount)
        {
            var sf = new ServicesFixture();
            sf.CreateWebHost();
            var leaveService = sf.Get<LeaveService>();

            bool actualApprovalEmailSent = false;
            int actualNotifyEmailCount = 0;

            var emailMock = sf.EmailServiceMock.As<IEmailService>();
            emailMock.Setup(service => service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<EmailTemplate>(),
                    It.IsAny<PersonWithStaff>(),
                    It.IsAny<PersonWithStaff>()))
                .Returns(Task.CompletedTask)
                .Callback<Dictionary<string, string>, string, EmailTemplate, PersonWithStaff, PersonWithStaff>(
                    (dictionary, subject, template, from, to) =>
                    {
                        if (template == EmailTemplate.RequestLeaveApproval) actualApprovalEmailSent = true;
                        else if (template == EmailTemplate.NotifyLeaveRequest)
                            actualNotifyEmailCount++;
                    });


            var actualApprover = await leaveService.ResolveLeaveRequestEmails(request,
                requestedBy,
                new List<OrgGroupWithSupervisor>
                {
                    department,
                    devision,
                    supervisorGroup
                }.FindAll(g => g != null),
                new LeaveUsage {LeaveType = LeaveType.Sick, TotalAllowed = 20, Used = 0});
            Assert.True((expectedApproverId == Guid.Empty) == (actualApprover == null));
            if (actualApprover != null)
                Assert.Equal(expectedApproverId, actualApprover.Id);
            Assert.Equal(expectApprovalEmailSent, actualApprovalEmailSent);
            Assert.Equal(expectedNotifyEmailCount, actualNotifyEmailCount);
        }

        public static IEnumerable<object[]> ValidateEmailValues()
        {
            PersonWithStaff MakeStaff(OrgGroup orgGroup)
            {
                var staffId = Guid.NewGuid();
                var personId = Guid.NewGuid();
                return new PersonWithStaff
                {
                    Id = personId,
                    StaffId = staffId,
                    FirstName = orgGroup.GroupName + " staff",
                    Staff = new StaffWithOrgName
                    {
                        Id = staffId,
                        OrgGroupId = orgGroup.Id
                    }
                };
            }

            (OrgGroupWithSupervisor root, PersonWithStaff rootStaff,
            OrgGroupWithSupervisor a, PersonWithStaff aStaff,
            OrgGroupWithSupervisor aa, PersonWithStaff aaStaff,
            OrgGroupWithSupervisor ab, PersonWithStaff abStaff,
            OrgGroupWithSupervisor aaa, PersonWithStaff aaaStaff,
            PersonWithStaff[] people, OrgGroup[] groups) Extract(
                OrgGroupWithSupervisor root,
                PersonWithStaff rootStaff,
                OrgGroupWithSupervisor a,
                PersonWithStaff aStaff,
                OrgGroupWithSupervisor aa,
                PersonWithStaff aaStaff,
                OrgGroupWithSupervisor ab,
                PersonWithStaff abStaff,
                OrgGroupWithSupervisor aaa,
                PersonWithStaff aaaStaff)
            {
                return (
                    root, rootStaff,
                    a, aStaff,
                    aa, aaStaff,
                    ab, abStaff,
                    aaa, aaaStaff,
                    new[]
                    {
                        rootStaff, aStaff, aaStaff, abStaff, aaaStaff, root.SupervisorPerson, a.SupervisorPerson,
                        aa.SupervisorPerson, ab.SupervisorPerson, aaa.SupervisorPerson
                    },
                    new[]
                    {
                        root, a, aa, ab, aaa
                    });
            }

            (OrgGroupWithSupervisor root, PersonWithStaff rootStaff,
            OrgGroupWithSupervisor a, PersonWithStaff aStaff,
            OrgGroupWithSupervisor aa, PersonWithStaff aaStaff,
            OrgGroupWithSupervisor ab, PersonWithStaff abStaff,
            OrgGroupWithSupervisor aaa, PersonWithStaff aaaStaff,
            PersonWithStaff[] people, OrgGroup[] groups) MakeTree()
            {
                var (root, a, aa, ab, aaa) = ServicesFixture.MakeGroupTree();
                return Extract(root,
                    MakeStaff(root),
                    a,
                    MakeStaff(a),
                    aa,
                    MakeStaff(aa),
                    ab,
                    MakeStaff(ab),
                    aaa,
                    MakeStaff(aaa));
            }

            IEnumerable<(Guid requestForId,
                PersonWithStaff[] people,
                OrgGroup[] groups,
                string approver, string[] notified)> MakeValues()
            {
                var (root, rootStaff, a, aStaff, aa, aaStaff, ab, abStaff, aaa, aaaStaff, people, groups) = MakeTree();
                aaa.ApproverIsSupervisor = true;
                yield return (aaaStaff.Id, people, groups, aaa.SupervisorPerson.FirstName, new string[0]);

                (root, rootStaff, a, aStaff, aa, aaStaff, ab, abStaff, aaa, aaaStaff, people, groups) = MakeTree();
                aaa.ApproverIsSupervisor = false;
                aa.ApproverIsSupervisor = true;
                yield return (aaaStaff.Id, people, groups, aa.SupervisorPerson.FirstName,
                    new[] {aaa.SupervisorPerson.FirstName});

                (root, rootStaff, a, aStaff, aa, aaStaff, ab, abStaff, aaa, aaaStaff, people, groups) = MakeTree();
                aaa.ApproverIsSupervisor = false;
                aa.ApproverIsSupervisor = false;
                a.ApproverIsSupervisor = true;
                yield return (aaaStaff.Id, people, groups, a.SupervisorPerson.FirstName,
                    new[] {aaa.SupervisorPerson.FirstName, aa.SupervisorPerson.FirstName});

                (root, rootStaff, a, aStaff, aa, aaStaff, ab, abStaff, aaa, aaaStaff, people, groups) = MakeTree();
                aaa.ApproverIsSupervisor = false;
                aa.ApproverIsSupervisor = true;
                yield return (aaa.SupervisorPerson.Id, people, groups, aa.SupervisorPerson.FirstName, new string[0]);

                (root, rootStaff, a, aStaff, aa, aaStaff, ab, abStaff, aaa, aaaStaff, people, groups) = MakeTree();
                aaa.ApproverIsSupervisor = true;
                aa.ApproverIsSupervisor = true;
                yield return (aaa.SupervisorPerson.Id, people, groups, aa.SupervisorPerson.FirstName, new string[0]);

                (root, rootStaff, a, aStaff, aa, aaStaff, ab, abStaff, aaa, aaaStaff, people, groups) = MakeTree();
                aaa.ApproverIsSupervisor = true;
                aa.ApproverIsSupervisor = false;
                a.ApproverIsSupervisor = true;
                yield return (aaa.SupervisorPerson.Id, people, groups, a.SupervisorPerson.FirstName,
                    new[] {aa.SupervisorPerson.FirstName});
            }

            return MakeValues().Select(t => t.ToArray());
        }

        [Theory]
        [MemberData(nameof(ValidateEmailValues))]
        public async Task ValidateSupervisorEmails(Guid requestForId,
            PersonWithStaff[] people,
            OrgGroup[] groups,
            string approver,
            string[] notified)
        {
            _sf.DbConnection.BulkCopy(people);
            _sf.DbConnection.BulkCopy(people.Select(p => p.Staff));
            _sf.DbConnection.BulkCopy(groups);
            var emailMock = _sf.EmailServiceMock;
            var approvalEmails = new List<string>();
            var notifyEmails = new List<string>();

            emailMock.Invocations.Clear();
            emailMock.Setup(service => service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<EmailTemplate>(),
                    It.IsAny<PersonWithStaff>(),
                    It.IsAny<PersonWithStaff>()))
                .Returns(Task.CompletedTask)
                .Callback<Dictionary<string, string>, string, EmailTemplate, PersonWithStaff, PersonWithStaff>(
                    (dictionary, subject, template, from, to) =>
                    {
                        if (template == EmailTemplate.RequestLeaveApproval) approvalEmails.Add(to.FirstName);
                        else if (template == EmailTemplate.NotifyLeaveRequest) notifyEmails.Add(to.FirstName);
                    });

            await _leaveService.RequestLeave(new LeaveRequest
            {
                PersonId = requestForId,
                StartDate = new DateTime(2018, 10, 19),
                EndDate = new DateTime(2018, 10, 19),
                Days = 1
            });

            emailMock.Verify(e => e.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<EmailTemplate>(),
                    It.IsAny<PersonWithStaff>(),
                    It.IsAny<PersonWithStaff>()),
                Times.AtLeastOnce());

            if (!string.IsNullOrEmpty(approver))
            {
                approvalEmails.ShouldHaveSingleItem().ShouldBe(approver);
            }
            else
            {
                approvalEmails.ShouldBeEmpty();
            }

            notifyEmails.ShouldBe(notified);
        }

        public void Dispose()
        {
            _transaction?.Dispose();
        }
    }
}