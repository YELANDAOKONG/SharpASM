using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Structs;

public class ConstantMethodHandleInfoStruct
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
        using (var stream = new MemoryStream())
        {
            stream.WriteByte(Tag);
            stream.WriteByte(ReferenceKind);
            ByteUtils.WriteUInt16(ReferenceIndex, stream);
            return stream.ToArray();
        }
    }
}