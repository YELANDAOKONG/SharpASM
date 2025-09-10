namespace SharpASM.Models.Struct;

public class LocalVariableTableAttribute
{
    /*
     * LocalVariableTable_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 local_variable_table_length;
           {   u2 start_pc;
               u2 length;
               u2 name_index;
               u2 descriptor_index;
               u2 index;
           } local_variable_table[local_variable_table_length];
       }
     */

    public class LocalVariableTableStruct
    {
        public ushort StartPc { get; set; }
        public ushort Length { get; set; }
        public ushort NameIndex { get; set; }
        public ushort DescriptorIndex { get; set; }
        public ushort Index { get; set; }
    }
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort LocalVariableTableLength { get; set; }
    public LocalVariableTableStruct[] LocalVariableTable { get; set; } = [];
}