//#define TRIAL
//#define FEATURE

using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using System.Threading;
using NAudio.Wave;
using System.Windows.Threading;
using System.IO;
using System.Linq;
using VoiceRecorder;
using GalaSoft.MvvmLight.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Input;
using Looper.About;

namespace Looper {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();

#if !FEATURE
            AVRButton.Visibility = Visibility.Collapsed;
            Chords.Visibility = Visibility.Collapsed;
#endif

#if TRIAL
            AVRButton.Visibility = Visibility.Collapsed;
            AllStartStop.Visibility = Visibility.Collapsed;
            SaveCh3.Visibility = Visibility.Collapsed;
            SaveCh4.Visibility = Visibility.Collapsed;
            SaveCh5.Visibility = Visibility.Collapsed;
            ClearCh3.Visibility = Visibility.Collapsed;
            ClearCh4.Visibility = Visibility.Collapsed;
            ClearCh5.Visibility = Visibility.Collapsed;
            EditCh3.Visibility = Visibility.Collapsed;
            EditCh4.Visibility = Visibility.Collapsed;
            EditCh5.Visibility = Visibility.Collapsed;
            VolCh3.Visibility = Visibility.Collapsed;
            VolCh4.Visibility = Visibility.Collapsed;
            VolCh5.Visibility = Visibility.Collapsed;
            VolDrum.Visibility = Visibility.Collapsed;
            RecCh3.Visibility = Visibility.Collapsed;
            RecCh4.Visibility = Visibility.Collapsed;
            RecCh5.Visibility = Visibility.Collapsed;
            RecDrum.Visibility = Visibility.Collapsed;
            ProgressCh3.Visibility = Visibility.Collapsed;
            ProgressCh4.Visibility = Visibility.Collapsed;
            ProgressCh5.Visibility = Visibility.Collapsed;
            ProgressDrum.Visibility = Visibility.Collapsed;
#endif


            InputController.Visibility = Visibility.Hidden;

            DispatcherHelper.Initialize();
            var vm = new MainWindowViewModel();
            InputController.DataContext = vm;

            for (int i = 0; i < 6; i++)
            {
                Channels[i] = new Channel ();
                Channels[i].audioOutput = new DirectSoundOut();
                Channels[i].Clip = null;
                Channels[i].TempFileName = "Ch" + i + ".wav";
                Channels[i].ChannelActive = true;
            }

            Channels[0].RecButton = RecCh1;
            Channels[1].RecButton = RecCh2;
            Channels[2].RecButton = RecCh3;
            Channels[3].RecButton = RecCh4;
            Channels[4].RecButton = RecCh5;
            Channels[5].RecButton = RecDrum;

            Channels[0].EditButton = EditCh1;
            Channels[1].EditButton = EditCh2;
            Channels[2].EditButton = EditCh3;
            Channels[3].EditButton = EditCh4;
            Channels[4].EditButton = EditCh5;

            Channels[0].VolumeSlider = VolCh1;
            Channels[1].VolumeSlider = VolCh2;
            Channels[2].VolumeSlider = VolCh3;
            Channels[3].VolumeSlider = VolCh4;
            Channels[4].VolumeSlider = VolCh5;
            Channels[5].VolumeSlider = VolDrum;

            Channels[0].ProgressBar = ProgressCh1;
            Channels[1].ProgressBar = ProgressCh2;
            Channels[2].ProgressBar = ProgressCh3;
            Channels[3].ProgressBar = ProgressCh4;
            Channels[4].ProgressBar = ProgressCh5;
            Channels[5].ProgressBar = ProgressDrum;

            Channels [0].Loaded = LoadedCh1;
            Channels [1].Loaded = LoadedCh2;
            Channels [2].Loaded = LoadedCh3;
            Channels [3].Loaded = LoadedCh4;
            Channels [4].Loaded = LoadedCh5;

            waveSource      = new WaveOut (); 

            waveSource.Volume = 0;

            LoadDevices ();
            LoadDrumKit ();
            
            this.MouseLeftButtonDown += delegate { DragMove (); };

            SerialController.InitSettings (ReceivedFromSerialPort, InputSelectionChanged);
            this.DataContext = this;

