
using ProtoBuf;

public class MsgHandler
{
    
    
    
    public static void MsgTest(uint guid,IExtensible msgBase)
    {
        Console.WriteLine("MsgTest");
        NetManager.UdpSendTo(msgBase,guid);
        
    }

    public static void MsgStart(uint guid,IExtensible msgBase)
    {
        MsgStart msgStart = (MsgStart)msgBase;
        Console.WriteLine("MsgStart");

        Player player = new Player(guid);
        PlayerManager.AddPlayer(guid,player);
        if (PlayerManager.MPlayersDic.Count==1)
        {
            Room room = RoomManager.AddRoom();
            room.MId = 1;
            room.AddPlayer(guid);
        }
        else
        {
            Room room = RoomManager.GetRoom(1);
            room.AddPlayer(guid);

            room.StartLockStep();
            
            msgStart.res = true;
            for (int i = 0; i < room.MPlayerIdList.Count; i++)
            {
                msgStart.guid.Add(room.MPlayerIdList[i]);
            }
            room.TcpBroadCast(msgStart);
        }
    }

    
    /// <summary>
    /// 获取信息
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetInfo(uint guid,IExtensible msgBase)
    {
        MsgGetInfo msg=(MsgGetInfo)msgBase;
        msg.guid = guid;
        NetManager.Send(msgBase,guid);

    }


    public static void MsgLockStep(uint guid,MsgBase msgBase)
    {
        MsgLockStep msg=(MsgLockStep) msgBase;

        Player player = PlayerManager.GetPlayer(guid);
        if (player==null)
            return;

        Room room = RoomManager.GetRoom(player.MRoomId);
        if (room==null)
            return;

        LockStepManager lockStepManager = room.MLockStepManager;

        lock (lockStepManager)
        {
            if (!lockStepManager.MAllOpt.ContainsKey(msg.turn))
            {
                lockStepManager.MAllOpt[msg.turn] = new List<Opts>();
            }
            for (int i = 0; i < msg.opts.Length; i++)
            {
                lockStepManager.MAllOpt[msg.turn].Add(msg.opts[i]);
            }
            
            //客户端已经同步到了这一帧 客户端操作的时候是会+1
            room.MPlayerUnSyncOpt[guid] = msg.turn - 1;
        }
    } 
    
}