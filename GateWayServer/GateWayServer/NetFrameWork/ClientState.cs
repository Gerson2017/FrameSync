
#nullable disable

using System.Net.Sockets;

/// <summary>
/// 描述客户端的对象
/// </summary>
public class ClientState
{

    public uint MGuid;
    
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


    public byte[] GeneralGuidBytes()
    {
        byte[] guidBytes = new byte[4];
        guidBytes[0] = (byte)(MGuid >> 24); //guid转成字节 
        guidBytes[1] = (byte)((MGuid>> 16) & 0xff); //前面的位数全部归0
        guidBytes[2] = (byte)((MGuid >> 8) & 0xff);
        guidBytes[3] = (byte)((MGuid) & 0xff);
        return guidBytes;
    }

}