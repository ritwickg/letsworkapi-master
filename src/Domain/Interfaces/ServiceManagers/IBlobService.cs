using System.IO;
using System.Threading.Tasks;

namespace LetsWork.Domain.Interfaces.ServiceManagers
{
    public interface IBlobService
    {
        Task<string> UploadFileToBlobAsync(string ContainerName, string FileName, Stream FileStream);
        Task DeleteFileFromBlobAsync(string ContainerName, string FileName);
    }
}
