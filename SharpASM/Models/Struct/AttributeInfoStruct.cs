using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class AttributeInfoStruct
{
    /*
     * attribute_info {
           u2 attribute_name_index;
           u4 attribute_length;
           u1 info[attribute_length];
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public byte[] Info { get; set; } = [];
    
    public static AttributeInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var attribute = new AttributeInfoStruct();
        attribute.AttributeNameIndex = ByteUtils.ReadUInt16(data, ref offset);
        attribute.AttributeLength = ByteUtils.ReadUInt32(data, ref offset);
        attribute.Info = ByteUtils.ReadBytes(data, ref offset, (int)attribute.AttributeLength);
        return attribute;
    }
        
    public virtual byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            ByteUtils.WriteUInt16(AttributeNameIndex, stream);
            ByteUtils.WriteUInt32(AttributeLength, stream);
            stream.Write(Info, 0, Info.Length);
            return stream.ToArray();
        }
    }
}