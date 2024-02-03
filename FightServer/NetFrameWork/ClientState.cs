
#nullable disable

using System.Net.Sockets;

/// <summary>
/// 描述客户端的对象
/// </summary>
public class ClientState
{
    /// <summary>
    /// 客户端Socket
    /// </summary>
    public Socket MSocket;

    /// <summary>
    /// 客户端的缓存区 
    /// </summary>
    public ByteArray MReadBuffer=new ByteArray();

    /// <summary>
    /// 上一次服务端收到ping的时间戳
    /// </summary>
    public long LastPingTime;

}