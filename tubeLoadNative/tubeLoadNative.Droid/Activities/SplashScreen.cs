using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
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

        protected override async void OnResume()
        {
            base.OnResume();
       
            try
            {
                await YoutubeApiClient.Search();
                StartActivity(new Intent(Application.Context, typeof(SearchSongs)));
            }
            catch
            {
                Toast.MakeText(Application.Context, "Could not connect, please check your internet connection", ToastLength.Long).Show();
                StartActivity(new Intent(Application.Context, typeof(SongsPlayer)));
            }
        }
    }
}