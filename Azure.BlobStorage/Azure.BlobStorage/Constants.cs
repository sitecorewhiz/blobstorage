using Sitecore.Data;

namespace Azure.BlobStorage
{
    public class AzureBlobStorageConstants
    {
        /// <summary>
        /// path: /sitecore/templates/System/Media/Unversioned/File/Extended information/CDN file path
        /// Description: The path to the media item in Azure (not including the container name)
        /// </summary>
        public static readonly ID AzureStoragePathFieldID = new ID("{2C434CFF-2C29-461A-A1E9-54F8EE457575}");

        // 
        /// <summary>
        /// Path: /sitecore/templates/System/Media/Unversioned/File/Extended information/Uploaded To Cloud CDN
        /// Description: Checkbox field that is true if the media item has been uploaded
        /// </summary>
        public static readonly ID HasBeenUploadedToAzureFieldID = new ID("{BC52E204-EE4D-46F7-A6D1-C0C82B0760A6}");
    }
}
