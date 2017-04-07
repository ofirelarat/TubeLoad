using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Google.Apis.YouTube.v3.Data;
using Android.Graphics;
using System;
using tubeLoadNative.Droid.Utils;

namespace tubeLoadNative.Droid
{
    public class VideosAdapter : BaseAdapter
    {
        private Android.App.Activity context;
        private SearchResult[] searchResults;
        private static LayoutInflater inflater = null;

        private Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();
    
        public VideosAdapter(Android.App.Activity context,SearchResult[] searchResults, Dictionary<string, Bitmap> images)
        {
            this.context = context;
            this.searchResults = searchResults;
            this.images = images;
            inflater = (LayoutInflater)context.GetSystemService(Android.Content.Context.LayoutInflaterService);       
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
                convertView = inflater.Inflate(Resource.Layout.adapter_videos, null);
            }

            TextView videoName = convertView.FindViewById<TextView>(Resource.Id.videoName);
            TextView channelName = convertView.FindViewById<TextView>(Resource.Id.videoChannelName);
            ImageView videoImg = convertView.FindViewById<ImageView>(Resource.Id.videoImg);
            ImageButton downloadButton = convertView.FindViewById<ImageButton>(Resource.Id.searchActivityDownloadButton);

            if (FileManager.GetSongNameById(searchResults[position].Id.VideoId) == null)
            {
                downloadButton.Click += (sender, e) => OnDownloadClick(sender, e, searchResults[position]);
            }
            else
            {
                downloadButton.Enabled = false;
                downloadButton.Visibility = ViewStates.Gone;
            }

            videoName.Text = searchResults[position].Snippet.Title;
            channelName.Text = searchResults[position].Snippet.ChannelTitle;

            Bitmap bitmap;

            if (images.TryGetValue(searchResults[position].Id.VideoId, out bitmap))
            {
                videoImg.SetImageBitmap(bitmap);
            }

            return convertView;
        }

        private async void OnDownloadClick(object sender, EventArgs e, SearchResult video)
        {
            var downloadButton = (ImageButton)sender;
            downloadButton.Enabled = false;
            downloadButton.SetImageResource(Resource.Drawable.ic_downloading);
            string fileName = video.Snippet.Title + ".mp3";

            try
            {
                if (await Downloader.DownloadSong(video.Id.VideoId, fileName))
                {
                    downloadButton.Visibility = ViewStates.Gone;
                    Toast.MakeText(context, "Download succeed", ToastLength.Short).Show();
                }
                else
                {
                    FailDownload(downloadButton);
                }
            }
            catch
            {
                FailDownload(downloadButton);
            }
        }

        private void FailDownload(ImageButton downloadButton)
        {
            downloadButton.SetImageResource(Resource.Drawable.ic_download);
            downloadButton.Enabled = true;
            Toast.MakeText(context, "Download failed", ToastLength.Short).Show();
        }
    }
}