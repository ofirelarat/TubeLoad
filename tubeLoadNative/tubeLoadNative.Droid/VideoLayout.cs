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

namespace tubeLoadNative.Droid.Resources
{
    [Activity(Label = "TubeLoad" ,NoHistory = true)]
    public class VideoLayout : Android.App.Activity
    {
        private SearchResult video;
        private Button downloadBtn;
        private string path;
        private TextView videoName;
        private ImageView videoImg;
        private TextView channelName;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.video_layout);

            video = MainActivity.video;

            videoName = FindViewById<TextView>(Resource.Id.videoName);
            channelName = FindViewById<TextView>(Resource.Id.channelName);
            videoImg = FindViewById<ImageView>(Resource.Id.videoImg);
            downloadBtn = FindViewById<Button>(Resource.Id.downloadBtn);

            UpdateView(video);    
        }

        private async void OnDownloadClick(object sender, EventArgs e)
        {
            HttpResponseMessage response;
            downloadBtn.Enabled = false;
            string FileName = video.Snippet.Title + ".mp3";

            // Erasing illegal charachters from file name
            string[] forbiddenChars = { "|", "\\", "?", "*", "<", "\"", ":", ">"};
            foreach (string c in forbiddenChars)
            {
                FileName = FileName.Replace(c, string.Empty);
            }

            try
            {
                response = await YoutubeHandler.downloadStream(video.Id.VideoId);
            }
            catch(Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                downloadBtn.Enabled = true;

                return;
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (await SongsHandler.Instance.SaveSong(FileHandler.PATH, FileName, video.Id.VideoId, await response.Content.ReadAsStreamAsync()))
                {
                    downloadBtn.Enabled = true;
                    downloadBtn.Text = "Play";
                    downloadBtn.Click -= OnDownloadClick;
                    downloadBtn.Click += OnPlayClick;

                    Toast.MakeText(this, "Download succeed", ToastLength.Short).Show();
                }
                else
                {
                    downloadBtn.Enabled = true;
                    Toast.MakeText(this, "Download failed", ToastLength.Short).Show();
                }
            }
        }

        private void OnPlayClick(object sender, EventArgs e)
        {
            SongsHandler.Play(video.Id.VideoId);
            Intent intent = new Intent(this, typeof(mySongs));
            StartActivity(intent);
        }

        private void UpdateView(SearchResult video)
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
                downloadBtn.SetBackgroundColor(Color.Rgb(41, 128, 185));

                SongsHandler.CheckFilesExist();
                path = FileHandler.GetSongNameById(video.Id.VideoId);

                if (path != null)
                {
                    downloadBtn.Text = "Play";
                    downloadBtn.Click += OnPlayClick;
                }
                else
                {
                    downloadBtn.Text = "Download";
                    downloadBtn.Click += OnDownloadClick;
                }
            }
            else
            {
                Toast.MakeText(this, "Error - Data not available", ToastLength.Long).Show();
                Intent intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            }
        }
        
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_details, menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent intent;

            switch (item.ItemId)
            {
                case Resource.Id.addSong:
                    intent = new Intent(this, typeof(MainActivity));
                    StartActivity(intent);
                    return true;

                case Resource.Id.mySong:
                    if (SongsHandler.Songs.Count > 0)
                    {
                        intent = new Intent(this, typeof(mySongs));
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