using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Structs;

public class ConstantMethodTypeInfoStruct
{
    /*
     * CONSTANT_MethodType_info {
           u1 tag;
           u2 descriptor_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.MethodType;
    public ushort DescriptorIndex { get; set; } 
    
    public static ConstantMethodTypeInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantMethodTypeInfoStruct();
        info.Tag = data[offset++];
        info.DescriptorIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
        
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            stream.WriteByte(Tag);
            ByteUtils.WriteUInt16(DescriptorIndex, stream);
            return stream.ToArray();
        }
    }
}