using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
namespace Looper.CustomControls
{
    /// <summary>
    /// Interaktionslogik für UnvisibleButton.xaml
    /// </summary>
    public partial class UnvisibleButton : UserControl
    {
        public UnvisibleButton()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler Custom_Unvisible_Button_Click;

        private void Unvisible_Button_Click(object sender, RoutedEventArgs e)
        {
            Custom_Unvisible_Button_Click(this, new RoutedEventArgs());
        }
    }
}
