using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace Looper.CustomControls {
    /// <summary>
    /// Interaktionslogik für EditBtn.xaml
    /// </summary>
    public partial class AllStartStopBtn : UserControl {
        public AllStartStopBtn ()
        {
            InitializeComponent ();
        }

        public event RoutedEventHandler CustomPlayAllClick;

        private static BitmapImage GetImage (string imageUri)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit ();
            bitmapImage.UriSource = new Uri (imageUri, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit ();
            return bitmapImage;
        }

        private void OnPlayAllClick (object sender, RoutedEventArgs e)
        {
            if (CustomPlayAllClick != null)
            {
                CustomPlayAllClick (this, new RoutedEventArgs ());
            }
        }

        public short PlayAllStatus {
            get
            {
                return playAllStatus;
            }
            set
            {
                playAllStatus = value;
                if (playAllStatus == 1)
                {
                    Image simpleImage = new Image();
                    simpleImage.Source = GetImage ("/Pics/Button_green.png");
                    Btn.Content = simpleImage;
                } else if (playAllStatus == 0)
                {
                    Image simpleImage = new Image();
                    simpleImage.Source = GetImage ("/Pics/Button_white.png");
                    Btn.Content = simpleImage;
                }
            }
        }


        private short playAllStatus = 0;
        public enum PlayAllButtonStatus { Stop = 0, Play };
    }
}
