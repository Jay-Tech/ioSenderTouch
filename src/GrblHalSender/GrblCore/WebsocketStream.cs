
using System.Text;
using System.Windows.Threading;
using WebSocketSharp;

namespace GrblHalSender.GrblCore;



    public class WebsocketStream : StreamComms
    {
        private WebSocket websocket = null;
        private volatile bool _isOpen = false;
        private volatile Comms.State state = Comms.State.ACK;
        private StringBuilder input = new StringBuilder(1024);
        private Dispatcher Dispatcher { get; set; }

        public event DataReceivedHandler DataReceived;

        public WebsocketStream(string host, Dispatcher dispatcher)
        {
            Comms.com = this;
            Reply = string.Empty;
            Dispatcher = dispatcher;

            try
            {
                websocket = new WebSocket(host);
                websocket.OnMessage += OnMessage;
                websocket.OnOpen += OnOpen;
                websocket.OnClose += OnClose;
                websocket.Connect();
            }
            catch
            {
            }
        }

        ~WebsocketStream()
        {
            Close();
        }

        public Comms.StreamType StreamType { get { return Comms.StreamType.Websocket; } }
        public bool IsOpen { get { return websocket != null && _isOpen; } }
        public int OutCount { get { return 0; } }
        public Comms.State CommandState { get { return state; } set { state = value; } }
        public string Reply { get; private set; }
        public bool EventMode { get; set; } = true;
        public Action<int> ByteReceived { get; set; }

        public void PurgeQueue()
        {
            Reply = string.Empty;
            if (!EventMode)
                input.Clear();
        }

        public void Close()
        {
            if (IsOpen)
            {
                websocket.OnMessage -= OnMessage;
                websocket.OnOpen -= OnOpen;
                websocket.Close();
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
            var m = new byte[1] { data };
            websocket?.Send(m);
        }

        public void WriteBytes(byte[] bytes, int len)
        {
            websocket?.Send(bytes);
        }

        public void WriteString(string data)
        {
            byte[] bytes = Encoding.Default.GetBytes(data);

            websocket?.Send(bytes);
        }

        public void WriteCommand(string command)
        {
            state = Comms.State.AwaitAck;

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

            while (state == Comms.State.AwaitAck)
                EventUtils.DoEvents();

            return Reply;
        }

        private void OnOpen(object sender, EventArgs e)
        {
            _isOpen = true;
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            _isOpen = false;
            websocket.OnClose -= OnClose;
            websocket = null;
        }

        private int gp()
        {
            int pos = 0; bool found = false;

            while (!found && pos < input.Length)
                found = input[pos++] == '\n';

            return found ? pos - 1 : 0;
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            int pos = 0;

            lock (input)
            {
                if (e.IsText)
                    input.Append(e.Data);
                else
                    input.Append(Encoding.Default.GetString(e.RawData, 0, e.RawData.Length));

                if (EventMode)
                {
                    while (input.Length > 0 && (pos = gp()) > 0)
                    {
                        Reply = input.ToString(0, pos - 1);
                        input.Remove(0, pos + 1);
                        state = Reply == "ok" ? Comms.State.ACK : (Reply.StartsWith("error") ? Comms.State.NAK : Comms.State.DataReceived);
                        if (Reply.Length != 0 && DataReceived != null)
                            Dispatcher.Invoke(DataReceived, Reply);
                    }
                }
                else
                    ByteReceived?.Invoke(ReadByte());
            }
        }
    }


//public class WebSocketClient
//{
//    private ClientWebSocket _ws;

//    public WebSocketClient()
//    {
        
//    }

//    public async Task<bool> BuildClient()
//    {
//        _ws = new ClientWebSocket();
//        await _ws.ConnectAsync(new Uri("ws://192.168.5.1:80/ws"), CancellationToken.None);
//        switch (_ws.State)
//        {
//            case WebSocketState.Open:
//                await Connect();
//                return true;
//            case WebSocketState.Closed:
//                return false;
//            case WebSocketState.None:
//            case WebSocketState.Connecting:
//            case WebSocketState.CloseSent:
//            case WebSocketState.CloseReceived:
//            case WebSocketState.Aborted:
//            default:
//                return false;
//        }
//    }

//    public async Task Connect()
//    {
//        var  receiveTask =  Task.Run(async () =>
//        {
//            var buffer = new byte[1024];
//            while (true)
//            {
//                var result = _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
//                if (result.Result.MessageType == WebSocketMessageType.Close)
//                {
//                    break;
//                }
//                var message = Encoding.UTF8.GetString(buffer, 0, result.Result.Count);
//                Console.WriteLine($"{message}\r\n");
//            }
            
//        });
//        await receiveTask;
//    }

//    public void Send(byte [] message)
//    {
//        _ws.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
//    }
//    public void Send(string message)
//    {
//        byte[] bytes = Encoding.Default.GetBytes(message);
//        _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text,true, CancellationToken.None );
//    }
    
//}