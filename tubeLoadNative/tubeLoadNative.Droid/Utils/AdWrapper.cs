using Android.Content;
using Android.Gms.Ads;

namespace tubeLoadNative.Droid.Utils
{
    public static class AdWrapper
    {
        public static AdView ConstructStandardBanner(Context con, AdSize adsize, string UnitID)
        {
            var ad = new AdView(con);
            ad.AdSize = adsize;
            ad.AdUnitId = UnitID;
            return ad;
        }
        public static AdView CustomBuild(this AdView ad)
        {
            var requestbuilder = new AdRequest.Builder();
            ad.LoadAd(requestbuilder.Build());
            return ad;
        }
    }
}