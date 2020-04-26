using System;
using System.Threading.Tasks;

namespace LetsWork.Domain.Interfaces.ServiceManagers
{
    public interface IProfileService
    {
        Task DeleteProfileImageAsync(Guid ProfileImageID);
    }
}

