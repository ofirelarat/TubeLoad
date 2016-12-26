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

namespace tubeLoadNative.Droid
{
    [Activity(Label = "TubeLoad")]
    public class CurrentSongActivity : Activity
    {
        ImageButton playBtn;
        TextView songLength;
        TextView songTitle;
        ImageView songImg;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.current_song_layout);

            songImg = FindViewById<ImageView>(Resource.Id.songImg);
            songTitle = FindViewById<TextView>(Resource.Id.songTitle);
            songLength = FindViewById<TextView>(Resource.Id.songSize);

            ImageButton nextBtn = FindViewById<ImageButton>(Resource.Id.nextBtn);
            ImageButton prevBtn = FindViewById<ImageButton>(Resource.Id.prevBtn);
            playBtn = FindViewById<ImageButton>(Resource.Id.playBtn);

            SongsHandler.OnComplete += delegate
            {
                UpdatePage(SongsHandler.CurrentSong.Id);
            };
                nextBtn.Click += delegate
            {
                playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
                playBtn.Click -= Start;
                playBtn.Click += Pause;
                SongsHandler.PlayNext();
                UpdatePage(SongsHandler.CurrentSong.Id);
            };

            prevBtn.Click += delegate
            {
                playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
                playBtn.Click -= Start;
                playBtn.Click += Pause;
                SongsHandler.PlayPrev();
                UpdatePage(SongsHandler.CurrentSong.Id);
            };

            if (SongsHandler.IsPlaying)
            {
                playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
                playBtn.Click += Pause;
            }
            else
            {
                playBtn.Click += Start;
            }

            string currentSongId = Intent.GetStringExtra("currentSongId");
            if (currentSongId != null)
            {
                UpdatePage(currentSongId);
            }
        }

        private void Start(object sender, EventArgs e)
        {
            SongsHandler.Start();
            playBtn.Click -= Start;
            playBtn.Click += Pause;
            playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
        }

        private void Pause(object sender, EventArgs e)
        {
            SongsHandler.Pause();
            playBtn.Click -= Pause;
            playBtn.Click += Start;
            playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_play));
        }

        private void UpdatePage(string songId)
        {
            MediaMetadataRetriever mmr = SongsHandler.GetMetadata(songId);
            string title = mmr.ExtractMetadata(MetadataKey.Title) + " - " + mmr.ExtractMetadata(MetadataKey.Artist);
            string length = TimeSpan.FromMilliseconds(Double.Parse(mmr.ExtractMetadata(MetadataKey.Duration))).TotalMinutes.ToString("#.##");

            if (title == null || length == null)
            {
                title = FileHandler.GetSongNameById(songId);
                length = string.Empty;
            }

            songTitle.Text = title;
            songLength.Text = length;

            Drawable picture = SongsHandler.GetSongPicture(songId);
            if (picture != null)
            {
                songImg.SetImageDrawable(picture);
            }
            else
            {
                songImg.SetImageResource(Resource.Drawable.icon);
            }
        }
    }
}
