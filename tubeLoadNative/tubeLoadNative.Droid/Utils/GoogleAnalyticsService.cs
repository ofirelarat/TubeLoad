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
using Android.Gms.Analytics;

namespace tubeLoadNative.Droid.Utils
{
    public class GoogleAnalyticsService
    {
        public string TrackingId = "UA-99905085-1";

        private static GoogleAnalytics GAInstance;
        private static Tracker GATracker;

        #region Singleton

        static GoogleAnalyticsService instance = new GoogleAnalyticsService();

        static GoogleAnalyticsService() { }

        GoogleAnalyticsService() { }

        public static GoogleAnalyticsService Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        public enum GAEventCategory
        {
            EnteringApp,
            DonloadingSong,
            DownloadFailed,
            PlayingSong
        };

        public void Initialize(Context AppContext)
        {
            GAInstance = GoogleAnalytics.GetInstance(AppContext.ApplicationContext);
            GAInstance.SetLocalDispatchPeriod(10);

            GATracker = GAInstance.NewTracker(TrackingId);
            GATracker.EnableExceptionReporting(true);
            GATracker.EnableAdvertisingIdCollection(true);
            GATracker.EnableAutoActivityTracking(true);
        }

        public void TrackAppPage(string pageToTrack)
        {
            GATracker.SetScreenName(pageToTrack);
            GATracker.Send(new HitBuilders.ScreenViewBuilder().Build());
        }

        public void TrackAppEvent(GAEventCategory category, string eventToTrack)
        {
            HitBuilders.EventBuilder builder = new HitBuilders.EventBuilder();
            builder.SetCategory(category.ToString());
            builder.SetAction(eventToTrack);
            builder.SetLabel("AppEvent");

            GATracker.Send(builder.Build());
        }

        public void TrackAppException(string exceptionMessage, bool isFatalException)
        {
            HitBuilders.ExceptionBuilder builder = new HitBuilders.ExceptionBuilder();
            builder.SetDescription(exceptionMessage);
            builder.SetFatal(isFatalException);

            GATracker.Send(builder.Build());
        }
    }
}