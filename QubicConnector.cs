using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class QubicConnector
{
    private const int PORT = 21841;
    private TcpClient? tcpClient;
    private NetworkStream? stream;
    private bool peerConnected = false;
    private string? connectedPeerAddress;
    private byte[] buffer = new byte[4 * 1024 * 1024];
    private int bufferWritePosition = 0;
    private int bufferReadPosition = 0;
    private int currentTick = 0;
    private Timer? timer;

    public event Action? OnReady;
    public event Action? OnPeerConnected;
    public event Action? OnPeerDisconnected;
    public event Action<int>? OnTick;

    public QubicConnector()
    {
    }

    public bool ConnectPeer(string ipAddress)
    {
        try
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(ipAddress, PORT);
            stream = tcpClient.GetStream();
            connectedPeerAddress = ipAddress;
            peerConnected = true;
            OnPeerConnected?.Invoke();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void DisconnectPeer()
    {
        if (tcpClient != null)
        {
            tcpClient.Close();
            tcpClient = null;
            stream = null;
            peerConnected = false;
            connectedPeerAddress = null;
            OnPeerDisconnected?.Invoke();
        }
    }

    public bool ReconnectPeer()
    {
        if (connectedPeerAddress != null)
        {
            DisconnectPeer();
            return ConnectPeer(connectedPeerAddress);
        }
        return false;
    }

    private void ProcessBuffer()
    {
        // Implementiere hier die Logik zum Verarbeiten der Daten im Puffer.
    }

    public void Start()
    {
        timer = new Timer(_ => RequestTickInfo(), null, 0, 500);
        OnReady?.Invoke();
    }

    public void Stop()
    {
        timer?.Dispose();
        DisconnectPeer();
    }

    private void RequestTickInfo()
    {
        if (peerConnected)
        {
            byte[] request = Encoding.ASCII.GetBytes("REQUEST_TICK_INFO"); // Beispiel für eine Anfrage
            SendPackage(request);
        }
    }

    public bool SendPackage(byte[] data)
    {
        if (stream == null || !peerConnected)
            return false;

        try
        {
            stream.Write(data, 0, data.Length);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
