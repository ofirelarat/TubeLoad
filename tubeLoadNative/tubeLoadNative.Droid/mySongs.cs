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

namespace tubeLoadNative.Droid
{
    [Activity(Label = "TubeLoad")]
    public class mySongs : Activity
    {
        private ListView songsListView;
        private List<Song> songs;
        ImageButton playBtn;

        SeekBar seekBar = null;
        AlertDialog myAlertSeekBar;
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

            string videoId = Intent.GetStringExtra("videoId");

            if (videoId != null)
            {
                Play(videoId);
            }

            UpdateList();

            songsListView.ItemClick += (object sender, Android.Widget.AdapterView.ItemClickEventArgs e) =>
            {
                Play(songs[e.Position].Id);
            };

            RegisterForContextMenu(songsListView);


            SongsMediaPlayer.mediaPlayer.Completion += delegate
            {
                if (myAlertSeekBar != null)
                {
                    myAlertSeekBar.Cancel(); 
                }

                seekThread.Abort();
                SongsHandler.PlayNext();
            };

            if (SongsMediaPlayer.IsPlaying())
            {
                playBtn.Click += Start;
            }
            else
            {
                playBtn.Click += Stop;
            }

            nextBtn.Click += delegate
            {
                SongsHandler.PlayNext();
            };

            prevBtn.Click += delegate
            {
                SongsHandler.PlayPrev();
            };
        }

        private void UpdateList()
        {
            songs = FileHandler.ReadFile();

            if (songs.Count > 0)
            {
                ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, songs.Select((x) => x.Name).ToArray());
                songsListView.Adapter = adapter;
            }
        }

        private void Start(object sender, EventArgs e)
        {
            SongsMediaPlayer.Start();
            playBtn.Click -= Start;
            playBtn.Click += Stop;
            playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
        }

        private void Stop(object sender, EventArgs e)
        {
            SongsMediaPlayer.Stop();
            playBtn.Click -= Stop;
            playBtn.Click += Start;
            playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_play));
        }

        private void Play(string id)
        {
            SongsHandler.Play(id);
            playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
        }

        private Drawable GetSongPicture(string id)
        {
            MediaMetadataRetriever metadata = SongsHandler.GetMetadata(id);
            byte[] pictureByteArray = metadata.GetEmbeddedPicture();

            if (pictureByteArray != null)
            {
                return new BitmapDrawable(Resources, BitmapFactory.DecodeByteArray(pictureByteArray, 0, pictureByteArray.Length));
            }
            else
            {
                return null;
            }
        }

        public override void OnCreateContextMenu(IContextMenu menu, View view, IContextMenuContextMenuInfo menuInfo)
        {
            if (view.Id == Resource.Id.songsListView)
            {
                var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
                selectedSong = songs[info.Position];
                menu.SetHeaderTitle(selectedSong.Name);
                MediaMetadataRetriever metadata = SongsHandler.GetMetadata(selectedSong.Id);

                Drawable picture = GetSongPicture(selectedSong.Id);

                if (picture != null)
                {
                    menu.SetHeaderIcon(picture);
                }

                var inflater = MenuInflater;
                inflater.Inflate(Resource.Menu.popup_menu, menu);

                if (SongsMediaPlayer.IsPlaying() && info.Position == SongsHandler.position)
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
                    SongsHandler.DeleteSong(selectedSong.Id);
                    UpdateList();

                    return true;

                case Resource.Id.item_play:
                    Play(selectedSong.Id);

                    return true;

                case Resource.Id.item_rename:
                    AlertDialog.Builder alertRename = new AlertDialog.Builder(this);
                    EditText edittext = new EditText(this);
                    edittext.Text = selectedSong.Name;
                    alertRename.SetTitle("Rename");
                    alertRename.SetView(edittext);

                    alertRename.SetPositiveButton("ok", (s, e) =>
                    {
                        SongsHandler.RenameSong(selectedSong.Id, edittext.Text);

                        UpdateList();
                    });

                    alertRename.Show();

                    return true;

                case Resource.Id.seek_bar:
                    AlertDialog.Builder alertSeekBar = new AlertDialog.Builder(this);
                    seekBar = new SeekBar(this);
                    seekBar.Max = SongsMediaPlayer.mediaPlayer.Duration;
                    seekBar.Progress = SongsMediaPlayer.mediaPlayer.CurrentPosition;

                    seekBar.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
                    {
                        if (e.FromUser)
                        {
                            SongsMediaPlayer.SeekTo(e.Progress);
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

                    Drawable picture = GetSongPicture(selectedSong.Id);

                    if (picture != null)
                    {
                        alertSeekBar.SetIcon(picture);
                    }

                    myAlertSeekBar = alertSeekBar.Create();
                    myAlertSeekBar.Show();

                    return true;

                default:
                    return base.OnContextItemSelected(item);
            }
        }

        private void UpdateSongTime()
        {
            while (true)
            {
                if (SongsMediaPlayer.IsPlaying())
                {
                    seekBar.Progress = SongsMediaPlayer.mediaPlayer.CurrentPosition;
                }
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_details, menu);
            menu.FindItem(Resource.Id.mySong).SetVisible(false);

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
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}