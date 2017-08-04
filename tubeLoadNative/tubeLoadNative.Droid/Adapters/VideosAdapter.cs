using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Google.Apis.YouTube.v3.Data;
using Android.Graphics;
using System;
using tubeLoadNative.Droid.Utils;
using System.Linq;
using tubeLoadNative.Models;
using tubeLoadNative.Droid.Activities;
using tubeLoadNative.Services;

namespace tubeLoadNative.Droid
{
    public class VideosAdapter : BaseAdapter
    {
        private Android.App.Activity context;
        private List<SearchResultDownloadItem> searchResults;
        private static LayoutInflater inflater = null;

        private Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();

        public VideosAdapter(Android.App.Activity context, List<SearchResultDownloadItem> searchResults, Dictionary<string, Bitmap> images)
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
                return searchResults.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return searchResults[position].YoutubeResult.Id.VideoId;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            convertView = inflater.Inflate(Resource.Layout.adapter_videos, null);

            TextView videoName = convertView.FindViewById<TextView>(Resource.Id.videoName);
            TextView channelName = convertView.FindViewById<TextView>(Resource.Id.videoChannelName);
            ImageView videoImg = convertView.FindViewById<ImageView>(Resource.Id.videoImg);
            ImageButton downloadButton = convertView.FindViewById<ImageButton>(Resource.Id.searchActivityDownloadButton);

            if (!downloadButton.HasOnClickListeners)
            {
                downloadButton.Click += (sender, e) => OnDownloadClick(sender, e, position);

                DownloadWatcher.onDownloading += (sender, e) =>
                {
                    UpdateVideoButtonByState(position, SearchResultDownloadItem.State.Downloading, (ImageButton)sender);
                };

                DownloadWatcher.onDownloaded += (sender, e) =>
                {
                    UpdateVideoButtonByState(position, SearchResultDownloadItem.State.Downloaded, (ImageButton)sender);
                };

                DownloadWatcher.onDownloadFailed += (sender, e) =>
                {
                    UpdateVideoButtonByState(position, SearchResultDownloadItem.State.Downloadable, (ImageButton)sender);
                };
            }

            var currentVideoYoutube = searchResults[position].YoutubeResult;

            if (AndroidSongsManager.Instance.GetSong(currentVideoYoutube.Id.VideoId) != null)
            {
                UpdateVideoButtonByState(position, SearchResultDownloadItem.State.Downloaded, downloadButton);
            }
            else if (searchResults[position].DownloadState == SearchResultDownloadItem.State.Downloaded)
            {
                UpdateVideoButtonByState(position, SearchResultDownloadItem.State.Downloadable, downloadButton);
            }

            videoName.Text = currentVideoYoutube.Snippet.Title;
            channelName.Text = currentVideoYoutube.Snippet.ChannelTitle;

            Bitmap bitmap;

            if (images.TryGetValue(currentVideoYoutube.Id.VideoId, out bitmap))
            {
                videoImg.SetImageBitmap(bitmap);
            }

            return convertView;
        }

        private void UpdateVideoButtonByState(int position, SearchResultDownloadItem.State state, ImageButton downloadButton)
        {
            SearchResultDownloadItem video = searchResults[position];
            video.DownloadState = state;

            switch (state)
            {
                case SearchResultDownloadItem.State.Downloaded:
                    downloadButton.Enabled = false;
                    downloadButton.Visibility = ViewStates.Gone;
                    break;
                case SearchResultDownloadItem.State.Downloading:
                    downloadButton.Enabled = false;
                    downloadButton.SetImageResource(Resource.Drawable.ic_downloading);
                    break;
                case SearchResultDownloadItem.State.Downloadable:
                    downloadButton.Enabled = true;
                    downloadButton.SetImageResource(Resource.Drawable.ic_download);
                    break;
                default:
                    break;
            }
        }

        private async void OnDownloadClick(object sender, EventArgs e, int videoPosition)
        {
            SearchResultDownloadItem video = searchResults[videoPosition];
            string fileName = video.YoutubeResult.Snippet.Title + ".mp3";
            DownloadWatcher.Download(sender, e);

            try
            {
                if (await Downloader.DownloadSong(video.YoutubeResult.Id.VideoId, fileName))
                {
                    DownloadWatcher.DownloadFinished(sender, e);
                    Toast.MakeText(context, "Download succeed", ToastLength.Short).Show();
                }
                else
                {
                    FailDownload(sender, e);
                }
            }
            catch
            {
                FailDownload(sender, e);
            }
            finally
            {
                NotifyDataSetInvalidated();
            }
        }

        private void FailDownload(object sender, EventArgs e)
        {
            DownloadWatcher.DownloadFailed(sender, e);
            Toast.MakeText(context, "Download failed", ToastLength.Short).Show();
        }
    }
}