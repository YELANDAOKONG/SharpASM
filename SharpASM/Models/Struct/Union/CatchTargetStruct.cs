namespace SharpASM.Models.Struct.Union;

public class CatchTargetStruct
{
    /*
     * catch_target {
           u2 exception_table_index;
       }
     */
    
    public ushort ExceptionTableIndex { get; set; }
}