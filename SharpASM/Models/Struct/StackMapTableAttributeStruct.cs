using SharpASM.Models.Struct.Union;

namespace SharpASM.Models.Struct;

public class StackMapTableAttributeStruct
{
    /*
     * StackMapTable_attribute {
           u2              attribute_name_index;
           u4              attribute_length;
           u2              number_of_entries;
           stack_map_frame entries[number_of_entries];
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public StackMapFrameStruct[] Entries { get; set; } = [];
}