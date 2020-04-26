using LetsWork.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace LetsWork.Domain.Interfaces.ServiceManagers
{
    public interface ITokenProviderService
    {
        Task<string> GenerateJWTAsync(ApplicationUser RequestingUser);
        JwtSecurityToken DecodeJWTAsync(string EncodedToken);
    }
}
