using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantDoubleInfoStruct : IConstantStruct
{
    /*
     * CONSTANT_Double_info {
           u1 tag;
           u4 high_bytes;
           u4 low_bytes;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Double;
    public uint HighBytes { get; set; } 
    public uint LowBytes { get; set; }
    
    public static ConstantDoubleInfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantDoubleInfoStruct();
        info.Tag = tag;
        info.HighBytes = ByteUtils.ReadUInt32(data, ref offset);
        info.LowBytes = ByteUtils.ReadUInt32(data, ref offset);
        return info;
    }

    public static ConstantDoubleInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantDoubleInfoStruct();
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

    public double GetValue()
    {
        long longValue = ((long)HighBytes << 32) | LowBytes;
        return BitConverter.Int64BitsToDouble(longValue);
    }
        
    public void SetValue(double value)
    {
        long longValue = BitConverter.DoubleToInt64Bits(value);
        HighBytes = (uint)(longValue >> 32);
        LowBytes = (uint)longValue;
    }
}