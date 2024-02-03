namespace FrameSyncServer;

public class MainClass
{
    public static void Main()
    {
        //主线程会直接结束
        NetManager.Connect("127.0.1",9000);
        //防止主线程直接结束
        while (true) ;
    }
}