using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlPanel : BasePanel
{
   // private ETCJoystick joystick;

    private int currTurn;
    public override void OnInit()
    {
        base.OnInit();
    }
    public override void OnShow(params object[] args)
    {
        base.OnShow(args);
     //   joystick = skin.transform.Find("Joystick").GetComponent<ETCJoystick>();

      //  NetManager.AddMsgListener("MsgLockStepBack", OnMsgLockStepBack);
    }
    public override void OnClose()
    {
        base.OnClose();
    }
    //private void Update()
    //{
    //    GetJoystick();
    //}
    private Opts GetJoystick()
    {
        Opts opts = new Opts();
        opts.operation = Operation.Joystick;
        opts.param = new Fixed64[2];
        // opts.param[0] = (Fixed64)joystick.axisX.axisValue;
        // opts.param[1] = (Fixed64)joystick.axisY.axisValue;
        // if (joystick.axisX.axisValue == 0 && joystick.axisY.axisValue == 0)
        //     return null;
        return opts;
    }
    
    /// <summary>
    /// 客户端被动收到消息后 将信息发送给服务端
    /// </summary>
    /// <param name="msg"></param>
    private void OnMsgLockStepBack(MsgBase msg)
    {
        //ͬ��֮ǰ���߼�֡
        MsgLockStepBack msgLockStepBack = (MsgLockStepBack)msg;
        //Debug.Log("msgLockStepBack.turn:"+ msgLockStepBack.turn);
        if (msgLockStepBack.turn < currTurn)
        {
            return;
        }
        for (int i = 0; i < msgLockStepBack.unsyncOpts.Length; i++)
        {
            OnOpts(msgLockStepBack.unsyncOpts[i]);
        }
        
        
        currTurn = msgLockStepBack.turn;
        
        MsgLockStep msgLockStep = new MsgLockStep();
        msgLockStep.turn = currTurn + 1;
        List<Opts> opts = new List<Opts>();
        Opts joysstickOpt = GetJoystick();
        if(joysstickOpt != null)
        {
            opts.Add(joysstickOpt);
        }
        msgLockStep.opts=opts.ToArray();
        
      //  NetManager.UdpSendTo(msgLockStep, NetManager.ServerType.Fighter);
    }
    private void OnOpts(UnsyncOpts unsyncOpts)
    {
        Debug.Log(123);
        for (int i = 0; i < unsyncOpts.opts.Length; i++)
        {
            foreach (var item in PlayerManager.Instance.players)
            {
                if (item.Key == unsyncOpts.opts[i].guid)
                {
                    item.Value.OnOpts(unsyncOpts.opts[i]);
                }
            }
        }
    }
}
