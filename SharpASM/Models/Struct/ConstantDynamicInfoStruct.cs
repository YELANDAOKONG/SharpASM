using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantDynamicInfoStruct : IConstantStruct
{
    /*
     * CONSTANT_Dynamic_info {
           u1 tag;
           u2 bootstrap_method_attr_index;
           u2 name_and_type_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Dynamic;
    public ushort BootstrapMethodAttrIndex { get; set; } 
    public ushort NameAndTypeIndex { get; set; } 
    
    public static ConstantDynamicInfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantDynamicInfoStruct();
        info.Tag = tag;
        info.BootstrapMethodAttrIndex = ByteUtils.ReadUInt16(data, ref offset);
        info.NameAndTypeIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }

    public static ConstantDynamicInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantDynamicInfoStruct();
        info.Tag = data[offset++];
        info.BootstrapMethodAttrIndex = ByteUtils.ReadUInt16(data, ref offset);
        info.NameAndTypeIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
    
    public byte GetTag() => Tag;
        
    public byte[] ToBytes()
    {
        using var stream = new MemoryStream();
        stream.WriteByte(Tag);
        ByteUtils.WriteUInt16(BootstrapMethodAttrIndex, stream);
        ByteUtils.WriteUInt16(NameAndTypeIndex, stream);
        return stream.ToArray();
    }

    public byte[] ToBytesWithoutTag()
    {
        using var stream = new MemoryStream();
        ByteUtils.WriteUInt16(BootstrapMethodAttrIndex, stream);
        ByteUtils.WriteUInt16(NameAndTypeIndex, stream);
        return stream.ToArray();
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