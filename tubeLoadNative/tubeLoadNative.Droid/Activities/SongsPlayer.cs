using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Media;
using Android.Graphics.Drawables;
using Android.Graphics;
using System.IO;
using tubeLoadNative.Droid.Utils;
using tubeLoadNative.Models;
using tubeLoadNative.Droid.Views;
using Android.Support.V4.Content;

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Label = "TubeLoad")]
    public class SongsPlayer : Activity
    {
        AndroidSongsManager mediaPlayer = AndroidSongsManager.Instance;

        ListView songsListView;
        List<Song> songs;
        ImageButton playBtn;

        SeekbarView seekbarview;
        AlertDialog seekbarDialog;
        Song selectedSong;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_songs_player);

            GoogleAnalyticsService.Instance.Initialize(this);
            GoogleAnalyticsService.Instance.TrackAppPage("Songs player");

            songsListView = FindViewById<ListView>(Resource.Id.songsListView);
            playBtn = FindViewById<ImageButton>(Resource.Id.playBtn);
            ImageButton nextBtn = FindViewById<ImageButton>(Resource.Id.nextBtn);
            ImageButton prevBtn = FindViewById<ImageButton>(Resource.Id.prevBtn);

            playBtn.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkassets)));
            nextBtn.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkassets)));
            prevBtn.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.darkassets)));

            FileManager.SongsListUpdate();
            UpdateList();

            songsListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
            {
                GoogleAnalyticsService.Instance.TrackAppEvent(GoogleAnalyticsService.GAEventCategory.PlayingSong, "Playing " + songs[e.Position].Name);
                Play(songs[e.Position].Id);
            };

            RegisterForContextMenu(songsListView);

            mediaPlayer.Completing += delegate
            {
                if (seekbarDialog != null)
                {
                    seekbarDialog.Cancel();
                    CloseContextMenu();
                }

                if (seekbarview != null)
                {
                    seekbarview.AbortSeekbarThread();
                }

                if (!mediaPlayer.IsPlaying)
                {
                    TogglePause();
                }
            };

            mediaPlayer.Saving += (sender, e) => UpdateList();

            mediaPlayer.Starting += delegate
            {
                TogglePlay();
            };

            mediaPlayer.StartingNewSong += delegate
            {
                UpdateList();
            };

            mediaPlayer.Pausing += delegate
            {
                TogglePause();
            };

            ChangePlayingView();

            nextBtn.Click += delegate
            {
                mediaPlayer.PlayNext();
            };

            prevBtn.Click += delegate
            {
                mediaPlayer.PlayPrev();
            };
        }

        void TogglePlay()
        {
            playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
            playBtn.Click -= Start;
            playBtn.Click += Pause;
        }

        void TogglePause()
        {
            playBtn.SetImageResource(Resource.Drawable.ic_media_play);
            playBtn.Click -= Pause;
            playBtn.Click += Start;
        }

        void UpdateList()
        {
            songs = mediaPlayer.Songs;

            int index = songsListView.FirstVisiblePosition;
            View songView = songsListView.GetChildAt(0);
            int top = (songView == null) ? 0 : songView.Top - songsListView.ListPaddingTop;

            BaseAdapter adapter = new SongsAdapter(this, songs.Select((x) => x.Name.Replace(".mp3", string.Empty)).ToArray());
            songsListView.Adapter = adapter;

            songsListView.SetSelectionFromTop(index, top);
        }

        void Start(object sender, EventArgs e)
        {
            mediaPlayer.Start();
        }

        void Pause(object sender, EventArgs e)
        {
            mediaPlayer.Pause();
        }

        void Play(string id)
        {
            FileManager.SongsListUpdate(id);
            mediaPlayer.Start(id);
        }

        void ChangePlayingView()
        {
            if (mediaPlayer.IsPlaying)
            {
                TogglePlay();
            }
            else
            {
                TogglePause();
            }
        }

        protected override void OnResume()
        {
            FileManager.SongsListUpdate();
            UpdateList();
            ChangePlayingView();

            base.OnResume();
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if (v.Id == Resource.Id.songsListView)
            {
                var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
                selectedSong = songs[info.Position];

                menu.SetHeaderTitle(GetSelectedSongTitle());

                Drawable picture = SongMetadata.GetSongPicture(selectedSong.Id);

                if (picture != null)
                {
                    menu.SetHeaderIcon(picture);
                }
                else
                {
                    menu.SetHeaderIcon(Resource.Drawable.default_song_image);
                }

                var inflater = MenuInflater;
                inflater.Inflate(Resource.Menu.song_actions_menu, menu);

                if (mediaPlayer.CurrentSong != null && mediaPlayer.IsPlaying && selectedSong.Id == mediaPlayer.CurrentSong.Id)
                {
                    menu.FindItem(Resource.Id.seek_bar).SetVisible(true);
                }
            }
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.item_delete:
                    try
                    {
                        mediaPlayer.DeleteSong(selectedSong.Id);
                        UpdateList();
                    }
                    catch (Exception ex)
                    {
                        GoogleAnalyticsService.Instance.TrackAppException(ex.Message, false);
                        Toast.MakeText(Application.Context, "could not delete this song", ToastLength.Long).Show();
                    }

                    return true;

                case Resource.Id.item_play:
                    Play(selectedSong.Id);
                    Intent intent = new Intent(this, typeof(CurrentSong));
                    StartActivity(intent);

                    return true;

                case Resource.Id.item_rename:
                    RenameSong();

                    return true;

                case Resource.Id.seek_bar:
                    AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(this);

                    seekbarview = new SeekbarView(alertDialogBuilder.Context, null);
                    alertDialogBuilder.SetView(seekbarview);

                    alertDialogBuilder.SetCancelable(false);
                    alertDialogBuilder.SetPositiveButton("OK", (s, e) =>
                    {
                        seekbarview.AbortSeekbarThread();
                    });

                    alertDialogBuilder.SetTitle(GetSelectedSongTitle());

                    Drawable picture = SongMetadata.GetSongPicture(selectedSong.Id);

                    if (picture != null)
                    {
                        alertDialogBuilder.SetIcon(picture);
                    }
                    else
                    {
                        alertDialogBuilder.SetIcon(Resource.Drawable.default_song_image);
                    }

                    seekbarDialog = alertDialogBuilder.Create();
                    seekbarDialog.Show();

                    return true;

                default:
                    return base.OnContextItemSelected(item);
            }
        }

        string GetSelectedSongTitle()
        {
            MediaMetadataRetriever metadata = SongMetadata.GetMetadata(selectedSong.Id);
            string title = metadata.ExtractMetadata(MetadataKey.Title);

            if (title == null)
            {
                title = selectedSong.Name.Replace(".mp3", string.Empty);
            }

            return title;
        }

        void RenameSong()
        {
            AlertDialog.Builder renameDialog = new AlertDialog.Builder(this);
            EditText renameText = new EditText(this);
            renameText.Text = selectedSong.Name.Replace(".mp3", string.Empty);
            renameText.SetSingleLine();
            renameDialog.SetTitle("Rename");
            renameDialog.SetView(renameText);

            renameDialog.SetPositiveButton("OK", (s, e) =>
            {
                try
                {
                    if (renameText.Text != string.Empty && FileManager.FindSong(renameText.Text) == null && !File.Exists(FileManager.PATH + renameText.Text) && renameText.Text.Length <= 100)
                    {
                        mediaPlayer.RenameSong(selectedSong.Id, renameText.Text);
                        UpdateList();
                    }
                    else
                    {
                        Toast.MakeText(Application.Context, "Not a valid name", ToastLength.Long).Show();
                    }
                }
                catch (Exception ex)
                {
                    GoogleAnalyticsService.Instance.TrackAppException(ex.Message, false);
                    Toast.MakeText(Application.Context, "Could not rename the song", ToastLength.Long).Show();
                }
            });

            renameDialog.Show();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.main_menu, menu);
            menu.FindItem(Resource.Id.mySong).SetVisible(false);

            menu.FindItem(Resource.Id.currentSong).SetVisible(true);
            menu.FindItem(Resource.Id.orderSongs).SetVisible(true);
            menu.FindItem(Resource.Id.suffleSongs).SetVisible(true);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent intent;

            switch (item.ItemId)
            {
                case Resource.Id.addSong:
                    intent = new Intent(this, typeof(SearchSongs));
                    StartActivity(intent);
                    Finish();

                    return true;

                case Resource.Id.currentSong:
                    if (mediaPlayer.CurrentSong != null)
                    {
                        intent = new Intent(this, typeof(CurrentSong));
                        StartActivity(intent);
                        Finish();
                    }
                    else
                    {
                        Toast.MakeText(this, "No song is playing", ToastLength.Long).Show();
                    }
                    return true;

                case Resource.Id.alphabeticSort:
                    mediaPlayer.SortSongs(Song.AlphabeticCompare);
                    item.SetChecked(true);
                    UpdateList();
                    return true;

                case Resource.Id.modifiedSort:
                    mediaPlayer.UpdateSongsList();
                    item.SetChecked(true);
                    UpdateList();
                    return true;

                case Resource.Id.suffleSongs:
                    mediaPlayer.ShuffleSongs();
                    item.SetChecked(true);
                    UpdateList();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}