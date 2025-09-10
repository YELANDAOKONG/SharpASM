using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class ConstantPackageInfoStruct
{
    /*
     * CONSTANT_Package_info {
           u1 tag;
           u2 name_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Package;
    public ushort NameIndex { get; set; } 
    
    public static ConstantPackageInfoStruct FromBytesWithTag(byte tag, byte[] data, ref int offset)
    {
        var info = new ConstantPackageInfoStruct();
        info.Tag = tag;
        info.NameIndex = ByteUtils.ReadUInt16(data, ref offset);
        return info;
    }

    public static ConstantPackageInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var info = new ConstantPackageInfoStruct();
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