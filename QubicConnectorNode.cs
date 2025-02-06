using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class QubicConnectorNode
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
    public event Action<QubicEntityResponse>? OnBalance;
    public event Action<int>? OnTick;
    public event Action<ReceivedPackage>? OnPackageReceived;
    public event Action<Exception>? OnSocketError;

    public QubicConnectorNode()
    {
        tcpClient = new TcpClient();
        tcpClient.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), null);
    }

    private void OnDataReceived(IAsyncResult ar)
    {
        try
        {
            int bytesRead = tcpClient.Client.EndReceive(ar);
            if (bytesRead > 0)
            {
                WriteBuffer(new ArraySegment<byte>(buffer, 0, bytesRead).ToArray());
                tcpClient.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), null);
            }
            else
            {
                OnPeerDisconnected?.Invoke();
            }
        }
        catch (Exception ex)
        {
            OnSocketError?.Invoke(ex);
        }
    }

    private void OnPeerConnect()
    {
        peerConnected = true;
        OnPeerConnected?.Invoke();
    }

    private bool ConnectPeer(string ipAddress)
    {
        try
        {
            tcpClient.Connect(ipAddress, PORT);
            stream = tcpClient.GetStream();
            OnPeerConnect();
            connectedPeerAddress = ipAddress;
            return true;
        }
        catch (Exception ex)
        {
            OnSocketError?.Invoke(ex);
            return false;
        }
    }

    private void DisconnectPeer()
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

    private bool ReconnectPeer()
    {
        DisconnectPeer();
        if (connectedPeerAddress != null)
        {
            return ConnectPeer(connectedPeerAddress);
        }
        return false;
    }

    private void WriteBuffer(byte[] data)
    {
        int writeLength = data.Length;
        if (bufferWritePosition + data.Length > buffer.Length)
            writeLength = buffer.Length - bufferWritePosition;

        Array.Copy(data, 0, buffer, bufferWritePosition, writeLength);
        bufferWritePosition += writeLength;

        if (writeLength < data.Length)
        {
            bufferWritePosition = 0;
            Array.Copy(data, writeLength, buffer, bufferWritePosition, data.Length - writeLength);
            bufferWritePosition += data.Length - writeLength;
        }

        ProcessBuffer();
    }

    private byte[] ReadFromBuffer(int numberOfBytes, bool setReadPosition = false)
    {
        byte[] extract = new byte[numberOfBytes];
        if (bufferReadPosition + numberOfBytes <= buffer.Length)
        {
            Array.Copy(buffer, bufferReadPosition, extract, 0, numberOfBytes);
        }
        else
        {
            int firstPartLength = buffer.Length - bufferReadPosition;
            Array.Copy(buffer, bufferReadPosition, extract, 0, firstPartLength);
            Array.Copy(buffer, 0, extract, firstPartLength, numberOfBytes - firstPartLength);
        }
        if (setReadPosition)
            SetReadPosition(numberOfBytes);

        return extract;
    }

    private void SetReadPosition(int numberOfReadBytes)
    {
        if (bufferReadPosition + numberOfReadBytes > buffer.Length)
            bufferReadPosition = (bufferReadPosition + numberOfReadBytes) - buffer.Length;
        else
            bufferReadPosition += numberOfReadBytes;
    }

    private void ProcessBuffer()
    {
        while (true)
        {
            int toReadBytes = Math.Abs(bufferWritePosition - bufferReadPosition);
            if (toReadBytes < 8) // header size
                break;

            RequestResponseHeader header = new RequestResponseHeader();
            header.Parse(ReadFromBuffer(8)); // header size
            if (header == null || toReadBytes < header.GetSize())
                break;

            SetReadPosition(header.GetPackageSize());
            ReceivedPackage recPackage = new ReceivedPackage
            {
                Header = header,
                IpAddress = connectedPeerAddress ?? ""
            };
            if (header.GetSize() > 8)
            {
                recPackage.PayLoad = ReadFromBuffer(header.GetSize() - header.GetPackageSize(), true);
            }
            else
            {
                recPackage.PayLoad = new byte[0];
            }
            ProcessPackage(recPackage);
            OnPackageReceived?.Invoke(recPackage);
        }
    }

    private void ProcessPackage(ReceivedPackage p)
    {
        if (p.Header.GetType() == QubicPackageType.RespondCurrentTickInfo)
        {
            QubicTickInfo tickInfo = new QubicTickInfo().Parse(p.PayLoad);
            if (tickInfo != null && currentTick < tickInfo.GetTick())
            {
                currentTick = tickInfo.GetTick();
                OnTick?.Invoke(currentTick);
            }
        }
        else if (p.Header.GetType() == QubicPackageType.RespondEntity && OnBalance != null)
        {
            QubicEntityResponse entityResponse = new QubicEntityResponse().Parse(p.PayLoad);
            OnBalance(entityResponse);
        }
    }

    private void RequestTickInfo()
    {
        if (peerConnected)
        {
            RequestResponseHeader header = new RequestResponseHeader(QubicPackageType.RequestCurrentTickInfo);
            header.RandomizeDejaVu();
            SendPackage(header.GetPackageData());
        }
    }

    public void RequestBalance(PublicKey pkey)
    {
        if (!peerConnected)
            return;

        RequestResponseHeader header = new RequestResponseHeader(QubicPackageType.RequestEntity, pkey.GetPackageSize());
        header.RandomizeDejaVu();
        QubicPackageBuilder builder = new QubicPackageBuilder(header.GetSize());
        builder.Add(header);
        builder.Add(new QubicEntityRequest(pkey));
        byte[] data = builder.GetData();
        SendPackage(data);
    }

    public async Task<(byte[] privateKey, byte[] publicKey)> GetPrivatePublicKey(byte[] seed)
    {
        var crypto = await Crypto.GetInstance();
        KeyHelper keyHelper = new KeyHelper();

        byte[] privateKey = keyHelper.PrivateKey(seed, 0, crypto.K12);
        byte[] publicKey = keyHelper.CreatePublicKey(privateKey, crypto.Schnorrq, crypto.K12);

        return (privateKey, publicKey);
    }

    private void Initialize()
    {
        bufferReadPosition = 0;
        bufferWritePosition = 0;

        timer = new Timer(_ => RequestTickInfo(), null, 0, 500);
        OnReady?.Invoke();
    }

    public void Connect(string ip)
    {
        ConnectPeer(ip);
    }

    public bool SendPackage(byte[] data)
    {
        return SendTcpPackage(data);
    }

    private bool SendTcpPackage(byte[] data)
    {
        if (!peerConnected)
            return false;

        try
        {
            stream?.Write(data, 0, data.Length);
            return true;
        }
        catch (Exception ex)
        {
            OnSocketError?.Invoke(ex);
            return false;
        }
    }

    public void Start()
    {
        Initialize();
    }

    public void Stop()
    {
        timer?.Dispose();
        DisconnectPeer();
    }

    public void Destroy()
    {
        Stop();
        tcpClient?.Close();
    }
}
