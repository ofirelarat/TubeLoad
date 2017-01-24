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
using Android.Text;

namespace tubeLoadNative.Droid
{
    [Activity(Label = "TubeLoad",LaunchMode = Android.Content.PM.LaunchMode.SingleInstance)]
    public class mySongs : Activity
    {
        private ListView songsListView;
        private List<Song> songs;
        ImageButton playBtn;

        SeekBar seekBar;
        AlertDialog seekbarDialog;
        Thread seekThread;
        Song selectedSong;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.my_songs);

            songsListView = FindViewById<ListView>(Resource.Id.songsListView);
            playBtn = FindViewById<ImageButton>(Resource.Id.playBtn);
            ImageButton nextBtn = FindViewById<ImageButton>(Resource.Id.nextBtn);
            ImageButton prevBtn = FindViewById<ImageButton>(Resource.Id.prevBtn);

            playBtn.SetBackgroundColor(Color.Rgb(41, 128, 185));
            nextBtn.SetBackgroundColor(Color.Rgb(41, 128, 185));
            prevBtn.SetBackgroundColor(Color.Rgb(41, 128, 185));

            SongsHandler.CheckFilesExist();
            UpdateList();

            songsListView.ItemClick += (object sender, Android.Widget.AdapterView.ItemClickEventArgs e) =>
            {
                Play(songs[e.Position].Id);
            };

            RegisterForContextMenu(songsListView);


            SongsHandler.OnComplete += delegate
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
               
