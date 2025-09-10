namespace SharpASM.Models.Struct.Attribute;

public class EnclosingMethodAttributeStruct
{
    /*
     * EnclosingMethod_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 class_index;
           u2 method_index;
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort ClassIndex { get; set; }
    public ushort MethodIndex { get; set; }
}