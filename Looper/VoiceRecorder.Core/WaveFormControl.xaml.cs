using System;
using System.Windows;
using System.Windows.Controls;

using VoiceRecorder.Audio;

namespace VoiceRecorder.Core
{
    /// <summary>
    /// Interaction logic for WaveFileTrimmerControl.xaml
    /// </summary>
    public partial class WaveFormControl : UserControl
    {
        public static readonly DependencyProperty WFCSampleAggregatorProperty = DependencyProperty.Register(
            "WFCSampleAggregator", typeof(SampleAggregator), typeof(WaveFormControl), new PropertyMetadata(null, OnSampleAggregatorChanged));

        public static readonly DependencyProperty WFCTotalWaveFormSamplesProperty = DependencyProperty.Register(
            "WFCTotalWaveFormSamples", typeof(int), typeof(WaveFormControl), new PropertyMetadata(0, OnNotificationCountChanged));

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            "Position", typeof(int), typeof(WaveFileTrimmerControl), new PropertyMetadata(0, OnPositionChanged));


        public WaveFormControl()
        {
            InitializeComponent();
        }

       public SampleAggregator WFCSampleAggregator
        {
           get { return (SampleAggregator)this.GetValue(WFCSampleAggregatorProperty); }
           set { this.SetValue(WFCSampleAggregatorProperty, value); }
       }

       public int WFCTotalWaveFormSamples
        {
           get { return (int)this.GetValue(WFCTotalWaveFormSamplesProperty); }
           set { this.SetValue(WFCTotalWaveFormSamplesProperty, value); }
       }

        public int Position
        {
            get { return (int)this.GetValue(PositionProperty); }
            set { this.SetValue(PositionProperty, value); posSelection.Pos = value; }
        }

        private static void OnSampleAggregatorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            WaveFormControl control = (WaveFormControl)sender;
            control.Subscribe();
        }

        private static void OnNotificationCountChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            WaveFormControl control = (WaveFormControl)sender;
            control.SetWidth();
        }

        private static void OnPositionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            WaveFormControl control = (WaveFormControl)sender;
            //control.posSelection.Pos = Position;
        }

        private void SetWidth()
        {
            waveFormRenderer.Width = WFCTotalWaveFormSamples * waveFormRenderer.XSpacing;          
        }

        private void Subscribe()
        {
            WFCSampleAggregator.MaximumCalculated += SampleAggregator_MaximumCalculated;
            WFCSampleAggregator.Restart += new EventHandler(SampleAggregator_Restart);
        }

        void SampleAggregator_Restart(object sender, EventArgs e)
        {
            this.waveFormRenderer.Reset();
        }

        void SampleAggregator_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            waveFormRenderer.AddValue(e.MaxSample, e.MinSample);
        }
    }
}
