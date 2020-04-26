using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LetsWork.Infrastructure.ServiceManagers
{
    public class BlobService : IBlobService
    {
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly ConfigSettings _configSettings;
        public BlobService(IOptions<ConfigSettings> ConfigurationSettingsOptions)
        {
            _configSettings = ConfigurationSettingsOptions.Value;

            // Retrieve the connection string for blob storage
            string storageConnectionString = _configSettings.AzureBlobStorage.ConnectionString;

            CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount);

            // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
            _cloudBlobClient = storageAccount.CreateCloudBlobClient();
        }
        public async Task<string> UploadFileToBlobAsync(string ContainerName, string FileName, Stream FileStream)
        {
            try
            {
                if (string.IsNullOrEmpty(ContainerName))
                    throw new ArgumentNullException(nameof(ContainerName), "Container name cannot be null!");

                if (string.IsNullOrEmpty(FileName))
                    throw new ArgumentNullException(nameof(FileName), "File Name cannot be null!");

                if (FileStream == null)
                    throw new ArgumentNullException(nameof(FileStream), "File stream cannot be null");

                //1) Create a container
                CloudBlobContainer blobContainer = _cloudBlobClient.GetContainerReference(ContainerName);
                await blobContainer.CreateIfNotExistsAsync();

                //2) Set the permissions so the blobs are public.
                BlobContainerPermissions permissions = new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob };
                await blobContainer.SetPermissionsAsync(permissions);

                //3) Get a reference to the blob address, then upload the file to the blob.
                CloudBlockBlob cloudBlockBlob = blobContainer.GetBlockBlobReference(FileName);
                await cloudBlockBlob.UploadFromStreamAsync(FileStream);

                //4) Return the hosted URL of the blob resource
                return cloudBlockBlob.Uri.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task DeleteFileFromBlobAsync(string ContainerName, string ResourceName)
        {
            try
            {
                //1) Get Blob Container Reference
                CloudBlobContainer blobContainer = _cloudBlobClient.GetContainerReference(ContainerName);

                //2) Get Resource Reference
                CloudBlockBlob _blockBlob = blobContainer.GetBlockBlobReference(ResourceName);

                //3) Delete resource from Blob Container    
                await _blockBlob.DeleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}