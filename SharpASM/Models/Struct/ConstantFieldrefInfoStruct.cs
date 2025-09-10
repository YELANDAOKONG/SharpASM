using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantFieldrefInfoStruct
{
    /*
     * CONSTANT_Fieldref_info {
           u1 tag;
           u2 class_index;
           u2 name_and_type_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Fieldref;
    public ushort ClassIndex { get; set; }
    public ushort NameAndTypeIndex { get; set; }
    
    public static ConstantFieldrefInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantFieldrefInfoStruct();
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