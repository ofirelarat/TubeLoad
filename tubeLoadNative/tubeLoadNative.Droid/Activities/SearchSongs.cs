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
using Android.Support.V4.Content;
using tubeLoadNative.Models;
using System;

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Label = "TubeLoad", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleInstance, Icon = "@drawable/icon")]
    public class SearchSongs : Android.App.Activity
    {
        AndroidSongsManager mediaPlayer = AndroidSongsManager.Instance;
        
        static List<SearchResultDownloadItem> videos;
        static SearchResultDownloadItem selectedVideo;
        ListView myVideosListView;
        EditText searchString;        

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_search_songs);

            myVideosListView = FindViewById<ListView>(Resource.Id.songsListView);

            ImageButton searchButton = FindViewById<ImageButton>(Resource.Id.searchBtn);
            searchString = FindViewById<EditText>(Resource.Id.searchEditText);
            searchString.Text = string.Empty;
            searchString.Background.SetTint(ContextCompat.GetColor(this, Resource.Color.darkassets));

            searchButton.Click += async delegate
            {
                HideKeyboard(searchButton.Context);
                await UpdateVideos(searchString.Text);
            };

            searchString.KeyPress += async (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;

                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    HideKeyboard(searchString.Context);
                    await UpdateVideos(searchString.Text);
                    e.Handled = true;
                }
            };

            myVideosListView.ItemClick += (sender, e) =>
            {
                selectedVideo = videos[e.Position];
                Intent intent = new Intent(this, typeof(DownloadSong));
                StartActivity(intent);
            };

            await UpdateVideos(string.Empty);
        }

        protected async override void OnResume()
        {
            base.OnResume();
            await UpdateVideos(string.Empty);
        }

        void HideKeyboard(Context context)
        {
            var inputMethodManager = context.GetSystemService(InputMethodService) as InputMethodManager;
            
            if (inputMethodManager != null && context is Android.App.Activity)
            {
                var activity = context as Android.App.Activity;
                var token = activity.CurrentFocus == null ? null : activity.CurrentFocus.WindowToken;
                inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.ImplicitOnly);
            }
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

                    List<SearchResult> originalResults = YoutubeApiClient.MostPopularSongs;
                    videos = Common.GetSearchResultsItems(originalResults.ToArray());
                }
                else
                {
                    progress.Show();
                   
                    List<SearchResult> originalResults = await YoutubeApiClient.Search(searchQuery);
                    videos = Common.GetSearchResultsItems(originalResults.ToArray());
                }

                if (videos != null)
                {
                    Dictionary<string, Bitmap> images = await LoadImages(videos.ToArray());
                    var adapter = new VideosAdapter(this, videos, images);
                    myVideosListView.Adapter = adapter;
                }
                else
                {
                    Toast.MakeText(this, "Didn't find results", ToastLength.Long).Show();
                }
            }
            catch
            {
                Toast.MakeText(this, "Could not connect to Youtube", ToastLength.Long).Show();
            }
            finally
            {
                progress.Dismiss();
            }
        }

        async Task<Dictionary<string, Bitmap>> LoadImages(SearchResultDownloadItem[] searchResults)
        {
            Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();

            foreach (SearchResultDownloadItem result in searchResults)
            {
                Thumbnail logo = result.YoutubeResult.Snippet.Thumbnails.Medium;
                Bitmap imageBitmap = await Common.GetImageBitmapFromUrlAsync(logo.Url);
                images.Add(result.YoutubeResult.Id.VideoId, imageBitmap);
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

        public static List<SearchResultDownloadItem> getSearchedVideos()
        {
            return videos;
        }

        public static SearchResultDownloadItem getSelectedVideo()
        {
            return selectedVideo;
        }
    }
}


