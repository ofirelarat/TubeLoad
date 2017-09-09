using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Views;

using tubeLoadNative.Droid.Utils;

namespace tubeLoadNative.Droid.BroadcastReceivers
{
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionMediaButton, BluetoothAdapter.ActionStateChanged, BluetoothAdapter.ActionConnectionStateChanged })]
    public class BluetoothRemoteControlReciever : BroadcastReceiver
    {
        public string ComponentName { get { return Class.Name; } }
        private static bool secondTimeFlag = false;

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals(BluetoothAdapter.ActionStateChanged))
            {
                State state = (State)intent.GetIntExtra(BluetoothAdapter.ExtraState, BluetoothAdapter.Error);

                if (AndroidSongsManager.Instance.IsPlaying)
                {
                    if (state.Equals(State.TurningOff) || state.Equals(State.Off))
                    {
                        AndroidSongsManager.Instance.Pause();
                    }
                }
            }
            else if (intent.Action.Equals(BluetoothAdapter.ActionConnectionStateChanged))
            {
                State state = (State)intent.GetIntExtra(BluetoothAdapter.ExtraConnectionState, BluetoothAdapter.Error);
                if (AndroidSongsManager.Instance.IsPlaying)
                {
                    if (state.Equals(State.Disconnected))
                    {
                        AndroidSongsManager.Instance.Pause();
                    }
                }
            }
            else
            {
                if (!secondTimeFlag && intent.Extras != null)
                {
                    // get the key event
                    KeyEvent state = (KeyEvent)intent.GetParcelableExtra(Intent.ExtraKeyEvent);

                    switch (state.KeyCode)
                    {
                        case Keycode.MediaPlay:
                            AndroidSongsManager.Instance.Start();
                            break;

                        case Keycode.MediaPause:
                            AndroidSongsManager.Instance.Pause();
                            break;

                        case Keycode.MediaStop:
                            AndroidSongsManager.Instance.Stop();
                            break;

                        case Keycode.Headsethook:
                        case Keycode.MediaPlayPause:
                            if (AndroidSongsManager.Instance.IsPlaying)
                            {
                                AndroidSongsManager.Instance.Pause();
                            }
                            else
                            {
                                AndroidSongsManager.Instance.Start();
                            }
                            break;

                        case Keycode.MediaNext:
                            AndroidSongsManager.Instance.PlayNext();
                            break;

                        case Keycode.MediaPrevious:
                            AndroidSongsManager.Instance.PlayPrev();
                            break;
                    }
                }

                secondTimeFlag = !secondTimeFlag;
            }
        }
    }
}