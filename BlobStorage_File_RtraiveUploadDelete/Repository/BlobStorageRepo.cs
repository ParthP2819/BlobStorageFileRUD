using BlobStorage_File_RtraiveUploadDelete.Models;
using BlobStorage_File_RtraiveUploadDelete.Repository.IRepository;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Reflection.Metadata;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting.Server;
using System.ComponentModel;

namespace BlobStorage_File_RtraiveUploadDelete.Repository
{
    public class BlobStorageRepo : IBlobStorageRepo
    {
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;

        public BlobStorageRepo(IConfiguration configuration)
        {
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
        }

        public async Task<List<BlobStorage>> GetAllBlobFiles()
        {
            try
            {
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_storageConnectionString);
                // Create the blob client.
                CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_storageContainerName);
                CloudBlobDirectory dirb = container.GetDirectoryReference(_storageContainerName);

                BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(string.Empty,
                    true, BlobListingDetails.Metadata, 100, null, null, null);
                List<BlobStorage> FileList = new List<BlobStorage>();

                foreach (var blobItem in resultSegment.Results)
                {
                    var blob = (CloudBlob)blobItem;
                    FileList.Add(new BlobStorage()
                    {
                        FileName = blob.Name,
                        FileSize = Math.Round((blob.Properties.Length / 1024f) / 1024f, 2).ToString(),
                        Modified = DateTime.Parse(blob.Properties.LastModified.ToString()).ToLocalTime().ToString()
                    });
                }
                return FileList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task DeleteDocumentAsync(string blobName)
        {
            try
            {
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_storageConnectionString);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(_storageContainerName);
                var blob = cloudBlobContainer.GetBlobReference(blobName);
                await blob.DeleteIfExistsAsync();
            }
            catch (Exception)
            {

                throw;
            }
           
        }
        
       

        [RequestSizeLimit(524288000)]
        public async Task UploadBlobFileAsync(IFormFile fileS)
        {
            try
            {
                byte[] dataFiles;
                // Retrieve storage account from connection string.
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_storageConnectionString);
                // Create the blob client.
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                // Retrieve a reference to a container.
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(_storageContainerName);

                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                string systemFileName = fileS.FileName;
                await cloudBlobContainer.SetPermissionsAsync(permissions);
                await using (var target = new MemoryStream())
                {
                    fileS.CopyTo(target);
                    dataFiles = target.ToArray();
                }
                // This also does not make a service call; it only creates a local object.
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(systemFileName);
                await cloudBlockBlob.UploadFromByteArrayAsync(dataFiles, 0, dataFiles.Length);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
