using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantNameAndTypeInfoStruct
{
    /*
     * CONSTANT_NameAndType_info {
           u1 tag;
           u2 name_index;
           u2 descriptor_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.NameAndType;
    public ushort NameIndex { get; set; } 
    public ushort DescriptorIndex { get; set; }
}