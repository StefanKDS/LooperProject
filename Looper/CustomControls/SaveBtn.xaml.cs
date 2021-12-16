using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace Looper.CustomControls {
    /// <summary>
    /// Interaktionslogik für EditBtn.xaml
    /// </summary>
    public partial class SaveBtn : UserControl {
        public SaveBtn()
        {
            InitializeComponent ();
        }

        public event RoutedEventHandler CustomSaveClick;

        private static BitmapImage GetImage (string imageUri)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit ();
            bitmapImage.UriSource = new Uri (imageUri, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit ();
            return bitmapImage;
        }

        private void OnSaveButtonClick (object sender, RoutedEventArgs e)
        {
            if (CustomSaveClick != null)
            {
                CustomSaveClick(this, new RoutedEventArgs ());
            }
        }

        public short ChannelEditStatus {
            get
            {
                return channelStatus;
            }
            set
            {
                channelStatus = value;
                if (channelStatus == 1)
                {
                    Image simpleImage = new Image();
                    simpleImage.Source = GetImage ("/Pics/Button_Rec_Green_svg.png");
                    Btn.Content = simpleImage;
                } else if (channelStatus == 0)
                {
                    Image simpleImage = new Image();
                    simpleImage.Source = GetImage ("/Pics/Button_Rec_White_svg.png");
                    Btn.Content = simpleImage;
                }
            }
        }


        private short channelStatus = 0;
        public enum EditButtonStatus { NoEdit = 0, Edit };
    }
}
