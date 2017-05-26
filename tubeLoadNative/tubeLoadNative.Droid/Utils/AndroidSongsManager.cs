using Android.App;
using Android.Widget;
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

        public override async Task<bool> SaveSong(string path, string songName, string id, Stream songStream)
        {
            string fileName = path + songName;

            try
            {
                using (Stream output = File.OpenWrite(fileName))
                using (Stream input = songStream)
                {
                    await input.CopyToAsync(output);
                }

                FileManager.WriteToJsonFile(id, songName);
                Songs = FileManager.ReadFile();
                OnSave(null, null);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public new void DeleteSong(string id)
        {
            base.DeleteSong(id);
            string fileName = FileManager.PATH + GetSong(id).Name;
            File.Delete(fileName);
            FileManager.DeleteSong(id);
            Songs = FileManager.ReadFile();
        }

        public void RenameSong(string id, string newName)
        {
            RenameSong(id, ref newName);
            string fileName = FileManager.PATH + GetSong(id).Name;

            // Renaming the song file
            File.Move(fileName, FileManager.PATH + newName);

            FileManager.WriteToJsonFile(id, newName);
            Songs = FileManager.ReadFile();
        }

        public void UpdateSongsList()
        {
            Songs = FileManager.ReadFile();
            Songs.Reverse();
        }

        #endregion
    }
}