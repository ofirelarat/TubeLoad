using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using tubeLoadNative.Interfaces;
using tubeLoadNative.Models;

namespace tubeLoadNative.Abstracts
{
    public abstract class SongsManager
    {
        protected IMediaPlayer mediaPlayer;

        public event EventHandler Completing;
        public event EventHandler Starting;
        public event EventHandler Saving;

        protected int currentSongIndex;

        protected SongsManager()
        {
           currentSongIndex = -1;
        }

        #region Props

        public Song CurrentSong
        {
            get
            {
                return Songs[currentSongIndex];
            }
        }

        public abstract int Duration { get; }

        public abstract int CurrentPosition { get; }

        public abstract bool IsPlaying { get; }

        public List<Song> Songs { get; protected set; } 

        #endregion

        public abstract void Start();
        public abstract void Start(string songId);
        public abstract void Pause();

        public void Stop()
        {
            currentSongIndex = -1;
        }

        public abstract void SeekTo(int position);

        public void PlayNext()
        {
            currentSongIndex = (++currentSongIndex) % Songs.Count;

            Start(Songs[currentSongIndex].Name);
        }

        public void PlayPrev()
        {
            if (currentSongIndex == -1)
            {
                currentSongIndex = Songs.Count;
            }
            else
            {
                currentSongIndex = (--currentSongIndex + Songs.Count) % Songs.Count;
            }

            Start(Songs[currentSongIndex].Name);
        }

        public abstract Task<bool> SaveSong(string path, string songName, string id, Stream songStream);

        public void DeleteSong(string id)
        {
            int pos = Songs.IndexOf(Songs.Single((x) => x.Id == id));

            if (pos < currentSongIndex)
            {
                currentSongIndex--;
            }
            else if (pos == currentSongIndex)
            {
                currentSongIndex = -1;
            }
        }

        public void RenameSong(string id, string newName)
        {
            int pos = Songs.IndexOf(Songs.Single((x) => x.Id == id));
            if (pos < currentSongIndex)
            {
                currentSongIndex--;
            }
            else if (pos == currentSongIndex)
            {
                currentSongIndex = Songs.Count - 1;
            }
        }

        protected void OnComplete(object sender, EventArgs e)
        {
            Completing?.Invoke(sender, e);
        }

        protected void OnSave(object sender, EventArgs e)
        {
            Saving?.Invoke(sender, e);
        }

        protected void OnStart(object sender, EventArgs e)
        {
            Starting?.Invoke(sender, e);
        }
    }
}
