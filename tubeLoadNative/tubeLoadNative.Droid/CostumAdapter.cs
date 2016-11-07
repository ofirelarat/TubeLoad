using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Apis.YouTube.v3.Data;
using Java.Lang;
using Android.Graphics;
using System.Net;

namespace tubeLoadNative.Droid
{
    public class CostumAdapter : BaseAdapter
    {
        private Android.App.Activity context;
        private SearchResult[] searchResults;
        private static LayoutInflater inflater = null;

        private Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();
    
        public CostumAdapter(Android.App.Activity context,SearchResult[] searchResults)
        {
            this.context = context;
            this.searchResults = searchResults;
            inflater = (LayoutInflater)context.GetSystemService(Android.App.Activity.LayoutInflaterService);

            foreach (var result in searchResults)
            {
                Thumbnail logo = result.Snippet.Thumbnails.High;
                Bitmap imageBitmap = GetImageBitmapFromUrl(logo.Url);
                images.Add(result.Id.VideoId,imageBitmap);
            }
        }

        public override int Count
        {
            get
            {
                return this.searchResults.Length;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return this.searchResults[position].Id.VideoId;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View vi = convertView;
            if (vi == null)
            {
                vi = inflater.Inflate(Resource.Layout.custom_row, null);
            }

            TextView videoName = vi.FindViewById<TextView>(Resource.Id.videoName);
            TextView channelName = vi.FindViewById<TextView>(Resource.Id.videoChannelName);
            ImageView videoImg = vi.FindViewById<ImageView>(Resource.Id.videoImg);

            videoName.Text = this.searchResults[position].Snippet.Title;
            channelName.Text = this.searchResults[position].Snippet.ChannelTitle;

            Bitmap bitmap;
            if (images.TryGetValue(this.searchResults[position].Id.VideoId, out bitmap))
            {
                videoImg.SetImageBitmap(bitmap);
            }

            return vi;
        }

        private Bitmap GetImageBitmapFromUrl(string url)
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