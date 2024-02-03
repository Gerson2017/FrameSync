namespace FrameSyncServer;

#nullable  disable

public class MainClass
{

    public static ServerState MFightServer;
    
    public static void Main()
    {
        MFightServer=  GateWay.ConnectServer("127.0.1",9000);
        GateWay.MType2ServerDic.Add(GateWay.ServerType.Fighter,MFightServer);
        
        GateWay.Connect("127.0.1",8888);
    }
}