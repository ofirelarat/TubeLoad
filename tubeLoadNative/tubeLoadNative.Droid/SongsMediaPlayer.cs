using Android.Media;
using Java.Lang;

namespace tubeLoadNative.Droid
{
    internal class SongsMediaPlayer
    {
        public static MediaPlayer mediaPlayer = new MediaPlayer();

        public static bool IsPlaying()
        {
            return mediaPlayer.IsPlaying;
        }

        public static void SeekTo(int progress)
        {
            mediaPlayer.SeekTo(progress);
        }

        public static void Play(string fileName)
        {
            mediaPlayer.Reset();
            mediaPlayer.SetDataSource(fileName);

            try
            {
                mediaPlayer.Prepare();
                mediaPlayer.Start();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void Start()
        {
            mediaPlayer.Start();
        }

        public static void Stop()
        {
            mediaPlayer.Stop();
        }
    }
}