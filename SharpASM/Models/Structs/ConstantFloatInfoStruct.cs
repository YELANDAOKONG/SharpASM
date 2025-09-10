using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantFloatInfoStruct
{
    /*
     * CONSTANT_Float_info {
           u1 tag;
           u4 bytes;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Float;
    public uint Bytes { get; set; } 
}