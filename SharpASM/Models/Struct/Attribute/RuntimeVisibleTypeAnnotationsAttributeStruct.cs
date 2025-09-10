using SharpASM.Models.Struct.Union;

namespace SharpASM.Models.Struct.Attribute;

public class RuntimeVisibleTypeAnnotationsAttributeStruct
{
    /*
     * RuntimeVisibleTypeAnnotations_attribute {
           u2              attribute_name_index;
           u4              attribute_length;
           u2              num_annotations;
           type_annotation annotations[num_annotations];
       }
     */
    
    public ushort AttributeNameIndex { get; set; }
    public uint AttributeLength { get; set; }
    public ushort NumAnnotations { get; set; }
    public TypeAnnotationStruct[] Annotations { get; set; } = [];
}