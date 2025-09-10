using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantIntegerInfoStruct
{
    /*
     * CONSTANT_Integer_info {
           u1 tag;
           u4 bytes;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Integer;
    public uint Bytes { get; set; } 
}