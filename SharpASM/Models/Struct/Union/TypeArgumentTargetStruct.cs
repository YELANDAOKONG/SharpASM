namespace SharpASM.Models.Struct.Union;

public class TypeArgumentTargetStruct
{
    /*
     * type_argument_target {
           u2 offset;
           u1 type_argument_index;
       }
     */
    
    public ushort Offset { get; set; }
    public byte TypeArgumentIndex { get; set; }
}