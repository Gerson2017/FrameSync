using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using ProtoBuf;
using Unity.VisualScripting;
using UnityEngine;

public static class NetManager
{
    
    public enum ServerType
    {
        /// <summary>
        /// 网关服务器
        /// </summary>
        GateWay,

        /// <summary>
        /// 战斗服务器
        /// </summary>
        Fighter,
    }
    
    /// <summary>
    /// 客户端套接字
    /// </summary>
    private static Socket _socket;

    private static ByteArray _byteArray;

    /// <summary>
    /// 消息队列 
    /// </summary>
   // private static List<MsgBase> _msgList;  //json
    private static List<IExtensible> _msgList; //protobuff
    
    /// <summary>
    /// 是否正在连接
    /// </summary>
    private static bool _isConnecting;

    /// <summary>
    /// 客户端是否正在关闭
    /// </summary>
    private static bool _isCloseing;

    /// <summary>
    /// 发送队列
    /// </summary>
    private static Queue<ByteArray> _writeQuene;

    /// <summary>
    /// 一帧最大的消息处理量
    /// </summary>
    private static int _processMsgCount = 10;


    /// <summary>
    /// 是否启用心跳机制
    /// </summary>
    private static bool _usePing = true;

    /// <summary>
    /// 上次发送ping的时间
    /// </summary>
    private static float _lastPingTime = 0;
    /// <summary>
    /// 上次收到pong的时间
    /// </summary>
    private static float _lastPongTime = 0;

    /// <summary>
    /// 心跳机制的时间间隔 秒
    /// </summary>
    private static float _pingInterval =2;// 30;

    /// <summary>
    /// 客户端udp
    /// </summary>
    private static UdpClient _udpClient;
    
    /// <summary>
    /// 网络事件
    /// </summary>
    public enum NetEvent
    {
        ConnectSuccess = 1,
        ConnectFail = 2,
        Close
    }

    /// <summary>
    /// 网络事件委托
    /// </summary>
    public delegate void NetEventListener(string error);

    /// <summary>
    /// 网络事件字典
    /// </summary>
    private static Dictionary<NetEvent, NetEventListener> _netEventListenerDic =
        new Dictionary<NetEvent, NetEventListener>();

    /// <summary>
    /// 添加网络事件
    /// </summary>
    /// <param name="netEvent">网络事件类型</param>
    /// <param name="listener">网络事件回调</param>
    public static void AddNetEventListener(NetEvent netEvent, NetEventListener listener)
    {
        if (_netEventListenerDic.ContainsKey(netEvent))
            _netEventListenerDic[netEvent] += listener;
        else
            _netEventListenerDic.Add(netEvent, listener);
    }

    /// <summary>
    /// 移除网络事件
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void RemoveNetEventListener(NetEvent netEvent, NetEventListener listener)
    {
        if (_netEventListenerDic.ContainsKey(netEvent))
        {
            _netEventListenerDic[netEvent] -= listener;
            if (_netEventListenerDic[netEvent] == null)
                _netEventListenerDic.Remove(netEvent);
        }
    }

    /// <summary>
    /// 派发网络事件
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="err"></param>
    public static void FireEvent(NetEvent netEvent, string err)
    {
        if (_netEventListenerDic.ContainsKey(netEvent))
        {
            _netEventListenerDic[netEvent].Invoke(err);
        }
    }

    /// <summary>
    /// 消息处理委托
    /// </summary>
    public delegate void MsgListener(IExtensible msgBase); 
  //  public delegate void MsgListener(MsgBase msgBase); //json方式
  
  

    /// <summary>
    /// 消息字典
    /// </summary>
    private static Dictionary<string, MsgListener> _msgListenerDic = new Dictionary<string, MsgListener>();


