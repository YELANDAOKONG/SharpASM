using SharpASM.Models.Type;

namespace SharpASM.Models.Structs;

public class ConstantDynamicInfoStruct
{
    /*
     * CONSTANT_Dynamic_info {
           u1 tag;
           u2 bootstrap_method_attr_index;
           u2 name_and_type_index;
       }
     */
    public byte Tag { get; set; } = (byte)ConstantPoolTag.Dynamic;
    public ushort BootstrapMethodAttrIndex { get; set; } 
    public ushort NameAndTypeIndex { get; set; } 
}