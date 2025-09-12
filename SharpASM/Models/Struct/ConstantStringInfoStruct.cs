using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantStringInfoStruct : IConstantStruct
{
    /*
     * CONSTANT_String_info {
           u1 tag;
           u2 string_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.String;
    public ushort NameIndex { get; set; }
    
    public static ConstantStringInfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantStringInfoStruct();
        info.Tag = tag;
        info.NameIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }

    public static ConstantStringInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantStringInfoStruct();
        info.Tag = data[offset++];
        info.NameIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
    
    public byte GetTag() => Tag;
        
    public byte[] ToBytes()
    {
        using var stream = new MemoryStream();
        stream.WriteByte(Tag);
        ByteUtils.WriteUInt16(NameIndex, stream);
        return stream.ToArray();
    }

    public byte[] ToBytesWithoutTag()
    {
        using var stream = new MemoryStream();
        ByteUtils.WriteUInt16(NameIndex, stream);
        return stream.ToArray();
    }
}