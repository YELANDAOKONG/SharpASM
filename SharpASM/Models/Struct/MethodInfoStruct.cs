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
}