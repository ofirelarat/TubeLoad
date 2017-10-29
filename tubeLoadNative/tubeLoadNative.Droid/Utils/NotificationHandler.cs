using System;

using Android.App;
using Android.Content;
using Android.Media;
using tubeLoadNative.Droid.Activities;
using Android.Views;

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

            CreateNotificationMediaActions();
          
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

        private static void CreateNotificationMediaActions()
        {
            Intent prevIntent = new Intent(Intent.ActionMediaButton);
            prevIntent.PutExtra(Intent.ExtraKeyEvent, new KeyEvent(KeyEventActions.Down, Keycode.MediaPrevious));
            PendingIntent prevPendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, prevIntent, PendingIntentFlags.UpdateCurrent);
            Notification.Action prevAction = new Notification.Action(Resource.Drawable.ic_media_previous, "", prevPendingIntent);
            builder.AddAction(prevAction);

            Intent playPauseIntent = new Intent(Intent.ActionMediaButton);
            playPauseIntent.PutExtra(Intent.ExtraKeyEvent, new KeyEvent(KeyEventActions.Down, Keycode.MediaPlayPause));
            PendingIntent playPausePendingIntent = PendingIntent.GetBroadcast(Application.Context, 1, playPauseIntent, PendingIntentFlags.UpdateCurrent);
            Notification.Action playPauseAction = new Notification.Action(Resource.Drawable.ic_media_pause, "", playPausePendingIntent);
            builder.AddAction(playPauseAction);

            Intent nextIntent = new Intent(Intent.ActionMediaButton);
            nextIntent.PutExtra(Intent.ExtraKeyEvent, new KeyEvent(KeyEventActions.Down, Keycode.MediaNext));
            PendingIntent nextPendingIntent = PendingIntent.GetBroadcast(Application.Context, 2, nextIntent, PendingIntentFlags.UpdateCurrent);
            Notification.Action nextAction = new Notification.Action(Resource.Drawable.ic_media_next, "", nextPendingIntent);
            builder.AddAction(nextAction);
        }
    }
}