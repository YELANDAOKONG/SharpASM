namespace SharpASM.Models.Structs;

public class AttributeInfoStruct
{
    /*
     * attribute_info {
           u2 attribute_name_index;
           u4 attribute_length;
           u1 info[attribute_length];
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public byte[] Info { get; set; } = [];
}