using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantLongInfoStruct : IConstantStruct
{
    /*
     * CONSTANT_Long_info {
           u1 tag;
           u4 high_bytes;
           u4 low_bytes;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Long;
    public uint HighBytes { get; set; } 
    public uint LowBytes { get; set; }
    
    public static ConstantLongInfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantLongInfoStruct();
        info.Tag = tag;
        info.HighBytes = ByteUtils.ReadUInt32(data, ref offset);
        info.LowBytes = ByteUtils.ReadUInt32(data, ref offset);
        return info;
    }
    
    public static ConstantLongInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantLongInfoStruct();
        info.Tag = data[offset++];
        info.HighBytes = ByteUtils.ReadUInt32(data, ref offset);
        info.LowBytes = ByteUtils.ReadUInt32(data, ref offset);
        return info;
    }
        
    public byte[] ToBytes()
    {
        using var stream = new MemoryStream();
        stream.WriteByte(Tag);
        ByteUtils.WriteUInt32(HighBytes, stream);
        ByteUtils.WriteUInt32(LowBytes, stream);
        return stream.ToArray();
    }

    public byte[] ToBytesWithoutTag()
    {
        using var stream = new MemoryStream();
        ByteUtils.WriteUInt32(HighBytes, stream);
        ByteUtils.WriteUInt32(LowBytes, stream);
        return stream.ToArray();
    }

    public long GetValue()
    {
        return ((long)HighBytes << 32) | LowBytes;
    }
        
    public void SetValue(long value)
    {
        HighBytes = (uint)(value >> 32);
        LowBytes = (uint)value;
    }
}