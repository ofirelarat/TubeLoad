using System.Collections.Generic;
using System.Linq;

namespace tubeLoadNative.Models
{
    public class Song
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public static void AddSongsToList(List<Song> songsList, Dictionary<string, string> songsToAdd)
        {
            foreach (string id in songsToAdd.Keys)
            {
                Song song = new Song()
                {
                    Id = id,
                    Name = songsToAdd[id]
                };

                int index = songsList.FindIndex((x) => x.Id == song.Id);

                if (index != -1)
                {
                    songsList.RemoveAt(index);
                }

                songsList.Add(song);
            }
        }

        public static string GetSongNameById(List<Song> songs,string id)
        {
            Song song = songs.FirstOrDefault((x) => x.Id == id);

            if (song != null)
            {
                return song.Name;
            }

            return null;
        }

        public static string GetSongIdByName(List<Song> songs, string name)
        {
            string songId = null;
            Song song = songs.FirstOrDefault((x) => x.Name == name);

            if (song != null)
            {
                songId = song.Id;
            }

            return songId;
        }

        public static bool DeleteSongFromList(List<Song> songsList, string id)
        {
            bool didDelete = false;
            int index = songsList.FindIndex((x) => x.Id == id);

            if (index != -1)
            {
                didDelete = true;
                songsList.RemoveAt(index);
            }

            return didDelete;
        }

        public static int AlphabeticCompare(Song song1, Song song2)
        {
            return song1.Name.CompareTo(song2.Name);
        }
    }
}
