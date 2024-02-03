using System;
/// <summary>
/// 用于客户端给服务端转发的协议
/// </summary>
public class MsgLockStep : MsgBase
{
    /// <summary>
    /// 当前客户端的帧数
    /// </summary>
    public int turn;
    /// <summary>
    /// 一帧的所有操作
    /// </summary>
    public Opts[] opts;
}
/// <summary>
/// 一帧的操作
/// </summary>
[Serializable]
public class Opts
{
    /// <summary>
    /// 发出操作的玩家
    /// </summary>
    public uint guid;
    /// <summary>
    /// 操作
    /// </summary>
    public Operation operation;
    /// <summary>
    /// 参数
    /// </summary>
    public Fixed64[] param;
}