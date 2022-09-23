using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAD.DataCollector.DatabaseBackupFile.Models
{
    [Table("DatabaseBackupFile_stage")]
    public class DatabaseBackupFileModel
    {
        public int? Id { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string StorageAccountAzureId { get; set; }
        public string StorageAccountName { get; set; }
        public string ServerName { get; set; }
        public string InstanceName { get; set; }
        public string DatabaseName { get; set; }
        public string BackupFileName { get; set; }
        public DateTime? LastModifiedDateUtc { get; set; }
        public long? SizeInMb { get; set; }

        public bool IsEqual(DatabaseBackupFileModel azureDatabaseBackupFile)
        {
            return string.Equals(SubscriptionId, azureDatabaseBackupFile.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, azureDatabaseBackupFile.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, azureDatabaseBackupFile.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(StorageAccountAzureId, azureDatabaseBackupFile.StorageAccountAzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(StorageAccountName, azureDatabaseBackupFile.StorageAccountName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ServerName, azureDatabaseBackupFile.ServerName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(InstanceName, azureDatabaseBackupFile.InstanceName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(DatabaseName, azureDatabaseBackupFile.DatabaseName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(BackupFileName, azureDatabaseBackupFile.BackupFileName, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(LastModifiedDateUtc, azureDatabaseBackupFile.LastModifiedDateUtc) &&
                   SizeInMb == azureDatabaseBackupFile.SizeInMb;
        }

        public bool IsDateEqual(DateTime? date1, DateTime? date2)
        {
            if (date1.HasValue && date2.HasValue) {
                return date1 == date2 || date1 - date2 < TimeSpan.FromSeconds(1);
            }

            return !date1.HasValue && !date2.HasValue;
        }
    }
}
