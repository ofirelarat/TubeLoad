using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Apis.YouTube.v3.Data;
using Android.Graphics;
using System.Net;
using System.Net.Http;
using tubeLoadNative.Droid.Utils;
using tubeLoadNative.Services;
using Android.Support.V4.Content;

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Label = "TubeLoad", NoHistory = true)]
    public class DownloadSong : Android.App.Activity
    {
        AndroidSongsManager mediaPlayer = AndroidSongsManager.Instance;

        SearchResult video;
        Button downloadBtn;
        string path;
        TextView videoName;
        ImageView videoImg;
        TextView channelName;
        ProgressBar progressBar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_download_song);

            video = SearchSongs.video;

            videoName = FindViewById<TextView>(Resource.Id.videoName);
            channelName = FindViewById<TextView>(Resource.Id.channelName);
            videoImg = FindViewById<ImageView>(Resource.Id.videoImg);
            progressBar = FindViewById<ProgressBar>(Resource.Id.downloadingProgressBar);
            downloadBtn = FindViewById<Button>(Resource.Id.downloadBtn);
            downloadBtn.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkassets)));

            UpdateView();
        }

        async void OnDownloadClick(object sender, EventArgs e)
        {
            downloadBtn.Enabled = false;
            progressBar.Visibility = ViewStates.Visible;
            string fileName = video.Snippet.Title + ".mp3";

            try
            {
                if(await Downloader.DownloadSong(video.Id.VideoId, fileName))
                {
                    TogglePlay();
                    Toast.MakeText(this, "Download succeed", ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(this, "Download failed", ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();                
            }
            finally
            {
                downloadBtn.Enabled = true;
                progressBar.Visibility = ViewStates.Invisible;
            }
        }

        void OnPlayClick(object sender, EventArgs e)
        {
            mediaPlayer.Start(video.Id.VideoId);
            Intent intent = new Intent(this, typeof(CurrentSong));
            StartActivity(intent);
        }

        private void TogglePlay()
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

        void UpdateView()
        {
            if (video != null)
            {
                Thumbnail logo = video.Snippet.Thumbnails.High;
                Bitmap imageBitmap = Common.GetImageBitmapFromUrl(logo.Url);
                videoImg.SetImageBitmap(imageBitmap);

                videoImg.Click += delegate
                {
                    var uri = Android.Net.Uri.Parse("http://www.youtube.com/watch?v=" + video.Id.VideoId);
                    var intent = new Intent(Intent.ActionView, uri);
                    StartActivity(intent);
                };

                videoName.Text = video.Snippet.Title;
                channelName.Text = video.Snippet.ChannelTitle;
                downloadBtn.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkassets)));

                FileManager.SongsListUpdate();
                path = FileManager.GetSongNameById(video.Id.VideoId);

                if (path != null)
                {
                    TogglePlay();
                }
                else
                {
                    ToggelDwonload();
                }
            }
            else
            {
                Toast.MakeText(this, "Error - Data not available", ToastLength.Long).Show();
                Intent intent = new Intent(this, typeof(SearchSongs));
                StartActivity(intent);
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