using System;
/// <summary>
/// ���ڷ���˷��ظ��ͻ��˵�Э��
/// </summary>
public class MsgLockStepBack : MsgBase
{
    /// <summary>
    /// ��ǰ����˵�֡��
    /// </summary>
    public int turn;
    /// <summary>
    /// δͬ�������в���
    /// </summary>
    public UnsyncOpts[] unsyncOpts;
}
/// <summary>
/// δͬ���Ĳ���
/// </summary>
[Serializable]
public class UnsyncOpts
{
    /// <summary>
    /// ֡��
    /// </summary>
    public int turn;
    /// <summary>
    /// ����
    /// </summary>
    public Opts[] opts;
}