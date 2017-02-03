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
using Android.Media;
using Android.Graphics.Drawables;
using System.Threading;
using Android.Graphics;

namespace tubeLoadNative.Droid
{
    [Activity(Label = "TubeLoad", LaunchMode = Android.Content.PM.LaunchMode.SingleInstance)]
    public class CurrentSongActivity : Activity
    {
        ImageButton playBtn;
        TextView songLength;
        TextView songTitle;
        ImageView songImg;
        SeekBar seekBar;
        Thread seekbarThread;
        TextView songPosition;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.current_song_layout);


            songImg = FindViewById<ImageView>(Resource.Id.songImg);
            songTitle = FindViewById<TextView>(Resource.Id.songTitle);
            songLength = FindViewById<TextView>(Resource.Id.songSize);
            songPosition = FindViewById<TextView>(Resource.Id.songPosition);
            seekBar = FindViewById<SeekBar>(Resource.Id.seekBar);

            ImageButton nextBtn = FindViewById<ImageButton>(Resource.Id.nextBtn);
            ImageButton prevBtn = FindViewById<ImageButton>(Resource.Id.prevBtn);
            playBtn = FindViewById<ImageButton>(Resource.Id.playBtn);

            playBtn.SetBackgroundColor(Color.Rgb(41, 128, 185));
            nextBtn.SetBackgroundColor(Color.Rgb(41, 128, 185));
            prevBtn.SetBackgroundColor(Color.Rgb(41, 128, 185));

            if (SongsHandler.CurrentSong != null)
            {
                UpdatePage(SongsHandler.CurrentSong.Id);
            }
            else
            {
                NotificationHandler.DeleteNotification();
                Intent intent = new Intent(this, typeof(mySongs));
                StartActivity(intent);
            }

            nextBtn.Click += delegate
            {
                playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
                playBtn.Click -= Start;
                playBtn.Click += Pause;
                SongsHandler.PlayNext();
                UpdatePage(SongsHandler.CurrentSong.Id);
            };

            prevBtn.Click += delegate
            {
                playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
                playBtn.Click -= Start;
                playBtn.Click += Pause;
                SongsHandler.PlayPrev();
                UpdatePage(SongsHandler.CurrentSong.Id);
            };

            if (SongsHandler.IsPlaying)
            {
                playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
                playBtn.Click += Pause;
            }
            else
            {
                playBtn.SetImageResource(Resource.Drawable.ic_media_play);
                playBtn.Click += Start;
            }

            SongsHandler.OnComplete += delegate
            {
                UpdatePage(SongsHandler.CurrentSong.Id);
            };

            seekBar.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    SongsHandler.SeekTo(e.Progress);
                    songPosition.Text = TimeSpan.FromMilliseconds(SongsHandler.CurrentPosition).TotalMinutes.ToString("0.00").Replace(".", ":");
                }
            };
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (SongsHandler.CurrentSong != null)
            {
                UpdatePage(SongsHandler.CurrentSong.Id);
            }
            else
            {
                NotificationHandler.DeleteNotification();
                Intent intent = new Intent(this, typeof(mySongs));
                StartActivity(intent);
            }

            if (SongsHandler.IsPlaying)
            {
                playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
                playBtn.Click += Pause;
            }
            else
            {
                playBtn.SetImageResource(Resource.Drawable.ic_media_play);
                playBtn.Click += Start;
            }
        }

        private void Start(object sender, EventArgs e)
        {
            SongsHandler.Start();
            playBtn.Click -= Start;
            playBtn.Click += Pause;
            playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
        }

        private void Pause(object sender, EventArgs e)
        {
            SongsHandler.Pause();
            playBtn.Click -= Pause;
            playBtn.Click += Start;
            playBtn.SetImageResource(Resource.Drawable.ic_media_play);
        }

        private void UpdatePage(string songId)
        {
            if (seekbarThread != null)
            {
                seekbarThread.Abort();
            }

            MediaMetadataRetriever mmr = SongsHandler.GetMetadata(songId);
            string title = mmr.ExtractMetadata(MetadataKey.Title) + " - " + mmr.ExtractMetadata(MetadataKey.Artist);
            string length = TimeSpan.FromMilliseconds(Double.Parse(mmr.ExtractMetadata(MetadataKey.Duration))).TotalMinutes.ToString("0.00");

            if (title == null || length == null)
            {
                title = FileHandler.GetSongNameById(songId);
                length = string.Empty;
            }

            songTitle.Text = title;
            songLength.Text = length.Replace(".", ":");

            Drawable picture = SongsHandler.GetSongPicture(songId);
            if (picture != null)
            {
                songImg.SetImageDrawable(picture);
            }
            else
            {
                songImg.SetImageResource(Resource.Drawable.icon);
            }

            seekBar.Max = SongsHandler.Duration;
            seekBar.Progress = SongsHandler.CurrentPosition;
            songPosition.Text = TimeSpan.FromMilliseconds(SongsHandler.CurrentPosition).TotalMinutes.ToString("0.00").Replace(".", ":");

            seekbarThread = new Thread(new ThreadStart(UpdateSekkbarProgress));
            seekbarThread.Start();
        }

        private void UpdateSekkbarProgress()
        {
            while (SongsHandler.CurrentSong != null)
            {
                Thread.Sleep(1000);
                seekBar.Progress = SongsHandler.CurrentPosition;
                try
                {
                    songPosition.Text = TimeSpan.FromMilliseconds(SongsHandler.CurrentPosition).TotalMinutes.ToString("0.00").Replace(".", ":");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message + '\n' + ex.StackTrace);
                }
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_details, menu);

            return base.OnCreateOptionsMenu(menu);
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
