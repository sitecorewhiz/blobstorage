using log4net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.BlobStorage
{
    public class AzureStorageUpload
    {
        private readonly CloudBlobContainer container;
        public ILog Logger = LoggerFactory.GetLogger("Sitecore.Diagnostics.cdnUploading");
        public AzureStorageUpload()
        {
            CloudStorageAccount storageAccount;
            CloudBlobClient blobClient;

            var accountName = Settings.GetSetting("Azure.AccountName");
            var primaryKey = Settings.GetSetting("Azure.AccountPrimaryKey");
            var endpointProtocol = Settings.GetSetting("Azure.EndpointsProtocol");


            var containerName = Settings.GetSetting("Azure.ContainerName");

            string connectionString = string.Format("DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2};", endpointProtocol, accountName, primaryKey);

            //use ConfigurationManager to retrieve the connection string
            storageAccount = CloudStorageAccount.Parse(connectionString);

            //create a CloudBlobClient object using the storage account to retrieve objects that represent containers and blobs stored within the Blob Storage Service
            blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            container = blobClient.GetContainerReference(containerName);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            //By default, the new container is private and you must specify your storage access key to download blobs from this container. If you want to make the files within the container available to everyone, you can set the container to be public
            container.SetPermissions(
                new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });
        }

        public void UploadMediaToAzure(MediaItem mediaItem, string extension = "", string language = "")
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(GetMediaPath(mediaItem, extension));
            blockBlob.DeleteIfExists();

            if (string.IsNullOrEmpty(mediaItem.Extension))
                return;

            if (mediaItem.HasMediaStream("Media"))
            {
                using (var fileStream = (System.IO.FileStream)mediaItem.GetMediaStream())
                {
                    blockBlob.Properties.ContentType = mediaItem.MimeType;
                    blockBlob.UploadFromStream(fileStream);
                }
            }
            else
            {
                blockBlob.DeleteIfExists();
            }

            Item item = mediaItem;
            using (new EditContext(item, SecurityCheck.Disable))
            {
                item["CDN file path"] = GetMediaPath(mediaItem, extension);
                item["Uploaded to Cloud CDN"] = "1";
            }

            UpdateCDNFilePathWebDBItem(mediaItem, extension, item);


            Logger.Info(string.Format("CDN File Uploaded : {0}", GetMediaPath(mediaItem, extension)));
        }

        public void DeleteMediaFromAzure(MediaItem mediaItem, string extension = "", string language = "")
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(GetMediaPath(mediaItem, extension));
            blockBlob.DeleteIfExists();

            Logger.Info(string.Format(" CDN File Deleted : {0}", GetMediaPath(mediaItem, extension)));
        }

        public void ReplaceMediaFromAzure(MediaItem mediaItem, string extension = "", string language = "")
        {

            //Delete old files from the blob
            Item item = mediaItem;
            if (!string.IsNullOrEmpty(item["CDN file path"]))
            {
                CloudBlockBlob oldblockBlob = container.GetBlockBlobReference(item["CdnFilePath"]);
                oldblockBlob.DeleteIfExists();

                Logger.Info(string.Format(" CDN File Deleted : {0}  ", item["CdnFilePath"].ToLower()));
            }


            CloudBlockBlob blockBlob = container.GetBlockBlobReference(GetMediaPath(mediaItem, extension));

            if (string.IsNullOrEmpty(mediaItem.Extension))
                return;
            using (var fileStream = (System.IO.FileStream)mediaItem.GetMediaStream())
            {
                blockBlob.Properties.ContentType = mediaItem.MimeType;
                blockBlob.UploadFromStream(fileStream);
            }


            using (new EditContext(item, SecurityCheck.Disable))
            {
                item["CDN file path"] = GetMediaPath(mediaItem, extension);
                item["Uploaded to Cloud CDN"] = "1";
            }

            UpdateCDNFilePathWebDBItem(mediaItem, extension, item);

            Logger.Info(string.Format("CDN File Uploaded : {0}", GetMediaPath(mediaItem, extension)));
        }

        private void UpdateCDNFilePathWebDBItem(MediaItem mediaItem, string extension, Item item)
        {
            var db = Sitecore.Configuration.Factory.GetDatabase("web");
            Item webDbItem = db.GetItem(item.ID);
            if (webDbItem != null)
            {
                using (new SecurityDisabler())
                {
                    webDbItem.Editing.BeginEdit();
                    webDbItem["CDN file path"] = GetMediaPath(mediaItem, extension);
                    webDbItem["Uploaded to Cloud CDN"] = "1";
                    webDbItem.Editing.EndEdit();
                }
            }
        }

        public string GetMediaPath(MediaItem mediaItem, string extension = "")
        {
            string AzureSyncPath = string.Empty;
            string newFileName = Sitecore.MainUtil.EncodeName(mediaItem.Name);
            if (!string.IsNullOrEmpty(AzureSyncPath))
                return (AzureSyncPath + "\\" + mediaItem.MediaPath.TrimStart('/').Replace(mediaItem.DisplayName, newFileName + "." + extension).ToLower());
            else
                return (mediaItem.MediaPath.TrimStart('/').Replace(mediaItem.DisplayName, newFileName + "." + extension).ToLower());
        }
    }
}
