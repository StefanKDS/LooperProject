using System;
using System.Windows.Controls;
using NAudio.Wave;
using VoiceRecorder.Audio;

namespace Looper.CustomControls
{
    /// <summary>
    /// Interaktionslogik für Cutter.xaml
    /// </summary>
    public partial class Chords : UserControl
    {
        private SampleAggregator sampleAggregator;
        private int totalWaveFormSamples;

        public Chords()
        {
            InitializeComponent();

            SampleAggregator = new SampleAggregator();
            SampleAggregator.NotificationCount = 800; // gets set correctly later on
        }

        private void RenderFile(String FileName)
        {
            SampleAggregator.RaiseRestart();
            using (WaveFileReader reader = new WaveFileReader(FileName))
            {
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
            }
        }

        public SampleAggregator SampleAggregator {
            get
            {
                sampleAggregator = WaveForm.WFCSampleAggregator;
                return sampleAggregator;
            }
            set
            {
                if (sampleAggregator != value)
                {
                    sampleAggregator = value;
                    WaveForm.WFCSampleAggregator = sampleAggregator;
                }
            }
        }

        public int TotalWaveFormSamples {
            get
            {
                totalWaveFormSamples = WaveForm.WFCTotalWaveFormSamples;
                return totalWaveFormSamples;
            }
            set
            {
                if (totalWaveFormSamples != value)
                {
                    totalWaveFormSamples = value;
                    WaveForm.WFCTotalWaveFormSamples = totalWaveFormSamples;
                }
            }
        }

        public int Position
        {
            get
            {
                return WaveForm.Position;
            }
            set
            {
                if(WaveForm.Position != value)
                {
                    WaveForm.Position = value;
                }
            }
        }

        public void Load(string FileName)
        {
            RenderFile(FileName);
        }
    }
}
