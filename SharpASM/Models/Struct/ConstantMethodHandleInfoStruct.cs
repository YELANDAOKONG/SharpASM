using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantMethodHandleInfoStruct : IConstantStruct
{
    /*
     * CONSTANT_MethodHandle_info {
           u1 tag;
           u1 reference_kind;
           u2 reference_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.MethodHandle;
    public byte ReferenceKind { get; set; } 
    public ushort ReferenceIndex { get; set; } 
    
    public static ConstantMethodHandleInfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantMethodHandleInfoStruct();
        info.Tag = tag;
        info.ReferenceKind = data[offset++];
        info.ReferenceIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
    
    public static ConstantMethodHandleInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantMethodHandleInfoStruct();
        info.Tag = data[offset++];
        info.ReferenceKind = data[offset++];
        info.ReferenceIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
        
    public byte[] ToBytes()
    {
        using var stream = new MemoryStream();
        stream.WriteByte(Tag);
        stream.WriteByte(ReferenceKind);
        ByteUtils.WriteUInt16(ReferenceIndex, stream);
        return stream.ToArray();
    }

    public byte[] ToBytesWithoutTag()
    {
        using var stream = new MemoryStream();
        stream.WriteByte(ReferenceKind);
        ByteUtils.WriteUInt16(ReferenceIndex, stream);
        return stream.ToArray();
    }
}