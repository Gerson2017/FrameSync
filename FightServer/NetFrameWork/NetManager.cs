using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using ProtoBuf;

#nullable disable

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
    private static float _pingInterval = 2; // 30;


    /// <summary>
    /// udp
    /// </summary>
    private static UdpClient _udpClient;


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
        _writeQuene = new Queue<ByteArray>();
        _isConnecting = false;
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
            Console.WriteLine("连接失败，当前正连接");
            return;
        }

        if (_isConnecting)
        {
            Console.WriteLine("连接失败，正在连接");
            return;
        }

        _isConnecting = true;
        Init();

        Console.WriteLine("ip " + ip + " ，正在连接 " + port);
        //异步等待连接
        _socket.BeginConnect(ip, port, ConnectCallBack, _socket);
    }


    /// <summary>
    /// 链接回调
    /// </summary>
    /// <param name="asyncResult"></param>
    static void ConnectCallBack(IAsyncResult asyncResult)
    {
        Console.WriteLine("ConnectCallBack");
        try
        {
            Socket socket = (Socket)asyncResult.AsyncState;
            socket.EndConnect(asyncResult);
            Console.WriteLine("连接成功");
            _udpClient = new UdpClient((IPEndPoint)socket.LocalEndPoint);
            //链接网关
            _udpClient.Connect((IPEndPoint)socket.RemoteEndPoint);
            _udpClient.BeginReceive(ReceiveUdpCallBack, null);

            _isConnecting = false;


            socket.BeginReceive(_byteArray.MBytes, _byteArray.MWriteIndex, _byteArray.MRemain, 0, ReceiveCallBack,
                _socket);
        }
        catch (SocketException e)
        {
            Console.WriteLine("ConnectCallBack " + e.Message);
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
            Console.WriteLine("接收消息失败 " + e.Message);
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
            Console.WriteLine("客户端正在使用 无法关闭");
            return;
        }

        //Debug.LogError("消息队列还存在着未处理完的消息 无法关闭");
        if (_writeQuene.Count > 0)
            _isCloseing = true;
        else
        {
            _socket.Close();
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

        //解析guid
        uint guid = (uint)(bytes[readIndex + 2] << 24 |
                           bytes[readIndex + 3] << 16 |
                           bytes[readIndex + 4] << 8 |
                           bytes[readIndex + 5]);


        _byteArray.MReadIndex += 6;

        int nameCount = 0;
        //json
        //   string protoName = MsgBase.DecodeProtoName(_byteArray.MBytes, _byteArray.MReadIndex, out nameCount);
        string protoName = ProtoBuffTool.DecodeProtoName(_byteArray.MBytes, _byteArray.MReadIndex, out nameCount);
        if (string.IsNullOrEmpty(protoName))
        {
            Console.WriteLine("协议名解析失败");
            return;
        }

        _byteArray.MReadIndex += nameCount;
        //解析协议体 再减去guid的长度
        int bodyLength = length - nameCount - 4;
        //json
        //  MsgBase msgBase = MsgBase.Decode(protoName, _byteArray.MBytes, _byteArray.MReadIndex, bodyLength);
        IExtensible msgBase = ProtoBuffTool.Decode(protoName, _byteArray.MBytes, _byteArray.MReadIndex, bodyLength);
        _byteArray.MReadIndex += bodyLength;
        //解析完成后 向前移动数据
        _byteArray.MoveBytes();

        MethodInfo mi = typeof(MsgHandler).GetMethod(protoName);
        if (mi != null)
        {
            object[] o = { guid, msgBase };
            mi.Invoke(null, o);
        }
        else
        {
            Console.WriteLine("OnReceive Data Fail 反射失败");
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
    public static void Send(IExtensible msg, uint guid)
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
        int len = nameBytes.Length + bodyBytes.Length + 4; //消息体长度 1字节存储发送服务器编号
        byte[] sendBytes = new byte[len + 2]; //使用两个字节存储 len
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        //开始存储guid
        sendBytes[2] = (byte)(guid >> 24);
        sendBytes[3] = (byte)((guid >> 16) & 0xff);
        sendBytes[4] = (byte)((guid >> 8) & 0xff);
        sendBytes[5] = (byte)((guid) & 0xff);
        //nameBytes 拷贝进发送字节数组 前两位存大小 后4位存存量guid
        Array.Copy(nameBytes, 0, sendBytes, 6, nameBytes.Length);
        //bodyBytes 拷贝进发送字节数组
        Array.Copy(bodyBytes, 0, sendBytes, 6 + nameBytes.Length, bodyBytes.Length);

        _socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, _socket);
    }


    /// <summary>
    /// 发送回调
    /// </summary>
    /// <param name="ar"></param>
    static void SendCallBack(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        if (socket == null || !socket.Connected)
            return;

        int count = socket.EndSend(ar);
    }


    #region UDP

    private static void ReceiveUdpCallBack(IAsyncResult ar)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receiveBuff = _udpClient.EndReceive(ar, ref ipEndPoint);

        uint guid = (uint)(receiveBuff[0] << 24 | receiveBuff[1] << 16 | receiveBuff[2] << 8 | receiveBuff[3]);

        int namecount;
        string protoName = ProtoBuffTool.DecodeProtoName(receiveBuff, 4, out namecount);
        if (string.IsNullOrEmpty(protoName))
        {
            _udpClient.BeginReceive(ReceiveUdpCallBack, null);
            Console.WriteLine("ReceiveUdpCallBack 解析失败!");
            return;
        }

        int bodyCount = receiveBuff.Length - namecount - 4;
        IExtensible msgBase = ProtoBuffTool.Decode(protoName, receiveBuff, 4 + namecount, bodyCount);
        if (msgBase == null)
        {
            _udpClient.BeginReceive(ReceiveUdpCallBack, null);
            return;
        }

        MethodInfo mi = typeof(MsgHandler).GetMethod(protoName);
        if (mi != null)
        {
            object[] o = { guid, msgBase };
            mi.Invoke(null, o);
        }
        else
        {
            Console.WriteLine("ReceiveUdpCallBack 调用函数失败");
        }

        _udpClient.BeginReceive(ReceiveUdpCallBack, null);
    }

    /// <summary>
    /// udp 发送
    /// </summary>
    /// <param name="msg">消息</param>
    /// <param name="guid">客户端guid</param>
    public static void UdpSendTo(IExtensible msg,uint guid)
    {
        byte[] nameBytes = ProtoBuffTool.EncodeProtoName(msg);
        byte[] bodyBytes = ProtoBuffTool.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length+4;
        byte[] sendBytes = new byte[len];

        sendBytes[0] = (byte)(guid >> 24);
        sendBytes[1] = (byte)((guid >> 16) & 0xff);
        sendBytes[2] = (byte)((guid >> 8) & 0xff);
        sendBytes[3] = (byte)((guid) & 0xff);
        //拷贝到发送数组
        Array.Copy(nameBytes,0,sendBytes,4,nameBytes.Length);
        Array.Copy(bodyBytes,0,sendBytes,nameBytes.Length +4,bodyBytes.Length);

        _udpClient.Send(sendBytes,sendBytes.Length);
    }
    
    
    #endregion
}