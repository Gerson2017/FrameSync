using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

public class TestPanel : BasePanel
{
    private GameObject playerPrefab;
    public override void OnInit()
    {
        base.OnInit();
    }
    public override void OnShow(params object[] args)
    {
        base.OnShow(args);

        NetManager.AddMsgListener("MsgTest", OnMsgTest);
        NetManager.AddMsgListener("MsgStart", OnMsgStart);
        
        //测试udp
        MsgTest msgTest = new MsgTest();
        NetManager.UdpSendTo(msgTest, NetManager.ServerType.Fighter);

        MsgStart msgStart = new MsgStart();
        NetManager.Send(msgStart, NetManager.ServerType.Fighter);


        playerPrefab = Resources.Load<GameObject>("Hero/Player1");

    }
    public override void OnClose()
    {
        base.OnClose();
        PanelManager.Instance.Open<CtrlPanel>();
    }
    private void OnMsgTest(IExtensible msgBase)
    {
        Debug.Log("收到 OnMsgTest");
    }
    /// <summary>
    /// 开始游戏
    /// </summary>
    /// <param name="msgBase"></param>
    private void OnMsgStart(IExtensible msgBase)
    {
        MsgStart msgStart = (MsgStart)msgBase;
        if (!msgStart.res)
            return;
        for (int i = 0; i < msgStart.guid.Count; i++)
        {
            GameObject playerGo = Instantiate(playerPrefab);
            Player player = playerGo.AddComponent<Player>();
            player.guid = msgStart.guid[i];
            PlayerManager.Instance.Add(player.guid, player);
        }
        Close();
        Debug.Log("开启帧同步");
    }
}
