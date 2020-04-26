using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LetsWork.Infrastructure.ServiceManagers
{
    public class TokenProviderService : ITokenProviderService
    {
        private readonly ConfigSettings _configSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        public TokenProviderService (IOptions<ConfigSettings> ConfigurationSettingsOptions, UserManager<ApplicationUser> UserManager)
        {
            _configSettings = ConfigurationSettingsOptions.Value;
            _userManager = UserManager;
        }       
        public async Task<string> GenerateJWTAsync(ApplicationUser RequestingUser)
        {
            try
            {
                IList<string> roleList = await _userManager.GetRolesAsync(RequestingUser);
              
                List<Claim> claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, RequestingUser.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, RequestingUser.Id.ToString()),
                    new Claim(ClaimTypes.Role,roleList[0]),
                    new Claim(JwtRegisteredClaimNames.Iat,DateTime.UtcNow.ToString())
                };
                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configSettings.JWT.Secret));
                SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                DateTime expires = DateTime.UtcNow.AddMinutes(60);

                JwtSecurityToken token = new JwtSecurityToken(
                    _configSettings.JWT.Issuer,
                    _configSettings.JWT.Issuer,
                    claims,
                    expires: expires,
                    signingCredentials: credentials
                );

                string jwtSecurityToken = new JwtSecurityTokenHandler().WriteToken(token);
                return jwtSecurityToken;
            }
            catch (Exception)
            {
                throw;
            }  
        }

        public JwtSecurityToken DecodeJWTAsync(string EncodedToken)
        {
            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken decodedToken = handler.ReadToken(EncodedToken) as JwtSecurityToken;
                return decodedToken;
            }
            catch(Exception)
            {
                return null;
            }
        }
    }
}
