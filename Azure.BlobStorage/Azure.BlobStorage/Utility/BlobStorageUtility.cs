using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.BlobStorage.Utility
{
    public class BlobStorageUtility
    {
        private CloudStorageAccount StorageAccount { get; set; }
        private CloudBlobClient BlobClient { get; set; }

        /// <summary>
        /// Instantiate the utility with connections to the Azure Blob Storage CDN
        /// </summary>
        /// <param name="accountName">e.g. demostoragepoc</param>
        /// <param name="primaryKey">e.g. /8oVewThisKeyIsFakePii5A/V9ePleaseDontUseIt73I/KSjeNykDoesntWork182kgnA==</param>
        /// <param name="endpointProtocol">e.g. https</param>
        public BlobStorageUtility(string accountName, string primaryKey, string endpointProtocol)
        {
            //AzureSyncPath = Settings.GetSetting("Azure.SyncFolder");
            string connectionString = string.Format("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2};", endpointProtocol, accountName, primaryKey);

            //use ConfigurationManager to retrieve the connection string
            StorageAccount = CloudStorageAccount.Parse(connectionString);

            //create a CloudBlobClient object using the storage account to retrieve objects that represent containers and blobs stored within the Blob Storage Service
            BlobClient = StorageAccount.CreateCloudBlobClient();
        }


        /// <summary>
        /// Upload a blob to blob storage.
        /// </summary>
        /// <param name="containerName">e.g. sitecorepdfcontainer</param>
        /// <param name="blobStream">The blob's filestream</param>
        /// <param name="blobFileNameAndPath">e.g. {path_in_media_library}/icon-restore.png</param>
        /// <param name="mimeType">e.g. image/jpeg, image/gif, etc</param>
        /// <param name="blobEndpoint">Optional to create a fully useable return URL, but not required for upload. e.g. https://demostoragepoc.blob.core.windows.net</param>
        /// <param name="sasToken">Optional to create a fully useable return URL, but not required for upload. e.g. sv=2017-04-17&amp;ss=bfqt&amp;srt=sco&amp;sp=r&amp;se=2018-07-31T05:12:38Z&amp;st=2018-01-18T21:12:38Z&amp;spr=https,http&amp;sig=NotTheRealSigSoDontUseThisPlease%3D</param>
        /// <returns>A path representing the uploaded file (fully qualified if</returns>
        public string UploadBlob(string containerName, FileStream blobStream, string blobFileNameAndPath, string mimeType, string blobEndpoint = "", string sasToken = "")
        {
            CloudBlobContainer container = GetBlobContainer(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobFileNameAndPath.TrimStart('/'));
            blockBlob.DeleteIfExists();

            blockBlob.Properties.ContentType = mimeType;
            blockBlob.UploadFromStream(blobStream);

            if (!string.IsNullOrWhiteSpace(sasToken))
            {
                sasToken = "?" + sasToken;
            }

            return $"{blobEndpoint}/{containerName}/{blobFileNameAndPath}{sasToken}"; ;
        }

        /// <summary>
        /// Deletes a blob from storage
        /// </summary>
        /// <param name="containerName">e.g. sitecorepdfcontainer</param>
        /// <param name="blobFileNameAndPath">e.g. {path_in_media_library}/icon-restore.png</param>
        public void DeleteBlob(string containerName, string blobFileNameAndPath)
        {
            CloudBlockBlob blockBlob = GetBlobContainer(containerName).GetBlockBlobReference(blobFileNameAndPath);
            blockBlob.DeleteIfExists();
        }

        /// <summary>
        /// Gets the public URL to the blob, along with the public SAS token
        /// </summary>
        /// <param name="blobEndpoint">e.g. https://demostoragepoc.blob.core.windows.net</param>
        /// <param name="containerName">e.g. sitecorepdfcontainer</param>
        /// <param name="blobName">e.g. {path_in_media_library}/icon-restore.png</param>
        /// <param name="sasToken">e.g. sv=2017-04-17&amp;ss=bfqt&amp;srt=sco&amp;sp=r&amp;se=2018-07-31T05:12:38Z&amp;st=2018-01-18T21:12:38Z&amp;spr=https,http&amp;sig=NotTheRealSigSoDontUseThisPlease%3D</param>
        /// <returns></returns>
        public string GetPublicBlobPath(string blobEndpoint, string containerName, string blobName, string sasToken)
        {
            return $"{blobEndpoint}/{containerName}/{blobName}?{sasToken}";
        }

        private CloudBlobContainer GetBlobContainer(string containerName)
        {
            CloudBlobContainer container = BlobClient.GetContainerReference(containerName);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            //By default, the new container is private and you must specify your storage access key to download blobs from this container. If you want to make the files within the container available to everyone, you can set the container to be public
            container.SetPermissions(
                new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });

            return container;
        }
    }
}
