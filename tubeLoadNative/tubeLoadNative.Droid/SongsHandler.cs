using Android.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace tubeLoadNative.Droid
{
    public class SongsHandler
    {
        static Java.IO.File directory;

        public static EventHandler OnComplete;

        static MediaPlayer mediaPlayer = new MediaPlayer();

        static List<Song> songs = FileHandler.ReadFile();

        public static int CurrentSongIndex { get; private set; }

        public static int Duration { get { return mediaPlayer.Duration; } }

        public static int CurrentPosition { get { return mediaPlayer.CurrentPosition; } }

        public static bool IsPlaying { get { return mediaPlayer.IsPlaying; } }

        static readonly SongsHandler instance = new SongsHandler();

        static SongsHandler()
        {
            Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
            directory = new Java.IO.File(FileHandler.PATH);
            directory.Mkdirs();
            CurrentSongIndex = -1;
            mediaPlayer.Completion += (sender, e) =>
            {
                OnComplete(sender, e);
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

        public static void Start(string fileName)
        {
            mediaPlayer.Reset();
            mediaPlayer.SetDataSource(fileName);

            try
            {
                mediaPlayer.Prepare();
                mediaPlayer.Start();
            }
            catch (Java.Lang.Exception)
            {
                throw;
            }
        }

        public static void Play(string id)
        {
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(id);
            CurrentSongIndex = songs.FindIndex((x) => x.Id == id);

            Start(fileName);
        }

        public static void PlayNext()
        {
            CurrentSongIndex = (++CurrentSongIndex) % songs.Count;
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(songs[CurrentSongIndex].Id);

            Start(fileName);
        }

        public static void PlayPrev()
        {
            CurrentSongIndex = (--CurrentSongIndex) % songs.Count;
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(songs[CurrentSongIndex].Id);

            Start(fileName);
        }

        public static void Pause()
        {
            mediaPlayer.Pause();
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

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void DeleteSong(string id)
        {
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(id);
            File.Delete(fileName);
            FileHandler.DeleteSong(id);
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
        }

        public static void CheckFilesExist()
        {
            List<Song> songsInJsonFile = FileHandler.ReadFile();

            foreach (Song song in songsInJsonFile)
            {
                if (!File.Exists(FileHandler.PATH + song.Name))
                {
                    FileHandler.DeleteSong(song.Id);
                }    
            }
        }

        public static void CheckFileExist(string songId)
        {
            string name = FileHandler.GetSongNameById(songId);

            if (!File.Exists(FileHandler.PATH + name))
            {
                FileHandler.DeleteSong(songId);
            }
        }
    }
}
