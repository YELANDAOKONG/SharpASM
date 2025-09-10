namespace SharpASM.Models.Struct.Attribute;

public class RecordAttributeStruct
{
    /*
     * Record_attribute {
           u2                    attribute_name_index;
           u4                    attribute_length;
           u2                    components_count;
           record_component_info components[components_count];
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort ComponentsCount { get; set; }
    public RecordComponentInfoStruct[] Components { get; set; } = [];
}