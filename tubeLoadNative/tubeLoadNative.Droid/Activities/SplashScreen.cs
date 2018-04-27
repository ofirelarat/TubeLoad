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
        bool passedVersionCheck = true;
        const string MOB_ID = "ca-app-pub-2772184448965971~2893271479";

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            GoogleAnalyticsService.Instance.Initialize(this);
            GoogleAnalyticsService.Instance.TrackAppEvent(GoogleAnalyticsService.GAEventCategory.EnteringApp, "Entered splash screen");

            MobileAds.Initialize(this, MOB_ID);

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

            string currentVerion = this.PackageManager.GetPackageInfo(PackageName, 0).VersionName;
            VersionChecker.VersionStatus versionStatus = await VersionChecker.isVersionUpToDate(currentVerion);
            if (versionStatus == VersionChecker.VersionStatus.NeedUpdate)
            {
                passedVersionCheck = false;
                createNewVersionDialog();
            }
            else if (versionStatus == VersionChecker.VersionStatus.MissingHotFix)
            {
                Toast.MakeText(this, "New version available, please check it at tubeloadweb.com", ToastLength.Long).Show();
            }
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

            if (passedVersionCheck)
            {
                StartActivity(new Intent(Application.Context, typeof(SongsPlayer)));
            }
        }

        private void createNewVersionDialog()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetCancelable(false);
            builder.SetTitle("New Version");
            builder.SetMessage("Your version is out of date. Do you want to install the new one?");
            builder.SetPositiveButton("accept", (s, e) =>
            {
                var uri = Android.Net.Uri.Parse("http://www.tubeloadweb.com");
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
                Finish();
            });
            builder.SetNegativeButton("ignore", (s, e) => {
                StartActivity(new Intent(Application.Context, typeof(SongsPlayer)));
            });
            builder.Show();
        }
    }
}