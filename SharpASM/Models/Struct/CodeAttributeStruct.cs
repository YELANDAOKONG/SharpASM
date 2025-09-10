namespace SharpASM.Models.Struct;

public class CodeAttributeStruct
{
    /*
     * Code_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 max_stack;
           u2 max_locals;
           u4 code_length;
           u1 code[code_length];
           u2 exception_table_length;
           {   u2 start_pc;
               u2 end_pc;
               u2 handler_pc;
               u2 catch_type;
           } exception_table[exception_table_length];
           u2 attributes_count;
           attribute_info attributes[attributes_count];
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort MaxStack { get; set; }
    public ushort MaxLocals { get; set; }
    public uint CodeLength { get; set; }
    public byte[] Code { get; set; } = [];
    public ushort ExceptionTableLength { get; set; }
    public CodeAttributeExceptionTableStruct[] ExceptionTable { get; set; } = [];
    public ushort AttributesCount { get; set; }
    public AttributeInfoStruct[] Attributes { get; set; } = [];
    
}