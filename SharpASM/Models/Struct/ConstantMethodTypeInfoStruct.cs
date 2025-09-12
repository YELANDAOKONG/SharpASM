using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantMethodTypeInfoStruct : IConstantStruct
{
    /*
     * CONSTANT_MethodType_info {
           u1 tag;
           u2 descriptor_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.MethodType;
    public ushort DescriptorIndex { get; set; } 
    
    public static ConstantMethodTypeInfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantMethodTypeInfoStruct();
        info.Tag = tag;
        info.DescriptorIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }

    public static ConstantMethodTypeInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantMethodTypeInfoStruct();
        info.Tag = data[offset++];
        info.DescriptorIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
        
    public byte[] ToBytes()
    {
        using var stream = new MemoryStream();
        stream.WriteByte(Tag);
        ByteUtils.WriteUInt16(DescriptorIndex, stream);
        return stream.ToArray();
    }

    public byte[] ToBytesWithoutTag()
    {
        using var stream = new MemoryStream();
        ByteUtils.WriteUInt16(DescriptorIndex, stream);
        return stream.ToArray();
    }
}