using SharpASM.Models.Struct.Interfaces;
using SharpASM.Utilities;

namespace SharpASM.Models.Struct.Attribute;

public class ConstantValueAttributeStruct : IAttributeStruct
{
    /*
     * ConstantValue_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 constantvalue_index;
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort ConstantValueIndex { get; set; }
    
    public byte[] ToBytes()
    {
        return ToStructInfo().ToBytes();
    }
    
    public byte[] ToBytesWithoutIndexAndLength()
    {
        using (var stream = new MemoryStream())
        {
            ByteUtils.WriteUInt16(ConstantValueIndex, stream);
            return stream.ToArray();
        }
    }
    
    public AttributeInfoStruct ToStructInfo()
    {
        var infoBytes = ToBytesWithoutIndexAndLength();
        return new AttributeInfoStruct
        {
            AttributeNameIndex = AttributeNameIndex,
            AttributeLength = (uint)infoBytes.Length,
            Info = infoBytes
        };
    }
}