            this.PreviewKeyDown += new KeyEventHandler(Looper_KeyDown);
        }

        /// <summary>
        /// Event handler. Called by Looper for key down events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Key event information.
        /// </param>
        void Looper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                int channel = 0;

                if (EditCh1.ChannelEditStatus == Constants.Edit)
                    channel = 1;
                else if (EditCh2.ChannelEditStatus == Constants.Edit)
                    channel = 2;
                else if (EditCh2.ChannelEditStatus == Constants.Edit)
                    channel = 3;
                else if (EditCh2.ChannelEditStatus == Constants.Edit)
                    channel = 4;
                else if (EditCh2.ChannelEditStatus == Constants.Edit)
                    channel = 5;

                if (channel != 0)
                {
                    PlayRecordButton(channel);
                    e.Handled = true;
                }
            }

            if (e.Key == Key.Up)
            {
                Poti_OutputLevel.VolumeUp();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Down)
            {
                Poti_OutputLevel.VolumeDown();
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// Gets the is trial version.
        /// </summary>
        /// <value>
        /// The is trial version.
        /// </value>
        public Visibility IsTrialVersion
        {
            get
            {
#if TRIAL
                return Visibility.Hidden;
#else
                return Visibility.Visible;
#endif
            }
        }

        /// <summary>
        /// Gets the is feature version.
        /// </summary>
        /// <value>
        /// The is feature version.
        /// </value>
        public Visibility IsFeatureVersion {
            get
            {
#if FEATURE
                return Visibility.Visible;
#else
                return Visibility.Hidden;
#endif
            }
        }

        /// <summary>
        /// Lädt die Liste der verfügbaren Eingabegeräte
        /// </summary>
        private void LoadDevices ()
        {
            List<NAudio.Wave.WaveInCapabilities> sources = new List<NAudio.Wave.WaveInCapabilities>();
            for (int i = 0; i < NAudio.Wave.WaveIn.DeviceCount; i++)
            {
                sources.Add (NAudio.Wave.WaveIn.GetCapabilities (i));
            }
            SerialController.ClearSourceList ();
            foreach (var source in sources)
            {
                string item = source.Channels.ToString () + " " + source.ProductName;
                SerialController.AddSourceListItem (item);
            }
        }

        /// <summary>
        /// Lädt die Drumkits aus dem Verzeichnis in die ComboBox "DrumKit"
        /// </summary>
        private void LoadDrumKit ()
        {
            string folder = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            folder += "\\Drum";

            if (!Directory.Exists(folder))
            {
                try
                { Directory.CreateDirectory(folder); }
                catch
                { return; }

            }

            var files = Directory.EnumerateFiles(folder).OrderByDescending(filename => filename);

            foreach (var file in files)
            {
                string filename = Path.GetFileName(file.ToString());
                DrumKit.Items.Add (filename);
            }
        }

        /// <summary>
        /// Übernimmt die Auswahl des Eingabegerätes
        /// </summary>
        private void InputSelectionChanged (int channel)
        {
            selectedDeviceChannel = channel;
        }
        
        /// <summary>
        /// Übernimmt die Auswahl des Drumkits
        /// </summary>
        private void DrumList_SelectionChanged (object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string file = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            file += "\\Drum\\";
            file += DrumKit.SelectedItem.ToString ();

            Channels [5].Clip = new WavePlayer(file);
            Channels [5].Clip.Channel.Volume = (float)VolDrum.CustomValue;

            Channels[5].audioOutput.Init(Channels[5].Clip.Channel);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                         //
        // Play / Record Buttons                                                                   //
        //                                                                                         //
        /////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reaktion auf den PlayButton
        /// </summary>
        private void PlayRecordButton( int Channel )
        {
            --Channel;

            short                       status;
            short                       editStatus;
            ThreadStart                 ChThreadStart = null;

            status          = Channels[Channel].RecButton.ChannelStatus;
            editStatus      = Channels[Channel].EditButton.ChannelEditStatus;

            // No Edit, Wav File geladen und spielt gerade
            if (editStatus == Constants.NoEdit && Channels[Channel].Clip != null && status == Constants.Play)
            {
                Channels[Channel].RecButton.ChannelStatus = Constants.Idle;
                Channels[Channel].audioOutput.Stop ();
                Channels[Channel].runChannel = false;

                if (Channels[Channel].progressThread != null)
                    Channels[Channel].progressThread.Suspend();
            }

            // No Edit, Wav File geladen und spielt gerade nicht
            if (editStatus == Constants.NoEdit && Channels[Channel].Clip != null && status == Constants.Idle)
            {
                if (Channel == 0)
                    ChThreadStart = new ThreadStart(SetProgressCh1);
                else if (Channel == 1)
                    ChThreadStart = new ThreadStart(SetProgressCh2);
                else if (Channel == 2)
                    ChThreadStart = new ThreadStart(SetProgressCh3);
                else if (Channel == 3)
                    ChThreadStart = new ThreadStart(SetProgressCh4);
                else if (Channel == 4)
                    ChThreadStart = new ThreadStart(SetProgressCh5);

                Channels[Channel].RecButton.ChannelStatus = Constants.Play;
                Channels[Channel].audioOutput.Play();
                Channels[Channel].progressThread = new System.Threading.Thread(ChThreadStart);
                Channels[Channel].progressThread.IsBackground = true;
                Channels[Channel].runChannel = true;
                Channels[Channel].progressThread.Start();

                Chords.Position = 500;
            }

            // Edit, Start Record
            if (editStatus == Constants.Edit && status == Constants.Idle)
            {
                if (selectedDeviceChannel == -1)
                {
                    MessageBox.Show("Please select an input device", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (Channels[Channel].Clip != null)
                {
                    Channels[Channel].Clip.Dispose();
                    Channels[Channel].Clip = null;
                    Channels[Channel].audioOutput.Dispose();
                }

                if (Channels [Channel].progressThread != null)
                {
                    Channels [Channel].runChannel = false;
                    Channels [Channel].ProgressBar.ActPosition = 0;
                }
                Channels [Channel].progressThread = null;

                AudioRecorder = new Audio(selectedDeviceChannel);
                AudioRecorder.StartRecord(Channels[Channel].TempFileName);
                Channels[Channel].RecButton.ChannelStatus = Constants.Rec;
            }

            if (editStatus == Constants.Edit && status == Constants.Rec)
            {
                if (Channel == 0)
                    AudioRecorder.RecordingStopped += AudioRecorder_RecordingStopped_Ch1;
                else if (Channel == 1)
                    AudioRecorder.RecordingStopped += AudioRecorder_RecordingStopped_Ch2;
                else if (Channel == 2)
                    AudioRecorder.RecordingStopped += AudioRecorder_RecordingStopped_Ch3;
                else if (Channel == 3)
                    AudioRecorder.RecordingStopped += AudioRecorder_RecordingStopped_Ch4;
                else if (Channel == 4)
                    AudioRecorder.RecordingStopped += AudioRecorder_RecordingStopped_Ch5;

                AudioRecorder.StopRecord();
                Channels[Channel].RecButton.ChannelStatus = Constants.Idle;

                if (Channels[Channel].Clip != null)
                {
                    Channels[Channel].Loaded.ChannelLoadedStatus = Constants.Loaded;
                    // Wiedergabe starten ( ooder in RecordingStooped ? )
                    if (Channel == 0)
                        ChThreadStart = new ThreadStart(SetProgressCh1);
                    else if (Channel == 1)
                        ChThreadStart = new ThreadStart(SetProgressCh2);
                    else if (Channel == 2)
                        ChThreadStart = new ThreadStart(SetProgressCh3);
                    else if (Channel == 3)
                        ChThreadStart = new ThreadStart(SetProgressCh4);
                    else if (Channel == 4)
                        ChThreadStart = new ThreadStart(SetProgressCh5);
                   
                    Channels[Channel].RecButton.ChannelStatus = Constants.Play;
                    Channels[Channel].audioOutput.Play();
                    Channels[Channel].progressThread = new System.Threading.Thread(ChThreadStart);
                    Channels[Channel].progressThread.IsBackground = true;
                    Channels[Channel].runChannel = true;
                    Channels[Channel].progressThread.Start();
                }
                else
                    Channels[Channel].Loaded.ChannelLoadedStatus = Constants.NotLoaded;
            }            
        }

        /// <summary>
        /// Aufnahme wurde gestoppt
        /// </summary>
        private void RecordingStopped ( int Channel)
        {
            --Channel;
            Channels[Channel].Clip                  = new WavePlayer(AudioRecorder.FileName);
            Channels[Channel].Clip.Channel.Volume   = ((float)Channels[Channel].VolumeSlider.CustomValue);

            Channels[Channel].audioOutput.Init(Channels[Channel].Clip.Channel);

            if (Channels[Channel].Clip != null)
            {
                Channels[Channel].Loaded.ChannelLoadedStatus = Constants.Loaded;
            }
            else
                Channels[Channel].Loaded.ChannelLoadedStatus = Constants.NotLoaded;
        }

        /// <summary>
        /// Reagiert auf den Klick auf den Play / Record Button des Kanals 1
        /// </summary>
        private void RecCh1_CustomClick (object sender, RoutedEventArgs e)
        {
            PlayRecordButton (1);
        }

        /// <summary>
        /// Event Message, wenn die Aufnahme beendet wurde auf dem Kanal 1
        /// </summary>
        private void AudioRecorder_RecordingStopped_Ch1(object sender, RoutedEventArgs e)
        {
            RecordingStopped (1);
            EditCh1_CustomEditClick(null, null);
        }

        /// <summary>
        /// Reagiert auf den Klick auf den Play / Record Button des Kanals 2
        /// </summary>
        private void RecCh2_CustomClick(object sender, RoutedEventArgs e)
        {
            PlayRecordButton (2);
        }

        /// <summary>
        /// Event Message, wenn die Aufnahme beendet wurde auf dem Kanal 2
        /// </summary>
        private void AudioRecorder_RecordingStopped_Ch2(object sender, RoutedEventArgs e)
        {
            RecordingStopped (2);
            EditCh2_CustomEditClick(null, null);
        }

        /// <summary>
        /// Reagiert auf den Klick auf den Play / Record Button des Kanals 3
        /// </summary>
        private void RecCh3_CustomClick(object sender, RoutedEventArgs e)
        {
            PlayRecordButton (3);
        }

        /// <summary>
        /// Event Message, wenn die Aufnahme beendet wurde auf dem Kanal 3
        /// </summary>
        private void AudioRecorder_RecordingStopped_Ch3(object sender, RoutedEventArgs e)
        {
            RecordingStopped (3);
            EditCh3_CustomEditClick(null, null);
        }

        /// <summary>
        /// Reagiert auf den Klick auf den Play / Record Button des Kanals 4
        /// </summary>
        private void RecCh4_CustomClick (object sender, RoutedEventArgs e)
        {
            PlayRecordButton (4);
        }

        /// <summary>
        /// Event Message, wenn die Aufnahme beendet wurde auf dem Kanal 4
        /// </summary>
        private void AudioRecorder_RecordingStopped_Ch4(object sender, RoutedEventArgs e)
        {
            RecordingStopped (4);
            EditCh4_CustomEditClick(null, null);
        }

        /// <summary>
        /// Reagiert auf den Klick auf den Play / Record Button des Kanals 5
        /// </summary>
        private void RecCh5_CustomClick(object sender, RoutedEventArgs e)
        {
            PlayRecordButton (5);
        }

        /// <summary>
        /// Event Message, wenn die Aufnahme beendet wurde auf dem Kanal 5
        /// </summary>
        private void AudioRecorder_RecordingStopped_Ch5(object sender, RoutedEventArgs e)
        {
            RecordingStopped (5);
            EditCh5_CustomEditClick(null, null);
        }

        /// <summary>
        /// Reagiert auf den Klick auf den Play / Record Button des Kanals 5
        /// </summary>
        private void RecDrum_CustomClick (object sender, RoutedEventArgs e)
        {
            short status        = RecDrum.ChannelStatus;

            // No Edit, Wav File geladen und spielt gerade
            if (Channels[5].Clip != null && status == Constants.Play)
            {
                Channels[5].RecButton.ChannelStatus = Constants.Idle;
                Channels[5].runChannel = false;
                Channels[5].audioOutput.Stop();
                Channels[5].progressThread.Suspend();
            }

            // No Edit, Wav File geladen und spielt gerade nicht
            if (Channels[5].Clip != null && status == Constants.Idle)
            {
                RecDrum.ChannelStatus = Constants.Play;
                Channels[5].audioOutput.Play ();
                Channels[5].runChannel = true;
                Channels[5].progressThread = new System.Threading.Thread(new ThreadStart(SetProgressDrum));
                Channels[5].progressThread.IsBackground = true;
                Channels[5].progressThread.Start ();
            }
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////////
        ///                                                                                          //
        ///  Save Buttons                                                                            //
        ///                                                                                          //
        /// ///////////////////////////////////////////////////////////////////////////////////////////

        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        private void SaveChannel(short channel)
        {
            if (Channels[channel].Clip == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "WAV file (.wav)|*.wav",
                DefaultExt = ".wav"
            };
            bool? result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                if (File.Exists(saveFileDialog.FileName))
                    File.Delete(saveFileDialog.FileName);

                File.Copy(Channels[channel].Clip.FileName, saveFileDialog.FileName);
            }
        }

        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        private void SaveCh1_Click(object sender, RoutedEventArgs e)
        {
            SaveChannel(0);
        }

        /// <summary>
        /// Event handler. Called by ClearCh2 for click events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        private void SaveCh2_Click(object sender, RoutedEventArgs e)
        {
            SaveChannel(1);
        }

        /// <summary>
        /// Event handler. Called by ClearCh3 for click events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        private void SaveCh3_Click(object sender, RoutedEventArgs e)
        {
            SaveChannel(2);
        }

        /// <summary>
        /// Event handler. Called by ClearCh4 for click events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        private void SaveCh4_Click(object sender, RoutedEventArgs e)
        {
            SaveChannel(3);
        }

        /// <summary>
        /// Event handler. Called by ClearCh5 for click events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        private void SaveCh5_Click(object sender, RoutedEventArgs e)
        {
            SaveChannel(4);
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////////
        ///                                                                                          //
        ///  Clear Buttons                                                                           //
        ///                                                                                          //
        /// ///////////////////////////////////////////////////////////////////////////////////////////

        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        private void ClearChannel(short channel)
        {
            var result = MessageBox.Show("Are you sure ?", "Clear Channel", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            if (Channels[channel].RecButton.ChannelStatus == Constants.Play)
                return;

            if (Channels[channel].ProgressBar != null)
                Channels[channel].ProgressBar.ActPosition = 0;

            if (Channels[channel].Loaded != null)
                Channels[channel].Loaded.ChannelLoadedStatus = Constants.NotLoaded;

            Channels[channel] = new Channel();
            Channels[channel].audioOutput = new DirectSoundOut();
            Channels[channel].Clip = null;
            Channels[channel].TempFileName = "Ch" + 0 + ".wav";
            Channels[channel].ChannelActive = true;

            switch (channel)
            {
                case 0:
                    {
                        Channels[channel].RecButton = RecCh1;                      
                        Channels[channel].EditButton = EditCh1;
                        Channels[channel].VolumeSlider = VolCh1;
                        Channels[channel].ProgressBar = ProgressCh1;
                        Channels[channel].Loaded = LoadedCh1;
                    }
                    break;
                case 1:
                    {
                        Channels[channel].RecButton = RecCh2;
                        Channels[channel].EditButton = EditCh2;
                        Channels[channel].VolumeSlider = VolCh2;
                        Channels[channel].ProgressBar = ProgressCh2;
                        Channels[channel].Loaded = LoadedCh2;
                    }
                    break;
                case 2:
                    {
                        Channels[channel].RecButton = RecCh3;
                        Channels[channel].EditButton = EditCh3;
                        Channels[channel].VolumeSlider = VolCh3;
                        Channels[channel].ProgressBar = ProgressCh3;
                        Channels[channel].Loaded = LoadedCh3;
                    }
                    break;
                case 3:
                    {
                        Channels[channel].RecButton = RecCh4;
                        Channels[channel].EditButton = EditCh4;
                        Channels[channel].VolumeSlider = VolCh4;
                        Channels[channel].ProgressBar = ProgressCh4;
                        Channels[channel].Loaded = LoadedCh4;
                    }
                    break;
                case 4:
                    {
                        Channels[channel].RecButton = RecCh5;
                        Channels[channel].EditButton = EditCh5;
                        Channels[channel].VolumeSlider = VolCh5;
                        Channels[channel].ProgressBar = ProgressCh5;
                        Channels[channel].Loaded = LoadedCh5;
                    }
                    break;
            }

            if (Channels[channel].ProgressBar != null)
                Channels[channel].ProgressBar.ActPosition = 0;

            if (Channels[channel].Loaded != null)
                Channels[channel].Loaded.ChannelLoadedStatus = Constants.NotLoaded;
        }

        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        private void ClearCh1_Click(object sender, RoutedEventArgs e)
        {
            ClearChannel(0);
        }

        /// <summary>
        /// Event handler. Called by ClearCh2 for click events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        private void ClearCh2_Click(object sender, RoutedEventArgs e)
        {
            ClearChannel(1);
        }

        /// <summary>
        /// Event handler. Called by ClearCh3 for click events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        private void ClearCh3_Click(object sender, RoutedEventArgs e)
        {
            ClearChannel(2);
        }

        /// <summary>
        /// Event handler. Called by ClearCh4 for click events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        private void ClearCh4_Click(object sender, RoutedEventArgs e)
        {
            ClearChannel(3);
        }

        /// <summary>
        /// Event handler. Called by ClearCh5 for click events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        private void ClearCh5_Click(object sender, RoutedEventArgs e)
        {
            ClearChannel(4);
        }


        /////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                         //
        // Edit Buttons                                                                            //
        //                                                                                         //
        /////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reagiert auf den Klick auf den Edit des Kanals 1
        /// </summary>
        private void EditCh1_CustomEditClick (object sender, RoutedEventArgs e)
        {
            if (RecCh1.ChannelStatus != Constants.Idle)
                return;

            if (RecCh2.ChannelStatus == Constants.Rec ||
                RecCh3.ChannelStatus == Constants.Rec ||
                RecCh4.ChannelStatus == Constants.Rec ||
                RecCh5.ChannelStatus == Constants.Rec)
                return;

            short status = EditCh1.ChannelEditStatus;

            if (status == Constants.NoEdit) // No edit
            {
                EditCh1.ChannelEditStatus = Constants.Edit;
                EditCh2.ChannelEditStatus = Constants.NoEdit;
                EditCh3.ChannelEditStatus = Constants.NoEdit;
                EditCh4.ChannelEditStatus = Constants.NoEdit;
                EditCh5.ChannelEditStatus = Constants.NoEdit;
            } else if (status == Constants.Edit) // Edit
            {
                EditCh1.ChannelEditStatus = Constants.NoEdit;
            }
        }

        /// <summary>
        /// Reagiert auf den Klick auf den Edit des Kanals 2
        /// </summary>
        private void EditCh2_CustomEditClick (object sender, RoutedEventArgs e)
        {
            if (RecCh2.ChannelStatus != Constants.Idle)
                return;

            if (RecCh1.ChannelStatus == Constants.Rec ||
                RecCh3.ChannelStatus == Constants.Rec ||
                RecCh4.ChannelStatus == Constants.Rec ||
                RecCh5.ChannelStatus == Constants.Rec)
                return;

            short status = EditCh2.ChannelEditStatus;

            if (status == Constants.NoEdit) // No edit
            {
                EditCh2.ChannelEditStatus = Constants.Edit;
                EditCh1.ChannelEditStatus = Constants.NoEdit;
                EditCh3.ChannelEditStatus = Constants.NoEdit;
                EditCh4.ChannelEditStatus = Constants.NoEdit;
                EditCh5.ChannelEditStatus = Constants.NoEdit;
            } else if (status == Constants.Edit) // Edit
            {
                EditCh2.ChannelEditStatus = Constants.NoEdit;
            }
        }

        /// <summary>
        /// Reagiert auf den Klick auf den Edit des Kanals 3
        /// </summary>
        private void EditCh3_CustomEditClick (object sender, RoutedEventArgs e)
        {
            if (RecCh3.ChannelStatus != Constants.Idle)
                return;

            if (RecCh1.ChannelStatus == Constants.Rec ||
                RecCh2.ChannelStatus == Constants.Rec ||
                RecCh4.ChannelStatus == Constants.Rec ||
                RecCh5.ChannelStatus == Constants.Rec)
                return;

            short status = EditCh3.ChannelEditStatus;

            if (status == Constants.NoEdit) // No edit
            {
                EditCh3.ChannelEditStatus = Constants.Edit;
                EditCh1.ChannelEditStatus = Constants.NoEdit;
                EditCh2.ChannelEditStatus = Constants.NoEdit;
                EditCh4.ChannelEditStatus = Constants.NoEdit;
                EditCh5.ChannelEditStatus = Constants.NoEdit;
            } else if (status == Constants.Edit) // Edit
            {
                EditCh3.ChannelEditStatus = Constants.NoEdit;
            }
        }

        /// <summary>
        /// Reagiert auf den Klick auf den Edit des Kanals 4
        /// </summary>
        private void EditCh4_CustomEditClick (object sender, RoutedEventArgs e)
        {
            if (RecCh4.ChannelStatus != Constants.Idle)
                return;

            if (RecCh1.ChannelStatus == Constants.Rec ||
                RecCh2.ChannelStatus == Constants.Rec ||
                RecCh3.ChannelStatus == Constants.Rec ||
                RecCh5.ChannelStatus == Constants.Rec)
                return;

            short status = EditCh4.ChannelEditStatus;

            if (status == Constants.NoEdit) // No edit
            {
                EditCh4.ChannelEditStatus = Constants.Edit;
                EditCh3.ChannelEditStatus = Constants.NoEdit;
                EditCh1.ChannelEditStatus = Constants.NoEdit;
                EditCh2.ChannelEditStatus = Constants.NoEdit;
                EditCh5.ChannelEditStatus = Constants.NoEdit;
            } else if (status == Constants.Edit) // Edit
            {
                EditCh4.ChannelEditStatus = Constants.NoEdit;
            }
        }

        /// <summary>
        /// Reagiert auf den Klick auf den Edit des Kanals 5
        /// </summary>
        private void EditCh5_CustomEditClick (object sender, RoutedEventArgs e)
        {
            if (RecCh5.ChannelStatus != Constants.Idle)
                return;

            if (RecCh1.ChannelStatus == Constants.Rec ||
                RecCh2.ChannelStatus == Constants.Rec ||
                RecCh3.ChannelStatus == Constants.Rec ||
                RecCh4.ChannelStatus == Constants.Rec)
                return;

            short status = EditCh5.ChannelEditStatus;

            if (status == Constants.NoEdit) // No edit
            {
                EditCh5.ChannelEditStatus = Constants.Edit;
                EditCh1.ChannelEditStatus = Constants.NoEdit;
                EditCh2.ChannelEditStatus = Constants.NoEdit;
                EditCh3.ChannelEditStatus = Constants.NoEdit;
                EditCh4.ChannelEditStatus = Constants.NoEdit;
            } else if (status == Constants.Edit) // Edit
            {
                EditCh5.ChannelEditStatus = Constants.NoEdit;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                         //
        // Volume slider                                                                           //
        //                                                                                         //
        /////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reagiert auf die Änderung der Lautstärke
        /// </summary>
        private void CustomVolumChange (int Channel)
        {
            --Channel;

            if (Channels[Channel].Clip == null)
                return;

            Channels[Channel].Clip.Channel.Volume = (float)Channels[Channel].VolumeSlider.CustomValue; 
        }
    
        /// <summary>
        /// Reagiert auf die Änderung der Lautstärke des Kanals 1
        /// </summary>
        private void RecCh1_CustomVolumeChange (object sender, RoutedEventArgs e)
        {
            CustomVolumChange(1);
        }

        /// <summary>
        /// Reagiert auf die Änderung der Lautstärke des Kanals 2
        /// </summary>
        private void RecCh2_CustomVolumeChange (object sender, RoutedEventArgs e)
        {
            CustomVolumChange(2);
        }

        /// <summary>
        /// Reagiert auf die Änderung der Lautstärke des Kanals 3
        /// </summary>
        private void RecCh3_CustomVolumeChange (object sender, RoutedEventArgs e)
        {
            CustomVolumChange(3);
        }

        /// <summary>
        /// Reagiert auf die Änderung der Lautstärke des Kanals 4
        /// </summary>
        private void RecCh4_CustomVolumeChange (object sender, RoutedEventArgs e)
        {
            CustomVolumChange(4);
        }

        /// <summary>
        /// Reagiert auf die Änderung der Lautstärke des Kanals 5
        /// </summary>
        private void RecCh5_CustomVolumeChange (object sender, RoutedEventArgs e)
        {
            CustomVolumChange(5);
        }

        /// <summary>
        /// Reagiert auf die Änderung der Lautstärke des Kanals 5
        /// </summary>
        private void Drum_CustomVolumeChange (object sender, RoutedEventArgs e)
        {
            CustomVolumChange(6);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                         //
        // Right click Play / Record Buttons                                                       //
        //                                                                                         //
        /////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Läft eine Datei in einen Kanal
        /// </summary>
        private void LoadFileOnChannel ( int Channel )
        {
            string      filename;

            --Channel;
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".wav";
            dlg.Filter = "Wave (.wav)|*.wav";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
                filename = dlg.FileName;
            else
                return;

            Channels[Channel].Clip = new WavePlayer(filename);
            Channels[Channel].Clip.Channel.Volume = (float)Channels[Channel].VolumeSlider.CustomValue;
            Channels[Channel].RecButton.ChannelStatus = Constants.Idle;
            Channels[Channel].audioOutput.Init(Channels[Channel].Clip.Channel);
            Channels [Channel].runChannel = false;
            Channels [Channel].Loaded.ChannelLoadedStatus = Constants.Loaded;

            Chords.Load(filename);
        }

        /// <summary>
        /// Reagiert auf den Rechts-Klick auf den Play / Record Button des Kanals 1
        /// Es kann eine wave Datei geladen werden
        /// </summary>
        private void RecCh1_CustomClickRight (object sender, RoutedEventArgs e)
        {
            LoadFileOnChannel (1);
        }

        /// <summary>
        /// Reagiert auf den Rechts-Klick auf den Play / Record Button des Kanals 2
        /// Es kann eine wave Datei geladen werden
        /// </summary>
        private void RecCh2_CustomClickRight (object sender, RoutedEventArgs e)
        {
            LoadFileOnChannel (2);
        }

        /// <summary>
        /// Reagiert auf den Rechts-Klick auf den Play / Record Button des Kanals 3
        /// Es kann eine wave Datei geladen werden
        /// </summary>
        private void RecCh3_CustomClickRight (object sender, RoutedEventArgs e)
        {
            LoadFileOnChannel (3);
        }

        /// <summary>
        /// Reagiert auf den Rechts-Klick auf den Play / Record Button des Kanals 4
        /// Es kann eine wave Datei geladen werden
        /// </summary>
        private void RecCh4_CustomClickRight (object sender, RoutedEventArgs e)
        {
            LoadFileOnChannel (4);
        }


        /// <summary>
        /// Reagiert auf den Rechts-Klick auf den Play / Record Button des Kanals 5
        /// Es kann eine wave Datei geladen werden
        /// </summary>
        private void RecCh5_CustomClickRight (object sender, RoutedEventArgs e)
        {
            LoadFileOnChannel (5);

        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                         //
        // Stop Buttons                                                                            //
        //                                                                                         //
        /////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Reagiert wenn die rechte Maustaste losgelassen wird.
        /// </summary>
        /// <param name="Channel"></param>
        private new void MouseRightButtonUp (int Channel)
        {
            --Channel;

            if (Channels[Channel].RecButton.ChannelStatus != Constants.Idle)
                return;

            StopChannel(Channel+1);
            Channels[Channel].ProgressBar.ActPosition = 0;
            Channels[Channel].progressThread = null;
            if (Channels[Channel].Clip != null)
                Channels[Channel].Clip.Dispose();
            Channels[Channel].Clip = null;
            Channels[Channel].audioOutput.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        private void StopCh1_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MouseRightButtonUp(1);
        }

        /// <summary>
        /// 
        /// </summary>
        private void StopCh2_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MouseRightButtonUp(2);
        }

        /// <summary>
        /// 
        /// </summary>
        private void StopCh3_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MouseRightButtonUp(3);
        }

        /// <summary>
        /// 
        /// </summary>
        private void StopCh4_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MouseRightButtonUp(4);
        }

        /// <summary>
        /// 
        /// </summary>
        private void StopCh5_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MouseRightButtonUp(5);
        }

        /// <summary>
        /// 
        /// </summary>
        private void StopDrum_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MouseRightButtonUp(6);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Channel"></param>
        private void StopChannel ( int Channel )
        { 
            --Channel;

            if (Channels[Channel].Clip == null)
                return;

            Channels[Channel].audioOutput.Stop();
            if (Channels[Channel].progressThread != null)
            {
                Channels[Channel].runChannel = false;
                Channels[Channel].ProgressBar.ActPosition = 0;
            }
            Channels[Channel].progressThread = null;
            Channels[Channel].Clip.Channel.Position = 0;
            Channels[Channel].RecButton.ChannelStatus = Constants.Idle;
        }
        
        private void RecCh1_CustomStopClick (object sender, RoutedEventArgs e)
        {
            StopChannel (1);
        }

        private void RecCh2_CustomStopClick (object sender, RoutedEventArgs e)
        {
            StopChannel (2);
        }

        private void RecCh3_CustomStopClick (object sender, RoutedEventArgs e)
        {
            StopChannel (3);
        }

        private void RecCh4_CustomStopClick (object sender, RoutedEventArgs e)
        {
            StopChannel (4);
        }

        private void RecCh5_CustomStopClick (object sender, RoutedEventArgs e)
        {
            StopChannel (5);
        }

        private void RecCh6_CustomStopClick (object sender, RoutedEventArgs e)
        {
            StopChannel (6);
        }
        

        /////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                         //
        // Diverse Buttons                                                                         //
        //                                                                                         //
        /////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        private void Grid_Loaded (object sender, RoutedEventArgs e)
{
            this.MouseDown += delegate { DragMove (); };
        }

        /// <summary>
        /// 
        /// </summary>
        private void Button_Close_Click (object sender, RoutedEventArgs e)
        {
            Close ();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ButtonVoiceRec_Click (object sender, RoutedEventArgs e)
        {
            OutputRecorder.Visibility = Visibility.Hidden;
            SerialController.Visibility = Visibility.Hidden;

            if (InputController.Visibility == Visibility.Hidden)
                InputController.Visibility = Visibility.Visible;
            else
                InputController.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Button_Minimize_Click (object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        /// <summary>
        /// Event handler. Called by Button_About for click events.
        /// </summary>
        /// <param name="sender">
        /// Source of the event.
        /// </param>
        /// <param name="e">
        /// Routed event information.
        /// </param>
        private void Button_About_Click (object sender, RoutedEventArgs e)
        {
            var aboutwin = new AboutWin();
            aboutwin.Show();
        }

        /// <summary>
        /// Reagiert auf den PlayAll Button
        /// </summary>
        private void ClickPlayAll (object sender, RoutedEventArgs e)
        {
            short status = AllStartStop.PlayAllStatus;

            if (status == Constants.StopAll) // Play
            {
                if (RecCh1.ChannelStatus    != Constants.Idle ||
                EditCh1.ChannelEditStatus   != Constants.NoEdit ||
                RecCh2.ChannelStatus        != Constants.Idle ||
                EditCh2.ChannelEditStatus   != Constants.NoEdit ||
                RecCh3.ChannelStatus        != Constants.Idle ||
                EditCh3.ChannelEditStatus   != Constants.NoEdit ||
                RecCh4.ChannelStatus        != Constants.Idle ||
                EditCh4.ChannelEditStatus   != Constants.NoEdit ||
                RecCh5.ChannelStatus        != Constants.Idle ||
                EditCh5.ChannelEditStatus   != Constants.NoEdit ||
                RecDrum.ChannelStatus       != Constants.Idle)
                    return;

                AllStartStop.PlayAllStatus = Constants.PlayAll;

                StopChannel (1);
                StopChannel (2);
                StopChannel (3);
                StopChannel (4);
                StopChannel (5);
                StopChannel (6);
                
                if (Channels[0].ChannelActive)
                    RecCh1_CustomClick  (null, null);
                if (Channels[1].ChannelActive)
                    RecCh2_CustomClick  (null, null);
                if (Channels[2].ChannelActive)
                    RecCh3_CustomClick  (null, null);
                if (Channels[3].ChannelActive)
                    RecCh4_CustomClick  (null, null);
                if (Channels[4].ChannelActive)
                    RecCh5_CustomClick  (null, null);
                if (Channels[5].ChannelActive)
                    RecDrum_CustomClick (null, null);

            } else if (status == Constants.PlayAll) // Stop
            {
                AllStartStop.PlayAllStatus = Constants.StopAll;

                for(int i=1; i<7; i++)
                {
                    StopChannel (i);
                    Channels[i - 1].runChannel = false;
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                         //
        // Potis                                                                                   //
        //                                                                                         //
        /////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Ändert die Lautstärke des Masterausgangs
        /// </summary>
        private void OutputLevel_ValueChange (object sender, RoutedEventArgs e)
        {
            float temp = Poti_OutputLevel.PotiPosInPercent;

            if (temp < 0)
            {
                Poti_OutputLevel.PotiPosInPercent = 0;
                temp = 0;
            }

            if (temp > 100)
            {
                Poti_OutputLevel.PotiPosInPercent = 100;
                temp = 100;
            }

            waveSource.Volume = temp/(float)100;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                         //
        // Progress Bars                                                                           //
        //                                                                                         //
        /////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        private void SetProgressCh1 ()
        {
            while (Channels[0].runChannel)
            {
                Thread.Sleep (100);
                this.Dispatcher.Invoke ((Action) (() =>
                {
                    int pos = (int) ((100 / Channels[0].Clip.Channel.TotalTime.TotalSeconds) * Channels[0].Clip.Channel.CurrentTime.TotalSeconds);
                    if (pos > 100)
                        pos = pos - (pos / 100) * 100;

                    ProgressCh1.ActPosition = pos;
                }), DispatcherPriority.Send);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetProgressCh2 ()
        {
            while (Channels[1].runChannel)
            {
                Thread.Sleep (100);
                this.Dispatcher.Invoke ((Action) (() =>
                {
                    int pos = (int) ((100 / Channels[1].Clip.Channel.TotalTime.TotalSeconds) * Channels[1].Clip.Channel.CurrentTime.TotalSeconds);
                    if (pos > 100)
                        pos = pos - (pos / 100) * 100;
                    ProgressCh2.ActPosition = pos;
                }), DispatcherPriority.Send);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetProgressCh3 ()
        {
            while (Channels[2].runChannel)
            {
                Thread.Sleep (100);
                this.Dispatcher.Invoke ((Action) (() =>
                {
                    int pos = (int) ((100 / Channels[2].Clip.Channel.TotalTime.TotalSeconds) * Channels[2].Clip.Channel.CurrentTime.TotalSeconds);
                    if (pos > 100)
                        pos = pos - (pos / 100) * 100;
                    ProgressCh3.ActPosition = pos;
                }), DispatcherPriority.Send);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetProgressCh4 ()
        {
            while (Channels[3].runChannel)
            {
                Thread.Sleep (100);
                this.Dispatcher.Invoke ((Action) (() =>
                {
                    int pos = (int) ((100 / Channels[3].Clip.Channel.TotalTime.TotalSeconds) * Channels[3].Clip.Channel.CurrentTime.TotalSeconds);
                    if (pos > 100)
                        pos = pos - (pos / 100) * 100;
                    ProgressCh4.ActPosition = pos;
                }));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetProgressCh5 ()
        {
            while (Channels[4].runChannel)
            {
                Thread.Sleep (100);
                this.Dispatcher.Invoke ((Action) (() =>
                {
                    int pos = (int) ((100 / Channels[4].Clip.Channel.TotalTime.TotalSeconds) * Channels[4].Clip.Channel.CurrentTime.TotalSeconds);
                    if (pos > 100)
                        pos = pos - (pos / 100) * 100;
                    ProgressCh5.ActPosition = pos;
                }), DispatcherPriority.Send);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetProgressDrum ()
        {
            while (Channels[5].runChannel && ProgressDrum != null)
            {
                Thread.Sleep (100);
                this.Dispatcher.Invoke ((Action) (() =>
                {
                    int pos = (int) ((100 / Channels[5].Clip.Channel.TotalTime.TotalSeconds) * Channels[5].Clip.Channel.CurrentTime.TotalSeconds);
                    if (pos > 100)
                        pos = pos - (pos / 100) * 100;
                    ProgressDrum.ActPosition = pos;
                }), DispatcherPriority.Send);
            }
        }

        private int m_activeChannel = 1;

        /// <summary>
        /// 
        /// </summary>
        private void ReceivedFromSerialPort (string serialData)
        {
            if (serialData.Contains("LooperRemote/B1"))
            {
                string remoteData = serialData.Remove(0, 16);

                int milliseconds = Int32.Parse(remoteData);

                if (milliseconds > 1000)
                {

                    switch (m_activeChannel)
                    {
                        case 1:
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() => this.EditCh1_CustomEditClick(null, null)));
                            }
                            break;
                        case 2:
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() => this.EditCh2_CustomEditClick(null, null)));
                            }
                            break;
                        case 3:
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() => this.EditCh3_CustomEditClick(null, null)));
                            }
                            break;
                        case 4:
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() => this.EditCh5_CustomEditClick(null, null)));
                            }
                            break;
                        case 5:
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() => this.EditCh5_CustomEditClick(null, null)));
                            }
                            break;
                    }
                }
                else
                {
                    switch(m_activeChannel)
                    {
                        case 1:
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() => this.RecCh1_CustomClick(null, null)));
                            }
                            break;
                        case 2:
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() => this.RecCh2_CustomClick(null, null)));
                            }
                            break;
                        case 3:
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() => this.RecCh3_CustomClick(null, null)));
                            }
                                break;
                        case 4:
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() => this.RecCh4_CustomClick(null, null)));
                            }
                                break;
                        case 5:
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() => this.RecCh5_CustomClick(null, null)));
                            }
                                break;
                        case 6:
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() => this.RecDrum_CustomClick(null, null)));
                            }
                            break;
                    }
                }
            }
            else if (serialData.Contains("LooperRemote/B2"))
            {
                string temp = serialData.Remove(0, 16);
                m_activeChannel = Convert.ToInt32(temp[0].ToString());
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                         //
        // Variablen                                                                               //
        //                                                                                         //
        /////////////////////////////////////////////////////////////////////////////////////////////

        private int                 selectedDeviceChannel   = -1;
        public  WaveOut             waveSource              = null;
        private Audio               AudioRecorder;
        private Channel[]           Channels                = new Channel[6];

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        private static BitmapImage GetImage (string imageUri)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit ();
            bitmapImage.UriSource = new Uri (@imageUri, UriKind.Absolute);
            bitmapImage.EndInit ();
            return bitmapImage;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Click_Ch1_Aktiv (object sender, RoutedEventArgs e)
        {
            if (Channels[0].ChannelActive)
                Channels[0].ChannelActive = false;
            else
                Channels[0].ChannelActive = true;


            ImageBrush ib = new ImageBrush();
            if (Channels[0].ChannelActive)
                ib.ImageSource = GetImage ("pack://application:,,,/Pics/Ch1_green.png");
            else
                ib.ImageSource = GetImage ("pack://application:,,,/Pics/Ch1_red.png");

            Btn_Ch1_Aktiv.Background = ib;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Click_Ch2_Aktiv (object sender, RoutedEventArgs e)
        {
            if (Channels[1].ChannelActive)
                Channels[1].ChannelActive = false;
            else
                Channels[1].ChannelActive = true;


            ImageBrush ib = new ImageBrush();
            if (Channels[1].ChannelActive)
                ib.ImageSource = GetImage ("pack://application:,,,/Pics/Ch2_green.png");
            else
                ib.ImageSource = GetImage ("pack://application:,,,/Pics/Ch2_red.png");

            Btn_Ch2_Aktiv.Background = ib;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Click_Ch3_Aktiv (object sender, RoutedEventArgs e)
        {
            if (Channels[2].ChannelActive)
                Channels[2].ChannelActive = false;
            else
                Channels[2].ChannelActive = true;


            ImageBrush ib = new ImageBrush();
            if (Channels[2].ChannelActive)
                ib.ImageSource = GetImage ("pack://application:,,,/Pics/Ch3_green.png");
            else
                ib.ImageSource = GetImage ("pack://application:,,,/Pics/Ch3_red.png");

            Btn_Ch3_Aktiv.Background = ib;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Click_Ch4_Aktiv (object sender, RoutedEventArgs e)
        {
            if (Channels[3].ChannelActive)
                Channels[3].ChannelActive = false;
            else
                Channels[3].ChannelActive = true;


            ImageBrush ib = new ImageBrush();
            if (Channels[3].ChannelActive)
                ib.ImageSource = GetImage ("pack://application:,,,/Pics/Ch4_green.png");
            else
                ib.ImageSource = GetImage ("pack://application:,,,/Pics/Ch4_red.png");

            Btn_Ch4_Aktiv.Background = ib;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Click_Ch5_Aktiv (object sender, RoutedEventArgs e)
        {
            if (Channels[4].ChannelActive)
                Channels[4].ChannelActive = false;
            else
                Channels[4].ChannelActive = true;


            ImageBrush ib = new ImageBrush();
            if (Channels[4].ChannelActive)
                ib.ImageSource = GetImage ("pack://application:,,,/Pics/Ch5_green.png");
            else
                ib.ImageSource = GetImage ("pack://application:,,,/Pics/Ch5_red.png");

            Btn_Ch5_Aktiv.Background = ib;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Click_ChD_Aktiv (object sender, RoutedEventArgs e)
        {
            if (Channels[5].ChannelActive)
                Channels[5].ChannelActive = false;
            else
                Channels[5].ChannelActive = true;


            ImageBrush ib = new ImageBrush();
            if (Channels[5].ChannelActive)
                ib.ImageSource = GetImage ("pack://application:,,,/Pics/ChD_green.png");
            else
                ib.ImageSource = GetImage ("pack://application:,,,/Pics/ChD_red.png");

            Btn_ChD_Aktiv.Background = ib;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ClickRec (object sender, RoutedEventArgs e)
        {
            InputController.Visibility = Visibility.Hidden;
            SerialController.Visibility = Visibility.Hidden;

            if (OutputRecorder.Visibility == Visibility.Hidden)
                OutputRecorder.Visibility = Visibility.Visible;
            else
                OutputRecorder.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ClickController (object sender, RoutedEventArgs e)
        {
            InputController.Visibility = Visibility.Hidden;
            OutputRecorder.Visibility = Visibility.Hidden;
            Cutter.Visibility = Visibility.Hidden;
            AVR.Visibility = Visibility.Hidden;

            if (SerialController.Visibility == Visibility.Hidden)
                SerialController.Visibility = Visibility.Visible;
            else
                SerialController.Visibility = Visibility.Hidden;
        }

        private void CutterBtn_Click(object sender, RoutedEventArgs e)
        {
            OutputRecorder.Visibility = Visibility.Hidden;
            InputController.Visibility = Visibility.Hidden;
            SerialController.Visibility = Visibility.Hidden;
            AVR.Visibility = Visibility.Hidden;

            if (Cutter.Visibility == Visibility.Hidden)
                Cutter.Visibility = Visibility.Visible;
            else
                Cutter.Visibility = Visibility.Hidden;
        }
         
        private void ButtonAVR_Click(object sender, RoutedEventArgs e)
        {
            OutputRecorder.Visibility = Visibility.Hidden;
            InputController.Visibility = Visibility.Hidden;
            SerialController.Visibility = Visibility.Hidden;
            Cutter.Visibility = Visibility.Hidden;

            if (AVR.Visibility == Visibility.Hidden)
                AVR.Visibility = Visibility.Visible;
            else
                AVR.Visibility = Visibility.Hidden;
        }

    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    //                                                                                         //
    // Konstanten                                                                              //
    //                                                                                         //
    /////////////////////////////////////////////////////////////////////////////////////////////
    static class Constants {
        public const short Idle     = 2;
        public const short Play     = 1;
        public const short Rec      = 0;
        public const short Edit     = 1;
        public const short NoEdit   = 0;
        public const short PlayAll  = 1;
        public const short StopAll  = 0;
        public const short NotLoaded = 0;
        public const short Loaded  = 1;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    //                                                                                         //
    // WavePlayer class                                                                        //
    //                                                                                         //
    /////////////////////////////////////////////////////////////////////////////////////////////
    public class WavePlayer {
        WaveFileReader Reader;
        public WaveChannel32 Channel { get; set; }

        public string FileName { get; set; }

        public WavePlayer (string FileName)
        {
            this.FileName = FileName;
            Reader = new WaveFileReader (FileName);
            var loop = new LoopStream(Reader);
            Channel = new WaveChannel32 (loop) { PadWithZeroes = false };
        }

        public void Dispose ()
        {
            if (Channel != null)
            {
                Channel.Dispose ();
                Reader.Dispose ();
            }
        }

    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    //                                                                                         //
    // LoopStream class                                                                        //
    //                                                                                         //
    /////////////////////////////////////////////////////////////////////////////////////////////
    public class LoopStream : WaveStream {
        WaveStream sourceStream;

        /// <summary>
        /// Creates a new Loop stream
        /// </summary>
        /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
        /// or else we will not loop to the start again.</param>
        public LoopStream (WaveStream sourceStream)
        {
            this.sourceStream = sourceStream;
            this.EnableLooping = true;
        }

        /// <summary>
        /// Use this to turn looping on or off
        /// </summary>
        public bool EnableLooping { get; set; }

        /// <summary>
        /// Return source stream's wave format
        /// </summary>
        public override WaveFormat WaveFormat {
            get { return sourceStream.WaveFormat; }
        }

        /// <summary>
        /// LoopStream simply returns
        /// </summary>
        public override long Length {
            get { return sourceStream.Length; }
        }

        /// <summary>
        /// LoopStream simply passes on positioning to source stream
        /// </summary>
        public override long Position {
            get { return sourceStream.Position; }
            set { sourceStream.Position = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read (byte [] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (sourceStream.Position == 0 || !EnableLooping)
                    {
                        // something wrong with the source stream
                        break;
                    }
                    // loop
                    sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }
    }
}

