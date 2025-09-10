using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantUtf8InfoStruct
{
    /*
     * CONSTANT_Utf8_info {
           u1 tag;
           u2 length;
           u1 bytes[length];
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Utf8;
    public ushort Length { get; set; }
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
}