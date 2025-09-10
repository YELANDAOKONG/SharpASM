namespace SharpASM.Models.Struct.Union;

public class TypeAnnotationStruct
{
    /*
     * type_annotation {
           u1 target_type;
           union {
               type_parameter_target;
               supertype_target;
               type_parameter_bound_target;
               empty_target;
               formal_parameter_target;
               throws_target;
               localvar_target;
               catch_target;
               offset_target;
               type_argument_target;
           } target_info;
           type_path target_path;
           u2        type_index;
           u2        num_element_value_pairs;
           {   u2            element_name_index;
               element_value value;
           } element_value_pairs[num_element_value_pairs];
       }
     */
    
    public class TargetInfoUnion
    {
        public TypeParameterTargetStruct TypeParameterTarget { get; set; } = new();
        public SupertypeTargetStruct SupertypeTarget { get; set; } = new();
        public TypeParameterBoundTargetStruct TypeParameterBoundTarget { get; set; } = new();
        public EmptyTargetStruct EmptyTarget { get; set; } = new();
        public FormalParameterTargetStruct FormalParameterTarget { get; set; } = new();
        public ThrowsTargetStruct ThrowsTarget { get; set; } = new();
        public LocalvarTargetStruct LocalvarTarget { get; set; } = new();
        public CatchTargetStruct CatchTarget { get; set; } = new();
        public OffsetTargetStruct OffsetTarget { get; set; } = new();
        public TypeArgumentTargetStruct TypeArgumentTarget { get; set; } = new();
    }
    
    public byte TargetType { get; set; }
    public TargetInfoUnion TargetInfo { get; set; } = new();
    public TypePathStruct TargetPath { get; set; } = new();
    public ushort TypeIndex { get; set; }
    public ushort NumElementValuePairs { get; set; }
    public ElementValuePairStruct[] ElementValuePairs { get; set; } = [];
}