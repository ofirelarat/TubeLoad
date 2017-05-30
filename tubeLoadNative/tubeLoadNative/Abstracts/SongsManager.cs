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
        #region Data Members

        protected int currentSongIndex;
        protected IMediaPlayer mediaPlayer;
        private string[] forbiddenChars = { "|", "\\", "?", "*", "<", "\"", ":", ">", "/" };

        #endregion

        #region Events

        public event EventHandler Completing;
        public event EventHandler Starting;
        public event EventHandler Saving;

        #endregion

        #region Ctor

        protected SongsManager()
        {
            currentSongIndex = -1;
        }

        #endregion

        #region Props

        public Song CurrentSong
        {
            get
            {
                if (currentSongIndex != -1)
                {
                    return Songs[currentSongIndex];
                }

                return null;
            }
        }

        public abstract int Duration { get; }

        public abstract int CurrentPosition { get; }

        public abstract bool IsPlaying { get; }

        public virtual List<Song> Songs { get; protected set; }

        #endregion

        #region Functions

        #region Protected Functions

        protected string GetValidFileName(string name)
        {
            string[] forbiddenChars = { "|", "\\", "?", "*", "<", "\"", ":", ">", "/" };

            foreach (string c in forbiddenChars)
            {
                name = name.Replace(c, string.Empty);
            }

            if (!name.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                name += ".mp3";
            }

            return name;
        }

        #endregion

        #region Abstract Functions

        public abstract bool Start();
        public abstract void Start(string songId);
        public abstract void Pause();
        public abstract void SeekTo(int position);
        public abstract Task<bool> SaveSong(string path, string songName, string id, Stream songStream);

        #endregion

        #region Public Functions

        public Song GetSong(string id)
        {
            return Songs.FirstOrDefault(song => song.Id == id);
        }

        public void Stop()
        {
            currentSongIndex = -1;
        }

        public bool PlayNext()
        {
            if (Songs.Count > 0)
            {
                currentSongIndex = (++currentSongIndex) % Songs.Count;
                Start(Songs[currentSongIndex].Id);

                return true;
            }

            return false;
        }

        public bool PlayPrev()
        {
            if (Songs.Count > 0)
            {
                if (currentSongIndex == -1)
                {
                    currentSongIndex = Songs.Count - 1;
                }
                else
                {
                    currentSongIndex = (--currentSongIndex + Songs.Count) % Songs.Count;
                }

                Start(Songs[currentSongIndex].Id);

                return true;
            }

            return false;
        }

        public string CorrectSongNameForSave(string fileName)
        {
            // Erasing illegal charachters from file name
            foreach (string charachter in forbiddenChars)
            {
                fileName = fileName.Replace(charachter, string.Empty);
            }

            return fileName;
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

        public void SortSongs(Comparison<Song> comparer)
        {
            Song currentSong = CurrentSong;
            Songs.Sort(comparer);
            currentSongIndex = Songs.IndexOf(currentSong);
        }

        #endregion 

        #endregion
    }
}
