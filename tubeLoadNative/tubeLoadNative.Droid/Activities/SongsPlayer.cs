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
using System.Threading;
using System.IO;
using tubeLoadNative.Droid.Utils;
using tubeLoadNative.Models;

namespace tubeLoadNative.Droid.Activities
{
    [Activity(Label = "TubeLoad", LaunchMode = Android.Content.PM.LaunchMode.SingleInstance)]
    public class SongsPlayer : Activity
    {
        AndroidSongsManager mediaPlayer = AndroidSongsManager.Instance;

        ListView songsListView;
        List<Song> songs;
        ImageButton playBtn;

        SeekBar seekBar;
        AlertDialog seekbarDialog;
        Thread seekThread;
        Song selectedSong;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_songs_player);

            songsListView = FindViewById<ListView>(Resource.Id.songsListView);
            playBtn = FindViewById<ImageButton>(Resource.Id.playBtn);
            ImageButton nextBtn = FindViewById<ImageButton>(Resource.Id.nextBtn);
            ImageButton prevBtn = FindViewById<ImageButton>(Resource.Id.prevBtn);

            playBtn.SetBackgroundColor(new Color(Resource.Color.darkassets));
            nextBtn.SetBackgroundColor(new Color(Resource.Color.darkassets));
            prevBtn.SetBackgroundColor(new Color(Resource.Color.darkassets));

            FileManager.SongsListUpdate();
            UpdateList();

            songsListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
            {
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

                if (seekThread != null)
                {
                    seekThread.Abort();
                }

                if (!mediaPlayer.IsPlaying)
                {
                    TogglePause();
                }
            };

            mediaPlayer.Saving += (sender, e) => UpdateList();

            mediaPlayer.Starting += delegate
            {
                int index = songsListView.FirstVisiblePosition;
                View v = songsListView.GetChildAt(0);
                int top = (v == null) ? 0 : v.Top - songsListView.ListPaddingTop;
                UpdateList();
                songsListView.SetSelectionFromTop(index, top);
            };

            if (mediaPlayer.IsPlaying)
            {
                TogglePlay();
            }
            else
            {
                TogglePause();
            }


            nextBtn.Click += delegate
            {
                if (mediaPlayer.PlayNext())
                {
                    TogglePlay();
                }
            };

            prevBtn.Click += delegate
            {
                if (mediaPlayer.PlayPrev())
                {
                    TogglePlay();
                }
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

            BaseAdapter adapter = new SongsAdapter(this, songs.Select((x) => x.Name.Replace(".mp3", string.Empty)).ToArray());
            songsListView.Adapter = adapter;
        }

        void Start(object sender, EventArgs e)
        {
            if (mediaPlayer.Start())
            {
                playBtn.Click -= Start;
                playBtn.Click += Pause;
                playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
            }
        }

        void Pause(object sender, EventArgs e)
        {
            mediaPlayer.Pause();
            playBtn.Click -= Pause;
            playBtn.Click += Start;
            playBtn.SetImageResource(Resource.Drawable.ic_media_play);
        }

        void Play(string id)
        {
            FileManager.SongsListUpdate(id);
            mediaPlayer.Start(id);
            TogglePlay();
        }

        protected override void OnResume()
        {
            FileManager.SongsListUpdate();
            UpdateList();

            if (mediaPlayer.IsPlaying)
            {
                TogglePlay();
            }
            else
            {
                TogglePause();
            }

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
                    catch
                    {
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
                    CreateSeekBar();

                    seekThread = new Thread(new ThreadStart(UpdateSongTime));
                    seekThread.Start();

                    alertDialogBuilder.SetView(seekBar);

                    alertDialogBuilder.SetCancelable(false);
                    alertDialogBuilder.SetPositiveButton("OK", (s, e) =>
                    {
                        seekThread.Abort();
                    });

                    alertDialogBuilder.SetTitle(GetSelectedSongTitle());
                    
                    Drawable picture = SongMetadata.GetSongPicture(selectedSong.Id);

                    if (picture != null)
                    {
                        alertDialogBuilder.SetIcon(picture);
                    }

                    seekbarDialog = alertDialogBuilder.Create();
                    seekbarDialog.Show();

                    return true;

                default:
                    return base.OnContextItemSelected(item);
            }
        }

        void CreateSeekBar()
        {
            seekBar = new SeekBar(this);
            seekBar.Max = mediaPlayer.Duration;
            seekBar.Progress = mediaPlayer.CurrentPosition;

            seekBar.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    mediaPlayer.SeekTo(e.Progress);
                }
            };
        }

        string GetSelectedSongTitle()
        {
            MediaMetadataRetriever metadata = SongMetadata.GetMetadata(selectedSong.Id);
            string title = metadata.ExtractMetadata(MetadataKey.Title);

            if (title == null && !title.Equals(string.Empty))
            {
                title = selectedSong.Name;
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
                catch
                {
                    Toast.MakeText(Application.Context, "Could not rename the song", ToastLength.Long).Show();
                }
            });

            renameDialog.Show();
        }

        void UpdateSongTime()
        {
            while (mediaPlayer.IsPlaying)
            {
                seekBar.Progress = mediaPlayer.CurrentPosition;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.main_menu, menu);
            menu.FindItem(Resource.Id.mySong).SetVisible(false);

            menu.FindItem(Resource.Id.currentSong).SetVisible(true);

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

                    return true;

                case Resource.Id.currentSong:
                    if (mediaPlayer.CurrentSong != null)
                    {
                        intent = new Intent(this, typeof(CurrentSong));
                        StartActivity(intent);
                    }
                    else
                    {
                        Toast.MakeText(this, "No song is playing", ToastLength.Long).Show();
                    }
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}