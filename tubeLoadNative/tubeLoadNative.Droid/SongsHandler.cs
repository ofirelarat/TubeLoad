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

namespace tubeLoadNative.Droid
{
    class SongsHandler
    {
        private static Java.IO.File directory;

        private static readonly SongsHandler instance = new SongsHandler();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static SongsHandler()
        {
            Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
            directory = new Java.IO.File(sdCard.AbsolutePath + "/TubeLoad");
            directory.Mkdirs();
        }

        private SongsHandler()
        {
        }

        public static SongsHandler Instance
        {
            get
            {
                return instance;
            }
        }
    }
}