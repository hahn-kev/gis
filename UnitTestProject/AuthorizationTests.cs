using Xunit;

namespace UnitTestProject
{
    public class AuthorizationTests : IClassFixture<ServicesFixture>
    {
        private readonly ServicesFixture _servicesFixture;

        public AuthorizationTests(ServicesFixture servicesFixture)
        {
            _servicesFixture = servicesFixture;
            _servicesFixture.SetupPeople();
        }

//        [Fact]
//        public async void UserCanEditSelf()
//        {
//            var client = _servicesFixture.CreateClient();
//            var expectedPhone = Guid.NewGuid().ToString("N");
//            var identityUser = _servicesFixture.AuthenticateAs(client, "jacob");
//            var responseMessage = await client.PutAsJsonAsync("/api/user/self",
//                new RegisterUser
//                {
//                    Id = identityUser.Id,
//                    UserName = identityUser.UserName,
//                    PhoneNumber = expectedPhone
//                });
//
//            responseMessage.StatusCode.ShouldNotBe(HttpStatusCode.Redirect,
//                () => responseMessage.Headers.Location.OriginalString);
//            responseMessage.EnsureSuccessStatusCode();
//
//            var actualUser = _servicesFixture.DbConnection.Users.Single(user => user.Id == identityUser.Id);
//            actualUser.PhoneNumber.ShouldBe(expectedPhone);
//        }
    }
}