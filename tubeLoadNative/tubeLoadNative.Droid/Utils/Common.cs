using Android.Graphics;
using System.Net;
using System.Threading.Tasks;
using tubeLoadNative.Models;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;
using Android.Content;

namespace tubeLoadNative.Droid.Utils
{
    internal static class Common
    {
        public static Task<Bitmap> GetImageBitmapFromUrlAsync(Context context, string url)
        {
            Task<Bitmap> imageBitmap = null;
            try
            {
                using (var webClient = new WebClient())
                {
                    var imageBytes = webClient.DownloadData(url);

                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        imageBitmap = BitmapFactory.DecodeByteArrayAsync(imageBytes, 0, imageBytes.Length);
                    }
                }
            }
            catch
            {
                return Task.Run(() => (BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.default_song_image)));
            }

            return imageBitmap;
        }

        public static Bitmap GetImageBitmapFromUrl(Context context, string url)
        {
            Bitmap imageBitmap = null;
            try
            {
                using (var webClient = new WebClient())
                {
                    var imageBytes = webClient.DownloadData(url);

                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                    }
                }
            }
            catch
            {
                imageBitmap = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.default_song_image);
            }

            return imageBitmap;
        }

        public static List<SearchResultDownloadItem> GetSearchResultsItems(SearchResult[] searchResults)
        {
            List<SearchResultDownloadItem> outputResults = new List<SearchResultDownloadItem>();

            foreach (var searchResult in searchResults)
            {
                outputResults.Add(new SearchResultDownloadItem(searchResult));
            }

            return outputResults;
        }

    }
}