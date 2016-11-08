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

namespace tubeLoadNative.Droid
{
    [Activity(Label = "CostumAdapterSong")]
    public class CostumAdapterSong : BaseAdapter
    {
        Activity context;
        string[] songNames;
        private static LayoutInflater inflater = null;

        public CostumAdapterSong(Activity context, string[] songNames)
        {
            this.songNames = songNames;
            this.context = context;
            inflater = (LayoutInflater)context.GetSystemService(Activity.LayoutInflaterService);
        }

        public override int Count
        {
            get
            {
                return songNames.Length;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return songNames[position];
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View vi = convertView;
            if (vi == null)
            {
                vi = inflater.Inflate(Resource.Layout.custom_row_songs,null);
            }

            TextView songName = vi.FindViewById<TextView>(Resource.Id.songName);
            songName.Text = songNames[position];

            return vi;
        }
    }
}