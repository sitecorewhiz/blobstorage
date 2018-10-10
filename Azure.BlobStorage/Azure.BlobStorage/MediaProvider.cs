using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Events.Hooks;
using Sitecore.Resources.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.BlobStorage
{
    class MediaProvider : Sitecore.Resources.Media.MediaProvider, IHook
    {
        /// <summary>
        /// Property defined in the config
        /// </summary>
        public string OriginPrefix { get; set; }

        /// <summary>
        /// Property defined in the config
        /// </summary>
        public string Sites { get; set; }

        public void Initialize()
        {
            MediaManager.Provider = this;
        }

        public override string GetMediaUrl(MediaItem item)
        {
            string mediaUrl = base.GetMediaUrl(item);
            return GetMediaUrl(mediaUrl, item);
        }

        public override string GetMediaUrl(MediaItem item, MediaUrlOptions options)
        {
            string mediaUrl = base.GetMediaUrl(item, options);
            return GetMediaUrl(mediaUrl, item);
        }

        /// <summary>
        /// Sites that are allows to use the CDN Media Provider
        /// </summary>
        public List<string> AllowedSites
        {
            get
            {
                if (string.IsNullOrEmpty(Sites))
                {
                    return new List<string>();
                }

                return Sites.Split('|').Where(x => !string.IsNullOrEmpty(x)).ToList();
            }
        }

        /// <summary>
        /// Sites that are allows to use the CDN Media Provider
        /// </summary>
        public string GetMediaPath(MediaItem mediaItem, string extension = "")
        {
            string newFileName = MainUtil.EncodeName(mediaItem.Name);
            return (mediaItem.MediaPath.TrimStart('/').Replace(mediaItem.DisplayName, newFileName + "." + extension).ToLower());
        }

        /// <summary>
        /// Determines if we should be pulling from the CDN or not and return item with its version 
        /// </summary>
        /// <param name="mediaUrl"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public string GetMediaUrl(string mediaUrl, MediaItem item)
        {
            if (Context.Database.Name != "core")
            {
                if (string.IsNullOrEmpty(OriginPrefix))
                {
                    return mediaUrl;
                }
                if (mediaUrl.ToLower().Contains("-/media/"))
                {
                    mediaUrl = OriginPrefix + mediaUrl.Substring(mediaUrl.LastIndexOf("-/media/") + 8, mediaUrl.Length - 8 - mediaUrl.LastIndexOf("-/media/")).ToLower();
                }
                if (mediaUrl.ToLower().Contains("?"))
                {
                    mediaUrl = mediaUrl.Split('?')[0];
                }

                mediaUrl = string.Format("{0}?rv={1}", mediaUrl, item.InnerItem.Statistics.Revision);
            }
            return mediaUrl;
        }
    }
}
