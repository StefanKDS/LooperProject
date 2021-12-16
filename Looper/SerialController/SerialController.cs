using System.IO.Ports;

namespace Looper
{
    class SerialControllerBase
    {
        SerialPort _serialPort = new SerialPort ();
        public delegate void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e);

        void ClosePorts()
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }

        public void OpenPort(string port, _serialPort_DataReceived DR)
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();

            _serialPort.BaudRate = 9600;
            _serialPort.DataBits = 8;
            _serialPort.Handshake = Handshake.XOnXOff;
            _serialPort.Parity = Parity.None;
            _serialPort.PortName = port;
            _serialPort.StopBits = StopBits.One;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DR);

            _serialPort.Open();
        }

        public bool IsPortOpen()
        {
            return _serialPort.IsOpen;
        }

        public string[] GetAllPorts()
        {
            return SerialPort.GetPortNames();
        }

        public void Send(string sendString)
        {
            if (sendString.Length <= 0)
                return;

            if (!IsPortOpen())
                return;

            _serialPort.Write(sendString);
        }

        public void ClosePort()
        {
            if (_serialPort.IsOpen)
                _serialPort.Close ();
        }
    }
}
