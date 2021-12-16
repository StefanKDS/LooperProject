using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Interop;
using LooperApp.Helper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Essentials;

namespace LooperApp
{
    [Activity(Label = "sampler", Theme = "@style/AppTheme.NoActionBar", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class SamplerActivity : AppCompatActivity, SeekBar.IOnSeekBarChangeListener
    {
        private static readonly int REQUEST_PATH = 1;
        private static readonly int REQUEST_SETLIST = 2;
        private List<Slot> m_slots;    
        private int activeRemoteChannel = 1;
        private bool remoteActive = false;
        private bool autoNextActive = false;
        private int m_masterChannel = -1;
        private bool m_sync = false;
        private List<string> m_trackListToSave = new List<string>();
        private List<int> m_waitingForPlay = new List<int>();
        private int selectedChannel = 0;
        private List<int> selectedSyncChannels;
        private MultiSelectSpinner syncChannels;
        private Slot applauseSlot;


        [Export("BtnClick")]
        /// <summary>
        /// ///
        /// </summary>
        /// <param name="view"></param>
        public void BtnClick(View view)
        {
            ChannelNew tempChannel = null;
            int channel = -1;

            var slot = m_slots.Where(x => x.RessourceID == view.Id).SingleOrDefault();

            if (m_sync && m_masterChannel == channel && tempChannel.isPlaying())
            {
                Sync(null);
                m_masterChannel = -1;

                foreach(var ch in m_slots)
                {
                    StopChannel(ch.ChannelNbr);
                }

                return;
            }

            if (slot != null)
            {
                if (slot.Channel.trackLoaded() && !slot.Channel.isPlaying())
                {
                    var playingChannel = ChannelPlaying();
                    if (m_sync && selectedSyncChannels.Contains(channel))
                    {
                        if (playingChannel == -1)
                        {
                            slot.Channel.playMedia();
                            m_masterChannel = channel;
                        }
                        else if (playingChannel != -1 && playingChannel != channel)
                        {
                            m_waitingForPlay.Add(channel);
                        }
                    }
                    else
                        slot.Channel.playMedia();

                    if(slot.IV_Button != null)
                        slot.IV_Button.SetImageResource(Resource.Drawable.green_rect);
                }
                else if (slot.Channel.trackLoaded() && slot.Channel.isPlaying())
                {
                    slot.Channel.stopMedia();
                    if (slot.IV_Button != null)
                        slot.IV_Button.SetImageResource(Resource.Drawable.orange_rect);
                }
                else if (slot.Channel.trackLoaded() && !slot.Channel.isPlaying())
                {
                    slot.Channel.playMedia();
                    if (slot.IV_Button != null)
                        slot.IV_Button.SetImageResource(Resource.Drawable.green_rect);
                }
            }
        }

        [Export("BtnClickLoop")]
        public void BtnClickLoop(View view)
        {
            var slot = m_slots.Where(x => x.LoopRessourceID == view.Id).Single();

            if (slot.Channel.isPlaying())
                return;

            if (slot.LoopSlot)
            {
                slot.LoopSlot = false;
                slot.IV_Loop_Button.SetImageResource(Resource.Drawable.red_rect_L);
            }
            else
            {
                slot.LoopSlot = false;
                slot.IV_Loop_Button.SetImageResource(Resource.Drawable.green_rect_L);
            }

            slot.Channel.LoopChannel(slot.LoopSlot);
        }

        /// <summary>
        /// ///
        /// </summary>
        /// <param name="channelIndex"></param>
        public void ChannelRestart(int channelIndex)
        {
            if (!m_sync)
                return;

            if (m_masterChannel == channelIndex)
            {
                if (m_waitingForPlay.Count > 0)
                {
                    foreach (var channel in m_waitingForPlay)
                    {
                        var slot = m_slots.Where(x => x.ChannelNbr == channel).Single();

                        slot.Channel.playMedia();
                    }

                    m_waitingForPlay.Clear();
                }
            }
        }

        public void ChannelStoped(int channelIndex)
        {
            var slot = m_slots.Where(x => x.ChannelNbr == channelIndex).Single();

            slot.Channel.loadTrackAgain();
            slot.IV_Button.SetImageResource(Resource.Drawable.orange_rect);
        }

        /// <summary>
        /// Getfiles the specified view.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <exception cref="RuntimeException">Unknow button ID</exception>
        public void GetFile(View view)
        {
            var slot = m_slots.Where(x => x.RessourceID == view.Id).Single();
            selectedChannel = slot.ChannelNbr;

            List<string> extList = new List<string>();
            extList.Add(".mp3");
            extList.Add(".wav");
            extList.Add(".3gp");

            Bundle mybundle = new Bundle();
            mybundle.PutStringArrayList("ExtList", extList);

            Bundle filePathBundle = new Bundle();
            filePathBundle.PutString("FilePath", Android.App.Application.Context.GetExternalFilesDir(null) + Java.IO.File.Separator + "Looper");

            Intent i = new Intent(this, typeof(FilePickerActivity));
            i.PutExtra("ExtList", mybundle);
            i.PutExtra("FilePath", filePathBundle);

            StartActivityForResult(i, REQUEST_PATH);
        }

        /// <summary>
        /// ///
        /// </summary>
        /// <param name="seekBar"></param>
        /// <param name="progress"></param>
        /// <param name="fromUser"></param>
        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            float volume = (float)progress / 100;

            var slot = m_slots.Where(x => x.VolumeRessourceID == seekBar.Id).Single();
            slot.Channel.setVolume(volume);
        }

        /// <summary>
        /// ///
        /// </summary>
        /// <param name="seekBar"></param>
        public void OnStartTrackingTouch(SeekBar seekBar)
        {
        }

        /// <summary>
        /// ///
        /// </summary>
        /// <param name="seekBar"></param>
        public void OnStopTrackingTouch(SeekBar seekBar)
        {
        }

        public void SaveAsDialog()
        {
            if (m_trackListToSave.Count == 0)
            {
                return;
            }

            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle("Speichern als ...");

            // Set up the input
            EditText input = new EditText(this);
            input.Id = 5;

            builder.SetView(input);

            // Set up the buttons
            builder.SetPositiveButton("OK", HandlerOKButton);

            builder.SetNegativeButton("Abbrechen", HandlerCancelButton);
            builder.Show();
        }

        [Export("SaveAsSetlist")]
        public void SaveAsSetlist(View view)
        {
            foreach(var slot in m_slots)
            {
                if (!string.IsNullOrEmpty(slot.Channel.getLoadedTrack()))
                {
                    m_trackListToSave.Add(slot.Channel.getLoadedTrack());
                }
                else
                    m_trackListToSave.Add(string.Empty);
            }

            SaveAsDialog();
        }

        [Export("Setlist")]
        /// <summary>
        /// ///
        /// </summary>
        public void Setlist(View view)
        {
            List<string> extList = new List<string>();
            extList.Add(".slsl");

            Bundle mybundle = new Bundle();
            mybundle.PutStringArrayList("ExtList", extList);

            Bundle filePathBundle = new Bundle();
            filePathBundle.PutString("FilePath", Android.App.Application.Context.GetExternalFilesDir(null) + Java.IO.File.Separator + "Looper");

            Intent i = new Intent(this, typeof(FilePickerActivity));
            i.PutExtra("ExtList", mybundle);
            i.PutExtra("FilePath", filePathBundle);

            StartActivityForResult(i, REQUEST_SETLIST);
        }

        [Export("ReactOnAutoNext")]
        /// <summary>
        /// ///
        /// </summary>
        public void ReactOnAutoNext(View view)
        {
            if (!remoteActive)
                return;

            if(!autoNextActive)
            {
                autoNextActive = true;

                Button bv = (Button)FindViewById(Resource.Id.AutoNext);
                bv.SetBackgroundColor(Android.Graphics.Color.ParseColor("#25b027"));
            }
            else
            {
                autoNextActive = false;
                Button bv = (Button)FindViewById(Resource.Id.AutoNext);
                bv.SetBackgroundColor(Android.Graphics.Color.ParseColor("#b02525"));
            }
        }

        [Export("ReactOnApplaus")]
        /// <summary>
        /// ///
        /// </summary>
        public void ReactOnApplaus(View view)
        {
            ReactOnKey3();
        }

        [Export("ReactOnRemote")]
        /// <summary>
        /// ///
        /// </summary>
        public void ReactOnRemote(View view)
        {
            if(!remoteActive)
            {
                remoteActive = true;
                Button bv = (Button)FindViewById(Resource.Id.Remote);
                bv.SetBackgroundColor(Android.Graphics.Color.ParseColor("#25b027"));

                TextView tv = (TextView)FindViewById(Resource.Id.Ch1Text);
                tv.SetTextColor(Android.Graphics.Color.ParseColor("#25b027"));
            }
            else
            {
                remoteActive = false;
                Button bv = (Button)FindViewById(Resource.Id.Remote);
                bv.SetBackgroundColor(Android.Graphics.Color.ParseColor("#b02525"));

                foreach (var slot in m_slots)
                {
                    slot.TV_Title.SetTextColor(Android.Graphics.Color.ParseColor("#FFFFFF"));
                }
            }
        }

        /// <summary>
        /// ///
        /// </summary>
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (remoteActive)
            {
                switch (keyCode)
                {
                    case Keycode.PageUp:
                        ReactOnKey1();
                        return true;
                    case Keycode.PageDown:
                        ReactOnKey2();
                        return true;
                    case Keycode.C:
                        ReactOnKey3();
                        return true;
                    case Keycode.D:
                        ReactOnKey4();
                        return true;
                }
            }

            return base.OnKeyDown(keyCode, e);
        }

        /// <summary>
        /// ///
        /// </summary>
        private void ReactOnKey1()
        {

            foreach(var slot in m_slots)
            {
                slot.TV_Title.SetTextColor(Android.Graphics.Color.ParseColor("#FFFFFF"));
            }
           
            bool channelLoaded = false;

            while (!channelLoaded)
            {
                if (activeRemoteChannel < 8)
                {
                    activeRemoteChannel++;

                    var slot = m_slots.Where(x => x.ChannelNbr == activeRemoteChannel).Single();

                    if (slot.Channel.trackLoaded())
                    {
                        channelLoaded = true;
                    }
                }
                else
                {
                    activeRemoteChannel = 1;
                    channelLoaded = true;
                }
            }

            var newSlot = m_slots.Where(x => x.ChannelNbr == activeRemoteChannel).Single();
            newSlot.TV_Title.SetTextColor(Android.Graphics.Color.ParseColor("#25b027"));
        }

        /// <summary>
        /// ///
        /// </summary>
        private bool ChannelLoaded(int channelNbr)
        {
            var slot = m_slots.Where(x => x.ChannelNbr == channelNbr).Single();
            return slot.Channel.trackLoaded();
        }

        /// <summary>
        /// ///
        /// </summary>
        private void ReactOnKey2()
        {
            var view = new View(Android.App.Application.Context);

            var slot = m_slots.Where(x => x.ChannelNbr == activeRemoteChannel).Single();
            view.Id = slot.RessourceID;
            BtnClick(view);

            if (remoteActive && autoNextActive)
            {
                ReactOnKey1();
            }
        }

        private void ReactOnKey3()
        {
            if (!applauseSlot.Channel.isPlaying())
            {
                applauseSlot = new Slot(9, true);
                applauseSlot.Channel.setVolume(0.9f);
                applauseSlot.Channel.loadTrack(Assets.OpenFd("Applause.mp3"));
                applauseSlot.RessourceID = Resource.Id.Applaus;
                applauseSlot.Channel.playMedia();
            }
            else
            {
                applauseSlot.Channel.stopMedia();
            }
        }

        private void ReactOnKey4()
        {

        }

        [Export("Sync")]
        /// <summary>
        /// ///
        /// </summary>
        public void Sync(View view)
        {
            if (m_sync)
            {
                m_sync = false;
                Button bv = (Button)FindViewById(Resource.Id.CheckSync);
                bv.SetBackgroundColor(Android.Graphics.Color.ParseColor("#25b027"));
                MultiSelectSpinner mss = (MultiSelectSpinner)FindViewById(Resource.Id.syncChannels);
                mss.SetBackgroundColor(Android.Graphics.Color.ParseColor("#25b027"));
            }
            else
            {
                if (ChannelPlaying() == -1)
                {
                    var selStrings = syncChannels.GetSelectedStrings();

                    if (selStrings.Count == 0)
                        return;

                    if (selectedSyncChannels == null)
                        selectedSyncChannels = new List<int>();
                    else
                        selectedSyncChannels?.Clear();

                    foreach (var item in selStrings)
                    {
                        selectedSyncChannels.Add(int.Parse(item.Substring(item.Length - 1)));
                    }

                    m_sync = true;
                    Button bv = (Button)FindViewById(Resource.Id.CheckSync);
                    bv.SetBackgroundColor(Android.Graphics.Color.ParseColor("#b02525"));
                    MultiSelectSpinner mss = (MultiSelectSpinner)FindViewById(Resource.Id.syncChannels);
                    mss.SetBackgroundColor(Android.Graphics.Color.ParseColor("#b02525"));
                }
            }
        }

        /// <summary>
        /// ///
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="data"></param>
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

            base.OnActivityResult(requestCode, resultCode, data);
        }
        public void ToggleScreenLock()
        {
            DeviceDisplay.KeepScreenOn = true;
        }

