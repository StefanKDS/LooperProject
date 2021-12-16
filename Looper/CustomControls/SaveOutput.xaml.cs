using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Looper.CustomControls {
    /// <summary>
    /// Interaktionslogik für SaveOutput.xaml
    /// </summary>
    public partial class SaveOutput : UserControl {
        public SaveOutput ()
        {
            InitializeComponent ();

            RecButton.IsEnabled = false;
            StopButton.IsEnabled = false;
        }

        private void Button_RecClick (object sender, RoutedEventArgs e)
        {
            if (Recording)
                return;

            StopButton.IsEnabled = true;
            AudioRecorder = new Audio (-1);
            AudioRecorder.StartRecordFromSoundcard (FileName);
            RecStatus.Content = "- Recording started -";
            RecStatus.Foreground = new SolidColorBrush (Colors.Red);
            Recording = true;
        }

        private void Button_StopClick (object sender, RoutedEventArgs e)
        {
            AudioRecorder.StopSCRecord ();
            RecStatus.Content = "- Recording stopped -";
            RecStatus.Foreground = new SolidColorBrush (Colors.Green);
            Recording = false;
        }

        private void Button_SaveClick (object sender, RoutedEventArgs e)
        {
            if (Recording)
                return;

            SaveFileDialog dlg = new SaveFileDialog();

            dlg.DefaultExt = ".wav";
            dlg.Filter = "Wave (.wav)|*.wav";

            Nullable<bool> result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                RecButton.IsEnabled = true;
                FileName = dlg.FileName;
            } else {
                RecButton.IsEnabled = false;
                StopButton.IsEnabled = false;
                return;
            }
        }

        private Audio   AudioRecorder;
        private String  FileName = "";
        private bool    Recording = false;
    }
}
