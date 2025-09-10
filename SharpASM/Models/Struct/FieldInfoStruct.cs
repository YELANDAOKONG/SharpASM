using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class FieldInfoStruct
{
    /*
     * field_info {
           u2             access_flags;
           u2             name_index;
           u2             descriptor_index;
           u2             attributes_count;
           attribute_info attributes[attributes_count];
       }
     */
    
    public ushort AccessFlags { get; set; }
    public ushort NameIndex { get; set; }
    public ushort DescriptorIndex { get; set; }
    public ushort AttributesCount { get; set; }
    public AttributeInfoStruct[] Attributes { get; set; } = [];
    
    public static FieldInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var field = new FieldInfoStruct();
        field.AccessFlags = ByteUtils.ReadUInt16(data, ref offset);
        field.NameIndex = ByteUtils.ReadUInt16(data, ref offset);
        field.DescriptorIndex = ByteUtils.ReadUInt16(data, ref offset);
        field.AttributesCount = ByteUtils.ReadUInt16(data, ref offset);
        
        field.Attributes = new AttributeInfoStruct[field.AttributesCount];
        for (int i = 0; i < field.AttributesCount; i++)
        {
            field.Attributes[i] = AttributeInfoStruct.FromBytes(data, ref offset);
        }
        
        return field;
    }
        
    public byte[] ToBytes()
    {
        using (var stream = new MemoryStream())
        {
            ByteUtils.WriteUInt16(AccessFlags, stream);
            ByteUtils.WriteUInt16(NameIndex, stream);
            ByteUtils.WriteUInt16(DescriptorIndex, stream);
            ByteUtils.WriteUInt16(AttributesCount, stream);
            
            foreach (var attribute in Attributes)
            {
                var bytes = attribute.ToBytes();
                stream.Write(bytes, 0, bytes.Length);
            }
            
            return stream.ToArray();
        }
    }
}