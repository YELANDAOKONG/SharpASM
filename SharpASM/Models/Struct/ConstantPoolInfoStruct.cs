using SharpASM.Models.Struct.Interfaces;

namespace SharpASM.Models.Struct;

public class ConstantPoolInfoStruct // : IConstantStruct
{
    /*
     * cp_info {
           u1 tag;
           u1 info[];
       }
     */
    
    public byte Tag { get; set; }
    public byte[] Info { get; set; } = [];
    
    public byte GetTag() => Tag;
}