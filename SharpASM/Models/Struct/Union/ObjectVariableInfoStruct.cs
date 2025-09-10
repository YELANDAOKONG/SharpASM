namespace SharpASM.Models.Struct.Union;

public class ObjectVariableInfoStruct
{
    /*
     * Object_variable_info {
           u1 tag = ITEM_Object; /* 7 * /
           u2 cpool_index;
       }
     */
    public byte Tag { get; set; }
    public ushort CPoolIndex { get; set; }
}