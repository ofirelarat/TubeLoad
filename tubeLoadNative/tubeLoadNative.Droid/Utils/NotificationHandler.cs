using System;

using Android.App;
using Android.Content;
using Android.Media;
using tubeLoadNative.Droid.Activities;

namespace tubeLoadNative.Droid.Utils
{
    public class NotificationHandler
    {
        static Notification songNotification;
        static Notification.Builder builder;
        static NotificationManager notificationManager;
        const int SONG_NOTIFICATION_ID = 0;

        static NotificationHandler()
        {
            Intent intent = new Intent(Application.Context, typeof(CurrentSong));
            PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent);
            
            builder = new Notification.Builder(Application.Context);
            builder.SetContentIntent(pendingIntent);

            notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
        }

        public static void BuildNotification(String songId)
        {
            MediaMetadataRetriever metadata = SongMetadata.GetMetadata(songId);

            string title = metadata.ExtractMetadata(MetadataKey.Title);
            string content = metadata.ExtractMetadata(MetadataKey.Artist);

            if (title == null || content == null)
            {
                title = "TubeLoad";
                content = AndroidSongsManager.Instance.GetSong(songId).Name;
            }

            builder.SetSmallIcon(Resource.Drawable.icon);
            builder.SetContentTitle(title);
            builder.SetContentText(content);

            songNotification = builder.Build();

            notificationManager.Notify(SONG_NOTIFICATION_ID, songNotification);
        }

        public static void DeleteNotification()
        {
            notificationManager.CancelAll();
        }
    }
}