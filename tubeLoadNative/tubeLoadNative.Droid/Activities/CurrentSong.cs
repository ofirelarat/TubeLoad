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
using Android.Gms.Ads;
using tubeLoadNative.Droid.Services;

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Label = "TubeLoad")]
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

            GoogleAnalyticsService.Instance.Initialize(this);
            GoogleAnalyticsService.Instance.TrackAppPage("Current Song");

            if (AdsService.CurrentSongAd == null)
            {
                AdsService.CurrentSongAd = FindViewById<AdView>(Resource.Id.adView);
                AdsService.CurrentSongAd.AdListener = new AdsService.AdListenerService(() => {  }, () => AdsService.LoadBanner(AdsService.CurrentSongAd));
                AdsService.LoadBanner(AdsService.CurrentSongAd);
            }

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
                mediaPlayer.PlayNext();
            };

            prevBtn.Click += delegate
            {
                mediaPlayer.PlayPrev();
            };

            ChangePlayingView();

            mediaPlayer.Completing += delegate
            {
                UpdatePage(mediaPlayer.CurrentSong.Id);
            };

            mediaPlayer.Starting += delegate
            {
                TogglePlay();
            };

            mediaPlayer.StartingNewSong += delegate
            {
                UpdatePage(mediaPlayer.CurrentSong.Id);
            };

            mediaPlayer.Pausing += delegate
            {
                TogglePause();
            };
        }

        protected override void OnResume()
        {
            base.OnResume();

            //if (AdsService.CurrentSongAd != null && !AdsService.CurrentSongAd.IsShown)
            //{
            //    AdsService.CurrentSongAd.Resume();
            //}

            checkCurrentSong();

            ChangePlayingView();
        }

        protected override void OnPause()
        {
            //if (AdsService.CurrentSongAd != null)
            //    AdsService.CurrentSongAd.Pause();
            base.OnPause();
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
            GoogleAnalyticsService.Instance.TrackAppEvent(GoogleAnalyticsService.GAEventCategory.PlayingSong, "Playing " + mediaPlayer.CurrentSong.Name);
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
        }

        void Pause(object sender, EventArgs e)
        {
            mediaPlayer.Pause();
        }

        void UpdatePage(string songId)
        {
            MediaMetadataRetriever mmr = SongMetadata.GetMetadata(songId);
            string title = mmr.ExtractMetadata(MetadataKey.Title);
            string artist = mmr.ExtractMetadata(MetadataKey.Artist);

            if (title == null || artist == null)
            {
                songTitle.Text = AndroidSongsManager.Instance.GetSong(songId).Name.Replace(".mp3",string.Empty);
            }
            else
            {
                songTitle.Text = title + " - " + artist;
            }
            seekbar.CreateSeekBar();

            Drawable picture = SongMetadata.GetSongPicture(songId);

            if (picture != null)
            {
                songImg.SetImageDrawable(picture);
            }
            else
            {
                songImg.SetImageResource(Resource.Drawable.default_song_image);
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
                    Finish();
                    return true;

                case Resource.Id.mySong:
                    intent = new Intent(this, typeof(SongsPlayer));
                    StartActivity(intent);
                    Finish();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}
