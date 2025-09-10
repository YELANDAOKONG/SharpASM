namespace SharpASM.Models.Struct.Attribute;

public class SourceDebugExtensionAttributeStruct
{
    /*
     * SourceDebugExtension_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u1 debug_extension[attribute_length];
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public byte[] DebugExtension { get; set; } = [];
}