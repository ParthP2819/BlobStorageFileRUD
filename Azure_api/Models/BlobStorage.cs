using System.ComponentModel.DataAnnotations;

namespace Azure_api.Models
{
    public class BlobStorage
    {
        [Display(Name = "Filename")]
        public string FileName { get; set; }
        [Display(Name = "FileSize")]
        public string FileSize { get; set; }
        public string Modified { get; set; }
    }
}
