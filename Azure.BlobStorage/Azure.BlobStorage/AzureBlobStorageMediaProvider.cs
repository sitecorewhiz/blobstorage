using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.BlobStorage
{
    public class AzureBlobStorageMediaProvider : Sitecore.Resources.Media.MediaProvider
    {
        public override string GetMediaUrl(MediaItem item, MediaUrlOptions options)
        {
            // If the Azure path exists and isn't empty, and the flag which says the item has been uploaded is TRUE
            if (item.InnerItem.Fields[AzureBlobStorageConstants.AzureStoragePathFieldID] != null && !string.IsNullOrWhiteSpace(item.InnerItem[AzureBlobStorageConstants.AzureStoragePathFieldID])
                && item.InnerItem.Fields[AzureBlobStorageConstants.HasBeenUploadedToAzureFieldID] != null && ((CheckboxField)item.InnerItem.Fields[AzureBlobStorageConstants.HasBeenUploadedToAzureFieldID]).Checked)
            {
                var blobEndpoint = Settings.GetSetting("Azure.BlobEndpoint");
                var containerName = Settings.GetSetting("Azure.ContainerName");
                var sasToken = Settings.GetSetting("Azure.PublicSASToken");
                var mediaItemPath = item.InnerItem[AzureBlobStorageConstants.AzureStoragePathFieldID];

                return string.Format(@"{0}/{1}/{2}?{3}", blobEndpoint, containerName, mediaItemPath, sasToken);
            }

            return base.GetMediaUrl(item, options);
        }
    }
}
