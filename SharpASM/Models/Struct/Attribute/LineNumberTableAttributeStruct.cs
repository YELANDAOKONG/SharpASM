namespace SharpASM.Models.Struct.Attribute;

public class LineNumberTableAttributeStruct
{
    /*
     * LineNumberTable_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 line_number_table_length;
           {   u2 start_pc;
               u2 line_number;
           } line_number_table[line_number_table_length];
       }
     */
    
    public class LineNumberTableStruct
    {
        public ushort StartPc { get; set; }
        public ushort LineNumber { get; set; }
    }
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort LineNumberTableLength { get; set; }
    public LineNumberTableStruct[] LineNumberTable { get; set; } = [];
}