namespace SharpASM.Models.Struct.Attribute;

public class SignatureAttributeStruct
{
    /*
     * Signature_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 signature_index;
       }
     */

    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort SignatureIndex { get; set; }
}