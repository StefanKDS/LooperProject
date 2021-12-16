using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Looper.CustomControls {
    public partial class RecButton : UserControl {
        public RecButton ()
        {
            InitializeComponent ();
        }

        public event RoutedEventHandler CustomClick;
        public event RoutedEventHandler CustomStopClick;
        public event RoutedEventHandler CustomClickRight;
        public event RoutedEventHandler CustomFreeChannel;

        Stopwatch stopwatch;

        private static BitmapImage GetImage (string imageUri)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit ();
            bitmapImage.UriSource = new Uri (imageUri, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit ();
            return bitmapImage;
        }
        
        public short ChannelStatus {
            get
            {
                return channelStatus;
            }
            set
            {
                channelStatus = value;

                if (channelStatus == 0)
                {
                    Image simpleImage = new Image();
                    simpleImage.Source = GetImage ("/Pics/RedLED.png");
                    Btn.Content = simpleImage;
                } else if (channelStatus == 1)
                {
                    Image simpleImage = new Image();
                    simpleImage.Source = GetImage ("/Pics/GreenLED.png");
                    Btn.Content = simpleImage;
                } else if (channelStatus == 2)
                {
                    Image simpleImage = new Image();
                    simpleImage.Source = GetImage ("/Pics/WhiteLED.png");
                    Btn.Content = simpleImage;
                }
            }
        }


        private short channelStatus = 2;

        private void Btn_MouseRightButtonUp (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CustomClickRight != null)
                CustomClickRight (this, new RoutedEventArgs ());
        }

        private void Button_PreviewMouseDown (object sender, MouseButtonEventArgs e)
        {
            stopwatch = new Stopwatch ();
            stopwatch.Start ();
        }

        private void Button_PreviewMouseUp (object sender, MouseButtonEventArgs e)
        {
            stopwatch.Stop ();

            if (stopwatch.ElapsedMilliseconds < 500)
            {
                if (CustomClick != null)
                    CustomClick (this, new RoutedEventArgs ());

            } else if(stopwatch.ElapsedMilliseconds > 500 && stopwatch.ElapsedMilliseconds < 2500)
            {
                if (CustomStopClick != null)
                    CustomStopClick (this, new RoutedEventArgs ());
            }
            else
            {
                if (CustomFreeChannel != null)
                    CustomFreeChannel (this, new RoutedEventArgs ());
            }
        }
    }
}
