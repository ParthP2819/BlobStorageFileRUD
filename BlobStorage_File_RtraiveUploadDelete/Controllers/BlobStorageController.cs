using BlobStorage_File_RtraiveUploadDelete.Models;
using BlobStorage_File_RtraiveUploadDelete.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;

namespace BlobStorage_File_RtraiveUploadDelete.Controllers
{
    public class BlobStorageController : Controller
    {
        private readonly IBlobStorageRepo _blobStorageRepo;
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;

        public BlobStorageController(IBlobStorageRepo blobStorageRepo, IConfiguration configuration)
        {
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
            _blobStorageRepo = blobStorageRepo;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _blobStorageRepo.GetAllBlobFiles());
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [RequestSizeLimit(524288000)]
        public async Task<IActionResult> Upload(IFormFile fileS)
        {
            await _blobStorageRepo.UploadBlobFileAsync(fileS);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(string blobName)
        {
            await _blobStorageRepo.DeleteDocumentAsync(blobName);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Download(string blobName)
        {
            CloudBlockBlob blockBlob;
            await using (MemoryStream memoryS = new MemoryStream())
            {
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_storageConnectionString);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(_storageContainerName);
                blockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
                await blockBlob.DownloadToStreamAsync(memoryS);
            }

            Stream blobStream = blockBlob.OpenReadAsync().Result;
            return File(blobStream, blockBlob.Properties.ContentType, blockBlob.Name);
        }        
    }
}