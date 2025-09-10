namespace SharpASM.Models.Structs;

public class ConstantValueAttributeStruct
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
}