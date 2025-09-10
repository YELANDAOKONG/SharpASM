using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantPackageInfoStruct
{
    /*
     * CONSTANT_Package_info {
           u1 tag;
           u2 name_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Package;
    public ushort NameIndex { get; set; } 
}