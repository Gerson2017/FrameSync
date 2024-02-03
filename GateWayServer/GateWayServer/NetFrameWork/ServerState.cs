
#nullable disable

using System.Net.Sockets;

/// <summary>
/// 描述客户端的对象
/// </summary>
public class ServerState
{
    /// <summary>
    /// 服务器socket
    /// </summary>
    public Socket MSocket;

    /// <summary>
    /// 客户端的缓存区 
    /// </summary>
    public ByteArray MReadBuffer=new ByteArray();


    /// <summary>
    /// 服务端类型
    /// </summary>
    public GateWay.ServerType MServerType;




}