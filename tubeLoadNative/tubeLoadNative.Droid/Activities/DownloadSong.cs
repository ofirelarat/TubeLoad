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

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Label = "TubeLoad", NoHistory = true)]
    public class DownloadSong : Android.App.Activity
    {
        static AndroidSongsManager mediaPlayer = AndroidSongsManager.Instance;
        static SearchResultDownloadItem video;
        static Button downloadBtn;
        TextView videoName;
        ImageView videoImg;
        TextView channelName;
        static ProgressBar progressBar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_download_song);
            video = SearchSongs.getSelectedVideo();
            videoName = FindViewById<TextView>(Resource.Id.videoName);
            channelName = FindViewById<TextView>(Resource.Id.channelName);
            videoImg = FindViewById<ImageView>(Resource.Id.videoImg);
            progressBar = FindViewById<ProgressBar>(Resource.Id.downloadingProgressBar);
            downloadBtn = FindViewById<Button>(Resource.Id.downloadBtn);
            downloadBtn.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkassets)));

            UpdateView();
        }

        async static void OnDownloadClick(object sender, EventArgs e)
        {
            downloadBtn.Enabled = false;
            progressBar.Visibility = ViewStates.Visible;
            string fileName = video.YoutubeResult.Snippet.Title + ".mp3";
            video.DownloadState = SearchResultDownloadItem.State.Downloading;

            try
            {
                if(await Downloader.DownloadSong(video.YoutubeResult.Id.VideoId, fileName))
                {
                    TogglePlay();
                    Toast.MakeText(Application.Context, "Download succeed", ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(Application.Context, "Download failed", ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Long).Show();                
            }
            finally
            {
                downloadBtn.Enabled = true;
                progressBar.Visibility = ViewStates.Invisible;
            }
        }

        static void OnPlayClick(object sender, EventArgs e)
        {
            mediaPlayer.Start(video.YoutubeResult.Id.VideoId);
            Intent intent = new Intent(Application.Context, typeof(CurrentSong));
            Application.Context.StartActivity(intent);
        }

        private static void TogglePlay()
        {
            downloadBtn.Text = "Play";
            downloadBtn.Click -= OnDownloadClick;
            downloadBtn.Click += OnPlayClick;
        }

        private void ToggelDwonload()
        {
            downloadBtn.Text = "Download";
            downloadBtn.Click -= OnPlayClick;
            downloadBtn.Click += OnDownloadClick;
        }

        private static void ToggleDownloading()
        {
            progressBar.Visibility = ViewStates.Visible;
            downloadBtn.Enabled = false;
        }

        void UpdateView()
        {
            if (video != null)
            {
                Thumbnail logo = video.YoutubeResult.Snippet.Thumbnails.High;
                Bitmap imageBitmap = Common.GetImageBitmapFromUrl(logo.Url);
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

        public static void updateStateFromDownloadEvent() 
        {
            progressBar.Visibility = ViewStates.Invisible;
            downloadBtn.Enabled = true;
            TogglePlay();
        }

        public void updateViewReferState(SearchResultDownloadItem.State DownloadState)
        {
            switch(DownloadState)
            {
                case (SearchResultDownloadItem.State.Downloadable):
                    {
                        ToggelDwonload();
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