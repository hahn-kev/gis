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
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using SendGrid.Helpers.Mail;
using Shouldly;
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
            _servicesFixture.SetupPeople();
        }

        [Fact]
        public async Task FindsSupervisor()
        {
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
            var emailMock = _servicesFixture.EmailServiceMock;
            emailMock.Setup(service => service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                It.IsAny<string>(),
                It.IsAny<EmailTemplate>(),
                It.IsAny<PersonWithStaff>(),
                It.IsAny<PersonWithStaff>())).Returns(Task.CompletedTask);
            var jacob = _dbConnection.People.FirstOrDefault(person => person.FirstName == "Jacob");
            Assert.NotNull(jacob);
            await _leaveService.RequestLeave(new LeaveRequest {PersonId = jacob.Id});

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
            var jacob = _dbConnection.People.FirstOrDefault(person => person.FirstName == "Jacob");
            Assert.NotNull(jacob);
            var expectedLeaveRequest = new LeaveRequest {PersonId = jacob.Id};
            await _leaveService.RequestLeave(expectedLeaveRequest);
            _servicesFixture.EntityServiceMock.Verify(service => service.Save(It.IsAny<LeaveRequest>()), Times.Once);
        }

        [Fact]
        public async Task EmailThrowingAnErrorWillCauseALeaveRequestToBeDeleted()
        {
            var expectedException = new Exception("email test exception");
            _servicesFixture.EmailServiceMock
                .Setup(email => email.SendEmail(It.IsAny<SendGridMessage>()))
                .Throws(expectedException);
            var jacob = _dbConnection.People.FirstOrDefault(person => person.FirstName == "Jacob");
            Assert.NotNull(jacob);
            var expectedLeaveRequest = new LeaveRequest {PersonId = jacob.Id};
            var actualException =
                await Assert.ThrowsAsync<Exception>(() => _leaveService.RequestLeave(expectedLeaveRequest));
            Assert.Same(expectedException, actualException);
            _servicesFixture.EntityServiceMock.Verify(service => service.Save(expectedLeaveRequest), Times.Once);
            _servicesFixture.EntityServiceMock.Verify(service => service.Delete(expectedLeaveRequest), Times.Once);
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
        public void NotifiesHrWhenAppropriate(LeaveRequest request, PersonWithStaff personWithStaff, int usedLeave,
            int totalYears, bool expectedEmailed)
        {
            var job = _servicesFixture.InsertJob(j =>
            {
                //force job to provide time off
                j.Type = JobType.FullTime;
                j.OrgGroup.Type = GroupType.Department;
            });
            _dbConnection.Insert(personWithStaff);
            _dbConnection.Insert(personWithStaff.Staff);
            //create roles for # of years
            _servicesFixture.InsertRole(job.Id, personWithStaff.Id, totalYears);
            //insert used leave
            _servicesFixture.InsertLeaveRequest(request.Type, personWithStaff.Id, usedLeave);
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
                    new OrgGroupWithSupervisor {Id = departmentId},
                    new OrgGroupWithSupervisor {Id = devisionId},
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
                    new OrgGroupWithSupervisor {Id = departmentId},
                    new OrgGroupWithSupervisor
                    {
                        Id = devisionId,
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
                        ApproverIsSupervisor = false,
                        Supervisor = person4Id,
                        SupervisorPerson = Person(person4Id)
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = devisionId,
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

                yield return ("person with a group inbetween to not notify", new LeaveRequest {PersonId = person1Id},
                    Person(person1Id),
                    new OrgGroupWithSupervisor
                    {
                        Id = departmentId,
                        ApproverIsSupervisor = false,
                        Supervisor = person4Id,
                        SupervisorPerson = Person(person4Id)
                    },
                    new OrgGroupWithSupervisor {Id = devisionId},
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
                        ApproverIsSupervisor = true,
                        Supervisor = person4Id,
                        SupervisorPerson = Person(person4Id)
                    },
                    new OrgGroupWithSupervisor {Id = devisionId},
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
                        ApproverIsSupervisor = true,
                        Supervisor = person1Id,
                        SupervisorPerson = Person(person1Id)
                    },
                    new OrgGroupWithSupervisor {Id = devisionId},
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
                        ApproverIsSupervisor = true,
                        Supervisor = person1Id,
                        SupervisorPerson = Person(person1Id)
                    },
                    new OrgGroupWithSupervisor
                    {
                        Id = devisionId,
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
            }

            return MakeValues().Select(tuple => tuple.ToArray());
        }


        [Theory]
        [MemberData(nameof(GetExpectedEmailValues))]
        public async Task SendsExpectedEmails(string reason,
            LeaveRequest request,
            PersonWithStaff requestedBy,
            OrgGroupWithSupervisor department,
            OrgGroupWithSupervisor devision,
            OrgGroupWithSupervisor supervisorGroup,
            Guid expectedApproverId,
            bool expectApprovalEmailSent,
            int expectedNotifyEmailCount)
        {
            bool actualApprovalEmailSent = false;
            int actualNotifyEmailCount = 0;

            var emailMock = _servicesFixture.EmailServiceMock.As<IEmailService>();
            emailMock.Setup(service => service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<EmailTemplate>(),
                    It.IsAny<PersonWithStaff>(),
                    It.IsAny<PersonWithStaff>()))
                .Returns(Task.CompletedTask)
                .Callback<Dictionary<string, string>, string, EmailTemplate, PersonWithStaff, PersonWithStaff>(
                    (dictionary, subject, template, to, from) =>
                    {
                        if (template == EmailTemplate.RequestLeaveApproval) actualApprovalEmailSent = true;
                        else if (template == EmailTemplate.NotifyLeaveRequest)
                            actualNotifyEmailCount++;
                    });


            var actualApprover = await _leaveService.ResolveLeaveRequestChain(request,
                requestedBy,
                department,
                devision,
                supervisorGroup, new LeaveUseage() {LeaveType = LeaveType.Sick, TotalAllowed = 20, Used = 0});
            Assert.True((expectedApproverId == Guid.Empty) == (actualApprover == null));
            if (actualApprover != null)
                Assert.Equal(expectedApproverId, actualApprover.Id);
            Assert.Equal(expectApprovalEmailSent, actualApprovalEmailSent);
            Assert.Equal(expectedNotifyEmailCount, actualNotifyEmailCount);
        }

        private LeaveRequest GenerateRequest()
        {
            var leaveRequest = AutoFaker.Generate<LeaveRequest>();
            leaveRequest.Approved = null;
            if (leaveRequest.StartDate > leaveRequest.EndDate)
            {
                var tmp = leaveRequest.StartDate;
                leaveRequest.StartDate = leaveRequest.EndDate;
                leaveRequest.EndDate = tmp;
            }

            leaveRequest.OverrideDays = false;
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

        [Fact]
        public void ThrowsForNewRequestsWhenOverridingDays()
        {
            LeaveRequest request = GenerateRequest();
            request.OverrideDays = true;
            Assert.Throws<UnauthorizedAccessException>(() =>
                _leaveService.ThrowIfHrRequiredForUpdate(null, request, request.PersonId));
        }

        [Fact]
        public void FixesDaysIfCalculationIsOff()
        {
            LeaveRequest request = GenerateRequest();

            //16th is a friday
            request.StartDate = new DateTime(2018, 3, 14);
            request.EndDate = new DateTime(2018, 3, 16);
            //should be 3 days
            request.Days = 2;
            _leaveService.ThrowIfHrRequiredForUpdate(null, request, request.PersonId);
            request.Days.ShouldBe(3);
        }

        [Fact]
        public void ThrowsWhenModifyingApprovedLeave()
        {
            LeaveRequest oldRequest = GenerateRequest();
            oldRequest.Approved = true;
            Assert.Throws<UnauthorizedAccessException>(() =>
                _leaveService.ThrowIfHrRequiredForUpdate(oldRequest, oldRequest, oldRequest.PersonId));
            oldRequest.Approved = false;
            Assert.Throws<UnauthorizedAccessException>(() =>
                _leaveService.ThrowIfHrRequiredForUpdate(oldRequest, oldRequest, oldRequest.PersonId));
            oldRequest.Approved = null;
            //doesn't throw
            _leaveService.ThrowIfHrRequiredForUpdate(oldRequest, oldRequest, oldRequest.PersonId);
        }
    }
}