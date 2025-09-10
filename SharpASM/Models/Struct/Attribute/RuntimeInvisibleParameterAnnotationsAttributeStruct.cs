namespace SharpASM.Models.Struct.Attribute;

public class RuntimeInvisibleParameterAnnotationsAttributeStruct
{
    /*
     * RuntimeInvisibleParameterAnnotations_attribute {
           u2 attribute_name_index;
           u4 attribute_length;
           u1 num_parameters;
           {   u2         num_annotations;
               annotation annotations[num_annotations];
           } parameter_annotations[num_parameters];
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public byte NumParameters { get; set; }
    public ParameterAnnotationsStruct[] ParameterAnnotations { get; set; } = [];
    
}