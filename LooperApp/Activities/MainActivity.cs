using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Java.IO;
using Java.Lang;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using Xamarin.Essentials;

namespace LooperApp
{
    [Activity(Label = "Looper", Theme = "@style/AppTheme.NoActionBar", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : AppCompatActivity, SeekBar.IOnSeekBarChangeListener
    {
        private static readonly int REQUEST_PATH = 1;
        private static readonly int REQUEST_PERMISSIONS = 200;
        private static readonly int REQUEST_SETLIST = 2;
        private static readonly int START_SAMPLER = 3;
        private static string mFileName = null;
        private readonly string[] permissions = { Android.Manifest.Permission.RecordAudio, Android.Manifest.Permission.ReadExternalStorage, Android.Manifest.Permission.WriteExternalStorage, Android.Manifest.Permission.RecordAudio, Android.Manifest.Permission.Internet, Android.Manifest.Permission.WakeLock, Android.Manifest.Permission.AccessMediaLocation };
        private ChannelNew ch1;
        private bool Ch1Record = false;
        private ChannelNew ch2;
        private bool Ch2Record = false;
        private ChannelNew ch3;
        private bool Ch3Record = false;
        private ChannelNew ch4;
        private bool Ch4Record = false;
        private ChannelNew ch5;
        private bool Ch5Record = false;
        private SeekBar channelPitch;
        private SeekBar channelSpeed;
        private ChannelNew chd;
        private bool ChDRecord = false;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private List<string> FilesToDelete = new List<string>();
        private List<float> m_pitchListToSave = new List<float>();
        private List<float> m_speedListToSave = new List<float>();
        private List<string> m_trackListToSave = new List<string>();
        private List<float> m_volumeListToSave = new List<float>();
        private int mActiveChannel = 1;
        private MediaRecorder mRecorder = null;
        private ProgressBar pb1;
        private ProgressBar pb2;
        private ProgressBar pb3;
        private ProgressBar pb4;
        private ProgressBar pb5;
        private ProgressBar pbd;
        private bool PlayAllEnabled = false;
        private bool recording = false;
        private int selectedChannel = 0;
        private VerticalSeekbar volume1;
        private VerticalSeekbar volume2;
        private VerticalSeekbar volume3;
        private VerticalSeekbar volume4;
        private VerticalSeekbar volume5;
        private VerticalSeekbar volumeD;
        private bool m_foldersExisting = false;

        /// <summary>
        /// Change channel.
        /// </summary>
        /// <exception cref="RuntimeException">Thrown when a Runtime error condition occurs.</exception>
        /// <param name="view">The view.</param>
        [Export("ChangeChannel")]
        public void ChangeChannel(View view)
        {
            ((ImageView)FindViewById(Resource.Id.CH1_Number)).SetImageResource(Resource.Drawable.Ch1_green);
            ((ImageView)FindViewById(Resource.Id.CH2_Number)).SetImageResource(Resource.Drawable.Ch2_green);
            ((ImageView)FindViewById(Resource.Id.CH3_Number)).SetImageResource(Resource.Drawable.Ch3_green);
            ((ImageView)FindViewById(Resource.Id.CH4_Number)).SetImageResource(Resource.Drawable.Ch4_green);
            ((ImageView)FindViewById(Resource.Id.CH5_Number)).SetImageResource(Resource.Drawable.Ch5_green);
            ((ImageView)FindViewById(Resource.Id.CHD_Number)).SetImageResource(Resource.Drawable.ChD_green);

            ChannelNew tempCh;

            switch (view.Id)
            {
                case Resource.Id.Sel1:
                    {
                        mActiveChannel = 1;
                        ((ImageView)FindViewById(Resource.Id.CH1_Number)).SetImageResource(Resource.Drawable.Ch1_red);
                        tempCh = ch1;
                    }
                    break;

                case Resource.Id.Sel2:
                    {
                        mActiveChannel = 2;
                        ((ImageView)FindViewById(Resource.Id.CH2_Number)).SetImageResource(Resource.Drawable.Ch2_red);
                        tempCh = ch2;
                    }
                    break;

                case Resource.Id.Sel3:
                    {
                        mActiveChannel = 3;
                        ((ImageView)FindViewById(Resource.Id.CH3_Number)).SetImageResource(Resource.Drawable.Ch3_red);
                        tempCh = ch3;
                    }
                    break;

                case Resource.Id.Sel4:
                    {
                        mActiveChannel = 4;
                        ((ImageView)FindViewById(Resource.Id.CH4_Number)).SetImageResource(Resource.Drawable.Ch4_red);
                        tempCh = ch4;
                    }
                    break;

                case Resource.Id.Sel5:
                    {
                        mActiveChannel = 5;
                        ((ImageView)FindViewById(Resource.Id.CH5_Number)).SetImageResource(Resource.Drawable.Ch5_red);
                        tempCh = ch5;
                    }
                    break;

                case Resource.Id.Sel6:
                    {
                        mActiveChannel = 6;
                        ((ImageView)FindViewById(Resource.Id.CHD_Number)).SetImageResource(Resource.Drawable.ChD_red);
                        tempCh = chd;
                    }
                    break;

                default:
                    throw new RuntimeException("Unknow button ID");
            }

            channelSpeed.Progress = (int)(tempCh.Speed * 100);
            channelPitch.Progress = (int)(tempCh.Pitch * 100);
        }

        /// <summary>
        /// Clears the channel described by view.
        /// </summary>
        /// <param name="view">The view.</param>
        [Export("ClearChannel")]
        public void ClearChannel(View view)
        {
            ChannelNew tempCh;
            bool rec = false;

            switch (view.Id)
            {
                case Resource.Id.CH1_Rec:
                    tempCh = ch1;
                    rec = Ch1Record;
                    break;

                case Resource.Id.CH2_Rec:
                    tempCh = ch2;
                    rec = Ch2Record;
                    break;

                case Resource.Id.CH3_Rec:
                    tempCh = ch3;
                    rec = Ch3Record;
                    break;

                case Resource.Id.CH4_Rec:
                    tempCh = ch4;
                    rec = Ch4Record;
                    break;

                case Resource.Id.CH5_Rec:
                    tempCh = ch5;
                    rec = Ch5Record;
                    break;

                case Resource.Id.CHD_Rec:
                    tempCh = chd;
                    rec = ChDRecord;
                    break;

                default:
                    return;
            }

            if (tempCh == null || rec == true)
                return;

            if (tempCh.isPlaying())
                return;

            ImageView iv;

            switch (view.Id)
            {
                case Resource.Id.CH1_Rec:
                    ch1 = new ChannelNew(1);
                    volume1.Progress = 100;
                    pb1.Progress = 0;
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh1);
                    iv.SetImageResource(Resource.Drawable.WhiteLED);
                    break;

                case Resource.Id.CH2_Rec:
                    ch2 = new ChannelNew(2);
                    volume2.Progress = 100;
                    pb2.Progress = 0;
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh2);
                    iv.SetImageResource(Resource.Drawable.WhiteLED);
                    break;

                case Resource.Id.CH3_Rec:
                    ch3 = new ChannelNew(3);
                    volume3.Progress = 100;
                    pb3.Progress = 0;
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh3);
                    iv.SetImageResource(Resource.Drawable.WhiteLED);
                    break;

                case Resource.Id.CH4_Rec:
                    ch4 = new ChannelNew(4);
                    volume4.Progress = 100;
                    pb4.Progress = 0;
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh4);
                    iv.SetImageResource(Resource.Drawable.WhiteLED);
                    break;

                case Resource.Id.CH5_Rec:
                    ch5 = new ChannelNew(5);
                    volume5.Progress = 100;
                    pb5.Progress = 0;
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh5);
                    iv.SetImageResource(Resource.Drawable.WhiteLED);
                    break;

                case Resource.Id.CHD_Rec:
                    chd = new ChannelNew(6);
                    volumeD.Progress = 100;
                    pbd.Progress = 0;
                    iv = (ImageView)FindViewById(Resource.Id.LoadedChD);
                    iv.SetImageResource(Resource.Drawable.WhiteLED);
                    break;

