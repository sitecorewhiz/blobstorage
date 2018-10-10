using Sitecore.Resources.Media;
using System.Web;

namespace Azure.BlobStorage
{
    public class AzureBlobStorageMediaRequestHandler : Sitecore.Resources.Media.MediaRequestHandler
    {
        protected override bool DoProcessRequest(HttpContext context)
        {
            return base.DoProcessRequest(context);
        }

        protected override MediaRequest GetMediaRequest(HttpRequest request)
        {
            return base.GetMediaRequest(request);
        }
    }
}
