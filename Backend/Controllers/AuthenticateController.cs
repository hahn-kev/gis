using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using IdentityUser = Backend.Entities.IdentityUser;

namespace Backend.Controllers
{
    ///setup using this guide: https://auth0.com/blog/asp-dot-net-core-authentication-tutorial/
    ///and this https://pioneercode.com/post/authentication-in-an-asp-dot-net-core-api-part-3-json-web-token
    [Route("api/[controller]")]
    public class AuthenticateController : MyController
    {
        public const string JwtCookieName = ".JwtAccessToken";
        private readonly JWTSettings _jwtOptions;
        private readonly UserService _userService;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly PersonService _personService;
        private readonly Settings _settings;
        private readonly JwtService _jwtService;

        public AuthenticateController(IOptions<JWTSettings> jwtOptions,
            SignInManager<IdentityUser> signInManager,
            IOptions<Settings> options,
            UserService userService,
            PersonService personService,
            JwtService jwtService)
        {
            _signInManager = signInManager;
            _userService = userService;
            _personService = personService;
            _jwtService = jwtService;
            _jwtOptions = jwtOptions.Value;
            _settings = options.Value;
        }

        [Authorize(Roles = "admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser)
        {
            var user = new IdentityUser().CopyFrom(registerUser);
            user.ResetPassword = true;
            if (string.IsNullOrEmpty(user.Email))
            {
                throw new UserError("User email required");
            }

            var result = await _userService.CreateAsync(user, registerUser.Password);
            if (!result.Succeeded)
            {
                throw result.Errors();
            }

            if (user.Id <= 0)
            {
                throw new ArgumentException("user id not generated error");
            }

            return Json(new {Status = "Success"});
        }
#if DEBUG
        [HttpGet("impersonate/{email}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Impersonate(string email)
        {
            var identityUser = await _userService.FindByEmailAsync(email);
            identityUser.ResetPassword = false;
            return await JsonLoginResult(identityUser);
        }
#endif
        [HttpGet("google")]
        [AllowAnonymous]
        public IActionResult Google(string redirectTo = null)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = string.IsNullOrEmpty(redirectTo) ? "/home" : redirectTo
            };
            if (Request.Cookies.TryGetValue(".Sub", out var subject) && !string.IsNullOrWhiteSpace(subject))
            {
                properties.SetParameter("login_hint", subject);
            }

            return Challenge(properties, "Google");
        }

        public const string ClaimTypeId = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        public const string ClaimTypeFirstName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        public const string ClaimTypeLastName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
        public const string ClaimTypeEmail = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        public const string ClaimPersonId = "personId";
        public const string ClaimSupervisor = "supervisesGroupId";
        public const string ClaimSupervisorType = "supervisesGroupType";
        public const string ClaimLeaveDelegate = "leaveDelegateGroupId";
        public const string GoogleOAuthTokenName = "access_token";

        public const string GisEmailSufix = "@gisthailand.org";

        public async Task GoogleSignIn(AuthenticationProperties authProperties)
        {
            var googleId = User.FindFirstValue(ClaimTypeId);
            IdentityUser user = await _userService.FindByLoginAsync("Google", googleId);

            if (user == null)
            {
                var email = User.FindFirstValue(ClaimTypeEmail);
                if (!email.EndsWith(GisEmailSufix) && email != "hahn.kev@gmail.com")
                {
                    throw new AuthenticationException("Only gis users or khahn are allowed to login with google sso");
                }

                //check for user by email
                user = await _userService.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        Email = email,
                        UserName = email.Substring(0, email.Length - GisEmailSufix.Length)
                    };
                    var newUserResult = await _userService.CreateAsync(user);
                    if (!newUserResult.Succeeded) throw newUserResult.Errors();
                }

                var newLoginResult =
                    await _userService.AddLoginAsync(user, new UserLoginInfo("Google", googleId, User.Identity.Name));
                if (!newLoginResult.Succeeded) throw newLoginResult.Errors();
            }

            var authTokenResult = await _signInManager.UserManager.SetAuthenticationTokenAsync(user,
                "Google",
                GoogleOAuthTokenName,
                authProperties.GetTokenValue(GoogleOAuthTokenName));
            await OnlyHrAndAdminLogin(user);
            if (!authTokenResult.Succeeded) throw authTokenResult.Errors();
            Response.Cookies.Append(JwtCookieName, await _jwtService.GetJwtSecurityTokenAsString(user));
        }

        private async Task OnlyHrAndAdminLogin(IdentityUser user)
        {
            var roles = await _signInManager.UserManager.GetRolesAsync(user);
            if (!roles.Contains("admin") && !roles.Contains("hr") && !roles.Contains("hradmin"))
            {
                throw new UserError("Only admins and HR can login");
            }
        }

        [HttpPost("signin")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn([FromBody] Credentials credentials)
        {
            var identityUser = await _userService.FindByNameAsync(credentials.Username);
            if (identityUser == null) throw ThrowLoginFailed();
            await OnlyHrAndAdminLogin(identityUser);
            return await SignIn(identityUser, credentials.Password);
        }

        [HttpPost("reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetSignIn([FromBody] CredentialsReset credentials)
        {
            var identityUser = await _userService.FindByNameAsync(credentials.Username);
            if (identityUser == null) throw ThrowLoginFailed();
            await OnlyHrAndAdminLogin(identityUser);
            var identityResult =
                await _userService.ChangePasswordAsync(identityUser, credentials.Password, credentials.NewPassword);
            if (!identityResult.Succeeded)
            {
                throw identityResult.Errors();
            }

            identityUser.ResetPassword = false;
            await _userService.UpdateAsync(identityUser);
            return await JsonLoginResult(identityUser);
        }

        private async Task<IActionResult> SignIn(IdentityUser user, string password)
        {
            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (signInResult.IsLockedOut)
            {
                throw new ArgumentException("Account Locked, please contact an administrator");
            }

            if (!signInResult.Succeeded)
            {
                throw ThrowLoginFailed();
            }

            return await JsonLoginResult(user);
        }

        private async Task<IActionResult> JsonLoginResult(IdentityUser user)
        {
            if (user.ResetPassword)
            {
                //don't generate and return an access token if the reset password flag is set
                return Json(new Dictionary<string, object>
                {
                    {"user", new UserProfile(user)}
                });
            }

            var accessTokenString = await _jwtService.GetJwtSecurityTokenAsString(user);
            Response.Cookies.Append(JwtCookieName, accessTokenString);
            return Json(new Dictionary<string, object>
            {
                {"access_token", accessTokenString}
            });
        }

        private Exception ThrowLoginFailed()
        {
            return new UserError("Invalid UserName or Password");
        }
    }
}