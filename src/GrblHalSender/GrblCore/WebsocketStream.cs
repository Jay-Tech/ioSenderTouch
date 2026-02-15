
using System.Net.WebSockets;
using System.Text;
using System.Windows.Threading;


namespace GrblHalSender.GrblCore;

public class WebsocketStream : StreamComms
{
    public event DataReceivedHandler DataReceived;

    private readonly string _basePortParams;
    private readonly Dispatcher _dispatcher;
    private ClientWebSocket _ws;
    private StringBuilder input = new StringBuilder(1024);
    private volatile Comms.State _state = Comms.State.ACK;
    public bool IsOpen { get; }
    public int OutCount { get; }
    public string Reply { get; set; }
    public Comms.StreamType StreamType { get; }
    public Comms.State CommandState { get; set; }
    public bool EventMode { get; set; }
    
    public Action<int> ByteReceived { get; set; }
   

    public WebsocketStream(string basePortParams, Dispatcher dispatcher)
    {
        _basePortParams = basePortParams;
        _dispatcher = dispatcher;
        var results =  BuildClient();
    }

    public async Task<bool> BuildClient()
    {
        _ws = new ClientWebSocket();
        await _ws.ConnectAsync(new Uri(_basePortParams), CancellationToken.None);
        switch (_ws.State)
        {
            case WebSocketState.Open:
                await Connect();
                return true;
            case WebSocketState.Closed:
                return false;
            case WebSocketState.None:
            case WebSocketState.Connecting:
            case WebSocketState.CloseSent:
            case WebSocketState.CloseReceived:
            case WebSocketState.Aborted:
            default:
                return false;
        }
    }

    public async Task Connect()
    {
        var receiveTask = Task.Run( async () =>
        {
            var buffer = new byte[1024];
            while (true)
            {
                var result = _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.Result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                var message = Encoding.UTF8.GetString(buffer, 0, result.Result.Count);

                Console.WriteLine($"{message}\r\n");
                _dispatcher.Invoke(DataReceived, Reply);
            }

            return Task.CompletedTask;
        });
        await receiveTask;
    }

    public  async Task CloseWebSocketAsync(ClientWebSocket ws)
    {
        if (ws.State == WebSocketState.Open)
        {
            try
            {
                // Initiate the close handshake
                await ws.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, // Status code
                    "Client closed the connection normally.", // Description
                    CancellationToken.None); // Cancellation token

                Console.WriteLine("WebSocket connection closed gracefully.");
            }
            catch (WebSocketException ex)
            {
                // Handle exceptions that might occur during the close handshake
                Console.WriteLine($"WebSocket exception during close: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other potential exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Cannot close: WebSocket is in state {ws.State}");
        }
    }

    public void Send(byte[] message)
    {
        _ws.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    public void Send(string message)
    {
        byte[] bytes = Encoding.Default.GetBytes(message);
        _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

   
    public void Close()
    {
        try
        {
            var c = CloseWebSocketAsync(_ws);
            _ws.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
        var d = new byte[1] { data };
        Send(d);
    }

    public void WriteBytes(byte[] bytes, int len)
    {
        Send(bytes);
    }

    public void WriteString(string data)
    {
        byte[] bytes = Encoding.Default.GetBytes(data);
        Send(bytes);
    }

    public void WriteCommand(string command)
    {
        
        if (command.Length == 1 && command != GrblConstants.CMD_PROGRAM_DEMARCATION)
            WriteByte((byte)command.ToCharArray()[0]);
        else
        {
            command += "\r";
            WriteString(command);
        }
    }

    public string GetReply(string command)
    {
        Reply = string.Empty;
        WriteCommand(command);

        return Reply;
    }

    public void AwaitAck()
    {
        
    }

    public void AwaitAck(string command)
    {
        WriteCommand(command);

      
    }

    public void AwaitResponse(string command)
    {
        WriteCommand(command);
       
    }

    public void AwaitResponse()
    {
       
    }

    public void PurgeQueue()
    {
       
    }

}