using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantInvokeDynamicInfoStruct
{
    /*
     * CONSTANT_InvokeDynamic_info {
           u1 tag;
           u2 bootstrap_method_attr_index;
           u2 name_and_type_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.InvokeDynamic;
    public ushort BootstrapMethodAttrIndex { get; set; } 
    public ushort NameAndTypeIndex { get; set; } 
    
    public static ConstantInvokeDynamicInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantInvokeDynamicInfoStruct();
        info.Tag = data[offset++];
        info.BootstrapMethodAttrIndex = ByteUtils.ReadUInt16(data, ref offset);
        info.NameAndTypeIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }
    
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            stream.WriteByte(Tag);
            ByteUtils.WriteUInt16(BootstrapMethodAttrIndex, stream);
            ByteUtils.WriteUInt16(NameAndTypeIndex, stream);
            return stream.ToArray();
        }
    }
}