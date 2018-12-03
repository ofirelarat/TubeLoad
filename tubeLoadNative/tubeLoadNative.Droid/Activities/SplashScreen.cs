using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.Media;
using Android.OS;
using Android.Widget;
using Java.Lang;
using System.Threading.Tasks;
using tubeLoadNative.Droid.BroadcastReceivers;
using tubeLoadNative.Droid.Services;
using tubeLoadNative.Droid.Utils;
using tubeLoadNative.Services;

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashScreen : Activity
    {
        const string MOB_ID = "ca-app-pub-2772184448965971~2893271479";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            GoogleAnalyticsService.Instance.Initialize(this);
            GoogleAnalyticsService.Instance.TrackAppEvent(GoogleAnalyticsService.GAEventCategory.EnteringApp, "Entered splash screen");

            StartService(new Intent(this, typeof(ClosingService)));

            AudioManager audioManager = (AudioManager)this.GetSystemService(AudioService);
            ComponentName componentName = new ComponentName(this.PackageName, new BluetoothRemoteControlReciever().ComponentName);
            audioManager.RegisterMediaButtonEventReceiver(componentName);
            // build the PendingIntent for the remote control client
            Intent mediaButtonIntent = new Intent(Intent.ActionMediaButton);
            mediaButtonIntent.SetComponent(componentName);
            PendingIntent mediaPendingIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, mediaButtonIntent, 0);
            // create and register the remote control client
            RemoteControlClient remoteControlClient = new RemoteControlClient(mediaPendingIntent);
            audioManager.RegisterRemoteControlClient(remoteControlClient);

            Task.Run(() =>
            {
                checkForUpdateVersionAsync();
            });
        }

        protected override void OnResume()
        {
            base.OnResume();
           
            Task.Run(async () =>
             {
                 try
                 {
                     await YoutubeApiClient.Search();
                 }
                 catch (Exception ex)
                 {
                     GoogleAnalyticsService.Instance.TrackAppException(ex.Message, true);
                     RunOnUiThread(new Runnable(
                         () => Toast.MakeText(Application.Context, "Could not connect, please check your internet connection", ToastLength.Long).Show()));
                 };
             });

            StartActivity(new Intent(Application.Context, typeof(SongsPlayer)));
        }

        private async void checkForUpdateVersionAsync()
        {
            string currentVerion = PackageManager.GetPackageInfo(PackageName, 0).VersionName;
            VersionChecker.VersionStatus versionStatus = await VersionChecker.isVersionUpToDate(currentVerion);

            if (versionStatus == VersionChecker.VersionStatus.MissingHotFix || versionStatus == VersionChecker.VersionStatus.NeedUpdate)
            {
                RunOnUiThread(new Runnable(
                    () => Toast.MakeText(this, "New version available, please check it at tubeloadweb.com", ToastLength.Long).Show()  
                ));
            }
        }
    }
}