

using System.Net.Sockets;
using System.Text;
using System.Windows.Threading;

namespace GrblHalSender.GrblCore
{
    public class TelnetStream : StreamComms
    {
        private TcpClient _ipServer = null;
        private NetworkStream _ipStream = null;
        private readonly byte[] _buffer = new byte[512];
        private volatile Comms.State _state = Comms.State.ACK;
        private StringBuilder input = new StringBuilder(1024);
        private Dispatcher Dispatcher { get; set; }
        public Comms.StreamType StreamType { get { return Comms.StreamType.Telnet; } }
        public bool IsOpen { get { return _ipServer != null && _ipServer.Connected; } }
        public int OutCount { get { return 0; } }
        public Comms.State CommandState { get { return _state; } set { _state = value; } }
        public string Reply { get; private set; }
        public bool EventMode { get; set; } = true;
        public Action<int> ByteReceived { get; set; }
        public event DataReceivedHandler DataReceived;

        public TelnetStream(string host, Dispatcher dispatcher)
        {
            Comms.com = this;
            Reply = string.Empty;
            Dispatcher = dispatcher;

            if (!host.Contains(":"))
                host += ":23";

            string[] parameter = host.Split(':');

            if (parameter.Length == 2) try
                {
                    _ipServer = new TcpClient(parameter[0], int.Parse(parameter[1]));
                    _ipServer.NoDelay = true;
                    _ipStream = _ipServer.GetStream();
                    _ipStream.BeginRead(_buffer, 0, _buffer.Length, ReadComplete, _buffer);
                }
                catch
                {
                }
        }

        ~TelnetStream()
        {
            Close();
        }

       

        public void PurgeQueue()
        {
            _ipStream?.Flush();
            //while (_ipStream.DataAvailable)
            //    _ipStream.ReadByte();
            Reply = string.Empty;
            if (!EventMode)
                input.Clear();
        }

        public void Close()
        {
            if (IsOpen)
            {
                PurgeQueue();
                _ipStream?.Close(300);
                _ipStream?.Dispose();
                _ipServer?.Close();
                _ipServer?.Dispose();
            }
        }

        public int ReadByte()
        {
            int c = input.Length == 0 ? -1 : input[0];

            if (c != -1)
                input.Remove(0, 1);

            return c;
        }

        public void WriteByte(byte data)
        {
            _ipStream.WriteAsync([data], 0, 1);
        }

        public void WriteBytes(byte[] bytes, int len)
        {
            _ipStream.WriteAsync(bytes, 0, len);
        }

        public void WriteString(string data)
        {
            byte[] bytes = Encoding.Default.GetBytes(data);
            _ipStream.WriteAsync(bytes, 0, bytes.Length);
        }

        public void WriteCommand(string command)
        {
            _state = Comms.State.AwaitAck;

            if (command.Length == 1 && command != GrblConstants.CMD_PROGRAM_DEMARCATION)
                WriteByte((byte)command.ToCharArray()[0]);
            else
            {
                command += "\r";
                WriteString(command);
            }
        }

        public void AwaitAck()
        {
            while (Comms.com.CommandState == Comms.State.DataReceived || Comms.com.CommandState == Comms.State.AwaitAck)
                EventUtils.DoEvents();
        }

        public void AwaitAck(string command)
        {
            WriteCommand(command);

            while (Comms.com.CommandState == Comms.State.DataReceived || Comms.com.CommandState == Comms.State.AwaitAck) ;
        }

        public void AwaitResponse()
        {
            while (Comms.com.CommandState == Comms.State.AwaitAck)
                EventUtils.DoEvents();
        }
        public void AwaitResponse(string command)
        {
            WriteCommand(command);

            while (Comms.com.CommandState == Comms.State.AwaitAck) ;
        }

        public string GetReply(string command)
        {
            Reply = string.Empty;
            WriteCommand(command);

            while (_state == Comms.State.AwaitAck)
                EventUtils.DoEvents();

            return Reply;
        }

        private int Gp()
        {
            int pos = 0; bool found = false;

            while (!found && pos < input.Length)
                found = input[pos++] == '\n';

            return found ? pos - 1 : 0;
        }

        void ReadComplete(IAsyncResult iar)
        {
            int bytesAvailable = 0;
            byte[] buffer = (byte[])iar.AsyncState;

            try
            {
                bytesAvailable = _ipStream.EndRead(iar);
            }
            catch
            {
                // error handling required here (and many other places)...
            }

            int pos = 0;

            lock (input)
            {
                input.Append(Encoding.ASCII.GetString(buffer, 0, bytesAvailable));

                if (EventMode)
                {
                    while (input.Length > 0 && (pos = Gp()) > 0)
                    {
                        Reply = input.ToString(0, pos - 1);
                        input.Remove(0, pos + 1);
                        _state = Reply == "ok" ? Comms.State.ACK : (Reply.StartsWith("error") ? Comms.State.NAK : Comms.State.DataReceived);
                        if (Reply.Length != 0 && DataReceived != null)
                            Dispatcher.Invoke(DataReceived, Reply);
                    }
                }
                else
                    ByteReceived?.Invoke(ReadByte());

                if (_ipStream != null && _ipServer.Connected)
                    _ipStream.BeginRead(buffer, 0, buffer.Length, ReadComplete, buffer);
            }
        }
    }
}
