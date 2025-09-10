namespace SharpASM.Models.Struct;

public class DeprecatedAttribute
{
    /*
     * Deprecated_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
}