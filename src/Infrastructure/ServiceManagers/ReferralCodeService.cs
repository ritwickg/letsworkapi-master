using LetsWork.Domain.Interfaces.Repositories;
using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LetsWork.Infrastructure.ServiceManagers
{
    public class ReferralCodeService : IReferralCodeService
    {
        private readonly IAsyncRepository<ReferralCode> _referralRepository;
        public ReferralCodeService(IAsyncRepository<ReferralCode> ReferralRepository)
        {
            this._referralRepository = ReferralRepository;
        }
        public async Task AddReferralCode(ReferralCode ReferralCodeDetails)
        {
            await _referralRepository.AddAsync(ReferralCodeDetails);
        }
    }
}
