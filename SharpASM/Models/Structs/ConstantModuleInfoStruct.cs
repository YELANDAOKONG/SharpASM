using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantModuleInfoStruct
{
    /*
     * CONSTANT_Module_info {
           u1 tag;
           u2 name_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Module;
    public ushort NameIndex { get; set; } 
}