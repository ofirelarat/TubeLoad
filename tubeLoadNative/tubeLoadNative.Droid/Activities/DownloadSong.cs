using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Apis.YouTube.v3.Data;
using Android.Graphics;
using tubeLoadNative.Droid.Utils;
using Android.Support.V4.Content;
using tubeLoadNative.Models;
using tubeLoadNative.Services;
using Android.Gms.Ads;

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Label = "TubeLoad", NoHistory = true)]
    public class DownloadSong : Android.App.Activity
    {
        AndroidSongsManager mediaPlayer = AndroidSongsManager.Instance;
        SearchResultDownloadItem video;
        Button downloadBtn;
        TextView videoName;
        ImageView videoImg;
        TextView channelName;
        ProgressBar progressBar;
        AdView bannerad;
        private const string bannerID = "ca-app-pub-2772184448965971/2833438017";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_download_song);

            GoogleAnalyticsService.Instance.Initialize(this);
            GoogleAnalyticsService.Instance.TrackAppPage("Download Song");

            video = SearchSongs.getSelectedVideo();
            videoName = FindViewById<TextView>(Resource.Id.videoName);
            channelName = FindViewById<TextView>(Resource.Id.channelName);
            videoImg = FindViewById<ImageView>(Resource.Id.videoImg);
            progressBar = FindViewById<ProgressBar>(Resource.Id.downloadingProgressBar);
            downloadBtn = FindViewById<Button>(Resource.Id.downloadBtn);
            downloadBtn.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkassets)));

            DownloadWatcher.onDownloaded += (sender, e) => TogglePlay();
            DownloadWatcher.onDownloadFailed += (sender, e) => ToggelDownload();

            bannerad = AdWrapper.ConstructStandardBanner(this, AdSize.SmartBanner, bannerID);
            bannerad.CustomBuild();

            UpdateView();
        }

        async void OnDownloadClick(object sender, EventArgs e)
        {
            string fileName = video.YoutubeResult.Snippet.Title + ".mp3";
            video.DownloadState = SearchResultDownloadItem.State.Downloading;
            ToggleDownloading();

            try
            {
                if (await Downloader.DownloadSong(video.YoutubeResult.Id.VideoId, fileName))
                {
                    video.DownloadState = SearchResultDownloadItem.State.Downloaded;
                    DownloadWatcher.DownloadFinished(null, null);

                    GoogleAnalyticsService.Instance.TrackAppEvent(GoogleAnalyticsService.GAEventCategory.DonloadingSong, $"Donwloaded {video.YoutubeResult.Snippet.Title}");
                    Toast.MakeText(Application.Context, "Download succeed", ToastLength.Short).Show();
                }
                else
                {
                    GoogleAnalyticsService.Instance.TrackAppEvent(GoogleAnalyticsService.GAEventCategory.DownloadFailed, $"Download {video.YoutubeResult.Snippet.Title} failed");
                    video.DownloadState = SearchResultDownloadItem.State.Downloadable;
                    DownloadWatcher.DownloadFailed(null, null);
                    Toast.MakeText(Application.Context, "Download failed", ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                GoogleAnalyticsService.Instance.TrackAppException(video.YoutubeResult.Snippet.Title + "\t" + ex.Message, true);
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Long).Show();
                video.DownloadState = SearchResultDownloadItem.State.Downloadable;
                DownloadWatcher.DownloadFailed(null, null);
            }
        }

        void OnPlayClick(object sender, EventArgs e)
        {
            mediaPlayer.Start(video.YoutubeResult.Id.VideoId);
            Intent intent = new Intent(Application.Context, typeof(CurrentSong));
            StartActivity(intent);
        }

        private void TogglePlay()
        {
            ActivateDownloadButton();
            downloadBtn.Text = "Play";
            downloadBtn.Click -= OnDownloadClick;
            downloadBtn.Click += OnPlayClick;
        }

        private void ToggelDownload()
        {
            ActivateDownloadButton();
            downloadBtn.Text = "Download";
            downloadBtn.Click -= OnPlayClick;
            downloadBtn.Click += OnDownloadClick;
        }

        private void ActivateDownloadButton()
        {
            progressBar.Visibility = ViewStates.Invisible;
            downloadBtn.Enabled = true;
        }

        private void ToggleDownloading()
        {
            progressBar.Visibility = ViewStates.Visible;
            downloadBtn.Enabled = false;
        }

        void UpdateView()
        {
            if (video != null)
            {
                Thumbnail logo = video.YoutubeResult.Snippet.Thumbnails.High;
                Bitmap imageBitmap = Common.GetImageBitmapFromUrl(this,logo.Url);
                videoImg.SetImageBitmap(imageBitmap);

                videoImg.Click += delegate
                {
                    var uri = Android.Net.Uri.Parse("http://www.youtube.com/watch?v=" + video.YoutubeResult.Id.VideoId);
                    var intent = new Intent(Intent.ActionView, uri);
                    StartActivity(intent);
                };

                videoName.Text = video.YoutubeResult.Snippet.Title;
                channelName.Text = video.YoutubeResult.Snippet.ChannelTitle;
                downloadBtn.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkassets)));

                updateViewReferState(video.DownloadState);
            }
            else
            {
                Toast.MakeText(this, "Error - Data not available", ToastLength.Long).Show();
                Intent intent = new Intent(this, typeof(SearchSongs));
                StartActivity(intent);
            }
        }

        public void updateViewReferState(SearchResultDownloadItem.State DownloadState)
        {
            switch (DownloadState)
            {
                case (SearchResultDownloadItem.State.Downloadable):
                {
                    ToggelDownload();
                    break;
                }
                case (SearchResultDownloadItem.State.Downloaded):
                {
                    TogglePlay();
                    break;
                }
                case (SearchResultDownloadItem.State.Downloading):
                {
                    ToggleDownloading();
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        protected override void OnResume()
        {
            if (bannerad != null)
                bannerad.Resume();
            base.OnResume();
        }
        protected override void OnPause()
        {
            if (bannerad != null)
                bannerad.Pause();
            base.OnPause();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.main_menu, menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent intent;

            switch (item.ItemId)
            {
                case Resource.Id.addSong:
                    intent = new Intent(this, typeof(SearchSongs));
                    StartActivity(intent);
                    return true;

                case Resource.Id.mySong:
                    if (mediaPlayer.Songs.Count > 0)
                    {
                        intent = new Intent(this, typeof(SongsPlayer));
                        StartActivity(intent);
                    }
                    else
                    {
                        Toast.MakeText(this, "first download some songs", ToastLength.Long).Show();
                    }
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}