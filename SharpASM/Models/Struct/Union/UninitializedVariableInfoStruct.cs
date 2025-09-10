namespace SharpASM.Models.Struct.Union;

public class UninitializedVariableInfoStruct
{
    /*
     * Uninitialized_variable_info {
           u1 tag = ITEM_Uninitialized; /* 8 * /
           u2 offset;
       }
     */
    public byte Tag { get; set; }
    public ushort Offset { get; set; }
}