using Android.Graphics;
using Android.Graphics.Drawables;
using System.Net;
using System.Threading.Tasks;

namespace tubeLoadNative.Droid
{
    internal static class Common
    {
        public static Task<Bitmap> GetImageBitmapFromUrlAsync(string url)
        {
            Task<Bitmap> imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArrayAsync(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }

        public static Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }
    }
}