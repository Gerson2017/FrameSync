using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetManager.AddNetEventListener(NetManager.NetEvent.ConnectSuccess,OnNetEvtConnectSuccess);
        NetManager.AddNetEventListener(NetManager.NetEvent.ConnectFail,OnNetEvtConnectFail);
        NetManager.AddNetEventListener(NetManager.NetEvent.Close,OnNetEvtConnectClose);
       
        NetManager.Connect("127.0.1",8888);
        PanelManager.Instance.Open<TestPanel>();
        
    }

   


    private void Update()
    {
        NetManager.Update();
    }

    // private void Test()
    // {
    //     MsgTest msgTest = new MsgTest();
    //     NetManager.Send(msgTest, NetManager.ServerType.Fighter);
    // }

    
    private void OnMsgTest(IExtensible msgbase)
    {
        Debug.Log("OnMsgTest 收到");
    }
    
    private void OnNetEvtConnectSuccess(string error)
    {
        Debug.Log("Main 连接成功");
    }
    
    
    private void OnNetEvtConnectFail(string error)
    {
        Debug.Log("连接失败 "+error);
    }
    
    private void OnNetEvtConnectClose(string error)
    {
        Debug.Log("连接关闭 "+error);
    }
    
}
