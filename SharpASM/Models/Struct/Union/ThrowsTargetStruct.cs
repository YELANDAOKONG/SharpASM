namespace SharpASM.Models.Struct.Union;

public class ThrowsTargetStruct
{
    /*
     * throws_target {
           u2 throws_type_index;
       }
     */
    
    public ushort ThrowsTypeIndex { get; set; }
}