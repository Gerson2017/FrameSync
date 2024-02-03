using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public uint guid;
    private void Start()
    {
        NetManager.AddMsgListener("MsgGetInfo", OnMsgGetInfo);
        NetManager.Send(new MsgGetInfo(), NetManager.ServerType.Fighter);
    }
    private void OnMsgGetInfo(IExtensible msg)
    {
        MsgGetInfo msgGetInfo = (MsgGetInfo)msg;
        guid = msgGetInfo.guid;
    }
}
