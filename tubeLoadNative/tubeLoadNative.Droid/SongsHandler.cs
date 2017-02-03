using Android.App;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using tubeLoadNative.Abstracts;
using tubeLoadNative.Droid.Services;
using tubeLoadNative.Models;

namespace tubeLoadNative.Droid
{
    public class SongsHandler : SongsManager
    {        
        public override int Duration { get { return mediaPlayer.Duration; } }

        public override int CurrentPosition { get { return mediaPlayer.CurrentPosition; } }

        public override bool IsPlaying { get { return mediaPlayer.IsPlaying; } }

        static readonly SongsHandler instance = new SongsHandler();

        static SongsHandler()
        {
        }

        SongsHandler()
        {
            Songs = FileHandler.ReadFile();
            mediaPlayer = AndroidMediaPlayer.Instance;
            mediaPlayer.OnComplete += (sender, e) =>
            {
                OnComplete(sender, e);
            };
        }

        public static SongsHandler Instance
        {
            get
            {
                return instance;
            }
        }

        public new void DeleteSong(string id)
        {
            base.DeleteSong(id);
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(id);
            File.Delete(fileName);
            FileHandler.DeleteSong(id);
            Songs = FileHandler.ReadFile();
        }

        public new void RenameSong(string id, string newName)
        {
            base.RenameSong(id, newName);
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(id);

            if (!newName.EndsWith(".mp3", System.StringComparison.OrdinalIgnoreCase))
            {
                newName += ".mp3";
            }

            File.Move(fileName, FileHandler.PATH + newName);
            FileHandler.WriteToJsonFile(id, newName);
            Songs = FileHandler.ReadFile();
        }
        
        public override void Start()
        {
            if (mediaPlayer.HasDataSource)
            {
                PlayNext();
            }
            else
            {
                mediaPlayer.Continue();
            }
        }

        public override void Start(string songId)
        {
            string fileName = FileHandler.PATH + FileHandler.GetSongNameById(songId);

            if(mediaPlayer.Start(fileName))
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

                FileHandler.WriteToJsonFile(id, songName);
                Songs = FileHandler.ReadFile();
                OnSave(null, null);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}






//public static MediaMetadataRetriever GetMetadata(string id)
//{
//    MediaMetadataRetriever metadata = new MediaMetadataRetriever();
//    string fileName = FileHandler.PATH + FileHandler.GetSongNameById(id);
//    metadata.SetDataSource(fileName);
//    return metadata;
//}

//public static Drawable GetSongPicture(string id)
//{
//    MediaMetadataRetriever metadata = SongsHandler.GetMetadata(id);
//    byte[] pictureByteArray = metadata.GetEmbeddedPicture();

//    if (pictureByteArray != null)
//    {
//        return new BitmapDrawable(Application.Context.Resources, BitmapFactory.DecodeByteArray(pictureByteArray, 0, pictureByteArray.Length));
//    }
//    else
//    {
//        return null;
//    }
//}








//public static void CheckFilesExist()
//{
//    List<Song> songsInJsonFile = FileHandler.ReadFile();

//    foreach (Song song in songsInJsonFile)
//    {
//        CheckFileExist(song.Id);
//    }
//}

//public static void CheckFileExist(string songId)
//{
//    string name = FileHandler.GetSongNameById(songId);

//    if (!File.Exists(FileHandler.PATH + name))
//    {
//        FileHandler.DeleteSong(songId);
//        Songs = FileHandler.ReadFile();
//    }
//}