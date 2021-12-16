using System;
using System.Windows;
using System.Windows.Controls;
using NAudio.Wave;
using Microsoft.Win32;
using System.IO;
using VoiceRecorder.Audio;
using Looper.Properties;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;

namespace Looper.CustomControls
{
    /// <summary>
    /// Interaktionslogik für Cutter.xaml
    /// </summary>
    public partial class Cutter : UserControl
    {
        private SampleAggregator sampleAggregator;
        private int leftPosition;
        private int rightPosition;
        private int totalWaveFormSamples;
        private IAudioPlayer audioPlayer;
        private int samplesPerSecond;
        private String inputFile;

        public Cutter()
        {
            InitializeComponent();

            SampleAggregator = new SampleAggregator();
            SampleAggregator.NotificationCount = 800; // gets set correctly later on
            audioPlayer = new AudioPlayer();
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "WAV file (.wav)|*.wav";// |MP3 file (.mp3)|.mp3";
            saveFileDialog.DefaultExt = ".wav";
            bool? result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                SaveAs(saveFileDialog.FileName);
            }
        }

        private TimeSpan PositionToTimeSpan(int position)
        {
            int samples = SampleAggregator.NotificationCount * position;
            return TimeSpan.FromSeconds((double)samples / samplesPerSecond);
        }

        private void SaveAs(string fileName)
        {
            CutterAudioSaver saver = new CutterAudioSaver(inputFile);
            saver.TrimFromStart = PositionToTimeSpan(LeftPosition);
            saver.TrimFromEnd = PositionToTimeSpan(TotalWaveFormSamples - RightPosition);

            if (fileName.ToLower().EndsWith(".wav"))
            {
                saver.SaveFileFormat = SaveFileFormat.Wav;
                saver.SaveAudio(fileName);
            }
            else if (fileName.ToLower().EndsWith(".mp3"))
            {
                string lameExePath = LocateLame();
                if (lameExePath != null)
                {
                    saver.SaveFileFormat = SaveFileFormat.Mp3;
                    saver.LameExePath = lameExePath;
                    saver.SaveAudio(fileName);
                }
            }
            else
            {
                MessageBox.Show("Please select a supported output format");
            }
        }

        public int LeftPosition {
            get
            {
                leftPosition = WaveFileTrimmer.LeftSelection;
                return leftPosition;
            }
            set
            {
                if (leftPosition != value)
                {
                    leftPosition = value;
                    WaveFileTrimmer.LeftSelection = leftPosition;
                }
            }
        }

        public int RightPosition {
            get
            {
                rightPosition = WaveFileTrimmer.RightSelection;
                return rightPosition;
            }
            set
            {
                if (rightPosition != value)
                {
                    rightPosition = value;
                    WaveFileTrimmer.RightSelection = rightPosition;
                }
            }
        }

