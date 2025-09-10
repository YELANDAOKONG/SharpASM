using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantClassInfoStruct
{
    /*
     * CONSTANT_Class_info {
           u1 tag;
           u2 name_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Class;
    public ushort NameIndex { get; set; }
}