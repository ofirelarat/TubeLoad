using Android.App;
using Android.Gms.Ads;

namespace tubeLoadNative.Droid.Services
{
    static class AdsService
    {
        const string MOB_ID = "ca-app-pub-2772184448965971~2893271479";
        public static InterstitialAd InterstitialAd { get; set; }
        public static AdView CurrentSongAd { get; set; }
        public static AdView DownloadSongAd { get; set; }

        static AdsService()
        {
            MobileAds.Initialize(Application.Context, MOB_ID);

            InterstitialAd = new InterstitialAd(Application.Context);
            InterstitialAd.AdUnitId = "ca-app-pub-2772184448965971/8660508944";
            InterstitialAd.LoadAd(new AdRequest.Builder().Build());
        }

        public static void LoadInterstitial(InterstitialAd ad)
        {
            ad.LoadAd(new AdRequest.Builder().Build());
        }

        public static void LoadBanner(AdView ad)
        {
            ad.LoadAd(new AdRequest.Builder().Build());
            ad.Resume();
        }
    }
}