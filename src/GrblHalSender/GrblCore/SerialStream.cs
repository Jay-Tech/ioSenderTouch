

using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace GrblHalSender.GrblCore
{
    public class SerialStream : StreamComms
    {
        private readonly SerialPort _serialPort = null;
        private byte[] _buffer = new byte[Comms.RXBUFFERSIZE];
        private readonly StringBuilder _input = new(Comms.RXBUFFERSIZE);
        private volatile Comms.State _state = Comms.State.ACK;
        private Dispatcher Dispatcher { get; set; }

        public event DataReceivedHandler DataReceived;

        public SerialStream(string PortParams, int ResetDelay, Dispatcher dispatcher)
        {
            Comms.com = this;
            Dispatcher = dispatcher;
            Reply = string.Empty;

            if (PortParams.IndexOf(":") < 0)
                PortParams += ":115200,N,8,1";

            string[] parameter = PortParams.Substring(PortParams.IndexOf(":") + 1).Split(',');

            if (parameter.Count() < 4)
            {
                MessageBox.Show(string.Format(LibStrings.FindResource("SerialPortError"), PortParams), "ioSender");
                System.Environment.Exit(2);
            }

            _serialPort = new SerialPort();
            _serialPort.PortName = PortParams.Substring(0, PortParams.IndexOf(":"));
            _serialPort.BaudRate = int.Parse(parameter[0]);
            _serialPort.Parity = ParseParity(parameter[1]);
            _serialPort.DataBits = int.Parse(parameter[2]);
            _serialPort.StopBits = int.Parse(parameter[3]) == 1 ? StopBits.One : StopBits.Two;
            _serialPort.ReceivedBytesThreshold = 1;
            _serialPort.ReadTimeout = 50;
            _serialPort.ReadBufferSize = Comms.RXBUFFERSIZE;
            _serialPort.WriteBufferSize = Comms.TXBUFFERSIZE;

            if (parameter.Length > 4)
                _serialPort.Handshake = parameter[4] switch
                {
                    "P" => // Cannot be used With ESP32!
                        Handshake.RequestToSend,
                    "X" => Handshake.XOnXOff,
                    _ => _serialPort.Handshake
                };

            try
            {
                _serialPort.Open();
            }
            catch
            {
                //
            }

            if (_serialPort.IsOpen)
            {
                _serialPort.DtrEnable = true;

                var resetMode = Comms.ResetMode.None;

                PurgeQueue();
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);

                if (parameter.Count() > 5)
                    Enum.TryParse(parameter[5], true, out resetMode);

                switch (resetMode)
                {
                    case Comms.ResetMode.RTS:
                        /* For resetting ESP32 */
                        _serialPort.RtsEnable = true;
                        Thread.Sleep(5);
                        _serialPort.RtsEnable = false;
                        if (ResetDelay > 0)
                            Thread.Sleep(ResetDelay);
                        break;

                    case Comms.ResetMode.DTR:
                        /* For resetting Arduino */
                        _serialPort.DtrEnable = false;
                        Thread.Sleep(5);
                        _serialPort.DtrEnable = true;
                        if (ResetDelay > 0)
                            Thread.Sleep(ResetDelay);
                        break;
                }
            }
        }

        ~SerialStream()
        {
            if (!IsClosing && IsOpen)
                Close();
        }

        public Comms.StreamType StreamType { get { return Comms.StreamType.Serial; } }
        public Comms.State CommandState { get { return _state; } set { _state = value; } }
        public string Reply { get; private set; }
        public bool IsOpen { get { return _serialPort != null && _serialPort.IsOpen; } }
        public bool IsClosing { get; private set; }
        public int OutCount { get { return _serialPort.BytesToWrite; } }
        public bool EventMode { get; set; } = true;
        public Action<int> ByteReceived { get; set; }

        public void PurgeQueue()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                }
            }
            Reply = string.Empty;
            if (!EventMode)
                _input.Clear();
        }

        private Parity ParseParity(string parity)
        {
            Parity res = Parity.None;

            switch (parity)
            {
                case "E":
                    res = Parity.Even;
                    break;

                case "O":
                    res = Parity.Odd;
                    break;

                case "M":
                    res = Parity.Mark;
                    break;

                case "S":
                    res = Parity.Space;
                    break;
            }

            return res;
        }

        public void Close()
        {
            if (!IsClosing && IsOpen)
            {
                IsClosing = true;
                try
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.BaseStream.Close();
                    Thread.Sleep(100);
                    _serialPort.Close();
                    _serialPort?.Dispose();
                }
                catch
                {
                    //
                }
                finally
                {
                    IsClosing = false;
                }

            }
        }

        public int ReadByte()
        {
            int c = _input.Length == 0 ? -1 : _input[0];

            if (c != -1)
                _input.Remove(0, 1);

            return c;
        }

        public void TryReconnectSerial()
        {
            try
            {
                _serialPort?.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
        public void WriteByte(byte data)
        {
            if (_serialPort is { IsOpen: true })
                _serialPort.BaseStream.Write([data], 0, 1);
            else
            {
                TryReconnectSerial();
            }
        }

        public void WriteBytes(byte[] bytes, int len)
        {
            _serialPort.BaseStream.WriteAsync(bytes, 0, len);
        }

        public void WriteString(string data)
        {
            var bytes = Encoding.Default.GetBytes(data);
            WriteBytes(bytes, bytes.Length);
        }

        public void WriteCommand(string command)
        {
            _state = Comms.State.AwaitAck;
            if (_serialPort == null) return;
            if (command.Length == 1 && command != GrblConstants.CMD_PROGRAM_DEMARCATION)
                WriteByte((byte)command.ToCharArray()[0]);
            else
            {
                command += "\r";
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(command);
                if (_serialPort.IsOpen)
                    _serialPort.BaseStream.Write(bytes, 0, bytes.Length);
            }
        }

        public void AwaitAck()
        {
            while (Comms.com.CommandState == Comms.State.DataReceived || Comms.com.CommandState == Comms.State.AwaitAck)
                EventUtils.DoEvents();
        }

        public void AwaitAck(string command)
        {
            PurgeQueue();
            Reply = string.Empty;
            WriteCommand(command);

            while (Comms.com.CommandState == Comms.State.DataReceived || Comms.com.CommandState == Comms.State.AwaitAck)
                EventUtils.DoEvents();
        }

        public void AwaitResponse()
        {
            while (Comms.com.CommandState == Comms.State.AwaitAck)
                EventUtils.DoEvents();
        }

        public void AwaitResponse(string command)
        {
            PurgeQueue();
            Reply = string.Empty;
            WriteCommand(command);

            while (Comms.com.CommandState == Comms.State.AwaitAck)
                System.Threading.Thread.Sleep(15);
        }

        public string GetReply(string command)
        {
            Reply = string.Empty;
            WriteCommand(command);

            AwaitResponse();

            return Reply;
        }

        private int gp()
        {
            int pos = 0; bool found = false;

            while (!found && pos < _input.Length)
                found = _input[pos++] == '\n';

            return found ? pos - 1 : 0;
        }


        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int pos = 0;

            lock (_input)
            {
                _input.Append(_serialPort.ReadExisting());

                if (EventMode)
                {
                    while (_input.Length > 0 && (pos = gp()) > 0)
                    {
                        Reply = pos == 0 ? string.Empty : _input.ToString(0, pos - 1);
                        _input.Remove(0, pos + 1);
                        if (Reply.Length != 0 && DataReceived != null)
                            Dispatcher.BeginInvoke(DataReceived, Reply);
                        //                            Dispatcher.Invoke(addEdge, Reply);

                        _state = Reply == "ok" ? Comms.State.ACK : (Reply.StartsWith("error") ? Comms.State.NAK : Comms.State.DataReceived);
                    }
                }
                else
                    ByteReceived?.Invoke(ReadByte());

            }
        }
    }

    public class ConnectMode : ViewModelBase
    {
        public ConnectMode(Comms.ResetMode mode, string name)
        {
            Mode = mode;
            Name = name;
        }

        public Comms.ResetMode Mode { get; private set; }

        public string Name { get; private set; }
    }

    public class ComPort
    {
        public ComPort()
        {
        }

        public ComPort(string name)
        {
            Name = FullName = name;
        }

        public string Name { get; set; }
        public string FullName { get; set; }
    }

    public class SerialPorts : ViewModelBase
    {
        string _selected = string.Empty;
        string _baud = "115200";
        private ConnectMode _mode = null;

        public SerialPorts()
        {
            var ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                Ports.Add(new ComPort(port));
            }


            if (Ports.Count > 0)
                _selected = Ports[0].Name;

            Baud.Add(_baud);
            Baud.Add("230400");
            Baud.Add("460800");
            Baud.Add("921600");

            ConnectModes.Add(new ConnectMode(Comms.ResetMode.None, "No action"));
            ConnectModes.Add(new ConnectMode(Comms.ResetMode.DTR, "Toggle DTR"));
            ConnectModes.Add(new ConnectMode(Comms.ResetMode.RTS, "Toggle RTS"));

            SelectedMode = ConnectModes[0];
        }

        public void Refresh()
        {
            var _portnames = SerialPort.GetPortNames();

            Ports.Clear();
            var ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                Ports.Add(new ComPort(port));
            }
            //if (_portnames.Length > 0)
            //{
            //    Array.Sort(_portnames);

            //    if (_portnames.Contains("COM1"))
            //    {
            //        var pn = _portnames.ToList();
            //        pn.Remove("COM1");
            //        _portnames = pn.ToArray();
            //    }

            //    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'")) try
            //        {
            //            var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
            //            var portList = _portnames.Select(n => ports.FirstOrDefault(s => s.Contains('(' + n + ')'))).ToList();
            //            foreach (var fullname in portList)
            //            {
            //                var name = fullname.Substring(fullname.IndexOf("(COM") + 1).Trim().TrimEnd(')');
            //                var port = new ComPort(name);

            //                port.FullName = name + " - " + fullname.Replace('(' + name + ')', string.Empty).Trim();

            //                Ports.Add(port);
            //            }
            //        }
            //        catch
            //        {
            //        }

            //    if (Ports.Count != _portnames.Length)
            //    {
            //        foreach (var port in _portnames)
            //        {
            //            if (port.StartsWith("COM") && Ports.Where(n => n.Name == port).FirstOrDefault() == null)
            //                Ports.Add(new ComPort(port));
            //        }
            //    }

            if (Ports.Count > 0)
                SelectedPort = Ports[0].Name;
            //}
        }

        public ObservableCollection<ComPort> Ports { get; private set; } = new ObservableCollection<ComPort>();
        public ObservableCollection<ConnectMode> ConnectModes { get; private set; } = new ObservableCollection<ConnectMode>();
        public ObservableCollection<string> Baud { get; private set; } = new ObservableCollection<string>();

        public string SelectedPort
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedBaud
        {
            get { return _baud; }
            set
            {
                if (_baud != value)
                {
                    _baud = value;
                    OnPropertyChanged();
                }
            }
        }

        public ConnectMode SelectedMode
        {
            get { return _mode; }
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
