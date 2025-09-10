using SharpASM.Utilities;

namespace SharpASM.Models.Structs;

public class ConstantValueAttributeStruct : AttributeInfoStruct
{
    /*
     * ConstantValue_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 constantvalue_index;
       }
     */
    
    // public ushort AttributeNameIndex { get; set; }
    // public uint AttributeLength { get; set; }
    public ushort ConstantValueIndex { get; set; }
    
    public static new ConstantValueAttributeStruct FromBytes(byte[] data, ref int offset)
    {
        var attribute = new ConstantValueAttributeStruct();
        attribute.AttributeNameIndex = ByteUtils.ReadUInt16(data, ref offset);
        attribute.AttributeLength = ByteUtils.ReadUInt32(data, ref offset);
        attribute.ConstantValueIndex = ByteUtils.ReadUInt16(data, ref offset);
        return attribute;
    }
    
    public override byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            ByteUtils.WriteUInt16(AttributeNameIndex, stream);
            ByteUtils.WriteUInt32(AttributeLength, stream);
            ByteUtils.WriteUInt16(ConstantValueIndex, stream);
            return stream.ToArray();
        }
    }
}