        /// <summary>
        /// ///
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.sampler);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            ToggleScreenLock();

            m_slots = new List<Slot>();

            for (int i=1; i<=8; i++)
            {
                var slot = new Slot(i, true);

                switch (i)
                {
                    case 1:
                        slot.IV_Button = FindViewById<ImageView>(Resource.Id.Ch1);    
                        slot.IV_Loop_Button = FindViewById<ImageView>(Resource.Id.Ch1_Loop);
                        slot.VolumeSeekbar = FindViewById<SeekBar>(Resource.Id.Volume1);
                        slot.TV_Title = (TextView)FindViewById(Resource.Id.Ch1Text);
                        slot.RessourceID = Resource.Id.Ch1;
                        slot.LoopRessourceID = Resource.Id.Ch1_Loop;
                        slot.VolumeRessourceID = Resource.Id.Volume1;
                        slot.TitleRessourceID = Resource.Id.Ch1Text;
                        break;
                    case 2:
                        slot.IV_Button = FindViewById<ImageView>(Resource.Id.Ch2);
                        slot.IV_Loop_Button = FindViewById<ImageView>(Resource.Id.Ch2_Loop);
                        slot.VolumeSeekbar = FindViewById<SeekBar>(Resource.Id.Volume2);
                        slot.TV_Title = (TextView)FindViewById(Resource.Id.Ch2Text);
                        slot.RessourceID = Resource.Id.Ch2;
                        slot.LoopRessourceID = Resource.Id.Ch2_Loop;
                        slot.VolumeRessourceID = Resource.Id.Volume2;
                        slot.TitleRessourceID = Resource.Id.Ch2Text;
                        break;
                    case 3:
                        slot.IV_Button = FindViewById<ImageView>(Resource.Id.Ch3);
                        slot.IV_Loop_Button = FindViewById<ImageView>(Resource.Id.Ch3_Loop);
                        slot.VolumeSeekbar = FindViewById<SeekBar>(Resource.Id.Volume3);
                        slot.TV_Title = (TextView)FindViewById(Resource.Id.Ch3Text);
                        slot.RessourceID = Resource.Id.Ch3;
                        slot.LoopRessourceID = Resource.Id.Ch3_Loop;
                        slot.VolumeRessourceID = Resource.Id.Volume3;
                        slot.TitleRessourceID = Resource.Id.Ch3Text;
                        break;
                    case 4:
                        slot.IV_Button = FindViewById<ImageView>(Resource.Id.Ch4);
                        slot.IV_Loop_Button = FindViewById<ImageView>(Resource.Id.Ch4_Loop);
                        slot.VolumeSeekbar = FindViewById<SeekBar>(Resource.Id.Volume4);
                        slot.TV_Title = (TextView)FindViewById(Resource.Id.Ch4Text);
                        slot.RessourceID = Resource.Id.Ch4;
                        slot.LoopRessourceID = Resource.Id.Ch4_Loop;
                        slot.VolumeRessourceID = Resource.Id.Volume4;
                        slot.TitleRessourceID = Resource.Id.Ch4Text;
                        break;
                    case 5:
                        slot.IV_Button = FindViewById<ImageView>(Resource.Id.Ch5);
                        slot.IV_Loop_Button = FindViewById<ImageView>(Resource.Id.Ch5_Loop);
                        slot.VolumeSeekbar = FindViewById<SeekBar>(Resource.Id.Volume5);
                        slot.TV_Title = (TextView)FindViewById(Resource.Id.Ch5Text);
                        slot.RessourceID = Resource.Id.Ch5;
                        slot.LoopRessourceID = Resource.Id.Ch5_Loop;
                        slot.VolumeRessourceID = Resource.Id.Volume5;
                        slot.TitleRessourceID = Resource.Id.Ch5Text;
                        break;
                    case 6:
                        slot.IV_Button = FindViewById<ImageView>(Resource.Id.Ch6);
                        slot.IV_Loop_Button = FindViewById<ImageView>(Resource.Id.Ch6_Loop);
                        slot.VolumeSeekbar = FindViewById<SeekBar>(Resource.Id.Volume6);
                        slot.TV_Title = (TextView)FindViewById(Resource.Id.Ch6Text);
                        slot.RessourceID = Resource.Id.Ch6;
                        slot.LoopRessourceID = Resource.Id.Ch6_Loop;
                        slot.VolumeRessourceID = Resource.Id.Volume6;
                        slot.TitleRessourceID = Resource.Id.Ch6Text;
                        break;
                    case 7:
                        slot.IV_Button = FindViewById<ImageView>(Resource.Id.Ch7);
                        slot.IV_Loop_Button = FindViewById<ImageView>(Resource.Id.Ch7_Loop);
                        slot.VolumeSeekbar = FindViewById<SeekBar>(Resource.Id.Volume7);
                        slot.TV_Title = (TextView)FindViewById(Resource.Id.Ch7Text);
                        slot.RessourceID = Resource.Id.Ch7;
                        slot.LoopRessourceID = Resource.Id.Ch7_Loop;
                        slot.VolumeRessourceID = Resource.Id.Volume7;
                        slot.TitleRessourceID = Resource.Id.Ch7Text;
                        break;
                    case 8:
                        slot.IV_Button = FindViewById<ImageView>(Resource.Id.Ch8);
                        slot.IV_Loop_Button = FindViewById<ImageView>(Resource.Id.Ch8_Loop);
                        slot.VolumeSeekbar = FindViewById<SeekBar>(Resource.Id.Volume8);
                        slot.TV_Title = (TextView)FindViewById(Resource.Id.Ch8Text);
                        slot.RessourceID = Resource.Id.Ch8;
                        slot.LoopRessourceID = Resource.Id.Ch8_Loop;
                        slot.VolumeRessourceID = Resource.Id.Volume8;
                        slot.TitleRessourceID = Resource.Id.Ch8Text;
                        break;
                }

                slot.IV_Button.LongClick += CustomViewLongClick;
                slot.VolumeSeekbar.SetOnSeekBarChangeListener(this);
                slot.Channel.NewLoopStarts += new ChannelNew.CallbackEventHandler(ChannelRestart);
                slot.Channel.ChannelStoped += new ChannelNew.CallbackEventHandler(ChannelStoped);
                slot.LoopSlot = false;

                m_slots.Add(slot);
            }

            applauseSlot = new Slot(9, true);
            applauseSlot.Channel.setVolume(0.9f);
            applauseSlot.Channel.loadTrack(Assets.OpenFd("Applause.mp3"));
            //applauseSlot.IV_Button = FindViewById<ImageView>(Resource.Id.Applaus);
            applauseSlot.RessourceID = Resource.Id.Applaus;

            string[] strings = { "Channel 1", "Channel 2", "Channel 3", "Channel 4", "Channel 5", "Channel 6", "Channel 7", "Channel 8" };

            syncChannels = (MultiSelectSpinner)FindViewById(Resource.Id.syncChannels);
            syncChannels.SetItems(strings);
        }

        /// <summary>
        /// Called as part of the activity lifecycle when an activity is going into the background,
        /// but has not (yet) been killed.
        /// </summary>
        protected override void OnPause()
        {
            StopAllChannels();
            foreach(var slot in m_slots)
            {
                slot.Channel.Reset();
            }

            base.OnPause();
        }

        /// <summary>
        /// ///
        /// </summary>
        /// <returns></returns>
        private int ChannelPlaying()
        {
            var slot = m_slots.Where(x => x.Channel.isPlaying()).FirstOrDefault();

            if (slot != null)
                return slot.ChannelNbr;

            return -1;
        }

        private string CreateDataString()
        {
            string dataString = string.Empty; ;

            for (int i = 0; i < m_trackListToSave.Count; i++)
            {
                dataString += m_trackListToSave[i] + "|";
            }

            var selStrings = syncChannels.GetSelectedStrings();

            if (selStrings.Count > 0)
            {
                foreach (var item in selStrings)
                {
                    dataString += item.Substring(item.Length - 1) + "|";
                }
            }

            dataString = dataString.Remove(dataString.Length - 1, 1);

            return dataString;
        }

        /// <summary>
        /// ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="lev"></param>
        private void CustomViewLongClick(object sender, View.LongClickEventArgs lev)
        {
            GetFile((View)sender);
        }

        private void HandlerCancelButton(object sender, DialogClickEventArgs e)
        {
        }

        private void HandlerOKButton(object sender, DialogClickEventArgs e)
        {
            var dialog = (Android.Support.V7.App.AlertDialog)sender;
            var input = dialog.FindViewById(5);
            string filename = ((EditText)input).Text;

            filename += ".slsl";

            string DefaultInitialDirectory = Android.App.Application.Context.GetExternalFilesDir(null) + Java.IO.File.Separator + "Looper" + Java.IO.File.Separator + "setlists" + Java.IO.File.Separator;

            string destinationFile = DefaultInitialDirectory + filename;

            if (!File.Exists(destinationFile))
            {
                var fs = File.Create(destinationFile);

                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                var dataString = CreateDataString();
                var byteArray = enc.GetBytes(dataString);
                fs.Write(byteArray, 0, byteArray.Length);

                fs.Close();
                m_trackListToSave.Clear();

                var intent = new Intent();
                
                OpenSetList(destinationFile);
            }
            else
            {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);

                alert.SetTitle("Schade !");
                alert.SetMessage("Es ist bereits eine Liste mit diesem Namen vorhanden.");

                alert.SetNeutralButton("OK", HandlerCancelButton);

                Dialog alertDialog = alert.Create();
                alertDialog.Show();
            }
        }

        private void InitChannels()
        {
            StopAllChannels();

            foreach(var slot in m_slots)
            {
                slot.Channel.Reset();
                slot.Channel.setVolume(0.5f);
                slot.VolumeSeekbar.Progress = 50;
                slot.LoopSlot = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curFile"></param>
        private void OpenSetList(string curFile)
        {
            if (Path.GetExtension(curFile) != ".slsl")
            {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);

                alert.SetTitle("Schade !");
                alert.SetMessage("Es handelt sich nicht um eine Setliste dieser App");

                alert.SetNeutralButton("OK", HandlerCancelButton);

                Dialog alertDialog = alert.Create();
                alertDialog.Show();
                return;
            }

            string destinationFile = curFile;
            if (System.IO.File.Exists(destinationFile))
            {
                InitChannels();

                var fs = System.IO.File.OpenRead(destinationFile);
                byte[] bytes = new byte[fs.Length];
                int numBytesToRead = (int)fs.Length;

                fs.Read(bytes, 0, numBytesToRead);
                fs.Close();

                var stringList = ReadDataToString(bytes);

                for (int i = 0; i < stringList.Count; i++)
                {
                    //    if (i >= 8)
                    //    {

                    //        var channelList = new List<string>();

                    //        for (int i = 8; i < stringList.Count; i++)
                    //        {
                    //            string tempString = "Channel " + stringList[i];
                    //            channelList.Add(tempString);
                    //        }

                    //        syncChannels.SetSelection(channelList);
                    //    }
                    //}

                    var slot = m_slots.Where(x => x.ChannelNbr == i+1).SingleOrDefault();

                    if (!string.IsNullOrEmpty(stringList[i]))
                    {
                        slot.Channel.loadTrack(stringList[i]);
                        if (slot.Channel.trackLoaded())
                        {
                            slot.IV_Button.SetImageResource(Resource.Drawable.orange_rect);
                            slot.TV_Title.Text = Path.GetFileNameWithoutExtension(stringList[i]);
                        }
                    }
                    else
                    {
                        slot.Channel = new ChannelNew(1, true);
                        slot.Channel.ChannelStoped += new ChannelNew.CallbackEventHandler(ChannelStoped);
                        slot.Channel.NewLoopStarts += new ChannelNew.CallbackEventHandler(ChannelRestart);
                        slot.IV_Button.SetImageResource(Resource.Drawable.red_rect);
                        slot.TV_Title.Text = "---";
                    }
                }
            
                syncChannels.ClearSelection();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void OpenSetList(Intent data)
        {
            string curFile;

            curFile = data.GetStringExtra("GetPath");

            if (curFile == null)
                return;

            OpenSetList(curFile);
        }

        /// <summary>
        /// ///
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="data"></param>
        private void ReactOnPath(int requestCode, Intent data)
        {
            if (requestCode != 1)
                return;

            string curFile;

            curFile = data.GetStringExtra("GetPath");

            if (curFile == null)
                return;

            var slot = m_slots.Where(x => x.ChannelNbr == selectedChannel).Single();

            slot.Channel.loadTrack(curFile);
            slot.Channel.Speed = 1;
            slot.Channel.Pitch = 1;
            slot.IV_Button.SetImageResource(Resource.Drawable.orange_rect);
            slot.TV_Title.Text = Path.GetFileNameWithoutExtension(curFile);
        }

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
        /// Stops all channels.
        /// </summary>
        private void StopAllChannels()
        {
            foreach(var slot in m_slots)
            {
                if (slot.Channel.isPlaying())
                    slot.Channel.stopMedia();
            }
        }

        /// <summary>
        /// ///
        /// </summary>
        /// <param name="channel"></param>
        private void StopChannel(int channel)
        {
            var slot = m_slots.Where(x => x.ChannelNbr == channel).Single();

            if (slot.Channel.trackLoaded() && slot.Channel.isPlaying())
            {
                slot.Channel.stopMedia();
                slot.IV_Button.SetImageResource(Resource.Drawable.orange_rect);
 
            }
        }
    }
}