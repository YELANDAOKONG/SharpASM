namespace SharpASM.Models.Struct;

public class ParameterAnnotationsStruct
{
    public ushort NumAnnotations { get; set; }
    public AnnotationStruct[] Annotations { get; set; } = [];
}