

using ProtoBuf;

public class Player
{
    public uint MGuid;
    /// <summary>
    /// 所在房间
    /// </summary>
    public int MRoomId=-1;

    public Player(uint guid)
    {
        this.MGuid = guid;
    }

    /// <summary>
    /// 发送
    /// </summary>
    /// <param name="msg"></param>
    public void Send(IExtensible msg)
    {
        NetManager.Send(msg,MGuid);
    }

    /// <summary>
    /// UDP发送
    /// </summary>
    /// <param name="msg"></param>
    public void UdpSend(IExtensible msg)
    {
        NetManager.UdpSendTo(msg,MGuid);
    }
    
}