using System;
using Android.Media;

namespace tubeLoadNative.Droid.Utils
{
    public class AndroidMediaPlayer : Interfaces.IMediaPlayer
    {
        #region Data Members

        MediaPlayer mediaPlayer; 

        #endregion

        #region Singleton

        static AndroidMediaPlayer instance = new AndroidMediaPlayer();

        static AndroidMediaPlayer()
        {
        }

        AndroidMediaPlayer()
        {
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Completion += (sender, e) =>
            {
                if (!mediaPlayer.IsPlaying)
                {
                    OnComplete?.Invoke(sender, e);
                }
            };
        }

        public static AndroidMediaPlayer Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region Props

        public int CurrentPosition
        {
            get
            {
                return mediaPlayer.CurrentPosition;
            }
        }

        public int Duration
        {
            get
            {
                return mediaPlayer.Duration;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return mediaPlayer.IsPlaying;
            }
        }

        public bool HasDataSource { get; set; }

        #endregion

        #region Events

        public event EventHandler OnComplete;

        #endregion

        #region Functions

        public void Continue()
        {
            if (HasDataSource)
            {
                mediaPlayer.Start(); 
            }
        }

        public void Pause()
        {
            mediaPlayer.Pause();
        }

        public void SeekTo(int position)
        {
            if (HasDataSource)
            {
                mediaPlayer.SeekTo(position);
            }
        }

        public bool Start(string path)
        {
            mediaPlayer.Reset();

            try
            {
                mediaPlayer.SetDataSource(path);
                mediaPlayer.Prepare();
                mediaPlayer.Start();
                HasDataSource = true;

                return true;
            }
            catch (Java.Lang.Exception)
            {
                return false;
            }
        }

        public void Stop()
        {
            mediaPlayer.Stop();
            HasDataSource = false;
        }

        #endregion
    }
}