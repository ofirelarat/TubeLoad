using Android.App;
using Android.Gms.Ads;
using System;
using System.Diagnostics;

namespace tubeLoadNative.Droid.Services
{
    static class AdsService
    {
        const string MOB_ID = "ca-app-pub-2772184448965971~2893271479";
        public static InterstitialAd InterstitialAd { get; set; }
        public static AdView CurrentSongAd { get; set; }
        public static AdView DownloadSongAd { get; set; }

        public class AdListenerService : AdListener
        {
            Action loadAction;
            Action failAction;

            public AdListenerService(Action loadAction, Action failAction)
            {
                this.loadAction = loadAction;
                this.failAction = failAction;
            }

            public override void OnAdFailedToLoad(int errorCode)
            {
                base.OnAdFailedToLoad(errorCode);
                failAction.Invoke();
            }

            public override void OnAdLoaded()
            {
                base.OnAdLoaded();
                loadAction.Invoke();
            }

            public override void OnAdClosed()
            {
                base.OnAdClosed();
                failAction.Invoke();
            }
        }

        static AdsService()
        {
            MobileAds.Initialize(Application.Context, MOB_ID);

            InterstitialAd = new InterstitialAd(Application.Context);
            InterstitialAd.AdUnitId = "ca-app-pub-2772184448965971/8660508944";
            LoadInterstitial(InterstitialAd);
            AdListener adListener = new AdListenerService(() => { }, () => LoadInterstitial(InterstitialAd));
            InterstitialAd.AdListener = adListener;
        }

        public static void LoadInterstitial(InterstitialAd ad)
        {
            ad.LoadAd(new AdRequest.Builder().AddTestDevice("AC40E1EB360E55D557720956A84B5A0C").AddTestDevice("578B474E3B66E8C5906331F12515E179").Build());
        }

        public static void LoadBanner(AdView ad)
        {
            AdRequest x = new AdRequest.Builder().AddTestDevice("AC40E1EB360E55D557720956A84B5A0C").AddTestDevice("578B474E3B66E8C5906331F12515E179").Build();
            if (x.IsTestDevice(Application.Context))
            {
                Debugger.Break();
            }
            ad.LoadAd(x);
            
        }
    }
}