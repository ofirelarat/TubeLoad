using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using tubeLoadNative.Models;

namespace tubeLoadNative.Droid.Utils
{
    public static class FileManager
    {
        static FileManager()
        {
            Java.IO.File directory = new Java.IO.File(PATH);
            directory.Mkdirs();
        }

        public static Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
        public static readonly string PATH = sdCard.AbsolutePath + "/TubeLoad/";

        public const string ID_FILE = "ids.json";

        static void UpdateFile(List<Song> songsList)
        {
            var jsonData = JsonConvert.SerializeObject(songsList, Formatting.Indented);
            File.WriteAllText(PATH + ID_FILE, jsonData, Encoding.UTF8);
        }

        public static string GetSongNameById(string id)
        {
            List<Song> songs = ReadFile();
            return Song.GetSongNameById(songs, id);
        }

        public static List<Song> ReadFile()
        {
            string jsonData = null;
            List<Song> songsList = new List<Song>();

            // Read existing json data
            if (File.Exists(PATH + ID_FILE))
                jsonData = File.ReadAllText(PATH + ID_FILE, Encoding.UTF8);

            // De-serialize to object or leave the new list
            if (jsonData != null)
                songsList = JsonConvert.DeserializeObject<List<Song>>(jsonData);

            return songsList;
        }

        public static void WriteToJsonFile(Dictionary<string, string> songsToAdd)
        {
            var currentList = ReadFile();
            Song.AddSongsToList(currentList, songsToAdd);
            UpdateFile(currentList);
        }

        public static void WriteToJsonFile(string id, string name)
        {
            Dictionary<string, string> songs = new Dictionary<string, string>();
            songs[id] = name;
            WriteToJsonFile(songs);
        }

        public static string FindSong(string name)
        {
            List<Song> songs = ReadFile();
            return Song.GetSongIdByName(songs, name);
        }

        public static bool DeleteSong(string id)
        {
            var songsList = ReadFile();

            if (Song.DeleteSongFromList(songsList, id))
            {
                UpdateFile(songsList);
                return true;
            }

            return false;
        }

        public static void SongsListUpdate()
        {
            List<Song> songsInJsonFile = ReadFile();

            foreach (Song song in songsInJsonFile)
            {
                SongsListUpdate(song.Id);
            }
        }

        public static void SongsListUpdate(string songId)
        {
            string name = GetSongNameById(songId);

            if (!File.Exists(PATH + name))
            {
                DeleteSong(songId);
            }
        }

        public static bool ExistCaseSensetive(string name)
        {
            DirectoryInfo directory = new DirectoryInfo(PATH);
            foreach (FileInfo fileInfo in directory.GetFiles("*.mp3"))
            {
                if (fileInfo.Name.Equals(name))
                {
                    return true;
                }
            }

            return false;
        }
    }
}