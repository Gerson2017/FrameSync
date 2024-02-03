using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ByteArray
{
    /// <summary>
    /// 字节缓存默认大小
    /// </summary>
    private const int DEFAULT_SIZE = 1024;

    /// <summary>
    /// 字节数组
    /// </summary>
    public byte[] MBytes;

    /// <summary>
    /// 读的位置
    /// </summary>
    public int MReadIndex;

    /// <summary>
    /// 写的位置
    /// </summary>
    public int MWriteIndex;

    /// <summary>
    /// 读写之间的长度 发送数据时使用
    /// </summary>
    public int Length => MWriteIndex - MReadIndex;

    /// <summary>
    /// 初始大小
    /// </summary>
    private int _initSize;

    /// <summary>
    /// 容量
    /// </summary>
    public int Capacity;

    /// <summary>
    /// 剩余的长度
    /// </summary>
    public int MRemain => Capacity - MWriteIndex;

    /// <summary>
    /// 构造一个带有初始容量的字节数组
    /// </summary>
    /// <param name="size"></param>
    public ByteArray(int size = DEFAULT_SIZE)
    {
        MBytes = new byte[size];
        this.Capacity = size;
        _initSize = size;
        this.MReadIndex = 0;
        this.MWriteIndex = 0;
    }

    /// <summary>
    /// 创建字节数组 
    /// </summary>
    /// <param name="默认字节数组"></param>
    public ByteArray(byte[] defaltBytes)
    {
        MBytes = defaltBytes;
        this.Capacity = MBytes.Length;
        _initSize = MBytes.Length;
        this.MReadIndex = 0;
        //从数组后面开始写
        this.MWriteIndex = defaltBytes.Length;
    }

    /// <summary>
    /// 发送完或者处理完数据后 将字节中存在的数据前移
    /// </summary>
    public void MoveBytes()
    {
        if (Length > 0)
            Array.Copy(MBytes, MReadIndex, MBytes, 0, Length);

        MWriteIndex = Length;
        MReadIndex = 0;
    }

    /// <summary>
    /// 对字节数组大小进行扩容
    /// </summary>
    /// <param name="size"></param>
    public void ReSize(int size)
    {
        if (size < Length)
            return;
        //扩容后的大小小于当前大小 则不处理
        if (size < _initSize)
            return;

        Capacity = size;
        byte[] newBytes = new byte[Capacity];
        //将现有数据拷贝到扩容后的数组中
        Array.Copy(MBytes,MReadIndex,newBytes,0,Length);
        MBytes = newBytes;
        MReadIndex = 0;
        MWriteIndex = Length;
    
    }
}