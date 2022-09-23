using System;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace AzureStorageAccountSASGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string storageAccount = args[0];
            string container = args[1];
            string blobEndpoint = $"https://{storageAccount}.blob.core.windows.net";
            BlobServiceClient blobClient = new BlobServiceClient(new Uri(blobEndpoint), new DefaultAzureCredential());

            var startDate = DateTimeOffset.UtcNow;
            var expiryDate = startDate.AddDays(1);

            // Get a user delegation key for the Blob service that's valid for 7 days.
            // You can use the key to generate any number of shared access signatures 
            // over the lifetime of the key.
            UserDelegationKey userDelegationKey = blobClient.GetUserDelegationKey(startDate, expiryDate);

            // Create a SAS token 
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = container,
                Resource = "c",
                StartsOn = startDate,
                ExpiresOn = expiryDate
            };

            // Specify read and write permissions for the SAS.
            sasBuilder.SetPermissions(BlobSasPermissions.Read |
                                      BlobSasPermissions.Write);
            
            // Specify racwl permissions for the SAS.
            sasBuilder.SetPermissions(
                BlobContainerSasPermissions.Read |
                BlobContainerSasPermissions.Add |
                BlobContainerSasPermissions.Create |
                BlobContainerSasPermissions.Write |
                BlobContainerSasPermissions.List
            );

            //// Add the SAS token to the container URI.
            //BlobUriBuilder blobUriBuilder = new BlobUriBuilder(new Uri(blobEndpoint))
            //{
            //    // Specify the user delegation key.
            //    Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, storageAccount)
            //};

            //Console.WriteLine("Container user delegation SAS URI: {0}", blobUriBuilder);

            var userDelegationSas = sasBuilder.ToSasQueryParameters(userDelegationKey, storageAccount);
            Console.WriteLine(userDelegationSas.ToString());
        }
    }
}
