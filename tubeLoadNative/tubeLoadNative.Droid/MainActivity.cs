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
        private static List<SearchResult> videos;
        public static SearchResult video;
        private ListView myVideosListView;
        private EditText searchTxt;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            myVideosListView = FindViewById<ListView>(Resource.Id.songsListView);
            ImageButton searchBtn = FindViewById<ImageButton>(Resource.Id.searchBtn);
            searchTxt = FindViewById<EditText>(Resource.Id.searchEditText);

            searchBtn.Click += delegate
            {
                InputMethodManager imm = (InputMethodManager)GetSystemService(InputMethodService);
                imm.HideSoftInputFromWindow(searchTxt.WindowToken, 0);
                UpdateVideos(searchTxt.Text);
            };

            searchTxt.KeyPress += (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;

                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
                    imm.HideSoftInputFromWindow(searchTxt.WindowToken, 0);
                    UpdateVideos(searchTxt.Text);
                    e.Handled = true;
                }
            };

            myVideosListView.ItemClick += (sender, e) =>
            {
                video = videos[e.Position];
                Intent intent = new Intent(this,typeof(VideoLayout));
                StartActivity(intent);  
            };
        }

        private void UpdateVideos(string searchQuery)
        {
            try
            {
                videos = YoutubeHandler.Search(searchQuery);

                if (videos != null)
                {
                    var adapter = new VideosAdapter(this, videos.ToArray());
                    myVideosListView.Adapter = adapter;
                }
                else
                {
                    Toast.MakeText(this, "Didn't find results", ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_details, menu);
            menu.FindItem(Resource.Id.addSong).SetVisible(false);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent intent;

            switch (item.ItemId)
            {
                case Resource.Id.mySong:
                    intent = new Intent(this, typeof(mySongs));
                    StartActivity(intent);
                    return true;
                default:
                    return false;
            }
        }
    }
}


