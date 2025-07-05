using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Diagnostics;

namespace client.App_Code
{
    public class AzureBlobStorageService
    {

        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobStorageService(IConfiguration config)
        {
            _connectionString = config["AzureStorage:ConnectionString"];
            _containerName = config["AzureStorage:ContainerName"];
        }
        //Guid.NewGuid()
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            var uniqueFileName = Helper.GenerateRandomStringU(8) + Path.GetExtension(file.FileName);
            var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);
  
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    Debug.WriteLine("String is Null or empty 1 ");
                    return false;
                }

                var blobServiceClient = new BlobServiceClient(_connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                var blobName = GetBlobNameFromUrl(imageUrl);
                Debug.WriteLine("Deleting blob " + blobName);
                if (string.IsNullOrEmpty(blobName))
                {
                    Debug.WriteLine("String is Null or empty 2 ");
                    return false;
                }

                var blobClient = blobContainerClient.GetBlobClient(blobName);
                var response = await blobClient.DeleteIfExistsAsync();

                return response.Value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Virhe poistossa : "+ex.Message);
                // Loggaa virhe tarvittaessa
                return false;
            }
        }

        private string GetBlobNameFromUrl(string blobUrl)
        {
            try
            {
                var uri = new Uri(blobUrl);
                // Erotellaan koko polku
                var fullPath = uri.AbsolutePath.TrimStart('/');
                var blobName = fullPath.Substring(fullPath.IndexOf('/') + 1);
                return blobName;
            }
            catch
            {
                return null;
            }
        }


    }
}
