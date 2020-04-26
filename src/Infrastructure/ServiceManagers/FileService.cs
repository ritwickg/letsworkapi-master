using LetsWork.Domain.Interfaces.ServiceManagers;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LetsWork.Infrastructure.ServiceManagers
{
    public class FileService : IFileService
    {
        private readonly IBlobService _blobService;
        public FileService(IBlobService BlobService)
        {
            _blobService = BlobService;
        }
        public async Task<List<(string HostedURL, string ResourceName)>> UploadFileToBlob(IFormFileCollection FilesCollection, Guid Id)
        {
            try
            {
                List<(string HostedURL, string ResourceName)> hostedImageURLList = new List<(string HostedURL, string ResourceName)>();
                foreach (IFormFile fileToUpload in FilesCollection)
                {
                    string fileName = ContentDispositionHeaderValue.Parse(fileToUpload.ContentDisposition).FileName.Trim('"');
                    string hostedURL = await _blobService.UploadFileToBlobAsync(Id.ToString(), fileName, fileToUpload.OpenReadStream());
                    hostedImageURLList.Add((HostedURL: hostedURL, ResourceName: fileName));
                }
                return hostedImageURLList;

            }
            catch (StorageException)
            {
                throw;
            }
          
        }
    }
}