                default:
                    return;
            }
        }

        /// <summary>
        /// Clears the channel by number described by nbr.
        /// </summary>
        /// <param name="nbr">Number of.</param>
        public void ClearChannelByNbr(int nbr)
        {
            ChannelNew tempCh;
            bool rec = false;

            switch (nbr)
            {
                case 1:
                    tempCh = ch1;
                    rec = Ch1Record;
                    break;

                case 2:
                    tempCh = ch2;
                    rec = Ch2Record;
                    break;

                case 3:
                    tempCh = ch3;
                    rec = Ch3Record;
                    break;

                case 4:
                    tempCh = ch4;
                    rec = Ch4Record;
                    break;

                case 5:
                    tempCh = ch5;
                    rec = Ch5Record;
                    break;

                case 6:
                    tempCh = chd;
                    rec = ChDRecord;
                    break;

                default:
                    return;
            }

            if (tempCh == null || rec == true)
                return;

            if (tempCh.isPlaying())
                return;

            ImageView iv;

            switch (nbr)
            {
                case 1:
                    ch1 = new ChannelNew(1);
                    volume1.Progress = 100;
                    pb1.Progress = 0;
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh1);
                    iv.SetImageResource(Resource.Drawable.WhiteLED);
                    break;

                case 2:
                    ch2 = new ChannelNew(2);
                    volume2.Progress = 100;
                    pb2.Progress = 0;
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh2);
                    iv.SetImageResource(Resource.Drawable.WhiteLED);
                    break;

                case 3:
                    ch3 = new ChannelNew(3);
                    volume3.Progress = 100;
                    pb3.Progress = 0;
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh3);
                    iv.SetImageResource(Resource.Drawable.WhiteLED);
                    break;

                case 4:
                    ch4 = new ChannelNew(4);
                    volume4.Progress = 100;
                    pb4.Progress = 0;
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh4);
                    iv.SetImageResource(Resource.Drawable.WhiteLED);
                    break;

                case 5:
                    ch5 = new ChannelNew(5);
                    volume5.Progress = 100;
                    pb5.Progress = 0;
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh5);
                    iv.SetImageResource(Resource.Drawable.WhiteLED);
                    break;

                case 6:
                    chd = new ChannelNew(6);
                    volumeD.Progress = 100;
                    pbd.Progress = 0;
                    iv = (ImageView)FindViewById(Resource.Id.LoadedChD);
                    iv.SetImageResource(Resource.Drawable.WhiteLED);
                    break;

                default:
                    return;
            }
        }

        [Export("Getfile")]
        /// <summary>
        /// Getfiles the specified view.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <exception cref="RuntimeException">Unknow button ID</exception>
        public void Getfile(View view)
        {
            if (!m_foldersExisting)
                InitLooperFolder();

            switch (view.Id)
            {
                case Resource.Id.progressBarCh1:
                    selectedChannel = 1;
                    break;

                case Resource.Id.progressBarCh2:
                    selectedChannel = 2;
                    break;

                case Resource.Id.progressBarCh3:
                    selectedChannel = 3;
                    break;

                case Resource.Id.progressBarCh4:
                    selectedChannel = 4;
                    break;

                case Resource.Id.progressBarCh5:
                    selectedChannel = 5;
                    break;

                case Resource.Id.progressBarChD:
                    selectedChannel = 6;
                    break;

                default:
                    selectedChannel = 0;
                    break;
            }

            List<string> extList = new List<string>();
            extList.Add(".mp3");
            extList.Add(".wav");
            extList.Add(".3gp");

            Bundle extListBundle = new Bundle();
            extListBundle.PutStringArrayList("ExtList", extList);

            Bundle filePathBundle = new Bundle();
            filePathBundle.PutString("FilePath", Android.App.Application.Context.GetExternalFilesDir(null) + File.Separator + "Looper");


            Intent i = new Intent(this, typeof(FilePickerActivity));
            i.PutExtra("ExtList", extListBundle);
            i.PutExtra("FilePath", filePathBundle);

            StartActivityForResult(i, REQUEST_PATH);
        }

        /// <summary>
        /// Initialize the contents of the Activity's standard options menu.
        /// </summary>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        /// <summary>
        /// This hook is called whenever an item in your options menu is selected.
        /// </summary>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        /// <summary>
        /// Executes the progress changed action.
        /// </summary>
        /// <param name="seekBar">The seek bar.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="fromUser">True to from user.</param>
        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            int stepSize = 10;
            float volume = (float)progress / 100;

            if (seekBar.GetType() == typeof(Android.Support.V7.Widget.AppCompatSeekBar) && (Android.Support.V7.Widget.AppCompatSeekBar)seekBar == channelSpeed)
            {
                if (progress <= 0)
                    return;

                int myProgress = progress / stepSize * stepSize;

                switch (mActiveChannel)
                {
                    case 1:
                        if (ch1.trackLoaded())
                            ch1.Speed = (float)myProgress / 100;
                        else
                            seekBar.Progress = (int)(ch1.Speed * 100);
                        break;

                    case 2:
                        if (ch2.trackLoaded())
                            ch2.Speed = (float)myProgress / 100;
                        else
                            seekBar.Progress = (int)(ch2.Speed * 100);
                        break;

                    case 3:
                        if (ch3.trackLoaded())
                            ch3.Speed = (float)myProgress / 100;
                        else
                            seekBar.Progress = (int)(ch3.Speed * 100);
                        break;

                    case 4:
                        if (ch4.trackLoaded())
                            ch4.Speed = (float)myProgress / 100;
                        else
                            seekBar.Progress = (int)(ch4.Speed * 100);
                        break;

                    case 5:
                        if (ch5.trackLoaded())
                            ch5.Speed = (float)myProgress / 100;
                        else
                            seekBar.Progress = (int)(ch5.Speed * 100);
                        break;

                    case 6:
                        if (chd.trackLoaded())
                            chd.Speed = (float)myProgress / 100;
                        else
                            seekBar.Progress = (int)(chd.Speed * 100);
                        break;
                }
                return;
            }
            else if (seekBar.GetType() == typeof(Android.Support.V7.Widget.AppCompatSeekBar) && (Android.Support.V7.Widget.AppCompatSeekBar)seekBar == channelPitch)
            {
                if (progress <= 0)
                    return;

                int myProgress = progress / stepSize * stepSize;

                switch (mActiveChannel)
                {
                    case 1:
                        if (ch1.trackLoaded())
                            ch1.Pitch = (float)myProgress / 100;
                        else
                            seekBar.Progress = (int)(ch1.Pitch * 100);
                        break;

                    case 2:
                        if (ch2.trackLoaded())
                            ch2.Pitch = (float)myProgress / 100;
                        else
                            seekBar.Progress = (int)(ch2.Pitch * 100);
                        break;

                    case 3:
                        if (ch3.trackLoaded())
                            ch3.Pitch = (float)myProgress / 100;
                        else
                            seekBar.Progress = (int)(ch3.Pitch * 100);
                        break;

                    case 4:
                        if (ch4.trackLoaded())
                            ch4.Pitch = (float)myProgress / 100;
                        else
                            seekBar.Progress = (int)(ch4.Pitch * 100);
                        break;

                    case 5:
                        if (ch5.trackLoaded())
                            ch5.Pitch = (float)myProgress / 100;
                        else
                            seekBar.Progress = (int)(ch5.Pitch * 100);
                        break;

                    case 6:
                        if (chd.trackLoaded())
                            chd.Pitch = (float)myProgress / 100;
                        else
                            seekBar.Progress = (int)(chd.Pitch * 100);
                        break;
                }
                return;
            }

            if ((VerticalSeekbar)seekBar == volume1)
            {
                ch1.setVolume(volume);
            }

            if ((VerticalSeekbar)seekBar == volume2)
            {
                ch2.setVolume(volume);
            }

            if ((VerticalSeekbar)seekBar == volume3)
            {
                ch3.setVolume(volume);
            }

            if ((VerticalSeekbar)seekBar == volume4)
            {
                ch4.setVolume(volume);
            }

            if ((VerticalSeekbar)seekBar == volume5)
            {
                ch5.setVolume(volume);
            }

            if ((VerticalSeekbar)seekBar == volumeD)
            {
                chd.setVolume(volume);
            }
        }

        /// <summary>
        /// Executes the request permissions result action.
        /// </summary>
        /// <param name="requestCode">The request code.</param>
        /// <param name="permissions">The permissions.</param>
        /// <param name="grantResults">The grant results.</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /// <summary>
        /// Executes the start tracking touch action.
        /// </summary>
        /// <param name="seekBar">The seek bar.</param>
        public void OnStartTrackingTouch(SeekBar seekBar)
        {
        }

        /// <summary>
        /// Executes the stop tracking touch action.
        /// </summary>
        /// <param name="seekBar">The seek bar.</param>
        public void OnStopTrackingTouch(SeekBar seekBar)
        {
        }

        /// <summary>
        /// Opens set list.
        /// </summary>
        /// <param name="data">The data.</param>
        public void OpenSetList(Intent data)
        {
            string curFile;

            curFile = data.GetStringExtra("GetPath");

            if (curFile == null)
                return;

            if (System.IO.Path.GetExtension(curFile) != ".sll")
            {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);

                alert.SetTitle("Sorry !");
                alert.SetMessage("It's not a setlist from these app.");

                alert.SetNeutralButton("OK", HandlerCancelButton);

                Dialog alertDialog = alert.Create();
                alertDialog.Show();
                return;
            }

            string destinationFile = curFile;
            if (System.IO.File.Exists(destinationFile))
            {
                var fs = System.IO.File.OpenRead(destinationFile);
                byte[] bytes = new byte[fs.Length];
                int numBytesToRead = (int)fs.Length;

                fs.Read(bytes, 0, numBytesToRead);
                fs.Close();

                var stringList = ReadDataToString(bytes);

                // Track
                for (int i = 0; i < 6; i++)
                {
                    if (!string.IsNullOrEmpty(stringList[i]))
                    {
                        PrepareChannel(stringList[i], i + 1);
                        SetVolume(i + 1, float.Parse(stringList[i + 6]));
                    }
                    else
                        ClearChannelByNbr(i + 1);
                }
            }
        }

        [Export("Play")]
        /// <summary>
        /// Plays the specified view.
        /// </summary>
        /// <param name="view">The view.</param>
        public void Play(View view)
        {
            ChannelNew tempCh;
            ImageView iv = null;
            string channelNbr = "0";

            switch (view.Id)
            {
                case Resource.Id.CH1_Rec:
                    iv = (ImageView)FindViewById(Resource.Id.PlayingCh1);
                    if (Ch1Record)
                    {
                        StartRec(1);
                        return;
                    }
                    else
                    {
                        tempCh = ch1;
                        channelNbr = "1";
                    }
                    break;

                case Resource.Id.CH2_Rec:
                    iv = (ImageView)FindViewById(Resource.Id.PlayingCh2);
                    if (Ch2Record)
                    {
                        StartRec(2);
                        return;
                    }
                    else
                    {
                        tempCh = ch2;
                        channelNbr = "2";
                    }
                    break;

                case Resource.Id.CH3_Rec:
                    iv = (ImageView)FindViewById(Resource.Id.PlayingCh3);
                    if (Ch3Record)
                    {
                        StartRec(3);
                        return;
                    }
                    else
                    {
                        tempCh = ch3;
                        channelNbr = "3";
                    }
                    break;

                case Resource.Id.CH4_Rec:
                    iv = (ImageView)FindViewById(Resource.Id.PlayingCh4);
                    if (Ch4Record)
                    {
                        StartRec(4);
                        return;
                    }
                    else
                    {
                        tempCh = ch4;
                        channelNbr = "4";
                    }
                    break;

                case Resource.Id.CH5_Rec:
                    iv = (ImageView)FindViewById(Resource.Id.PlayingCh5);
                    if (Ch5Record)
                    {
                        StartRec(5);
                        return;
                    }
                    else
                    {
                        tempCh = ch5;
                        channelNbr = "5";
                    }
                    break;

                case Resource.Id.CHD_Rec:
                    tempCh = chd;
                    iv = (ImageView)FindViewById(Resource.Id.PlayingChD);
                    if (ChDRecord)
                    {
                        StartRec(6);
                        return;
                    }
                    else
                    {
                        tempCh = chd;
                        channelNbr = "6";
                    }
                    break;

                default:
                    return;
            }

            if (tempCh.isPlaying())
            {
                // ch1PauseTime = (int)(new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                tempCh.pauseMedia();
                iv.SetImageResource(Resource.Drawable.WhiteLED);
                return;
            }

            if (tempCh.isPause())
            {
                tempCh.resumeMedia();

                switch (channelNbr)
                {
                    case "1":
                        {
                            ch1StartTime = (double)(new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds - ch1.getCurPos();
                            ch1EndTime = ch1StartTime + ch1.getDuration();
                        }
                        break;

                    case "2":
                        {
                            ch2StartTime = (double)(new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds - ch2.getCurPos();
                            ch2EndTime = ch2StartTime + ch2.getDuration();
                        }
                        break;

                    case "3":
                        {
                            ch3StartTime = (double)(new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds - ch3.getCurPos();
                            ch3EndTime = ch3StartTime + ch3.getDuration();
                        }
                        break;

                    case "4":
                        {
                            ch4StartTime = (double)(new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds - ch4.getCurPos();
                            ch4EndTime = ch4StartTime + ch4.getDuration();
                        }
                        break;

                    case "5":
                        {
                            ch5StartTime = (double)(new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds - ch5.getCurPos();
                            ch5EndTime = ch5StartTime + ch5.getDuration();
                        }
                        break;

                    case "6":
                        {
                            chdStartTime = (double)(new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds - chd.getCurPos();
                            chdEndTime = chdStartTime + chd.getDuration();
                        }
                        break;
                }

                iv.SetImageResource(Resource.Drawable.GreenLED);
                return;
            }

            if (tempCh.trackLoaded())
            {
                switch (channelNbr)
                {
                    case "1":
                        {
                            ch1StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                            ch1EndTime = ch1StartTime + ch1.getDuration();
                        }
                        break;

                    case "2":
                        {
                            ch2StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                            ch2EndTime = ch2StartTime + ch2.getDuration();
                        }
                        break;

                    case "3":
                        {
                            ch3StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                            ch3EndTime = ch3StartTime + ch3.getDuration();
                        }
                        break;

                    case "4":
                        {
                            ch4StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                            ch4EndTime = ch4StartTime + ch4.getDuration();
                        }
                        break;

                    case "5":
                        {
                            ch5StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                            ch5EndTime = ch5StartTime + ch5.getDuration();
                        }
                        break;

                    case "6":
                        {
                            chdStartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                            chdEndTime = chdStartTime + chd.getDuration();
                        }
                        break;
                }

                tempCh.playMedia();
                iv.SetImageResource(Resource.Drawable.GreenLED);
            }

            var result = MyBackgroundTask(cts.Token);
        }

        private double ch1StartTime;
        private double ch1EndTime;
        private double ch2StartTime;
        private double ch2EndTime;
        private double ch3StartTime;
        private double ch3EndTime;
        private double ch4StartTime;
        private double ch4EndTime;
        private double ch5StartTime;
        private double ch5EndTime;
        private double chdStartTime;
        private double chdEndTime;

        /// <summary>
        /// Playalls the given view.
        /// </summary>
        /// <param name="view">The view.</param>
        [Export("Playall")]
        public void Playall(View view)
        {
            ImageView iv;

            iv = (ImageView)FindViewById(Resource.Id.playAll);

            if (!PlayAllEnabled)
            {
                ChannelNew tempCh = null;
                bool noTrackLoaded = true;

                for (int i = 1; i <= 6; i++)
                {
                    switch (i)
                    {
                        case 1:
                            tempCh = ch1;
                            iv = (ImageView)FindViewById(Resource.Id.PlayingCh1);
                            pb1.Progress = 0;
                            ch1StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                            ch1EndTime = ch1StartTime + ch1.getDuration();
                            break;

                        case 2:
                            tempCh = ch2;
                            iv = (ImageView)FindViewById(Resource.Id.PlayingCh2);
                            pb2.Progress = 0;
                            ch2StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                            ch2EndTime = ch2StartTime + ch2.getDuration();
                            break;

                        case 3:
                            tempCh = ch3;
                            iv = (ImageView)FindViewById(Resource.Id.PlayingCh3);
                            pb3.Progress = 0;
                            ch3StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                            ch3EndTime = ch3StartTime + ch3.getDuration();
                            break;

                        case 4:
                            tempCh = ch4;
                            iv = (ImageView)FindViewById(Resource.Id.PlayingCh4);
                            pb4.Progress = 0;
                            ch4StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                            ch4EndTime = ch4StartTime + ch4.getDuration();
                            break;

                        case 5:
                            tempCh = ch5;
                            iv = (ImageView)FindViewById(Resource.Id.PlayingCh5);
                            pb5.Progress = 0;
                            ch5StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                            ch5EndTime = ch5StartTime + ch5.getDuration();
                            break;

                        case 6:
                            tempCh = chd;
                            iv = (ImageView)FindViewById(Resource.Id.PlayingChD);
                            pbd.Progress = 0;
                            chdStartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                            chdEndTime = chdStartTime + chd.getDuration();
                            break;
                    }

                    if (tempCh != null && tempCh.trackLoaded())
                    {
                        noTrackLoaded = false;
                        tempCh.stopMedia();
                        tempCh.goToStart();
                        tempCh.playMedia();
                        iv.SetImageResource(Resource.Drawable.GreenLED);
                    }
                }

                if (noTrackLoaded)
                    return;

                iv = (ImageView)FindViewById(Resource.Id.playAll);
                iv.SetImageResource(Resource.Drawable.Button_Play_Green_svg);
                PlayAllEnabled = true;

                var result = MyBackgroundTask(cts.Token);
            }
            else
            {
                iv.SetImageResource(Resource.Drawable.Button_Play_White_svg);
                PlayAllEnabled = false;

                ChannelNew tempCh = null;

                for (int i = 1; i <= 6; i++)
                {
                    switch (i)
                    {
                        case 1:
                            tempCh = ch1;
                            iv = (ImageView)FindViewById(Resource.Id.PlayingCh1);
                            break;

                        case 2:
                            tempCh = ch2;
                            iv = (ImageView)FindViewById(Resource.Id.PlayingCh2);
                            break;

                        case 3:
                            tempCh = ch3;
                            iv = (ImageView)FindViewById(Resource.Id.PlayingCh3);
                            break;

                        case 4:
                            tempCh = ch4;
                            iv = (ImageView)FindViewById(Resource.Id.PlayingCh4);
                            break;

                        case 5:
                            tempCh = ch5;
                            iv = (ImageView)FindViewById(Resource.Id.PlayingCh5);
                            break;

                        case 6:
                            tempCh = chd;
                            iv = (ImageView)FindViewById(Resource.Id.PlayingChD);
                            break;
                    }

                    if (tempCh != null && tempCh.isPlaying())
                    {
                        tempCh.stopMedia();
                        tempCh.goToStart();
                        iv.SetImageResource(Resource.Drawable.WhiteLED);
                    }
                }
            }
        }

        /// <summary>
        /// Recordmodes the given view.
        /// </summary>
        /// <exception cref="RuntimeException">Thrown when a Runtime error condition occurs.</exception>
        /// <param name="view">The view.</param>
        [Export("Recordmode")]
        public void Recordmode(View view)
        {
            if (recording)
                return;

            ImageView iv;

            if (Ch1Record || Ch2Record || Ch3Record || Ch4Record || Ch5Record || ChDRecord)
                return;

            switch (view.Id)
            {
                case Resource.Id.CH1_Edit:
                    if (Ch1Record)
                    {
                        Ch1Record = false;
                        iv = (ImageView)FindViewById(Resource.Id.CH1_Edit);
                        iv.SetBackgroundResource(Resource.Drawable.Button_Rec_White_svg);
                    }
                    else
                    {
                        if (ch1.isPlaying())
                            return;
                        Ch1Record = true;
                        iv = (ImageView)FindViewById(Resource.Id.CH1_Edit);
                        iv.SetBackgroundResource(Resource.Drawable.Button_Rec_Red_svg);
                    }
                    break;

                case Resource.Id.CH2_Edit:
                    if (Ch2Record)
                    {
                        Ch2Record = false;
                        iv = (ImageView)FindViewById(Resource.Id.CH2_Edit);
                        iv.SetBackgroundResource(Resource.Drawable.Button_Rec_White_svg);
                    }
                    else
                    {
                        if (ch2.isPlaying())
                            return;
                        Ch2Record = true;
                        iv = (ImageView)FindViewById(Resource.Id.CH2_Edit);
                        iv.SetBackgroundResource(Resource.Drawable.Button_Rec_Red_svg);
                    }
                    break;

                case Resource.Id.CH3_Edit:
                    if (Ch3Record)
                    {
                        Ch3Record = false;
                        iv = (ImageView)FindViewById(Resource.Id.CH3_Edit);
                        iv.SetBackgroundResource(Resource.Drawable.Button_Rec_White_svg);
                    }
                    else
                    {
                        if (ch3.isPlaying())
                            return;
                        Ch3Record = true;
                        iv = (ImageView)FindViewById(Resource.Id.CH3_Edit);
                        iv.SetBackgroundResource(Resource.Drawable.Button_Rec_Red_svg);
                    }
                    break;

                case Resource.Id.CH4_Edit:
                    if (Ch4Record)
                    {
                        if (ch4.isPlaying())
                            return;
                        Ch4Record = false;
                        iv = (ImageView)FindViewById(Resource.Id.CH4_Edit);
                        iv.SetBackgroundResource(Resource.Drawable.Button_Rec_White_svg);
                    }
                    else
                    {
                        Ch4Record = true;
                        iv = (ImageView)FindViewById(Resource.Id.CH4_Edit);
                        iv.SetBackgroundResource(Resource.Drawable.Button_Rec_Red_svg);
                    }
                    break;

                case Resource.Id.CH5_Edit:
                    if (Ch5Record)
                    {
                        Ch5Record = false;
                        iv = (ImageView)FindViewById(Resource.Id.CH5_Edit);
                        iv.SetBackgroundResource(Resource.Drawable.Button_Rec_White_svg);
                    }
                    else
                    {
                        if (ch5.isPlaying())
                            return;
                        Ch5Record = true;
                        iv = (ImageView)FindViewById(Resource.Id.CH5_Edit);
                        iv.SetBackgroundResource(Resource.Drawable.Button_Rec_Red_svg);
                    }
                    break;

                case Resource.Id.CHD_Edit:
                    if (ChDRecord)
                    {
                        ChDRecord = false;
                        iv = (ImageView)FindViewById(Resource.Id.CHD_Edit);
                        iv.SetBackgroundResource(Resource.Drawable.Button_Rec_White_svg);
                    }
                    else
                    {
                        if (chd.isPlaying())
                            return;
                        ChDRecord = true;
                        iv = (ImageView)FindViewById(Resource.Id.CHD_Edit);
                        iv.SetBackgroundResource(Resource.Drawable.Button_Rec_Red_svg);
                    }
                    break;

                default:
                    throw new RuntimeException("Unknow button ID");
            }

            string currentDateTimeString = (new Date()).ToString();
            currentDateTimeString = currentDateTimeString.Replace(" ", "");
            currentDateTimeString = currentDateTimeString.Replace(".", "");
            currentDateTimeString = currentDateTimeString.Replace(":", "");
            mFileName = ExternalCacheDir.AbsolutePath;
            mFileName += "/" + currentDateTimeString + ".3gp";
            FilesToDelete.Add(mFileName);

            mRecorder = new MediaRecorder();
            mRecorder.SetAudioSource(AudioSource.Mic);
            mRecorder.SetOutputFormat(OutputFormat.ThreeGpp);
            mRecorder.SetOutputFile(mFileName);
            mRecorder.SetAudioEncoder(AudioEncoder.AmrNb);

            try
            {
                mRecorder.Prepare();
            }
            catch (IOException)
            {
            }
        }

        /// <summary>
        /// Resets the pitch described by view.
        /// </summary>
        /// <param name="view">The view.</param>
        [Export("ResetPitch")]
        public void ResetPitch(View view)
        {
            switch (mActiveChannel)
            {
                case 1:
                    if (ch1.trackLoaded())
                    {
                        ch1.Pitch = 1;
                        channelPitch.Progress = 100;
                    }
                    break;

                case 2:
                    if (ch2.trackLoaded())
                    {
                        ch2.Pitch = 1;
                        channelPitch.Progress = 100;
                    }
                    break;

                case 3:
                    if (ch3.trackLoaded())
                    {
                        ch3.Pitch = 1;
                        channelPitch.Progress = 100;
                    }
                    break;

                case 4:
                    if (ch4.trackLoaded())
                    {
                        ch4.Pitch = 1;
                        channelPitch.Progress = 100;
                    }
                    break;

                case 5:
                    if (ch5.trackLoaded())
                    {
                        ch5.Pitch = 1;
                        channelPitch.Progress = 100;
                    }
                    break;

                case 6:
                    if (chd.trackLoaded())
                    {
                        chd.Pitch = 1;
                        channelPitch.Progress = 100;
                    }
                    break;
            }
        }

        /// <summary>
        /// Resets the speed described by view.
        /// </summary>
        /// <param name="view">The view.</param>
        [Export("ResetSpeed")]
        public void ResetSpeed(View view)
        {
            switch (mActiveChannel)
            {
                case 1:
                    if (ch1.trackLoaded())
                    {
                        ch1.Speed = 1;
                        channelSpeed.Progress = 100;
                    }
                    break;

                case 2:
                    if (ch2.trackLoaded())
                    {
                        ch2.Speed = 1;
                        channelSpeed.Progress = 100;
                    }
                    break;

                case 3:
                    if (ch3.trackLoaded())
                    {
                        ch3.Speed = 1;
                        channelSpeed.Progress = 100;
                    }
                    break;

                case 4:
                    if (ch4.trackLoaded())
                    {
                        ch4.Speed = 1;
                        channelSpeed.Progress = 100;
                    }
                    break;

                case 5:
                    if (ch5.trackLoaded())
                    {
                        ch5.Speed = 1;
                        channelSpeed.Progress = 100;
                    }
                    break;

                case 6:
                    if (chd.trackLoaded())
                    {
                        chd.Speed = 1;
                        channelSpeed.Progress = 100;
                    }
                    break;
            }
        }

        /// <summary>
        /// Saves as dialog.
        /// </summary>
        public void SaveAsDialog()
        {
            if(!m_foldersExisting)
                InitLooperFolder();

            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle("Save channel as ...");

            // Set up the input
            EditText input = new EditText(this);
            input.Id = 5;

            builder.SetView(input);

            // Set up the buttons
            builder.SetPositiveButton("OK", HandlerOKButton);

            builder.SetNegativeButton("Cancel", HandlerCancelButton);
            builder.Show();
        }

        /// <summary>
        /// Saves a channel.
        /// </summary>
        /// <param name="view">The view.</param>
        [Export("SaveChannel")]
        public void SaveChannel(View view)
        {
            ChannelNew tempCh;

            switch (mActiveChannel)
            {
                case 1:
                    tempCh = ch1;
                    break;

                case 2:
                    tempCh = ch2;
                    break;

                case 3:
                    tempCh = ch3;
                    break;

                case 4:
                    tempCh = ch4;
                    break;

                case 5:
                    tempCh = ch5;
                    break;

                case 6:
                    tempCh = chd;
                    break;

                default:
                    tempCh = null;
                    break;
            }

            if (tempCh != null && tempCh.trackLoaded())
            {
                SaveAsDialog();
            }
        }


        [Export("ShowVersion")]
        public void ShowVersion(View view)
        {
            var pInfo = this.PackageManager.GetPackageInfo(this.PackageName, 0);
            ShowAlert(pInfo.VersionName);
        }

        /// <summary>
        /// ///
        /// </summary>
        /// <param name="view"></param>
        [Export("SaveSetlist")]
        public void SaveSetlist(View view)
        {
            m_trackListToSave = new List<string>();
            m_volumeListToSave = new List<float>();
            m_pitchListToSave = new List<float>();
            m_speedListToSave = new List<float>();

            if (!string.IsNullOrEmpty(ch1.getLoadedTrack()))
            {
                m_trackListToSave.Add(ch1.getLoadedTrack());
                m_volumeListToSave.Add(ch1.getVolume());
                m_pitchListToSave.Add(ch1.getPitch());
                m_speedListToSave.Add(ch1.getSpeed());
            }
            else
            {
                m_trackListToSave.Add(string.Empty);
                m_volumeListToSave.Add(0);
                m_pitchListToSave.Add(0);
                m_speedListToSave.Add(0);
            }

            if (!string.IsNullOrEmpty(ch2.getLoadedTrack()))
            {
                m_trackListToSave.Add(ch2.getLoadedTrack());
                m_volumeListToSave.Add(ch2.getVolume());
                m_pitchListToSave.Add(ch2.getPitch());
                m_speedListToSave.Add(ch2.getSpeed());
            }
            else
            {
                m_trackListToSave.Add(string.Empty);
                m_volumeListToSave.Add(0);
                m_pitchListToSave.Add(0);
                m_speedListToSave.Add(0);
            }

            if (!string.IsNullOrEmpty(ch3.getLoadedTrack()))
            {
                m_trackListToSave.Add(ch3.getLoadedTrack());
                m_volumeListToSave.Add(ch3.getVolume());
                m_pitchListToSave.Add(ch3.getPitch());
                m_speedListToSave.Add(ch3.getSpeed());
            }
            else
            {
                m_trackListToSave.Add(string.Empty);
                m_volumeListToSave.Add(0);
                m_pitchListToSave.Add(0);
                m_speedListToSave.Add(0);
            }

            if (!string.IsNullOrEmpty(ch4.getLoadedTrack()))
            {
                m_trackListToSave.Add(ch4.getLoadedTrack());
                m_volumeListToSave.Add(ch4.getVolume());
                m_pitchListToSave.Add(ch4.getPitch());
                m_speedListToSave.Add(ch4.getSpeed());
            }
            else
            {
                m_trackListToSave.Add(string.Empty);
                m_volumeListToSave.Add(0);
                m_pitchListToSave.Add(0);
                m_speedListToSave.Add(0);
            }

            if (!string.IsNullOrEmpty(ch5.getLoadedTrack()))
            {
                m_trackListToSave.Add(ch5.getLoadedTrack());
                m_volumeListToSave.Add(ch5.getVolume());
                m_pitchListToSave.Add(ch5.getPitch());
                m_speedListToSave.Add(ch5.getSpeed());
            }
            else
            {
                m_trackListToSave.Add(string.Empty);
                m_volumeListToSave.Add(0);
                m_pitchListToSave.Add(0);
                m_speedListToSave.Add(0);
            }

            if (!string.IsNullOrEmpty(chd.getLoadedTrack()))
            {
                m_trackListToSave.Add(chd.getLoadedTrack());
                m_volumeListToSave.Add(chd.getVolume());
                m_pitchListToSave.Add(chd.getPitch());
                m_speedListToSave.Add(chd.getSpeed());
            }
            else
            {
                m_trackListToSave.Add(string.Empty);
                m_volumeListToSave.Add(0);
                m_pitchListToSave.Add(0);
                m_speedListToSave.Add(0);
            }

            SaveSetlistDialog();
        }

        /// <summary>
        /// Saves the setlist dialog.
        /// </summary>
        public void SaveSetlistDialog()
        {
            if (!m_foldersExisting)
                InitLooperFolder();

            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle("Save Setlist as ...");

            // Set up the input
            EditText input = new EditText(this);
            input.Id = 5;

            builder.SetView(input);

            // Set up the buttons
            builder.SetPositiveButton("OK", HandlerSLOKButton);

            builder.SetNegativeButton("Cancel", HandlerSLCancelButton);
            builder.Show();
        }

        /// <summary>
        /// Setlists the given view.
        /// </summary>
        /// <param name="view">The view.</param>
        [Export("Setlist")]
        public void Setlist(View view)
        {
            if (!m_foldersExisting)
                InitLooperFolder();

            List<string> extList = new List<string>();
            extList.Add(".sll");

            Bundle extListBundle = new Bundle();
            extListBundle.PutStringArrayList("ExtList", extList);

            Bundle filePathBundle = new Bundle();
            filePathBundle.PutString("FilePath", Android.App.Application.Context.GetExternalFilesDir(null) + File.Separator + "Looper");


            Intent i = new Intent(this, typeof(FilePickerActivity));
            i.PutExtra("ExtList", extListBundle);
            i.PutExtra("FilePath", filePathBundle);

            StartActivityForResult(i, REQUEST_SETLIST);
        }

        /// <summary>
        /// Shows the alert.
        /// </summary>
        /// <param name="str">The string.</param>
        public void ShowAlert(string str)
        {
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle(str);
            alert.SetPositiveButton("OK", (senderAlert, args) =>
            {
                // write your own set of instructions
            });

            //run the alert in UI thread to display in the screen
            RunOnUiThread(() =>
            {
                alert.Show();
            });
        }

        [Export("StartRec")]
        /// <summary>
        /// Starts the record.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <exception cref="RuntimeException">Unknow button ID</exception>
        public void StartRec(int selectedChannel)
        {
            if (!recording)
            {
                StartRecording();
                pb1.Progress = 0;
                recording = true;
            }
            else
            {
                StopRecording();
                recording = false;
            }

            ImageView iv = null;

            switch (selectedChannel)
            {
                case 1:
                    iv = (ImageView)FindViewById(Resource.Id.PlayingCh1);
                    break;

                case 2:
                    iv = (ImageView)FindViewById(Resource.Id.PlayingCh2);
                    break;

                case 3:
                    iv = (ImageView)FindViewById(Resource.Id.PlayingCh3);
                    break;

                case 4:
                    iv = (ImageView)FindViewById(Resource.Id.PlayingCh4);
                    break;

                case 5:
                    iv = (ImageView)FindViewById(Resource.Id.PlayingCh5);
                    break;

                case 6:
                    iv = (ImageView)FindViewById(Resource.Id.PlayingChD);
                    break;
            }

            if (recording)
            {
                iv.SetImageResource(Resource.Drawable.RedLED);
            }
            else
            {
                iv.SetImageResource(Resource.Drawable.WhiteLED);

                switch (selectedChannel)
                {
                    case 1:
                        pb1.Progress = 0;
                        ch1.loadTrack(mFileName);
                        pb1.Max = ch1.getDuration();
                        iv = (ImageView)FindViewById(Resource.Id.LoadedCh1);
                        iv.SetImageResource(Resource.Drawable.RedLED);
                        break;

                    case 2:
                        pb2.Progress = 0;
                        ch2.loadTrack(mFileName);
                        pb2.Max = ch2.getDuration();
                        iv = (ImageView)FindViewById(Resource.Id.LoadedCh2);
                        iv.SetImageResource(Resource.Drawable.RedLED);
                        break;

                    case 3:
                        pb3.Progress = 0;
                        ch3.loadTrack(mFileName);
                        pb3.Max = ch3.getDuration();
                        iv = (ImageView)FindViewById(Resource.Id.LoadedCh3);
                        iv.SetImageResource(Resource.Drawable.RedLED);
                        break;

                    case 4:
                        pb4.Progress = 0;
                        ch4.loadTrack(mFileName);
                        pb4.Max = ch4.getDuration();
                        iv = (ImageView)FindViewById(Resource.Id.LoadedCh4);
                        iv.SetImageResource(Resource.Drawable.RedLED);
                        break;

                    case 5:
                        pb5.Progress = 0;
                        ch5.loadTrack(mFileName);
                        pb5.Max = ch5.getDuration();
                        iv = (ImageView)FindViewById(Resource.Id.LoadedCh5);
                        iv.SetImageResource(Resource.Drawable.RedLED);
                        break;

                    case 6:
                        pbd.Progress = 0;
                        chd.loadTrack(mFileName);
                        pbd.Max = chd.getDuration();
                        iv = (ImageView)FindViewById(Resource.Id.LoadedChD);
                        iv.SetImageResource(Resource.Drawable.RedLED);
                        break;
                }
            }
        }

        /// <summary>
        /// Starts a sampler.
        /// </summary>
        /// <param name="view">The view.</param>
        [Export("StartSampler")]
        public void StartSampler(View view)
        {
            StopAllChannels();
            ClearChannelByNbr(1);
            ClearChannelByNbr(2);
            ClearChannelByNbr(3);
            ClearChannelByNbr(4);
            ClearChannelByNbr(5);
            ClearChannelByNbr(6);
            InitLooperFolder();
            StartActivityForResult(typeof(SamplerActivity), START_SAMPLER);
        }

        /// <summary>
        /// Stops all channels.
        /// </summary>
        private void StopAllChannels()
        {
            if (ch1.isPlaying())
            {
                ch1.stopMedia();
            }

            if (ch2.isPlaying())
            {
                ch2.stopMedia();
            }

            if (ch3.isPlaying())
            {
                ch3.stopMedia();
            }

            if (ch4.isPlaying())
            {
                ch4.stopMedia();
            }

            if (ch5.isPlaying())
            {
                ch5.stopMedia();
            }

            if (chd.isPlaying())
            {
                chd.stopMedia();
            }
        }

        /// <summary>
        /// Called as part of the activity lifecycle when an activity is going into the background,
        /// but has not (yet) been killed.
        /// </summary>
        protected override void OnPause()
        {
            StopAllChannels();
            base.OnPause();
        }

        /// <summary>
        /// Called when an activity you launched exits, giving you the requestCode you started it
        /// with, the resultCode it returned, and any additional data from it.
        /// </summary>
        /// <param name="requestCode">
        /// The integer request code originally supplied to startActivityForResult(), allowing you to
        /// identify who this result came from.
        /// </param>
        /// <param name="resultCode">
        /// The integer result code returned by the child activity through its setResult().
        /// </param>
        /// <param name="data">
        /// An Intent, which can return result data to the caller (various data can be attached to
        /// Intent "extras").
        /// </param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (data == null)
                return;

            if (requestCode == REQUEST_PATH)
            {
                ReactOnPath(requestCode, data);
            }
            if (requestCode == REQUEST_SETLIST)
            {
                OpenSetList(data);
            }
            if (requestCode == START_SAMPLER)
            {
                
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        public void ToggleScreenLock()
        {
            DeviceDisplay.KeepScreenOn = true;
        }

        /// <summary>
        /// Called when the activity is starting.
        /// </summary>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            ToggleScreenLock();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                ActivityCompat.RequestPermissions(this, permissions, REQUEST_PERMISSIONS);
            }

            volume1 = FindViewById<VerticalSeekbar>(Resource.Id.CH1_Volume);
            volume1.SetOnSeekBarChangeListener(this);

            volume2 = FindViewById<VerticalSeekbar>(Resource.Id.CH2_Volume);
            volume2.SetOnSeekBarChangeListener(this);

            volume3 = FindViewById<VerticalSeekbar>(Resource.Id.CH3_Volume);
            volume3.SetOnSeekBarChangeListener(this);

            volume4 = FindViewById<VerticalSeekbar>(Resource.Id.CH4_Volume);
            volume4.SetOnSeekBarChangeListener(this);

            volume5 = FindViewById<VerticalSeekbar>(Resource.Id.CH5_Volume);
            volume5.SetOnSeekBarChangeListener(this);

            volumeD = FindViewById<VerticalSeekbar>(Resource.Id.CHD_Volume);
            volumeD.SetOnSeekBarChangeListener(this);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                channelSpeed = FindViewById<SeekBar>(Resource.Id.ChannelSpeed);
                channelSpeed.SetOnSeekBarChangeListener(this);

                channelPitch = FindViewById<SeekBar>(Resource.Id.ChannelPitch);
                channelPitch.SetOnSeekBarChangeListener(this);
            }
            else
            {
                FindViewById<SeekBar>(Resource.Id.ChannelSpeed).Visibility = ViewStates.Invisible;
                FindViewById<SeekBar>(Resource.Id.ChannelPitch).Visibility = ViewStates.Invisible;
            }

            ImageView btn = FindViewById<ImageView>(Resource.Id.CH1_Rec);
            btn.LongClick += CustomViewLongClick;

            btn = FindViewById<ImageView>(Resource.Id.CH2_Rec);
            btn.LongClick += CustomViewLongClick;

            btn = FindViewById<ImageView>(Resource.Id.CH3_Rec);
            btn.LongClick += CustomViewLongClick;

            btn = FindViewById<ImageView>(Resource.Id.CH4_Rec);
            btn.LongClick += CustomViewLongClick;

            btn = FindViewById<ImageView>(Resource.Id.CH5_Rec);
            btn.LongClick += CustomViewLongClick;

            btn = FindViewById<ImageView>(Resource.Id.CHD_Rec);
            btn.LongClick += CustomViewLongClick;

            InitChannels();
            InitProgressbars();
            InitLooperFolder();
            ChangeChannel(FindViewById(Resource.Id.Sel1));

            m_foldersExisting = FoldersExisting();
        }

        /// <summary>
        /// Perform any final cleanup before an activity is destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            foreach (var file in FilesToDelete)
            {
                DeleteFile(file);
            }

            FilesToDelete.Clear();

            base.OnDestroy();
        }

        /// <summary>
        /// Creates data string.
        /// </summary>
        /// <returns>The new data string.</returns>
        private string CreateDataString()
        {
            string dataString = string.Empty; ;

            for (int i = 0; i < m_trackListToSave.Count; i++)
            {
                dataString += m_trackListToSave[i] + "|";
            }

            for (int i = 0; i < m_volumeListToSave.Count; i++)
            {
                dataString += m_volumeListToSave[i] + "|";
            }

            for (int i = 0; i < m_pitchListToSave.Count; i++)
            {
                dataString += m_pitchListToSave[i] + "|";
            }

            for (int i = 0; i < m_speedListToSave.Count; i++)
            {
                dataString += m_speedListToSave[i] + "|";
            }

            dataString = dataString.Remove(dataString.Length - 1, 1);

            return dataString;
        }

        /// <summary>
        /// Custom view long click.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="lev">Long click event information.</param>
        private void CustomViewLongClick(object sender, View.LongClickEventArgs lev)
        {
            ClearChannel((View)sender);
        }

        /// <summary>
        /// Handler cancel button.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Dialog click event information.</param>
        private void HandlerCancelButton(object sender, DialogClickEventArgs e)
        {
        }

        /// <summary>
        /// Handler ok button.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Dialog click event information.</param>
        private void HandlerOKButton(object sender, DialogClickEventArgs e)
        {
            var dialog = (Android.Support.V7.App.AlertDialog)sender;
            var input = dialog.FindViewById(5);
            string filename = ((EditText)input).Text;

            filename += ".3gp";

            ChannelNew tempCh;

            switch (mActiveChannel)
            {
                case 1:
                    tempCh = ch1;
                    break;

                case 2:
                    tempCh = ch2;
                    break;

                case 3:
                    tempCh = ch3;
                    break;

                case 4:
                    tempCh = ch4;
                    break;

                case 5:
                    tempCh = ch5;
                    break;

                case 6:
                    tempCh = chd;
                    break;

                default:
                    tempCh = null;
                    break;
            }

            if (!tempCh.SaveTrackAs(filename))
            {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);

                alert.SetTitle("Sorry !");
                alert.SetMessage("There's already a track with the name.");

                alert.SetNeutralButton("OK", HandlerCancelButton);

                Dialog alertDialog = alert.Create();
                alertDialog.Show();
            }
            else
            {
                var savedFile = Android.App.Application.Context.GetExternalFilesDir(null) + Java.IO.File.Separator + "Looper" + Java.IO.File.Separator + "loops" + Java.IO.File.Separator;//  Android.App.Application.Context.GetExternalFilesDir(null) + Java.IO.File.Separator + "Looper" + Java.IO.File.Separator + "loops" + Java.IO.File.Separator;
                savedFile += filename;

                ImageView iv;

                ClearChannelByNbr(mActiveChannel);

                switch (mActiveChannel)
                {
                    case 1:
                        ch1.loadTrack(savedFile);
                        iv = (ImageView)FindViewById(Resource.Id.LoadedCh1);
                        iv.SetImageResource(Resource.Drawable.RedLED);
                        break;

                    case 2:
                        ch2.loadTrack(savedFile);
                        iv = (ImageView)FindViewById(Resource.Id.LoadedCh2);
                        iv.SetImageResource(Resource.Drawable.RedLED);
                        break;

                    case 3:
                        ch3.loadTrack(savedFile);
                        iv = (ImageView)FindViewById(Resource.Id.LoadedCh3);
                        iv.SetImageResource(Resource.Drawable.RedLED);
                        break;

                    case 4:
                        ch4.loadTrack(savedFile);
                        iv = (ImageView)FindViewById(Resource.Id.LoadedCh4);
                        iv.SetImageResource(Resource.Drawable.RedLED);
                        break;

                    case 5:
                        ch5.loadTrack(savedFile);
                        iv = (ImageView)FindViewById(Resource.Id.LoadedCh5);
                        iv.SetImageResource(Resource.Drawable.RedLED);
                        break;

                    case 6:
                        chd.loadTrack(savedFile);
                        iv = (ImageView)FindViewById(Resource.Id.LoadedChD);
                        iv.SetImageResource(Resource.Drawable.RedLED);
                        break;

                    default:
                        
                        break;
                }
            }
        }

        /// <summary>
        /// Handler sl cancel button.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Dialog click event information.</param>
        private void HandlerSLCancelButton(object sender, DialogClickEventArgs e)
        {
        }

        /// <summary>
        /// Handler slok button.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Dialog click event information.</param>
        private void HandlerSLOKButton(object sender, DialogClickEventArgs e)
        {
            var dialog = (Android.Support.V7.App.AlertDialog)sender;
            var input = dialog.FindViewById(5);
            string filename = ((EditText)input).Text;

            filename += ".sll";

            string DefaultInitialDirectory = Android.App.Application.Context.GetExternalFilesDir(null) + Java.IO.File.Separator + "Looper" + Java.IO.File.Separator + "setlists" + Java.IO.File.Separator;

            string destinationFile = DefaultInitialDirectory + filename;

            if (!System.IO.File.Exists(destinationFile))
            {
                var fs = System.IO.File.Create(destinationFile);

                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                var dataString = CreateDataString();
                var byteArray = enc.GetBytes(dataString);
                fs.Write(byteArray, 0, byteArray.Length);

                fs.Close();
                m_trackListToSave.Clear();
                m_volumeListToSave.Clear();
                m_pitchListToSave.Clear();
                m_speedListToSave.Clear();
            }
            else
            {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);

                alert.SetTitle("Sorry !");
                alert.SetMessage("There's already a list with these name.");

                alert.SetNeutralButton("OK", HandlerCancelButton);

                Dialog alertDialog = alert.Create();
                alertDialog.Show();
            }
        }

        /// <summary>
        /// Initializes the channels.
        /// </summary>
        private void InitChannels()
        {
            ch1 = new ChannelNew(1);
            ch2 = new ChannelNew(2);
            ch3 = new ChannelNew(3);
            ch4 = new ChannelNew(4);
            ch5 = new ChannelNew(5);
            chd = new ChannelNew(6);
            volume1.Progress = 100;
            volume2.Progress = 100;
            volume3.Progress = 100;
            volume4.Progress = 100;
            volume5.Progress = 100;
            volumeD.Progress = 100;

            channelSpeed.Progress = 100;
            channelPitch.Progress = 100;
        }

        /// <summary>
        /// Initializes the looper folder.
        /// </summary>
        private void InitLooperFolder()
        {
            File folder = new File(Android.App.Application.Context.GetExternalFilesDir(null) + File.Separator + "Looper" + File.Separator + "loops");
            if (!folder.Exists())
                folder.Mkdirs();

            folder = new File(Android.App.Application.Context.GetExternalFilesDir(null) + File.Separator + "Looper" + File.Separator + "drums");
            if (!folder.Exists())
                folder.Mkdirs();

            folder = new File(Android.App.Application.Context.GetExternalFilesDir(null) + File.Separator + "Looper" + File.Separator + "setlists");
            if (!folder.Exists())
                folder.Mkdirs();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool FoldersExisting()
        {
            File folder = new File(Android.App.Application.Context.GetExternalFilesDir(null) + File.Separator + "Looper" + File.Separator + "loops");
            if (!folder.Exists())
                return false;

            folder = new File(Android.App.Application.Context.GetExternalFilesDir(null) + File.Separator + "Looper" + File.Separator + "drums");
            if (!folder.Exists())
                return false;

            folder = new File(Android.App.Application.Context.GetExternalFilesDir(null) + File.Separator + "Looper" + File.Separator + "setlists");
            if (!folder.Exists())
                return false;

            return true;
        }

        /// <summary>
        /// Initializes the progressbars.
        /// </summary>
        private void InitProgressbars()
        {
            pb1 = (ProgressBar)FindViewById(Resource.Id.progressBarCh1);
            pb1.Progress = 0;
            pb1.Max = 0;

            pb2 = (ProgressBar)FindViewById(Resource.Id.progressBarCh2);
            pb2.Progress = 0;
            pb2.Max = 0;

            pb3 = (ProgressBar)FindViewById(Resource.Id.progressBarCh3);
            pb3.Progress = 0;
            pb3.Max = 0;

            pb4 = (ProgressBar)FindViewById(Resource.Id.progressBarCh4);
            pb4.Progress = 0;
            pb4.Max = 0;

            pb5 = (ProgressBar)FindViewById(Resource.Id.progressBarCh5);
            pb5.Progress = 0;
            pb5.Max = 0;

            pbd = (ProgressBar)FindViewById(Resource.Id.progressBarChD);
            pbd.Progress = 0;
            pbd.Max = 0;
        }

        /// <summary>
        /// Loops this instance.
        /// </summary>
        private void Loop()
        {
            if (ch1.isPlaying())
            {
                if (ch1EndTime > (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds)
                {
                    pb1.Progress = (int)((new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds - ch1StartTime);
                }
                else
                {
                    pb1.Progress = 0;
                    ch1StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                    ch1EndTime = ch1StartTime + ch1.getDuration();
                }
            }

            if (ch2.isPlaying())
            {
                if (ch2EndTime > (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds)
                {
                    pb2.Progress = (int)((new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds - ch2StartTime);
                }
                else
                {
                    pb2.Progress = 0;
                    ch2StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                    ch2EndTime = ch2StartTime + ch2.getDuration();
                }
            }

            if (ch3.isPlaying())
            {
                if (ch3EndTime > (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds)
                {
                    pb3.Progress = (int)((new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds - ch3StartTime);
                }
                else
                {
                    pb3.Progress = 0;
                    ch3StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                    ch3EndTime = ch3StartTime + ch3.getDuration();
                }
            }

            if (ch4.isPlaying())
            {
                if (ch4EndTime > (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds)
                {
                    pb4.Progress = (int)((new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds - ch4StartTime);
                }
                else
                {
                    pb4.Progress = 0;
                    ch4StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                    ch4EndTime = ch4StartTime + ch4.getDuration();
                }
            }

            if (ch5.isPlaying())
            {
                if (ch5EndTime > (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds)
                {
                    pb5.Progress = (int)((new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds - ch5StartTime); 
                }
                else
                {
                    pb5.Progress = 0;
                    ch5StartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                    ch5EndTime = ch5StartTime + ch5.getDuration();
                }
            }

            if (chd.isPlaying())
            {
                if (chdEndTime > (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds)
                {
                    pbd.Progress = (int)((new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds - chdStartTime);
                }
                else
                {
                    pbd.Progress = 0;
                    chdStartTime = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
                    chdEndTime = chdStartTime + chd.getDuration();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                case Keycode.A:
                    ReactOnKey1();
                    return true;
                case Keycode.B:
                    ReactOnKey2();
                    return true;
            }
            return base.OnKeyDown(keyCode, e);
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReactOnKey1()
        {
            View view = null;
            //View RecView = null;
            switch (mActiveChannel)
            {
                case 1:
                    view = FindViewById(Resource.Id.CH1_Edit);
                    break;

                case 2:
                    view = FindViewById(Resource.Id.CH2_Edit);
                    break;

                case 3:
                    view = FindViewById(Resource.Id.CH3_Edit);
                    break;

                case 4:
                    view = FindViewById(Resource.Id.CH4_Edit);
                    break;

                case 5:
                    view = FindViewById(Resource.Id.CH5_Edit);
                    break;

                case 6:
                    view = FindViewById(Resource.Id.CHD_Edit);
                    break;
            }
            Recordmode(view);
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReactOnKey2()
        {
            View view = null;

            switch (mActiveChannel)
            {
                case 1:
                    view = FindViewById(Resource.Id.CH1_Rec);
                    break;

                case 2:
                    view = FindViewById(Resource.Id.CH2_Rec);
                    break;

                case 3:
                    view = FindViewById(Resource.Id.CH3_Rec);
                    break;

                case 4:
                    view = FindViewById(Resource.Id.CH4_Rec);
                    break;

                case 5:
                    view = FindViewById(Resource.Id.CH5_Rec);
                    break;

                case 6:
                    view = FindViewById(Resource.Id.CHD_Rec);
                    break;
            }

            Play(view);
        }

        /// <summary>
        /// Mies the background task.
        /// </summary>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task MyBackgroundTask(System.Threading.CancellationToken cancelToken)
        {
            while (true)
            {
                RunOnUiThread(() => Loop());
                await System.Threading.Tasks.Task.Delay(1, cancelToken);
            }
        }

        /// <summary>
        /// Prepare channel.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="channel">The channel.</param>
        private void PrepareChannel(string file, int channel)
        {
            ImageView iv;

            ClearChannelByNbr(channel);

            switch (channel)
            {
                case 1:
                    pb1.Progress = 0;
                    ch1.loadTrack(file);
                    pb1.Max = ch1.getDuration();
                    ch1.Speed = 1;
                    ch1.Pitch = 1;
                    if (mActiveChannel == 1)
                    {
                        channelSpeed.Progress = (int)(ch1.Speed * 100);
                        channelPitch.Progress = (int)(ch1.Pitch * 100);
                    }
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh1);
                    iv.SetImageResource(Resource.Drawable.RedLED);
                    break;

                case 2:
                    pb2.Progress = 0;
                    ch2.loadTrack(file);
                    pb2.Max = ch2.getDuration();
                    ch2.Speed = 1;
                    ch2.Pitch = 1;
                    if (mActiveChannel == 2)
                    {
                        channelSpeed.Progress = (int)(ch2.Speed * 100);
                        channelPitch.Progress = (int)(ch2.Pitch * 100);
                    }
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh2);
                    iv.SetImageResource(Resource.Drawable.RedLED);
                    break;

                case 3:
                    pb3.Progress = 0;
                    ch3.loadTrack(file);
                    pb3.Max = ch3.getDuration();
                    ch3.Speed = 1;
                    ch3.Pitch = 1;
                    if (mActiveChannel == 3)
                    {
                        channelSpeed.Progress = (int)(ch3.Speed * 100);
                        channelPitch.Progress = (int)(ch3.Pitch * 100);
                    }
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh3);
                    iv.SetImageResource(Resource.Drawable.RedLED);
                    break;

                case 4:
                    pb4.Progress = 0;
                    ch4.loadTrack(file);
                    pb4.Max = ch4.getDuration();
                    ch4.Speed = 1;
                    ch4.Pitch = 1;
                    if (mActiveChannel == 4)
                    {
                        channelSpeed.Progress = (int)(ch4.Speed * 100);
                        channelPitch.Progress = (int)(ch4.Pitch * 100);
                    }
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh4);
                    iv.SetImageResource(Resource.Drawable.RedLED);
                    break;

                case 5:
                    pb5.Progress = 0;
                    ch5.loadTrack(file);
                    pb5.Max = ch5.getDuration();
                    ch5.Speed = 1;
                    ch5.Pitch = 1;
                    if (mActiveChannel == 5)
                    {
                        channelSpeed.Progress = (int)(ch5.Speed * 100);
                        channelPitch.Progress = (int)(ch5.Pitch * 100);
                    }
                    iv = (ImageView)FindViewById(Resource.Id.LoadedCh5);
                    iv.SetImageResource(Resource.Drawable.RedLED);
                    break;

                case 6:
                    pbd.Progress = 0;
                    chd.loadTrack(file);
                    pbd.Max = chd.getDuration();
                    chd.Speed = 1;
                    chd.Pitch = 1;
                    if (mActiveChannel == 6)
                    {
                        channelSpeed.Progress = (int)(chd.Speed * 100);
                        channelPitch.Progress = (int)(chd.Pitch * 100);
                    }
                    iv = (ImageView)FindViewById(Resource.Id.LoadedChD);
                    iv.SetImageResource(Resource.Drawable.RedLED);
                    break;
            }
        }

        /// <summary>
        /// React on path.
        /// </summary>
        /// <param name="requestCode">The request code.</param>
        /// <param name="data">The data.</param>
        private void ReactOnPath(int requestCode, Intent data)
        {
            if (requestCode != 1)
                return;

            string curFile;

            curFile = data.GetStringExtra("GetPath");

            if (curFile == null)
                return;

            PrepareChannel(curFile, selectedChannel);
        }

        /// <summary>
        /// Reads data to string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The data to string.</returns>
        private List<string> ReadDataToString(byte[] data)
        {
            var list = new List<string>();

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            var dataString = enc.GetString(data);

            var pathList = dataString.Split("|");

            foreach (var path in pathList)
            {
                list.Add(path);
            }

            return list;
        }

        /// <summary>
        /// Sets a volume.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="volume">The volume.</param>
        private void SetVolume(int channel, float volume)
        {
            switch (channel)
            {
                case 1:
                    ch1.setVolume(volume);
                    volume1.SetProgress((int)(volume * 100), false);
                    break;

                case 2:
                    ch2.setVolume(volume);
                    volume2.SetProgress((int)(volume * 100), false);
                    break;

                case 3:
                    ch3.setVolume(volume);
                    volume3.SetProgress((int)(volume * 100), false);
                    break;

                case 4:
                    ch4.setVolume(volume);
                    volume4.SetProgress((int)(volume * 100), false);
                    break;

                case 5:
                    ch5.setVolume(volume);
                    volume5.SetProgress((int)(volume * 100), false);
                    break;

                case 6:
                    chd.setVolume(volume);
                    volumeD.SetProgress((int)(volume * 100), false);
                    break;
            }
        }

        /// <summary>
        /// Starts the recording.
        /// </summary>
        private void StartRecording()
        {
            if (mRecorder == null)
                return;

            mRecorder.Start();
        }

        /// <summary>
        /// Stops the recording.
        /// </summary>
        private void StopRecording()
        {
            mRecorder.Stop();
            mRecorder.Release();
            mRecorder = null;

            Ch1Record = false;
            FindViewById(Resource.Id.CH1_Edit).SetBackgroundResource(Resource.Drawable.Button_Rec_White_svg);

            Ch2Record = false;
            FindViewById(Resource.Id.CH2_Edit).SetBackgroundResource(Resource.Drawable.Button_Rec_White_svg);

            Ch3Record = false;
            FindViewById(Resource.Id.CH3_Edit).SetBackgroundResource(Resource.Drawable.Button_Rec_White_svg);

            Ch4Record = false;
            FindViewById(Resource.Id.CH4_Edit).SetBackgroundResource(Resource.Drawable.Button_Rec_White_svg);

            Ch5Record = false;
            FindViewById(Resource.Id.CH5_Edit).SetBackgroundResource(Resource.Drawable.Button_Rec_White_svg);

            ChDRecord = false;
            FindViewById(Resource.Id.CHD_Edit).SetBackgroundResource(Resource.Drawable.Button_Rec_White_svg);
        }
    }
}