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
using Android.Graphics;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace tubeLoadNative.Droid.Resources
{
    [Activity(Label = "TubeLoad")]
    public class VideoLayout : Android.App.Activity
    {
        private SearchResult myVideo;
        private Button downloadBtn;
        private Java.IO.File dir;
        private Button playBtn;
        private string path;
        private TextView videoName;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.video_layout);

            string inputData = Intent.GetStringExtra("selectedVideo") ?? "Data not available";
            if (!inputData.Equals("Data not available"))
            {
                videoName = FindViewById<TextView>(Resource.Id.videoName);
                TextView channelName = FindViewById<TextView>(Resource.Id.channelName);
                ImageView videoImg = FindViewById<ImageView>(Resource.Id.videoImg);
                //WebView videoView = FindViewById<WebView>(Resource.Id.webViewVideo);
                downloadBtn = FindViewById<Button>(Resource.Id.downloadBtn);
                playBtn = FindViewById<Button>(Resource.Id.playBtn);

                Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
                dir = new Java.IO.File(sdCard.AbsolutePath + "/TubeLoad");
                dir.Mkdirs();

                foreach (SearchResult video in MainActivity.videos)
                {
                    if (inputData.Contains(video.Id.VideoId))
                    {
                        myVideo = video;

                        Thumbnail logo = video.Snippet.Thumbnails.High;

                        Bitmap imageBitmap = GetImageBitmapFromUrl(logo.Url);
                        videoImg.SetImageBitmap(imageBitmap);
                        videoImg.Click += delegate
                        {
                            var uri = Android.Net.Uri.Parse("http://www.youtube.com/watch?v=" + video.Id.VideoId);
                            var intent = new Intent(Intent.ActionView, uri);
                            StartActivity(intent);
                        };

                        videoName.Text = video.Snippet.Title;
                        channelName.Text = video.Snippet.ChannelTitle;
                    }
                }

                path = findSong(dir, videoName.Text);
                if (path != null)
                {
                    playBtn.Visibility = ViewStates.Visible;
                    downloadBtn.Enabled = false;
                }
            }
            else
            {
                Toast.MakeText(this, "Error - Data not available", ToastLength.Long).Show();
                Intent intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            }

            downloadBtn.Click += async delegate
            {
                try
                {
                    downloadBtn.Enabled = false;

                    string FileName = dir + "/" + myVideo.Snippet.Title.Replace("\"", "").Replace("\\", "").Replace("/", "").Replace("*", "").Replace(":", "").Replace("?", "").Replace("|", "").Replace("<", "").Replace(">", "") + ".mp3";

                    await downloadStream(myVideo, FileName);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "Error - " + ex.Message, ToastLength.Long).Show();
                    downloadBtn.Enabled = true;
                }
            };

            playBtn.Click += delegate
            {
                Intent intent = new Intent(this, typeof(mySongs));
                intent.PutExtra("selectedVideo", path);
                StartActivity(intent);
            };

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


        private async Task downloadStream(SearchResult itemSelected, string FileName)
        {
            try
            {
                string videoUrl = "https://www.youtube.com/watch?v=" + itemSelected.Id.VideoId;

                using (var client = new HttpClient())
                {
                    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("http://www.youtubeinmp3.com/fetch/?video=" + videoUrl);
                    httpRequest.Method = "GET";
                    WebResponse response = await httpRequest.GetResponseAsync();
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    saveFile(itemSelected.Id.VideoId, FileName, httpResponse);
                }
            }
            catch { throw; }
        }

        private async void saveFile(string id, string FileName, HttpWebResponse httpResponse)
        {
            try
            {
                using (Stream output = File.OpenWrite(FileName))
                using (Stream input = httpResponse.GetResponseStream())
                {
                     await input.CopyToAsync(output); 
                }

                Toast.MakeText(this, "Download succeed!!", ToastLength.Long).Show();
                string[] file = FileName.Split('/');
                FilesHandler.WriteToJsonFile(FilesHandler.ID_FILE, id, file[file.Length - 1]);

                path = findSong(dir, videoName.Text);

                if (path != null)
                {
                    playBtn.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Error - " + ex.Message, ToastLength.Long).Show();
                downloadBtn.Enabled = true;
            }
        }

        private string findSong(Java.IO.File root, string songName)
        {
            string path = null;
            List<Java.IO.File> al = new List<Java.IO.File>();
            Java.IO.File[] files = root.ListFiles();
            songName = songName.Replace("\"", "").Replace("\\", "").Replace("/", "").Replace("*", "").Replace(":", "").Replace("?", "").Replace("|", "").Replace("<", "").Replace(">", "") + ".mp3";

            foreach (Java.IO.File singleFile in files)
            {
                if ((songName).Contains(singleFile.Name))
                {
                    path = singleFile.Path;
                    return path;
                }
            }

            return path;
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
                    intent = new Intent(this, typeof(mySongs));
                    StartActivity(intent);
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}