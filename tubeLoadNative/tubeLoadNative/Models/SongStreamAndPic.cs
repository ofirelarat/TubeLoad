using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace tubeLoadNative.Models
{
    public class SongStreamAndPic
    {
        public HttpResponseMessage SongStream { get; set; }
        public HttpResponseMessage PicStream { get; set; }
    }
}
