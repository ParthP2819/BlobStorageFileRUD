using BlobStorage_File_RtraiveUploadDelete.Models;

namespace BlobStorage_File_RtraiveUploadDelete.Repository.IRepository
{
    public interface IBlobStorageRepo
    {
        Task<List<BlobStorage>> GetAllBlobFiles();
        Task UploadBlobFileAsync(IFormFile file);
        Task DeleteDocumentAsync(string blobName);

        
    }
}
