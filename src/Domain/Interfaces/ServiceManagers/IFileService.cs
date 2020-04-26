using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LetsWork.Domain.Interfaces.ServiceManagers
{
    public interface IFileService
    {
        Task<List<(string HostedURL, string ResourceName)>> UploadFileToBlob(IFormFileCollection FilesCollection, Guid Id);
    }
}
