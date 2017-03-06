
using Android.App;
using Android.Content;
using Android.Telephony;

namespace tubeLoadNative.Droid.Utils
{
    [BroadcastReceiver()]
    [IntentFilter(new[] { "android.intent.action.PHONE_STATE" })]
    public class IncomingCallReceiver : BroadcastReceiver
    {
        static bool isPlayed = false;
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
                        if (AndroidSongsManager.Instance.IsPlaying)
                        {
                            isPlayed = true;
                            AndroidSongsManager.Instance.Pause();
                        }
                        else
                        {
                            isPlayed = false;
                        }
                    }
                }
                else if (state == TelephonyManager.ExtraStateOffhook)
                {
                    // incoming call answer
                }
                else if (state == TelephonyManager.ExtraStateIdle)
                {
                    // incoming call end
                    if (AndroidSongsManager.Instance.CurrentSong != null && isPlayed)
                    {
                        AndroidSongsManager.Instance.Start();
                    }
                }
            }
        }
    }
}