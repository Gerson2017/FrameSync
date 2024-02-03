using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 协议基类 一条消息包含的信息有以下这些 消息长度 协议长度 协议名(消息名 用作解析使用) 协议体（消息体）
/// </summary>
public class MsgBase
{
    /// <summary>
    /// 协议名字
    /// </summary>
    public string MProtoName;


    /// <summary>
    /// 将数据转成 字节数组 编码
    /// </summary>
    /// <returns></returns>
    public static byte[] Encode(MsgBase msg)
    {
        string s = JsonUtility.ToJson(msg);
        return Encoding.UTF8.GetBytes(s);
    }

    /// <summary>
    /// 解码 从数组中的什么位置开始解码
    /// </summary>
    /// <param name="protoName">协议名</param>
    /// <param name="bytes"></param>
    /// <param name="offset">解码起始位置</param>
    /// <param name="length">解码长度</param>
    /// <returns></returns>
    public static MsgBase Decode(string protoName, byte[] bytes, int offset, int length)
    {
        string s = Encoding.UTF8.GetString(bytes,offset,length);
        //使用反射获取具体的数据类型
        object msg = JsonUtility.FromJson(s, Type.GetType(protoName));
        return (MsgBase)msg;
    }

    /// <summary>
    /// 协议名编码
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] EncodeProtoName(MsgBase msgBase)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(msgBase.MProtoName);
        short len = (short)nameBytes.Length;
        //新的字节数组的长度len + 2 因为需要两个字节存储长度大小
        byte[] bytes = new byte[len + 2];
        bytes[0] = (byte)(len % 256); //0000 0001 0000 0001 (257 在二进制中的表示)
        bytes[1] = (byte)(len / 256);
        
        //前面两个字节是协议名长度 根据长度获取协议名
        Array.Copy(nameBytes, 0, bytes, 2, len);
        return bytes;
    }
    
    
    /// <summary>
    /// 协议名解码
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset">起始位置</param>
    /// <param name="count">需要返回的解析长度</param>
    /// <returns></returns>
    public static string DecodeProtoName(byte[] bytes,int offset,out  int count)
    {
        count = 0;
        //没有消息名可以解析的
        if (offset+2>bytes.Length)
            return "";
        //对消息长度进行解码  从offset 开始取两个字节来计算协议名的字节位数
        short len = (short)(bytes[offset + 1] * 256 + bytes[offset]);
        if (len<=0)
            return "";
        // short len=(short) (bytes[offset+1]<<8|bytes[offset]); 位运算效率更高
       count = len + 2;
       return Encoding.UTF8.GetString(bytes,offset+2,len);
    }

}