        public string LocateLame()
        {
            string lameExePath = Settings.Default.LameExePath;

            if (String.IsNullOrEmpty(lameExePath) || !File.Exists(lameExePath))
            {
                if (MessageBox.Show("Um MP3's zu speichern benötigst du LAME.exe. Bitte auswählen...",
                    "Als MP3 speichern",
                    MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.FileName = "lame.exe";
                    bool? result = ofd.ShowDialog();
                    if (result != null && result.HasValue)
                    {
                        if (File.Exists(ofd.FileName) && ofd.FileName.ToLower().EndsWith("lame.exe"))
                        {
                            Settings.Default.LameExePath = ofd.FileName;
                            Settings.Default.Save();
                            return ofd.FileName;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            return lameExePath;
        }

        private void RenderFile(String FileName)
        {
            SampleAggregator.RaiseRestart();
            using (WaveFileReader reader = new WaveFileReader(FileName))
            {
                inputFile = FileName;
                samplesPerSecond = reader.WaveFormat.SampleRate;
                SampleAggregator.NotificationCount = reader.WaveFormat.SampleRate / 10;

                byte[] buffer = new byte[1024];
                WaveBuffer waveBuffer = new WaveBuffer(buffer);
                waveBuffer.ByteBufferCount = buffer.Length;
                int bytesRead;
                do
                {
                    bytesRead = reader.Read(waveBuffer, 0, buffer.Length);
                    int samples = bytesRead / 2;
                    for (int sample = 0; sample < samples; sample++)
                    {
                        if (bytesRead > 0)
                        {
                            sampleAggregator.Add(waveBuffer.ShortBuffer[sample] / 32768f);
                        }
                    }
                } while (bytesRead > 0);
                int totalSamples = (int)reader.Length / 2;
                TotalWaveFormSamples = totalSamples / sampleAggregator.NotificationCount;
                SelectAll(null,null);
            }
            audioPlayer.LoadFile(FileName);
        }

        private void Play(object sender, RoutedEventArgs e)
        { 
            audioPlayer.StartPosition = new TimeSpan(PositionToTimeSpan(LeftPosition).Ticks / 2);
            audioPlayer.EndPosition = new TimeSpan(PositionToTimeSpan(RightPosition).Ticks / 2);
            audioPlayer.Play();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            audioPlayer.Stop();
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            LeftPosition = 0;
            RightPosition = TotalWaveFormSamples;
        }

        public SampleAggregator SampleAggregator {
            get
            {
                sampleAggregator = WaveFileTrimmer.SampleAggregator;
                return sampleAggregator;
            }
            set
            {
                if (sampleAggregator != value)
                {
                    sampleAggregator = value;
                    WaveFileTrimmer.SampleAggregator = sampleAggregator;
                }
            }
        }

        public int TotalWaveFormSamples {
            get
            {
                totalWaveFormSamples = WaveFileTrimmer.TotalWaveFormSamples;
                return totalWaveFormSamples;
            }
            set
            {
                if (totalWaveFormSamples != value)
                {
                    totalWaveFormSamples = value;
                    WaveFileTrimmer.TotalWaveFormSamples = totalWaveFormSamples;
                }
            }
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            string filename;

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".wav";
            dlg.Filter = "Wave (.wav)|*.wav";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
                filename = dlg.FileName;
            else
                return;

            RenderFile(filename);
        }
    }

    public enum SaveFileFormat
    {
        Wav,
        Mp3
    }

    public class CutterAudioSaver
    {
        private string inputFile;

        public TimeSpan TrimFromStart { get; set; }
        public TimeSpan TrimFromEnd { get; set; }
        public SaveFileFormat SaveFileFormat { get; set; }
        public string LameExePath { get; set; }

        public CutterAudioSaver(string inputFile)
        {
            this.inputFile = inputFile;
        }

        public bool IsTrimNeeded {
            get
            {
                return TrimFromStart != TimeSpan.Zero || TrimFromEnd != TimeSpan.Zero;
            }
        }

        public void SaveAudio(string outputFile)
        {
            List<string> tempFiles = new List<string>();
            string fileToProcess = inputFile;
            if (IsTrimNeeded)
            {
                string tempFile = WavFileUtils.GetTempWavFileName();
                tempFiles.Add(tempFile);
                WavFileUtils.TrimWavFile(inputFile, tempFile, TrimFromStart, TrimFromEnd);
                fileToProcess = tempFile;
            }
            if (SaveFileFormat == SaveFileFormat.Mp3)
            {
                ConvertToMp3(this.LameExePath, fileToProcess, outputFile);
            }
            else
            {
                if (File.Exists(outputFile))
                    File.Delete(outputFile);
                File.Copy(fileToProcess, outputFile);
            }
            DeleteTempFiles(tempFiles);
        }

        private void DeleteTempFiles(IEnumerable<string> tempFiles)
        {
            foreach (string tempFile in tempFiles)
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public static void ConvertToMp3(string lameExePath, string waveFile, string mp3File)
        {
            Process converter = Process.Start(lameExePath, "-V2 \"" + waveFile + "\" \"" + mp3File + "\"");
            converter.WaitForExit();
        }
    }
}
