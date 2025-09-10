namespace SharpASM.Models.Struct.Attribute;

public class SyntheticAttributeStruct
{
    /*
     * Synthetic_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
}