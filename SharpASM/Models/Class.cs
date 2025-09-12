using SharpASM.Models.Struct;
using SharpASM.Models.Type;
using SharpASM.Utilities;

namespace SharpASM.Models;

public class Class
{
    
    public uint Magic { get; set; } = 0xCAFEBABE;
    public ushort MinorVersion { get; set; } = 0;
    public ClassFileVersion MajorVersion { get; set; } = ClassFileVersion.V17;
    
    public List<ConstantPoolInfo> ConstantPool { get; set; } = [];
    public ushort ConstantPoolCount => CalculateConstantPoolCount();
    public ushort ConstantPoolIndexCount => CalculateConstantPoolIndexCount();
    
    public ClassAccessFlags AccessFlags { get; set; }
    
    public string ThisClass { get; set; } = string.Empty;
    public string SuperClass { get; set; } = "java/lang/Object";
    
    public ushort InterfacesCount => (ushort)Interfaces.Count;
    public List<string> Interfaces { get; set; } = new();
    
    public List<Field> Fields { get; set; } = new();
    public List<Method> Methods { get; set; } = new();
    public List<Attribute> Attributes { get; set; } = new();

    
    public ushort CalculateConstantPoolCount()
    {
        return (ushort)(ConstantPool.Count + 1);
    }

    public ushort CalculateConstantPoolIndexCount()
    {
        ushort count = 1;
        
        foreach (var constant in ConstantPool)
        {
            if (constant.Tag == ConstantPoolTag.Long || 
                constant.Tag == ConstantPoolTag.Double)
            {
                count += 2;
            }
            else
            {
                count += 1;
            }
        }
        
        return count;
    }

    #region Functions

    public ConstantPoolHelper GetConstantPoolHelper()
    {
        return new ConstantPoolHelper(ConstantPool);
    }

    #endregion
    
}