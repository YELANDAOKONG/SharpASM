namespace SharpASM.Models.Structs;

public class ConstantPoolInfoStruct
{
    /*
     * cp_info {
           u1 tag;
           u1 info[];
       }
     */
    
    public byte Tag { get; set; }
    public byte[] Info { get; set; } = [];
}