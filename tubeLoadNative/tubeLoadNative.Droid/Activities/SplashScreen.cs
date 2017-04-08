using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Java.Lang;
using System.Threading.Tasks;
using tubeLoadNative.Services;

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashScreen : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
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
                catch
                {
                    RunOnUiThread(new Runnable(
                        () => Toast.MakeText(Application.Context, "Could not connect, please check your internet connection", ToastLength.Long).Show()));
                };
            });                

            StartActivity(new Intent(Application.Context, typeof(SongsPlayer)));
        }
    }
}