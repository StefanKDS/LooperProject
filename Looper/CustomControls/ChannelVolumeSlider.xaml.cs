using System.Windows;
using System.Windows.Controls;


namespace Looper.CustomControls {
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class ChannelVolumeSlider : UserControl {
        public ChannelVolumeSlider ()
        {
            InitializeComponent ();
        }

        public event RoutedEventHandler CustomValueChanged;

        private void Slider_ValueChanged (object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (CustomValueChanged != null)
                CustomValueChanged (this, new RoutedEventArgs ());
        }

        public double CustomValue {
            get
            {
                return slider.Value;
            }
           
        }
    }
}
