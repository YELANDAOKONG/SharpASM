using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantInterfaceMethodrefInfoStruct : IConstantStruct
{
    /*
     * CONSTANT_InterfaceMethodref_info {
           u1 tag;
           u2 class_index;
           u2 name_and_type_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.InterfaceMethodref;
    public ushort ClassIndex { get; set; }
    public ushort NameAndTypeIndex { get; set; }
    
    public static ConstantInterfaceMethodrefInfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantInterfaceMethodrefInfoStruct();
        info.Tag = tag;
        info.ClassIndex = ByteUtils.ReadUInt16(data, ref offset);
        info.NameAndTypeIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }

    public static ConstantInterfaceMethodrefInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantInterfaceMethodrefInfoStruct();
        info.Tag = data[offset++];
        info.ClassIndex = ByteUtils.ReadUInt16(data, ref offset);
        info.NameAndTypeIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
    
    public byte GetTag() => Tag;
        
    public byte[] ToBytes()
    {
        using var stream = new MemoryStream();
        stream.WriteByte(Tag);
        ByteUtils.WriteUInt16(ClassIndex, stream);
        ByteUtils.WriteUInt16(NameAndTypeIndex, stream);
        return stream.ToArray();
    }

    public byte[] ToBytesWithoutTag()
    {
        using var stream = new MemoryStream();
        ByteUtils.WriteUInt16(ClassIndex, stream);
        ByteUtils.WriteUInt16(NameAndTypeIndex, stream);
        return stream.ToArray();
    }
}