namespace SharpASM.Models.Struct.Attribute;

public class AnnotationDefaultAttributeStruct
{
    /*
     * AnnotationDefault_attribute {
           u2            attribute_name_index;
           u4            attribute_length;
           element_value default_value;
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ElementValueStruct DefaultValue { get; set; } = new();
}