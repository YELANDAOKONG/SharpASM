namespace SharpASM.Models.Struct.Union;

public class LocalvarTargetStruct
{
    /*
     * localvar_target {
           u2 table_length;
           {   u2 start_pc;
               u2 length;
               u2 index;
           } table[table_length];
       }
     */
    
    public class LocalvarTargetEntryStruct
    {
        public ushort StartPc { get; set; }
        public ushort Length { get; set; }
        public ushort Index { get; set; }
    }
    
    public ushort TableLength { get; set; }
    public LocalvarTargetEntryStruct[] Table { get; set; } = [];
}