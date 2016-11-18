using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace tubeLoadNative.Droid
{
    public static class FilesHandler
    {
        private static Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
        private static readonly string PATH = sdCard.Path + "/TubeLoad/";

        public const string ID_FILE = "ids.json";

        private static void UpdateFile(List<Song> songsList)
        {
            var jsonData = JsonConvert.SerializeObject(songsList, Formatting.Indented);
            File.WriteAllText(PATH + ID_FILE, jsonData, Encoding.UTF8);
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

        public static void WriteToJsonFile(Dictionary<string, string> values)
        {
            var songsList = ReadFile();

            // Add the new songs
            foreach (string id in values.Keys)
            {
                Song song = new Song()
                {
                    Id = id,
                    Name = values[id]
                };

                int index = songsList.FindIndex((x) => x.Id == song.Id);

                if (index != -1)
                {
                    songsList.RemoveAt(index);
                }

                songsList.Add(song);
            }

            // Update json data string
            UpdateFile(songsList);
        }

        public static void WriteToJsonFile(string id, string name)
        {
            Dictionary<string, string> songs = new Dictionary<string, string>();
            songs[id] = name;
            WriteToJsonFile(songs);
        }

        public static string FindSong(string name)
        {
            string songId = null;
            List<Song> songs = ReadFile();
            Song song = songs.FirstOrDefault((x) => x.Name == name);

            if (song != null)
            {
                songId = song.Id;
            }

            return songId;
        }

        public static bool DeleteSong(string id)
        {
            bool didDelete = false;
            var songsList = ReadFile();

            int index = songsList.FindIndex((x) => x.Id == id);

            if (index != -1)
            {
                didDelete = true;
                songsList.RemoveAt(index);
            }

            UpdateFile(songsList);

            return didDelete;
        }
    }
}