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
using Java.IO;
using Android.Graphics.Drawables;
using Android.Graphics;
using System.Threading;

namespace tubeLoadNative.Droid
{
    [Activity(Label = "TubeLoad",LaunchMode = Android.Content.PM.LaunchMode.SingleInstance)]
    public class mySongs : Activity
    {
        public static MediaPlayer mediaPlayer = new MediaPlayer();
        private AudioManager myAoudioManager;
        private ListView songsListView;
        private File path;
        private static int pos = -1;
        private Notification notification;
        private const int notificationId = 0;
        private static NotificationManager notificationManager;
        Notification.Builder builder;
        private List<File> mySongsFiles;
        File SelectedSong;
        ImageButton playBtn;
        MediaMetadataRetriever mmr;

        SeekBar seekBar = null;
        AlertDialog myAlertSeekBar;
        Thread seekThread;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            SetContentView(Resource.Layout.my_songs);

            Java.IO.File sdCard = Android.OS.Environment.ExternalStorageDirectory;
            path = new Java.IO.File(sdCard.AbsolutePath + "/TubeLoad");

            songsListView = FindViewById<ListView>(Resource.Id.songsListView);
            playBtn = FindViewById<ImageButton>(Resource.Id.playBtn);
            ImageButton nextBtn = FindViewById<ImageButton>(Resource.Id.nextBtn);
            ImageButton prevBtn = FindViewById<ImageButton>(Resource.Id.prevBtn);

            if (mediaPlayer.IsPlaying)
            {
                playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
            }       

            mmr = new MediaMetadataRetriever();

            Intent notificationIntent = new Intent(this, typeof(mySongs));
            var pendingIntentClick = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
            Intent notificationStopIntent = new Intent(this, typeof(StopBtn));
            var pendingIntentStopClick = PendingIntent.GetBroadcast(this, 0, notificationStopIntent, PendingIntentFlags.UpdateCurrent);
            Notification.Action action = new Notification.Action.Builder(Resource.Drawable.ic_media_stop, "stop", pendingIntentStopClick).Build();
            builder = new Notification.Builder(this)
            .SetAutoCancel(false)
            .SetContentIntent(pendingIntentClick)
            .SetSmallIcon(Resource.Drawable.icon)
            .SetContentTitle("TubeLoad")
            .AddAction(action);

            notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            string inputData = Intent.GetStringExtra("selectedVideo") ?? "Data not available";
            if (!inputData.Equals("Data not available"))
            {
                mediaPlayer.Reset();
                try
                {
                    mediaPlayer.SetDataSource(inputData);
                    mediaPlayer.Prepare();
                    mediaPlayer.Start();

                    mmr.SetDataSource(inputData);
                    string title = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyTitle);
                    string artist = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyArtist);

                    if (title == null || artist == null)
                    {
                        builder.SetContentTitle("TubeLoad");
                        string[] arr = inputData.Split('/');
                        builder.SetContentText(arr[arr.Length - 1]);
                    }
                    else
                    {
                        builder.SetContentTitle(title);
                        builder.SetContentText(artist);
                    }
                    notification = builder.Build();
                    notificationManager.Notify(notificationId, notification);

