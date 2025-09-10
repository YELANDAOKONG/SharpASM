namespace SharpASM.Models.Struct.Attribute;

public class RuntimeInvisibleAnnotationsAttributeStruct
{
    /*
     * RuntimeInvisibleAnnotations_attribute {
           u2         attribute_name_index;
           u4         attribute_length;
           u2         num_annotations;
           annotation annotations[num_annotations];
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort NumAnnotations { get; set; }
    public AnnotationStruct[] Annotations { get; set; } = [];
}