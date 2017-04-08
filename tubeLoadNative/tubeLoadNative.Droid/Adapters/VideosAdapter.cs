using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Google.Apis.YouTube.v3.Data;
using Android.Graphics;
using System;
using tubeLoadNative.Droid.Utils;
using System.Linq;
using tubeLoadNative.Models;

namespace tubeLoadNative.Droid
{
    public class VideosAdapter : BaseAdapter
    {
        private Android.App.Activity context;
        private List<SearchResultDownloadItem> searchResults;
        private static LayoutInflater inflater = null;

        private Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();
    
        public VideosAdapter(Android.App.Activity context, SearchResult[] searchResults, Dictionary<string, Bitmap> images)
        {
            this.context = context;
            this.searchResults = GetSearchResultsItems(searchResults);
            this.images = images;
            inflater = (LayoutInflater)context.GetSystemService(Android.Content.Context.LayoutInflaterService);       
        }

        private List<SearchResultDownloadItem> GetSearchResultsItems(SearchResult[] searchResults)
        {
            List<SearchResultDownloadItem> outputResults = new List<SearchResultDownloadItem>();

            foreach (var searchResult in searchResults)
            {
                outputResults.Add(new SearchResultDownloadItem(searchResult));
            }

            return outputResults;
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

            Console.WriteLine(videoName.Text + " - " + position);

            if (!downloadButton.HasOnClickListeners)
            {
                downloadButton.Click += (sender, e) => OnDownloadClick(sender, e, position);
            }

            var currentVideoYoutube = searchResults[position].YoutubeResult;
            
            if (AndroidSongsManager.Instance.GetSong(currentVideoYoutube.Id.VideoId) != null)
            {
                searchResults[position].DownloadState = SearchResultDownloadItem.State.Downloaded;
            }

            UpdateVideoButtonByState(searchResults[position], downloadButton);

            videoName.Text = currentVideoYoutube.Snippet.Title;
            channelName.Text = currentVideoYoutube.Snippet.ChannelTitle;

            Bitmap bitmap;

            if (images.TryGetValue(currentVideoYoutube.Id.VideoId, out bitmap))
            {
                videoImg.SetImageBitmap(bitmap);
            }

            return convertView;
        }

        private void UpdateVideoButtonByState(SearchResultDownloadItem video, ImageButton downloadButton)
        {
            switch (video.DownloadState)
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
            var downloadButton = (ImageButton)sender;
            downloadButton.Enabled = false;
            downloadButton.SetImageResource(Resource.Drawable.ic_downloading);
            string fileName = video.YoutubeResult.Snippet.Title + ".mp3";
            video.DownloadState = SearchResultDownloadItem.State.Downloading;

            try
            {
                if (await Downloader.DownloadSong(video.YoutubeResult.Id.VideoId, fileName))
                {
                    video.DownloadState = SearchResultDownloadItem.State.Downloaded;
                    downloadButton.Visibility = ViewStates.Gone;
                    Toast.MakeText(context, "Download succeed", ToastLength.Short).Show();
                }
                else
                {
                    FailDownload(downloadButton, video);
                }
            }
            catch
            {
                FailDownload(downloadButton, video);
            }
            finally
            {
                NotifyDataSetInvalidated();
            }
        }

        private void FailDownload(ImageButton downloadButton, SearchResultDownloadItem video)
        {
            video.DownloadState = SearchResultDownloadItem.State.Downloadable;
            downloadButton.SetImageResource(Resource.Drawable.ic_download);
            downloadButton.Enabled = true;
            Toast.MakeText(context, "Download failed", ToastLength.Short).Show();
        }
    }
}