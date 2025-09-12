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
        // Extract the big-endian bytes from the stored integer value
        byte[] byteArray = new byte[4];
        byteArray[0] = (byte)(Bytes >> 24);
        byteArray[1] = (byte)(Bytes >> 16);
        byteArray[2] = (byte)(Bytes >> 8);
        byteArray[3] = (byte)Bytes;
        // If the system is little-endian, reverse the bytes to get little-endian order for BitConverter
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(byteArray);
        }
        return BitConverter.ToSingle(byteArray, 0);
    }
        
    public void SetValue(float value)
    {
        byte[] byteArray = BitConverter.GetBytes(value);
        // If the system is little-endian, reverse the bytes to get big-endian order
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(byteArray);
        }
        // Convert the big-endian bytes to an integer
        Bytes = (uint)((byteArray[0] << 24) | (byteArray[1] << 16) | (byteArray[2] << 8) | byteArray[3]);
    }
}