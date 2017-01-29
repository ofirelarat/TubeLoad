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
using Java.Lang;
using Android.Graphics.Drawables;

namespace tubeLoadNative.Droid
{
    class SongsAdapter : BaseAdapter
    {
        private Android.App.Activity context;
        private string[] songsNames;
        private static LayoutInflater inflater = null;

        public SongsAdapter(Activity context,string[] songsNames)
        {
            this.context = context;
            this.songsNames = songsNames;
            inflater = (LayoutInflater)context.GetSystemService(Android.Content.Context.LayoutInflaterService);
        }

        public override int Count
        {
            get
            {
                return songsNames.Length;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return songsNames[position];
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                convertView = inflater.Inflate(Resource.Layout.song_adapter, null);
            }

            TextView songName = convertView.FindViewById<TextView>(Resource.Id.songName);
            songName.Text = songsNames[position];

            if (position == SongsHandler.CurrentSongIndex)
            {
                convertView.SetBackgroundColor(new Android.Graphics.Color(52, 152, 219));
            }
            else
            {
                convertView.SetBackgroundColor(Android.Graphics.Color.Transparent);
            }

            return convertView;
        }
    }
}