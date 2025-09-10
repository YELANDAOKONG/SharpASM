namespace SharpASM.Models.Struct.Attribute;

public class MethodParametersAttributeStruct
{
    /*
     * MethodParameters_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u1 parameters_count;
           {   u2 name_index;
               u2 access_flags;
           } parameters[parameters_count];
       }
     */

    public class ParameterStruct
    {
        public ushort NameIndex { get; set; }
        public ushort AccessFlags { get; set; }
    }
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public byte ParametersCount { get; set; }
    public ParameterStruct[] Parameters { get; set; } = [];
}