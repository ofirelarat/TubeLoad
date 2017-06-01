using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Java.Lang;
using System.Threading.Tasks;
using tubeLoadNative.Droid.Utils;
using tubeLoadNative.Services;

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashScreen : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            GoogleAnalyticsService.Instance.Initialize(this);
            GoogleAnalyticsService.Instance.TrackAppEvent(GoogleAnalyticsService.GAEventCategory.EnteringApp, "Entered splash screen");


            string currentVerion = this.PackageManager.GetPackageInfo(PackageName, 0).VersionName;
            if (!VersionChecker.isVersionUpToDate(currentVerion))
            {
                Toast.MakeText(this, "Your App is not up to date, you can download newer version at: www.TubeLoadWeb.com", ToastLength.Long).Show();
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

            StartActivity(new Intent(Application.Context, typeof(SongsPlayer)));
        }
    }
}