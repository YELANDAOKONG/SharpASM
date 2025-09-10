using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantStringInfoStruct
{
    /*
     * CONSTANT_String_info {
           u1 tag;
           u2 string_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.String;
    public ushort NameIndex { get; set; }
}