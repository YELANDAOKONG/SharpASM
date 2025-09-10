using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantDoubleInfoStruct
{
    /*
     * CONSTANT_Double_info {
           u1 tag;
           u4 high_bytes;
           u4 low_bytes;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Double;
    public uint HighBytes { get; set; } 
    public uint LowBytes { get; set; }
}