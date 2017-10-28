using Android.App;
using Android.Media;
using Android.Graphics.Drawables;
using Android.Graphics;

namespace tubeLoadNative.Droid.Utils
{
    public class SongMetadata
    {
        public static MediaMetadataRetriever GetMetadata(string id)
        {
            MediaMetadataRetriever metadata = new MediaMetadataRetriever();
            string fileName = FileManager.PATH + AndroidSongsManager.Instance.GetSong(id).Name;
            metadata.SetDataSource(fileName);

            return metadata;
        }

        public static Drawable GetSongPicture(string id)
        {
            MediaMetadataRetriever metadata = GetMetadata(id);
            byte[] pictureByteArray = metadata.GetEmbeddedPicture();

            if (pictureByteArray != null)
            {
                return new BitmapDrawable(Application.Context.Resources, BitmapFactory.DecodeByteArray(pictureByteArray, 0, pictureByteArray.Length));
            }

            return null;
        }

        public static void setMetadata(string filePath,string imageFile)
        {
            TagLib.File file = TagLib.File.Create(filePath);
            file.Tag.Pictures = new TagLib.Picture[]{ TagLib.Picture.CreateFromPath(imageFile)};
            //TagLib.Image.File.Create(filePath, TagLib.ReadStyle.Average);
            file.Save();
        }
    }
}