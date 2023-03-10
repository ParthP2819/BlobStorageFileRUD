using Azure.Storage.Blobs;
using Azure_api.Models;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text;

namespace Azure_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        List<BlobStorage> FileList = new List<BlobStorage>();
        public FileController(IConfiguration configuration)
        {
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
        }

        [HttpGet]
        [Route("GetAll/")]
        public async Task<IActionResult> GetAllBlobFiles()
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            // Create the blob client.
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_storageContainerName);
            //CloudBlobDirectory dirb = container.GetDirectoryReference(_storageContainerName);

            BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(string.Empty,
                true, BlobListingDetails.Metadata, 100, null, null, null);
           

            foreach (var blobItem in resultSegment.Results)
            {
                var blob = (CloudBlob)blobItem;
                FileList.Add(new BlobStorage()
                {
                    FileName = blob.Name,
                    FileSize = Math.Round((blob.Properties.Length / 1024f) / 1024f, 2).ToString(),
                    Modified = DateTime.Parse(blob.Properties.LastModified.ToString()).
                    ToLocalTime().ToString()
                });
            }
            return Ok(FileList);

        }

        [HttpPost(nameof(DownloadFile))]
        public async Task<IActionResult> DownloadFile(string fileName, Name name)
        {
            CloudBlockBlob blockBlob;
            await using (MemoryStream memoryStream = new MemoryStream())
            {

                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_storageConnectionString);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(_storageContainerName);
                blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                await blockBlob.DownloadToStreamAsync(memoryStream);
            }
            Stream blobStream = blockBlob.OpenReadAsync().Result;
            return File(blobStream, blockBlob.Properties.ContentType, blockBlob.Name);
        }

        [HttpGet(nameof(Down))]
        public async Task<IActionResult> Down(Name name)
        {
            CloudBlockBlob blockBlob;
            return Ok();
        }

        [HttpDelete(nameof(DeleteFile))]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(_storageContainerName);
            var blob = cloudBlobContainer.GetBlobReference(fileName);
            await blob.DeleteIfExistsAsync();
            return Ok("File Deleted");
        }

        [HttpPost("[action]")]
        [RequestSizeLimit(524288000)]
        public async Task<IActionResult> UploadFilesToStorage(IList<IFormFile> files, string Roles)
        {
            BlobContainerClient blobContainerClient = new BlobContainerClient(_storageConnectionString, "blob-storage-parth");

            foreach (IFormFile file in files)
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    await blobContainerClient.UploadBlobAsync($"{Roles}/{file.FileName}", stream);
                }
            }
            return Ok("File Uploaded Successfully!.....");
        }

        [HttpPost("UploadLargeFile")]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<IActionResult> UploadLargeFile(IFormFile file)
        {
            var blobName = file.FileName;

            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_storageContainerName);
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference(blobName);
            var fileStream = file.OpenReadStream();

            var bufferSize = 20 * 1024 * 1024; // 20 MB
            var buffer = new byte[bufferSize];
            int bytesRead;

            List<string> blockList = new List<string>();
            var blockId = Guid.NewGuid().ToString();
            var blockNum = 0;

            while ((bytesRead = fileStream.Read(buffer, 0, bufferSize)) > 0)
            {
                var base64BlockId = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{blockId}{blockNum++:0000000}"));
                 await blob.PutBlockAsync(base64BlockId, new MemoryStream(buffer, 0, bytesRead), null);
                blockList.Add(base64BlockId);
            }

            await blob.PutBlockListAsync(blockList);

            return Ok("File uploaded successfully.");
        }

        [HttpPost("[action]")]
        [RequestSizeLimit(524288000)]
        public async Task<IActionResult> UploadFnameF(IFormFile file)
        {
            BlobContainerClient blobContainerClient = new BlobContainerClient(_storageConnectionString, "blob-storage-parth");
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                var FN = file.FileName;
                var name = FN.Split(".");
                var name1 = name[0];
                await blobContainerClient.UploadBlobAsync($"{name1}/{file.FileName}", stream);
            }
            return Ok("File Uploaded Successfully!.....");
        }

    }
}

