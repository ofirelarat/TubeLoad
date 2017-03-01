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
using Android.Telephony;

namespace tubeLoadNative.Droid.Utils
{
    [BroadcastReceiver()]
    [IntentFilter(new[] { "android.intent.action.PHONE_STATE" })]
    public class IncomingCallReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            // ensure there is information
            if (intent.Extras != null)
            {
                // get the incoming call state
                string state = intent.GetStringExtra(TelephonyManager.ExtraState);

                // check the current state
                if (state == TelephonyManager.ExtraStateRinging)
                {
                    // incoming call ring
                    if (AndroidSongsManager.Instance.CurrentSong != null)
                    {
                        AndroidSongsManager.Instance.Pause();
                    }
                }
                else if (state == TelephonyManager.ExtraStateOffhook)
                {
                    // incoming call answer
                }
                else if (state == TelephonyManager.ExtraStateIdle)
                {
                    // incoming call end
                    if (AndroidSongsManager.Instance.CurrentSong != null)
                    {
                        AndroidSongsManager.Instance.Start();
                    }
                }
            }
        }
    }
}