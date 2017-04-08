using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using tubeLoadNative.Droid.Utils;
using System.Threading;
using tubeLoadNative.Services;

namespace tubeLoadNative.Droid.Views
{
    public class SeekbarView : LinearLayout
    {
        SeekBar seekbar;
        TextView songPosition;
        TextView songLength;
        AndroidSongsManager mediaPlayer = AndroidSongsManager.Instance;
        Thread seekbarThread;

        public SeekbarView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize(context);
        }

        public SeekbarView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize(context);
        }

        void Initialize(Context context)
        {
            View seekbarView = Inflate(context, Resource.Layout.view_seekbar, this);
            seekbar = seekbarView.FindViewById<SeekBar>(Resource.Id.seekBar);
            songLength = seekbarView.FindViewById<TextView>(Resource.Id.songLength);
            songPosition = seekbarView.FindViewById<TextView>(Resource.Id.songPosition);

            seekbar.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    mediaPlayer.SeekTo(e.Progress);
                    songPosition.Text = Converter.MillisecondsToString(mediaPlayer.CurrentPosition);
                }
            };

            CreateSeekBar();
        }

        public void CreateSeekBar()
        {
            AbortSeekbarThread();

            seekbar.Max = mediaPlayer.Duration;
            songLength.Text = Converter.MillisecondsToString(mediaPlayer.Duration);
            songPosition.Text = Converter.MillisecondsToString(mediaPlayer.CurrentPosition);
            seekbar.Progress = mediaPlayer.CurrentPosition;

            seekbarThread = new Thread(new ThreadStart(UpdateSekkbarProgress));
            seekbarThread.Start();
        }

        void UpdateSekkbarProgress()
        {
            while (mediaPlayer.CurrentSong != null)
            {
                Thread.Sleep(500);
                seekbar.Progress = mediaPlayer.CurrentPosition;

                try
                {
                    songPosition.Text = Converter.MillisecondsToString(mediaPlayer.CurrentPosition);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message + '\n' + ex.StackTrace);
                }
            }
        }

        public void AbortSeekbarThread()
        {
            if (seekbarThread != null)
            {
                seekbarThread.Abort();
            }
        }
    }
}