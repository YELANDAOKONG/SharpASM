using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantClassInfoStruct
{
    /*
     * CONSTANT_Class_info {
           u1 tag;
           u2 name_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Class;
    public ushort NameIndex { get; set; }
    
    public static ConstantClassInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantClassInfoStruct();
        info.Tag = data[offset++];
        info.NameIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
    
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            stream.WriteByte(Tag);
            ByteUtils.WriteUInt16(NameIndex, stream);
            return stream.ToArray();
        }
    }
}