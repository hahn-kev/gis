using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Backend.Utils;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
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
        private IServiceScope _serviceScope;
        private IServiceProvider _scopeServiceProvider;

        public LeaveRequestTests(ServicesFixture sf)
        {
            _sf = sf;
            _leaveService = _sf.Get<LeaveService>();
            _orgGroupRepository = _sf.Get<OrgGroupRepository>();
            _sf.DoOnce(fixture => fixture.SetupPeople());
            _transaction = _sf.DbConnection.BeginTransaction();
            _serviceScope = _sf.ServiceProvider.CreateScope();
            _scopeServiceProvider = _serviceScope.ServiceProvider;
        }

        [Fact]
        public async Task FindsSupervisor()
        {
            var jacob = _sf.Jacob;
            jacob.ShouldNotBeNull();
            var expectedSupervisor = _sf.JacobSupervisor;
            expectedSupervisor.ShouldNotBeNull();
            var actualSupervisor = await _leaveService.RequestLeave(new LeaveRequest
                {PersonId = jacob.Id, StartDate = DateTime.Now});
            actualSupervisor.ShouldNotBeNull();
            actualSupervisor.FirstName.ShouldBe(expectedSupervisor.FirstName);
        }

        [Fact]
        public async Task SendsEmail()
        {
            var leaveService = _scopeServiceProvider.GetService<LeaveService>();

            var emailMock = _sf.GetScopedMockEmailService(_serviceScope);
            emailMock.Setup(service => service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                It.IsAny<string>(),
                It.IsAny<EmailTemplate>(),
                It.IsAny<PersonWithStaff>(),
                It.IsAny<PersonWithStaff>())).Returns(Task.CompletedTask);
            var jacob = _sf.Jacob;

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
        public async Task SavesLeaveRequest()
        {
            var leaveService = _scopeServiceProvider.GetService<LeaveService>();

            var jacob = _sf.Jacob;
            jacob.ShouldNotBeNull();
            var expectedLeaveRequest = new LeaveRequest {PersonId = jacob.Id, StartDate = DateTime.Now};
            await leaveService.RequestLeave(expectedLeaveRequest);

            _sf.GetScopedMockEntityService(_serviceScope)
                .Verify(service => service.Save(It.IsAny<LeaveRequest>()), Times.Once);
        }

        [Fact]
        public async Task EmailThrowingAnErrorWillCauseALeaveRequestToBeDeleted()
        {
            var mock = _sf.GetScopedMockEmailService(_serviceScope);
            var expectedException = new Exception("email test exception");
            mock.Setup(email => email.SendEmail(It.IsAny<SendGridMessage>()))
                .Throws(expectedException);
            var jacob = _sf.Jacob;
            jacob.ShouldNotBeNull();

            //act
            var leaveService = _scopeServiceProvider.GetService<LeaveService>();
            var expectedLeaveRequest = new LeaveRequest {PersonId = jacob.Id, StartDate = DateTime.Now};
            var actualException =
                await Should.ThrowAsync<Exception>(() => leaveService.RequestLeave(expectedLeaveRequest));
            //test
            actualException.ShouldBe(expectedException);
            var mockEntityService = _sf.GetScopedMockEntityService(_serviceScope);
            mockEntityService.Verify(service => service.Save(expectedLeaveRequest), Times.Once);
            mockEntityService.Verify(service => service.Delete(expectedLeaveRequest), Times.Once);
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
            var actualEmailed = LeaveRequestEmailService.ShouldNotifyHr(request,
                _leaveService.GetCurrentLeaveUseage(request.Type, personWithStaff.Id));
            actualEmailed.ShouldBe(expectedEmailed);
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
                PersonWithStaff toApprove,
                List<PersonWithStaff> toNotify,
                bool expectApprovalEmailSent,
                int expectedNotifyEmailCount)> MakeValues()
            {
                yield return ("Person with a single supervisor", new LeaveRequest {PersonId = person1Id},
                    Person(person1Id),
                    Person(person2Id),
                    new List<PersonWithStaff>(),
                    true,
                    0);

                yield return ("Person with a supervisor to notify", new LeaveRequest {PersonId = person1Id},
                    Person(person1Id),
                    Person(person2Id),
                    new List<PersonWithStaff> {Person(person3Id)},
                    true,
                    1);

                yield return ("Person with 2 supervisors to notify", new LeaveRequest {PersonId = person1Id},
                    Person(person1Id),
                    Person(person2Id),
                    new List<PersonWithStaff> {Person(person4Id), Person(person3Id)},
                    true,
                    2);
            }

            return MakeValues().Select(tuple => tuple.ToArray());
        }


        [Theory]
        [MemberData(nameof(GetExpectedEmailValues))]
        public async Task SendsExpectedEmails(string reason,
            LeaveRequest request,
            PersonWithStaff requestedBy,
            PersonWithStaff toApprove,
            List<PersonWithStaff> toNotify,
            bool expectApprovalEmailSent,
            int expectedNotifyEmailCount)
        {
            var leaveService = _scopeServiceProvider.GetService<LeaveService>();

            bool actualApprovalEmailSent = false;
            int actualNotifyEmailCount = 0;

            _sf.GetScopedMockEmailService(_serviceScope)
                .Setup(service => service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
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

            await leaveService.SendLeaveRequestEmails(request,
                requestedBy,
                toApprove,
                toNotify,
                new LeaveUsage {LeaveType = LeaveType.Sick, TotalAllowed = 20, Used = 0});
            actualApprovalEmailSent.ShouldBe(expectApprovalEmailSent, reason);
            actualNotifyEmailCount.ShouldBe(expectedNotifyEmailCount, reason);
        }

        public static IEnumerable<object[]> GetExpectedResolvedSupervisors()
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

            IEnumerable<(string reason,
                PersonWithStaff requestedBy,
                OrgGroupWithSupervisor department,
                OrgGroupWithSupervisor devision,
                OrgGroupWithSupervisor supervisorGroup,
                Guid expectedApproverId,
                int expectedNotifyEmailCount)> MakeValues()
            {
                yield return ("Person with a single supervisor",
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
                    0);

                yield return ("Person with a supervisor to notify",
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
                    1);

                yield return ("Person with 2 supervisors to notify",
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
                    2);

                yield return ("Person with 1 supervisor to notify, don't notify supervisor above approver",
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
                    1);

                yield return ("person with a group inbetween to not notify",
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
                    1);

                yield return ("person with 2 groups who could approve, stops at department",
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
                    0);

                yield return ("supervisor requesting leave, no one to notify",
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
                    0);

                yield return ("supervisor requesting leave, 1 person to notify",
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
                    1);

                yield return ("supervisor is in list twice",
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
                        0
                    );
            }

            return MakeValues().Select(tuple => tuple.ToArray());
        }

        [Theory]
        [MemberData(nameof(GetExpectedResolvedSupervisors))]
        public void ResolvesExpectedSupervisors(string reason,
            PersonWithStaff requestedBy,
            OrgGroupWithSupervisor department,
            OrgGroupWithSupervisor devision,
            OrgGroupWithSupervisor supervisorGroup,
            Guid expectedApproverId,
            int expectedNotifyEmailCount)
        {
            var (actualApprover, toNotify) = LeaveService.ResolveLeaveRequestEmails(requestedBy,
                new List<OrgGroupWithSupervisor>
                {
                    department,
                    devision,
                    supervisorGroup
                }.FindAll(g => g != null),
                new List<OrgGroupWithSupervisor>());

            if (expectedApproverId == Guid.Empty)
            {
                actualApprover.ShouldBeNull(reason);
            }
            else
            {
                actualApprover.ShouldNotBeNull(reason);
                actualApprover.Id.ShouldBe(expectedApproverId, reason);
            }

            toNotify.Count.ShouldBe(expectedNotifyEmailCount, reason);
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
                var (_, _, a, _, aa, _, _, _, aaa, aaaStaff, people, groups) = MakeTree();
                aaa.ApproverIsSupervisor = true;
                yield return (aaaStaff.Id, people, groups, aaa.SupervisorPerson.FirstName, new string[0]);

                (_, _, _, _, aa, _, _, _, aaa, aaaStaff, people, groups) = MakeTree();
                aaa.ApproverIsSupervisor = false;
                aa.ApproverIsSupervisor = true;
                yield return (aaaStaff.Id, people, groups, aa.SupervisorPerson.FirstName,
                    new[] {aaa.SupervisorPerson.FirstName});

                (_, _, a, _, aa, _, _, _, aaa, aaaStaff, people, groups) = MakeTree();
                aaa.ApproverIsSupervisor = false;
                aa.ApproverIsSupervisor = false;
                a.ApproverIsSupervisor = true;
                yield return (aaaStaff.Id, people, groups, a.SupervisorPerson.FirstName,
                    new[] {aaa.SupervisorPerson.FirstName, aa.SupervisorPerson.FirstName});

                (_, _, _, _, aa, _, _, _, aaa, _, people, groups) = MakeTree();
                aaa.ApproverIsSupervisor = false;
                aa.ApproverIsSupervisor = true;
                yield return (aaa.SupervisorPerson.Id, people, groups, aa.SupervisorPerson.FirstName, new string[0]);

                (_, _, _, _, aa, _, _, _, aaa, _, people, groups) = MakeTree();
                aaa.ApproverIsSupervisor = true;
                aa.ApproverIsSupervisor = true;
                yield return (aaa.SupervisorPerson.Id, people, groups, aa.SupervisorPerson.FirstName, new string[0]);

                (_, _, a, _, aa, _, _, _, aaa, _, people, groups) = MakeTree();
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

        [Fact]
        public void ShouldEmailSupervisorOfRoleNotJustReport()
        {
            var indirectReportGroup = Guid.NewGuid();
            var roleSupervisor = _sf.InsertStaff();
            var directSupervisor = _sf.InsertStaff();
            var directReportGroup = _sf.InsertOrgGroup(supervisorId: directSupervisor.person.Id,
                action: group => group.ApproverIsSupervisor = true);
            var requester = _sf.InsertStaff(directReportGroup.Id);

            var job = _sf.InsertStaffJob(group =>
            {
                group.OrgGroup.Id = indirectReportGroup;
                group.OrgGroupId = indirectReportGroup;
                group.OrgGroup.Supervisor = roleSupervisor.person.Id;
            });
            _sf.InsertRole(job.Id, requester.Id);

            var (toApprove, toNotify) = _leaveService.ResolveLeaveRequestEmails(requester);
            toApprove.FirstName.ShouldBe(directSupervisor.person.FirstName);
            toNotify.ShouldHaveSingleItem().FirstName.ShouldBe(roleSupervisor.person.FirstName);
        }

        [Fact]
        public async Task ApprovalWorksForStaffWithoutEmail()
        {
            var setup = _sf.SetupLeaveRequest(p => { p.Staff.Email = null; });
            var result = await _leaveService.ApproveLeaveRequest(setup.request.Id, setup.supervisor.Id);
            result.notified.ShouldBeFalse();
            result.requester.Id.ShouldBe(setup.requester.Id);
        }

        [Fact]
        public async Task ApprovalWorksForStaffEmail()
        {
            var setup = _sf.SetupLeaveRequest(p => { p.Staff.Email = "some test email"; });
            var result = await _leaveService.ApproveLeaveRequest(setup.request.Id, setup.supervisor.Id);
            result.notified.ShouldBeTrue();
            result.requester.Id.ShouldBe(setup.requester.Id);
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _serviceScope?.Dispose();
        }
    }
}