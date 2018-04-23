using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Views;

using tubeLoadNative.Droid.Utils;

namespace tubeLoadNative.Droid.BroadcastReceivers
{
    [BroadcastReceiver(Exported = false)]
    [IntentFilter(new[] { Intent.ActionMediaButton, BluetoothAdapter.ActionStateChanged, BluetoothAdapter.ActionConnectionStateChanged })]
    public class BluetoothRemoteControlReciever : BroadcastReceiver
    {
        public string ComponentName { get { return Class.Name; } }

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
                    if (state.Equals(State.Disconnected) || state.Equals(State.Connected))
                    {
                        AndroidSongsManager.Instance.Pause();
                    }
                }
            }
            else
            {
                if (intent.Extras != null)
                {
                    // get the key event
                    KeyEvent state = (KeyEvent)intent.GetParcelableExtra(Intent.ExtraKeyEvent);

                    if (state.Action.Equals(KeyEventActions.Down)){
                        switch (state.KeyCode)
                        {
                            case Keycode.MediaPlay:
                                AndroidSongsManager.Instance.Start();
                                break;

                            case Keycode.MediaPause:
                                AndroidSongsManager.Instance.Pause();
                                break;

                            case Keycode.MediaStop:
                                if (AndroidSongsManager.Instance.IsPlaying)
                                {
                                    AndroidSongsManager.Instance.Stop();
                                }
                                NotificationHandler.DeleteNotification();
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
                }
            }
        }
    }
}