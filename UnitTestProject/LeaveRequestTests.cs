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
        private DbConnection _dbConnection;
        private ServicesFixture _servicesFixture;

        public LeaveRequestTests()
        {
            _servicesFixture = new ServicesFixture();
        }

        private void Setup()
        {
            _leaveRequestService = _servicesFixture.Get<LeaveRequestService>();
            _dbConnection = _servicesFixture.Get<DbConnection>();
        }

        [Fact]
        public async Task FindsSupervisor()
        {
            _servicesFixture = new ServicesFixture(collection =>
            {
                collection.RemoveAll<IEmailService>().AddSingleton(Mock.Of<IEmailService>());
            });
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
                        It.Is<PersonExtended>(extended => extended.FirstName == "Jacob"),
                        It.Is<PersonExtended>(extended => extended.FirstName == "Bob")),
                Times.Once);
            
            emailMock.Verify(service =>
                    service.SendTemplateEmail(It.IsAny<Dictionary<string, string>>(),
                        It.IsAny<string>(),
                        It.Is<PersonExtended>(extended => extended.FirstName != "Jacob"),
                        It.Is<PersonExtended>(extended => extended.FirstName != "Bob")),
                Times.Never);
        }
    }
}