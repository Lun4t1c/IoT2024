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
        /// <summary>
        /// Gets blob container names from Azure
        /// </summary>
        /// <returns><c>List<string></c> with names of containers</returns>
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

        /// <summary>
        /// Uploads blob to container
        /// </summary>
        /// <param name="containerName">Name of container</param>
        /// <param name="blobName">Name of blob in container</param>
        /// <param name="filePath">Path to file to be uploaded as blob</param>
        /// <returns></returns>
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
