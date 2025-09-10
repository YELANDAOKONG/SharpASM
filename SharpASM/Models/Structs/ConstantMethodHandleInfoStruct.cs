using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantMethodHandleInfoStruct
{
    /*
     * CONSTANT_MethodHandle_info {
           u1 tag;
           u1 reference_kind;
           u2 reference_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.MethodHandle;
    public byte ReferenceKind { get; set; } 
    public ushort ReferenceIndex { get; set; } 
}