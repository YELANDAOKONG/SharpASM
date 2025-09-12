using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantNameAndTypeInfoStruct : IConstantStruct
{
    /*
     * CONSTANT_NameAndType_info {
           u1 tag;
           u2 name_index;
           u2 descriptor_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.NameAndType;
    public ushort NameIndex { get; set; } 
    public ushort DescriptorIndex { get; set; }
    
    public static ConstantNameAndTypeInfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantNameAndTypeInfoStruct();
        info.Tag = tag;
        info.NameIndex = ByteUtils.ReadUInt16(data, ref offset);
        info.DescriptorIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
    
    public static ConstantNameAndTypeInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantNameAndTypeInfoStruct();
        info.Tag = data[offset++];
        info.NameIndex = ByteUtils.ReadUInt16(data, ref offset);
        info.DescriptorIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
    
    public byte[] ToBytes()
    {
        using var stream = new MemoryStream();
        stream.WriteByte(Tag);
        ByteUtils.WriteUInt16(NameIndex, stream);
        ByteUtils.WriteUInt16(DescriptorIndex, stream);
        return stream.ToArray();
    }

    public byte[] ToBytesWithoutTag()
    {
        using var stream = new MemoryStream();
        ByteUtils.WriteUInt16(NameIndex, stream);
        ByteUtils.WriteUInt16(DescriptorIndex, stream);
        return stream.ToArray();
    }
}