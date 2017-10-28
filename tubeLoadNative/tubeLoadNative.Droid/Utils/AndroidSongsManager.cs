using Android.App;
using Android.Widget;
using System;
using System.IO;
using System.Threading.Tasks;
using tubeLoadNative.Abstracts;

namespace tubeLoadNative.Droid.Utils
{
    public class AndroidSongsManager : SongsManager
    {
        #region Singleton

        static readonly AndroidSongsManager instance = new AndroidSongsManager();

        static AndroidSongsManager()
        {
        }

        AndroidSongsManager()
        {
            UpdateSongsList();
            mediaPlayer = AndroidMediaPlayer.Instance;
            mediaPlayer.OnComplete += (sender, e) =>
            {
                PlayNext();
                OnComplete(sender, e);
            };
        }

        public static AndroidSongsManager Instance
        {
            get
            {
                return instance;
            }
        } 

        #endregion

        #region Props

        public override int Duration { get { return mediaPlayer.Duration; } }

        public override int CurrentPosition { get { return mediaPlayer.CurrentPosition; } }

        public override bool IsPlaying { get { return mediaPlayer.IsPlaying; } }

        #endregion

        #region Functions

        public override bool Start()
        {
            if (Songs.Count > 0)
            {
                if (!mediaPlayer.HasDataSource)
                {
                    PlayNext();
                }
                else
                {
                    mediaPlayer.Continue();
                }

                OnStart(null, null);

                return true;
            }

            return false;
        }

        public override void Start(string songId)
        {
            string fileName = FileManager.PATH + GetSong(songId).Name;

            if (mediaPlayer.Start(fileName))
            {
                currentSongIndex = Songs.FindIndex((x) => x.Id == songId);
                OnStart(null, null);
                OnStartingNewSong(null, null);
                NotificationHandler.BuildNotification(songId);
            }
            else
            {
                Toast.MakeText(Application.Context, "can't open this file", ToastLength.Long).Show();
            }
        }

        public override void Pause()
        {
            mediaPlayer.Pause();
            OnPause(null, null);
        }

        public new void Stop()
        {
            base.Stop();
            mediaPlayer.Stop();
        }

        public override void SeekTo(int position)
        {
            mediaPlayer.SeekTo(position);
        }

        public override async Task<bool> SaveSong(string path, string songName, string id, Stream songStream,Stream picStream)
        {
            string fileName = path + songName;
            string picFile = path + "thumnail.jpg";

            try
            {
                await saveStream(songStream, fileName);
                await saveStream(picStream, picFile);

                FileManager.WriteToJsonFile(id, songName);
                SongMetadata.setMetadata(fileName, picFile);

                File.Delete(picFile);

                UpdateSongsList();
                OnSave(null, null);
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }

        private async Task saveStream(Stream stream, string fileName)
        {
            using (Stream output = File.OpenWrite(fileName))
            using (Stream input = stream)
            {
                await input.CopyToAsync(output);
            }
        }

        public void DeleteSong(string id)
        {
            string fileName = FileManager.PATH + GetSong(id).Name;
            File.Delete(fileName);
            FileManager.DeleteSong(id);
            UpdateSongsList();
        }

        public void RenameSong(string id, string newName)
        {
            newName = GetValidFileName(newName);
            string fileName = FileManager.PATH + GetSong(id).Name;

            // Renaming the song file
            File.Move(fileName, FileManager.PATH + newName);

            FileManager.WriteToJsonFile(id, newName);
            UpdateSongsList();
        }

        public void UpdateSongsList()
        {
            Models.Song currentSong = CurrentSong;
            Songs = FileManager.ReadFile();
            Songs.Reverse();

            if (currentSong != null)
            {
                Models.Song songCur = Songs.Find(song => song.Id.Equals(currentSong.Id));
                currentSongIndex = Songs.IndexOf(songCur);
            }
        }

        #endregion
    }
}