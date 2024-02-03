#nullable disable
using System.Reflection;
using System.Text;
using ProtoBuf;

public class ProtoBuffTool
{
    /// <summary>
    /// 将数据转成 字节数组 编码
    /// </summary>
    /// <returns></returns>
    public static byte[] Encode(IExtensible msg)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            Serializer.Serialize(ms, msg);
            return ms.ToArray();
        }
    }

    /// <summary>
    /// 解码 从数组中的什么位置开始解码
    /// </summary>
    /// <param name="protoName">协议名</param>
    /// <param name="bytes"></param>
    /// <param name="offset">解码起始位置</param>
    /// <param name="length">解码长度</param>
    /// <returns></returns>
    public static IExtensible Decode(string protoName, byte[] bytes, int offset, int length)
    {
        using (var ms = new MemoryStream(bytes, offset, length))
        {
            Type t = Type.GetType(protoName);
            return (IExtensible)Serializer.NonGeneric.Deserialize(t, ms);
        }
    }

    /// <summary>
    /// 协议名编码
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] EncodeProtoName(IExtensible msgBase)
    {
        //通过反射获取协议名属性
        PropertyInfo propertyInfo = msgBase.GetType().GetProperty("protoName");
        if (propertyInfo != null)
        {
            string protoNameStr = propertyInfo.GetValue(msgBase).ToString();
            byte[] nameBytes = Encoding.UTF8.GetBytes(protoNameStr);
            short len = (short)nameBytes.Length;
            //新的字节数组的长度len + 2 因为需要两个字节存储长度大小
            byte[] bytes = new byte[len + 2];
            bytes[0] = (byte)(len % 256); //0000 0001 0000 0001 (257 在二进制中的表示)
            bytes[1] = (byte)(len / 256);

            //前面两个字节是协议名长度 根据长度获取协议名
            Array.Copy(nameBytes, 0, bytes, 2, len);
            return bytes;
        }
        Console.WriteLine("Error protoName 属性值为空 请检查协议 ");
        return null;
    }


    /// <summary>
    /// 协议名解码
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset">起始位置</param>
    /// <param name="count">需要返回的解析长度</param>
    /// <returns></returns>
    public static string DecodeProtoName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        //没有消息名可以解析的
        if (offset + 2 > bytes.Length)
            return "";
        //对消息长度进行解码  从offset 开始取两个字节来计算协议名的字节位数
        short len = (short)(bytes[offset + 1] * 256 + bytes[offset]);
        if (len <= 0)
            return "";
        // short len=(short) (bytes[offset+1]<<8|bytes[offset]); 位运算效率更高
        count = len + 2;
        return Encoding.UTF8.GetString(bytes, offset + 2, len);
    }
}