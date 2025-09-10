namespace SharpASM.Models.Struct.Attribute;

public class NestHostAttributeStruct
{
    /*
     * NestHost_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 host_class_index;
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort HostClassIndex { get; set; }
}