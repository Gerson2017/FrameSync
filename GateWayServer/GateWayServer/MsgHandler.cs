
using ProtoBuf;

public class MsgHandler
{
    
    
    // public static void MsgPing(ClientState client,MsgBase msgBase)
    // {
    //     Console.WriteLine("msgPing "+client.MSocket.RemoteEndPoint);
    //     client.LastPingTime = ServerNetManager.GetTimeStamp();
    //
    //     //json方式
    //     // MsgPong msgPong = new MsgPong();
    //     // ServerNetManager.Send(client,msgPong);
    //     
    //     
    //
    // }
    
    public static void MsgPing(ClientState client,IExtensible msgBase)
    {
        Console.WriteLine("msgPing "+client.MSocket.RemoteEndPoint);
        client.LastPingTime = GateWay.GetTimeStamp();
        
        MsgPong msgPong = new MsgPong();
        GateWay.Send(client,msgPong);
    }
    
}