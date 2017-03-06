using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Media;
using Android.Graphics.Drawables;
using Android.Graphics;
using tubeLoadNative.Droid.Utils;
using tubeLoadNative.Droid.Views;
using Android.Support.V4.Content;

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Label = "TubeLoad", LaunchMode = Android.Content.PM.LaunchMode.SingleInstance)]
    public class CurrentSong : Activity
    {
        AndroidSongsManager mediaPlayer = AndroidSongsManager.Instance;

        ImageButton playBtn;
        TextView songTitle;
        ImageView songImg;
        SeekbarView seekbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_current_song);
            
            songImg = FindViewById<ImageView>(Resource.Id.songImg);
            songTitle = FindViewById<TextView>(Resource.Id.songTitle);
            seekbar = FindViewById<SeekbarView>(Resource.Id.seekbar);

            ImageButton nextBtn = FindViewById<ImageButton>(Resource.Id.nextBtn);
            ImageButton prevBtn = FindViewById<ImageButton>(Resource.Id.prevBtn);
            playBtn = FindViewById<ImageButton>(Resource.Id.playBtn);

            playBtn.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkassets)));
            nextBtn.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkassets)));
            prevBtn.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkassets)));

            checkCurrentSong();

            nextBtn.Click += delegate
            {
                TogglePlay();
                mediaPlayer.PlayNext();
                UpdatePage(mediaPlayer.CurrentSong.Id);
            };

            prevBtn.Click += delegate
            {
                TogglePlay();
                mediaPlayer.PlayPrev();
                UpdatePage(mediaPlayer.CurrentSong.Id);
            };

            ChangePlayingView();

            mediaPlayer.Completing += delegate
            {
                UpdatePage(mediaPlayer.CurrentSong.Id);
            };
        }

        protected override void OnResume()
        {
            base.OnResume();

            checkCurrentSong();

            ChangePlayingView();
        }

        void ChangePlayingView()
        {
            if (mediaPlayer.IsPlaying)
            {
                TogglePlay();
            }
            else
            {
                TogglePause();
            }
        }

        void checkCurrentSong()
        {
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
        }

        void TogglePlay()
        {
            playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
            playBtn.Click -= Start;
            playBtn.Click += Pause;
        }

        void TogglePause()
        {
            playBtn.SetImageResource(Resource.Drawable.ic_media_play);
            playBtn.Click -= Pause;
            playBtn.Click += Start;
        }

        void Start(object sender, EventArgs e)
        {
            mediaPlayer.Start();
            TogglePlay();
        }

        void Pause(object sender, EventArgs e)
        {
            mediaPlayer.Pause();
            TogglePause();
        }

        void UpdatePage(string songId)
        {
            MediaMetadataRetriever mmr = SongMetadata.GetMetadata(songId);
            string title = mmr.ExtractMetadata(MetadataKey.Title) + " - " + mmr.ExtractMetadata(MetadataKey.Artist);

            if (title == null)
            {
                title = FileManager.GetSongNameById(songId);
            }

            songTitle.Text = title;

            seekbar.CreateSeekBar();

            Drawable picture = SongMetadata.GetSongPicture(songId);

            if (picture != null)
            {
                songImg.SetImageDrawable(picture);
            }
            else
            {
                songImg.SetImageResource(Resource.Drawable.icon);
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
