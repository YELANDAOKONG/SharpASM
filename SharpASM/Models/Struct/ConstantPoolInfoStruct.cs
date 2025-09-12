using SharpASM.Models.Struct.Interfaces;
using SharpASM.Models.Type;

namespace SharpASM.Models.Struct;

public class ConstantPoolInfoStruct // : IConstantStruct
{
    /*
     * cp_info {
           u1 tag;
           u1 info[];
       }
     */
    
    public byte Tag { get; set; }
    public byte[] Info { get; set; } = [];
    
    public byte GetTag() => Tag;

    public IConstantStruct ToConstantStruct()
    {
        int offset = 0;
        
        switch ((ConstantPoolTag)Tag)
        {
            case ConstantPoolTag.Class:
                return ConstantClassInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.Fieldref:
                return ConstantFieldrefInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.Methodref:
                return ConstantMethodrefInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.InterfaceMethodref:
                return ConstantInterfaceMethodrefInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.String:
                return ConstantStringInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
            
            case ConstantPoolTag.Integer:
                return ConstantIntegerInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.Float:
                return ConstantFloatInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.Long:
                return ConstantLongInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.Double:
                return ConstantDoubleInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.NameAndType:
                return ConstantNameAndTypeInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.Utf8:
                return ConstantUtf8InfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.MethodHandle:
                return ConstantMethodHandleInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.MethodType:
                return ConstantMethodTypeInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.Dynamic:
                return ConstantDynamicInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.InvokeDynamic:
                return ConstantInvokeDynamicInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.Module:
                return ConstantModuleInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            case ConstantPoolTag.Package:
                return ConstantPackageInfoStruct.FromBytesWithTag(Tag, Info, ref offset);
                
            default:
                throw new NotSupportedException($"Unknown constant pool tag: {Tag}");
        }
    }
}