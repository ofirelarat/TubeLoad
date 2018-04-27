using Android.App;
using Android.Content;
using Android.OS;
using tubeLoadNative.Droid.Utils;

namespace tubeLoadNative.Droid.Services
{
    [Service]
    public class ClosingService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            NotificationHandler.DeleteNotification();
            base.OnTaskRemoved(rootIntent);
        }
    }
}