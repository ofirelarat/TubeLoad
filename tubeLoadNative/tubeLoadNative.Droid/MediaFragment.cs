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
using Android.Media;
using Android.Graphics;
using Java.Lang;

namespace tubeLoadNative.Droid
{
    public class MediaFragment : Fragment, SeekBar.IOnSeekBarChangeListener
    {
        double finalTime = mySongs.mediaPlayer.Duration;
        double startTime = mySongs.mediaPlayer.CurrentPosition;
        SeekBar seekBar;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_details,
                container, false);
            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            ImageView img = View.FindViewById<ImageView>(Resource.Id.imageView);
            TextView fileName = View.FindViewById<TextView>(Resource.Id.fileTextView);
            seekBar = View.FindViewById<SeekBar>(Resource.Id.seekBar);
            MediaMetadataRetriever mmr = new MediaMetadataRetriever();

            Bundle bundle = Arguments;
            if (bundle != null)
            {
                string filePath = bundle.GetString("FilePath");
                Java.IO.File currentFile = new Java.IO.File(filePath);

                mmr.SetDataSource(filePath);

                byte[] art = mmr.GetEmbeddedPicture();
                if (art != null)
                {
                    img.SetImageBitmap(BitmapFactory.DecodeByteArray(art, 0, art.Length));
                }
                else
                {
                    img.SetImageResource(Resource.Drawable.icon);
                }

                double finalTime = mySongs.mediaPlayer.Duration;
                double startTime = mySongs.mediaPlayer.CurrentPosition;

                seekBar.Max = ((int)finalTime);
                

                fileName.Text = currentFile.Name;
            }
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            if (fromUser)
            {
                mySongs.mediaPlayer.SeekTo(progress);
            }
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            throw new NotImplementedException();
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            throw new NotImplementedException();
        }
    }
}
