using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Looper.About
{
    /// <summary>
    /// Interaktionslogik für About.xaml
    /// </summary>
    public partial class AboutWin : Window
    {
        public AboutWin()
        {
            InitializeComponent();
            VersionString.Text = "Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void NAudio_License_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var directory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            directory += "\\About\\Lizenzen";
            var file = Path.Combine(directory, "NAudio.txt");
            Process.Start(file);
        }
    }
}
