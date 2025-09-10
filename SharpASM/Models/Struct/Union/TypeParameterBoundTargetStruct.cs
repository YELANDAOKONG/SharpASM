namespace SharpASM.Models.Struct.Union;

public class TypeParameterBoundTargetStruct
{
    /*
     * type_parameter_bound_target {
           u1 type_parameter_index;
           u1 bound_index;
       }
     */
    
    public byte TypeParameterIndex { get; set; }
    public byte BoundIndex { get; set; }
}