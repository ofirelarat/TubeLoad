using Android.Media;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace tubeLoadNative.Droid
{
    public class SongsHandler
    {
        private static Java.IO.File directory;

        private static readonly SongsHandler instance = new SongsHandler();

        static SongsHandler()
        {
            Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
            directory = new Java.IO.File(FileHandler.PATH);
            directory.Mkdirs();
            position = -1;
        }

        private SongsHandler()
        {
        }

        public static SongsHandler Instance
        {
            get
            {
                return instance;
            }
        }

        static List<Song> songs = FileHandler.ReadFile();

        public static int position { get; private set; }

        public static void Play(string id)
        {
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(id);
            position = songs.FindIndex((x) => x.Id == id);

            SongsMediaPlayer.Play(fileName);
        }

        public static void PlayNext()
        {
            position = (++position) % songs.Count;
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(songs[position].Id);

            SongsMediaPlayer.Play(fileName);
        }

        public static void PlayPrev()
        {
            position = (--position) % songs.Count;
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(songs[position].Id);

            SongsMediaPlayer.Play(fileName);
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

            if (!newName.EndsWith(".mp3"))
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
