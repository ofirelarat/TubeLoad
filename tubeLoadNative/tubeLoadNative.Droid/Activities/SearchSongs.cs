using System;
using Android.App;
using Android.OS;
using System.Collections.Generic;
using Google.Apis.YouTube.v3.Data;
using Android.Widget;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using System.Threading.Tasks;
using Android.Graphics;
using tubeLoadNative.Droid.Utils;
using tubeLoadNative.Services;

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Label = "TubeLoad", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleInstance, Icon = "@drawable/icon")]
    public class SearchSongs : Android.App.Activity
    {
        AndroidSongsManager mediaPlayer = AndroidSongsManager.Instance;

        static List<SearchResult> videos;
        public static SearchResult video;
        ListView myVideosListView;
        EditText searchTxt;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_search_songs);

            myVideosListView = FindViewById<ListView>(Resource.Id.songsListView);
            ImageButton searchBtn = FindViewById<ImageButton>(Resource.Id.searchBtn);
            searchTxt = FindViewById<EditText>(Resource.Id.searchEditText);
            searchTxt.Text = string.Empty;

            searchBtn.Click += async delegate
            {
                InputMethodManager imm = (InputMethodManager)GetSystemService(InputMethodService);
                imm.HideSoftInputFromWindow(searchTxt.WindowToken, 0);
                await UpdateVideos(searchTxt.Text);
            };

            searchTxt.KeyPress += async (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;

                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
                    imm.HideSoftInputFromWindow(searchTxt.WindowToken, 0);
                    await UpdateVideos(searchTxt.Text);
                    e.Handled = true;
                }
            };

            myVideosListView.ItemClick += (sender, e) =>
            {
                video = videos[e.Position];
                Intent intent = new Intent(this, typeof(DownloadSong));
                StartActivity(intent);
            };

            await UpdateVideos(string.Empty);
        }

        async Task UpdateVideos(string searchQuery)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("loading...");


            try
            {
                if (searchQuery == string.Empty)
                {
                    if (YoutubeApiClient.MostPopularSongs == null)
                    {
                        progress.Show();
                        await YoutubeApiClient.Search();
                    }

                    videos = YoutubeApiClient.MostPopularSongs;
                }
                else
                {
                    progress.Show();

                    videos = await YoutubeApiClient.Search(searchQuery);
                }

                if (videos != null)
                {
                    Dictionary<string, Bitmap> images = await LoadImages(videos.ToArray());
                    var adapter = new VideosAdapter(this, videos.ToArray(), images);
                    myVideosListView.Adapter = adapter;
                }
                else
                {
                    Toast.MakeText(this, "Didn't find results", ToastLength.Long).Show();
                }

                progress.Dismiss();
            }
            catch (Exception)
            {
                progress.Dismiss();
                Toast.MakeText(this, "Could not connect to Youtube", ToastLength.Long).Show();
            }
        }

        async Task<Dictionary<string, Bitmap>> LoadImages(SearchResult[] searchResults)
        {
            Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();

            foreach (var result in searchResults)
            {
                Thumbnail logo = result.Snippet.Thumbnails.Medium;
                Bitmap imageBitmap = await Common.GetImageBitmapFromUrlAsync(logo.Url);
                images.Add(result.Id.VideoId, imageBitmap);
            }

            return images;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.main_menu, menu);
            menu.FindItem(Resource.Id.addSong).SetVisible(false);
            menu.FindItem(Resource.Id.currentSong).SetVisible(true);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent intent;

            switch (item.ItemId)
            {
                case Resource.Id.mySong:
                    if (mediaPlayer.Songs.Count > 0)
                    {
                        intent = new Intent(this, typeof(SongsPlayer));
                        StartActivity(intent);
                    }
                    else
                    {
                        Toast.MakeText(this, "first download some songs", ToastLength.Long).Show();
                    }
                    return true;

                case Resource.Id.currentSong:
                    if (mediaPlayer.CurrentSong != null)
                    {
                        intent = new Intent(this, typeof(CurrentSong));
                        StartActivity(intent);
                    }
                    else
                    {
                        Toast.MakeText(this, "first play song", ToastLength.Long).Show();
                    }
                    return true;

                default:
                    return false;
            }
        }
    }
}


