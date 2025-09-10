using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Structs;

public class ConstantFloatInfoStruct
{
    /*
     * CONSTANT_Float_info {
           u1 tag;
           u4 bytes;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Float;
    public uint Bytes { get; set; } 
    
    public static ConstantFloatInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantFloatInfoStruct();
        info.Tag = data[offset++];
        info.Bytes = ByteUtils.ReadUInt32(data, ref offset);
        return info;
    }
        
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            stream.WriteByte(Tag);
            ByteUtils.WriteUInt32(Bytes, stream);
            return stream.ToArray();
        }
    }
        
    public float GetValue()
    {
        byte[] byteArray = BitConverter.GetBytes(Bytes);
        return BitConverter.ToSingle(byteArray, 0);
    }
        
    public void SetValue(float value)
    {
        byte[] byteArray = BitConverter.GetBytes(value);
        Bytes = BitConverter.ToUInt32(byteArray, 0);
    }
}