                SongsHandler.PlayNext();
            };

            SongsHandler.OnSongSaved += (sender, e) => UpdateList();

            SongsHandler.OnSongPlayedSetBackground += delegate
            {
                int index = songsListView.FirstVisiblePosition;
                View v = songsListView.GetChildAt(0);
                int top = (v == null) ? 0 : v.Top - songsListView.ListPaddingTop;
                UpdateList();
                songsListView.SetSelectionFromTop(index,top);
            }; 

            if (SongsHandler.IsPlaying)
            {
                playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
                playBtn.Click += Pause;
            }
            else
            {
                playBtn.Click += Start;
            }
   

            nextBtn.Click += delegate
            {
                if (SongsHandler.PlayNext())
                {
                    playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
                    playBtn.Click -= Start;
                    playBtn.Click += Pause;
                }
            };

            prevBtn.Click += delegate
            {
                if (SongsHandler.PlayPrev())
                {
                    playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
                    playBtn.Click -= Start;
                    playBtn.Click += Pause;
                }
            };
        }

        private void UpdateList()
        {
            songs = SongsHandler.Songs;

            if (songs.Count > 0)
            {
                //ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, songs.Select((x) => x.Name.Replace(".mp3",string.Empty)).ToArray());
                BaseAdapter adapter = new SongsAdapter(this, songs.Select((x) => x.Name.Replace(".mp3", string.Empty)).ToArray());
                songsListView.Adapter = adapter;
            }
        }

        private void Start(object sender, EventArgs e)
        {
            if (SongsHandler.Start())
            {
                playBtn.Click -= Start;
                playBtn.Click += Pause;
                playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
            }
        }

        private void Pause(object sender, EventArgs e)
        {
            SongsHandler.Pause();
            playBtn.Click -= Pause;
            playBtn.Click += Start;
            playBtn.SetImageResource(Resource.Drawable.ic_media_play);
        }

        private void Play(string id)
        {
            SongsHandler.CheckFileExist(id);
            SongsHandler.Play(id);
            playBtn.Click -= Start;
            playBtn.Click += Pause;
            playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
        }

        protected override void OnResume()
        {
            SongsHandler.CheckFilesExist();
            UpdateList();

            if (SongsHandler.IsPlaying)
            {
                playBtn.SetImageResource(Resource.Drawable.ic_media_pause);
                playBtn.Click += Pause;
            }
            else
            {
                playBtn.Click += Start;
            }

            base.OnResume();
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if (v.Id == Resource.Id.songsListView)
            {
                var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
                selectedSong = songs[info.Position];
                
                MediaMetadataRetriever metadata = SongsHandler.GetMetadata(selectedSong.Id);
                string title = metadata.ExtractMetadata(MetadataKey.Title);
                if (title == null)
                {
                    title = selectedSong.Name;
                }

                menu.SetHeaderTitle(title);

                Drawable picture = SongsHandler.GetSongPicture(selectedSong.Id);

                if (picture != null)
                {
                    menu.SetHeaderIcon(picture);
                }

                var inflater = MenuInflater;
                inflater.Inflate(Resource.Menu.popup_menu, menu);

                if (SongsHandler.CurrentSong != null && SongsHandler.IsPlaying && selectedSong.Id == SongsHandler.CurrentSong.Id)
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
                        SongsHandler.DeleteSong(selectedSong.Id);
                        UpdateList();
                    }
                    catch
                    {
                        Toast.MakeText(Application.Context, "could not delete this song", ToastLength.Long).Show();
                    }

                    return true;

                case Resource.Id.item_play:
                    Play(selectedSong.Id);
                    Intent intent = new Intent(this, typeof(CurrentSongActivity));
                    StartActivity(intent);

                    return true;

                case Resource.Id.item_rename:
                    AlertDialog.Builder alertRename = new AlertDialog.Builder(this);
                    EditText edittext = new EditText(this);
                    edittext.Text = selectedSong.Name.Replace(".mp3","");
                    edittext.SetSingleLine();
                    alertRename.SetTitle("Rename");
                    alertRename.SetView(edittext);

                    alertRename.SetPositiveButton("ok", (s, e) =>
                    {
                        try
                        {
                            if (edittext.Text != string.Empty && FileHandler.FindSong(edittext.Text) == null && !File.Exists(FileHandler.PATH + edittext.Text) && edittext.Text.Length <= 100)
                            {
                                SongsHandler.RenameSong(selectedSong.Id, edittext.Text);
                                UpdateList();
                            }
                            else
                            {
                                Toast.MakeText(Application.Context, "not valid name", ToastLength.Long).Show();
                            }
                        }
                        catch
                        {
                            Toast.MakeText(Application.Context, "could not rename this song", ToastLength.Long).Show();
                        }
                    });

                    alertRename.Show();

                    return true;

                case Resource.Id.seek_bar:
                    AlertDialog.Builder alertSeekBar = new AlertDialog.Builder(this);
                    seekBar = new SeekBar(this);
                    seekBar.Max = SongsHandler.Duration;
                    seekBar.Progress = SongsHandler.CurrentPosition;

                    seekBar.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
                    {
                        if (e.FromUser)
                        {
                            SongsHandler.SeekTo(e.Progress);
                        }
                    };

                    seekThread = new Thread(new ThreadStart(UpdateSongTime));
                    seekThread.Start();

                    alertSeekBar.SetTitle(selectedSong.Name);
                    alertSeekBar.SetView(seekBar);

                    alertSeekBar.SetCancelable(false);
                    alertSeekBar.SetPositiveButton("ok", (s, e) =>
                  {
                      seekThread.Abort();
                  });


                    MediaMetadataRetriever metadata = SongsHandler.GetMetadata(selectedSong.Id);
                    string title = metadata.ExtractMetadata(MetadataKey.Title);
                    if (title == null)
                    {
                        title = selectedSong.Name;
                    }

                    alertSeekBar.SetTitle(title);

                    Drawable picture = SongsHandler.GetSongPicture(selectedSong.Id);

                    if (picture != null)
                    {
                        alertSeekBar.SetIcon(picture);
                    }

                    seekbarDialog = alertSeekBar.Create();
                    seekbarDialog.Show();

                    return true;

                default:
                    return base.OnContextItemSelected(item);
            }
        }

        private void UpdateSongTime()
        {
            while (true)
            {
                if (SongsHandler.IsPlaying)
                {
                    seekBar.Progress = SongsHandler.CurrentPosition;
                }
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_details, menu);
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
                    intent = new Intent(this, typeof(MainActivity));
                    StartActivity(intent);

                    return true;

                case Resource.Id.currentSong:
                    if (SongsHandler.CurrentSong != null)
                    {
                        intent = new Intent(this, typeof(CurrentSongActivity));
                        StartActivity(intent);
                    }
                    else
                    {
                        Toast.MakeText(this, "first play song", ToastLength.Long).Show();
                    }
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}