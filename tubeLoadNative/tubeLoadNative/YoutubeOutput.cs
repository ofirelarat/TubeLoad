using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tubeLoadNative
{
    public class YoutubeOutput
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public SearchResultSnippet snippet { get; set; }
    }
}
