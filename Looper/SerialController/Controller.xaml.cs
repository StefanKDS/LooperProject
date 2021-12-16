using System.IO.Ports;
using System.Windows.Controls;

namespace Looper.SerialController {
    /// <summary>
    /// Interaktionslogik für Controller.xaml
    /// </summary>
    public partial class Controller : UserControl {
        public delegate void ReceivedFromSerialPort (string serialData);
        public delegate void InputChannel (int channel);

        SerialControllerBase    MySerialController  = new SerialControllerBase();
        string                  activePort          = string.Empty;
        bool                    connected           = false;
        ReceivedFromSerialPort  RFSP;
        InputChannel            IC;

        /// <summary>
        /// Konstruktor
        /// </summary>
        public Controller ()
        {
            InitializeComponent ();

            string[] ports = MySerialController.GetAllPorts ();

            foreach (string port in ports)
            {
                PortList.Items.Add (port);
            }
        }

        /// <summary>
        /// Initialisiert die Settings
        /// </summary>
        public void InitSettings (ReceivedFromSerialPort theRFSP, InputChannel theInputChannel)
        {
            RFSP    = theRFSP;
            IC      = theInputChannel;
        }

        /// <summary>
        /// Reagiert auf den Button "Verbinden". Wenn der Port gültig ist, dann wird 
        /// eine Serielle Verbindung aufgebaut
        /// </summary>
        private void ConnectBtn_Click (object sender, System.Windows.RoutedEventArgs e)
        {
            if (activePort == string.Empty)
                return;

            if (!connected)
            {
                MySerialController.OpenPort (activePort, _serialPort_DataReceived);
                ConnectBtn.Content = "Trennen";
                connected = true;
            } else
            {
                MySerialController.ClosePort ();
                ConnectBtn.Content = "Verbinden";
                connected = false;
            }

        }

        /// <summary>
        /// Wird aufgerufen wenn Daten vom Seriellen Port empfangen wurden und gibt
        /// diese an die Callback Funktion weiter
        /// </summary>
        void _serialPort_DataReceived (object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort) sender;
            string      s = sp.ReadExisting ();

            RFSP (s);
        }

        /// <summary>
        /// Reagiert auf eine Änderung der Auswahl in der ComboBox "PortList" und den
        /// Wert in die Variable "activePort"
        /// </summary>

        private void PortList_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            activePort = PortList.SelectedItem.ToString ();
        }

        /// <summary>
        /// Reagiert auf eine Änderung der Auswahl in der ComboBox "SourceList" und gibt diese der
        /// Callback funktion weiter
        /// </summary>
        private void SourceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            IC (int.Parse (SourceList.SelectedItem.ToString ().Substring (0, 1)));
        }

        /// <summary>
        /// Fügt einen Eintrag in die ComboBox "SourceList" hinzu
        /// </summary>
        public void AddSourceListItem (string item )
        {
            SourceList.Items.Add (item);
        }

        /// <summary>
        /// Löscht den Inhalt der Combobox "SourceList"
        /// </summary>
        public void ClearSourceList()
        {
            SourceList.Items.Clear ();
        }
    }
}
