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
using System.Threading.Tasks;
using Android.Util;

namespace tubeLoadNative.Droid
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        protected override async void OnResume()
        {
            base.OnResume();
       
            //Task.Delay(3000);
            try
            {
                await YoutubeHandler.Search();
                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            }
            catch
            {
                Toast.MakeText(Application.Context, "could not connect, please check your internet connection", ToastLength.Long).Show();
                StartActivity(new Intent(Application.Context, typeof(mySongs)));
            }
        }
    }
}