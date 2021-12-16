using System.Windows;
using System.Windows.Controls;


namespace Looper.CustomControls {
    /// <summary>
    /// Interaktionslogik für EditBtn.xaml
    /// </summary>
    public partial class UniversalBtn : UserControl {
        public UniversalBtn()
        {
            InitializeComponent ();
        }

        public event RoutedEventHandler CustomClick;
     
        private void OnButtonClick (object sender, RoutedEventArgs e)
        {
            if (CustomClick != null)
            {
                CustomClick (this, new RoutedEventArgs ());
            }
        }

        public enum UniversalButtonStatus { NoEdit = 0, Edit };
    }
}
