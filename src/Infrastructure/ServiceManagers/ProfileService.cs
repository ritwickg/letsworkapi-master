using LetsWork.Domain.Interfaces.Repositories;
using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using System;
using System.Threading.Tasks;

namespace LetsWork.Infrastructure.ServiceManagers
{
    public class ProfileService : IProfileService
    {
        public readonly IAsyncRepository<ProfileImage> _profileImageRepository;
        public ProfileService(IAsyncRepository<ProfileImage> ProfileImageRepository)
        {
            this._profileImageRepository = ProfileImageRepository;
        }

        public async Task DeleteProfileImageAsync(Guid ProfileImageID)
        {
            ProfileImage profileImage = await _profileImageRepository.GetByIdAsync(ProfileImageID);
            await _profileImageRepository.DeleteAsync(profileImage);
        }
    }
}
