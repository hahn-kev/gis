using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Backend.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using IdentityUser = Backend.Entities.IdentityUser;

namespace Backend.Services
{
    public class JwtService
    {
        private readonly JWTSettings _jwtOptions;
        private readonly UserService _userService;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly SecurityTokenHandler _securityTokenHandler = new JwtSecurityTokenHandler();
        private readonly PersonService _personService;

        public JwtService(IOptions<JWTSettings> jwtOptions,
            UserService userService,
            SignInManager<IdentityUser> signInManager,
            PersonService personService)
        {
            _jwtOptions = jwtOptions.Value;
            _userService = userService;
            _signInManager = signInManager;
            _personService = personService;
        }

        public async Task<string> GetJwtSecurityTokenAsString(string userName)
        {
            return await GetJwtSecurityTokenAsString(await _userService.FindByNameAsync(userName));
        }
        public async Task<string> GetJwtSecurityTokenAsString(IdentityUser identityUser)
        {
            var token = await GetJwtSecurityToken(identityUser);
            return _securityTokenHandler.WriteToken(token);
        }

        public async Task<JwtSecurityToken> GetJwtSecurityToken(IdentityUser identityUser)
        {
            var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(identityUser);
            var oauthToken =
                await _signInManager.UserManager.GetAuthenticationTokenAsync(identityUser,
                    "Google",
                    AuthenticateController.GoogleOAuthTokenName);
            return new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: GetTokenClaims(identityUser, oauthToken).Union(claimsPrincipal.Claims),
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
                    SecurityAlgorithms.HmacSha256)
            );
        }

        public IEnumerable<Claim> GetTokenClaims(IdentityUser identityUser, string googleOauthToken)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, identityUser.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, identityUser.Email ?? string.Empty),
                new Claim("oauth", googleOauthToken ?? string.Empty)
            };
            if (identityUser.PersonId.HasValue)
            {
                claims.Add(new Claim(AuthenticateController.ClaimPersonId, identityUser.PersonId.ToString()));
                var group = _userService.FindGroupIdIfSupervisor(identityUser.PersonId.Value);
                if (@group != null)
                {
                    claims.Add(new Claim(AuthenticateController.ClaimSupervisor, @group.Id.ToString()));
                    claims.Add(new Claim(AuthenticateController.ClaimSupervisorType, @group.Type.ToString()));
                }

                var personWithStaff = _personService.GetStaffById(identityUser.PersonId.Value);

                if (personWithStaff.Staff?.LeaveDelegateGroupId != null)
                    claims.Add(new Claim(AuthenticateController.ClaimLeaveDelegate,
                        personWithStaff.Staff.LeaveDelegateGroupId.Value.ToString()));
            }

            return claims;
        }
    }
}