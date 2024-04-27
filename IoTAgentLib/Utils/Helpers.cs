using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace IoTAgentLib.Utils
{
    public static class Helpers
    {
        public async static Task<List<string>> GetContainerNamesAsync()
        {
            List<string> result = new List<string>();

            BlobServiceClient blobServiceClient = new BlobServiceClient(Utils.Config.BLOB_CONNECTION_STRING);

            var containers = blobServiceClient.GetBlobContainersAsync();
            await foreach (var container in containers)
            {
                result.Add(container.Name);
            }

            return result;
        }

        public static async Task UploadBlobAsync(string containerName, string blobName, string filePath)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(Utils.Config.BLOB_CONNECTION_STRING);
            BlobContainerClient containerClient;

            containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(filePath);
        }
    }
}
