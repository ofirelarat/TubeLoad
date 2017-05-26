using Android.Graphics;
using System.Net;
using System.Threading.Tasks;
using tubeLoadNative.Models;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;
using Android.Media;

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

        public static int AlphabeticTest(Song s1, Song s2)
        {
            return s1.Name.CompareTo(s2.Name);
        }

        public static int ArtistNameTest(Song s1, Song s2)
        {
            MediaMetadataRetriever mmr1 = SongMetadata.GetMetadata(s1.Id);
            MediaMetadataRetriever mmr2 = SongMetadata.GetMetadata(s2.Id);
            string artistS1 = mmr1.ExtractMetadata(MetadataKey.Date);
            string artistS2 = mmr2.ExtractMetadata(MetadataKey.Date);
            if (artistS1 == null || artistS2 == null)
            {
                return 0;
            }

            return artistS1.CompareTo(artistS2);
        }

        public static int SongDateTest(Song s1, Song s2)
        {
            MediaMetadataRetriever mmr1 = SongMetadata.GetMetadata(s1.Id);
            MediaMetadataRetriever mmr2 = SongMetadata.GetMetadata(s2.Id);
            string dateS1 = mmr1.ExtractMetadata(MetadataKey.Date);
            string dateS2 = mmr2.ExtractMetadata(MetadataKey.Date);
            if (dateS1 == null || dateS2 == null)
            {
                return 0;
            }

            return dateS1.CompareTo(dateS2);
        }
    }
}