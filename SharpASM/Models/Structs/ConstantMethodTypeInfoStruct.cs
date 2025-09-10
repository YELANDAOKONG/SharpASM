using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantMethodTypeInfoStruct
{
    /*
     * CONSTANT_MethodType_info {
           u1 tag;
           u2 descriptor_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.MethodType;
    public ushort DescriptorIndex { get; set; } 
}