using Google.Apis.YouTube.v3.Data;

namespace tubeLoadNative.Models
{
    public class YoutubeOutput
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public SearchResultSnippet snippet { get; set; }
    }
}
