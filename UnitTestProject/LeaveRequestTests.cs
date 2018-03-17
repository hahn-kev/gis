using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoBogus;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace UnitTestProject
{
    public class LeaveRequestTests
    {
        private LeaveService _leaveService;
        private OrgGroupRepository _orgGroupRepository;
        private IDbConnection _dbConnection;
        private ServicesFixture _servicesFixture;

        public LeaveRequestTests()
        {
            _servicesFixture = new ServicesFixture();
            _leaveService = _servicesFixture.Get<LeaveService>();
            _orgGroupRepository = _servicesFixture.Get<OrgGroupRepository>();
            _dbConnection = _servicesFixture.Get<IDbConnection>();
        }

        private void Setup(Action<IServiceCollection> configure = null)
        {
            if (configure != null)
            {
                _servicesFixture = new ServicesFixture(configure);
                _leaveService = _servicesFixture.Get<LeaveService>();
                _orgGroupRepository = _servicesFixture.Get<OrgGroupRepository>();
                _dbConnection = _servicesFixture.Get<IDbConnection>();
            }

            _servicesFixture.SetupPeople();
        }

        [Fact]
        public async Task FindsSupervisor()
        {
            _servicesFixture.SetupPeople();
            var jacob = _dbConnection.People.FirstOrDefault(person => person.FirstName == "Jacob");
            Assert.NotNull(jacob);
            var expectedSupervisor = _dbConnection.People.FirstOrDefault(person => person.FirstName == "Bob");
            Assert.NotNull(expectedSupervisor);
            var actualSupervisor = await _leaveService.RequestLeave(new LeaveRequest {PersonId = jacob.Id});
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
            Setup(collection => collection.RemoveAll<IEmailService>().AddSingleton(emailMock.Object));
            var jacob = _dbConnection.People.FirstOrDefault(person => person.FirstName == "Jacob");
            Assert.NotNull(jacob);
            await _leaveService.RequestLeave(new LeaveRequest {PersonId = jacob.Id});

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

        [Fact]
        public void OrgGroupChainResolvesAsExpected()
        {
            var personFaker = _servicesFixture.PersonFaker();
            var expectedPersonOnLeave = personFaker.Generate();
            var expectedDepartment = AutoFaker.Generate<OrgGroup>();
            var expectedDevision = AutoFaker.Generate<OrgGroup>();
            var expectedDevisionSupervisor = personFaker.Generate();

            expectedPersonOnLeave.Staff.OrgGroupId = expectedDepartment.Id;

            expectedDepartment.Supervisor = null;
            expectedDepartment.ParentId = expectedDevision.Id;

            expectedDevision.Supervisor = expectedDevisionSupervisor.Id;
            expectedDevision.ParentId = null;

            _dbConnection.Insert(expectedPersonOnLeave);
            _dbConnection.Insert(expectedPersonOnLeave.Staff);
            _dbConnection.Insert(expectedDepartment);
            _dbConnection.Insert(expectedDevision);
            _dbConnection.Insert(expectedDevisionSupervisor);
            _dbConnection.Insert(expectedDevisionSupervisor.Staff);

            //test method
            (var actualPersonOnLeave, var actualDepartment, var actualDevision, var actualSupervisorGroup) =
                _orgGroupRepository.PersonWithOrgGroupChain(expectedPersonOnLeave.Id);

            Assert.NotNull(actualPersonOnLeave);
            Assert.Equal(expectedPersonOnLeave.Id, actualPersonOnLeave.Id);
            Assert.NotNull(actualDepartment);
            Assert.Equal(expectedDepartment.Id, actualDepartment.Id);
            Assert.Null(actualDepartment.SupervisorPerson);
            Assert.NotNull(actualDevision);
            Assert.Equal(expectedDevision.Id, actualDevision.Id);
            Assert.Equal(expectedDevisionSupervisor.Id, actualDevision.SupervisorPerson.Id);
            Assert.Null(actualSupervisorGroup);
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
                        else if (template == EmailService.Template.NotifyLeaveRequest)
                            actualNotifyEmailCount++;
                    });

            Setup(collection => collection.Replace(ServiceDescriptor.Singleton(emailMock.Object)));

            var actualApprover = await _leaveService.ResolveLeaveRequestChain(request,
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

        private LeaveRequest GenerateRequest()
        {
            var leaveRequest = AutoFaker.Generate<LeaveRequest>();
            if (leaveRequest.StartDate > leaveRequest.EndDate)
            {
                var tmp = leaveRequest.StartDate;
                leaveRequest.StartDate = leaveRequest.EndDate;
                leaveRequest.EndDate = tmp;
            }

            leaveRequest.Days = leaveRequest.CalculateLength();
            return leaveRequest;
        }

        [Fact]
        public void ThrowsErrorWhenPersonIsInvalid()
        {
            var personId = Guid.NewGuid();
            LeaveRequest oldRequest = GenerateRequest();
            LeaveRequest newRequest = oldRequest.Copy();
            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                _leaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest, personId));
            Assert.Contains("modify this leave request", ex.Message);

            personId = oldRequest.PersonId;
            newRequest.PersonId = Guid.NewGuid();
            ex = Assert.Throws<UnauthorizedAccessException>(() =>
                _leaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest, personId));
            Assert.Contains("modify this leave request", ex.Message);
        }

        [Fact]
        public void ThrowsWhenChangingTheDays()
        {
            LeaveRequest oldRequest = GenerateRequest();
            oldRequest.OverrideDays = false;
            LeaveRequest newRequest = oldRequest.Copy();

            newRequest.Days--;

            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                _leaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest, oldRequest.PersonId));

            Assert.Contains("must match calculated", ex.Message);

            newRequest.Days = newRequest.CalculateLength();
            newRequest.EndDate += TimeSpan.FromDays(4);
            ex = Assert.Throws<UnauthorizedAccessException>(() =>
                _leaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest, oldRequest.PersonId));

            Assert.Contains("must match calculated", ex.Message);
        }

        [Fact]
        public void ThrowsWhenChangingDaysWhenOverriden()
        {
            LeaveRequest oldRequest = GenerateRequest();
            oldRequest.OverrideDays = true;
            LeaveRequest newRequest = oldRequest.Copy();
            newRequest.Days++;

            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                _leaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest, oldRequest.PersonId));

            Assert.Contains("modify the length", ex.Message);
        }

        [Fact]
        public void ThrowsWhenChangingStartAndEndForOverridenDays()
        {
            LeaveRequest oldRequest = GenerateRequest();
            oldRequest.OverrideDays = true;
            LeaveRequest newRequest = oldRequest.Copy();
            newRequest.EndDate += TimeSpan.FromDays(4);

            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                _leaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest, oldRequest.PersonId));

            Assert.Contains("modify the start or end", ex.Message);
        }

        [Fact]
        public void ThrowsIfChangingApproved()
        {
            LeaveRequest oldRequest = GenerateRequest();
            LeaveRequest newRequest = oldRequest.Copy();
            oldRequest.Approved = false;
            newRequest.Approved = true;
            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                _leaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest, oldRequest.PersonId));
            Assert.Contains("approve", ex.Message);
        }

        [Fact]
        public void AcceptMissmatchedCalculationForHalfDays()
        {
            LeaveRequest oldRequest = GenerateRequest();
            oldRequest.OverrideDays = false;
            //16th is a friday
            oldRequest.StartDate = new DateTime(2018, 3, 16);
            oldRequest.EndDate = new DateTime(2018, 3, 16);
            oldRequest.Days = oldRequest.CalculateLength();
            LeaveRequest newRequest = oldRequest.Copy();
            newRequest.Days = 0.5m;
            //does not throw
            _leaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest, oldRequest.PersonId);
        }


        [Fact]
        public void DontMissmatchedCalculationForHalfDaysOnWeeken()
        {
            LeaveRequest oldRequest = GenerateRequest();
            oldRequest.OverrideDays = false;
            //17th is a saturday
            oldRequest.StartDate = new DateTime(2018, 3, 17);
            oldRequest.EndDate = new DateTime(2018, 3, 17);
            oldRequest.Days = oldRequest.CalculateLength();
            LeaveRequest newRequest = oldRequest.Copy();
            newRequest.Days = 0.5m;
            //does not throw
            Assert.Throws<UnauthorizedAccessException>(() =>
                _leaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest, oldRequest.PersonId));
        }
    }
}