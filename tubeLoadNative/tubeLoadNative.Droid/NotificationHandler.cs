using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media;

namespace tubeLoadNative.Droid
{
    class NotificationHandler
    {
        private static Notification songNotification;
        private static Notification.Builder builder;
        private static NotificationManager notificationManager;
        private const int SONG_NOTIFICATION_ID = 0;

        static NotificationHandler()
        {
            Intent intent = new Intent(Application.Context, typeof(CurrentSongActivity));
            intent.PutExtra("currentSongId", SongsHandler.CurrentSong.Id);
            PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent);

            Intent stopIntent = new Intent(Application.Context, typeof(NotificationActionService));
            stopIntent.SetAction("stop");
            PendingIntent btnPendingIntent = PendingIntent.GetService(Application.Context, 0, stopIntent, PendingIntentFlags.UpdateCurrent);
            Notification.Action stop = new Notification.Action(Resource.Drawable.ic_media_stop, "stop", btnPendingIntent);

            builder = new Notification.Builder(Application.Context);
            builder.SetContentIntent(pendingIntent);
            //builder.AddAction(stop);

            notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
        }

        public static void BuildNotification(String songId)
        {
            MediaMetadataRetriever metadata = SongsHandler.GetMetadata(songId);

            string title = metadata.ExtractMetadata(MetadataKey.Title);
            string content = metadata.ExtractMetadata(MetadataKey.Artist);
            if (title == null || content == null)
            {
                title = "TubeLoad";
                content = FileHandler.GetSongNameById(songId);
            }

            builder.SetSmallIcon(Resource.Drawable.icon);
            builder.SetContentTitle(title);
            builder.SetContentText(content);

            songNotification = builder.Build();

            notificationManager.Notify(SONG_NOTIFICATION_ID, songNotification);
        }

        public class NotificationActionService : IntentService
        {
            public NotificationActionService()
            {
            }

            protected override void OnHandleIntent(Intent intent)
            {
                string action = intent.Action;
                switch (action)
                {
                    case "stop":
                        SongsHandler.Stop();
                        notificationManager.CancelAll();
                        break;
                }
            }
        }

    }
}