    /// <summary>
    /// 添加消息处理事件
    /// </summary>
    /// <param name="msgName"></param>
    /// <param name="listener"></param>
    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        if (_msgListenerDic.ContainsKey(msgName))
            _msgListenerDic[msgName] += listener;
        else
            _msgListenerDic.Add(msgName, listener);
    }

    /// <summary>
    /// 移除消息监听
    /// </summary>
    /// <param name="msgName"></param>
    /// <param name="listener"></param>
    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (_msgListenerDic.ContainsKey(msgName))
        {
            _msgListenerDic[msgName] -= listener;
            if (_msgListenerDic[msgName] == null)
                _msgListenerDic.Remove(msgName);
        }
    }

    /// <summary>
    /// 派发消息
    /// </summary>
    public static void FireMsg(string msgName, IExtensible msgBase)
    {
        if (_msgListenerDic.ContainsKey(msgName))
        {
            _msgListenerDic[msgName].Invoke(msgBase);
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    static void Init()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _byteArray = new ByteArray();
        _msgList = new List<IExtensible>();
        _writeQuene = new Queue<ByteArray>();
        _isConnecting = false;

        _lastPingTime = Time.time;
        _lastPongTime = Time.time;
        
        
    }

    /// <summary>
    /// 连接
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public static void Connect(string ip, int port)
    {
        if (_socket != null && _socket.Connected)
        {
            Debug.LogWarning("连接失败，当前正连接");
            return;
        }

        if (_isConnecting)
        {
            Debug.LogWarning("连接失败，正在连接");
            return;
        }

        _isConnecting = true;
        Init();
        //异步等待连接
        _socket.BeginConnect(ip, port, ConnectCallBack, _socket);
    }


    /// <summary>
    /// 链接回调
    /// </summary>
    /// <param name="asyncResult"></param>
    static void ConnectCallBack(IAsyncResult asyncResult)
    {
        try
        {
            Socket socket = (Socket)asyncResult.AsyncState;
            socket.EndConnect(asyncResult);
            Debug.Log("连接成功");

            _udpClient = new UdpClient((IPEndPoint)socket.LocalEndPoint);
            _udpClient.BeginReceive(ReceiveUdpCallBack, null);
            //通过Connect指定发送的远程端口
            _udpClient.Connect((IPEndPoint)socket.RemoteEndPoint);
            
            _isConnecting = false;
            FireEvent(NetEvent.ConnectSuccess, "");

            socket.BeginReceive(_byteArray.MBytes, _byteArray.MWriteIndex, _byteArray.MRemain, 0, ReceiveCallBack,
                _socket);
        }
        catch (SocketException e)
        {
            Debug.LogError(e.Message);
            FireEvent(NetEvent.ConnectFail, e.Message);
            _isConnecting = false;
        }
    }

  

    /// <summary>
    /// 消息接收回调
    /// </summary>
    /// <param name="ar"></param>
    static void ReceiveCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            //停止接收 并返回接收到的 数据量
            int count = socket.EndReceive(ar);
            if (count <= 0) //说明没有收到数据 断开了链接
            {
                Close();
                return;
            }

            //接收数据
            _byteArray.MWriteIndex += count; //可写入的位置往后移
            OnReceiveData();
            //如果剩余的容量过小 进行扩容
            if (_byteArray.MRemain < 8)
            {
                _byteArray.MoveBytes();
                _byteArray.ReSize(_byteArray.Length * 2);
            }

            //再次接收消息 实现循环接收
            socket.BeginReceive(_byteArray.MBytes, _byteArray.MWriteIndex, _byteArray.MRemain, 0, ReceiveCallBack,
                _socket);
        }
        catch (SocketException e)
        {
            Debug.LogError("接收消息失败 " + e.Message);
        }
    }


    /// <summary>
    /// 关闭客户端链接方法
    /// </summary>
    static void Close()
    {
        if (_socket == null || !_socket.Connected)
            return;
        if (_isConnecting)
        {
            Debug.LogError("客户端正在使用 无法关闭");
            return;
        }

        //Debug.LogError("消息队列还存在着未处理完的消息 无法关闭");
        if (_writeQuene.Count > 0)
            _isCloseing = true;
        else
        {
            _socket.Close();
            FireEvent(NetEvent.Close, "");
        }
    }


    /// <summary>
    /// 处理接收过来的消息
    /// </summary>
    private static void OnReceiveData()
    {
        //协议名都解析不出来
        if (_byteArray.Length <= 2)
            return;
        byte[] bytes = _byteArray.MBytes;
        int readIndex = _byteArray.MReadIndex;
        //解析消息总体的长度
        short length = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);


        if (_byteArray.Length < length + 2)
            return;
        _byteArray.MReadIndex += 2; //解析协议名
        int nameCount = 0;
        //json
        //   string protoName = MsgBase.DecodeProtoName(_byteArray.MBytes, _byteArray.MReadIndex, out nameCount);
        string protoName = ProtoBuffTool.DecodeProtoName(_byteArray.MBytes, _byteArray.MReadIndex, out nameCount);
        if (string.IsNullOrEmpty(protoName))
        {
            Debug.LogError("协议名解析失败");
            return;
        }

        _byteArray.MReadIndex += nameCount;

        //解析协议体
        int bodyLength = length - nameCount;
        //json
      //  MsgBase msgBase = MsgBase.Decode(protoName, _byteArray.MBytes, _byteArray.MReadIndex, bodyLength);
      IExtensible msgBase = ProtoBuffTool.Decode(protoName, _byteArray.MBytes, _byteArray.MReadIndex, bodyLength);
        _byteArray.MReadIndex += bodyLength;
        //解析完成后 向前移动数据
        _byteArray.MoveBytes();
        lock (_msgList) //多线程操作 防止数据错乱
        {
            _msgList.Add(msgBase);
        }

        //检查缓存中数据是否存在新的协议名
        if (_byteArray.Length > 2)
            OnReceiveData(); //循环调用
        // MsgTest test = (MsgTest)msgBase;
        // Debug.Log("OnReceiveData "+test.MProtoName);
    }


    /// <summary>
    /// 发送协议
    /// </summary>
    /// <param name="msg"></param>
    public static void Send(IExtensible msg,ServerType serverType)
    {
        if (_socket == null || !_socket.Connected)
            return;
        //如果是正在连接或者正在关闭 不发送
        if (_isCloseing || _isConnecting)
            return;

        //编码 json
        // byte[] nameBytes = MsgBase.EncodeProtoName(msg);
        // byte[] bodyBytes = MsgBase.Encode(msg);
        byte[] nameBytes = ProtoBuffTool.EncodeProtoName(msg);
        byte[] bodyBytes = ProtoBuffTool.Encode(msg);
        int len = nameBytes.Length +bodyBytes.Length+1; //消息体长度 1字节存储发送服务器编号
        byte[] sendBytes = new byte[len + 2]; //使用两个字节存储 len
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        sendBytes[2] = (byte)serverType;//服务器类型转成byte
        //nameBytes 拷贝进发送字节数组
        Array.Copy(nameBytes, 0, sendBytes, 3, nameBytes.Length);
        //bodyBytes 拷贝进发送字节数组
        Array.Copy(bodyBytes, 0, sendBytes, 3 + nameBytes.Length, bodyBytes.Length);
        //使用队列发送
        ByteArray array = new ByteArray(sendBytes);
        int count = 0;
        lock (_writeQuene) //防止线程抢夺
        {
            _writeQuene.Enqueue(array);
            count = _writeQuene.Count;
        }

        if (count == 1)
        {
            _socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, _socket);
        }
    }


    /// <summary>
    /// 发送回调
    /// </summary>
    /// <param name="ar"></param>
    static void SendCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            if (socket == null || !socket.Connected)
            {
                return;
            }

            int count = socket.EndSend(ar);
            ByteArray ba;
            lock (_writeQuene)
            {
                ba = _writeQuene.First();
            }

            ba.MReadIndex += count;
            //如果这个ByteArray已经发送完成
            if (ba.Length == 0)
            {
                lock (_writeQuene)
                {
                    //将消息去除
                    _writeQuene.Dequeue();
                    //队列中是否还存在消息
                    ba = _writeQuene.First();
                }
            }

            //还有消息要继续发送
            if (ba != null)
            {
                _socket.BeginSend(ba.MBytes, ba.MReadIndex, ba.MBytes.Length, 0, SendCallBack, _socket);
            }

            //所有消息发送完成后会走到这里 如果被标记为关闭 则关闭
            if (_isCloseing)
                _socket.Close();
        }
        catch (SocketException e)
        {
            Debug.LogError("SendCallBack " + e.Message);
        }
    }


    /// <summary>
    /// 处理消息
    /// </summary>
    private static void MsgUpdate()
    {
        if (_msgList.Count == 0)
            return;

        //如果消息一帧处理的数量过大则这这帧可能会卡住 所以限制一帧消息的处理
        for (int i = 0; i < _processMsgCount; i++)
        {
            IExtensible msgBase = null;
            lock (_msgList)
            {
                if (_msgList.Count>0)
                {
                    msgBase = _msgList[0];
                    _msgList.RemoveAt(0); 
                }
            }

            if (msgBase != null)
            {
                PropertyInfo propertyInfo = msgBase.GetType().GetProperty("protoName");
                if (propertyInfo != null)
                {
                    string protoNameStr = propertyInfo.GetValue(msgBase).ToString();
                    FireMsg(protoNameStr,  msgBase);
                }
            }
            else break;
        }
    }



    // private static void PingUpdate()
    // {
    //     if (!_usePing)
    //         return;
    //
    //     if (Time.time-_lastPingTime>_pingInterval)
    //     {
    //         //发送心跳ping
    //         MsgPing msg = new MsgPing();
    //         Send(msg,ServerType.GateWay);
    //         _lastPingTime = Time.time;
    //     }
    //     //断开处理  很长时间没有收到服务器发来的pong
    //     if (Time.time-_lastPongTime>_pingInterval*4)
    //     {
    //         Close();
    //     }
    //     
    // }


    public static void Update()
    {
       // PingUpdate();
        MsgUpdate();
    }


    #region UDP
    
    private static void ReceiveUdpCallBack(IAsyncResult ar)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
         byte[] bytes=_udpClient.EndReceive(ar, ref ipEndPoint);

       string protoName=  ProtoBuffTool.DecodeProtoName(bytes, 0, out int namecount);
       if (string.IsNullOrEmpty(protoName))
       {
           Debug.LogError(" ReceiveUdpCallBack 解析失败");
           return;
       }

       int bodyCount = bytes.Length - namecount;
       IExtensible msgBase = ProtoBuffTool.Decode(protoName,bytes,namecount,bodyCount);
       
       lock (_msgList)
       {
           if (msgBase!=null)
           {
               _msgList.Add(msgBase);
           }
       }

       _udpClient.BeginReceive(ReceiveUdpCallBack, null);

    }


    public static void UdpSendTo(IExtensible msgBase,ServerType serverType)
    {
        if (_udpClient==null)
        {
            Debug.LogError("UdpSendTo not found udpClient ");
            return;
        }
        byte[] nameBytes = ProtoBuffTool.EncodeProtoName(msgBase);
        byte[] bodyBytes = ProtoBuffTool.Encode(msgBase);
        int len = nameBytes.Length + bodyBytes.Length+1;
        byte[] sendBytes = new byte[len];

        sendBytes[0] = (byte)(serverType);
        Array.Copy(nameBytes,0,sendBytes,1,nameBytes.Length);
        Array.Copy(bodyBytes,0,sendBytes,nameBytes.Length +1,bodyBytes.Length);
        
        _udpClient.Send(sendBytes,sendBytes.Length);

    }
    

    #endregion
    
 
}


