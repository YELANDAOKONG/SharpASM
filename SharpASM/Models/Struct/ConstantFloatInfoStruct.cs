using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantFloatInfoStruct : IConstantStruct
{
    /*
     * CONSTANT_Float_info {
           u1 tag;
           u4 bytes;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Float;
    public uint Bytes { get; set; } 
    
    public static ConstantFloatInfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantFloatInfoStruct();
        info.Tag = tag;
        info.Bytes = ByteUtils.ReadUInt32(data, ref offset);
        return info;
    }

    public static ConstantFloatInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantFloatInfoStruct();
        info.Tag = data[offset++];
        info.Bytes = ByteUtils.ReadUInt32(data, ref offset);
        return info;
    }
    
    public byte GetTag() => Tag;
        
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

    
    public float GetValue()
    {
        byte[] byteArray = new byte[4];
        byteArray[0] = (byte)(Bytes >> 24);
        byteArray[1] = (byte)(Bytes >> 16);
        byteArray[2] = (byte)(Bytes >> 8);
        byteArray[3] = (byte)Bytes;
    
        // Convert from big-endian to system endianness
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(byteArray);
        }
        return BitConverter.ToSingle(byteArray, 0);
    }
        
    public void SetValue(float value)
    {
        byte[] byteArray = BitConverter.GetBytes(value);
    
        // Convert from system endianness to big-endian
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(byteArray);
        }
    
        Bytes = (uint)((byteArray[0] << 24) | (byteArray[1] << 16) | (byteArray[2] << 8) | byteArray[3]);
    }
    
    public ConstantPoolInfoStruct ToStructInfo()
    {
        ConstantPoolInfoStruct result = new ConstantPoolInfoStruct()
        {
            Tag = Tag,
            Info = ToBytesWithoutTag()
        };
        return result;
    }
}