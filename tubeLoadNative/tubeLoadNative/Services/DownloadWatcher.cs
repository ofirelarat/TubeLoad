using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tubeLoadNative.Services
{
    public static class DownloadWatcher
    {
        public static event EventHandler onDownloaded;
        public static event EventHandler onDownloading;
        public static event EventHandler onDownloadFailed;
        
        public static void Download(object sender, EventArgs e)
        {
            onDownloading?.Invoke(sender, e);
        }

        public static void DownloadFinished(object sender, EventArgs e)
        {
            onDownloaded?.Invoke(sender, e);
        }
        public static void DownloadFailed(object sender, EventArgs e)
        {
            onDownloadFailed?.Invoke(sender, e);
        }
    }
}
