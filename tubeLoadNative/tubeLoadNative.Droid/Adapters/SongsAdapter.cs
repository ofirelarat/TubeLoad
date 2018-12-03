using Android.App;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using tubeLoadNative.Droid.Utils;
using tubeLoadNative.Models;

namespace tubeLoadNative.Droid
{
    class SongsAdapter : BaseAdapter
    {
        AndroidSongsManager mediaPlayer = AndroidSongsManager.Instance;

        private Android.App.Activity context;
        private Song[] songs;
        private static LayoutInflater inflater = null;

        public SongsAdapter(Activity context,Song[] songs)
        {
            this.context = context;
            this.songs = songs;
            inflater = (LayoutInflater)context.GetSystemService(Android.Content.Context.LayoutInflaterService);
        }

        public override int Count
        {
            get
            {
                return songs.Length;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return songs[position].Name;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                convertView = inflater.Inflate(Resource.Layout.adapter_songs, null);
            }

            TextView songName = convertView.FindViewById<TextView>(Resource.Id.songName);
            songName.Text = songs[position].Name;

            if (mediaPlayer.CurrentSong != null && songs[position].Id.Equals(mediaPlayer.CurrentSong.Id))
            {
                convertView.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.brightassets)));
            }
            else
            {
                convertView.SetBackgroundColor(Color.Transparent);
            }

            return convertView;
        }
    }
}