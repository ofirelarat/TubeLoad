using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Google.Apis.YouTube.v3.Data;
using Android.Graphics;

namespace tubeLoadNative.Droid
{
    public class VideosAdapter : BaseAdapter
    {
        private Android.App.Activity context;
        private SearchResult[] searchResults;
        private static LayoutInflater inflater = null;

        private Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();
    
        public VideosAdapter(Android.App.Activity context,SearchResult[] searchResults)
        {
            this.context = context;
            this.searchResults = searchResults;
            inflater = (LayoutInflater)context.GetSystemService(Android.Content.Context.LayoutInflaterService);

            foreach (var result in searchResults)
            {
                Thumbnail logo = result.Snippet.Thumbnails.High;
                Bitmap imageBitmap = Common.GetImageBitmapFromUrl(logo.Url);
                images.Add(result.Id.VideoId,imageBitmap);
            }
        }

        public override int Count
        {
            get
            {
                return searchResults.Length;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return searchResults[position].Id.VideoId;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                convertView = inflater.Inflate(Resource.Layout.custom_row, null);
            }

            TextView videoName = convertView.FindViewById<TextView>(Resource.Id.videoName);
            TextView channelName = convertView.FindViewById<TextView>(Resource.Id.videoChannelName);
            ImageView videoImg = convertView.FindViewById<ImageView>(Resource.Id.videoImg);

            videoName.Text = searchResults[position].Snippet.Title;
            channelName.Text = searchResults[position].Snippet.ChannelTitle;

            Bitmap bitmap;

            if (images.TryGetValue(searchResults[position].Id.VideoId, out bitmap))
            {
                videoImg.SetImageBitmap(bitmap);
            }

            return convertView;
        }
    }
}