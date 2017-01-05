using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace tubeLoadNative.Droid
{
    public class SongsHandler
    {
        static Java.IO.File directory;

        public static event EventHandler OnComplete;

        public static event EventHandler OnSongSaved;

        public static event EventHandler OnSongPlayedSetBackground;

        static MediaPlayer mediaPlayer = new MediaPlayer();

        public static List<Song> Songs { get; private set; } 

        public static int CurrentSongIndex { get; private set; }

        public static Song CurrentSong { get; private set; }

        public static int Duration { get { return mediaPlayer.Duration; } }

        public static int CurrentPosition { get { return mediaPlayer.CurrentPosition; } }

        public static bool IsPlaying { get { return mediaPlayer.IsPlaying; } }

        static readonly SongsHandler instance = new SongsHandler();

        static SongsHandler()
        {
            Songs = FileHandler.ReadFile();
            Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
            directory = new Java.IO.File(FileHandler.PATH);
            directory.Mkdirs();
            CurrentSongIndex = -1;
            mediaPlayer.Completion += (sender, e) =>
            {
                OnComplete?.Invoke(sender, e);
            };
        }

        SongsHandler()
        {
        }

        public static SongsHandler Instance
        {
            get
            {
                return instance;
            }
        }

        // Start the current song
        public static void Start()
        {
            if (CurrentSongIndex == -1)
            {
                PlayNext();
            }
            else
            {
                mediaPlayer.Start();
            }
        }

        // Start a song from the begining
        public static void Start(string songName)
        {
            mediaPlayer.Reset();
            string fileName = FileHandler.PATH + songName;
            mediaPlayer.SetDataSource(fileName);

            try
            {
                mediaPlayer.Prepare();
                mediaPlayer.Start();
                    
                string songId = FileHandler.FindSong(songName);
                CurrentSong = new Song() { Id = songId, Name = songName };

                OnSongPlayedSetBackground?.Invoke(null, null);

                NotificationHandler.BuildNotification(songId);
            }
            catch (Java.Lang.Exception)
            {
                Toast.MakeText(Application.Context, "can't open this file", ToastLength.Long).Show();
            }
        }

        public static void Play(string id)
        {
            string fileName = FileHandler.GetSongNameById(id);
            CurrentSongIndex = Songs.FindIndex((x) => x.Id == id);

            Start(fileName);
        }

        public static void PlayNext()
        {
            CurrentSongIndex = (++CurrentSongIndex) % Songs.Count;
            string fileName = FileHandler.GetSongNameById(Songs[CurrentSongIndex].Id);

            Start(fileName);
        }

        public static void PlayPrev()
        {
            // If no song has played yet
            CurrentSongIndex = CurrentSongIndex == -1 ? 0 : CurrentSongIndex;
            CurrentSongIndex = (--CurrentSongIndex + Songs.Count) % Songs.Count;
            string fileName = FileHandler.GetSongNameById(Songs[CurrentSongIndex].Id);

            Start(fileName);
        }

        public static void Pause()
        {
            mediaPlayer.Pause();
        }

        public static void Stop()
        {
            mediaPlayer.Stop();
        }

        public static void SeekTo(int position)
        {
            mediaPlayer.SeekTo(position);
        }

        public static MediaMetadataRetriever GetMetadata(string id)
        {
            MediaMetadataRetriever metadata = new MediaMetadataRetriever();
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(id);
            metadata.SetDataSource(fileName);
            return metadata;
        }

        public static Drawable GetSongPicture(string id)
        {
            MediaMetadataRetriever metadata = SongsHandler.GetMetadata(id);
            byte[] pictureByteArray = metadata.GetEmbeddedPicture();

            if (pictureByteArray != null)
            {
                return new BitmapDrawable(Application.Context.Resources, BitmapFactory.DecodeByteArray(pictureByteArray, 0, pictureByteArray.Length));
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> SaveSong(string path, string songName, string id, System.IO.Stream songStream)
        {
            string fileName = path + songName;

            try
            {
                using (System.IO.Stream output = File.OpenWrite(fileName))
                using (System.IO.Stream input = songStream)
                {
                    await input.CopyToAsync(output);
                }

                FileHandler.WriteToJsonFile(id, songName);
                Songs = FileHandler.ReadFile();
                OnSongSaved?.Invoke(null, null);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static void DeleteSong(string id)
        {
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(id);
            File.Delete(fileName);
            FileHandler.DeleteSong(id);
            Songs = FileHandler.ReadFile();
            CurrentSong = null;
        }

        public static void RenameSong(string id, string newName)
        {
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(id);

            if (!newName.EndsWith(".mp3", System.StringComparison.OrdinalIgnoreCase))
            {
                newName += ".mp3";
            }

            File.Move(fileName, FileHandler.PATH + newName);
            FileHandler.WriteToJsonFile(id, newName);
            Songs = FileHandler.ReadFile();
            CurrentSongIndex = Songs.IndexOf(CurrentSong);
        }

        public static void CheckFilesExist()
        {
            List<Song> songsInJsonFile = FileHandler.ReadFile();

            foreach (Song song in songsInJsonFile)
            {
                CheckFileExist(song.Id);
            }
        }

        public static void CheckFileExist(string songId)
        {
            string name = FileHandler.GetSongNameById(songId);

            if (!File.Exists(FileHandler.PATH + name))
            {
                FileHandler.DeleteSong(songId);
                Songs = FileHandler.ReadFile();
            }
        }
    }
}
