using System;
/// <summary>
/// ���ڿͻ��˸������ת����Э��
/// </summary>
public class MsgLockStep : MsgBase
{
    /// <summary>
    /// ��ǰ�ͻ��˵�֡��
    /// </summary>
    public int turn;
    /// <summary>
    /// һ֡�����в���
    /// </summary>
    public Opts[] opts;
}
/// <summary>
/// һ֡�Ĳ���
/// </summary>
[Serializable]
public class Opts
{
    /// <summary>
    /// �������������
    /// </summary>
    public uint guid;
    /// <summary>
    /// ����
    /// </summary>
    public Operation operation;
    /// <summary>
    /// ����
    /// </summary>
    public Fixed64[] param;
}