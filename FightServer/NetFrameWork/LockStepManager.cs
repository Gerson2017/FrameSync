public class LockStepManager
{
    /// <summary>
    /// 上一帧时间
    /// </summary>
    private long _lastTime;

    /// <summary>
    /// 当前时间
    /// </summary>
    private long _curTime;

    /// <summary>
    /// 一帧的时间间隔
    /// </summary>
    private long _timeInterval = 20;


    /// <summary>
    /// 是否开启循环
    /// </summary>
    public bool IsStarted;


    /// <summary>
    /// 帧数
    /// </summary>
    public int MTurn;

    /// <summary>
    /// 运行的房间
    /// </summary>
    public Room MRoom;


    /// <summary>
    /// 记录所有操作 可以用作回放 key帧数 value 对应帧的操作
    /// </summary>
    public Dictionary<int, List<Opts>> MAllOpt = new Dictionary<int, List<Opts>>();


    /// <summary>
    /// 运行帧同步
    /// </summary>
    public void Run()
    {
        if (IsStarted)
            return;
        IsStarted = true;

        _lastTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        while (true)
        {
            _curTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;

            long time = _curTime - _lastTime;
            if (time > _timeInterval)
            {
                _lastTime = _curTime;
                MTurn++;

                MsgLockStepBack msgLockStepBack = new MsgLockStepBack();
                msgLockStepBack.turn = MTurn;

                foreach (var player in MRoom.MPlayerUnSyncOpt)
                {
                    List<UnsyncOpts> unsyncOptsList = new List<UnsyncOpts>();
                    //player.Value 当前玩家同步到的帧数 MTurn表示服务同步到的帧数
                    if (player.Value < MTurn) //有消息没有同步到
                    {
                        UnsyncOpts unsyncOpt = new UnsyncOpts();
                        for (int i = player.Value; i < MTurn; i++)
                        {
                            unsyncOpt.turn = i;
                            if (MAllOpt.ContainsKey(i))
                                unsyncOpt.opts = MAllOpt[i].ToArray();
                        }

                        //将所有未同步的添加到列表中
                        unsyncOptsList.Add(unsyncOpt);
                    }

                    msgLockStepBack.unsyncOpts = unsyncOptsList.ToArray();
                    //整合为一个打包 发送给客户端 这里因为proto问题 先注释掉
                    //NetManager.Send(msgLockStepBack,player.Key);
                }
            }
        }
    }
}