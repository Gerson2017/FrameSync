using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using ProtoBuf;

#nullable disable

public static class GateWay
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
    /// 服务端的Socket
    /// </summary>
    private static Socket _lisenFd;

    /// <summary>
    /// 网关Socket 用于链接其它服务端
    /// </summary>
    private static Socket _gateWayFd;

    /// <summary>
    /// 客户端Socket字典
    /// </summary>
    public static Dictionary<Socket, ClientState> MClentStatesDic;

    /// <summary>
    /// 其它服务端字典
    /// </summary>
    public static Dictionary<Socket, ServerState> MServerStatesDic;

    /// <summary>
    /// 服务类型和服务器的印射关系
    /// </summary>
    public static Dictionary<ServerType, ServerState> MType2ServerDic;

    /// <summary>
    /// 通过guid找到相应的客户端
    /// </summary>
    public static Dictionary<uint, ClientState> MId2CsDic;

    /// <summary>
    /// 用于检测的列表 存放所有的Socket 包括服务端的
    /// </summary>
    public static List<Socket> MSockets = new List<Socket>();

    /// <summary>
    /// 心跳检测间隔
    /// </summary>
    private static float _pingInterval = 2; // 30;

    /// <summary>
    /// 接受客户端udp
    /// </summary>
    private static UdpClient _receiveClientUdp;

    /// <summary>
    /// 接收服务端udp
    /// </summary>
    private static UdpClient _receiveServerClientUdp;


    private static bool _hasInit;

    /// <summary>
    ///  连接服务器
    /// </summary>
    /// <param name="ip">ip地址</param>
    /// <param name="port">端口号</param>
    public static void Connect(string ip, int port)
    {
        Init();
        _lisenFd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = IPAddress.Parse(ip);
        //创建IP地址和端口号
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
        //服务端绑定ip和端口号
        _lisenFd.Bind(ipEndPoint);
        _lisenFd.Listen(0); //0表示不限制客户端的数量

        //获取到和客户端连接的本地的endPoint
        _receiveClientUdp = new UdpClient((IPEndPoint)_lisenFd.LocalEndPoint);


        Console.WriteLine("服务器启动成功！");
        while (true) //服务端使用select
        {
            MSockets.Clear();
            //放服务端的Socket
            MSockets.Add(_lisenFd);
            //放入客户端的Socket
            foreach (Socket cleintSocket in MClentStatesDic.Keys)
            {
                MSockets.Add(cleintSocket);
            }

            Socket.Select(MSockets, null, null, 1000);
            for (int i = 0; i < MSockets.Count; i++)
            {
                Socket s = MSockets[i];
                if (s == _lisenFd) //如果是服务端 有客户端要连接
                    Accept(s);
                else //客户端发消息过来
                    Receive(s);
            }

            //循环监听心跳机制
            //   CheckPing();
        }
    }

    /// <summary>
    /// 链接其它服务端
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public static ServerState ConnectServer(string ip, int port)
    {
        Init();
        ServerState server = new ServerState();
        _gateWayFd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = IPAddress.Parse(ip);
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
        _gateWayFd.Bind(ipEndPoint);
        _gateWayFd.Listen(0);
        Console.WriteLine("网关服务器等待其他服务器链接");
        _gateWayFd.BeginAccept(AccpetServerCallback, server);
        return server;
    }

    static void Init()
    {
        if (!_hasInit)
        {
            MClentStatesDic = new Dictionary<Socket, ClientState>();
            MServerStatesDic = new Dictionary<Socket, ServerState>();
            MId2CsDic = new Dictionary<uint, ClientState>();
            MType2ServerDic = new Dictionary<ServerType, ServerState>();
            _hasInit = true;
        }
    }

    private static void AccpetServerCallback(IAsyncResult ar)
    {
        //封装链接过来的服务端对象
        ServerState serverState = (ServerState)ar.AsyncState;
        Socket socket = _gateWayFd.EndAccept(ar);

        Console.WriteLine("链接其它服务器成功 ");
        serverState.MSocket = socket;
        MServerStatesDic.Add(socket, serverState);
        //udp
        _receiveServerClientUdp = new UdpClient((IPEndPoint)_gateWayFd.LocalEndPoint);
        _receiveServerClientUdp.BeginReceive(RececiveUdpServerallback, serverState);

        ByteArray byteArray = serverState.MReadBuffer;
        //接收消息 往字节数组写入数据
        socket.BeginReceive(byteArray.MBytes, byteArray.MWriteIndex, byteArray.MRemain, 0, ReceiveServerCallBack,
            serverState);
    }


    private static void ReceiveServerCallBack(IAsyncResult ar)
    {
        ServerState serverState = (ServerState)ar.AsyncState;
        int count = 0;
        Socket server = serverState.MSocket;

        ByteArray byteArray = serverState.MReadBuffer;

        if (byteArray.MRemain <= 0)
            byteArray.MoveBytes();
        //移动完依然小于0
        if (byteArray.MRemain <= 0)
        {
            Console.WriteLine("Receive fail 数组长度不足");
            //关闭服务端
//            Close();
            return;
        }

        try
        {
            count = server.EndReceive(ar);
        }
        catch (Exception e)
        {
            Console.WriteLine("Receive fail " + e.Message);
            //关闭服务端
//            Close();
            return;
        }

        if (count <= 0)
        {
            Console.WriteLine("Socket Close: " + serverState.MSocket.RemoteEndPoint.ToString());
            //关闭服务端
//            Close();
            return;
        }

        //处理接收过来的消息
        byteArray.MWriteIndex += count;
        OnReceiveServerData(serverState);
        byteArray.MoveBytes();
        server.BeginReceive(byteArray.MBytes, byteArray.MWriteIndex, byteArray.MRemain, 0, ReceiveServerCallBack,
            serverState);
    }

    /// <summary>
    /// 接收其它服务端发来的消息
    /// </summary>
    /// <param name="state"></param>
    private static void OnReceiveServerData(ServerState state)
    {
        ByteArray byteArray = state.MReadBuffer;
        byte[] bytes = byteArray.MBytes;
        if (bytes.Length <= 2)
            return;

        short length = (short)(bytes[byteArray.MReadIndex + 1] * 256 + bytes[byteArray.MReadIndex]);
        if (byteArray.Length < length + 2)
            return;
        //guid 2-5号位置 4个字节存储guid
        uint guid = (uint)(bytes[byteArray.MReadIndex + 2] << 24 |
                           bytes[byteArray.MReadIndex + 3] << 16 |
                           bytes[byteArray.MReadIndex + 4] << 8 |
                           bytes[byteArray.MReadIndex + 5]);

        byteArray.MReadIndex += 6;

        try
        {
            //消息协议的长度
            int msgLength = length - 4;
            //发送给客户端的消息
            byte[] sendBytes = new byte[msgLength + 2];
            //打包长度
            sendBytes[0] = (byte)(msgLength % 256);
            sendBytes[1] = (byte)(msgLength / 256);

            Array.Copy(bytes, byteArray.MReadIndex, sendBytes, 2, msgLength);
            //通过guid找到客户端进行发送
            MId2CsDic[guid].MSocket.Send(sendBytes, 0);
        }
        catch (Exception e)
        {
            Console.WriteLine("OnReceiveServerData " + e.Message);
        }

        byteArray.MReadIndex += length - 4;

        //继续处理 还有消息要处理
        if (byteArray.Length > 2)
        {
            OnReceiveServerData(state);
        }
    }


    /// <summary>
    /// 接收客户端的连接
    /// </summary>
    /// <param name="listenfd">服务端的Socket</param>
    private static void Accept(Socket listenfd)
    {
        try
        {
            Socket socket = listenfd.Accept();
            Console.WriteLine("有客户端连接 " + socket.RemoteEndPoint.ToString());
            //创建描述客户端的对象 
            ClientState state = new ClientState();
            state.MSocket = socket;

            _receiveClientUdp.BeginReceive(RececiveUdpClientCallback, state);


            //给客户端分配guid
            uint guid = MyGuid.GetGuid();
            state.MGuid = guid;
            MId2CsDic.Add(guid, state);


            state.LastPingTime = GetTimeStamp();
            MClentStatesDic.Add(socket, state);
        }
        catch (Exception e)
        {
            Console.WriteLine("Accept 失败" + e.Message);
            throw;
        }
    }


    /// <summary>
    /// 接收客户端发送过来的消息
    /// </summary>
    /// <param name="socket">客户端</param>
    private static void Receive(Socket socket)
    {
        MClentStatesDic.TryGetValue(socket, out ClientState state);
        if (state != null)
        {
            ByteArray readbuff = state.MReadBuffer;

            if (readbuff.MRemain <= 0)
                readbuff.MoveBytes();
            if (readbuff.MRemain <= 0)
            {
                Console.WriteLine("Receive 失败 数组不够大");
                Close(state);
                return;
            }

            int count = 0;
            try
            {
                //同步接收
                count = socket.Receive(readbuff.MBytes, readbuff.MWriteIndex, readbuff.MRemain, 0);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Receive 失败 " + e.Message);
                Close(state);
                return;
            }

            //客户端主动关闭
            if (count <= 0)
            {
                Console.WriteLine("Socket Close " + socket.RemoteEndPoint.ToString());
                Close(state);
                return;
            }

            readbuff.MWriteIndex += count;
            OnReceiveData(state);
            readbuff.MoveBytes();
        }
    }

    /// <summary>
    /// 处理接收的消息
    /// </summary>
    /// <param name="state">客户端对象</param>
    private static void OnReceiveData(ClientState state)
    {
        ByteArray readBuffer = state.MReadBuffer;
        byte[] bytes = readBuffer.MBytes;
        int readIndex = readBuffer.MReadIndex;
        ///消息长度都没有解析出来
        if (readBuffer.Length <= 2)
            return;

        //解析总长度
        short length = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);
        //收到的消息没有解析出来需要的数据多 消息不全
        if (readBuffer.Length < length + 2)
            return;

        //解析服务器编号 一个字节
        ServerType serverType = (ServerType)bytes[readIndex + 2];
        readBuffer.MReadIndex += 3; //两个字节长度 1个字节服务器编号
        // string protoName= ProtoBuffTool.DecodeProtoName(bytes, readBuffer.MReadIndex, out int treadIndex);
        // Console.WriteLine("ProtoName "+protoName+" readIndex "+treadIndex);
        try
        {
            //减去一个字节的服务器编号 加上4个字节的guid长度
            int sendLength = length - 1 + 4;
            //加上消息的长度字节数组
            byte[] sendBytes = new byte[sendLength + 2];
            sendBytes[0] = (byte)(sendLength % 256);
            sendBytes[1] = (byte)(sendLength / 256);
            //网关给其他服务器发送的数据 消息长度2字节 guid 4字节 打包的协议
            byte[] guidBytes = state.GeneralGuidBytes();
            sendBytes[2] = guidBytes[0]; //guid转成字节 
            sendBytes[3] = guidBytes[1]; //前面的位数全部归0
            sendBytes[4] = guidBytes[2];
            sendBytes[5] = guidBytes[3];

            //拷贝数据 前6个字节已经占用了 2个长度 4个guid
            Array.Copy(bytes, readBuffer.MReadIndex, sendBytes, 6, sendLength - 4);
            if (MType2ServerDic.ContainsKey(serverType))
            {
                if (MType2ServerDic[serverType].MSocket != null)
                    MType2ServerDic[serverType].MSocket.Send(sendBytes, 0, sendBytes.Length, 0);
                else
                    Console.WriteLine("战斗服务器未连接 MSocket is null");
            }
            else
            {
                Console.WriteLine("战斗服务器未连接");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("OnReceiveData " + e.Message);
        }

        readBuffer.MWriteIndex += length - 1;

        readBuffer.MoveBytes();
        //还有消息没有处理完成
        if (readBuffer.Length > 2)
            OnReceiveData(state);

        //网关服务器不需要解析协议名
        //
        // int namecount = 0;
        // //json方式
        // //  string protoName = MsgBase.DecodeProtoName(readBuffer.MBytes, readBuffer.MReadIndex, out namecount);
        // //protobuff方式
        // string protoName = ProtoBuffTool.DecodeProtoName(readBuffer.MBytes, readBuffer.MReadIndex, out namecount);
        // if (string.IsNullOrEmpty(protoName))
        // {
        //     Console.WriteLine("OnReceiveData 失败,协议名为空");
        //     Close(state);
        //     return;
        // }
        //
        // readBuffer.MReadIndex += namecount;
        // int bodyLength = length - namecount;
        // //json 方式
        // //   MsgBase msgBase = MsgBase.Decode(protoName, readBuffer.MBytes, readBuffer.MReadIndex, bodyLength);
        // //protobuff 方式
        // IExtensible msgBase = ProtoBuffTool.Decode(protoName, readBuffer.MBytes, readBuffer.MReadIndex, bodyLength);
        // readBuffer.MReadIndex += bodyLength;
        // readBuffer.MoveBytes();
        //
        // //MsgHandler 中的 MsgPing方法和 protoName 一样 使用反射获取方法
        // MethodInfo methodInfo = typeof(MsgHandler).GetMethod(protoName);
        // Console.WriteLine("Receive Data " + protoName);
        // if (methodInfo != null)
        // {
        //     //构造反射方法的参数
        //     object[] o = { state, msgBase };
        //     //静态方法不需要传递调用对象
        //     methodInfo.Invoke(null, o);
        // }
        // else
        // {
        //     Console.WriteLine("Receive Data 获取反射 方法失败 ");
        // }
        //
        // //是否还有消息没有处理完
        // if (readBuffer.Length > 2)
        // {
        //     OnReceiveData(state);
        // }
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="state">指定发送的客户端</param>
    /// <param name="msgBase">消息</param>
    public static void Send(ClientState state, IExtensible msg)
    {
        if (state == null || !state.MSocket.Connected)
            return;
        //编码 json
        // byte[] nameBytes = MsgBase.EncodeProtoName(msg);
        // byte[] bodyBytes = MsgBase.Encode(msg);
        //编码 protobuff
        byte[] nameBytes = ProtoBuffTool.EncodeProtoName(msg);
        byte[] bodyBytes = ProtoBuffTool.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length; //消息体长度
        byte[] sendBytes = new byte[len + 2]; //使用两个字节存储 len
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        //nameBytes 拷贝进发送字节数组
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        //bodyBytes 拷贝进发送字节数组
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        try
        {
            state.MSocket.Send(sendBytes, 0, sendBytes.Length, 0);
            Console.WriteLine("Send Msg ");
        }
        catch (Exception e)
        {
            Console.WriteLine("Send 失败 " + e.Message);
        }
    }


    /// <summary>
    /// 关闭对应的客户端
    /// </summary>
    /// <param name="client"></param>
    private static void Close(ClientState client)
    {
        client.MSocket.Close();
        //从字典中移除
        MClentStatesDic.Remove(client.MSocket);
    }


    /// <summary>
    /// 获取时间戳
    /// </summary>
    /// <returns></returns>
    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        //将时间戳转成long 类型
        return Convert.ToInt64(ts.TotalSeconds);
    }


    private static void CheckPing()
    {
        foreach (var client in MClentStatesDic.Values)
        {
            if (GetTimeStamp() - client.LastPingTime > _pingInterval * 4)
            {
                Console.WriteLine("心跳机制断开链接 :" + client.MSocket.RemoteEndPoint);
                Close(client);
                return;
            }
        }
    }


    #region UDP

    private static void RececiveUdpClientCallback(IAsyncResult ar)
    {
        ClientState state = (ClientState)ar.AsyncState;
        IPEndPoint receiveFromIp = new IPEndPoint(IPAddress.Any, 0);

        byte[] receiveBuffer = _receiveClientUdp.EndReceive(ar, ref receiveFromIp);
        //udp采用数据报的形式 直接使用0号位
        ServerType serverType = (ServerType)receiveBuffer[0];
        if (MType2ServerDic.ContainsKey(serverType))
        {
            //要给服务器发送的ip地址
            IPEndPoint serIpEndPoint = (IPEndPoint)MType2ServerDic[serverType].MSocket.RemoteEndPoint;
            //减去一个serverType 加上4个guid
            byte[] sendBytes = new byte[receiveBuffer.Length + 3];
            byte[] guidBytes = state.GeneralGuidBytes();
            sendBytes[0] = guidBytes[0];
            sendBytes[1] = guidBytes[1];
            sendBytes[2] = guidBytes[2];
            sendBytes[3] = guidBytes[3];

            Array.Copy(receiveBuffer, 1, sendBytes, 4, receiveBuffer.Length - 1);

            _receiveServerClientUdp.Send(sendBytes, sendBytes.Length, serIpEndPoint);
            _receiveClientUdp.BeginReceive(RececiveUdpClientCallback, state);
        }
        else
        {
            Console.WriteLine("RececiveUdpClientCallback " + serverType + " 未链接");
        }
    }


    private static void RececiveUdpServerallback(IAsyncResult ar)
    {
        ServerState state = (ServerState)ar.AsyncState;
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receiveBuff = _receiveServerClientUdp.EndReceive(ar, ref ipEndPoint);
        //解析guid
        uint guid = (uint)(receiveBuff[0] << 24 | receiveBuff[1] << 16 | receiveBuff[2] << 8 | receiveBuff[3]);

        if (MId2CsDic.ContainsKey(guid))
        {
            IPEndPoint clientIpEndPoint = (IPEndPoint)MId2CsDic[guid].MSocket.RemoteEndPoint;
            //通过数据前移移除掉guid
            Array.Copy(receiveBuff,4,receiveBuff,0,receiveBuff.Length-4);
            //向服务器发送数据  receiveBuff.Length-4 消息前移后防止多处理了后面的消息
            _receiveClientUdp.Send(receiveBuff, receiveBuff.Length-4, clientIpEndPoint);
        }
        else
        {
            Console.WriteLine("RececiveUdpServerallback not found Guid "+guid);
        }
    }

    #endregion
}