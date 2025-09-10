namespace SharpASM.Models.Struct.Attribute;

public class ExceptionsAttributeStruct
{
    /*
     * Exceptions_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u2 number_of_exceptions;
           u2 exception_index_table[number_of_exceptions];
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort NumberOfExceptions { get; set; }
    public ushort[] ExceptionIndexTable { get; set; } = [];
}