                    playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
                }
                catch { }
            }

            mySongsFiles = findSongs(path);

            List<string> songsNames = new List<string>();
            if (mySongsFiles.Count > 0)
            {
                foreach (File song in mySongsFiles)
                {
                    songsNames.Add(song.Name);
                }
                ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, songsNames);
                songsListView.Adapter = adapter;
            }
            //songsListView.SetAdapter(new CostumAdapterSong(this, songsNames.ToArray()));

            songsListView.ItemClick += (object sender, Android.Widget.AdapterView.ItemClickEventArgs e) =>
            {
                mediaPlayer.Reset();
                pos = e.Position;
                try
                {
                    mediaPlayer.SetDataSource(mySongsFiles[pos].ToString());
                    mediaPlayer.Prepare();
                    mediaPlayer.Start();

                    mmr.SetDataSource(mySongsFiles[pos].Path);
                    string title = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyTitle);
                    string artist = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyArtist);

                    if (title == null || artist == null)
                    {
                        builder.SetContentTitle("TubeLoad");
                        builder.SetContentText(songsNames[pos]);
                    }
                    else
                    {
                        builder.SetContentTitle(title);
                        builder.SetContentText(artist);
                    }
                    notification = builder.Build();
                    notificationManager.Notify(notificationId, notification);

                    playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
                }
                catch (Exception ex) { Toast.MakeText(this, "cannot open this file - " + ex.Message, ToastLength.Long).Show(); }
            };

            RegisterForContextMenu(songsListView);
            /*
            songsListView.ItemLongClick += (s, e) =>
             {
                 PopupMenu popupMenu = new PopupMenu(this, songsListView);                
                 popupMenu.MenuItemClick += (s1, arg1) =>
                 {
                     switch (arg1.Item.ItemId)
                     {
                         case Resource.Id.item_delete:
                             mySongsFiles[e.Position].Delete();

                             mySongsFiles = findSongs(path);
                             songsNames = new List<string>();
                             if (mySongsFiles.Count > 0)
                             {
                                 foreach (File song in mySongsFiles)
                                 {
                                     songsNames.Add(song.Name);
                                 }
                                 ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, songsNames);
                                 songsListView.Adapter = adapter;
                             }
                             break;

                         case Resource.Id.item_play:
                             mediaPlayer.Reset();
                             mediaPlayer = new MediaPlayer();
                             pos = e.Position;
                             mediaPlayer.SetDataSource(mySongsFiles[e.Position].ToString());
                             mediaPlayer.Prepare();
                             mediaPlayer.Start();

                             builder.SetContentText(songsNames[e.Position]);
                             notification = builder.Build();
                             notificationManager.Notify(notificationId, notification);
                             break;
                     }
                 };
                 popupMenu.Inflate(Resource.Menu.popup_menu);
                 popupMenu.Show();
             };
             */

            mediaPlayer.Completion += delegate
            {
                myAlertSeekBar.Cancel();
                seekThread.Abort();

                if (pos < mySongsFiles.Count - 1 && pos != -1)
                {
                    mediaPlayer.Reset();
                    mediaPlayer.SetDataSource(mySongsFiles[++pos].ToString());
                    mediaPlayer.Prepare();
                    mediaPlayer.Start();

                    mmr.SetDataSource(mySongsFiles[pos].Path);
                    string title = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyTitle);
                    string artist = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyArtist);

                    if (title == null || artist == null)
                    {
                        builder.SetContentTitle("TubeLoad");
                        builder.SetContentText(songsNames[pos]);
                    }
                    else
                    {
                        builder.SetContentTitle(title);
                        builder.SetContentText(artist);
                    }
                    notification = builder.Build();
                    notificationManager.Notify(notificationId, notification);
                }
                else { pos = -1; }
            };

            playBtn.Click += delegate
            {
                try
                {
                    if (!mediaPlayer.IsPlaying)
                    {
                        if (pos == -1)
                        {
                            pos = 0;
                            mediaPlayer.Reset();
                            mediaPlayer.SetDataSource(mySongsFiles[pos].ToString());
                            mediaPlayer.Prepare();
                        }
                        mediaPlayer.Start();

                        mmr.SetDataSource(mySongsFiles[pos].Path);
                        string title = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyTitle);
                        string artist = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyArtist);

                        if (title == null || artist == null)
                        {
                            builder.SetContentTitle("TubeLoad");
                            builder.SetContentText(songsNames[pos]);
                        }
                        else
                        {
                            builder.SetContentTitle(title);
                            builder.SetContentText(artist);
                        }
                        notification = builder.Build();
                        notificationManager.Notify(notificationId, notification);

                        playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
                    }
                    else
                    {
                        mediaPlayer.Pause();
                        playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_play));
                    }
                }
                catch { throw; }
            };

            nextBtn.Click += delegate
            {
                pos = pos == mySongsFiles.Count - 1 ? 0 : ++pos;
                mediaPlayer.Reset();
                mediaPlayer.SetDataSource(mySongsFiles[pos].ToString());
                mediaPlayer.Prepare();
                mediaPlayer.Start();

                mmr.SetDataSource(mySongsFiles[pos].Path);
                string title = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyTitle);
                string artist = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyArtist);

                if (title == null || artist == null)
                {
                    builder.SetContentTitle("TubeLoad");
                    builder.SetContentText(songsNames[pos]);
                }
                else
                {
                    builder.SetContentTitle(title);
                    builder.SetContentText(artist);
                }
                notification = builder.Build();
                notificationManager.Notify(notificationId, notification);

                playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
            };

            prevBtn.Click += delegate
            {
                pos = pos <= 0 ? mySongsFiles.Count -1 : --pos;
                mediaPlayer.Reset();
                mediaPlayer.SetDataSource(mySongsFiles[pos].ToString());
                mediaPlayer.Prepare();
                mediaPlayer.Start();

                mmr.SetDataSource(mySongsFiles[pos].Path);
                string title = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyTitle);
                string artist = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyArtist);

                if (title == null || artist == null)
                {
                    builder.SetContentTitle("TubeLoad");
                    builder.SetContentText(songsNames[pos]);
                }
                else
                {
                    builder.SetContentTitle(title);
                    builder.SetContentText(artist);
                }
                notification = builder.Build();
                notificationManager.Notify(notificationId, notification);

                playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
            };
}

        private List<File> findSongs(File root)
        {
            List<File> al = new List<File>();
            File[] files = root.ListFiles();
            foreach (File singleFile in files)
            {
                if (singleFile.IsDirectory && !singleFile.IsHidden)
                {
                    al.AddRange(findSongs(singleFile));
                }
                else
                {
                    if (singleFile.Name.EndsWith(".mp3") || singleFile.Name.EndsWith(".wav"))
                    {
                        al.Add(singleFile);
                    }
                }
            }

            return al;
        }

        public class StopBtn : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                mediaPlayer.Stop();
                notificationManager.Cancel(notificationId);
                throw new NotImplementedException();
            }
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if (v.Id == Resource.Id.songsListView)
            {
                var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
                menu.SetHeaderTitle(mySongsFiles[info.Position].Name);
                SelectedSong = mySongsFiles[info.Position];
                mmr.SetDataSource(SelectedSong.Path);
                byte[] art = mmr.GetEmbeddedPicture();
                if (art != null)
                {
                    Drawable d = new BitmapDrawable(Resources, BitmapFactory.DecodeByteArray(art, 0, art.Length));
                    menu.SetHeaderIcon(d);
                }

                var inflater = MenuInflater;
                inflater.Inflate(Resource.Menu.popup_menu, menu);

                if (!(mediaPlayer.IsPlaying && pos >- 1 && mySongsFiles[pos].Path.Equals(SelectedSong.Path)))
                {
                    menu.FindItem(Resource.Id.seek_bar).SetVisible(false);
                }
            }
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.item_delete:
                    SelectedSong.Delete();

                    mySongsFiles = findSongs(path);
                    List<string> songsNames = new List<string>();
                    if (mySongsFiles.Count > 0)
                    {
                        foreach (File song in mySongsFiles)
                        {
                            songsNames.Add(song.Name);
                        }
                        ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, songsNames);
                        songsListView.Adapter = adapter;
                    }
                    return true;

                case Resource.Id.item_play:
                    mediaPlayer.Reset();
                    mediaPlayer = new MediaPlayer();
                    pos = -1;
                    mediaPlayer.SetDataSource(SelectedSong.ToString());
                    mediaPlayer.Prepare();
                    mediaPlayer.Start();

                    mmr.SetDataSource(SelectedSong.Path);
                    string title = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyTitle);
                    string artist = mmr.ExtractMetadata(MediaMetadataRetriever.MetadataKeyArtist);

                    if (title == null || artist == null)
                    {
                        builder.SetContentTitle("TubeLoad");
                        builder.SetContentText(SelectedSong.Name);
                    }
                    else
                    {
                        builder.SetContentTitle(title);
                        builder.SetContentText(artist);
                    }
                    notification = builder.Build();
                    notificationManager.Notify(notificationId, notification);

                    playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.ic_media_pause));
                    return true;

                case Resource.Id.item_rename:
                    AlertDialog.Builder alertRename = new AlertDialog.Builder(this);
                    EditText edittext = new EditText(this);
                    edittext.Text = SelectedSong.Name;
                    alertRename.SetTitle("Rename");
                    alertRename.SetView(edittext);
                    alertRename.SetPositiveButton("ok", (s, e) =>
                    {
                        string newPath = SelectedSong.Path.Replace(SelectedSong.Name, edittext.Text);
                        //if(SelectedSong.RenameTo(new File(newPath)))
                        //{
                        //    FilesHandler.WriteToJsonFile(FilesHandler.ID_FILE, itemSelected.Id.VideoId, FileName);
                        //}

                        mySongsFiles = findSongs(path);
                        songsNames = new List<string>();
                        if (mySongsFiles.Count > 0)
                        {
                            foreach (File song in mySongsFiles)
                            {
                                songsNames.Add(song.Name);
                            }
                            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, songsNames);
                            songsListView.Adapter = adapter;
                        }

                    });
                    alertRename.Show();
                    return true;

                case Resource.Id.seek_bar:
                    AlertDialog.Builder alertSeekBar = new AlertDialog.Builder(this);
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

                    seekThread = new Thread(new ThreadStart(UpdateSongTime));
                    seekThread.Start();

                    alertSeekBar.SetTitle(SelectedSong.Name);
                    alertSeekBar.SetView(seekBar);
                    
                    alertSeekBar.SetCancelable(false);
                    alertSeekBar.SetPositiveButton("ok",   (s, e) => 
                    {
                        seekThread.Abort();
                    });

                    mmr.SetDataSource(SelectedSong.Path);
                    byte[] art = mmr.GetEmbeddedPicture();
                    if (art != null)
                    {
                        Drawable d = new BitmapDrawable(Resources, BitmapFactory.DecodeByteArray(art, 0, art.Length));
                        alertSeekBar.SetIcon(d);
                    }

                    myAlertSeekBar = alertSeekBar.Create();
                    myAlertSeekBar.Show();     
                    return true;

                default:
                    return base.OnContextItemSelected(item);
            }
        }

        public void UpdateSongTime()
        {
            while (true)
            {
                if (seekBar != null && mediaPlayer != null)
                {
                    int startTime = mediaPlayer.CurrentPosition;
                    seekBar.Progress = startTime;
                }
            }
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
                    intent = new Intent(this, typeof(MainActivity));
                    StartActivity(intent);
                    return true;

                case Resource.Id.mySong:
                    //intent = new Intent(this, typeof(mySongs));
                    //StartActivity(intent);
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}