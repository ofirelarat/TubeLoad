using Android.Graphics;
using System.Net;
using System.Threading.Tasks;
using tubeLoadNative.Models;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;

namespace tubeLoadNative.Droid.Utils
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