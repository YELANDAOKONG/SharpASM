using SharpASM.Utilities;

namespace SharpASM.Models.Struct;

public class MethodInfoStruct
{
    /*
     * method_info {
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
    
    public static MethodInfoStruct FromBytes(byte[] data, ref int offset)
    {
        var method = new MethodInfoStruct();
        method.AccessFlags = ByteUtils.ReadUInt16(data, ref offset);
        method.NameIndex = ByteUtils.ReadUInt16(data, ref offset);
        method.DescriptorIndex = ByteUtils.ReadUInt16(data, ref offset);
        method.AttributesCount = ByteUtils.ReadUInt16(data, ref offset);
        
        method.Attributes = new AttributeInfoStruct[method.AttributesCount];
        for (int i = 0; i < method.AttributesCount; i++)
        {
            method.Attributes[i] = AttributeInfoStruct.FromBytes(data, ref offset);
        }
        
        return method;
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