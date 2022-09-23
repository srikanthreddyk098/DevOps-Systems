using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAD.DataCollector.StorageAccountSize.Models
{
    [Table("StorageAccountSize")]
    public class StorageAccountSizeModel
    {
        public int Id { get; set; }
        public string TenantId { get; set; }
        public string AzureId { get; set; }
        public DateTime DateCaptured { get; set; }
        public int NumberOfBlobContainers { get; set; }
        public int BlobSizeInGb { get; set; }
        public int NumberOfBlobs { get; set; }
        public int? FileShareSizeInGb { get; set; }
        public int NumberOfFileShares { get; set; }
    }
}