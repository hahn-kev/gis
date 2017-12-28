using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace UnitTestProject
{
    public class LeaveRequestTests
    {
        private LeaveRequestService _leaveRequestService;
        private IDbConnection _dbConnection;
        private ServicesFixture _servicesFixture;

        public LeaveRequestTests()
        {
            _servicesFixture = new ServicesFixture();
        }

        private void Setup(Action<IServiceCollection> configure = null)
        {
            if (configure != null) _servicesFixture = new ServicesFixture(configure);
            _servicesFixture.SetupPeople();
            _leaveRequestService = _servicesFixture.Get<LeaveRequestService>();
            _dbConnection = _servicesFixture.Get<IDbConnection>();
        }

        [Fact]
        public async Task FindsSupervisor()
        {
            Setup();
            var jacob = _dbConnection.People.FirstOrDefault(person => person.FirstName == "Jacob");
            Assert.NotNull(jacob);
            var expectedSupervisor = _dbConnection.People.FirstOrDefault(person => person.FirstName == "Bob");
            Assert.NotNull(expectedSupervisor);
            var actualSupervisor = await _leaveRequestService.RequestLeave(new LeaveRequest {PersonId = jacob.Id});
            Assert.NotNull(actualSupervisor);
            Assert.Equal(expectedSupervisor.FirstName, actualSupervisor.FirstName);
        }

        [Fact]
        public async Task SendsEmail()
        {
            var emailMock = new Mock<IEmailService>();
            emailMock.Setup(service => service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                It.IsAny<string>(),
                It.IsAny<EmailService.Template>(),
                It.IsAny<PersonExtended>(),
                It.IsAny<PersonExtended>())).Returns(Task.CompletedTask);
            _servicesFixture = new ServicesFixture(collection =>
            {
                collection.RemoveAll<IEmailService>().AddSingleton(emailMock.Object);
            });
            Setup();
            var jacob = _dbConnection.People.FirstOrDefault(person => person.FirstName == "Jacob");
            await _leaveRequestService.RequestLeave(new LeaveRequest {PersonId = jacob.Id});

            emailMock.Verify(service =>
                    service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                        It.IsAny<string>(),
                        It.Is<EmailService.Template>(template =>
                            template == EmailService.Template.RequestLeaveApproval),
                        It.Is<PersonExtended>(extended => extended.FirstName == "Jacob"),
                        It.Is<PersonExtended>(extended => extended.FirstName == "Bob")),
                Times.Once);

            emailMock.Verify(service =>
                    service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                        It.IsAny<string>(),
                        It.Is<EmailService.Template>(template => template == EmailService.Template.NotifyLeaveRequest),
                        It.IsAny<PersonExtended>(),
                        It.IsAny<PersonExtended>()),
                Times.Never);
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

            IEnumerable<(string reason, LeaveRequest request,
                PersonExtended requestedBy,
                OrgGroupWithSupervisor department,
                OrgGroupWithSupervisor devision,
                OrgGroupWithSupervisor supervisorGroup,
                Guid expectedApproverId,
                bool expectApprovalEmailSent,
                int expectedNotifyEmailCount)> MakeValues()
            {
                yield return ("Person with a single supervisor", new LeaveRequest {PersonId = person1Id},
                    new PersonExtended {Id = person1Id},
                    new OrgGroupWithSupervisor {Id = departmentId},
                    new OrgGroupWithSupervisor {Id = devisionId},
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = new PersonExtended {Id = person2Id}
                    },
                    person2Id,
                    true,
                    0);

                yield return ("Person with a supervisor to notify", new LeaveRequest {PersonId = person1Id},
                    new PersonExtended {Id = person1Id},
                    new OrgGroupWithSupervisor {Id = departmentId},
                    new OrgGroupWithSupervisor
                    {
                        Id = devisionId,
                        ApproverIsSupervisor = false,
                        Supervisor = person3Id,
                        SupervisorPerson = new PersonExtended {Id = person3Id}
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = new PersonExtended {Id = person2Id}
                    },
                    person2Id,
                    true,
                    1);

                yield return ("Person with 2 supervisors to notify", new LeaveRequest {PersonId = person1Id},
                    new PersonExtended {Id = person1Id},
                    new OrgGroupWithSupervisor
                    {
                        Id = departmentId,
                        ApproverIsSupervisor = false,
                        Supervisor = person4Id,
                        SupervisorPerson = new PersonExtended {Id = person4Id}
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = devisionId,
                        ApproverIsSupervisor = false,
                        Supervisor = person3Id,
                        SupervisorPerson = new PersonExtended {Id = person3Id}
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = new PersonExtended {Id = person2Id}
                    },
                    person2Id,
                    true,
                    2);

                yield return ("person with a group inbetween to not notify", new LeaveRequest {PersonId = person1Id},
                    new PersonExtended {Id = person1Id},
                    new OrgGroupWithSupervisor
                    {
                        Id = departmentId,
                        ApproverIsSupervisor = false,
                        Supervisor = person4Id,
                        SupervisorPerson = new PersonExtended {Id = person4Id}
                    },
                    new OrgGroupWithSupervisor {Id = devisionId},
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = new PersonExtended {Id = person2Id}
                    },
                    person2Id,
                    true,
                    1);

                yield return ("person with 2 groups who could approve, stops at department", new LeaveRequest
                    {
                        PersonId = person1Id
                    },
                    new PersonExtended {Id = person1Id},
                    new OrgGroupWithSupervisor
                    {
                        Id = departmentId,
                        ApproverIsSupervisor = true,
                        Supervisor = person4Id,
                        SupervisorPerson = new PersonExtended {Id = person4Id}
                    },
                    new OrgGroupWithSupervisor {Id = devisionId},
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = new PersonExtended {Id = person2Id}
                    },
                    person4Id,
                    true,
                    0);

                yield return ("supervisor requesting leave, no one to notify", new LeaveRequest {PersonId = person1Id},
                    new PersonExtended {Id = person1Id},
                    new OrgGroupWithSupervisor
                    {
                        Id = departmentId,
                        ApproverIsSupervisor = true,
                        Supervisor = person1Id,
                        SupervisorPerson = new PersonExtended {Id = person1Id}
                    },
                    new OrgGroupWithSupervisor {Id = devisionId},
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = new PersonExtended {Id = person2Id}
                    },
                    person2Id,
                    true,
                    0);

                yield return ("supervisor requesting leave, 1 person to notify", new LeaveRequest {PersonId = person1Id}
                    ,
                    new PersonExtended {Id = person1Id},
                    new OrgGroupWithSupervisor
                    {
                        Id = departmentId,
                        ApproverIsSupervisor = true,
                        Supervisor = person1Id,
                        SupervisorPerson = new PersonExtended {Id = person1Id}
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = devisionId,
                        ApproverIsSupervisor = false,
                        Supervisor = person3Id,
                        SupervisorPerson = new PersonExtended {Id = person3Id}
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = supervisorGroupId,
                        ApproverIsSupervisor = true,
                        Supervisor = person2Id,
                        SupervisorPerson = new PersonExtended {Id = person2Id}
                    },
                    person2Id,
                    true,
                    1);
            }

            return MakeValues().Select(tuple => tuple.ToArray());
        }

        [Theory]
        [MemberData(nameof(GetExpectedEmailValues))]
        public async Task SendsExpectedEmails(string reason,
            LeaveRequest request,
            PersonExtended requestedBy,
            OrgGroupWithSupervisor department,
            OrgGroupWithSupervisor devision,
            OrgGroupWithSupervisor supervisorGroup,
            Guid expectedApproverId,
            bool expectApprovalEmailSent,
            int expectedNotifyEmailCount)
        {
            bool actualApprovalEmailSent = false;
            int actualNotifyEmailCount = 0;

            var emailMock = new Mock<IEmailService>();
            emailMock.Setup(service => service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<EmailService.Template>(),
                    It.IsAny<PersonExtended>(),
                    It.IsAny<PersonExtended>()))
                .Returns(Task.CompletedTask)
                .Callback<Dictionary<string, string>, string, EmailService.Template, PersonExtended, PersonExtended>(
                    (dictionary, subject, template, to, from) =>
                    {
                        if (template == EmailService.Template.RequestLeaveApproval) actualApprovalEmailSent = true;
                        else if (template == EmailService.Template.NotifyLeaveRequest) actualNotifyEmailCount++;
                    });

            Setup(collection => collection.Replace(ServiceDescriptor.Singleton(emailMock.Object)));

            var actualApprover = await _leaveRequestService.ResolveLeaveRequestChain(request,
                requestedBy,
                department,
                devision,
                supervisorGroup);
            Assert.True((expectedApproverId == Guid.Empty) == (actualApprover == null));
            if (actualApprover != null)
                Assert.Equal(expectedApproverId, actualApprover.Id);
            Assert.Equal(expectApprovalEmailSent, actualApprovalEmailSent);
            Assert.Equal(expectedNotifyEmailCount, actualNotifyEmailCount);
        }
    }
}