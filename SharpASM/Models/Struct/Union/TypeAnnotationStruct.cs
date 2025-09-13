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
        public TypeParameterTargetStruct? TypeParameterTarget { get; set; } = null;
        public SupertypeTargetStruct? SupertypeTarget { get; set; } = null;
        public TypeParameterBoundTargetStruct? TypeParameterBoundTarget { get; set; } = null;
        public EmptyTargetStruct? EmptyTarget { get; set; } = null;
        public FormalParameterTargetStruct? FormalParameterTarget { get; set; } = null;
        public ThrowsTargetStruct? ThrowsTarget { get; set; } = null;
        public LocalvarTargetStruct? LocalvarTarget { get; set; } = null;
        public CatchTargetStruct? CatchTarget { get; set; } = null;
        public OffsetTargetStruct? OffsetTarget { get; set; } = null;
        public TypeArgumentTargetStruct? TypeArgumentTarget { get; set; } = null;
    }
    
    public byte TargetType { get; set; }
    public TargetInfoUnion TargetInfo { get; set; } = new();
    public TypePathStruct TargetPath { get; set; } = new();
    public ushort TypeIndex { get; set; }
    public ushort NumElementValuePairs { get; set; }
    public ElementValuePairStruct[] ElementValuePairs { get; set; } = [];
}