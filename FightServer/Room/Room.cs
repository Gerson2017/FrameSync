

using ProtoBuf;
#nullable disable

public class Room
{
    /// <summary>
    /// 房间号
    /// </summary>
    public int MId;

    /// <summary>
    /// 房间最大人数
    /// </summary>
    public int MaxPlayer = 2;

    /// <summary>
    /// 玩家列表
    /// </summary>
    public List<uint> MPlayerIdList = new List<uint>();

    /// <summary>
    /// 帧同步的对象
    /// </summary>
    public LockStepManager MLockStepManager = new LockStepManager();


    /// <summary>
    /// 记录玩家同步到的帧数 key是玩家uid value是帧数
    /// </summary>
    public Dictionary<uint, int> MPlayerUnSyncOpt = new Dictionary<uint, int>();
    
    
    public void StartLockStep()
    {
        //开启新线程运行帧同步
        Thread thread = new Thread(MLockStepManager.Run);
        thread.Start();
    }
    
    /// <summary>
    /// 添加玩家ID
    /// </summary>
    public bool AddPlayer(uint guid)
    {
        Player player = PlayerManager.GetPlayer(guid);
        if (player==null)
        {
            Console.WriteLine("AddPlayer AddPlayer 失败 玩家为空");
            return false;
        }

        if (MPlayerIdList.Count>MaxPlayer)
        {
            Console.WriteLine("AddPlayer 失败 房间已满");
            return false;
        }

        if (MPlayerIdList.Contains(guid))
        {
            Console.WriteLine("AddPlayer 失败 玩家已在房间");
            return false;
        }

        MPlayerIdList.Add(guid);
        player.MRoomId = MId;
        
        MPlayerUnSyncOpt.Add(guid,0);
        return true;

    }
    
    
    /// <summary>
    /// 向所有玩家进行tcp消息广播
    /// </summary>
    /// <param name="msg"></param>
    public void TcpBroadCast(IExtensible msg)
    {
        for (int i = 0; i < MPlayerIdList.Count; i++)
        {
            Player player = PlayerManager.GetPlayer(MPlayerIdList[i]);
            player.Send(msg);
        }
    }

    /// <summary>
    /// 向所有玩家进行Udp消息广播
    /// </summary>
    /// <param name="msg"></param>
    public void UdpBroadCast(IExtensible msg)
    {
        for (int i = 0; i < MPlayerIdList.Count; i++)
        {
            Player player = PlayerManager.GetPlayer(MPlayerIdList[i]);
            player.UdpSend(msg);
        };
    }

    
    
}