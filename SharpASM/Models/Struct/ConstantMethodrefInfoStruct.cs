using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantMethodrefInfoStruct
{
    /*
     * CONSTANT_Methodref_info {
           u1 tag;
           u2 class_index;
           u2 name_and_type_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Methodref;
    public ushort ClassIndex { get; set; }
    public ushort NameAndTypeIndex { get; set; }
    
    public static ConstantMethodrefInfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantMethodrefInfoStruct();
        info.Tag = tag;
        info.ClassIndex = ByteUtils.ReadUInt16(data, ref offset);
        info.NameAndTypeIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
    
    public static ConstantMethodrefInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantMethodrefInfoStruct();
        info.Tag = data[offset++];
        info.ClassIndex = ByteUtils.ReadUInt16(data, ref offset);
        info.NameAndTypeIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
    
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            stream.WriteByte(Tag);
            ByteUtils.WriteUInt16(ClassIndex, stream);
            ByteUtils.WriteUInt16(NameAndTypeIndex, stream);
            return stream.ToArray();
        }
    }
}