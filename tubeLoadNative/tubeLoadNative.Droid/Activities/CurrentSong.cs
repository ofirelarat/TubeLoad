using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Media;
using Android.Graphics.Drawables;
using System.Threading;
using Android.Graphics;
using tubeLoadNative.Droid.Utils;

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Label = "TubeLoad", LaunchMode = Android.Content.PM.LaunchMode.SingleInstance)]
    public class CurrentSong : Activity
    {
        AndroidSongsManager mediaPlayer = AndroidSongsManager.Instance;

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
            SetContentView(Resource.Layout.activity_current_song);


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

            if (mediaPlayer.CurrentSong != null)
            {
                UpdatePage(mediaPlayer.CurrentSong.Id);
            }
            else
            {
                NotificationHandler.DeleteNotification();
                Intent intent = new Intent(this, typeof(SongsPlayer));
                StartActivity(intent);
            }

            nextBtn.Click += delegate
            {
                playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
                playBtn.Click -= Start;
                playBtn.Click += Pause;
                mediaPlayer.PlayNext();
                UpdatePage(mediaPlayer.CurrentSong.Id);
            };

            prevBtn.Click += delegate
            {
                playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
                playBtn.Click -= Start;
                playBtn.Click += Pause;
                mediaPlayer.PlayPrev();
                UpdatePage(mediaPlayer.CurrentSong.Id);
            };

            if (mediaPlayer.IsPlaying)
            {
                playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
                playBtn.Click += Pause;
            }
            else
            {
                playBtn.SetImageResource(Resource.Drawable.ic_media_play);
                playBtn.Click += Start;
            }

            mediaPlayer.Completing += delegate
            {
                UpdatePage(mediaPlayer.CurrentSong.Id);
            };

            seekBar.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    mediaPlayer.SeekTo(e.Progress);
                    songPosition.Text = TimeSpan.FromMilliseconds(mediaPlayer.CurrentPosition).ToString(@"mm\:ss");
                }
            };
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (mediaPlayer.CurrentSong != null)
            {
                UpdatePage(mediaPlayer.CurrentSong.Id);
            }
            else
            {
                NotificationHandler.DeleteNotification();
                Intent intent = new Intent(this, typeof(SongsPlayer));
                StartActivity(intent);
            }

            if (mediaPlayer.IsPlaying)
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

        void Start(object sender, EventArgs e)
        {
            mediaPlayer.Start();
            playBtn.Click -= Start;
            playBtn.Click += Pause;
            playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
        }

        void Pause(object sender, EventArgs e)
        {
            mediaPlayer.Pause();
            playBtn.Click -= Pause;
            playBtn.Click += Start;
            playBtn.SetImageResource(Resource.Drawable.ic_media_play);
        }

        void UpdatePage(string songId)
        {
            if (seekbarThread != null)
            {
                seekbarThread.Abort();
            }

            MediaMetadataRetriever mmr = SongMetadata.GetMetadata(songId);
            string title = mmr.ExtractMetadata(MetadataKey.Title) + " - " + mmr.ExtractMetadata(MetadataKey.Artist);
            string length = TimeSpan.FromMilliseconds(Double.Parse(mmr.ExtractMetadata(MetadataKey.Duration))).ToString(@"mm\:ss");

            if (title == null || length == null)
            {
                title = FileManager.GetSongNameById(songId);
                length = string.Empty;
            }

            songTitle.Text = title;
            songLength.Text = length.Replace(".", ":");

            Drawable picture = SongMetadata.GetSongPicture(songId);
            if (picture != null)
            {
                songImg.SetImageDrawable(picture);
            }
            else
            {
                songImg.SetImageResource(Resource.Drawable.icon);
            }

            seekBar.Max = mediaPlayer.Duration;
            seekBar.Progress = mediaPlayer.CurrentPosition;
            songPosition.Text = TimeSpan.FromMilliseconds(mediaPlayer.CurrentPosition).ToString(@"mm\:ss");

            seekbarThread = new Thread(new ThreadStart(UpdateSekkbarProgress));
            seekbarThread.Start();
        }

        void UpdateSekkbarProgress()
        {
            while (mediaPlayer.CurrentSong != null)
            {
                Thread.Sleep(1000);
                seekBar.Progress = mediaPlayer.CurrentPosition;
                try
                {
                    songPosition.Text = TimeSpan.FromMilliseconds(mediaPlayer.CurrentPosition).ToString(@"mm\:ss");
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
            inflater.Inflate(Resource.Menu.main_menu, menu);

            return base.OnCreateOptionsMenu(menu);
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
                    intent = new Intent(this, typeof(SongsPlayer));
                    StartActivity(intent);
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}
