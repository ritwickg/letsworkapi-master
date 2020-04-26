using LetsWork.Domain.Models;
using System.Threading.Tasks;

namespace LetsWork.Domain.Interfaces.ServiceManagers
{
    public interface IReferralCodeService
    {
        Task AddReferralCode(ReferralCode ReferralCodeDetails);
    }
}
