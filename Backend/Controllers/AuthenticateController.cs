using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Backend.Entities;
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
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SecurityTokenHandler _securityTokenHandler;
        private readonly Settings _settings;

        public AuthenticateController(IOptions<JWTSettings> jwtOptions,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IOptions<Settings> options)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _securityTokenHandler = new JwtSecurityTokenHandler();
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
            var result = await _userManager.CreateAsync(user, registerUser.Password);
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

        [HttpGet("google")]
        [AllowAnonymous]
        public IActionResult Google(string redirectTo = null)
        {
            return Challenge(new AuthenticationProperties
                {
                    RedirectUri = string.IsNullOrEmpty(redirectTo) ? "/home" : redirectTo
                },
                "Google");
        }

        public const string ClaimTypeId = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        public const string ClaimTypeFirstName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        public const string ClaimTypeLastName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
        public const string ClaimTypeEmail = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

        public async Task GoogleSignIn()
        {
            var googleId = User.FindFirstValue(ClaimTypeId);
            var user = await _userManager.FindByLoginAsync("Google", googleId);

            if (user == null)
            {
                var email = User.FindFirstValue(ClaimTypeEmail);
                if (!email.EndsWith("@gisthailand.org") && email != "hahn.kev@gmail.com")
                {
                    throw new AuthenticationException("Only gis users or khahn are allowed to login with google sso");
                }
                //check for user by email
                user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        Email = email,
                        UserName = User.Identity.Name.Replace(" ", "_")
                    };
                    var newUserResult = await _userManager.CreateAsync(user);
                    if (!newUserResult.Succeeded) throw newUserResult.Errors();
                }

                var newLoginResult =
                    await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", googleId, User.Identity.Name));
                if (!newLoginResult.Succeeded) throw newLoginResult.Errors();
            }
            Response.Cookies.Append(JwtCookieName, _securityTokenHandler.WriteToken(await GetJwtSecurityToken(user)));
        }

        [HttpPost("signin")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn([FromBody] Credentials credentials)
        {
            var identityUser = await _userManager.FindByNameAsync(credentials.Username);
            if (identityUser == null) throw ThrowLoginFailed();
            return await SignIn(identityUser, credentials.Password);
        }

        [HttpPost("reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetSignIn([FromBody] CredentialsReset credentials)
        {
            var identityUser = await _userManager.FindByNameAsync(credentials.Username);
            if (identityUser == null) throw ThrowLoginFailed();
            var identityResult =
                await _userManager.ChangePasswordAsync(identityUser, credentials.Password, credentials.NewPassword);
            if (!identityResult.Succeeded)
            {
                throw identityResult.Errors();
            }
            identityUser.ResetPassword = false;
            await _userManager.UpdateAsync(identityUser);
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
            var token = await GetJwtSecurityToken(user);
            var accessTokenString = _securityTokenHandler.WriteToken(token);
            Response.Cookies.Append(JwtCookieName, accessTokenString);
            return Json(new Dictionary<string, object>
            {
                {"access_token", accessTokenString}
            });
        }

        private async Task<JwtSecurityToken> GetJwtSecurityToken(IdentityUser identityUser)
        {
            var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(identityUser);
            return new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: GetTokenClaims(identityUser).Union(claimsPrincipal.Claims),
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
                    SecurityAlgorithms.HmacSha256)
            );
        }

        private static IEnumerable<Claim> GetTokenClaims(IdentityUser identityUser)
        {
            return new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, identityUser.Id.ToString())
            };
        }

        private Exception ThrowLoginFailed()
        {
            return new UserError("Invalid UserName or Password");
        }
    }
}