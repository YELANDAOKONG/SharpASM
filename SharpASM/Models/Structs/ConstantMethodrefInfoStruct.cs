using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantMethodrefInfoStruct
{
    /*
     * CONSTANT_Methodref_info {
           u1 tag;
           u2 class_index;
           u2 name_and_type_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Methodref;
    public ushort ClassIndex { get; set; }
    public ushort NameAndTypeIndex { get; set; }
}