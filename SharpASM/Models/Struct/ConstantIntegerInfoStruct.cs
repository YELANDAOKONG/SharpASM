using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantIntegerInfoStruct : IConstantStruct
{
    /*
     * CONSTANT_Integer_info {
           u1 tag;
           u4 bytes;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Integer;
    public uint Bytes { get; set; } 
    
    public static ConstantIntegerInfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantIntegerInfoStruct();
        info.Tag = tag;
        info.Bytes = ByteUtils.ReadUInt32(data, ref offset);
        return info;
    }

    public static ConstantIntegerInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantIntegerInfoStruct();
        info.Tag = data[offset++];
        info.Bytes = ByteUtils.ReadUInt32(data, ref offset);
        return info;
    }
        
    public byte[] ToBytes()
    {
        using var stream = new MemoryStream();
        stream.WriteByte(Tag);
        ByteUtils.WriteUInt32(Bytes, stream);
        return stream.ToArray();
    }

    public byte[] ToBytesWithoutTag()
    {
        using var stream = new MemoryStream();
        ByteUtils.WriteUInt32(Bytes, stream);
        return stream.ToArray();
    }

    public int GetValue()
    {
        return (int)Bytes;
    }
        
    public void SetValue(int value)
    {
        Bytes = (uint)value;
    }
}