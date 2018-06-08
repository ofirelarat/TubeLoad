using System;

using Android.App;
using Android.Content;
using Android.Media;
using tubeLoadNative.Droid.Activities;
using Android.Views;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;

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
            intent.AddFlags(ActivityFlags.SingleTop);
            PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent);
            
            builder = new Notification.Builder(Application.Context);
            builder.SetContentIntent(pendingIntent);

            builder.SetOngoing(true);

            if (Android.OS.Build.VERSION.SdkInt <= Android.OS.Build.VERSION_CODES.M)
            {
                CreateNotificationMediaActions();
            }

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
                content = AndroidSongsManager.Instance.GetSong(songId).Name.Replace(".mp3","");
            }

            builder.SetSmallIcon(Resource.Drawable.icon);
            Drawable drawable = SongMetadata.GetSongPicture(songId);
            Bitmap bitmap;
            if (drawable != null)
            {
                bitmap = ((BitmapDrawable)SongMetadata.GetSongPicture(songId)).Bitmap;
            }
            else
            {
                bitmap = BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.default_song_image);
            }

            if (Android.OS.Build.VERSION.SdkInt > Android.OS.Build.VERSION_CODES.M)
            {
                RemoteViews notificationLayoutExpanded = new RemoteViews(Application.Context.PackageName, Resource.Layout.view_notification_actions);
                notificationLayoutExpanded.SetTextViewText(Resource.Id.notificationTitle, title);
                notificationLayoutExpanded.SetTextViewText(Resource.Id.notificationContent, content);
                notificationLayoutExpanded.SetImageViewBitmap(Resource.Id.songImg, bitmap);
                CreateNotificationMediaActions(notificationLayoutExpanded);
                builder.SetCustomBigContentView(notificationLayoutExpanded);
            }
            else
            {
                builder.SetLargeIcon(bitmap);
                builder.SetContentTitle(title);
                builder.SetContentText(content);
            }
        
            songNotification = builder.Build();

            notificationManager.Notify(SONG_NOTIFICATION_ID, songNotification);
        }

        public static void DeleteNotification()
        {
            notificationManager.CancelAll();
        }

        private static void CreateNotificationMediaActions(RemoteViews notificationLayoutExpanded)
        {
            Intent prevIntent = new Intent(Intent.ActionMediaButton);
            prevIntent.PutExtra(Intent.ExtraKeyEvent, new KeyEvent(KeyEventActions.Down, Keycode.MediaPrevious));
            PendingIntent prevPendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, prevIntent, PendingIntentFlags.UpdateCurrent);
            notificationLayoutExpanded.SetOnClickPendingIntent(Resource.Id.prevBtn, prevPendingIntent);
          
            Intent playPauseIntent = new Intent(Intent.ActionMediaButton);
            playPauseIntent.PutExtra(Intent.ExtraKeyEvent, new KeyEvent(KeyEventActions.Down, Keycode.MediaPlayPause));
            PendingIntent playPausePendingIntent = PendingIntent.GetBroadcast(Application.Context, 1, playPauseIntent, PendingIntentFlags.UpdateCurrent);
            notificationLayoutExpanded.SetOnClickPendingIntent(Resource.Id.playBtn, playPausePendingIntent);

            Intent nextIntent = new Intent(Intent.ActionMediaButton);
            nextIntent.PutExtra(Intent.ExtraKeyEvent, new KeyEvent(KeyEventActions.Down, Keycode.MediaNext));
            PendingIntent nextPendingIntent = PendingIntent.GetBroadcast(Application.Context, 2, nextIntent, PendingIntentFlags.UpdateCurrent);
            notificationLayoutExpanded.SetOnClickPendingIntent(Resource.Id.nextBtn, nextPendingIntent);

            Intent stopIntent = new Intent(Intent.ActionMediaButton);
            stopIntent.PutExtra(Intent.ExtraKeyEvent, new KeyEvent(KeyEventActions.Down, Keycode.MediaStop));
            PendingIntent stopPendingIntent = PendingIntent.GetBroadcast(Application.Context, 3, stopIntent, PendingIntentFlags.UpdateCurrent);
            notificationLayoutExpanded.SetOnClickPendingIntent(Resource.Id.closeBtn, stopPendingIntent);
        }

        private static void CreateNotificationMediaActions()
        {
            Intent stopIntent = new Intent(Intent.ActionMediaButton);
            stopIntent.PutExtra(Intent.ExtraKeyEvent, new KeyEvent(KeyEventActions.Down, Keycode.MediaStop));
            PendingIntent stopPendingIntent = PendingIntent.GetBroadcast(Application.Context, 3, stopIntent, PendingIntentFlags.UpdateCurrent);
            Notification.Action stopAction = new Notification.Action(Resource.Drawable.ic_cancel_blue, string.Empty, stopPendingIntent);
            builder.AddAction(stopAction);
        }
    }
}