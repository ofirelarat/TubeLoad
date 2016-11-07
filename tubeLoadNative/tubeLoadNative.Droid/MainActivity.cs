using System;
using Android.App;
using Android.OS;
using System.Collections.Generic;
using Google.Apis.YouTube.v3.Data;
using Android.Widget;
using Android.Content;
using tubeLoadNative.Droid.Resources;
using Android.Views;
using Android.Views.InputMethods;

namespace tubeLoadNative.Droid
{
	[Activity (Label = "TubeLoad", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Android.App.Activity
    {
        public static List<SearchResult> videos { get; private set; }
        private ListView myVideosListView;
        private EditText searchTxt;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // ActionBar.SetCustomView(Resource.Layout.action_bar);
            // ActionBar.SetDisplayShowCustomEnabled(true);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            myVideosListView = FindViewById<ListView>(Resource.Id.songsListView);
            ImageButton searchBtn = FindViewById<ImageButton>(Resource.Id.searchBtn);
            searchTxt = FindViewById<EditText>(Resource.Id.searchEditText);
            searchBtn.Click += delegate
            {
                InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
                imm.HideSoftInputFromWindow(searchTxt.WindowToken, 0);
                YoutubeResult();
            };

            searchTxt.KeyPress += (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
                    imm.HideSoftInputFromWindow(searchTxt.WindowToken, 0);
                    YoutubeResult();
                    e.Handled = true;
                }
            };

            myVideosListView.ItemClick += (sender, e) =>
            {
                string item = myVideosListView.GetItemAtPosition(e.Position).ToString();
                Intent intent = new Intent(this,typeof(VideoLayout));
                intent.PutExtra("selectedVideo", item);
                StartActivity(intent);  
            };
        }

        private void YoutubeResult()
        {
            try
            {
                Search search = new Search();
                videos = search.Run(searchTxt.Text);

                if (videos != null)
                {
                    List<string> videoNames = new List<string>();
                    foreach (var item in videos)
                    {
                        videoNames.Add(item.Snippet.Title + " <" + item.Id.VideoId + ">");
                    }

                    //ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, videoNames);
                    var adapter = new CostumAdapter(this, videos.ToArray());
                    myVideosListView.Adapter = adapter;
                }
                else { Toast.MakeText(this, "Error - could not find resoults", ToastLength.Long).Show(); }
            }
            catch (Exception ex) { Toast.MakeText(this, "Error - " + ex.Message, ToastLength.Long).Show(); }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_details, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent intent;
            switch (item.ItemId)
            {
                case Resource.Id.addSong:
                    //intent = new Intent(this, typeof(MainActivity));
                    //StartActivity(intent);
                    return true;

                case Resource.Id.mySong:
                    intent = new Intent(this, typeof(mySongs));
                    StartActivity(intent);
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}


